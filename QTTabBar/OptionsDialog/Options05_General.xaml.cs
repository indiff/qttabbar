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
using System.Windows;
using DialogResult = System.Windows.Forms.DialogResult;
using SaveFileDialog = System.Windows.Forms.SaveFileDialog;

namespace QTTabBarLib {
    internal partial class Options05_General : OptionsDialogTab {
        public Options05_General() {
            InitializeComponent();
        }

        public override void InitializeConfig() {
            btnRecentFilesClear.DataContext = btnRecentTabsClear.DataContext = new RecentButtonBinding();
        }

        public override void ResetConfig() {
            DataContext = WorkingConfig.misc = new Config._Misc();
        }

        public override void CommitConfig() {
            // Not needed; everything is done through bindings
        }

        private void btnRecentFilesClear_Click(object sender, RoutedEventArgs e) {
            // TODO: sync
            var resp = MessageBox.Show(
                    QTUtility.TextResourcesDic["Options_Page05_General"][13],
                    QTUtility.TextResourcesDic["OptionsDialog"][3],
                    MessageBoxButton.OKCancel, MessageBoxImage.Question, MessageBoxResult.Cancel);
            if(resp == MessageBoxResult.Cancel) return;
            StaticReg.ExecutedPathsList.Clear();
            btnRecentFilesClear.GetBindingExpression(IsEnabledProperty).UpdateTarget();
        }

        private void btnRecentTabsClear_Click(object sender, RoutedEventArgs e) {
            // TODO: sync
            var resp = MessageBox.Show(
                    QTUtility.TextResourcesDic["Options_Page05_General"][14],
                    QTUtility.TextResourcesDic["OptionsDialog"][3],
                    MessageBoxButton.OKCancel, MessageBoxImage.Question, MessageBoxResult.Cancel);
            if(resp == MessageBoxResult.Cancel) return;
            StaticReg.ClosedTabHistoryList.Clear();
            btnRecentTabsClear.GetBindingExpression(IsEnabledProperty).UpdateTarget();
        }

        private void btnUpdateNow_Click(object sender, RoutedEventArgs e) {
            UpdateChecker.Check(true);
        }

        private void btnExport_Click(object sender, RoutedEventArgs e) {
            using(SaveFileDialog sf = new SaveFileDialog()) {
                // todo: localize?
                sf.Filter = "Registry file (*.reg)|*.reg";
                sf.RestoreDirectory = true;
                DateTime dt = DateTime.Now;
                sf.FileName = "QTTabBarSettings-" + dt.Year + "-" + dt.Month + "-" + dt.Day + ".reg";
                if(DialogResult.OK == sf.ShowDialog()) {
                    RegFileWriter.Export(RegConst.Root, sf.FileName);
                }
            }
        }

        #region ---------- Binding Classes ----------
        // INotifyPropertyChanged is implemented automatically by Notify Property Weaver!
        #pragma  warning disable 0067 // "The event 'PropertyChanged' is never used"
        // ReSharper disable MemberCanBePrivate.Local
        // ReSharper disable UnusedMember.Local
        // ReSharper disable UnusedAutoPropertyAccessor.Local

        private class RecentButtonBinding {
            public bool HaveRecentTabs { get { return StaticReg.ClosedTabHistoryList.Count != 0; } }
            public bool HaveRecentFiles { get { return StaticReg.ExecutedPathsList.Count != 0; } }
        }

        #endregion
    }
}
