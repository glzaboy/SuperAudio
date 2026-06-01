


# SuperAudio

## 项目简介

SuperAudio 是一个基于 WinUI 3 构建的现代化 Windows 桌面应用程序。该项目使用 .NET 和 Microsoft UI XAML 技术栈开发，专为 Windows 11 及以上版本设计，提供优质的音频处理和管理体验。

## 技术栈

- **框架**：WinUI 3 (Windows UI Library)
- **编程语言**：C#
- **.NET 版本**：根据项目配置确定
- **目标平台**：Windows 10/11 (UWP)

## 功能特性

- 原生 Windows 11 风格的现代界面设计
- 高性能音频处理能力
- 流畅的动画和交互体验

## 系统要求

- Windows 10 版本 1809 (build 17763) 或更高版本
- Windows 11

## 安装说明

### 开发环境设置

1. 确保已安装 Visual Studio 2022 或更高版本
2. 安装 ".NET 桌面开发" 工作负载
3. 安装 "Windows 应用开发" 工作负载（含 WinUI 3 支持）
4. 克隆项目仓库并打开 `SuperAudio.slnx` 解决方案文件
5. 在 Visual Studio 中选择目标运行设备
6. 按 F5 或点击运行按钮启动应用程序

### 构建发布版本

在 Visual Studio 中执行以下操作：
1. 将解决方案配置切换为 "Release"
2. 选择合适的目标架构（x64/arm64）
3. 右键点击项目 -> 发布
4. 按照向导完成打包部署

## 项目结构

```
SuperAudio/
├── Assets/                 # 应用资源（图标、启动画面等）
├── Properties/             # 项目属性配置
├── App.xaml               # 应用程序入口定义
├── App.xaml.cs            # 应用程序逻辑代码
├── MainWindow.xaml       # 主窗口界面定义
├── MainWindow.xaml.cs   # 主窗口逻辑代码
└── SuperAudio.csproj    # 项目配置文件
```

## 许可证

本项目的许可证信息请参阅仓库中的 LICENSE 文件。