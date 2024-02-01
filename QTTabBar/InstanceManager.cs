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
using QTTabBarLib.Interop;

namespace QTTabBarLib {
    internal static class InstanceManager {
        private static Dictionary<string, List<string>> selectDict = new Dictionary<string, List<string>>();
        private static Dictionary<Thread, QTTabBarClass> dictTabInstances = new Dictionary<Thread, QTTabBarClass>();
        private static Dictionary<Thread, QTButtonBar> dictBBarInstances = new Dictionary<Thread, QTButtonBar>();
        private static StackDictionary<IntPtr, QTTabBarClass> sdTabHandles = new StackDictionary<IntPtr, QTTabBarClass>();
        private static ReaderWriterLock rwLockBtnBar = new ReaderWriterLock();
        private static ReaderWriterLock rwLockTabBar = new ReaderWriterLock();
        private static ReaderWriterLock rwLockSelectDict = new ReaderWriterLock();



        private static DuplexClient commClient;
        private static bool isServer;

        // Server-only stuff
        private static ServiceHost serviceHost;
        private static List<ICommClient> callbacks = new List<ICommClient>();
        private static StackDictionary<IntPtr, ICommClient> sdInstances = new StackDictionary<IntPtr, ICommClient>();
        private static TrayIcon trayIcon;
        // add by indiff
        private static ReaderWriterLockSlim rwLock = new ReaderWriterLockSlim();


        #region Comm Classes and Interfaces

        private class DuplexClient : DuplexClientBase<ICommService> {
            public DuplexClient(InstanceContext callbackInstance, Binding binding, EndpointAddress remoteAddress)
                : base(callbackInstance, binding, remoteAddress) {
            }
            public new ICommService Channel { get { return base.Channel; } }
        }

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

        [ServiceBehavior(
                ConcurrencyMode = ConcurrencyMode.Reentrant,
                InstanceContextMode = InstanceContextMode.PerSession)]
        private class CommService : ICommService {

            private static bool IsDead(ICommClient client) {
                ICommunicationObject ico = client as ICommunicationObject;
                return ico != null && ico.State != CommunicationState.Opened;                
            }

            private static void CheckConnections() {
                callbacks.RemoveAll(IsDead);
                sdInstances.RemoveAllValues(c => !callbacks.Contains(c));
            }

            private static ICommClient GetCallback() {
                return OperationContext.Current.GetCallbackChannel<ICommClient>();
            }

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
                    QTUtility2.log("ExecuteOnMainProcess callback.Execute doAsync");
                    // if (!IsDead( callback ))
                    // {
                        AsyncHelper.BeginInvoke(new Action(() => {
                            try {
                                if (!IsDead(callback))
                                {
                                    callback.Execute(encodedAction);
                                }
                            }
                            catch(Exception e) {
                                QTUtility2.MakeErrorLog(e, "AsyncHelper.BeginInvoke");
                            }
                        }));
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
                            AsyncHelper.BeginInvoke(action);
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
                AsyncHelper.BeginInvoke(new Action(() => {
                    int i = 0;
                    foreach(ICommClient target in targets) {
                        try {
                            i++;
                            // QTUtility2.log("CommService Broadcast count : " + targets.Count + " handle index: " + i);
                            if (!IsDead(target)) {
                                target.Execute(encodedAction);
                            }
                        }
                        catch (Exception ex)
                        {
                            QTUtility2.MakeErrorLog(ex);
                        }
                    }

                    // TimeSpan abs2 = new TimeSpan(DateTime.Now.Ticks).Subtract(start).Duration();
                    // QTUtility2.log(string.Format("Broadcast async cost {0} ", abs2.TotalMilliseconds));
                }));

                // TimeSpan abs = new TimeSpan(DateTime.Now.Ticks).Subtract(start).Duration();
                // QTUtility2.log(string.Format("Broadcast sync cost {0} ", abs.TotalMilliseconds));
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
                try {
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
                catch(Exception ex) {
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
            }
        }

        #endregion

        #region Utility Methods

        private static byte[] DelToByte(Delegate del) {
            return QTUtility.ObjectToByteArray(new SerializeDelegate(del));
        }

        private static Delegate ByteToDel(byte[] buf) {
            if (buf == null || buf.Length == 0 ) { return null; }
            object v = QTUtility.ByteArrayToObject(buf);
            if (v == null) { return null; }
            return ((SerializeDelegate)v).Delegate;
            // return BinaryPack.BinaryConverter.Deserialize<SerializeDelegate>(buf);
        }

