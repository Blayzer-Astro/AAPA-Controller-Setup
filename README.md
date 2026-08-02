# AAPA Controller Setup

This is the standalone Windows setup and firmware flashing utility for the **Astrophilos Automated Polar Alignment (AAPA) Controller**.

## Features

- **Portable Single-File Executable**: Contains all necessary dependencies.
- **NINA Plugin Installation**: Extracts and installs the `NINA.Plugin.AAPA.dll` directly to the user's NINA plugin directory (`%LOCALAPPDATA%\NINA\Plugins\3.0.0\NINA.Plugin.AAPA`).
- **ESP32 Firmware Flasher**: Automatically detects COM ports and uses an embedded `esptool.exe` to flash the ESP32.
- **Smart Flashing**: 
  - If placed next to a `firmware` folder containing just the application binary (`AAPA_for_software.ino.bin`), it performs a quick update flash.
  - If the `firmware` folder also contains `bootloader.bin`, `partitions.bin`, and `boot_app0.bin`, it performs a full factory flash.
- **Modern UI**: Features a sleek, dark-mode Astro-Design with transparent glassmorphism effects.

## How to Build

This project is built using WPF and .NET 8.

1. Clone the repository.
2. Open the folder in Visual Studio or your preferred IDE.
3. Build the project using the .NET CLI or Visual Studio:
   ```bash
   dotnet publish -c Release -r win-x64 --self-contained false /p:PublishSingleFile=true
   ```
4. The resulting `AAPA Controller Setup.exe` will be located in the publish output directory.

## Embedding New Firmware/Plugins

If you wish to update the embedded files (e.g., the NINA plugin DLL or `esptool.exe`), simply replace the files in the project directory. They are included as **Embedded Resources** in the `.csproj` file and will be automatically packed into the executable on the next build.

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

*Note: This software bundles `esptool.exe` (by Espressif Systems), which is licensed under the GNU General Public License v2.0 (GPLv2). The source code for esptool can be found at https://github.com/espressif/esptool.*
