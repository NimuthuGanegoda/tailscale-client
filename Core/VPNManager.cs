using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace TailscaleClient.Core;

internal static class VPNManager
{
    private static readonly string ConfigFilePath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "TailscaleClient",
        "vpn_configs.json"
    );

    private static List<Types.VPNConfiguration> _configurations = new List<Types.VPNConfiguration>();
    private static Types.VPNConfiguration _activeConnection = null;

    public static void Initialize()
    {
        LoadConfigurations();
        
        // Add default VPNBook configurations if no configurations exist
        if (_configurations.Count == 0)
        {
            AddDefaultVPNBookConfigurations();
        }
    }

    private static void AddDefaultVPNBookConfigurations()
    {
        // Add some example VPNBook servers
        // Note: These are example configurations. Users should verify current credentials from vpnbook.com
        var defaultConfigs = new List<Types.VPNConfiguration>
        {
            new Types.VPNConfiguration
            {
                Name = "VPNBook US1",
                ServerAddress = "us1.vpnbook.com",
                Username = "vpnbook",
                Password = "", // Users need to get current password from vpnbook.com
                Protocol = Types.VPNProtocol.OpenVPN,
                IsConnected = false
            },
            new Types.VPNConfiguration
            {
                Name = "VPNBook CA1",
                ServerAddress = "ca1.vpnbook.com",
                Username = "vpnbook",
                Password = "", // Users need to get current password from vpnbook.com
                Protocol = Types.VPNProtocol.OpenVPN,
                IsConnected = false
            },
            new Types.VPNConfiguration
            {
                Name = "VPNBook DE1",
                ServerAddress = "de1.vpnbook.com",
                Username = "vpnbook",
                Password = "", // Users need to get current password from vpnbook.com
                Protocol = Types.VPNProtocol.OpenVPN,
                IsConnected = false
            }
        };

        _configurations.AddRange(defaultConfigs);
        SaveConfigurations();
    }

    public static List<Types.VPNConfiguration> GetConfigurations()
    {
        return _configurations;
    }

    public static void AddConfiguration(Types.VPNConfiguration config)
    {
        _configurations.Add(config);
        SaveConfigurations();
    }

    public static void UpdateConfiguration(int index, Types.VPNConfiguration config)
    {
        if (index >= 0 && index < _configurations.Count)
        {
            _configurations[index] = config;
            SaveConfigurations();
        }
    }

    public static void DeleteConfiguration(int index)
    {
        if (index >= 0 && index < _configurations.Count)
        {
            _configurations.RemoveAt(index);
            SaveConfigurations();
        }
    }

    public static async Task<bool> ConnectVPN(Types.VPNConfiguration config)
    {
        try
        {
            // Disconnect any existing connection first
            if (_activeConnection != null)
            {
                await DisconnectVPN();
            }

            // For Windows, we'll use rasdial for PPTP/L2TP connections
            // For OpenVPN, this would require OpenVPN client to be installed
            switch (config.Protocol)
            {
                case Types.VPNProtocol.PPTP:
                case Types.VPNProtocol.L2TP:
                    return await ConnectWindowsVPN(config);
                case Types.VPNProtocol.OpenVPN:
                    return await ConnectOpenVPN(config);
                default:
                    Debug.WriteLine($"Unsupported VPN protocol: {config.Protocol}");
                    return false;
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error connecting to VPN: {ex.Message}");
            return false;
        }
    }

    private static async Task<bool> ConnectWindowsVPN(Types.VPNConfiguration config)
    {
        try
        {
            // First, create the VPN connection if it doesn't exist
            var connectionName = $"TailscaleVPN_{config.Name.Replace(" ", "_")}";
            
            // Check if connection exists, if not create it
            var checkProcess = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "rasdial.exe",
                    Arguments = connectionName,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                }
            };

            checkProcess.Start();
            await checkProcess.WaitForExitAsync();

            // Create the connection using rasphone.exe or PowerShell
            if (checkProcess.ExitCode != 0)
            {
                var createProcess = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "powershell.exe",
                        Arguments = $"-Command \"Add-VpnConnection -Name '{connectionName}' -ServerAddress '{config.ServerAddress}' -TunnelType {config.Protocol} -Force\"",
                        UseShellExecute = false,
                        CreateNoWindow = true,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        Verb = "runas" // May need admin rights
                    }
                };

                createProcess.Start();
                await createProcess.WaitForExitAsync();
            }

            // Connect to the VPN
            var connectProcess = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "rasdial.exe",
                    Arguments = $"\"{connectionName}\" \"{config.Username}\" \"{config.Password}\"",
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                }
            };

            connectProcess.Start();
            var output = await connectProcess.StandardOutput.ReadToEndAsync();
            await connectProcess.WaitForExitAsync();

            if (connectProcess.ExitCode == 0)
            {
                config.IsConnected = true;
                config.LastConnected = DateTime.UtcNow;
                _activeConnection = config;
                
                // Update the configuration in the list
                var index = _configurations.FindIndex(c => c.Name == config.Name);
                if (index >= 0)
                {
                    _configurations[index] = config;
                    SaveConfigurations();
                }
                
                return true;
            }
            else
            {
                Debug.WriteLine($"VPN connection failed: {output}");
                return false;
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error in ConnectWindowsVPN: {ex.Message}");
            return false;
        }
    }

    private static async Task<bool> ConnectOpenVPN(Types.VPNConfiguration config)
    {
        // OpenVPN would require the OpenVPN client to be installed
        // This is a placeholder for OpenVPN connection logic
        Debug.WriteLine("OpenVPN support requires OpenVPN client to be installed");
        Debug.WriteLine($"To connect to {config.ServerAddress}, please use OpenVPN client with VPNBook configuration files");
        
        // Return false as we're not actually connecting
        // Users would need to manually configure OpenVPN
        return false;
    }

    public static async Task<bool> DisconnectVPN()
    {
        if (_activeConnection == null)
        {
            return true;
        }

        try
        {
            var connectionName = $"TailscaleVPN_{_activeConnection.Name.Replace(" ", "_")}";
            
            var disconnectProcess = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "rasdial.exe",
                    Arguments = $"\"{connectionName}\" /DISCONNECT",
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                }
            };

            disconnectProcess.Start();
            await disconnectProcess.WaitForExitAsync();

            _activeConnection.IsConnected = false;
            
            // Update the configuration in the list
            var index = _configurations.FindIndex(c => c.Name == _activeConnection.Name);
            if (index >= 0)
            {
                _configurations[index].IsConnected = false;
                SaveConfigurations();
            }
            
            _activeConnection = null;
            return true;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error disconnecting VPN: {ex.Message}");
            return false;
        }
    }

    public static Types.VPNConfiguration GetActiveConnection()
    {
        return _activeConnection;
    }

    public static bool IsConnected()
    {
        return _activeConnection != null && _activeConnection.IsConnected;
    }

    private static void LoadConfigurations()
    {
        try
        {
            if (File.Exists(ConfigFilePath))
            {
                var json = File.ReadAllText(ConfigFilePath);
                _configurations = JsonSerializer.Deserialize<List<Types.VPNConfiguration>>(json) ?? new List<Types.VPNConfiguration>();
                
                // Mark all as disconnected on startup
                foreach (var config in _configurations)
                {
                    config.IsConnected = false;
                }
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error loading VPN configurations: {ex.Message}");
            _configurations = new List<Types.VPNConfiguration>();
        }
    }

    private static void SaveConfigurations()
    {
        try
        {
            var directory = Path.GetDirectoryName(ConfigFilePath);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            var json = JsonSerializer.Serialize(_configurations, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(ConfigFilePath, json);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error saving VPN configurations: {ex.Message}");
        }
    }
}
