//    This file is part of QTTabBar, a shell extension for Microsoft
//    Windows Explorer.
//    Copyright (C) 2007-2025  Quizo, Paul Accisano, indiff
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
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Resources;
using System.Threading;
using System.Windows.Forms;
using QTTabBarLib.Dpi;

namespace QTTabBarLib.FileRename
{
    /// <summary>
    /// 增强版文件重命名对话框。借鉴 2048 Beta2 的 FileRenameDialog 设计。
    /// 支持：查找替换（正则）、顺序编号、列表重命名、大小写转换、批量改扩展名、预览、预设。
    /// 支持多语言：中文（默认）、英文、日文。
    /// </summary>
    public partial class FileRenameDialog : DpiAwareForm
    {
        private readonly FileRenamerEngine _engine = new FileRenamerEngine();
        private readonly string _folderPath;
        private readonly IEnumerable<string> _selectedFiles;
        private List<RenameItem> _previewItems = new List<RenameItem>();

        // 多语言资源管理器（用 ComponentResourceManager，自动根据类型找资源，无需手动指定 baseName）
        private static readonly ComponentResourceManager _resManager =
            new ComponentResourceManager(typeof(FileRenameDialog));

        public FileRenameDialog(string folderPath, IEnumerable<string> selectedFiles = null)
        {
            _folderPath = folderPath;
            _selectedFiles = selectedFiles;
            InitializeComponent();
            ApplyLanguage();
            BindEvents();
            UpdatePreview();
        }

        /// <summary>
        /// 从资源文件应用多语言文本。
        /// 根据当前线程 CurrentUICulture 自动选择 zh-Cn / en / ja。
        /// </summary>
        private void ApplyLanguage()
        {
            // 窗体
            this.Text = T("FormTitle");

            // 模式选择
            lblMode.Text = T("lblMode");
            cboMode.Items.Clear();
            cboMode.Items.AddRange(new object[] {
                T("cboMode_Replace"),
                T("cboMode_Sequential"),
                T("cboMode_List")
            });
            cboMode.SelectedIndex = 0;
            btnPreset.Text = T("btnPreset");

            // 查找替换
            lblFind.Text = T("lblFind");
            lblReplace.Text = T("lblReplace");
            chkRegex.Text = T("chkRegex");
            chkCase.Text = T("chkCase");
            chkPreserveExt.Text = T("chkPreserveExt");

            // 大小写转换
            lblCase.Text = T("lblCase");
            cboCase.Items.Clear();
            cboCase.Items.AddRange(new object[] {
                T("cboCase_None"),
                T("cboCase_Lower"),
                T("cboCase_Upper"),
                T("cboCase_Title")
            });
            cboCase.SelectedIndex = 0;

            // 顺序编号
            lblSeq.Text = T("lblSeq");
            lblStart.Text = T("lblStart");
            lblInc.Text = T("lblInc");
            lblDigits.Text = T("lblDigits");

            // 子文件夹
            chkSubfolders.Text = T("chkSubfolders");
            lblDepth.Text = T("lblDepth");
            lblDepthHint.Text = T("lblDepthHint");

            // 冲突处理
            lblCollision.Text = T("lblCollision");
            cboCollision.Items.Clear();
            cboCollision.Items.AddRange(new object[] {
                T("cboCollision_Confirm"),
                T("cboCollision_Auto"),
                T("cboCollision_KeepNewer")
            });
            cboCollision.SelectedIndex = 1;

            // 范围
            lblScope.Text = T("lblScope");
            chkFiles.Text = T("chkFiles");
            chkFolders.Text = T("chkFolders");

            // 预览
            lblPreview.Text = T("lblPreview");
            colOriginal.Text = T("colOriginal");
            colNew.Text = T("colNew");
            colType.Text = T("colType");
            colStatus.Text = T("colStatus");

            // 底部
            lblStatus.Text = T("lblStatus_Ready");
            btnExecute.Text = T("btnExecute");
            btnClose.Text = T("btnClose");
        }

