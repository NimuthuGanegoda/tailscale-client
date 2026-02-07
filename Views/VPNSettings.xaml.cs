using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using TailscaleClient.Core;

namespace TailscaleClient.Views;

public class VPNConfigItem : INotifyPropertyChanged
{
    public int Index { get; set; }
    public string Name { get; set; }
    public string ServerAddress { get; set; }
    public string ProtocolText { get; set; }
    public Types.VPNConfiguration Config { get; set; }

    public event PropertyChangedEventHandler PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

public sealed partial class VPNSettings : Page, INotifyPropertyChanged
{
    private ObservableCollection<VPNConfigItem> _vpnItems = new ObservableCollection<VPNConfigItem>();

    public event PropertyChangedEventHandler PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public VPNSettings()
    {
        InitializeComponent();
        VPNManager.Initialize();
        LoadVPNConfigurations();
        UpdateVPNStatus();
    }

    private void LoadVPNConfigurations()
    {
        _vpnItems.Clear();
        var configs = VPNManager.GetConfigurations();
        
        for (int i = 0; i < configs.Count; i++)
        {
            var config = configs[i];
            _vpnItems.Add(new VPNConfigItem
            {
                Index = i,
                Name = config.Name,
                ServerAddress = config.ServerAddress,
                ProtocolText = $"Protocol: {config.Protocol}",
                Config = config
            });
        }

        VPNListView.ItemsSource = _vpnItems;
    }

    private void UpdateVPNStatus()
    {
        var activeConnection = VPNManager.GetActiveConnection();
        if (activeConnection != null && activeConnection.IsConnected)
        {
            VPNStatusText.Text = $"Connected to {activeConnection.Name}";
            DisconnectButton.IsEnabled = true;
        }
        else
        {
            VPNStatusText.Text = "Not Connected";
            DisconnectButton.IsEnabled = false;
        }
    }

    private async void ConnectVPN_Click(object sender, RoutedEventArgs e)
    {
        var button = sender as Button;
        if (button?.Tag is int index)
        {
            button.IsEnabled = false;
            button.Content = "Connecting...";

            var configs = VPNManager.GetConfigurations();
            if (index >= 0 && index < configs.Count)
            {
                var config = configs[index];
                
                // Check if password is set
                if (string.IsNullOrEmpty(config.Password))
                {
                    await ShowPasswordDialog(config, index);
                    button.IsEnabled = true;
                    button.Content = "Connect";
                    return;
                }

                var success = await VPNManager.ConnectVPN(config);
                
                if (success)
                {
                    UpdateVPNStatus();
                    await ShowMessageDialog("Success", $"Connected to {config.Name}");
                }
                else
                {
                    await ShowMessageDialog("Connection Failed", 
                        $"Failed to connect to {config.Name}. Please check your credentials and ensure you have administrator privileges.");
                }
            }

            button.IsEnabled = true;
            button.Content = "Connect";
        }
    }

    private async void DisconnectVPN_Click(object sender, RoutedEventArgs e)
    {
        DisconnectButton.IsEnabled = false;
        var success = await VPNManager.DisconnectVPN();
        UpdateVPNStatus();
        DisconnectButton.IsEnabled = VPNManager.IsConnected();
    }

    private async void EditVPN_Click(object sender, RoutedEventArgs e)
    {
        var button = sender as Button;
        if (button?.Tag is int index)
        {
            var configs = VPNManager.GetConfigurations();
            if (index >= 0 && index < configs.Count)
            {
                await ShowEditDialog(configs[index], index);
            }
        }
    }

    private async void AddVPN_Click(object sender, RoutedEventArgs e)
    {
        await ShowEditDialog(null, -1);
    }

    private async System.Threading.Tasks.Task ShowPasswordDialog(Types.VPNConfiguration config, int index)
    {
        var dialog = new ContentDialog
        {
            Title = "Enter VPN Password",
            PrimaryButtonText = "Connect",
            CloseButtonText = "Cancel",
            DefaultButton = ContentDialogButton.Primary,
            XamlRoot = this.XamlRoot
        };

        var passwordBox = new PasswordBox
        {
            PlaceholderText = "Password",
            Margin = new Thickness(0, 8, 0, 8)
        };

        var stackPanel = new StackPanel();
        stackPanel.Children.Add(new TextBlock 
        { 
            Text = $"Please enter the password for {config.Name}",
            TextWrapping = TextWrapping.WrapWholeWords,
            Margin = new Thickness(0, 0, 0, 8)
        });
        stackPanel.Children.Add(new TextBlock 
        { 
            Text = "You can find the current password at vpnbook.com",
            FontSize = 12,
            Foreground = (Microsoft.UI.Xaml.Media.Brush)Application.Current.Resources["TextFillColorSecondaryBrush"],
            TextWrapping = TextWrapping.WrapWholeWords,
            Margin = new Thickness(0, 0, 0, 8)
        });
        stackPanel.Children.Add(passwordBox);

        dialog.Content = stackPanel;

        var result = await dialog.ShowAsync();

        if (result == ContentDialogResult.Primary && !string.IsNullOrEmpty(passwordBox.Password))
        {
            config.Password = passwordBox.Password;
            VPNManager.UpdateConfiguration(index, config);
            
            var success = await VPNManager.ConnectVPN(config);
            
            if (success)
            {
                UpdateVPNStatus();
                await ShowMessageDialog("Success", $"Connected to {config.Name}");
            }
            else
            {
                await ShowMessageDialog("Connection Failed", 
                    $"Failed to connect to {config.Name}. Please check your credentials.");
            }
        }
    }

