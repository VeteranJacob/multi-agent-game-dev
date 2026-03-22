using System.Windows;
using HealingTempleLedger.Services;

namespace HealingTempleLedger;

public partial class App : Application
{
    public static DatabaseService Database { get; private set; } = null!;
    public static SettingsService Settings { get; private set; } = null!;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        // Initialize core services
        Database = new DatabaseService();
        Database.Initialize();

        Settings = new SettingsService();
        Settings.Load();

        DispatcherUnhandledException += (s, ex) =>
        {
            MessageBox.Show(
                $"An unexpected error occurred:\n\n{ex.Exception.Message}",
                "Healing Temple Ledger — Error",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
            ex.Handled = true;
        };
    }

    protected override void OnExit(ExitEventArgs e)
    {
        Settings.Save();
        base.OnExit(e);
    }
}