        /// <summary>
        /// 获取多语言字符串。
        /// 优先从 resx 资源读取，失败时从内联后备字典读取（中文默认值），
        /// 确保即使 resx 未正确嵌入也不会显示裸键名。
        /// </summary>
        private static string T(string key)
        {
            // 1. 优先从 ComponentResourceManager 读取（支持 en / ja 附属资源）
            try
            {
                _resManager.GetString(key);
                var culture = CultureInfo.CurrentCulture;
                /*switch (Config.Lang.BuiltInLangSelectedIndex)
                {
                    case 0: culture = CultureInfo.GetCultureInfo("en-US"); break;
                    case 1: culture = CultureInfo.GetCultureInfo("zh-CN"); break;
                    case 2: culture = CultureInfo.GetCultureInfo("de-DE"); break;
                    case 3: culture = CultureInfo.GetCultureInfo("pt-BR"); break;
                    case 4: culture = CultureInfo.GetCultureInfo("es-ES"); break;
                    case 5: culture = CultureInfo.GetCultureInfo("fr-FR"); break;
                    case 6: culture = CultureInfo.GetCultureInfo("tr-TR"); break;
                    case 7: culture = CultureInfo.GetCultureInfo("ru-RU"); break;
                }*/

                if (1 == Config.Lang.BuiltInLangSelectedIndex)
                {
                    culture = CultureInfo.GetCultureInfo("zh-CN");
                } else
                {
                    culture = CultureInfo.GetCultureInfo("en-US");
                }

                string val = _resManager.GetString(key,culture);
                if (!string.IsNullOrEmpty(val)) return val;
            }
            catch { /* 资源未嵌入或读取失败，走后备 */ }

            // 2. 内联后备字典（中文默认值，确保不会显示裸键名）
            if (_fallbackZhCN.TryGetValue(key, out string fallback))
                return fallback;

            return key;
        }

        /// <summary>
        /// 内联后备字典（中文）。当 resx 资源未正确嵌入时使用，确保 UI 可读。
        /// 新增字符串时需同时更新此字典和三个 resx 文件。
        /// </summary>
        private static readonly Dictionary<string, string> _fallbackZhCN = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            { "FormTitle", "批量重命名" },
            { "lblMode", "重命名模式:" },
            { "cboMode_Replace", "查找替换" },
            { "cboMode_Sequential", "顺序编号" },
            { "cboMode_List", "列表重命名" },
            { "btnPreset", "预设方案" },
            { "lblFind", "查找内容:" },
            { "lblReplace", "替换为:" },
            { "chkRegex", "使用正则表达式" },
            { "chkCase", "区分大小写" },
            { "chkPreserveExt", "保留文件扩展名" },
            { "lblCase", "大小写转换:" },
            { "cboCase_None", "不转换" },
            { "cboCase_Lower", "全部小写" },
            { "cboCase_Upper", "全部大写" },
            { "cboCase_Title", "首字母大写" },
            { "lblSeq", "顺序编号:" },
            { "lblStart", "起始" },
            { "lblInc", "步长" },
            { "lblDigits", "位数" },
            { "chkSubfolders", "包含子文件夹" },
            { "lblDepth", "扫描深度:" },
            { "lblDepthHint", "(0 = 无限制)" },
            { "lblCollision", "命名冲突:" },
            { "cboCollision_Confirm", "弹出确认" },
            { "cboCollision_Auto", "自动追加序号" },
            { "cboCollision_KeepNewer", "保留较新文件" },
            { "lblScope", "重命名范围:" },
            { "chkFiles", "文件" },
            { "chkFolders", "文件夹" },
            { "lblPreview", "  预览列表 （原名  →  新名称）" },
            { "colOriginal", "原名称" },
            { "colNew", "新名称" },
            { "colType", "类型" },
            { "colStatus", "状态" },
            { "type_File", "文件" },
            { "type_Folder", "文件夹" },
            { "status_Unchanged", "不变" },
            { "status_WillRename", "将重命名" },
            { "status_Success", "成功" },
            { "status_Failed", "失败" },
            { "lblStatus_Ready", "就绪" },
            { "lblStatus_Count", "共 {0} 项，{1} 项将重命名" },
            { "lblStatus_Result", "成功: {0}, 失败: {1}" },
            { "btnExecute", "执行重命名" },
            { "btnClose", "关闭" },
            { "msg_ConfirmTitle", "确认重命名" },
            { "msg_Confirm", "确定要重命名 {0} 个项目吗？" },
            { "msg_ErrorTitle", "错误" },
            { "msg_Error", "重命名失败: {0}" },
        };

