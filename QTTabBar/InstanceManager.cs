//    This file is part of QTTabBar, a shell extension for Microsoft
//    Windows Explorer.
//    Copyright (C) 2007-2022  Quizo, Paul Accisano, indiff
//
//    QTTabBar is free software: you can redistribute it and/or modify
//    it under the terms of the GNU General Public License as published by
//    the Free Software Foundation, either version 3 of the License, or
//    (at your option) any later version.
//
//    QTTabBar is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//    GNU General Public License for more details.
//
//    You should have received a copy of the GNU General Public License
//    along with QTTabBar.  If not, see <http://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Threading;
using System.Threading.Tasks;
using QTTabBarLib.Interop;

namespace QTTabBarLib {
    internal static class InstanceManager {
        // 选中项字典，key为路径，value为选中的项列表
        private static Dictionary<string, List<string>> selectDict = new Dictionary<string, List<string>>();
        // 线程与QTTabBar实例的映射，每个线程对应一个QTTabBarClass实例
        private static Dictionary<Thread, QTTabBarClass> dictTabInstances = new Dictionary<Thread, QTTabBarClass>();
        // 线程与QTButtonBar实例的映射，每个线程对应一个QTButtonBar实例
        private static Dictionary<Thread, QTButtonBar> dictBBarInstances = new Dictionary<Thread, QTButtonBar>();
        // 句柄与QTTabBar实例的映射，支持句柄快速查找QTTabBarClass
        private static StackDictionary<IntPtr, QTTabBarClass> sdTabHandles = new StackDictionary<IntPtr, QTTabBarClass>();
        // 按钮栏的读写锁，保证多线程安全
        private static ReaderWriterLock rwLockBtnBar = new ReaderWriterLock();
        // 标签栏的读写锁，保证多线程安全
        private static ReaderWriterLock rwLockTabBar = new ReaderWriterLock();
        // 选中项字典的读写锁，保证多线程安全
        private static ReaderWriterLock rwLockSelectDict = new ReaderWriterLock();

        // WCF双工通信客户端，用于进程间通信
        private static DuplexClient commClient;
        // 是否为主进程标志
        private static bool isServer;

        // 仅主进程使用的变量
        // WCF服务端对象
        // Server-only stuff
        private static ServiceHost serviceHost;
        // 所有已连接的WCF客户端回调接口列表
        private static List<ICommClient> callbacks = new List<ICommClient>();
        // 句柄与WCF客户端回调接口的映射
        private static StackDictionary<IntPtr, ICommClient> sdInstances = new StackDictionary<IntPtr, ICommClient>();

        // 托盘图标管理对象
        private static TrayIcon trayIcon;
        // add by indiff
        // 读写锁，辅助多线程同步（indiff添加）
        private static ReaderWriterLockSlim rwLock = new ReaderWriterLockSlim();


        #region Comm Classes and Interfaces
        /// <summary>
        /// WCF双工通信客户端实现
        /// </summary>
        private class DuplexClient : DuplexClientBase<ICommService> {
            public DuplexClient(InstanceContext callbackInstance, Binding binding, EndpointAddress remoteAddress)
                : base(callbackInstance, binding, remoteAddress) {
            }
            public new ICommService Channel { get { return base.Channel; } }
        }

        /// <summary>
        /// WCF服务契约接口，定义了所有进程间可以调用的方法
        /// </summary>
        [ServiceContract(SessionMode = SessionMode.Required, CallbackContract = typeof(ICommClient))]
        private interface ICommService {
            [OperationContract]
            void Subscribe();

            [OperationContract]
            void PushInstance(IntPtr hwnd);

            [OperationContract]
            void DeleteInstance(IntPtr hwnd);

            [OperationContract]
            bool IsMainProcess();

            [OperationContract]
            int GetTotalInstanceCount();

            [OperationContract]
            void AddToTrayIcon(IntPtr tabBarHandle, IntPtr explorerHandle, string currentPath, string[] tabNames, string[] tabPaths);

            [OperationContract]
            void RemoveFromTrayIcon(IntPtr tabBarHandle);

            [OperationContract]
            void SelectTabOnOtherTabBar(IntPtr tabBarHandle, int index);

