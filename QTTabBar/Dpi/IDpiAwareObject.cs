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

namespace QTTabBarLib.Dpi
{
    /// <summary>
    /// DPI 感知对象接口。所有需要自动响应 DPI 变化的控件都实现此接口。
    /// 借鉴 2048 Beta2 的 IDpiAwareObject 设计。
    /// </summary>
    public interface IDpiAwareObject
    {
        /// <summary>当前 DPI 缩放因子（如 1.0, 1.25, 1.5, 2.0）</summary>
        float DpiScale { get; }

        /// <summary>DPI 变化时调用，参数为新旧缩放因子的比值</summary>
        void OnDpiChanged(float scaleFactor);

        /// <summary>是否自动缩放子控件</summary>
        bool ScaleChildren { get; set; }
    }
}
