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
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace QTTabBarLib.FileRename
{
    /// <summary>
    /// 重命名模式
    /// </summary>
    public enum RenameMode
    {
        /// <summary>查找替换</summary>
        Replace,
        /// <summary>列表重命名</summary>
        List,
        /// <summary>顺序编号</summary>
        Sequential
    }

    /// <summary>
    /// 冲突处理方式
    /// </summary>
    public enum CollisionMode
    {
        /// <summary>确认</summary>
        Confirm,
        /// <summary>自动（添加序号）</summary>
        Automatic,
        /// <summary>保留较新的文件</summary>
        KeepNewer
    }

    /// <summary>
    /// 大小写转换模式
    /// </summary>
    public enum CaseMode
    {
        None,
        Lowercase,
        Uppercase,
        TitleCase
    }

    /// <summary>
    /// 重命名项
    /// </summary>
    public class RenameItem
    {
        public string OriginalPath { get; set; }
        public string OriginalName { get; set; }
        public string NewName { get; set; }
        public string NewPath { get; set; }
        public bool Success { get; set; }
        public string Error { get; set; }
        public bool IsFolder { get; set; }
    }

    /// <summary>
    /// 文件重命名引擎。借鉴 2048 Beta2 的 FileRenamerHelper 设计。
    /// 支持：查找替换（正则）、顺序编号、大小写转换、批量改扩展名、子文件夹递归。
    /// </summary>
    public class FileRenamerEngine
    {
        #region 属性

        public RenameMode Mode { get; set; } = RenameMode.Replace;
        public string FindText { get; set; } = "";
        public string ReplaceText { get; set; } = "";
        public bool UseRegex { get; set; }
        public bool CaseSensitive { get; set; }
        public bool IncludeSubfolders { get; set; }
        public int MaxDepth { get; set; } = 0; // 0 = 无限制
        public CollisionMode Collision { get; set; } = CollisionMode.Confirm;
        public CaseMode CaseConversion { get; set; } = CaseMode.None;
        public bool PreserveExtension { get; set; } = true;

        // 顺序编号
        public int SeqStart { get; set; } = 1;
        public int SeqIncrement { get; set; } = 1;
        public int SeqDigits { get; set; } = 2;
        public bool SeqResetInSubfolder { get; set; }
        public string SeqPrefix { get; set; } = "";
        public string SeqSuffix { get; set; } = "";

        // 列表模式
        public List<string> NameList { get; set; } = new List<string>();

        // 目标范围
        public bool RenameFolders { get; set; } = true;
        public bool RenameFiles { get; set; } = true;

        #endregion

        #region 核心方法

        /// <summary>预览重命名结果（不实际执行）</summary>
        public List<RenameItem> Preview(string folderPath, IEnumerable<string> selectedFiles = null)
        {
            var items = new List<RenameItem>();
            var sources = GetSourceFiles(folderPath, selectedFiles);
            int seqCounter = SeqStart;
            string lastFolder = null;

            foreach (var src in sources)
            {
                if (SeqResetInSubfolder && !string.Equals(lastFolder, Path.GetDirectoryName(src)))
                {
                    seqCounter = SeqStart;
                    lastFolder = Path.GetDirectoryName(src);
                }

                string dir = Path.GetDirectoryName(src);
                string name = Path.GetFileName(src);
                string ext = Path.GetExtension(name);
                string nameWithoutExt = Path.GetFileNameWithoutExtension(name);
                bool isFolder = Directory.Exists(src);

                if (isFolder && !RenameFolders) continue;
                if (!isFolder && !RenameFiles) continue;

                string newName = name;

                switch (Mode)
                {
                    case RenameMode.Replace:
                        newName = DoReplace(nameWithoutExt, ext);
                        break;
                    case RenameMode.Sequential:
                        newName = DoSequential(nameWithoutExt, ext, seqCounter);
                        seqCounter += SeqIncrement;
                        break;
                    case RenameMode.List:
                        int idx = items.Count;
                        if (idx < NameList.Count && !string.IsNullOrEmpty(NameList[idx]))
                        {
                            newName = PreserveExtension ? NameList[idx] + ext : NameList[idx];
                        }
                        break;
                }

                // 大小写转换
                newName = ApplyCaseConversion(newName);

                string newPath = Path.Combine(dir, newName);

                items.Add(new RenameItem
                {
                    OriginalPath = src,
                    OriginalName = name,
                    NewName = newName,
                    NewPath = newPath,
                    IsFolder = isFolder
                });
            }

            return items;
        }

        /// <summary>执行重命名</summary>
        public (int success, int failed, List<RenameItem> results) Execute(string folderPath, IEnumerable<string> selectedFiles = null)
        {
            var items = Preview(folderPath, selectedFiles);
            int success = 0, failed = 0;

            foreach (var item in items)
            {
                if (string.Equals(item.OriginalPath, item.NewPath, StringComparison.OrdinalIgnoreCase))
                {
                    item.Success = true;
                    success++;
                    continue;
                }

                try
                {
                    // 冲突处理
                    if (File.Exists(item.NewPath) || Directory.Exists(item.NewPath))
                    {
                        if (Collision == CollisionMode.Automatic)
                        {
                            item.NewPath = GetUniquePath(item.NewPath);
                            item.NewName = Path.GetFileName(item.NewPath);
                        }
                        else if (Collision == CollisionMode.KeepNewer)
                        {
                            if (File.Exists(item.NewPath) && File.Exists(item.OriginalPath))
                            {
                                var existing = new FileInfo(item.NewPath);
                                var current = new FileInfo(item.OriginalPath);
                                if (existing.LastWriteTime > current.LastWriteTime)
                                {
                                    item.Success = false;
                                    item.Error = "保留较新文件";
                                    failed++;
                                    continue;
                                }
                            }
                        }
                    }

                    if (item.IsFolder)
                        Directory.Move(item.OriginalPath, item.NewPath);
                    else
                        File.Move(item.OriginalPath, item.NewPath);

                    item.Success = true;
                    success++;
                }
                catch (Exception ex)
                {
                    item.Success = false;
                    item.Error = ex.Message;
                    failed++;
                }
            }

            return (success, failed, items);
        }

        #endregion

        #region 私有方法

        private List<string> GetSourceFiles(string folderPath, IEnumerable<string> selectedFiles)
        {
            var result = new List<string>();

            if (selectedFiles != null)
            {
                result.AddRange(selectedFiles.Where(f => File.Exists(f) || Directory.Exists(f)));
            }
            else if (!string.IsNullOrEmpty(folderPath) && Directory.Exists(folderPath))
            {
                var searchOption = IncludeSubfolders ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;

                if (RenameFiles)
                    result.AddRange(Directory.GetFiles(folderPath, "*", searchOption));
                if (RenameFolders)
                    result.AddRange(Directory.GetDirectories(folderPath, "*", searchOption));

                // 深度限制
                if (MaxDepth > 0 && IncludeSubfolders)
                {
                    result = result.Where(p =>
                    {
                        int depth = p.Count(c => c == Path.DirectorySeparatorChar) -
                                    folderPath.Count(c => c == Path.DirectorySeparatorChar);
                        return depth <= MaxDepth;
                    }).ToList();
                }
            }

            return result;
        }

        private string DoReplace(string nameWithoutExt, string ext)
        {
            string result = nameWithoutExt;

            if (!string.IsNullOrEmpty(FindText))
            {
                if (UseRegex)
                {
                    try
                    {
                        var options = CaseSensitive ? RegexOptions.None : RegexOptions.IgnoreCase;
                        result = Regex.Replace(result, FindText, ReplaceText ?? "", options);
                    }
                    catch { /* 正则无效，不替换 */ }
                }
                else
                {
                    //var comparison = CaseSensitive ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase;
                    //result = result.Replace(FindText, ReplaceText ?? "", comparison);
                    if (CaseSensitive)
                    {
                        result = result.Replace(FindText, ReplaceText ?? "");
                    }
                    else
                    {
                        // .NET Framework 不支持 Replace(string, string, StringComparison)
                        // 手动实现大小写不敏感替换
                        result = ReplaceIgnoreCase(result, FindText, ReplaceText ?? "");
                    }
                }
            }

            return PreserveExtension ? result + ext : result;
        }

        /// <summary>
        /// 大小写不敏感的字符串替换（兼容 .NET Framework）
        /// </summary>
        private static string ReplaceIgnoreCase(string source, string oldValue, string newValue)
        {
            if (string.IsNullOrEmpty(source) || string.IsNullOrEmpty(oldValue))
                return source;

            var sb = new System.Text.StringBuilder();
            int index = 0;
            int prevIndex = 0;

            while ((index = source.IndexOf(oldValue, prevIndex, StringComparison.OrdinalIgnoreCase)) >= 0)
            {
                sb.Append(source, prevIndex, index - prevIndex);
                sb.Append(newValue);
                prevIndex = index + oldValue.Length;
            }

            sb.Append(source, prevIndex, source.Length - prevIndex);
            return sb.ToString();
        }

        private string DoSequential(string nameWithoutExt, string ext, int counter)
        {
            string num = counter.ToString(new string('0', Math.Max(1, SeqDigits)));
            string result = SeqPrefix + num + SeqSuffix;
            return PreserveExtension ? result + ext : result;
        }

        private string ApplyCaseConversion(string name)
        {
            switch (CaseConversion)
            {
                case CaseMode.Lowercase:
                    return name.ToLowerInvariant();
                case CaseMode.Uppercase:
                    return name.ToUpperInvariant();
                case CaseMode.TitleCase:
                    return System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(name.ToLowerInvariant());
                default:
                    return name;
            }
        }

        private static string GetUniquePath(string path)
        {
            string dir = Path.GetDirectoryName(path);
            string name = Path.GetFileNameWithoutExtension(path);
            string ext = Path.GetExtension(path);
            int counter = 1;
            string newPath = path;

            while (File.Exists(newPath) || Directory.Exists(newPath))
            {
                newPath = Path.Combine(dir, $"{name} ({counter}){ext}");
                counter++;
            }

            return newPath;
        }

        #endregion

        #region 预设

        /// <summary>常用重命名预设</summary>
        public static Dictionary<string, Action<FileRenamerEngine>> GetPresets()
        {
            return new Dictionary<string, Action<FileRenamerEngine>>
            {
                ["批量改扩展名 abc -> xyz"] = e =>
                {
                    e.Mode = RenameMode.Replace;
                    e.UseRegex = true;
                    e.FindText = @"\.[^.]+$";
                    e.ReplaceText = ".xyz";
                    e.PreserveExtension = false;
                },
                ["全部改扩展名 ALL -> xyz"] = e =>
                {
                    e.Mode = RenameMode.Replace;
                    e.PreserveExtension = false;
                    e.FindText = "";
                },
                ["转小写"] = e => { e.CaseConversion = CaseMode.Lowercase; },
                ["转大写"] = e => { e.CaseConversion = CaseMode.Uppercase; },
                ["首字母大写"] = e => { e.CaseConversion = CaseMode.TitleCase; },
                ["顺序编号 001,002..."] = e =>
                {
                    e.Mode = RenameMode.Sequential;
                    e.SeqStart = 1;
                    e.SeqIncrement = 1;
                    e.SeqDigits = 3;
                },
            };
        }

        #endregion
    }
}
