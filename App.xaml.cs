using System;
using Microsoft.UI.Xaml;
using Velopack;

namespace TailscaleClient;

public partial class App : Application
{
    public static Window MainWindow { get; set; }
    public static bool CanCloseWindow { get; set; } = false;
    public static string UpdateError { get; set; } = null;

    public App()
    {
        VelopackApp.Build().Run();
        InitializeComponent();

        var mgr = new UpdateManager("https://tsc.xirreal.dev");

        try
        {
            var newVersion = mgr.CheckForUpdates();
            if (newVersion == null)
            {
                return;
            }

            mgr.DownloadUpdates(newVersion);
            mgr.ApplyUpdatesAndRestart(newVersion);
        } catch (Exception e)
        {
            System.Diagnostics.Debug.WriteLine($"[App] Update check failed: {e.Message}");
            UpdateError = e.Message;
            return;
        }
    }
    }

    protected override void OnLaunched(LaunchActivatedEventArgs args)
    {
        var initSuccess = Core.API.Init();
        Core.Utils.InitializeColors();

        MainWindow = new MainWindow
        {
            Content = initSuccess ? new Views.AppSkeleton() : new Views.Error()
        };

        MainWindow.Activate();
    }

    public static new void Exit()
    {
        Current.Exit();
    }
}
