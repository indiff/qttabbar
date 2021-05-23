> [汉化GitHub modify by indiff](https://openuserjs.org/scripts/indiff/GitHub_%E6%B1%89%E5%8C%96%E6%8F%92%E4%BB%B6_(indiff)%E4%BF%AE%E6%94%B9)
# Introduction [中文版本](https://github.com/indiff/qttabbar/blob/master/README_zh.md)
1. Qttabbar domestic optimized version is based on sf.net/projects/qttabbar/ (2012-06-17). The original author of this version has not released it. I don't know why. Adding some Chinese features is mainly for the convenience of domestic users; in addition, the capture window of quizo official website maintained by Japanese authors is not used to all the time, so this version retains the easy-to-use function of capture window.
2. Qttabbar is a small tool that allows you to use tab multi label function in Windows Explorer. Since then, there are no windows folder and awesome folder preview functions, which greatly improves the efficiency of your work. It's like IE 7, and it's like Firefox and opera. Qttabbar also provides some plug-ins, such as file operation tool, tree directory, display status bar and so on.
- [GitHub](https://indiff.github.io/qttabbar)
- [Gitee](https://gitee.com/qwop/qttabbar)

# Changes
- [1.5.5(2021)](https://github.com/indiff/qttabbar/releases/tag/v1.5.5.2021-beta) Fix the known problem. Fix the Explorer crash after clicking the option; Add plug-in mouse suspension activation label
- [1.5.4(2021)](https://github.com/indiff/qttabbar/releases/tag/1.5.4-beta) All plug-ins are built-in by default, which support one click to enable and one click to disable; fix the lock function bug
- [1.5.3(2020)](https://github.com/indiff/qttabbar/releases/tag/1.5.3-beta) Add custom button image, support button skin function; add new function, support new tab, take the path of the clipboard by default; add video preview image function
- [1.5.2(2020)](https://github.com/indiff/qttabbar/releases/tag/1.5.2) Repair open command prompt exception; Exception log add extended information prompt; add language configuration file
- [1.4(2020)](https://github.com/indiff/qttabbar/releases/tag/1.4) Adjust version information, fix hotkey conflict, create empty file, and load two plug-ins automatically by default
- [1.3](https://github.com/indiff/qttabbar/releases/tag/1.3) Update plug-ins to achieve de duplication, sorting function
- [1.2](https://github.com/indiff/qttabbar/releases/tag/1.2) Support win10, fix link failure
- [1.1](https://github.com/indiff/qttabbar/releases/tag/1.1) The installation interface supports Sinicization
- [1.0](https://github.com/indiff/qttabbar/releases/tag/1.0) Built in Chinese Language Options

# Download
* [qttabbar Latest version](https://github.com/indiff/qttabbar/releases/tag/v1.5.5.2021-beta)
* [qttabbar Chinese mirror](https://gitee.com/qwop/qttabbar/attach_files)
* Please install net framework 3.5 [Download](https://www.microsoft.com/zh-CN/download/details.aspx?id=21)
* ![Install net framework3.5 ](https://user-images.githubusercontent.com/501276/84343198-16aedc00-abda-11ea-8872-a654d011631f.png)

# Usage
- Run the installation package file to install qttabbar 
- Explorer > View > Options - > (qttabbar & buttons)[win10]
- In Explorer, organization > layout > menu bar  [xp,win7]
- Right click the blank area on the right side of the menu bar - > check qttabbar and other toolbars - > Press Alt + m - > Restart Explorer or restart the computer
![Enable qttabbar](https://user-images.githubusercontent.com/501276/72576075-907fb980-3909-11ea-9dc2-9a1ea0ca2f8e.png)


# Build
* Install [wix311.exe](https://github.com/wixtoolset/wix3/releases)
* Install wix visual [Plugin](https://marketplace.visualstudio.com/items?itemName=WixToolset.WixToolsetVisualStudio2010Extension)  
* Install visual studio 2010  [Baidu Pan](https://pan.baidu.com/s/1sldAQmD#list/path=%2FVS%E4%BE%BF%E6%90%BA%E7%B2%BE%E7%AE%80%E7%89%88%E5%90%88%E9%9B%86)
* Git pull NotifyPropertyWeaverVsPackage.vsix
* Qttabbar Project Add Referrence Ricciolo.Controls.TreeListView, Path QTTabBar\Resources\Ricciolo.Controls.TreeListView.dll

# Problems
* [Tutorial](https://gitee.com/qwop/qttabbar/attach_files/581155/download)
* [Open option flashback solution revision](https://gitee.com/qwop/qttabbar/attach_files/581136/download)
* [New folder refresh to see the solution](https://gitee.com/qwop/qttabbar/attach_files/581159/download)

# Preview
* [Soft Preview](https://github.com/indiff/qttabbar/issues/3)

# Languages
* [English Language File](https://raw.githubusercontent.com/indiff/qttabbar/master/Lng_QTTabBar_en.xml)
* [Chinese Language File](https://raw.githubusercontent.com/indiff/qttabbar/master/Lng_QTTabBar_zh.xml)

# QQ Group
* Group: [157604022](https://qm.qq.com/cgi-bin/qm/qr?k=AGA5sh_6eCEYIwofpvazRxMFin8jmVI2&jump_from=webapi)

# Thanks
* [Original author Quizo](https://twitter.com/QTTabBar) 
* [SF Version Author](https://sourceforge.net/u/masamunexgp/profile)
* [Donation List](https://github.com/indiff/qttabbar/wiki/Thanks-%E9%B8%A3%E8%B0%A2%E6%8D%90%E5%8A%A9)
* [Donation](https://www.paypal.com/cgi-bin/webscr?cmd=_s-xclick&hosted_button_id=7YNCVL5P9ZDY8)
* Thanks [JetBrains](https://www.jetbrains.com/?from=QtTabBar), Support for open source projects
![jetbrains](https://images.gitee.com/uploads/images/2020/0714/114152_d335c2f1_416720.png "jetbrains.png")
# TODOS
* Migration VS2019
* Add boommark
* Enhanced Preview
