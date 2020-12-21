# 软件介绍/Introduction
QTTabBar国内优化版是基于 https://sourceforge.net/projects/qttabbar/ 2012-06-17 提交的最新代码改版的。这个版本原作者没有发布过，具体不知道什么原因。增添一些汉化特性，主要是为了方便国内用户使用；另外日本作者维护的Quizo官网版本的捕获窗口一直用着不习惯，所以该版本保留了捕获窗口这个好用的功能。
QTTabBar是一款可以让你在Windows资源管理器中使用Tab多标签功能的小工具。从此以后工作时不再遍布文件夹窗口，还有给力的文件夹预览功能，大大提高了你工作的效率。就像IE 7和Firefox、Opera那样的。QTTabBar还提供了一些功能插件，如：文件操作工具、树型目录、显示状态栏等等。
- [GitHub主页](https://indiff.github.io/qttabbar)
- [Gitee主页](https://gitee.com/qwop/qttabbar)

# 版本更新/Changes
- [1.5.3(2020)](https://github.com/indiff/qttabbar/releases/tag/1.5.3-beta) 增加自定义按钮图片，支持按钮换肤功能；添加新功能，支持新开标签页，默认取剪贴板的路径；增加支持视频预览图片功能
- [1.5.2(2020)](https://github.com/indiff/qttabbar/releases/tag/1.5.2) 修复打开命令提示符异常;异常日志添加扩展信息提示;添加语言配置文件
- [1.4(2020)](https://github.com/indiff/qttabbar/releases/tag/1.4) 调整版本信息，修复热键冲突问题；实现创建空文件；默认自动加载两个插件
- [1.3](https://github.com/indiff/qttabbar/releases/tag/1.3) 更新插件实现去重，排序功能
- [1.2](https://github.com/indiff/qttabbar/releases/tag/1.2) 支持win10，修复链接失效问题
- [1.1](https://github.com/indiff/qttabbar/releases/tag/1.1) 安装界面支持汉化
- [1.0](https://github.com/indiff/qttabbar/releases/tag/1.0) 内置汉化语言选项

# 下载地址/Download
* [qttabbar最新版本](https://github.com/indiff/qttabbar/releases/tag/1.5.3-beta)
* [qttabbar国内镜像](https://gitee.com/qwop/qttabbar/attach_files)
* 请安装 net framework 3.5/Please install net framework 3.5 [下载/download](https://www.microsoft.com/zh-CN/download/details.aspx?id=21)
* ![请安装 net framework3.5 ](https://user-images.githubusercontent.com/501276/84343198-16aedc00-abda-11ea-8872-a654d011631f.png)

# 使用方法/Usage
- 运行安装包文件，安装QTTabBar 
- 资源管理器->查看->选项->(QTTabbar & Buttons )      【win10】
- 资源管理器中，组织—>布局—>菜单栏  【win10以下】
- 右键菜单栏右方的空白地区—>勾选QTTabBar等工具栏—>按Alt+M—>重启explorer或重启计算机
![启用qttabbar](https://user-images.githubusercontent.com/501276/72576075-907fb980-3909-11ea-9dc2-9a1ea0ca2f8e.png)


# 编译/Build
* 安装 [wix311.exe](https://github.com/wixtoolset/wix3/releases)
* 安装 wix visual [插件](https://marketplace.visualstudio.com/items?itemName=WixToolset.WixToolsetVisualStudio2010Extension)  
* 安装 visual studio 2010  [百度网盘](https://pan.baidu.com/s/1sldAQmD#list/path=%2FVS%E4%BE%BF%E6%90%BA%E7%B2%BE%E7%AE%80%E7%89%88%E5%90%88%E9%9B%86)
* 拉取代码安装插件 NotifyPropertyWeaverVsPackage.vsix
* Qttabbar项目 添加引用 Ricciolo.Controls.TreeListView, 路径 QTTabBar\Resources\Ricciolo.Controls.TreeListView.dll

# 多图预览/Preview
* [预览图片/Soft Preview](https://github.com/indiff/qttabbar/issues/3)

# 语言配置文件/Languages
* [英文配置文件/English Language File](https://raw.githubusercontent.com/indiff/qttabbar/master/Lng_QTTabBar_en.xml)
* [中文配置文件/Chinese Language File](https://raw.githubusercontent.com/indiff/qttabbar/master/Lng_QTTabBar_zh.xml)

# QQ交流群/QQ Group
* 群号: [157604022](https://qm.qq.com/cgi-bin/qm/qr?k=cIc9Svpa17jTpyA_rQ0SsG4gw4pG3Mw6&jump_from=webapi)

# 鸣谢/Thanks
* [捐助列表](https://github.com/indiff/qttabbar/issues/27)
* [Donation](https://www.paypal.com/cgi-bin/webscr?cmd=_s-xclick&hosted_button_id=7YNCVL5P9ZDY8)
