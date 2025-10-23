# JiScreenReco - Screen Recorder

A lightweight, high-quality screen recording application built with C# WPF and FFmpeg. Perfect for creating tutorials, recording presentations, or capturing any screen activity with professional results.

## 🚀 Features

### 🎥 Smart Screen Recording
- **Full Screen Capture**: Record your entire desktop seamlessly
- **Area Selection**: Drag to select specific regions for precision recording
- **High-Quality Output**: Professional-grade video with adjustable settings

### ⚡ Advanced Video Quality
- **Customizable Frame Rates**: 24fps (cinematic), 30fps (smooth), 60fps (gaming)
- **Adjustable Quality**: CRF settings from 18 (high quality) to 25 (balanced)
- **Optimized Encoding**: H.264 compression with ultrafast to slow presets
- **Lossless Frame Extraction**: Perfect PNG screenshots from recordings

### 📷 Tutorial-Focused Tools
- **Frame Extraction**: Convert video recordings into high-quality PNG images
- **Multiple Extraction Modes**:
  - **All Frames**: Every single frame for maximum detail
  - **Key Frames**: Only I-frames for best quality screenshots
  - **Time Interval**: Frames at custom intervals (every 2s, 5s, etc.)
- **Automatic Organization**: Frames saved in timestamped folders

### ⌨️ Efficient Workflow
- **One-Click Recording**: Start/Stop with single keyboard shortcuts
- **Visual Countdown**: 3-second preparation timer before recording
- **Real-time Timer**: Live recording duration display
- **Minimal Interface**: Compact toolbar that stays on top

### 🎯 User Experience
- **System Tray Ready**: Small footprint, doesn't interfere with work
- **Hardware Accelerated**: Efficient CPU usage during recording
- **Cross-Monitor Support**: Works with multi-monitor setups
- **No Watermarks**: Clean recordings without any branding

## 📋 Requirements

### System Requirements
- **OS**: Windows 10 / 11 (64-bit recommended)
- **.NET Framework**: 4.7.2 or later
- **RAM**: 4GB minimum, 8GB recommended
- **Storage**: 100MB free space for application + video storage

### Required Component
- **FFmpeg**: The application automatically detects FFmpeg in:
  - Same folder as `JiScreenReco.exe`
  - System PATH environment variable
  - Common installation directories

## 📥 Installation

### Step-by-Step Setup

1. **Download Application**
   - Go to [Releases](../../releases) page
   - Download the latest `JiScreenReco.zip` file
   - Extract to your preferred location (e.g., `C:\Programs\JiScreenReco\`)

2. **Install FFmpeg** (Required for Recording)
3. **Verify Installation**
- Run `JiScreenReco.exe`
- Status should show "Ready - Full Screen"
- If FFmpeg not found, follow on-screen instructions

## 🎮 Usage Guide

### Basic Recording
1. **Choose Recording Area**:
- Click `🖵` button for Full Screen
- Or select "Select Area" for custom region

2. **Start Recording**:
- Press `F9` or click `▶` button
- 3-second countdown begins
- Recording starts automatically

3. **Stop Recording**:
- Press `F10` or click `⏹` button
- Video saves to `Videos\JiScreenReco\` folder

### Advanced Features

#### Quality Settings
- Click `🖵` → `⚙ Quality Settings` to adjust:
- **Frame Rate**: 24fps, 30fps, or 60fps
- **Quality Level**: CRF 18-25 (lower = better quality)
- **Encoding Preset**: ultrafast, fast, medium, slow

#### Frame Extraction
1. Record your tutorial video
2. Press `F11` or click `📷` button
3. Select your recorded video file
4. Choose extraction mode:
- **Key Frames**: Best for tutorial screenshots
- **Time Interval**: Regular interval screenshots
- **All Frames**: Every single frame

#### Area Selection
1. Click `🖵` → "Select Area"
2. Application minimizes temporarily
3. Drag mouse to select recording area
4. Release to confirm selection
5. Application restores with area dimensions displayed

### Keyboard Shortcuts

| Shortcut | Action | Description |
|----------|--------|-------------|
| `F9` | Start Recording | Begins 3-second countdown |
| `F10` | Stop Recording | Saves current recording |
| `F11` | Extract Frames | Open frame extraction dialog |
| `Ctrl + R` | Start Recording | Alternative start shortcut |
| `Ctrl + S` | Stop Recording | Alternative stop shortcut |
| `ESC` | Emergency Stop | Immediately stops recording |

## 🛠 For Developers

### Project Structure
JiScreenReco/
├── MainWindow.xaml.cs # Main application logic
├── AreaSelectorWindow.xaml.cs # Screen area selection
├── FFmpegTester.cs # FFmpeg detection and validation
├── Frame Extraction/ # Video frame extraction features
└── Properties/ # Application settings and resources

### Technology Stack
- **Frontend**: WPF (Windows Presentation Foundation)
- **Language**: C# .NET Framework 4.7.2
- **Video Processing**: FFmpeg via Process API
- **UI Framework**: XAML with modern styling

### Building from Source
```bash
# Prerequisites
- Visual Studio 2019/2022
- .NET Framework 4.7.2 Developer Pack

# Build Steps
1. git clone https://github.com/yourusername/JiScreenReco.git
2. Open JiScreenReco.sln in Visual Studio
3. Restore NuGet packages
4. Build Solution (Ctrl+Shift+B)
5. Ensure FFmpeg is accessible for testing