            [OperationContract]
            bool ExecuteOnMainProcess(byte[] encodedAction, bool doAsync);

            [OperationContract]
            void ExecuteOnServerProcess(byte[] encodedAction, bool doAsync);

            [OperationContract]
            object GetFromServerProcess(byte[] encodedAction);

            [OperationContract]
            void Broadcast(byte[] encodedAction);
        }

        /// <summary>
        /// WCF服务端实现，处理所有进程间的请求
        /// </summary>
        [ServiceBehavior(
                ConcurrencyMode = ConcurrencyMode.Reentrant,
                InstanceContextMode = InstanceContextMode.PerSession)]
        private class CommService : ICommService {

            /// <summary>
            /// 判断客户端连接是否已断开
            /// </summary>
            private static bool IsDead(ICommClient client) {
                ICommunicationObject ico = client as ICommunicationObject;
                return ico != null && ico.State != CommunicationState.Opened;                
            }
            /// <summary>
            /// 检查并移除已断开的客户端连接
            /// </summary>
            private static void CheckConnections() {
                callbacks.RemoveAll(IsDead);
                sdInstances.RemoveAllValues(c => !callbacks.Contains(c));
            }

            /// <summary>
            /// 获取当前操作的回调通道
            /// </summary>
            private static ICommClient GetCallback() {
                return OperationContext.Current.GetCallbackChannel<ICommClient>();
            }

            /// <summary>
            /// 获取所有实例总数
            /// </summary>
            public int GetTotalInstanceCount() {
                CheckConnections();
                return sdInstances.Count;
            }

            public void AddToTrayIcon(IntPtr tabBarHandle, IntPtr explorerHandle, string currentPath, string[] tabNames, string[] tabPaths) {
                if(trayIcon == null) trayIcon = new TrayIcon();
                trayIcon.AddToTrayIcon(tabBarHandle, explorerHandle, currentPath, tabNames, tabPaths);
            }

            public void RemoveFromTrayIcon(IntPtr tabBarHandle) {
                if(trayIcon == null) trayIcon = new TrayIcon();
                trayIcon.RestoreWindow(tabBarHandle);
            }

            public void SelectTabOnOtherTabBar(IntPtr tabBarHandle, int index) {
                ICommClient comm;
                if(sdInstances.TryGetValue(tabBarHandle, out comm)) {
                    QTUtility2.log("SelectTabOnOtherTabBar comm.Execute");
                    comm.Execute(DelToByte(new Action(() => {
                        using(new Keychain(rwLockTabBar, false)) {
                            QTTabBarClass tabbar;
                            if(sdTabHandles.TryGetValue(tabBarHandle, out tabbar)) {
                                tabbar.SelectedTabIndex = index;
                            }
                        }
                    })));
                }
            }

            public bool ExecuteOnMainProcess(byte[] encodedAction, bool doAsync) {
                CheckConnections();
                if(IsMainProcess()) {
                    return true;
                }
                else if(sdInstances.Count == 0) {
                    return false;
                }
                ICommClient callback = sdInstances.Peek();
                if(doAsync) {
                    QTUtility2.log("ExecuteOnMainProcess: dispatching async Task.Run");
                    // if (!IsDead( callback ))
                    // {
                        Task.Run(() => {
                            QTUtility2.log("ExecuteOnMainProcess: Task.Run executing");
                            try {
                                if (!IsDead(callback))
                                {
                                    callback.Execute(encodedAction);
                                    QTUtility2.log("ExecuteOnMainProcess: Task.Run completed OK");
                                }
                                else {
                                    QTUtility2.log("ExecuteOnMainProcess: Task.Run skipped (dead callback)");
                                }
                            }
                            catch(Exception e) {
                                QTUtility2.MakeErrorLog(e, "ExecuteOnMainProcess Task.Run");
                            }
                        });
                    // }
                }
                else {
                    QTUtility2.log("ExecuteOnMainProcess callback.Execute");
                    callback.Execute(encodedAction);
                }
                return false;
            }