        /// <summary>
        /// 绑定所有控件事件。
        /// </summary>
        private void BindEvents()
        {
            // 模式
            cboMode.SelectedIndexChanged += (s, e) => {
                _engine.Mode = (RenameMode)cboMode.SelectedIndex;
                UpdatePreview();
            };
            btnPreset.Click += BtnPreset_Click;

            // 查找替换
            txtFind.TextChanged += (s, e) => { _engine.FindText = txtFind.Text; UpdatePreview(); };
            txtReplace.TextChanged += (s, e) => { _engine.ReplaceText = txtReplace.Text; UpdatePreview(); };
            chkRegex.CheckedChanged += (s, e) => { _engine.UseRegex = chkRegex.Checked; UpdatePreview(); };
            chkCase.CheckedChanged += (s, e) => { _engine.CaseSensitive = chkCase.Checked; UpdatePreview(); };
            chkPreserveExt.CheckedChanged += (s, e) => { _engine.PreserveExtension = chkPreserveExt.Checked; UpdatePreview(); };

            // 大小写转换
            cboCase.SelectedIndexChanged += (s, e) => {
                _engine.CaseConversion = (CaseMode)cboCase.SelectedIndex;
                UpdatePreview();
            };

            // 顺序编号
            numStart.ValueChanged += (s, e) => { _engine.SeqStart = (int)numStart.Value; UpdatePreview(); };
            numInc.ValueChanged += (s, e) => { _engine.SeqIncrement = (int)numInc.Value; UpdatePreview(); };
            numDigits.ValueChanged += (s, e) => { _engine.SeqDigits = (int)numDigits.Value; UpdatePreview(); };

            // 子文件夹
            chkSubfolders.CheckedChanged += (s, e) => { _engine.IncludeSubfolders = chkSubfolders.Checked; UpdatePreview(); };
            numDepth.ValueChanged += (s, e) => { _engine.MaxDepth = (int)numDepth.Value; UpdatePreview(); };

            // 冲突处理
            cboCollision.SelectedIndexChanged += (s, e) => {
                _engine.Collision = (CollisionMode)cboCollision.SelectedIndex;
                UpdatePreview();
            };

            // 范围
            chkFiles.CheckedChanged += (s, e) => { _engine.RenameFiles = chkFiles.Checked; UpdatePreview(); };
            chkFolders.CheckedChanged += (s, e) => { _engine.RenameFolders = chkFolders.Checked; UpdatePreview(); };

            // 执行
            btnExecute.Click += (s, e) => ExecuteRename();
        }

        private void BtnPreset_Click(object sender, EventArgs e)
        {
            var menu = new ContextMenuStrip();
            foreach (var preset in FileRenamerEngine.GetPresets())
            {
                var item = new ToolStripMenuItem(preset.Key);
                item.Click += (s, e2) =>
                {
                    preset.Value(_engine);
                    // 预设应用后同步到 UI
                    SyncEngineToUI();
                    UpdatePreview();
                };
                menu.Items.Add(item);
            }
            menu.Show((Control)sender, new Point(0, ((Control)sender).Height));
        }

