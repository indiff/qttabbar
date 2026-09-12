v1.5.5-beta.7适配暗黑模式，支持设置背景图片/Adapts to dark mode and supports setting background pictures

> 使用了 ExplorerTool 项目的hook方式设置背景
>1. 调整为自动加载方式
>2. 修复windows server 操作系统hook失效
>3. 修复默认配置工具栏和插件背景色异常、调整标签高度异常
>4. 调整标签文本为居中显示
>5. 适配暗色模式
>6. 支持设置背景图片
>7. 调整DPI适配
>8. 调整新的日志格式
>9. 修复已知问题
### 自适应暗黑模式
![image](https://user-images.githubusercontent.com/501276/193408254-5d06df30-5662-457f-8c9c-74f3f3a030eb.png)
![深色模式](https://user-images.githubusercontent.com/501276/193408343-301fa176-3593-4e08-8989-6e6c70464577.gif)

### 标签文本居中显示
![image](https://user-images.githubusercontent.com/501276/193408111-3d443f57-e805-4b93-a562-e3fea561b214.png)
### hook方式设置背景图片
![image](https://user-images.githubusercontent.com/501276/193408159-58489943-b2ea-4c1d-a69e-751aa8608886.png)
#### 加载配置文件 C:\ProgramData\QTTabBar\config.ini
```[image]
# 遍历模式生效
random=true
# 图片位置
#0 = Left top
#1 = Right top
#2 = Left bottom
#3 = Right bottom
#4 = Center
#5 = Zoom
#6 = Zoom Fill
posType=1
# 图片透明度
imgAlpha= 180
# 如果未遍历到则加载自定义的图片, 还是未加载则设置一个默认的图片地址 C:\\ProgramData\\QTTabBar\\Image\\bgImage.png
#imgPath="D:\Users\Administrator\Documents\Tencent Files\531299332\Image\Group2\HL\NA\HLNA4R8U3UQ9YK[T4F`X%~I.png"
imgPath="test"```
### 安装教程
[安装教程](https://www.ixigua.com/7149475511784669711)