using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
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
                
                // Warn about PPTP security
                if (config.Protocol == Types.VPNProtocol.PPTP)
                {
                    var warningDialog = new ContentDialog
                    {
                        Title = "Security Warning",
                        Content = "PPTP is a deprecated protocol with known security vulnerabilities. It is not recommended for use. Consider using L2TP, OpenVPN, or IKEv2 instead.\n\nDo you want to continue?",
                        PrimaryButtonText = "Continue Anyway",
                        CloseButtonText = "Cancel",
                        DefaultButton = ContentDialogButton.Close,
                        XamlRoot = this.XamlRoot
                    };
                    
                    var result = await warningDialog.ShowAsync();
                    if (result != ContentDialogResult.Primary)
                    {
                        button.IsEnabled = true;
                        button.Content = "Connect";
                        return;
                    }
                }
                
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
        protocolCombo.Items.Add("PPTP (Deprecated - Insecure)");
        protocolCombo.Items.Add("L2TP");
        protocolCombo.Items.Add("OpenVPN");
        protocolCombo.Items.Add("IKEv2");
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

        // Application Triggers (Split Tunneling)
        stackPanel.Children.Add(new TextBlock { Text = "Application Triggers (Split Tunneling):", FontWeight = Microsoft.UI.Text.FontWeights.SemiBold, Margin = new Thickness(0, 16, 0, 4) });
        stackPanel.Children.Add(new TextBlock { Text = "Added apps will trigger the VPN and route traffic through it.", FontSize = 12, Foreground = (Microsoft.UI.Xaml.Media.Brush)Application.Current.Resources["TextFillColorSecondaryBrush"], TextWrapping = TextWrapping.WrapWholeWords });

        var appTriggersPanel = new StackPanel { Spacing = 4, Margin = new Thickness(0, 8, 0, 8) };
        var currentTriggers = new List<string>(config.AppTriggers);

        void RefreshAppTriggers()
        {
            appTriggersPanel.Children.Clear();
            if (currentTriggers.Count == 0)
            {
                appTriggersPanel.Children.Add(new TextBlock { Text = "No app triggers defined.", FontStyle = Microsoft.UI.Text.FontStyle.Italic, FontSize = 12, Foreground = (Microsoft.UI.Xaml.Media.Brush)Application.Current.Resources["TextFillColorSecondaryBrush"] });
            }
            else
            {
                foreach (var app in currentTriggers)
                {
                    var appGrid = new Grid();
                    appGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                    appGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

                    var nameLabel = new TextBlock { Text = Path.GetFileName(app), VerticalAlignment = VerticalAlignment.Center, TextTrimming = TextTrimming.CharacterEllipsis };
                    ToolTipService.SetToolTip(nameLabel, app);
                    
                    var removeBtn = new Button { Content = "Remove", FontSize = 10, Padding = new Thickness(4, 2, 4, 2) };
                    var currentApp = app;
                    removeBtn.Click += (s, e) => { currentTriggers.Remove(currentApp); RefreshAppTriggers(); };

                    appGrid.Children.Add(nameLabel);
                    Grid.SetColumn(nameLabel, 0);
                    appGrid.Children.Add(removeBtn);
                    Grid.SetColumn(removeBtn, 1);

                    appTriggersPanel.Children.Add(appGrid);
                }
            }
        }

        RefreshAppTriggers();
        stackPanel.Children.Add(appTriggersPanel);

        var addAppBtn = new Button { Content = "Add Application", HorizontalAlignment = HorizontalAlignment.Stretch };
        addAppBtn.Click += async (s, e) =>
        {
            var picker = new Windows.Storage.Pickers.FileOpenPicker();
            picker.ViewMode = Windows.Storage.Pickers.PickerViewMode.List;
            picker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.ComputerFolder;
            picker.FileTypeFilter.Add(".exe");

            // Need to associate with window for WinUI3
            var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(App.MainWindow);
            WinRT.Interop.InitializeWithWindow.Initialize(picker, hwnd);

            var file = await picker.PickSingleFileAsync();
            if (file != null)
            {
                if (!currentTriggers.Contains(file.Path))
                {
                    currentTriggers.Add(file.Path);
                    RefreshAppTriggers();
                }
            }
        };
        stackPanel.Children.Add(addAppBtn);

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
            config.AppTriggers = currentTriggers;

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
