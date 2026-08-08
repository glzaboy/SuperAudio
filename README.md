# SuperAudio

[English](README.md) · [简体中文](README.zh-CN.md) · [繁體中文](README.zh-TW.md)

**Super Bluetooth Speaker** — turn your PC into a Bluetooth speaker and a lightweight audio hub.

[![Get it from Microsoft](https://get.microsoft.com/images/en-us%20dark.svg)](https://get.microsoft.com/installer/download/9ngsn37k2gcc?referrer=appbadge)

## Introduction

SuperAudio is a lightweight Windows desktop app that turns your computer into a **Bluetooth speaker (audio sink)**, so phones and tablets can stream music or any audio to your PC's speakers or headphones over Bluetooth. On top of that, it bundles **system audio recording (loopback)**, a **local media library**, and a **built-in player** — an all-in-one audio companion.

Click the badge above to install SuperAudio from the Microsoft Store.

## Features

### Core

- 🎵 **Audio device management** — auto-detect and manage system playback devices
- 🔗 **Audio playback connection** — enable / release the Bluetooth audio-sink connection
- 📱 **Device monitoring** — real-time tracking of audio device plug/unplug state
- 🔌 **Hot-plug support** — the device list refreshes automatically on connect/disconnect
- 🎙️ **System audio recording (loopback)** — capture the system's output stream and pick the output format
- 📂 **Media library** — browse local music folders and manage audio files
- ▶️ **Built-in player** — in-app playback experience

### Interface

- 🎨 **Native Windows 11 style** — modern UI built with WinUI 3
- 🌙 **Theme switching** — dark / light / follow system
- 📺 **Smooth animations** — fluid transitions and interactions
- 🧭 **Navigation modes** — left sidebar or top navigation bar

### System integration

- ⚙️ **Settings persistence** — user preferences saved via ApplicationData or JSON providers
- 🔲 **Window management** — minimize, resize, and more
- 📌 **Jump List** — Windows taskbar jump list support
- 🌐 **Multilingual** — Auto + 13 locales: English, 简体中文, 繁體中文（台灣）, 繁體中文（香港）, 简体中文（新加坡）, 日本語, 한국어, Français, Deutsch, Español, Italiano, Português (Brasil), Русский

## Tech Stack

| | |
|---|---|
| Framework | WinUI 3 (Windows App SDK 2.2.0) |
| Language | C# / .NET 10 |
| Target OS | Windows 10 (build 17763 / 1809) or later, Windows 11 |
| Architectures | x86, x64, ARM64 |
| Pattern | MVVM (CommunityToolkit.Mvvm 8.4.2) |
| Key libraries | NAudio 2.3.0, WinUIEx 2.9.1, CommunityToolkit.WinUI.Controls.SettingsControls |

## Project Structure

```
SuperAudio/
├── Assets/                # App assets (icons, splash, tiles)
├── Converters/            # Value converters
├── Helpers/               # Helper utilities
│   ├── AppLifeHelper.cs
│   ├── EnumHelper.cs
│   ├── ExplorerHelper.cs
│   ├── JumpListHelper.cs
│   ├── NativeMethods.cs
│   ├── NavigationHelper.cs
│   ├── NavigationOrientationHelper.cs
│   ├── ProcessInfoHelper.cs
│   ├── SettingsHelper/
│   ├── SuspensionManager.cs
│   ├── ThemeHelper.cs
│   ├── TitleBarHelper.cs
│   ├── UIHelper.cs
│   └── WindowHelper.cs
├── Pages/                 # UI pages
│   ├── HomePage.xaml(.cs)        # Home (device management)
│   ├── MediaLibraryPage.xaml(.cs) # Media library
│   ├── PlayerPage.xaml(.cs)      # Player
│   ├── SettingsPage.xaml(.cs)    # Settings
│   └── ChangelogPage.xaml(.cs)   # Changelog / what's new
├── Services/              # Service layer
│   ├── LoopbackRecorder.cs      # Audio recording
│   ├── PlayerInfoItem.cs        # Audio device item model
│   └── PlayerService.cs         # Audio playback service
├── ViewModels/            # View models
│   ├── HomePageViewModel.cs
│   ├── MediaLibraryPageViewModel.cs
│   ├── PlayerPageViewModel.cs
│   ├── SettingsViewModel.cs
│   └── MainWindowViewModel.cs
├── Strings/               # Localization (13 locales)
│   ├── en-US/  zh-CN/  zh-TW/  zh-HK/  zh-SG/
│   └── ja-JP/  ko-KR/  fr-FR/  de-DE/  es-ES/  it-IT/  pt-BR/  ru-RU/
├── App.xaml(.cs)          # Application entry
├── MainWindow.xaml(.cs)   # Main window
└── SuperAudio.csproj      # Project file
```

## Requirements

- Windows 10 (1809 / build 17763) or later, including Windows 11
- For building from source: Visual Studio 2022 with the **.NET Desktop Development** and **Windows App Development** (WinUI 3) workloads

## Build & Run

1. Install **Visual Studio 2022** (or later).
2. Add the workloads: **.NET Desktop Development** and **Windows App Development** (includes WinUI 3).
3. Clone the repository.
4. Open `SuperAudio.slnx` in Visual Studio.
5. Pick a target architecture (**x64** / **ARM64** / **x86**).
6. Press `F5` (or click Run) to launch.

### Create a release package

1. Switch the solution configuration to **Release**.
2. Choose the target architecture (x64 / ARM64 / x86).
3. Right-click the project → **Publish**, then follow the wizard to produce the MSIX package.

## Core Modules

### Helpers

| Helper | Responsibility |
|--------|----------------|
| `AppLifeHelper` | App lifecycle management / restart |
| `NavigationHelper` | Page navigation & state (back/forward) |
| `SettingsHelper` | Settings persistence (ApplicationData / JSON) |
| `ThemeHelper` | Theme switching (dark / light / system) |
| `WindowHelper` | Window creation & tracking |
| `SuspensionManager` | Session state save & restore |
| `UIHelper` | UI element lookup & accessibility |
| `TitleBarHelper` | Title bar styling & system theme adaptation |
| `ProcessInfoHelper` | Process info & version retrieval |
| `NativeMethods` | Windows native API calls / window messages |
| `JumpListHelper` | Taskbar jump list management |
| `NavigationOrientationHelper` | Navigation orientation (side / top) |
| `ExplorerHelper` | Reveal a file in File Explorer |

### Pages

- **HomePage** — device management and playback connection
- **MediaLibraryPage** — browse and manage local music
- **PlayerPage** — playback controls
- **SettingsPage** — theme, language, navigation, and more
- **ChangelogPage** — what's new / changelog

### Services

- **PlayerService** — audio playback service; device lifecycle, connection management, hot-plug support
- **PlayerInfoItem** — audio device item model (info, connection state, enable/release)
- **LoopbackRecorder** — captures system audio output, saves as WAV, manages recording tasks

### ViewModels

- **MainWindowViewModel** — main window state, title, recording control & format selection
- **HomePageViewModel** — device list & connection state
- **MediaLibraryPageViewModel** — file list, path navigation, file operations
- **PlayerPageViewModel** — playlist & playback state
- **SettingsViewModel** — app settings

## Usage

### Audio device management

1. On launch, the home page lists available playback devices.
2. Click **Connect** to enable the audio-sink connection.
3. Click **Release** to disconnect.
4. The list updates live as devices are plugged/unplugged.

### System audio recording

1. Click the record button on the main screen to start capturing system audio.
2. Choose a format (e.g. WAV).
3. When done, find the recording in the media library.

### Media library

1. Open the media library to browse local music folders.
2. Double-click a file to play it.
3. Use **Open file location** to reveal it in File Explorer.

### Settings

1. Open **Settings** from the navigation bar.
2. Configure:
   - **Theme**: Auto / Light / Dark
   - **Language**: Auto / 13 locales (see above)
   - **Navigation**: Sidebar / Top

## License

See the [LICENSE](LICENSE) file for details.
