# 遥感影像处理桌面系统

基于 C# WinForms + GDAL 的遥感影像处理桌面应用。

## 功能

- 影像打开、保存、波段合成、灰度化
- 2% 线性拉伸、亮度对比度、影像取反
- 植被指数（NDVI、EVI、SAVI、MSAVI）
- 地物指数（NDWI、MNDWI、NDBI）
- 图像增强（均值/高斯滤波、拉普拉斯锐化、Sobel 边缘检测、密度分割）
- K-Means 非监督分类
- 自定义波段表达式计算

## 直接运行（推荐）

到 [Releases](../../releases) 页面下载 `RemoteSensingProcessor-win-x64.zip`，解压后运行其中的 `RemoteSensingProcessor.exe`。

> 必须保留整个解压文件夹，不要只拷贝 exe。

## 从源码构建

```cmd
dotnet publish RemoteSensingProcessor.csproj -c Release -r win-x64 --self-contained true -o PUBLISH
```

或执行：

```cmd
build.cmd
```

## 环境要求

- Windows 10/11 x64
- 从 Release 下载的版本无需安装 .NET
- 从源码构建需要 .NET 9 SDK
