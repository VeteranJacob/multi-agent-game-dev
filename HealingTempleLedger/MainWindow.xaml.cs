using System.Windows;
using System.Windows.Controls;
using HealingTempleLedger.Views;

namespace HealingTempleLedger;

public partial class MainWindow : Window
{
    private readonly Dictionary<string, Func<Page>> _pages;

    public MainWindow()
    {
        InitializeComponent();

        _pages = new Dictionary<string, Func<Page>>
        {
            ["Dashboard"]       = () => new DashboardPage(),
            ["HistoricalRecord"] = () => new HistoricalRecordPage(),
            ["PetitionRedress"] = () => new PetitionRedressPage(),
            ["Congressional"]   = () => new CongressionalPetitionPage(),
            ["EmergencyPowers"] = () => new EmergencyPowersPage(),
            ["Ledger"]          = () => new LedgerPage(),
            ["Journal"]         = () => new JournalPage(),
            ["ChartAccounts"]   = () => new ChartOfAccountsPage(),
            ["Entities"]        = () => new EntitiesPage(),
            ["Statements"]      = () => new FinancialStatementsPage(),
            ["Form1040"]        = () => new Form1040Page(),
            ["Form1120"]        = () => new Form1120Page(),
            ["Form990"]         = () => new Form990Page(),
            ["Form1041"]        = () => new Form1041Page(),
            ["ScheduleC"]       = () => new ScheduleCPage(),
            ["TaxEstimator"]    = () => new TaxEstimatorPage(),
            ["AIAgent"]         = () => new AIAgentPage(),
            ["AIHumanizer"]     = () => new AIHumanizerPage(),
            ["DocumentCreator"] = () => new DocumentCreatorPage(),
            ["Reports"]         = () => new ReportsPage(),
            ["Settings"]        = () => new SettingsPage(),
        };

        Navigate("Dashboard");
    }

    private void Nav_Click(object sender, RoutedEventArgs e)
    {
        if (sender is RadioButton rb && rb.Tag is string tag)
            Navigate(tag);
    }

    private void Navigate(string tag)
    {
        if (_pages.TryGetValue(tag, out var factory))
            MainFrame.Navigate(factory());
    }
}