            public void ExecuteOnServerProcess(byte[] encodedAction, bool doAsync) {
                try
                {
                    Delegate action = ByteToDel(encodedAction);
                    if (action != null)
                    {
                        if (doAsync)
                        {
                            QTUtility2.log("ExecuteOnServerProcess: dispatching async Task.Run");
                            Task.Run(() => {
                                QTUtility2.log("ExecuteOnServerProcess: Task.Run executing");
                                try { action.DynamicInvoke(); QTUtility2.log("ExecuteOnServerProcess: Task.Run completed OK"); }
                                catch (Exception e) { QTUtility2.MakeErrorLog(e, "ExecuteOnServerProcess Task.Run"); }
                            });
                        }
                        else
                        {
                            action.DynamicInvoke();
                        }
                    }
                }
                catch (Exception ex)
                {
                    QTUtility2.MakeErrorLog(ex);
                }
            }

            public object GetFromServerProcess(byte[] encodedAction) {
                try {
                    Delegate action = ByteToDel(encodedAction);
                    if ( action != null)
                    { return action.DynamicInvoke(); }
                    return null;
                }
                catch(Exception ex) {
                    QTUtility2.MakeErrorLog(ex);
                    return null;
                }
            }

            /**
             *
             */
            public void Broadcast(byte[] encodedAction) {
                // TimeSpan start = new TimeSpan(DateTime.Now.Ticks);
                ICommClient sender = GetCallback();
                CheckConnections();
                List<ICommClient> targets = callbacks.Where(c => c != sender).ToList();
                QTUtility2.log("Broadcast: dispatching async Task.Run to " + targets.Count + " targets");
                Task.Run(() => {
                    int i = 0;
                    foreach(ICommClient target in targets) {
                        try {
                            i++;
                            QTUtility2.log("Broadcast Task.Run: sending to target " + i + "/" + targets.Count);
                            if (!IsDead(target)) {
                                target.Execute(encodedAction);
                            }
                            else {
                                QTUtility2.log("Broadcast Task.Run: target " + i + " is dead, skipping");
                            }
                        }
                        catch (Exception ex)
                        {
                            QTUtility2.MakeErrorLog(ex, "Broadcast Task.Run target " + i);
                        }
                    }
                    QTUtility2.log("Broadcast Task.Run: completed " + i + " targets");
                });

                // TimeSpan abs = new TimeSpan(DateTime.Now.Ticks).Subtract(start).Duration();
            }

            public void DeleteInstance(IntPtr hwnd) {
                CheckConnections();
                sdInstances.Remove(hwnd);
            }

            public bool IsMainProcess() {
                CheckConnections();
                return sdInstances.Count > 0 && GetCallback() == sdInstances.Peek();
            }

            public void Subscribe() {
                ICommClient callback = GetCallback();
                if(!callbacks.Contains(callback)) {
                    callbacks.Add(callback);
                }
            }

            public void PushInstance(IntPtr hwnd) {
                CheckConnections();
                if(!callbacks.Contains(GetCallback())) return; // hmmm....
                sdInstances.Push(hwnd, GetCallback());
            }
        }

        private interface ICommClient {
            [OperationContract]
            void Execute(byte[] encodedAction);
        }

        [CallbackBehavior(ConcurrencyMode = ConcurrencyMode.Reentrant, UseSynchronizationContext = false)]
        private class CommClient : ICommClient {
            public void Execute(byte[] encodedAction) {
                Delegate thedel = null;
                bool isBeginInvokeMain = false;
                try
                {
                    QTUtility2.log("InstanceManager CommClient Execute : "
                                   // +  encodedAction + 
                                   // " Length: " + encodedAction.Length + 
                                   // " str " + Encoding.Default.GetString(encodedAction)
                                   );
                    // add by indiff fix bug
                    if (null == encodedAction || encodedAction.Length == 0 ) {
                        return;
                    }
                    thedel = ByteToDel(encodedAction);
                    if (thedel != null && thedel.Method != null )
                    {
                        QTUtility2.log( "InstanceManager CommClient DynamicInvoke action: " + thedel  + " method:" + thedel.Method);
                        thedel.DynamicInvoke();
                    }
                }
                catch (Exception ex)
                {
                    string errStr = null;
                    if (thedel != null && thedel.Method != null)
                    {
                        errStr = "delegate name:" + thedel.GetType()  + " ";
                        errStr += "method name:" + thedel.Method.Name + " daynamic invoke error";
                    }
                    QTUtility2.MakeErrorLog(ex, errStr);
                    // re initialize 
                    Initialize();
                }
                // remove close
                finally
                {
                    if (!Config.Window.CaptureWeChatSelection) {

                        if (commClient != null && commClient.State != CommunicationState.Closed)
                        {
                            try
                            {
                                commClient.Close();
                            }
                            catch (Exception closeEx)
                            {
                                QTUtility2.MakeErrorLog(closeEx, "commClient.Close() failed, try Abort()");
                                commClient.Abort();
                            }
                        }
                    }
                } // end of finally
            }
        }
        #endregion

