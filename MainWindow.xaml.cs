using System;
using System.Diagnostics;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace AAPASetup
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            CheckFirmwareStatus();
            RefreshPorts();
        }

        private void CheckFirmwareStatus()
        {
            string baseDir = AppDomain.CurrentDomain.BaseDirectory;
            string firmwareDir = Path.Combine(baseDir, "firmware");
            string appBin = Path.Combine(firmwareDir, "AAPA_for_software.ino.bin");
            string bootloaderBin = Path.Combine(firmwareDir, "AAPA_for_software.ino.bootloader.bin");
            string partitionsBin = Path.Combine(firmwareDir, "AAPA_for_software.ino.partitions.bin");

            if (File.Exists(appBin))
            {
                bool fullFlash = File.Exists(bootloaderBin) && File.Exists(partitionsBin);
                Log(fullFlash ? "Firmware detected (Full Flash mode ready)." : "Firmware detected (Update mode ready).");
            }
            else
            {
                Log("WARNING: Firmware not found. Please place .bin files in the 'firmware' subfolder.");
            }
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                this.DragMove();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void BtnRefreshPorts_Click(object sender, RoutedEventArgs e)
        {
            RefreshPorts();
        }

        private void RefreshPorts()
        {
            var ports = SerialPort.GetPortNames().Distinct().OrderBy(p => p).ToList();
            CmbPorts.ItemsSource = ports;
            if (ports.Count > 0)
                CmbPorts.SelectedIndex = 0;
            Log("COM ports refreshed.");
        }

        private void Log(string message)
        {
            Dispatcher.Invoke(() =>
            {
                TxtStatus.AppendText($"[{DateTime.Now:HH:mm:ss}] {message}\n");
                TxtStatus.ScrollToEnd();
            });
        }

        private async void BtnInstallPlugin_Click(object sender, RoutedEventArgs e)
        {
            BtnInstallPlugin.IsEnabled = false;
            Log("Installing NINA Plugin...");

            await Task.Run(() =>
            {
                try
                {
                    string localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                    string pluginDir = Path.Combine(localAppData, "NINA", "Plugins", "3.0.0", "NINA.Plugin.AAPA");
                    
                    if (!Directory.Exists(pluginDir))
                    {
                        Directory.CreateDirectory(pluginDir);
                    }

                    string destPath = Path.Combine(pluginDir, "NINA.Plugin.AAPA.dll");

                    ExtractResource("AAPASetup.NINA.Plugin.AAPA_5.0.0.1.dll", destPath);
                    Log("NINA Plugin installed successfully!");
                }
                catch (Exception ex)
                {
                    Log($"Error installing plugin: {ex.Message}");
                }
            });

            BtnInstallPlugin.IsEnabled = true;
        }

        private async void BtnFlash_Click(object sender, RoutedEventArgs e)
        {
            if (CmbPorts.SelectedItem == null)
            {
                MessageBox.Show("Please select a COM port first.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            string comPort = CmbPorts.SelectedItem.ToString() ?? "";
            BtnFlash.IsEnabled = false;
            Log($"Preparing to flash firmware on {comPort}...");

            await Task.Run(() =>
            {
                try
                {
                    string baseDir = AppDomain.CurrentDomain.BaseDirectory;
                    string firmwareDir = Path.Combine(baseDir, "firmware");
                    
                    // Look for firmware bins in 'firmware' subfolder
                    string appBin = Path.Combine(firmwareDir, "AAPA_for_software.ino.bin");
                    string bootloaderBin = Path.Combine(firmwareDir, "AAPA_for_software.ino.bootloader.bin");
                    string partitionsBin = Path.Combine(firmwareDir, "AAPA_for_software.ino.partitions.bin");

                    if (!File.Exists(appBin))
                    {
                        Log("ERROR: Firmware not found! Please ensure all .bin files are inside a 'firmware' folder next to the Setup.exe.");
                        return;
                    }

                    string tempDir = Path.Combine(Path.GetTempPath(), "AAPASetup");
                    if (!Directory.Exists(tempDir)) Directory.CreateDirectory(tempDir);
                    
                    string esptoolPath = Path.Combine(tempDir, "esptool.exe");
                    ExtractResource("AAPASetup.esptool.exe", esptoolPath);

                    bool fullFlash = File.Exists(bootloaderBin) && File.Exists(partitionsBin);
                    Log(fullFlash ? "Performing FULL FLASH (incl. Bootloader)..." : "Performing UPDATE FLASH...");

                    string args = $"--chip esp32 --port {comPort} --baud 460800 --before default_reset --after hard_reset write_flash -z --flash_mode dio --flash_freq 40m --flash_size detect ";
                    
                    if (fullFlash)
                    {
                        // Some extra boot app bin is sometimes needed, but standard is 0x1000 bootloader, 0x8000 partitions, 0x10000 app
                        string bootApp = Path.Combine(firmwareDir, "boot_app0.bin");
                        args += $"0x1000 \"{bootloaderBin}\" 0x8000 \"{partitionsBin}\" 0x10000 \"{appBin}\"";
                        if (File.Exists(bootApp))
                            args += $" 0xe000 \"{bootApp}\"";
                    }
                    else
                    {
                        args += $"0x10000 \"{appBin}\"";
                    }

                    RunProcess(esptoolPath, args);
                }
                catch (Exception ex)
                {
                    Log($"Flash error: {ex.Message}");
                }
            });

            BtnFlash.IsEnabled = true;
        }

        private void ExtractResource(string resourceName, string outPath)
        {
            var assembly = Assembly.GetExecutingAssembly();
            using (Stream? s = assembly.GetManifestResourceStream(resourceName))
            {
                if (s == null) throw new Exception($"Resource {resourceName} not found.");
                using (FileStream fs = new FileStream(outPath, FileMode.Create))
                {
                    s.CopyTo(fs);
                }
            }
        }

        private void RunProcess(string exe, string args)
        {
            ProcessStartInfo psi = new ProcessStartInfo
            {
                FileName = exe,
                Arguments = args,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };

            using (Process process = new Process())
            {
                process.StartInfo = psi;
                
                process.OutputDataReceived += (s, e) => { if (!string.IsNullOrEmpty(e.Data)) Log(e.Data); };
                process.ErrorDataReceived += (s, e) => { if (!string.IsNullOrEmpty(e.Data)) Log(e.Data); };

                process.Start();
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();
                process.WaitForExit();

                if (process.ExitCode == 0)
                {
                    Log("Flash completed SUCCESSFULLY!");
                }
                else
                {
                    Log($"Flash failed with code {process.ExitCode}. See log above.");
                }
            }
        }

        private void Hyperlink_RequestNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri) { UseShellExecute = true });
            e.Handled = true;
        }
    }
}