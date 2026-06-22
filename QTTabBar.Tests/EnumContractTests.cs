using Microsoft.VisualStudio.TestTools.UnitTesting;
using QTTabBarLib;

namespace QTTabBar.Tests {
    // BindAction ordinal values are stored as integers in user settings.
    // The source has an explicit WARNING: reordering breaks existing settings.
    // These tests catch accidental renumbering during refactors.
    [TestClass]
    public class EnumContractTests {
        [TestMethod] public void BindAction_GoBack_Is0()               => Assert.AreEqual(0,  (int)BindAction.GoBack);
        [TestMethod] public void BindAction_GoForward_Is1()            => Assert.AreEqual(1,  (int)BindAction.GoForward);
        [TestMethod] public void BindAction_GoFirst_Is2()              => Assert.AreEqual(2,  (int)BindAction.GoFirst);
        [TestMethod] public void BindAction_GoLast_Is3()               => Assert.AreEqual(3,  (int)BindAction.GoLast);
        [TestMethod] public void BindAction_NextTab_Is4()              => Assert.AreEqual(4,  (int)BindAction.NextTab);
        [TestMethod] public void BindAction_PreviousTab_Is5()          => Assert.AreEqual(5,  (int)BindAction.PreviousTab);
        [TestMethod] public void BindAction_FirstTab_Is6()             => Assert.AreEqual(6,  (int)BindAction.FirstTab);
        [TestMethod] public void BindAction_LastTab_Is7()              => Assert.AreEqual(7,  (int)BindAction.LastTab);
        [TestMethod] public void BindAction_SwitchToLastActivated_Is8() => Assert.AreEqual(8, (int)BindAction.SwitchToLastActivated);
        [TestMethod] public void BindAction_NewTab_Is9()               => Assert.AreEqual(9,  (int)BindAction.NewTab);
        [TestMethod] public void BindAction_NewWindow_Is10()           => Assert.AreEqual(10, (int)BindAction.NewWindow);
        [TestMethod] public void BindAction_MergeWindows_Is11()        => Assert.AreEqual(11, (int)BindAction.MergeWindows);
        [TestMethod] public void BindAction_CloseCurrent_Is12()        => Assert.AreEqual(12, (int)BindAction.CloseCurrent);
        [TestMethod] public void BindAction_CloseAllButCurrent_Is13()  => Assert.AreEqual(13, (int)BindAction.CloseAllButCurrent);
        [TestMethod] public void BindAction_CloseLeft_Is14()           => Assert.AreEqual(14, (int)BindAction.CloseLeft);
        [TestMethod] public void BindAction_CloseRight_Is15()          => Assert.AreEqual(15, (int)BindAction.CloseRight);
        [TestMethod] public void BindAction_CloseWindow_Is16()         => Assert.AreEqual(16, (int)BindAction.CloseWindow);
        [TestMethod] public void BindAction_RestoreLastClosed_Is17()   => Assert.AreEqual(17, (int)BindAction.RestoreLastClosed);
        [TestMethod] public void BindAction_CloneCurrent_Is18()        => Assert.AreEqual(18, (int)BindAction.CloneCurrent);
        [TestMethod] public void BindAction_TearOffCurrent_Is19()      => Assert.AreEqual(19, (int)BindAction.TearOffCurrent);
        [TestMethod] public void BindAction_LockCurrent_Is20()         => Assert.AreEqual(20, (int)BindAction.LockCurrent);

        [TestMethod]
        public void MouseChord_FlagsArePowersOfTwo() {
            Assert.AreEqual(0,   (int)MouseChord.None);
            Assert.AreEqual(1,   (int)MouseChord.Shift);
            Assert.AreEqual(2,   (int)MouseChord.Ctrl);
            Assert.AreEqual(4,   (int)MouseChord.Alt);
            Assert.AreEqual(8,   (int)MouseChord.Left);
            Assert.AreEqual(16,  (int)MouseChord.Right);
            Assert.AreEqual(32,  (int)MouseChord.Middle);
            Assert.AreEqual(64,  (int)MouseChord.Double);
            Assert.AreEqual(128, (int)MouseChord.X1);
            Assert.AreEqual(256, (int)MouseChord.X2);
        }

        [TestMethod]
        public void MouseChord_CombinationsDoNotOverlap() {
            MouseChord ctrlShift = MouseChord.Ctrl | MouseChord.Shift;
            Assert.AreEqual(3, (int)ctrlShift);
            Assert.IsTrue(ctrlShift.HasFlag(MouseChord.Ctrl));
            Assert.IsTrue(ctrlShift.HasFlag(MouseChord.Shift));
            Assert.IsFalse(ctrlShift.HasFlag(MouseChord.Alt));
        }
    }
}