        #region Utility Methods

        /// <summary>
        /// 委托对象序列化为字节数组
        /// </summary>
        private static byte[] DelToByte(Delegate del) {
            return QTUtility.ObjectToByteArray(new SerializeDelegate(del));
        }

        /// <summary>
        /// 字节数组反序列化为委托对象
        /// </summary>
        private static Delegate ByteToDel(byte[] buf) {
            if (buf == null || buf.Length == 0 ) { return null; }
            object v = QTUtility.ByteArrayToObject(buf);
            if (v == null) { return null; }
            return ((SerializeDelegate)v).Delegate;
            // return BinaryPack.BinaryConverter.Deserialize<SerializeDelegate>(buf);
        }

        #endregion


        /// <summary>
        /// 初始化进程间通信和实例同步
        /// </summary>
        public static void Initialize(bool skipServer = false) {
            uint desktopPID;
            PInvoke.GetWindowThreadProcessId(WindowUtils.GetShellTrayWnd(), out desktopPID);
            isServer = desktopPID == PInvoke.GetCurrentProcessId();

            const string PipeName = "QTTabBarPipe";
            string address = "net.pipe://localhost/" + PipeName + desktopPID;
            Thread thread = null;

            // WFC channels should never be opened on any thread that has a message loop!
            // Otherwise reentrant calls will deadlock, for some reason.
            // So, create a new thread and open the channels there.
            // WCF通道不能在有消息循环的线程上打开，否则会死锁，所以新建线程
            thread = new Thread(() => {
                if(isServer && !skipServer) {
                    serviceHost = new ServiceHost(
                            typeof(CommService),
                            new Uri[] { new Uri(address) });
                    serviceHost.AddServiceEndpoint(
                            typeof(ICommService),
                            new NetNamedPipeBinding(NetNamedPipeSecurityMode.None) {
                                ReceiveTimeout = TimeSpan.MaxValue,
                                ReaderQuotas = {MaxArrayLength = int.MaxValue},
                                MaxBufferSize = int.MaxValue,
                                MaxReceivedMessageSize = int.MaxValue,
                            },
                            new Uri(address));
                    serviceHost.Open();
                }
                

                commClient = new DuplexClient(new InstanceContext(new CommClient()),
                        new NetNamedPipeBinding(NetNamedPipeSecurityMode.None) {
                            ReceiveTimeout = TimeSpan.MaxValue,
                            ReaderQuotas = { MaxArrayLength = int.MaxValue },
                            MaxBufferSize = int.MaxValue,
                            MaxReceivedMessageSize = int.MaxValue,
                        },
                        new EndpointAddress(address));
                try {
                    commClient.Open();
                    commClient.Channel.Subscribe();
                    using(new Keychain(rwLockTabBar, false)) {
                        foreach(IntPtr handle in sdTabHandles.Keys) {
                            commClient.Channel.PushInstance(handle);
                        }
                    }
                }
                catch(EndpointNotFoundException e) {
                    QTUtility2.MakeErrorLog( e, "Initialize commClient endpoint not found!" );
                }
                lock(thread) {
                    Monitor.Pulse(thread);
                }
                // Yes, we can just let the thread die here.
                // 线程结束
            });
            thread.Start();
            lock(thread) {
                Monitor.Wait(thread);
            }            
        }

        /// <summary>
        /// 获取WCF通信通道
        /// </summary>
        private static ICommService GetChannel() {
            if(commClient.State != CommunicationState.Opened) {
                Initialize(true);
            }
            return commClient.State == CommunicationState.Opened ? commClient.Channel : null;
        }

