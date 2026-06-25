# SuperAudio

## 项目简介

SuperAudio 是一个基于 WinUI 3 构建的现代化 Windows 桌面应用程序。该项目使用 .NET 和 Microsoft UI XAML 技术栈开发，专为 Windows 10/11 设计，提供优质的音频处理和管理体验。

## 技术栈

- **框架**：WinUI 3 (Windows UI Library)
- **编程语言**：C# / .NET
- **目标平台**：Windows 10/11 (UWP)
- **开发工具**：Visual Studio 2022
- **架构模式**：MVVM
- **UI 技术**：Microsoft UI XAML

## 功能特性

### 核心功能

- 🎵 **音频设备管理**：自动检测和管理系统音频播放设备
- 🔗 **音频播放连接**：支持启用和释放音频播放连接
- 📱 **设备监控**：实时监控音频设备的插拔状态

### 界面特性

- 🎨 **原生 Windows 11 风格**：采用 WinUI 3 构建的现代界面设计
- 🌙 **主题切换**：支持深色/浅色主题切换
- 📺 **流畅动画**：提供流畅的动画和交互体验

### 系统集成

- ⚙️ **设置管理**：持久化用户偏好设置
- 🔲 **窗口管理**：支持窗口最小化、大小调整等操作
- 📌 **跳转列表**：支持 Windows 任务栏跳转列表

## 项目结构

```
SuperAudio/
├── Assets/                      # 应用资源（图标、启动画面等）
├── Helpers/                     # 辅助工具类
│   ├── EnumHelper.cs           # 枚举辅助类
│   ├── JumpListHelper.cs       # 跳转列表辅助类
│   ├── NativeMethods.cs        # Windows 原生方法调用
│   ├── NavigationHelper.cs      # 页面导航辅助类
│   ├── NavigationOrientationHelper.cs  # 导航方向辅助类
│   ├── ProcessInfoHelper.cs    # 进程信息辅助类
│   ├── SettingsHelper/         # 设置辅助类
│   │   ├── ObservableSettings.cs
│   │   └── Providers/         # 设置提供者（ApplicationData/JSON）
│   ├── SuspensionManager.cs   # 状态管理类
│   ├── ThemeHelper.cs         # 主题辅助类
│   ├── TitleBarHelper.cs      # 标题栏辅助类
│   ├── UIHelper.cs            # UI辅助类
│   └── WindowHelper.cs        # 窗口辅助类
├── Pages/                       # 页面
│   ├── HomePage.xaml(.cs)      # 首页
│   └── SettingsPage.xaml(.cs) # 设置页
├── Services/                    # 服务层
│   └── PlayerService.cs       # 音频播放服务
├── ViewModels/                  # 视图模型
│   ├── HomePageViewModel.cs   # 首页视图模型
│   ├── SettingsViewModel.cs   # 设置页视图模型
│   └── MainWindowViewModel.cs # 主窗口视图模型
├── Properties/                 # 项目属性配置
├── App.xaml(.cs)             # 应用程序入口
├── MainWindow.xaml(.cs)      # 主窗口
└── SuperAudio.csproj       # 项目文件
```

## 系统要求

- Windows 10 版本 1809 (build 17763) 或更高版本
- Windows 11

## 安装说明

### 开发环境设置

1. 确保已安装 **Visual Studio 2022** 或更高版本
2. 安装以下工作负载：
   - ".NET 桌面开发"
   - "Windows 应用开发"（含 WinUI 3 支持）
3. 克隆项目仓库
4. 使用 Visual Studio 打开 `SuperAudio.slnx` 解决方案文件
5. 选择目标运行设备（x64/arm64）
6. 按 `F5` 或点击运行按钮启动应用程序

### 构建发布版本

1. 将解决方案配置切换为 **Release**
2. 选择合适的目标架构（x64/arm64）
3. 右键点击项目 → **发布**
4. 按照向导完成打包部署

## 核心模块说明

### 辅助类 (Helpers)

| 辅助类 | 功能说明 |
|--------|-----------|
| `NavigationHelper` | 处理页面导航和状态管理 |
| `SettingsHelper` | 应用程序设置管理，支持持久化配置（ApplicationData/JSON） |
| `ThemeHelper` | 主题切换管理，支持深色/浅色主题 |
| `WindowHelper` | 窗口创建和管理 |
| `SuspensionManager` | 会话状态管理和恢复 |
| `UIHelper` | UI 元素查找和辅助功能 |
| `TitleBarHelper` | 标题栏样式管理 |
| `ProcessInfoHelper` | 进程信息和版本获取 |
| `NativeMethods` | Windows 原生 API 调用 |

### 页面 (Pages)

- **HomePage**：应用程序主页，提供音频设备管理和播放连接功能
- **SettingsPage**：设置页面，包含主题、语言、导航等配置选项

### 服务 (Services)

- **PlayerService**：音频播放服务，管理音频设备生命周期
  - 设备监听和状态变化通知
  - 音频播放连接管理
  - 设备热插拔支持

### 视图模型 (ViewModels)

- **MainWindowViewModel**：主窗口视图模型，管理主窗口状态和标题
- **HomePageViewModel**：首页视图模型，管理设备列表和连接状态
- **SettingsViewModel**：设置页视图模型，管理应用程序设置

## 使用说明

### 音频设备管理

1. 启动应用程序后，首页会自动列出可用的音频播放设备
2. 点击设备对应的"连接"按钮启用音频播放连接
3. 点击"释放"按钮断开音频播放连接

### 设置配置

1. 进入设置页面（点击底部导航栏设置图标）
2. 可配置选项包括：
   - **主题模式**：浅色/深色/跟随系统
   - **语言**：自动/English/简体中文/繁體中文
   - **导航位置**：左侧/顶部

## 许可证

本项目的许可证信息请参阅仓库中的 **LICENSE** 文件。