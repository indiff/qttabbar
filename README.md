# Introduction
1. Qttabbar domestic optimized version is based on sf.net/projects/qttabbar/ (2012-06-17). The original author of this version has not released it. I don't know why. Adding some Chinese features is mainly for the convenience of domestic users; in addition, the capture window of quizo official website maintained by Japanese authors is not used to all the time, so this version retains the easy-to-use function of capture window.
2. Qttabbar is a small tool that allows you to use tab multi label function in Windows Explorer. Since then, there are no windows folder and awesome folder preview functions, which greatly improves the efficiency of your work. It's like IE 7, and it's like Firefox and opera. Qttabbar also provides some plug-ins, such as file operation tool, tree directory, display status bar and so on.
- [GitHub](https://indiff.github.io/qttabbar)
- [Gitee](https://gitee.com/qwop/qttabbar)

# Changes
- [1.5.4(2021)](https://github.com/indiff/qttabbar/releases/tag/1.5.4-beta) All plug-ins are built-in by default, which support one click to enable and one click to disable; fix the lock function bug
- [1.5.3(2020)](https://github.com/indiff/qttabbar/releases/tag/1.5.3-beta) Add custom button image, support button skin function; add new function, support new tab, take the path of the clipboard by default; add video preview image function
- [1.5.2(2020)](https://github.com/indiff/qttabbar/releases/tag/1.5.2) Repair open command prompt exception; Exception log add extended information prompt; add language configuration file
- [1.4(2020)](https://github.com/indiff/qttabbar/releases/tag/1.4) Adjust version information, fix hotkey conflict, create empty file, and load two plug-ins automatically by default
- [1.3](https://github.com/indiff/qttabbar/releases/tag/1.3) Update plug-ins to achieve de duplication, sorting function
- [1.2](https://github.com/indiff/qttabbar/releases/tag/1.2) Support win10, fix link failure
- [1.1](https://github.com/indiff/qttabbar/releases/tag/1.1) The installation interface supports Sinicization
- [1.0](https://github.com/indiff/qttabbar/releases/tag/1.0) Built in Chinese Language Options

# Download
* [qttabbar Latest version](https://github.com/indiff/qttabbar/releases/tag/1.5.4-beta)
* [qttabbar Chinese mirror](https://gitee.com/qwop/qttabbar/attach_files)
* Please install net framework 3.5 [Download](https://www.microsoft.com/zh-CN/download/details.aspx?id=21)
* ![请安装 net framework3.5 ](https://user-images.githubusercontent.com/501276/84343198-16aedc00-abda-11ea-8872-a654d011631f.png)

# Usage
- Run the installation package file to install qttabbar 
- Explorer > View > Options - > (qttabbar & buttons)【win10】
- In Explorer, organization > layout > menu bar  【win10以下】
- 右键菜单栏右方的空白地区—>勾选QTTabBar等工具栏—>按Alt+M—>重启explorer或重启计算机
![启用qttabbar](https://user-images.githubusercontent.com/501276/72576075-907fb980-3909-11ea-9dc2-9a1ea0ca2f8e.png)


# 编译/Build
* 安装 [wix311.exe](https://github.com/wixtoolset/wix3/releases)
* 安装 wix visual [插件](https://marketplace.visualstudio.com/items?itemName=WixToolset.WixToolsetVisualStudio2010Extension)  
* 安装 visual studio 2010  [百度网盘](https://pan.baidu.com/s/1sldAQmD#list/path=%2FVS%E4%BE%BF%E6%90%BA%E7%B2%BE%E7%AE%80%E7%89%88%E5%90%88%E9%9B%86)
* 拉取代码安装插件 NotifyPropertyWeaverVsPackage.vsix
* Qttabbar项目 添加引用 Ricciolo.Controls.TreeListView, 路径 QTTabBar\Resources\Ricciolo.Controls.TreeListView.dll

# 常见问题/Problems
* [插件使用教程](https://gitee.com/qwop/qttabbar/attach_files/581155/download)
* [打开选项闪退的解决方案修订版](https://gitee.com/qwop/qttabbar/attach_files/581136/download)
* [新建文件夹刷新才能看到解决方法](https://gitee.com/qwop/qttabbar/attach_files/581159/download)

# 多图预览/Preview
* [预览图片/Soft Preview](https://github.com/indiff/qttabbar/issues/3)

# 语言配置文件/Languages
* [英文配置文件/English Language File](https://raw.githubusercontent.com/indiff/qttabbar/master/Lng_QTTabBar_en.xml)
* [中文配置文件/Chinese Language File](https://raw.githubusercontent.com/indiff/qttabbar/master/Lng_QTTabBar_zh.xml)

# QQ交流群/QQ Group
* 群号: [157604022](https://qm.qq.com/cgi-bin/qm/qr?k=AGA5sh_6eCEYIwofpvazRxMFin8jmVI2&jump_from=webapi)

# 鸣谢/Thanks
* [捐助列表](https://github.com/indiff/qttabbar/issues/27)
* [Donation](https://www.paypal.com/cgi-bin/webscr?cmd=_s-xclick&hosted_button_id=7YNCVL5P9ZDY8)

# 待办/TODOS
* 迁移VS2019
* 添加书签功能
* 增强预览功能