        /// <summary>
        /// 静态广播，将操作广播到所有实例
        /// </summary>
        public static void StaticBroadcast(Action action) {
            ICommService service = GetChannel();
            if(service != null) service.Broadcast(DelToByte(action));
        }

        /// <summary>
        /// 标签栏广播，将操作广播到所有标签栏实例
        /// </summary>
        public static void TabBarBroadcast(Action<QTTabBarClass> action, bool includeCurrent) {
            LocalTabBroadcast(action, Thread.CurrentThread);
            if(includeCurrent) {
                var tabbar = GetThreadTabBar();
                if(tabbar != null) action(tabbar);
            }
            StaticBroadcast(() => LocalTabBroadcast(action));
        }

        /// <summary>
        /// 本地标签栏广播
        /// </summary>
        public static void LocalTabBroadcast(Action<QTTabBarClass> action, Thread skip = null) {
            using(new Keychain(rwLockTabBar, false)) {
                foreach(var pair in dictTabInstances) {
                    if(pair.Key != skip) {
                        pair.Value.BeginInvoke(action, pair.Value);   
                    }
                }
            }
        }

        /// <summary>
        /// 按钮栏广播，将操作广播到所有按钮栏实例
        /// </summary>
        public static void ButtonBarBroadcast(Action<QTButtonBar> action, bool includeCurrent) {
            LocalBBarBroadcast(action, Thread.CurrentThread);
            if(includeCurrent) {
                var bbar = GetThreadButtonBar();
                if(bbar != null) action(bbar);
            }
            StaticBroadcast(() => LocalBBarBroadcast(action));
        }

        /// <summary>
        /// 本地按钮栏广播
        /// </summary>
        public static void LocalBBarBroadcast(Action<QTButtonBar> action, Thread skip = null) {
            using(new Keychain(rwLockBtnBar, false)) {
                foreach(var pair in dictBBarInstances) {
                    if(pair.Key != skip) {
                        pair.Value.BeginInvoke(action, pair.Value);
                    }
                }
            }
        }

        /// <summary>
        /// 在主进程上执行操作
        /// </summary>
        private static void ExecuteOnMainProcess(Action action, bool doAsync) {
            ICommService service = GetChannel();
            if(service == null || service.ExecuteOnMainProcess(DelToByte(action), doAsync)) {
                action();
            }
        }

        /// <summary>
        /// 确保操作在主进程执行
        /// </summary>
        public static bool EnsureMainProcess(Action action) {
            ICommService service = GetChannel();
            if(service != null && service.IsMainProcess()) return true;
            QTUtility2.log("InstanceManager EnsureMainProcess");
            ExecuteOnMainProcess(action, false);
            return false;
        }

        /// <summary>
        /// 在主进程同步调用标签栏操作
        /// </summary>
        public static void InvokeMain(Action<QTTabBarClass> action) {
            // QTUtility2.log("InstanceManager InvokeMain");
            ExecuteOnMainProcess(() => LocalInvokeMain(action), false);
        }

        /// <summary>
        /// 在主进程异步调用标签栏操作
        /// </summary>
        public static void BeginInvokeMain(Action<QTTabBarClass> action) {
            // QTUtility2.log("InstanceManager BeginInvokeMain");
            ExecuteOnMainProcess(() => LocalInvokeMain(action, true), true);
        }

        /// <summary>
        /// 本地调用主标签栏实例
        /// </summary>
        public static void LocalInvokeMain(Action<QTTabBarClass> action, bool doAsync = false) {
            QTTabBarClass instance;
            // 获取主进程的 QTTabBar的实例
            using(new Keychain(rwLockTabBar, false)) {
                instance = sdTabHandles.Count == 0 ? null : sdTabHandles.Peek();
            }
            if(instance == null) return;
            if(doAsync) {
                QTUtility2.log("异步调用:");
                instance.BeginInvoke(action, instance);    
            }
            else {
                QTUtility2.log("同步调用:" );
                instance.Invoke(action, instance);   
            }
        }

        /// <summary>
        /// 注册按钮栏实例
        /// </summary>
        public static void RegisterButtonBar(QTButtonBar bbar) {
            using(new Keychain(rwLockBtnBar, true)) {
                dictBBarInstances[Thread.CurrentThread] = bbar;
            }
        }


