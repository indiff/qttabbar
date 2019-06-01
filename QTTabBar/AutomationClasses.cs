//    This file is part of QTTabBar, a shell extension for Microsoft
//    Windows Explorer.
//    Copyright (C) 2007-2010  Quizo, Paul Accisano
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
using System.Drawing;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Threading;
using QTTabBarLib.Interop;

namespace QTTabBarLib {
    // All interaction with AutomationElements MUST be done in a thread other
    // than the UI thread.  Use this class to execute code in the Automation 
    // thread.
    // http://msdn.microsoft.com/en-us/library/ee671692%28VS.85%29.aspx

    public static class AutomationManager {
        private static readonly Guid IID_IUIAutomation = new Guid("{30CBE57D-D9D0-452A-AB13-7AC5AC4825EE}");
        private static readonly Guid CLSID_CUIAutomation = new Guid("{FF48DBA4-60EF-4201-AA87-54103EEF594E}");
        private static IUIAutomation pAutomation;
        private static Dispatcher automationDispatch;
        public delegate T Query<out T>(AutomationElementFactory factory);

        static AutomationManager() {
            Guid rclsid = CLSID_CUIAutomation;
            Guid riid = IID_IUIAutomation;
            object obj;
            PInvoke.CoCreateInstance(ref rclsid, IntPtr.Zero, 1, ref riid, out obj);
            pAutomation = obj as IUIAutomation;
            Thread automationThread = new Thread(Dispatcher.Run) { IsBackground = true };
            automationThread.Start();
            while(true) {
                automationDispatch = Dispatcher.FromThread(automationThread);
                if(automationDispatch != null) break;
                Thread.Sleep(50);
            }
        }

        public static T DoQuery<T>(Query<T> query) {
            try {
                return (T)automationDispatch.Invoke(new Func<T>(() => {
                    try {
                        using(AutomationElementFactory factory = new AutomationElementFactory(pAutomation)) {
                            return query(factory);
                        }
                    }
                    catch(Exception exception) {
                        QTUtility2.MakeErrorLog(exception, "Automation Thread");
                        return default(T);
                    }
                }), DispatcherPriority.Normal);                
            }
            catch {
                return default(T);
            }
        }
    }

    // This class is used to create AutomationElements
    public class AutomationElementFactory : IDisposable {
        private List<AutomationElement> disposeList;
        private IUIAutomation pAutomation;

        internal AutomationElementFactory(IUIAutomation pAutomation) {
            disposeList = new List<AutomationElement>();
            this.pAutomation = pAutomation;
        }

        internal void AddToDisposeList(AutomationElement elem) {
            disposeList.Add(elem);
        }

        public IUIAutomationTreeWalker CreateTreeWalker() {
            IUIAutomationTreeWalker walker;
            pAutomation.get_ControlViewWalker(out walker);
            return walker;
        }

        public void Dispose() {
            foreach(AutomationElement elem in disposeList) {
                elem.Dispose();
            }
            disposeList.Clear();
        }

        public AutomationElement FromHandle(IntPtr hwnd) {
            try {
                IUIAutomationElement pElement;
                pAutomation.ElementFromHandle(hwnd, out pElement);
                return pElement == null ? null : new AutomationElement(pElement, this);
            }
            catch(COMException) {
                return null;
            }
        }

        public AutomationElement FromPoint(Point pt) {
            try {
                IUIAutomationElement pElement;
                pAutomation.ElementFromPoint(pt, out pElement);
                return pElement == null ? null : new AutomationElement(pElement, this);
            }
            catch(COMException) {
                return null;
            }
        }

        public AutomationElement FromKeyboardFocus() {
            try {
                IUIAutomationElement pElement;
                pAutomation.GetFocusedElement(out pElement);
                return pElement == null ? null : new AutomationElement(pElement, this);
            }
            catch(COMException) {
                return null;
            }
        }
    }

    public class AutomationElement : IDisposable {
        private static readonly Guid IID_IUIAutomationRegistrar = new Guid("{8609C4EC-4A1A-4D88-A357-5A66E060E1CF}");
        private static readonly Guid CLSID_CUIAutomationRegistrar = new Guid("{6E29FABF-9977-42D1-8D0E-CA7E61AD87E6}");
        private static readonly Guid ItemCount_Property_GUID = new Guid("{ABBF5C45-5CCC-47b7-BB4E-87CB87BBD162}");
        private static readonly Guid SelectedItemCount_Property_GUID = new Guid("{8FE316D2-0E52-460a-9C1E-48F273D470A3}");
        private static readonly Guid ItemIndex_Property_GUID = new Guid("{92A053DA-2969-4021-BF27-514CFC2E4A69}");

