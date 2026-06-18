# SuperAudio

## 项目简介

SuperAudio 是一个基于 WinUI 3 构建的现代化 Windows 桌面应用程序。该项目使用 .NET 和 Microsoft UI XAML 技术栈开发，专为 Windows 10/11 设计，提供优质的音频处理和管理体验。

## 技术栈

- **框架**：WinUI 3 (Windows UI Library)
- **编程语言**：C#
- **目标平台**：Windows 10/11 (UWP)
- **开发工具**：Visual Studio 2022
- **架构模式**：MVVM

## 功能特性

- 原生 Windows 11 风格的现代界面设计
- 高性能音频处理能力
- 音频设备管理与监控
- 音频播放连接管理
- 流畅的动画和交互体验
- 主题切换支持（深色/浅色主题）
- 设置管理功能
- 导航辅助功能
- 窗口管理辅助功能

## 项目结构

```
SuperAudio/
├── Assets/                      # 应用资源（图标、启动画面等）
├── Helpers/                     # 辅助工具类
│   ├── EnumHelper.cs           # 枚举辅助类
│   ├── JumpListHelper.cs       # 跳转列表辅助类
│   ├── NativeMethods.cs        # 原生方法调用
│   ├── NavigationHelper.cs      # 导航辅助类
│   ├── NavigationOrientationHelper.cs  # 导航方向辅助类
│   ├── ProcessInfoHelper.cs    # 进程信息辅助类
│   ├── SettingsHelper/        # 设置辅助类
│   │   ├── ObservableSettings.cs
│   │   └── Providers/         # 设置提供者
│   ├── SuspensionManager.cs    # 状态管理类
│   ├── ThemeHelper.cs         # 主题辅助类
│   ├── TitleBarHelper.cs      # 标题栏辅助类
│   ├── UIHelper.cs           # UI辅助类
│   └── WindowHelper.cs      # 窗口辅助类
├── Pages/                       # 页面
│   ├── HomePage.xaml          # 首页
│   └── SettingsPage.xaml      # 设置页
├── Services/                    # 服务层
│   └── PlayerService.cs      # 播放服务
├── ViewModels/                  # 视图模型
│   ├── HomePageViewModel.cs   # 首页视图模型
│   └── MainWindowViewModel.cs # 主窗口视图模型
├── Properties/                 # 项目属性配置
├── App.xaml                   # 应用程序入口定义
├── App.xaml.cs               # 应用程序逻辑代码
├── MainWindow.xaml          # 主窗口界面定义
├── MainWindow.xaml.cs       # 主窗口逻辑代码
└── SuperAudio.csproj       # 项目配置文件
```

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

## 核心模块说明

### 辅助类 (Helpers)

项目包含多个辅助类，用于支持应用程序的核心功能：

| 辅助类 | 功能说明 |
|--------|-----------|
| NavigationHelper | 处理页面导航和状态管理 |
| SettingsHelper | 应用程序设置管理，支持持久化配置 |
| ThemeHelper | 主题切换管理 |
| WindowHelper | 窗口创建和管理 |
| SuspensionManager | 会话状态管理 |
| UIHelper | UI 元素查找和辅助功能 |
| TitleBarHelper | 标题栏样式管理 |
| ProcessInfoHelper | 进程信息获取 |

### 页面 (Pages)

- **HomePage**：应用程序主页，提供音频设备管理和播放连接功能
- **SettingsPage**：设置页面，包含主题、声音、导航等配置选项

### 服务 (Services)

- **PlayerService**：音频播放服务，管理音频播放生命周期

### 视图模型 (ViewModels)

- **MainWindowViewModel**：主窗口视图模型，管理主窗口状态
- **HomePageViewModel**：首页视图模型，管理首页数据和状态

## 许可证

本项目的许可证信息请参阅仓库中的 LICENSE 文件。