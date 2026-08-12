v1.5.5-beta.7 Adapts to dark mode, supports setting background pictures/Adapts to dark mode and supports setting background pictures

> Uses the hooking approach from the ExplorerTool project to set the background
>1. Changed to auto-load
>2. Fix the hook not working on Windows Server
>3. Fix abnormal default toolbar/plugin background color, fix abnormal tab height adjustment
>4. Change tab text to centered display
>5. Adapt to dark mode
>6. Support setting a background image
>7. Adjust DPI adaptation
>8. Adjust to a new log format
>9. Fix known issues
### Adaptive dark mode
![image](https://user-images.githubusercontent.com/501276/193408254-5d06df30-5662-457f-8c9c-74f3f3a030eb.png)
![Dark mode](https://user-images.githubusercontent.com/501276/193408343-301fa176-3593-4e08-8989-6e6c70464577.gif)

### Centered tab text display
![image](https://user-images.githubusercontent.com/501276/193408111-3d443f57-e805-4b93-a562-e3fea561b214.png)
### Set background image via hooking
![image](https://user-images.githubusercontent.com/501276/193408159-58489943-b2ea-4c1d-a69e-751aa8608886.png)
#### Load config file C:\ProgramData\QTTabBar\config.ini
```[image]
# Only takes effect in random mode
random=true
# Image position
#0 = Left top
#1 = Right top
#2 = Left bottom
#3 = Right bottom
#4 = Center
#5 = Zoom
#6 = Zoom Fill
posType=1
# Image alpha
imgAlpha= 180
# If nothing is found while enumerating, load the custom image; if still not loaded, fall back to a default image path C:\\ProgramData\\QTTabBar\\Image\\bgImage.png
#imgPath="D:\Users\Administrator\Documents\Tencent Files\531299332\Image\Group2\HL\NA\HLNA4R8U3UQ9YK[T4F`X%~I.png"
imgPath="test"```
### Installation Tutorial
[Installation Tutorial](https://www.ixigua.com/7149475511784669711)