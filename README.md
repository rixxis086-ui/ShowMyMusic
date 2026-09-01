<div align="center">

# 🎵 ShowMyMusic

### *Next-Gen Music Overlay for Windows 10/11*
**Inspired by iOS Dynamic Island & macOS Liquid Glass Aesthetics**

[![.NET 8.0](https://img.shields.io/badge/.NET-8.0_LTS-512BD4?style=for-the-badge&logo=dotnet&logoColor=white)](https://dotnet.microsoft.com/)
[![Platform](https://img.shields.io/badge/Platform-Windows_10_%7C_11-0078D4?style=for-the-badge&logo=windows&logoColor=white)](https://www.microsoft.com/windows)
[![Architecture](https://img.shields.io/badge/Architecture-WPF_%7C_MVVM-68217A?style=for-the-badge&logo=csharp&logoColor=white)]()
[![License](https://img.shields.io/badge/License-MIT-F59E0B?style=for-the-badge&logo=opensourceinitiative&logoColor=white)](LICENSE)

<p align="center">
  <a href="#-key-features">Key Features</a> •
  <a href="#-curated-themes">Curated Themes</a> •
  <a href="#-supported-players">Supported Players</a> •
  <a href="#-getting-started">Getting Started</a> •
  <a href="#-architecture">Architecture</a> •
  <a href="#-license">License</a>
</p>

---

</div>

## 🌟 Overview

**ShowMyMusic** is a lightweight, ultra-sleek desktop overlay for Windows that seamlessly displays your active playing track, high-resolution album artwork, live synchronized timeline, and media controls right over any application or full-screen game.

---

## ✨ Key Features

### 🎨 18-Cluster Smart Adaptive Color Engine
- Real-time color quantization analyzes album artwork across 18 HSL hue clusters.
- Automatic luminance & saturation compensation ensures vivid, readable accents and neon glow on dark and light backgrounds alike.
- Seamless fallback for moody, lofi, or black & white covers (elegant slate/silver monochromatic palette).

### 🌟 Hardware-Accelerated Neon Glow
- Dedicated GPU blur emitter layer with `RenderingBias="Quality"`.
- Configurable glow radius, intensity, and custom RGB/HEX color overrides.

### ⏱️ Smooth 100ms Live Timeline
- Native integration with **Windows System Media Transport Controls (GSMTC)**.
- High-precision time interpolation delivers smooth 100ms progress tracking without stutter or sync resets.

### 🎛️ Dual-State Visibility (Passive & Active)
- **🌙 Passive Mode**: Minimalist, unobtrusive floating pill with transparent click-through (`WS_EX_TRANSPARENT`).
- **✨ Active Mode**: On mouse hover, smoothly fades in playback controls (Play/Pause, Skip, Previous), volume badges, and timeline.

### 🖥️ Multi-Display & DPI-Aware Positioning
- Per-monitor DPI scaling support.
- 8 standard docking presets (`Top-Center`, `Bottom-Right`, etc.) + free-form mouse drag-and-drop.

---

## 🎨 Curated Preset Themes

| Theme | Material | Aesthetics | Adaptive Glow |
| :--- | :--- | :--- | :---: |
| 🍏 **iOS Liquid Glass** | Frosted Glass | Apple-inspired translucent blur with specular highlight glaze | ✅ Active |
| ⬛ **Dynamic Island OLED** | Pure OLED Black | Pitch-black pill aesthetic with smooth curvature | ✅ Subtle |
| 🪟 **Windows 11 Acrylic** | Fluent Acrylic | Modern Windows 11 acrylic gradient with delicate border edges | ➖ Off |
| ⚡ **Cyberpunk Neon** | Dark Glass | High-voltage neon halo glow with vivid high-contrast border | ✅ Intense |
| ❄️ **Apple Light Frosted** | Light Frosted | Bright frosted glass with specular shine and dark typography | ✅ Soft |
| 🎯 **Minimalist Matte** | Solid Matte | Clean, distraction-free matte surface without glow | ➖ Off |

---

## 🎧 Supported Media Players

ShowMyMusic communicates directly with the Windows Media Session API, ensuring out-of-the-box compatibility with:

- 🟢 **Spotify** (Desktop Client & Web Player)
- 🔴 **Apple Music** for Windows
- 🟡 **Yandex Music**
- 🌐 **Web Browsers**: Google Chrome, Microsoft Edge, Mozilla Firefox, Opera, Brave, Vivaldi
- 🎬 **Local Media Players**: VLC, AIMP, MPC-HC, Windows Media Player
- 💬 **Telegram Desktop** & Discord media streams

---

## 🚀 Getting Started

### Prerequisites
- **OS**: Windows 10 (Build 19041+) or Windows 11
- **Runtime**: [.NET 8.0 Desktop Runtime](https://dotnet.microsoft.com/download/dotnet/8.0) *(or .NET 8.0 SDK to build from source)*

### Build & Run from Source
```powershell
# 1. Clone repository
git clone https://github.com/rixxis086-ui/ShowMyMusic.git
cd ShowMyMusic

# 2. Build Release binary
dotnet build -c Release

# 3. Launch application
dotnet run -c Release
```

The compiled standalone executable will be located at:
`bin/Release/net8.0-windows10.0.22621.0/ShowMyMusic.exe`

---

## ⌨️ Global Shortcuts & System Tray

| Action | Default Shortcut / Control |
| :--- | :--- |
| **Toggle Overlay Visibility** | <kbd>Ctrl</kbd> + <kbd>Alt</kbd> + <kbd>M</kbd> |
| **Open Settings** | Double-click System Tray Icon |
| **Play / Pause** | Tray Menu or Hover Controls |
| **Next / Previous Track** | Tray Menu or Hover Controls |
| **Pin / Always on Screen** | Tray Menu Toggle |

---

## 📂 Project Structure

```
ShowMyMusic/
├── Helpers/              # XAML Converters, Win32 Window Styles, GPU Backdrop & Animation Helpers
├── Models/               # AppSettings, TrackInfo, PresetTheme, ScreenInfo (INotifyPropertyChanged)
├── Resources/            # Modern dark UI styles, vector icons, brushes
├── Services/             # GSMTC Media, AudioVolume, ColorExtractor, Display, Settings, Hotkeys, Tray
├── ViewModels/           # OverlayViewModel, SettingsViewModel (MVVM Pattern)
├── Views/                # Transparent Overlay Window, Settings GUI, Custom Controls
│   ├── Controls/         # MusicCardControl (Dynamic Island music pill with neon glow)
│   ├── OverlayWindow     # Click-through Topmost WPF window
│   └── SettingsWindow    # Tabbed configuration suite with live preview
├── app.manifest          # Per-Monitor V2 DPI awareness & Windows 10/11 OS compatibility
└── App.xaml              # Single-instance mutex lifecycle & tray initialization
```

---

## 🤝 Contributing

Contributions, issues, and feature requests are welcome🦛!
Feel free to check the [issues page](https://github.com/rixxis086-ui/ShowMyMusic/issues).

---

## 📄 License

Distributed under the **MIT License**. See [`LICENSE`](LICENSE) for more information.

<div align="center">

Made with ❤️ for music enthusiasts on Windows

</div>