        private const int UIAutomationType_Int = 1;
        private const int UIA_ScrollPatternId = 10004;
        private const int UIA_GridPatternId = 10006;
        private const int UIA_SelectionItemPatternId = 10010;
        private static int UIA_ItemCountPropertyId;
        private static int UIA_SelectedCountPropertyId;
        private static int UIA_ItemIndexPropertyId;
        private const int UIA_BoundingRectanglePropertyId = 30001;
        private const int UIA_NamePropertyId = 30005;
        private const int UIA_AutomationIdPropertyId = 30011;
        private const int UIA_ClassNamePropertyId = 30012;

        AutomationElementFactory factory;
        IUIAutomationElement pElement;

        static AutomationElement() {
            // Register the ItemsView custom properties, as documented here:
            // http://msdn.microsoft.com/en-us/library/ff486373%28VS.85%29.aspx

            Guid rclsid = CLSID_CUIAutomationRegistrar;
            Guid riid = IID_IUIAutomationRegistrar;
            object obj;
            PInvoke.CoCreateInstance(ref rclsid, IntPtr.Zero, 1, ref riid, out obj);
            IUIAutomationRegistrar pRegistrar = obj as IUIAutomationRegistrar;
            if(pRegistrar == null) {
                return;
            }
            try {
                UIAutomationPropertyInfo propinfo = new UIAutomationPropertyInfo {
                    guid = ItemCount_Property_GUID,
                    pProgrammaticName = "ItemCount",
                    type = UIAutomationType_Int
                };
                pRegistrar.RegisterProperty(ref propinfo, out UIA_ItemCountPropertyId);
                propinfo = new UIAutomationPropertyInfo {
                    guid = SelectedItemCount_Property_GUID,
                    pProgrammaticName = "SelectedItemCount",
                    type = UIAutomationType_Int
                };
                pRegistrar.RegisterProperty(ref propinfo, out UIA_SelectedCountPropertyId);
                propinfo = new UIAutomationPropertyInfo {
                    guid = ItemIndex_Property_GUID,
                    pProgrammaticName = "ItemIndex",
                    type = UIAutomationType_Int
                };
                pRegistrar.RegisterProperty(ref propinfo, out UIA_ItemIndexPropertyId);
            }
            finally {
                Marshal.ReleaseComObject(pRegistrar);
            }
        }

        internal AutomationElement(IUIAutomationElement pElement, AutomationElementFactory factory) {
            this.factory = factory;
            this.pElement = pElement;
            factory.AddToDisposeList(this);
        }

        ~AutomationElement() {
            Dispose();
        }

        public void Dispose() {
            if(pElement != null) {
                Marshal.ReleaseComObject(pElement);
                pElement = null;
            }
            GC.SuppressFinalize(this);
        }


        public AutomationElement FindMatchingChild(Predicate<AutomationElement> pred) {
            IUIAutomationTreeWalker walker = factory.CreateTreeWalker();
            try {
                IUIAutomationElement elem;
                walker.GetFirstChildElement(pElement, out elem);
                while(elem != null) {
                    AutomationElement ae = new AutomationElement(elem, factory);
                    if(pred(ae)) {
                        return ae;
                    }
                    IUIAutomationElement next;
                    walker.GetNextSiblingElement(elem, out next);
                    elem = next;
                }
                return null;
            }
            catch(COMException) {
                return null;
            }
            finally {
                if(walker != null) Marshal.ReleaseComObject(walker);
            }
        }

        public bool IsSelected() {
            object obj;
            pElement.GetCurrentPattern(UIA_SelectionItemPatternId, out obj);
            try {
                if(obj == null) {
                    return false;
                }
                IUIAutomationSelectionItemPattern selprov = obj as IUIAutomationSelectionItemPattern;
                bool ret;
                selprov.get_CurrentIsSelected(out ret);
                return ret;
            }
            catch(COMException) {
                return false;
            }
            finally {
                if(obj != null) Marshal.ReleaseComObject(obj);
            }
        }

        public string GetAutomationId() {
            try {
                object obj;
                pElement.GetCurrentPropertyValue(UIA_AutomationIdPropertyId, out obj);
                return obj.ToString();
            }
            catch(COMException) {
                return "";
            }
        }

        public Rectangle GetBoundingRect() {
            try {
                object obj;
                pElement.GetCurrentPropertyValue(UIA_BoundingRectanglePropertyId, out obj);
                double[] rect = obj as double[];
                return rect == null ?
                    new Rectangle(0, 0, 0, 0) :
                    new Rectangle((int)rect[0], (int)rect[1], (int)rect[2], (int)rect[3]);
            }
            catch(COMException) {
                return new Rectangle(0, 0, 0, 0);
            }
        }

        public string GetClassName() {
            try {
                object obj;
                pElement.GetCurrentPropertyValue(UIA_ClassNamePropertyId, out obj);
                return obj.ToString();
            }
            catch(COMException) {
                return "";
            }
        }