        /// <summary>
        /// 推送标签栏实例到服务端
        /// </summary>
        public static void PushTabBarInstance(QTTabBarClass tabbar) {
            IntPtr handle = tabbar.Handle;
            using(new Keychain(rwLockTabBar, true)) {
                dictTabInstances[Thread.CurrentThread] = tabbar;
                sdTabHandles.Push(handle, tabbar);
            }
            ICommService service = GetChannel();
            if(service != null) service.PushInstance(handle);
        }

        /// <summary>
        /// 注销按钮栏实例
        /// </summary>
        public static void UnregisterButtonBar() {
            using(new Keychain(rwLockBtnBar, true)) {
                dictBBarInstances.Remove(Thread.CurrentThread);
            }
        }

        /// <summary>
        /// 注销标签栏实例
        /// </summary>
        public static bool UnregisterTabBar() {
            using(new Keychain(rwLockTabBar, true)) {
                QTTabBarClass tabbar;
                if(dictTabInstances.TryGetValue(Thread.CurrentThread, out tabbar)) {
                    IntPtr handle = tabbar.Handle;
                    dictTabInstances.Remove(Thread.CurrentThread);
                    sdTabHandles.Remove(handle);
                    ICommService service = GetChannel();
                    if(service != null) service.DeleteInstance(handle);
                }
                return false;
            }
        }

        /// <summary>
        /// 获取所有实例总数
        /// </summary>
        public static int GetTotalInstanceCount() {
            ICommService service = GetChannel();
            return service == null ? dictTabInstances.Count : service.GetTotalInstanceCount();
        }

        // 多线程原子操作 
        private static bool UseInterLocked = false;
        // selectDict操作的互斥标志，用于防止多线程同时操作selectDict
        private static int inTimer = 0;
        // selectDict操作的锁对象
        private static object LockSelectDict = new object();

        /// <summary>
        /// 设置指定key的选中项列表
        /// </summary>
        public static void PutSelect(string key , List<string> list ) 
        {
            /*using(new Keychain(rwLockSelectDict, true))
            {
                selectDict[key] = list;
            }*/
            if (UseInterLocked)
            {
                if (Interlocked.Exchange(ref inTimer, 1) != 0)
                {
                    QTUtility2.log("拒绝进入");
                    return;
                }
            }
            try
            {
                lock (LockSelectDict)
                {
                    selectDict[key] = list;
                }
            }
            catch (Exception e)
            {
                QTUtility2.log("异常");
            }
            finally
            {
                if (UseInterLocked)
                {
                    Interlocked.Exchange(ref inTimer, 0);
                }
            }
        }

        /// <summary>
        /// 移除指定key的选中项
        /// </summary>
        public static void RemoveSelect(string key  ) 
        {
            /*using(new Keychain(rwLockSelectDict, true))
            {
                selectDict.Remove(key);
            }*/
            if (UseInterLocked)
            {
                if (Interlocked.Exchange(ref inTimer, 1) != 0)
                {
                    QTUtility2.log("拒绝进入");
                    return;
                }
            }
            try
            {
                lock (LockSelectDict)
                {
                    selectDict.Remove(key);
                }
            }
            catch (Exception e)
            {
                QTUtility2.log("异常");
            }
            finally
            {
                if (UseInterLocked)
                {
                    Interlocked.Exchange(ref inTimer, 0);
                }
            }
        }

        /// <summary>
        /// 获取指定key的选中项列表
        /// </summary>
        public static List<string> GetSelect(string key)
        {
            /*using (new Keychain(rwLockSelectDict, false))
            {
                List<string> list;
                return selectDict.TryGetValue(key, out list) ? list : null;
            }*/
            if (UseInterLocked)
            {
                if (Interlocked.Exchange(ref inTimer, 1) != 0)
                {
                    QTUtility2.log("拒绝进入");
                    return null;
                }
            }
            try
            {
                lock ( LockSelectDict  )
                {
                    List<string> list;
                    return selectDict.TryGetValue(key, out list) ? list : null;
                }
            }
            catch (Exception e)
            {
                QTUtility2.log("异常");
                return null;
            }
            finally
            {
                if (UseInterLocked)
                {
                    Interlocked.Exchange(ref inTimer, 0);
                }
            }
        }

