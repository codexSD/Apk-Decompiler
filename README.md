# APK Decompiler

A modern Windows application for decompiling, editing, recompiling, and signing Android APK files.

## Features

- **Modern UI**: Clean, modern interface with step-by-step progress indication
- **Automatic Tool Management**: Automatically downloads and manages APKTool and Uber APK Signer
- **Java Detection**: Checks for Java installation and guides users to download if needed
- **Complete Workflow**: Decompile → Edit → Recompile → Sign in one application
- **Progress Tracking**: Real-time progress indication for all operations
- **Error Logging**: Comprehensive logging to file for troubleshooting

## Requirements

- Windows 10/11
- Java 8 or higher (will prompt to download if not installed)
- .NET 9.0 Runtime

## Tools Used

- **[APKTool](https://github.com/iBotPeaches/Apktool)**: For decompiling and recompiling APK files
- **[Uber APK Signer](https://github.com/patrickfav/uber-apk-signer)**: For signing APK files with default keys

## How to Use

1. **Launch the application** - It will automatically check for Java and download required tools
2. **Select APK** - Choose the APK file you want to decompile
3. **Decompile** - Extract the APK contents to the workspace folder
4. **Edit** - Click "Open Workspace" to modify the decompiled files externally
5. **Recompile & Sign** - Rebuild and automatically sign the APK with default keys
6. **Get Results** - Find your signed APK in the output folder

## Folder Structure

- `tools/` - Downloaded APKTool and Uber APK Signer
- `workspace/` - Decompiled APK contents (cleared on each new decompilation)
- `output/` - Final recompiled and signed APK files
- `logs/` - Application logs for troubleshooting

## Features

### Step-by-Step Process
1. ☕ **Java Check** - Verify Java installation
2. 🔧 **Tool Management** - Download/update required tools
3. 📱 **APK Selection** - Choose APK file to process
4. 📦 **Decompile** - Extract APK contents
5. ✏️ **Edit Phase** - Modify decompiled files
6. 🔨 **Recompile** - Rebuild APK file
7. 🔐 **Sign APK** - Sign with default keystore
8. ✅ **Complete** - Process finished

### Modern UI Features
- Dark/Light theme support
- Progress indicators for each step
- Real-time status updates
- Drag-and-drop APK selection
- One-click folder access

## Troubleshooting

- **Java not found**: The app will open the Java download page automatically
- **Tool download fails**: Check internet connection and firewall settings
- **Decompilation fails**: Check logs for detailed error information
- **Signing fails**: Ensure the recompiled APK is valid

## Legal Notice

This tool is for educational and development purposes only. Ensure you have proper rights to modify any APK files you process.
