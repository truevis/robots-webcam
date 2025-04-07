# Simple Webcam Viewer

A lightweight Windows application that displays your webcam feed in a minimal, always-on-top, borderless window.

## Features

- Displays live video feed from the first detected webcam
- Always-on-top window stays visible over other applications
- Borderless design for a clean overlay
- Draggable window - click and drag the video feed to move it
- Zoom functionality:
    - `Ctrl +` / `Ctrl =`: Zoom In
    - `Ctrl -` / `Ctrl _`: Zoom Out
    - `Ctrl 0`: Reset to default size (320x240)
- No installation required - portable executable
- Minimal CPU usage
- Simple and intuitive - just run it

## Requirements

- Windows 10/11 (64-bit)
- .NET 8.0 Runtime (usually included in the self-contained build)
- Working webcam

## Usage

2. Simply double-click the EXE file to run the application.
3. The webcam feed will appear in a small, borderless window.
4. Click and drag the video window to position it on your screen.
5. Use the keyboard shortcuts (`Ctrl +`, `Ctrl -`, `Ctrl 0`) to resize the window.
6. Close the window with `Alt-F4` to exit the application.

## Building from Source

The project includes a `publish.bat` script that automates the build process:

1. Clone the repository.
2. Ensure you have the .NET 8 SDK installed.
3. Run `publish.bat` from the command line in the project's root directory.
4. The script will:
   - Clean previous build files.
   - Build a self-contained, single-file executable for 64-bit Windows.
   - Copy the executable (e.g., `WebcamViewer.exe`) to the "output" folder.
   - Clean up temporary build files.

The resulting executable in the "output" folder is portable and typically doesn't require separate installation of the .NET runtime.

## License

This project is licensed under the **Do What The Fuck You Want To Public License (WTFPL)**.
See http://www.wtfpl.net/ for more details.

## Acknowledgments

- [AForge.NET Framework](http://www.aforgenet.com/framework/) (specifically AForge.Video.DirectShow) for webcam access. 