        /// <summary>
        /// 获取当前线程的标签栏实例
        /// </summary>
        public static QTTabBarClass GetThreadTabBar() {
            using(new Keychain(rwLockTabBar, false)) {
                QTTabBarClass tab;
                return dictTabInstances.TryGetValue(Thread.CurrentThread, out tab) ? tab : null;
            }
        }

        /// <summary>
        /// 获取当前线程的按钮栏实例
        /// </summary>
        public static QTButtonBar GetThreadButtonBar() {
            using(new Keychain(rwLockBtnBar, false)) {
                QTButtonBar bbar;
                return dictBBarInstances.TryGetValue(Thread.CurrentThread, out bbar) ? bbar : null;
            }
        }

        /// <summary>
        /// 获取当前线程的按钮栏句柄
        /// </summary>
        public static bool TryGetButtonBarHandle(IntPtr explorerHandle, out IntPtr ptr) {
            // todo
            QTButtonBar bbar;
            if(dictBBarInstances.TryGetValue(Thread.CurrentThread, out bbar)) {
                ptr = bbar.Handle;
                return true;
            }
            ptr = IntPtr.Zero;
            return false;
        }

        /// <summary>
        /// 在服务进程上执行操作
        /// </summary>
        public static void ExecuteOnServerProcess(Action action, bool doAsync) {
            ICommService service;
            if(isServer || (service = GetChannel()) == null) {
                try {
                    action();
                }
                catch(Exception ex) {
                    QTUtility2.MakeErrorLog(ex);
                }
            }
            else {
                service.ExecuteOnServerProcess(DelToByte(action), doAsync);                
            }
        }

        /// <summary>
        /// 在服务进程上获取返回值
        /// </summary>
        public static T GetFromServerProcess<T>(Func<T> func) {
            ICommService service;
            if(isServer || (service = GetChannel()) == null) {
                try {
                    return func();
                }
                catch(Exception ex) {
                    QTUtility2.MakeErrorLog(ex);
                    return default(T);
                }
            }
            else {
                object obj = service.GetFromServerProcess(DelToByte(func));
                return obj == null ? default(T) : (T)obj;
            }
        }

        /// <summary>
        /// 添加托盘图标
        /// </summary>
        public static void AddToTrayIcon(IntPtr tabBarHandle, IntPtr explorerHandle, string currentPath, string[] tabNames, string[] tabPaths) {
            ICommService service = GetChannel();
            if(service != null) service.AddToTrayIcon(tabBarHandle, explorerHandle, currentPath, tabNames, tabPaths);
        }

        /// <summary>
        /// 移除托盘图标
        /// </summary>
        public static void RemoveFromTrayIcon(IntPtr tabBarHandle) {
            ICommService service = GetChannel();
            if (service != null)
            {
                service.RemoveFromTrayIcon(tabBarHandle);
            }
        }

        /// <summary>
        /// 选中其它标签栏的指定标签
        /// </summary>
        public static void SelectTabOnOtherTabBar(IntPtr tabBarHandle, int index) {
            ICommService service = GetChannel();
            if(service != null) service.SelectTabOnOtherTabBar(tabBarHandle, index);
        }

        /// <summary>
        /// 同步所有标签栏的工具栏颜色
        /// </summary>
        public static void SyncToolbarColorThreads()
        {
            IntPtr lParam = MCR.MAKELPARAM(1, 0);
            /*foreach (IntPtr explorerManager in (IEnumerable<IntPtr>)InstanceManager.ExplorerManagers())
            {
                if (PInvoke.IsWindow(explorerManager))
                {
                    PInvoke.PostMessage(explorerManager, 47616, (IntPtr)9, lParam);
                    lParam = IntPtr.Zero;
                }
            }*/
            foreach(var pair in dictTabInstances) {

                if (PInvoke.IsWindow(pair.Value.Handle))
                {
                    PInvoke.PostMessage(pair.Value.Handle, 47616, (IntPtr)9, lParam);
                    lParam = IntPtr.Zero;
                }
            }
        }


    }
}
