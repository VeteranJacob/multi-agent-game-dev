using System.Windows;
using System.Windows.Controls;

namespace HealingTempleLedger.Views;

public partial class DashboardPage : Page
{
    public DashboardPage()
    {
        InitializeComponent();
        Loaded += (_, _) => LoadStats();
    }

    private void LoadStats()
    {
        var db = App.Database;
        var entries = db.GetLedgerEntries();
        var entities = db.GetEntities();
        var accounts = db.GetAccounts();

        LedgerCount.Text = entries.Count.ToString("N0");
        EntityCount.Text = entities.Count.ToString("N0");
        AccountCount.Text = accounts.Count.ToString("N0");

        var net = entries.Sum(e => e.Debit - e.Credit);
        NetBalance.Text = net.ToString("C2");
        NetBalance.Foreground = net >= 0
            ? (System.Windows.Media.Brush)FindResource("SuccessBrush")
            : (System.Windows.Media.Brush)FindResource("WarnBrush");

        RecentGrid.ItemsSource = entries.Take(10).ToList();
    }

    private void GoLedger_Click(object s, RoutedEventArgs e)      => Navigate("Ledger");
    private void GoAI_Click(object s, RoutedEventArgs e)           => Navigate("AIAgent");
    private void GoHistory_Click(object s, RoutedEventArgs e)      => Navigate("HistoricalRecord");
    private void GoForm1040_Click(object s, RoutedEventArgs e)     => Navigate("Form1040");
    private void GoForm1120_Click(object s, RoutedEventArgs e)     => Navigate("Form1120");
    private void GoReports_Click(object s, RoutedEventArgs e)      => Navigate("Reports");

    private void Navigate(string tag)
    {
        if (Window.GetWindow(this) is MainWindow mw)
            mw.GetType()
              .GetMethod("Navigate", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
              ?.Invoke(mw, new object[] { tag });
    }
}