    private async System.Threading.Tasks.Task ShowEditDialog(Types.VPNConfiguration config, int index)
    {
        var isNew = config == null;
        if (isNew)
        {
            config = new Types.VPNConfiguration
            {
                Protocol = Types.VPNProtocol.PPTP
            };
        }

        var dialog = new ContentDialog
        {
            Title = isNew ? "Add VPN Configuration" : "Edit VPN Configuration",
            PrimaryButtonText = "Save",
            CloseButtonText = "Cancel",
            DefaultButton = ContentDialogButton.Primary,
            XamlRoot = this.XamlRoot
        };

        var nameBox = new TextBox { PlaceholderText = "Name", Text = config.Name ?? "", Margin = new Thickness(0, 4, 0, 4) };
        var serverBox = new TextBox { PlaceholderText = "Server Address", Text = config.ServerAddress ?? "", Margin = new Thickness(0, 4, 0, 4) };
        var usernameBox = new TextBox { PlaceholderText = "Username", Text = config.Username ?? "", Margin = new Thickness(0, 4, 0, 4) };
        var passwordBox = new PasswordBox { PlaceholderText = "Password (optional)", Password = config.Password ?? "", Margin = new Thickness(0, 4, 0, 4) };
        
        var protocolCombo = new ComboBox { Margin = new Thickness(0, 4, 0, 4), HorizontalAlignment = HorizontalAlignment.Stretch };
        protocolCombo.Items.Add("PPTP");
        protocolCombo.Items.Add("L2TP");
        protocolCombo.Items.Add("OpenVPN");
        protocolCombo.SelectedIndex = (int)config.Protocol;

        var stackPanel = new StackPanel();
        stackPanel.Children.Add(new TextBlock { Text = "Name:", Margin = new Thickness(0, 8, 0, 0) });
        stackPanel.Children.Add(nameBox);
        stackPanel.Children.Add(new TextBlock { Text = "Server Address:", Margin = new Thickness(0, 8, 0, 0) });
        stackPanel.Children.Add(serverBox);
        stackPanel.Children.Add(new TextBlock { Text = "Username:", Margin = new Thickness(0, 8, 0, 0) });
        stackPanel.Children.Add(usernameBox);
        stackPanel.Children.Add(new TextBlock { Text = "Password:", Margin = new Thickness(0, 8, 0, 0) });
        stackPanel.Children.Add(passwordBox);
        stackPanel.Children.Add(new TextBlock { Text = "Protocol:", Margin = new Thickness(0, 8, 0, 0) });
        stackPanel.Children.Add(protocolCombo);

        if (!isNew)
        {
            var deleteButton = new Button 
            { 
                Content = "Delete Configuration", 
                Margin = new Thickness(0, 16, 0, 0),
                HorizontalAlignment = HorizontalAlignment.Stretch
            };
            deleteButton.Click += async (s, e) =>
            {
                dialog.Hide();
                VPNManager.DeleteConfiguration(index);
                LoadVPNConfigurations();
            };
            stackPanel.Children.Add(deleteButton);
        }

        dialog.Content = stackPanel;

        var result = await dialog.ShowAsync();

        if (result == ContentDialogResult.Primary)
        {
            config.Name = nameBox.Text;
            config.ServerAddress = serverBox.Text;
            config.Username = usernameBox.Text;
            config.Password = passwordBox.Password;
            config.Protocol = (Types.VPNProtocol)protocolCombo.SelectedIndex;

            if (isNew)
            {
                VPNManager.AddConfiguration(config);
            }
            else
            {
                VPNManager.UpdateConfiguration(index, config);
            }

            LoadVPNConfigurations();
        }
    }

    private async System.Threading.Tasks.Task ShowMessageDialog(string title, string message)
    {
        var dialog = new ContentDialog
        {
            Title = title,
            Content = message,
            CloseButtonText = "OK",
            XamlRoot = this.XamlRoot
        };

        await dialog.ShowAsync();
    }
}
