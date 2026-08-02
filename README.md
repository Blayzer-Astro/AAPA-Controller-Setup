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



## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

*Note: This software bundles `esptool.exe` (by Espressif Systems), which is licensed under the GNU General Public License v2.0 (GPLv2). The source code for esptool can be found at https://github.com/espressif/esptool.*