        public IEnumerable<AutomationElement> GetChildren() {
            IUIAutomationElement elem = null;
            IUIAutomationTreeWalker walker = factory.CreateTreeWalker();
            try {
                try {
                    walker.GetFirstChildElement(pElement, out elem);
                }
                catch(COMException) {
                    if(elem != null) {
                        Marshal.ReleaseComObject(elem);
                    }
                    yield break;
                }
                while(elem != null) {
                    yield return new AutomationElement(elem, factory);
                    IUIAutomationElement next = null;
                    try {
                        walker.GetNextSiblingElement(elem, out next);
                    }
                    catch(COMException) {
                        if(next != null) {
                            Marshal.ReleaseComObject(next);
                        }
                        yield break;
                    }
                    elem = next;
                }
            }
            finally {
                if(walker != null) Marshal.ReleaseComObject(walker);
            }
        }

        public int GetColumnCount() {
            object obj = null;
            try {
                pElement.GetCurrentPattern(UIA_GridPatternId, out obj);
                if(obj == null) return -1;
                int ret;
                ((IUIAutomationGridPattern)obj).get_CurrentColumnCount(out ret);
                return ret;
            }
            catch(COMException) {
                return -1;
            }
            finally {
                if(obj != null) Marshal.ReleaseComObject(obj);
            }
        }

        public AutomationElement GetFirstChild() {
            IUIAutomationTreeWalker walker = factory.CreateTreeWalker();
            try {
                IUIAutomationElement elem;
                walker.GetFirstChildElement(pElement, out elem);
                return (elem == null) ? null : new AutomationElement(elem, factory);
            }
            catch(COMException) {
                return null;
            }
            finally {
                if(walker != null) Marshal.ReleaseComObject(walker);
            }
        }

        // Only valid for ItemsView element.
        public int GetItemCount() {
            try {
                object obj;
                pElement.GetCurrentPropertyValue(UIA_ItemCountPropertyId, out obj);
                return (int)obj;
            }
            catch(COMException) {
                return 0;
            }
        }

        // Only valid for ListItem element.
        public int GetItemIndex() {
            try {
                object obj;
                pElement.GetCurrentPropertyValue(UIA_ItemIndexPropertyId, out obj);
                return (int)obj - 1;
            }
            catch(COMException) {
                return -1;
            }
        }

        public string GetItemName() {
            try {
                object obj;
                pElement.GetCurrentPropertyValue(UIA_NamePropertyId, out obj);
                return obj.ToString();
            }
            catch(COMException) {
                return "";
            }
        }

        public AutomationElement GetNextSibling() {
            IUIAutomationTreeWalker walker = factory.CreateTreeWalker();
            try {
                IUIAutomationElement elem;
                walker.GetNextSiblingElement(pElement, out elem);
                return (elem == null) ? null : new AutomationElement(elem, factory);
            }
            catch(COMException) {
                return null;
            }
            finally {
                if(walker != null) Marshal.ReleaseComObject(walker);
            }
        }

        public AutomationElement GetParent() {
            IUIAutomationTreeWalker walker = factory.CreateTreeWalker();
            try {
                IUIAutomationElement elem;
                walker.GetParentElement(pElement, out elem);
                return (elem == null) ? null : new AutomationElement(elem, factory);
            }
            catch(COMException) {
                return null;
            }
            finally {
                if(walker != null) Marshal.ReleaseComObject(walker);
            }
        }

        public int GetRowCount() {
            object obj = null;
            try {
                pElement.GetCurrentPattern(UIA_GridPatternId, out obj);
                if(obj == null) return -1;
                int ret;
                ((IUIAutomationGridPattern)obj).get_CurrentRowCount(out ret);
                return ret;
            }
            catch(COMException) {
                return -1;
            }
            finally {
                if(obj != null) Marshal.ReleaseComObject(obj);
            }
        }

        // Only valid for ItemsView element.
        public int GetSelectedCount() {
            try {
                object obj;
                pElement.GetCurrentPropertyValue(UIA_SelectedCountPropertyId, out obj);
                return (int)obj;
            }
            catch(COMException) {
                return 0;
            }
        }

        public void ScrollHorizontal(ScrollAmount amount, int times) {
            Scroll(amount, times, ScrollAmount.NoAmount, 0);
        }

        public void ScrollVertical(ScrollAmount amount, int times) {
            Scroll(ScrollAmount.NoAmount, 0, amount, times);
        }

        public void Scroll(ScrollAmount horizontal, int hTimes, ScrollAmount vertical, int vTimes) {
            object obj;
            pElement.GetCurrentPattern(UIA_ScrollPatternId, out obj);
            try {
                if(obj == null) {
                    return;
                }
                IUIAutomationScrollPattern scrollPattern = obj as IUIAutomationScrollPattern;
                while(hTimes > 0 || vTimes > 0) {
                    scrollPattern.Scroll(
                        hTimes > 0 ? horizontal : ScrollAmount.NoAmount,
                        vTimes > 0 ? vertical : ScrollAmount.NoAmount);
                    --hTimes;
                    --vTimes;
                }
            }
            catch(COMException) {
            }
            finally {
                if(obj != null) Marshal.ReleaseComObject(obj);
            }
        }
    }
}
