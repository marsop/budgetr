# Budgetr

Personal Time Budgeting and Accounting

## Live Version

Available at https://albertogregorio.com/budgetr/

## Overview

Budgetr is a cross-platform time-tracking application built with Blazor and .NET MAUI. It helps you track how you spend your time using customizable "meters" with different factors, giving you a balance sheet of your time budget.

## Features

### Time Tracking with Meters
- **Customizable Meters**: Create multiple time trackers with custom names and factors (positive or negative)
- **Factor System**: Assign different weights to activities (e.g., +1.0 for earning time, -0.5 for spending time)
- **One-Touch Activation**: Tap a meter to start/stop tracking time
- **Real-time Duration**: See live updates of running duration

### Overview Dashboard
- **Current Balance**: At-a-glance view of your total time balance in hours
- **Active Indicator**: Shows which meter is currently running

### Timeline View
- **Event History**: View all recorded time events chronologically
- **Daily Breakdown**: See how time was spent each day

### History
- **Complete Log**: Access your full time-tracking history
- **Detailed Events**: View start times, durations, and associated meters

### Sync & Backup
- **Local Export/Import**: Download your data as JSON files
- **Google Drive Integration**: Backup and restore data to/from Google Drive
- **Auto-Sync**: Enable automatic synchronization when connected to Google Drive

### Settings
- **Multi-Language Support**: Available in English, Deutsch, Español, Galego, and Vorarlbergerisch
- **Factory Reset**: Clear all data and start fresh

### Progressive Web App (PWA)
- **Installable**: Add to home screen on mobile devices or desktop
- **Offline Support**: Works offline once installed
- **Service Worker**: Background sync and caching

## Technology Stack

- **.NET 9.0**
- **Blazor WebAssembly** - Web application
- **.NET MAUI** - Native mobile/desktop applications (Android, iOS, macOS, Windows)
- **Shared Razor Components** - Shared UI library between Web and MAUI

## Project Structure

```
budgetr/
├── src/
│   ├── Budgetr.Shared/       # Shared components, services, models
│   │   ├── Components/       # Reusable UI components
│   │   ├── Layout/           # Application layout
│   │   ├── Models/           # Data models (Meter, MeterEvent, TimeAccount)
│   │   ├── Pages/            # Application pages (Overview, Meters, Timeline, etc.)
│   │   ├── Resources/        # Localization files
│   │   └── Services/         # Core business logic services
│   ├── Budgetr.Web/          # Blazor WebAssembly project
│   │   ├── Services/         # Web-specific service implementations
│   │   └── wwwroot/          # Static assets, CSS, JS
│   └── Budgetr.Maui/         # .NET MAUI project
│       ├── Platforms/        # Platform-specific code
│       └── Resources/        # MAUI resources (icons, fonts)
├── tests/                    # Test projects
└── Budgetr.sln               # Solution file
```

## Getting Started

### Prerequisites

- [.NET 9.0 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- For MAUI development:
  - Windows: Visual Studio 2022 with MAUI workload
  - macOS: Visual Studio for Mac or VS Code with C# Dev Kit
  - Android SDK (for Android builds)
  - Xcode (for iOS/macOS builds)

### Running the Web Application

```bash
# Navigate to the web project
cd src/Budgetr.Web

# Run the application
dotnet run
```

The application will be available at `https://localhost:5001` or `http://localhost:5000`.

### Running the MAUI Application

```bash
# Navigate to the MAUI project
cd src/Budgetr.Maui

# Run on Windows
dotnet build -t:Run -f net9.0-windows10.0.19041.0

# Run on Android
dotnet build -t:Run -f net9.0-android

# Run on iOS (macOS only)
dotnet build -t:Run -f net9.0-ios

# Run on macOS (macOS only)
dotnet build -t:Run -f net9.0-maccatalyst
```

### Building for Production

```bash
# Build the Web application
cd src/Budgetr.Web
dotnet publish -c Release

# Build the MAUI application
cd src/Budgetr.Maui
dotnet publish -c Release -f net9.0-android
```

## Google Drive Sync Setup

To enable Google Drive synchronization:

1. Create a Google Cloud Project at [console.cloud.google.com](https://console.cloud.google.com)
2. Enable the Google Drive API
3. Create OAuth 2.0 credentials (Web application type)
4. Add your deployment URL to authorized JavaScript origins
5. Copy the Client ID and configure it in the app's Sync settings

## Data Storage

- **Web**: Data is stored in browser's localStorage
- **MAUI**: Data is stored using platform-specific secure storage

## Author

Alberto Gregorio (https://albertogregorio.com)
