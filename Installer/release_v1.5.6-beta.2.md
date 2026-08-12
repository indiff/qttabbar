v1.5.6-beta.1修复文件捕获选中/Fix Capture to select

> 关于闪退或崩溃问题：关闭捕获选中文件功能查看如下截图

命令方式打开并选中文件
explorer /n, /e, /select, "C:\Demo\Demo.java"

>1. 修复默认忽略掉无法打开的路径
>2. 修复最小宽度的上限调整到100
>3. 修复忽略控制面板、网络连接、打印机等路径
>4. 修复资源被占用问题
>5. 修复关闭窗口不保存忽略路径
>6. 修复微信或者qq打开文件未选中
>7. 调整一些默认的配置

[详细查看](https://www.yuque.com/indiff/qttabbar/rhyprn)

## 调整安装时间和激活时间记录注册表的写入逻辑（build07）
![](https://cdn.nlark.com/yuque/0/2024/png/1617173/1724136118889-26464f25-34bf-4a68-8ff9-e647d14c1732.png)

![](https://cdn.nlark.com/yuque/0/2024/png/1617173/1724136128355-cb26746e-6c4a-496b-8e9f-895eba1ef82e.png)

**<font style="color:#74B602;">计算机\HKEY_LOCAL_MACHINE\SOFTWARE\QTTabBar\</font>****<font style="color:#DF2A3F;">InstallDate</font>****<font style="color:#74B602;"></font>**

**<font style="color:#74B602;">计算机\HKEY_CURRENT_USER\Software\QTTabBar\</font>****<font style="color:#DF2A3F;">ActivationDate</font>****<font style="color:#74B602;"></font>**

## 修复异常问题:  SetUsingListView InvalidComObjectException（build06）
## 调整日志文件,调试日志和异常日志（build06）
分为两个文件

C:\Users\Administrator\AppData\Roaming\QTTabBar\QTTabBar.log

C:\Users\Administrator\AppData\Roaming\QTTabBar\QTTabBarException.log

## 调整默认文本后缀名列表,支持文本预览（build05）
![](https://cdn.nlark.com/yuque/0/2024/png/1617173/1722224418214-f29a1b5a-1278-4182-b9e7-352b6a4ac2ac.png)

## 新增 ALT + 鼠标双击, 创建新文本文件（build05）
![](https://cdn.nlark.com/yuque/0/2024/gif/1617173/1722224310953-5bf76b2b-149b-414a-96de-2f5b807295c4.gif)

## 调整拖拽文字提示（build04）
**<font style="color:#DF2A3F;">按住 ctrl 是复制文件 ， 直接拖拽是 移动文件</font>**

![](https://cdn.nlark.com/yuque/0/2024/png/1617173/1721811671270-4508b7d3-8f83-4fb0-95a7-822c8aafa5d0.png)

## 修复按钮消失问题
## 修复一些异常提示问题
## <font style="color:#DF2A3F;">关于闪退或崩溃问题：关闭捕获选中文件功能</font>
+ **<font style="color:#74B602;">去掉勾选是关闭</font>**
+ **<font style="color:#74B602;">勾选中是打开</font>**

![](https://cdn.nlark.com/yuque/0/2023/png/1617173/1679703532591-1b087e55-a223-4ee3-a936-eb9b9c46b801.png)

## 
## 默认忽略掉无法打开的路径
<font style="color:#74B602;">有时候有一些路径无法打开，会弹出这个提示。默认是这个路径不存在或者失效就提示，暂忽略掉不提示</font>

![](https://cdn.nlark.com/yuque/0/2023/png/1617173/1684368715047-8418e499-f0b6-4cbb-97bc-8d3908857df0.png)

## Intellij Idea 打开并定位文件
<font style="color:#74B602;">由于已知软件捕获进程的方式，导致外包打开并选中文件无法正常工作；这里给出一个兼容qttabbar版本的实现方式如下。</font>**<font style="color:#DF2A3F;">参考命令行的实现方式</font>**

![](https://cdn.nlark.com/yuque/0/2023/png/1617173/1684286611292-6f488adc-11fb-4927-988e-c4bf67da9eb5.png)![](https://cdn.nlark.com/yuque/0/2023/png/1617173/1684287371153-951cdd23-7245-4e6b-91de-9ad139f428fc.png)

## 命令方式打开并选中文件
```bash
explorer /n, /e, /select, "C:\Demo\Demo.java"

explorer /n, /e, /select, "C:\Windows\System32\notepad.exe"
```

## 打开相应目录，修复自动选择文件问题
**<font style="color:#DF2A3F;">这里虽然功能实现了，但是第三方软件兼容还要观察。目前测试兼容微信、qq、钉钉打开所在文件.</font>**

**<font style="color:#DF2A3F;">需要勾选 Capture Selection</font>**

**<font style="color:#DF2A3F;">并不是一种好的实现方式，所以有可能导致崩溃。所以这个选项酌情考虑</font>**

![](https://cdn.nlark.com/yuque/0/2023/png/1617173/1684287127361-53ab16d0-996c-46e9-b9fc-15f2ec4268dd.png)

![](https://cdn.nlark.com/yuque/0/2023/png/1617173/1679703532591-1b087e55-a223-4ee3-a936-eb9b9c46b801.png?x-oss-process=image%2Fformat%2Cwebp)

## 最小宽度的上限调整到100
**<font style="color:#DF2A3F;">这里在上个版本是</font>**

![](https://cdn.nlark.com/yuque/0/2023/png/1617173/1679040896704-0c43b65a-adf1-4a5d-90d9-2731c877c20d.png)

## 忽略控制面板、网络连接、打印机等路径  <font style="color:#DF2A3F;">已修复</font>
![](https://cdn.nlark.com/yuque/0/2023/gif/1617173/1676469160928-a59bc288-6daf-497a-bd49-de58e1006f00.gif)

## 关闭窗口不保存忽略路径  <font style="color:#DF2A3F;">已修复</font>
## 修复图片资源被占用问题  <font style="color:#DF2A3F;">已修复</font>


## <font style="color:#DF2A3F;">Windows11 更新补丁 22621.2361 ，打开资源管理器崩溃报错</font>
> **<font style="color:#DF2A3F;">可能原因：更新补丁之后因为先前设置的禁用win11自带的标签功能，导致崩溃。 </font>**
>

### **版本	Windows 11 专业版**
版本	22H2

安装日期	‎2023/‎6/‎30

操作系统版本	22621.2361

体验	Windows Feature Experience Pack 1000.22674.1000.0

解决办法： 重新激活（重启电脑)、又禁用了一次  (重启电脑），参照链接 [在 Windows 11 上的文件资源管理器中禁用选项卡(需重启电脑)](https://www.yuque.com/indiff/qttabbar/cukpev6roebi325k)


### 安装教程
[安装教程](https://www.ixigua.com/7149475511784669711)

>
北有乔峰南慕容，紫朱何事动天龙。
山依虚竹居然远，人在梦姑无数重。
段誉痴心空欲伴，语焉仙态雾相从。
江湖谁识风流意，不是冤家不易逢。