        #endregion

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
                catch(EndpointNotFoundException) {
                }
                lock(thread) {
                    Monitor.Pulse(thread);
                }
                // Yes, we can just let the thread die here.
            });
            thread.Start();
            lock(thread) {
                Monitor.Wait(thread);
            }            
        }

        private static ICommService GetChannel() {
            if(commClient.State != CommunicationState.Opened) {
                Initialize(true);
            }
            return commClient.State == CommunicationState.Opened ? commClient.Channel : null;
        }

        public static void StaticBroadcast(Action action) {
            ICommService service = GetChannel();
            if(service != null) service.Broadcast(DelToByte(action));
        }

        public static void TabBarBroadcast(Action<QTTabBarClass> action, bool includeCurrent) {
            LocalTabBroadcast(action, Thread.CurrentThread);
            if(includeCurrent) {
                var tabbar = GetThreadTabBar();
                if(tabbar != null) action(tabbar);
            }
            StaticBroadcast(() => LocalTabBroadcast(action));
        }

        public static void LocalTabBroadcast(Action<QTTabBarClass> action, Thread skip = null) {
            using(new Keychain(rwLockTabBar, false)) {
                foreach(var pair in dictTabInstances) {
                    if(pair.Key != skip) {
                        pair.Value.BeginInvoke(action, pair.Value);   
                    }
                }
            }
        }

        public static void ButtonBarBroadcast(Action<QTButtonBar> action, bool includeCurrent) {
            LocalBBarBroadcast(action, Thread.CurrentThread);
            if(includeCurrent) {
                var bbar = GetThreadButtonBar();
                if(bbar != null) action(bbar);
            }
            StaticBroadcast(() => LocalBBarBroadcast(action));
        }

        public static void LocalBBarBroadcast(Action<QTButtonBar> action, Thread skip = null) {
            using(new Keychain(rwLockBtnBar, false)) {
                foreach(var pair in dictBBarInstances) {
                    if(pair.Key != skip) {
                        pair.Value.BeginInvoke(action, pair.Value);
                    }
                }
            }
        }

        private static void ExecuteOnMainProcess(Action action, bool doAsync) {
            ICommService service = GetChannel();
            if(service == null || service.ExecuteOnMainProcess(DelToByte(action), doAsync)) {
                action();
            }
        }

        public static bool EnsureMainProcess(Action action) {
            ICommService service = GetChannel();
            if(service != null && service.IsMainProcess()) return true;
            QTUtility2.log("InstanceManager EnsureMainProcess");
            ExecuteOnMainProcess(action, false);
            return false;
        }

        public static void InvokeMain(Action<QTTabBarClass> action) {
            // QTUtility2.log("InstanceManager InvokeMain");
            ExecuteOnMainProcess(() => LocalInvokeMain(action), false);
        }

        public static void BeginInvokeMain(Action<QTTabBarClass> action) {
            // QTUtility2.log("InstanceManager BeginInvokeMain");
            ExecuteOnMainProcess(() => LocalInvokeMain(action, true), true);
        }

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

        public static void RegisterButtonBar(QTButtonBar bbar) {
            using(new Keychain(rwLockBtnBar, true)) {
                dictBBarInstances[Thread.CurrentThread] = bbar;
            }
        }

        

        public static void PushTabBarInstance(QTTabBarClass tabbar) {
            IntPtr handle = tabbar.Handle;
            using(new Keychain(rwLockTabBar, true)) {
                dictTabInstances[Thread.CurrentThread] = tabbar;
                sdTabHandles.Push(handle, tabbar);
            }
            ICommService service = GetChannel();
            if(service != null) service.PushInstance(handle);
        }

        public static void UnregisterButtonBar() {
            using(new Keychain(rwLockBtnBar, true)) {
                dictBBarInstances.Remove(Thread.CurrentThread);
            }
        }

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

        public static int GetTotalInstanceCount() {
            ICommService service = GetChannel();
            return service == null ? dictTabInstances.Count : service.GetTotalInstanceCount();
        }
        private static int inTimer = 0;
        private static object LockSelectDict = new object();

        public static void PutSelect(string key , List<string> list ) 
        {
            /*using(new Keychain(rwLockSelectDict, true))
            {
                selectDict[key] = list;
            }*/
            if (Interlocked.Exchange(ref inTimer, 1) != 0)
            {
                QTUtility2.log("拒绝进入");
                return;
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
                Interlocked.Exchange(ref inTimer, 0);
            }
        }

        public static void RemoveSelect(string key  ) 
        {
            /*using(new Keychain(rwLockSelectDict, true))
            {
                selectDict.Remove(key);
            }*/
            if (Interlocked.Exchange(ref inTimer, 1) != 0)
            {
                QTUtility2.log("拒绝进入");
                return;
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
                Interlocked.Exchange(ref inTimer, 0);
            }
        }

        public static List<string> GetSelect(string key)
        {
            /*using (new Keychain(rwLockSelectDict, false))
            {
                List<string> list;
                return selectDict.TryGetValue(key, out list) ? list : null;
            }*/
            if (Interlocked.Exchange(ref inTimer, 1) != 0)
            {
                QTUtility2.log("拒绝进入");
                return null;
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
                Interlocked.Exchange(ref inTimer, 0);
            }
        }

        public static QTTabBarClass GetThreadTabBar() {
            using(new Keychain(rwLockTabBar, false)) {
                QTTabBarClass tab;
                return dictTabInstances.TryGetValue(Thread.CurrentThread, out tab) ? tab : null;
            }
        }

        public static QTButtonBar GetThreadButtonBar() {
            using(new Keychain(rwLockBtnBar, false)) {
                QTButtonBar bbar;
                return dictBBarInstances.TryGetValue(Thread.CurrentThread, out bbar) ? bbar : null;
            }
        }

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

        public static void AddToTrayIcon(IntPtr tabBarHandle, IntPtr explorerHandle, string currentPath, string[] tabNames, string[] tabPaths) {
            ICommService service = GetChannel();
            if(service != null) service.AddToTrayIcon(tabBarHandle, explorerHandle, currentPath, tabNames, tabPaths);
        }

        public static void RemoveFromTrayIcon(IntPtr tabBarHandle) {
            ICommService service = GetChannel();
            if (service != null)
            {
                service.RemoveFromTrayIcon(tabBarHandle);
            }
        }

        public static void SelectTabOnOtherTabBar(IntPtr tabBarHandle, int index) {
            ICommService service = GetChannel();
            if(service != null) service.SelectTabOnOtherTabBar(tabBarHandle, index);
        }


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