        /// <summary>
        /// 预设应用后，把引擎状态同步到 UI 控件。
        /// </summary>
        private void SyncEngineToUI()
        {
            cboMode.SelectedIndex = (int)_engine.Mode;
            txtFind.Text = _engine.FindText;
            txtReplace.Text = _engine.ReplaceText;
            chkRegex.Checked = _engine.UseRegex;
            chkCase.Checked = _engine.CaseSensitive;
            chkPreserveExt.Checked = _engine.PreserveExtension;
            cboCase.SelectedIndex = (int)_engine.CaseConversion;
            numStart.Value = Math.Min(numStart.Maximum, Math.Max(numStart.Minimum, _engine.SeqStart));
            numInc.Value = Math.Min(numInc.Maximum, Math.Max(numInc.Minimum, _engine.SeqIncrement));
            numDigits.Value = Math.Min(numDigits.Maximum, Math.Max(numDigits.Minimum, _engine.SeqDigits));
            chkSubfolders.Checked = _engine.IncludeSubfolders;
            numDepth.Value = Math.Min(numDepth.Maximum, Math.Max(numDepth.Minimum, _engine.MaxDepth));
            cboCollision.SelectedIndex = (int)_engine.Collision;
            chkFiles.Checked = _engine.RenameFiles;
            chkFolders.Checked = _engine.RenameFolders;
        }

        private void UpdatePreview()
        {
            try
            {
                _previewItems = _engine.Preview(_folderPath, _selectedFiles);
                lvPreview.Items.Clear();
                foreach (var item in _previewItems.Take(500))
                {
                    var lvi = new ListViewItem(item.OriginalName);
                    lvi.SubItems.Add(item.NewName);
                    lvi.SubItems.Add(item.IsFolder ? T("type_Folder") : T("type_File"));
                    lvi.SubItems.Add(string.Equals(item.OriginalName, item.NewName, StringComparison.Ordinal)
                        ? T("status_Unchanged")
                        : T("status_WillRename"));
                    lvPreview.Items.Add(lvi);
                }
                int changed = _previewItems.Count(i => !string.Equals(i.OriginalName, i.NewName, StringComparison.Ordinal));
                lblStatus.Text = string.Format(T("lblStatus_Count"), _previewItems.Count, changed);
            }
            catch (Exception ex)
            {
                QTUtility2.MakeErrorLog(ex, "FileRenameDialog.UpdatePreview");
            }
        }

        private void ExecuteRename()
        {
            int changed = _previewItems.Count(i => !string.Equals(i.OriginalName, i.NewName, StringComparison.Ordinal));
            var result = MessageBox.Show(
                string.Format(T("msg_Confirm"), changed),
                T("msg_ConfirmTitle"),
                MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result != DialogResult.Yes) return;

            try
            {
                var (success, failed, items) = _engine.Execute(_folderPath, _selectedFiles);
                lblStatus.Text = string.Format(T("lblStatus_Result"), success, failed);
                lvPreview.Items.Clear();
                foreach (var item in items)
                {
                    var lvi = new ListViewItem(item.OriginalName);
                    lvi.SubItems.Add(item.NewName);
                    lvi.SubItems.Add(item.IsFolder ? T("type_Folder") : T("type_File"));
                    lvi.SubItems.Add(item.Success ? T("status_Success") : (T("status_Failed") + ": " + item.Error));
                    if (!item.Success) lvi.ForeColor = Color.Red;
                    lvPreview.Items.Add(lvi);
                }
            }
            catch (Exception ex)
            {
                QTUtility2.MakeErrorLog(ex, "FileRenameDialog.ExecuteRename");
                MessageBox.Show(string.Format(T("msg_Error"), ex.Message),
                    T("msg_ErrorTitle"), MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void lblDepthHint_Click(object sender, EventArgs e)
        {

        }

        private void numDepth_ValueChanged(object sender, EventArgs e)
        {

        }
    }
}
