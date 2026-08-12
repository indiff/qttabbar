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
        // Selection dictionary: key is the path, value is the list of selected items
        private static Dictionary<string, List<string>> selectDict = new Dictionary<string, List<string>>();
        // Thread-to-QTTabBar map: one QTTabBarClass instance per thread
        private static Dictionary<Thread, QTTabBarClass> dictTabInstances = new Dictionary<Thread, QTTabBarClass>();
        // Thread-to-QTButtonBar map: one QTButtonBar instance per thread
        private static Dictionary<Thread, QTButtonBar> dictBBarInstances = new Dictionary<Thread, QTButtonBar>();
        // Handle-to-QTTabBar map, for looking up a QTTabBarClass by handle
        private static StackDictionary<IntPtr, QTTabBarClass> sdTabHandles = new StackDictionary<IntPtr, QTTabBarClass>();
        // Reader/writer lock for the button bar, for thread safety
        private static ReaderWriterLock rwLockBtnBar = new ReaderWriterLock();
        // Reader/writer lock for the tab bar, for thread safety
        private static ReaderWriterLock rwLockTabBar = new ReaderWriterLock();
        // Reader/writer lock for the selection dictionary, for thread safety
        private static ReaderWriterLock rwLockSelectDict = new ReaderWriterLock();

        // WCF duplex client, used for inter-process communication
        private static DuplexClient commClient;
        // Whether this is the main process
        private static bool isServer;

        // Variables used only by the main process
        // The WCF service host
        // Server-only stuff
        private static ServiceHost serviceHost;
        // Callback interfaces for every connected WCF client
        private static List<ICommClient> callbacks = new List<ICommClient>();
        // Handle-to-WCF-client-callback map
        private static StackDictionary<IntPtr, ICommClient> sdInstances = new StackDictionary<IntPtr, ICommClient>();

        // Tray icon manager
        private static TrayIcon trayIcon;
        // add by indiff
        // Reader/writer lock used for thread synchronization (added by indiff)
        private static ReaderWriterLockSlim rwLock = new ReaderWriterLockSlim();


        #region Comm Classes and Interfaces
        /// <summary>
        /// WCF duplex client implementation
        /// </summary>
        private class DuplexClient : DuplexClientBase<ICommService> {
            public DuplexClient(InstanceContext callbackInstance, Binding binding, EndpointAddress remoteAddress)
                : base(callbackInstance, binding, remoteAddress) {
            }
            public new ICommService Channel { get { return base.Channel; } }
        }

        /// <summary>
        /// WCF service contract - defines every method callable across processes
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
        /// WCF service implementation - handles all inter-process requests
        /// </summary>
        [ServiceBehavior(
                ConcurrencyMode = ConcurrencyMode.Reentrant,
                InstanceContextMode = InstanceContextMode.PerSession)]
        private class CommService : ICommService {

            /// <summary>
            /// Checks whether the client connection has dropped
            /// </summary>
            private static bool IsDead(ICommClient client) {
                ICommunicationObject ico = client as ICommunicationObject;
                return ico != null && ico.State != CommunicationState.Opened;                
            }
            /// <summary>
            /// Checks for and removes dropped client connections
            /// </summary>
            private static void CheckConnections() {
                callbacks.RemoveAll(IsDead);
                sdInstances.RemoveAllValues(c => !callbacks.Contains(c));
            }

            /// <summary>
            /// Gets the callback channel for the current operation
            /// </summary>
            private static ICommClient GetCallback() {
                return OperationContext.Current.GetCallbackChannel<ICommClient>();
            }

            /// <summary>
            /// Gets the total number of instances
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
        /// Serializes a delegate into a byte array
        /// </summary>
        private static byte[] DelToByte(Delegate del) {
            return QTUtility.ObjectToByteArray(new SerializeDelegate(del));
        }

        /// <summary>
        /// Deserializes a byte array back into a delegate
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
        /// Initializes inter-process communication and instance synchronization
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
            // A WCF channel cannot be opened on a thread with a message loop or it deadlocks, so use a new thread
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
                // Thread finished
            });
            thread.Start();
            lock(thread) {
                Monitor.Wait(thread);
            }            
        }

        /// <summary>
        /// Gets the WCF communication channel
        /// </summary>
        private static ICommService GetChannel() {
            if(commClient.State != CommunicationState.Opened) {
                Initialize(true);
            }
            return commClient.State == CommunicationState.Opened ? commClient.Channel : null;
        }

        /// <summary>
        /// Static broadcast - sends the operation to every instance
        /// </summary>
        public static void StaticBroadcast(Action action) {
            ICommService service = GetChannel();
            if(service != null) service.Broadcast(DelToByte(action));
        }

        /// <summary>
        /// Tab bar broadcast - sends the operation to every tab bar instance
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
        /// Local tab bar broadcast
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
        /// Button bar broadcast - sends the operation to every button bar instance
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
        /// Local button bar broadcast
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
        /// Runs the operation on the main process
        /// </summary>
        private static void ExecuteOnMainProcess(Action action, bool doAsync) {
            ICommService service = GetChannel();
            if(service == null || service.ExecuteOnMainProcess(DelToByte(action), doAsync)) {
                action();
            }
        }

        /// <summary>
        /// Ensures the operation runs on the main process
        /// </summary>
        public static bool EnsureMainProcess(Action action) {
            ICommService service = GetChannel();
            if(service != null && service.IsMainProcess()) return true;
            QTUtility2.log("InstanceManager EnsureMainProcess");
            ExecuteOnMainProcess(action, false);
            return false;
        }

        /// <summary>
        /// Synchronously invokes a tab bar operation on the main process
        /// </summary>
        public static void InvokeMain(Action<QTTabBarClass> action) {
            ExecuteOnMainProcess(() => LocalInvokeMain(action), false);
        }

        /// <summary>
        /// Asynchronously invokes a tab bar operation on the main process
        /// </summary>
        public static void BeginInvokeMain(Action<QTTabBarClass> action) {
            ExecuteOnMainProcess(() => LocalInvokeMain(action, true), true);
        }

        /// <summary>
        /// Invokes the main tab bar instance locally
        /// </summary>
        public static void LocalInvokeMain(Action<QTTabBarClass> action, bool doAsync = false) {
            QTTabBarClass instance;
            // Get the QTTabBar class instance for the current thread
            using(new Keychain(rwLockTabBar, false)) {
                instance = sdTabHandles.Count == 0 ? null : sdTabHandles.Peek();
            }
            if(instance == null) return;
            if(doAsync) {
                QTUtility2.log("Async call:");
                instance.BeginInvoke(action, instance);    
            }
            else {
                QTUtility2.log("Sync call:" );
                instance.Invoke(action, instance);   
            }
        }

        /// <summary>
        /// Registers a button bar instance
        /// </summary>
        public static void RegisterButtonBar(QTButtonBar bbar) {
            using(new Keychain(rwLockBtnBar, true)) {
                dictBBarInstances[Thread.CurrentThread] = bbar;
            }
        }


        /// <summary>
        /// Pushes a tab bar instance to the service host
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
        /// Unregisters a button bar instance
        /// </summary>
        public static void UnregisterButtonBar() {
            using(new Keychain(rwLockBtnBar, true)) {
                dictBBarInstances.Remove(Thread.CurrentThread);
            }
        }

        /// <summary>
        /// Unregisters a tab bar instance
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
        /// Gets the total number of instances
        /// </summary>
        public static int GetTotalInstanceCount() {
            ICommService service = GetChannel();
            return service == null ? dictTabInstances.Count : service.GetTotalInstanceCount();
        }

        // Atomic operations for multithreading 
        private static bool UseInterLocked = false;
        // Mutex flag guarding selectDict against concurrent access
        private static int inTimer = 0;
        // Lock object for selectDict operations
        private static object LockSelectDict = new object();

        /// <summary>
        /// Sets the selection list for the given key
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
                    QTUtility2.log("Access denied");
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
                QTUtility2.log("Exception");
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
        /// Removes the selection for the given key
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
                    QTUtility2.log("Access denied");
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
                QTUtility2.log("Exception");
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
        /// Gets the selection list for the given key
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
                    QTUtility2.log("Access denied");
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
                QTUtility2.log("Exception");
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
        /// Gets the tab bar instance for the current thread
        /// </summary>
        public static QTTabBarClass GetThreadTabBar() {
            using(new Keychain(rwLockTabBar, false)) {
                QTTabBarClass tab;
                return dictTabInstances.TryGetValue(Thread.CurrentThread, out tab) ? tab : null;
            }
        }

        /// <summary>
        /// Gets the button bar instance for the current thread
        /// </summary>
        public static QTButtonBar GetThreadButtonBar() {
            using(new Keychain(rwLockBtnBar, false)) {
                QTButtonBar bbar;
                return dictBBarInstances.TryGetValue(Thread.CurrentThread, out bbar) ? bbar : null;
            }
        }

        /// <summary>
        /// Gets the button bar handle for the current thread
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
        /// Runs the operation on the service process
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
        /// Gets a return value from the service process
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
        /// Adds the tray icon
        /// </summary>
        public static void AddToTrayIcon(IntPtr tabBarHandle, IntPtr explorerHandle, string currentPath, string[] tabNames, string[] tabPaths) {
            ICommService service = GetChannel();
            if(service != null) service.AddToTrayIcon(tabBarHandle, explorerHandle, currentPath, tabNames, tabPaths);
        }

        /// <summary>
        /// Removes the tray icon
        /// </summary>
        public static void RemoveFromTrayIcon(IntPtr tabBarHandle) {
            ICommService service = GetChannel();
            if (service != null)
            {
                service.RemoveFromTrayIcon(tabBarHandle);
            }
        }

        /// <summary>
        /// Selects the given tab on another tab bar
        /// </summary>
        public static void SelectTabOnOtherTabBar(IntPtr tabBarHandle, int index) {
            ICommService service = GetChannel();
            if(service != null) service.SelectTabOnOtherTabBar(tabBarHandle, index);
        }

        /// <summary>
        /// Syncs the toolbar colour across every tab bar
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
