// Views/PageCodeBehinds.cs  — code-behind for remaining fully functional pages

using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using HealingTempleLedger.Models;
using HealingTempleLedger.Services;

namespace HealingTempleLedger.Views
{
    // ── Petition for Redress ─────────────────────────────────────────────────
    public partial class PetitionRedressPage
    {
        private async void Generate_Click(object s, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(PetitionerName.Text) || string.IsNullOrWhiteSpace(GrievanceText.Text))
            { MessageBox.Show("Please fill in your name and describe your grievance.", "Required Fields"); return; }

            GeneratedPetition.Text = "Generating petition…";
            var prompt = $@"Generate a formal Petition for Redress of Grievances with these details:
Petitioner: {PetitionerName.Text}
Date: {PetitionDate.SelectedDate?.ToString("MMMM d, yyyy") ?? DateTime.Today.ToString("MMMM d, yyyy")}
Addressee: {Addressee.Text}
Grievance: {GrievanceText.Text}
Redress Demanded: {RedressText.Text}

Format as a professional formal petition invoking First Amendment rights. Include: opening recitals, statement of facts, legal basis, specific redress demanded, and closing affirmation. Use [PETITIONER NAME], [ADDRESS], and [DATE] placeholders where appropriate.";

            GeneratedPetition.Text = await AIService.ChatAsync(prompt,
                "You draft formal First Amendment petitions for redress of grievances.", agentType: "petition");
        }

        private void Save_Click(object s, RoutedEventArgs e)
        {
            App.Database.SetSetting("draft_petition", GeneratedPetition.Text);
            MessageBox.Show("Draft saved.", "Saved");
        }

        private void Export_Click(object s, RoutedEventArgs e)
            => ExportService.ExportTextReport("Petition_for_Redress", GeneratedPetition.Text);
    }

    // ── Congressional Petition ───────────────────────────────────────────────
    public partial class CongressionalPetitionPage
    {
        private async void Generate_Click(object s, RoutedEventArgs e)
        {
            GeneratedPetition.Text = "Generating memorial petition…";
            var subject = (SubjectCombo.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "Constitutional Governance";
            var prompt = $@"Draft a formal Congressional Memorial Petition with:
Petitioner: {PetitionerName.Text}
Representative: {Representative.Text}
State: {StateBox.Text}
Subject: {subject}
Memorial Statement: {MemorialText.Text}

Format as a professional Congressional Memorial Petition. Reference the relevant statutory history (National Emergencies Act, Proclamation 2039, Senate Report 93-549), invoke the First Amendment right to petition, and clearly state the legislative action requested. Use formal congressional language.";

            GeneratedPetition.Text = await AIService.ChatAsync(prompt,
                "You draft formal Congressional Memorial Petitions using proper legislative language.", agentType: "petition");
        }

        private void Export_Click(object s, RoutedEventArgs e)
            => ExportService.ExportTextReport("Congressional_Memorial_Petition", GeneratedPetition.Text);
    }

    // ── Emergency Powers ─────────────────────────────────────────────────────
    public partial class EmergencyPowersPage
    {
        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);
            Loaded += (_, _) => LoadData();
        }

        private void LoadData()
        {
            StatuteGrid.ItemsSource = new[]
            {
                new { Citation = "12 U.S.C. §95a", Year = "1933", Authority = "Control over banking, transactions, and property of foreign nations and their nationals", Status = "Active" },
                new { Citation = "Proclamation 2039", Year = "1933", Authority = "Emergency Banking Act — Trading with Enemy Act applied domestically to U.S. citizens", Status = "Active" },
                new { Citation = "50 U.S.C. §1701", Year = "1976", Authority = "IEEPA — President may regulate international commerce upon national emergency", Status = "Active" },
                new { Citation = "10 U.S.C. §12304", Year = "1976", Authority = "Presidential authority to order Selected Reserve to active duty during national emergency", Status = "Active" },
                new { Citation = "NEA §101(a), 90 Stat. 1255", Year = "1976", Authority = "Governs termination of national emergencies — conditional, not automatic", Status = "Active" },
                new { Citation = "NEA §502(a), 90 Stat. 1265", Year = "1976", Authority = "Savings Clause: prior emergency actions preserved upon termination", Status = "Active" },
                new { Citation = "Senate Report 93-549", Year = "1973", Authority = "Congressional finding: 470 statutes activated; authority to govern outside constitutional processes", Status = "Published" },
            };

            ElementsList.ItemsSource = new[]
            {
                new { Element = "Element 1 — Agreement / Combination", Finding = "Congress and Executive enacted and preserved emergency statutes enabling governance outside constitutional processes. Agreement evidenced by structure, enactment, and 50+ years of continuation." },
                new { Element = "Element 2 — Criminal Objective", Finding = "Congress's own words: 'enough authority to rule the country without reference to normal constitutional processes.' This is the documented objective and admitted effect." },
                new { Element = "Element 3 — Knowledge and Intent", Finding = "Senate Special Committee (1973) formally documented the scope, the absence of necessity, and the continuation. Actual knowledge is conclusively established by the committee's own findings." },
                new { Element = "Element 4 — Overt Acts", Finding = "Emergency Banking Act (1933). Trading with Enemy Act domestic application. 470 statutes activated. Property seizure authority. Martial law authority. NEA preserving all prior actions after full knowledge." },
                new { Element = "Element 5 — Ongoing Conspiracy", Finding = "The conspiracy is ongoing. Emergency-derived statutes remain operative. No full restoration of constitutional governance has occurred. An unterminated conspiracy continues as a matter of law." },
            };
        }
    }

    // ── Journal ──────────────────────────────────────────────────────────────
    public partial class JournalPage
    {
        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);
            Loaded += (_, _) => RefreshFull();
        }

        private void RefreshFull()
        {
            var entries = App.Database.GetLedgerEntries();
            JournalGrid.ItemsSource = entries;
            JournalCount.Text = entries.Count.ToString("N0");
            JournalDebits.Text = entries.Sum(x => x.Debit).ToString("C2");
            JournalCredits.Text = entries.Sum(x => x.Credit).ToString("C2");
        }

        private void NewEntry_Click(object s, RoutedEventArgs e)
        {
            // Navigate to ledger with new entry panel open
            MessageBox.Show("Use the General Ledger tab to add new journal entries.\nAll ledger entries are part of the journal.", "Journal Entry");
        }
    }

    // ── Chart of Accounts ────────────────────────────────────────────────────
    public partial class ChartOfAccountsPage
    {
        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);
            Loaded += (_, _) => RefreshAccounts();
        }

        private void RefreshAccounts()
        {
            AccountsGrid.ItemsSource = App.Database.GetAccounts();
        }

        private void AddAccount_Click(object s, RoutedEventArgs e)
        {
            var win = new Window
            {
                Title = "Add Account", Width = 400, Height = 320,
                Background = (System.Windows.Media.Brush)FindResource("BgBrush"),
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Owner = Window.GetWindow(this),
            };
            var sp = new StackPanel { Margin = new Thickness(20) };
            sp.Children.Add(new TextBlock { Text = "Account Code", Foreground = (System.Windows.Media.Brush)FindResource("TextDimBrush") });
            var code = new TextBox(); sp.Children.Add(code);
            sp.Children.Add(new TextBlock { Text = "Account Name", Foreground = (System.Windows.Media.Brush)FindResource("TextDimBrush"), Margin = new Thickness(0,8,0,0) });
            var name = new TextBox(); sp.Children.Add(name);
            sp.Children.Add(new TextBlock { Text = "Type", Foreground = (System.Windows.Media.Brush)FindResource("TextDimBrush"), Margin = new Thickness(0,8,0,0) });
            var type = new ComboBox();
            type.Items.Add(new ComboBoxItem { Content = "Asset" });
            type.Items.Add(new ComboBoxItem { Content = "Liability" });
            type.Items.Add(new ComboBoxItem { Content = "Equity" });
            type.Items.Add(new ComboBoxItem { Content = "Revenue" });
            type.Items.Add(new ComboBoxItem { Content = "Expense" });
            type.SelectedIndex = 0;
            sp.Children.Add(type);
            var save = new Button { Content = "💾 Save Account", Margin = new Thickness(0,16,0,0), Padding = new Thickness(12,8,12,8) };
            save.Click += (_, _) =>
            {
                if (string.IsNullOrWhiteSpace(code.Text) || string.IsNullOrWhiteSpace(name.Text)) { MessageBox.Show("Code and Name required."); return; }
                App.Database.SaveAccount(new Account { Code = code.Text.Trim(), Name = name.Text.Trim(), Type = (type.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "Asset", IsActive = true });
                win.Close();
                RefreshAccounts();
            };
            sp.Children.Add(save);
            win.Content = sp;
            win.ShowDialog();
        }
    }

    // ── Entities ─────────────────────────────────────────────────────────────
    public partial class EntitiesPage
    {
        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);
            Loaded += (_, _) => RefreshEntities();
        }

        private void RefreshEntities() => EntityGrid.ItemsSource = App.Database.GetEntities();

        private void AddEntity_Click(object s, RoutedEventArgs e)
        {
            EditPanel.Visibility = Visibility.Visible;
            EntityName.Text = ""; EntityEIN.Text = ""; EntityState.Text = "";
        }

        private void SaveEntity_Click(object s, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(EntityName.Text)) { MessageBox.Show("Name required."); return; }
            App.Database.SaveEntity(new Entity
            {
                Name = EntityName.Text.Trim(),
                Type = (EntityType.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "Corporation",
                EIN = EntityEIN.Text.Trim(),
                State = EntityState.Text.Trim(),
            });
            EditPanel.Visibility = Visibility.Collapsed;
            RefreshEntities();
        }

        private void CancelEntity_Click(object s, RoutedEventArgs e) => EditPanel.Visibility = Visibility.Collapsed;

        private void DeleteEntity_Click(object s, RoutedEventArgs e)
        {
            if (s is Button b && b.Tag is string id)
            {
                if (MessageBox.Show("Delete this entity?", "Confirm", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                { App.Database.DeleteEntity(id); RefreshEntities(); }
            }
        }
    }

    // ── Financial Statements ─────────────────────────────────────────────────
    public partial class FinancialStatementsPage
    {
        protected override void OnInitialized(EventArgs e) { base.OnInitialized(e); Loaded += (_, _) => LoadStatements(); }

        private void LoadStatements()
        {
            var entries = App.Database.GetLedgerEntries();
            var revenue = entries.Where(x => x.Category is "Income" or "Investment Income" or "Capital Gain/Loss").Sum(x => x.Debit);
            var expenses = entries.Where(x => x.Category is "Business Expense").Sum(x => x.Credit);
            var net = revenue - expenses;

            TotalRevenue.Text = revenue.ToString("C2");
            TotalExpenses.Text = expenses.ToString("C2");
            NetIncome.Text = net.ToString("C2");

            IncomeGrid.ItemsSource = entries
                .GroupBy(x => x.Category)
                .Select(g => new { Category = g.Key, Amount = g.Sum(x => x.Debit - x.Credit) })
                .OrderByDescending(x => x.Amount).ToList();

            BalanceGrid.ItemsSource = entries
                .GroupBy(x => x.AccountCode.Length >= 1 ? x.AccountCode[..1] : "?")
                .Select(g => new { Type = g.Key switch { "1" => "Assets", "2" => "Liabilities", "3" => "Equity", "4" => "Revenue", _ => "Expense" }, Balance = g.Sum(x => x.Debit - x.Credit) }).ToList();
        }

        private void Refresh_Click(object s, RoutedEventArgs e) => LoadStatements();
        private void ExportExcel_Click(object s, RoutedEventArgs e) => ExportService.ExportLedgerToExcel(App.Database.GetLedgerEntries());
    }

    // ── Tax Estimator ────────────────────────────────────────────────────────
    public partial class TaxEstimatorPage
    {
        private void Calculate_Click(object s, RoutedEventArgs e)
        {
            if (!decimal.TryParse(AGI.Text, out var agi)) { MessageBox.Show("Invalid AGI."); return; }
            if (!decimal.TryParse(SEIncome.Text, out var se)) se = 0;
            if (!decimal.TryParse(Withholding.Text, out var withholding)) withholding = 0;

            var year = (TaxYear.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "2024";
            var status = (FilingStatus.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "Single";

            var stdDed = (status, year) switch
            {
                ("Single", "2024") => 14600m,
                ("Married Filing Jointly", "2024") => 29200m,
                ("Head of Household", "2024") => 21900m,
                ("Single", "2025") => 15000m,
                ("Married Filing Jointly", "2025") => 30000m,
                _ => 14600m
            };

            var taxableIncome = Math.Max(0, agi - stdDed);

            // 2024 brackets (Single)
            decimal fedTax = status == "Single"
                ? taxableIncome switch
                {
                    <= 11600m => taxableIncome * 0.10m,
                    <= 47150m => 1160m + (taxableIncome - 11600m) * 0.12m,
                    <= 100525m => 5426m + (taxableIncome - 47150m) * 0.22m,
                    <= 191950m => 17168.50m + (taxableIncome - 100525m) * 0.24m,
                    <= 243725m => 39110.50m + (taxableIncome - 191950m) * 0.32m,
                    <= 609350m => 55678.50m + (taxableIncome - 243725m) * 0.35m,
                    _ => 183647.25m + (taxableIncome - 609350m) * 0.37m
                }
                : taxableIncome * 0.22m; // simplified for MFJ

            var seTax = se * 0.153m;
            var totalTax = fedTax + seTax;
            var amountDue = totalTax - withholding;
            var quarterly = Math.Max(0, amountDue) / 4m;

            StdDeduction.Text = stdDed.ToString("C2");
            TaxableIncome.Text = taxableIncome.ToString("C2");
            FedTax.Text = fedTax.ToString("C2");
            SETax.Text = seTax.ToString("C2");
            WithholdingResult.Text = withholding.ToString("C2");
            AmountDue.Text = amountDue.ToString("C2");
            AmountDue.Foreground = amountDue >= 0
                ? (System.Windows.Media.Brush)FindResource("WarnBrush")
                : (System.Windows.Media.Brush)FindResource("SuccessBrush");
            Q1.Text = Q2.Text = Q3.Text = Q4.Text = quarterly.ToString("C2");
            ResultsPanel.Visibility = Visibility.Visible;
        }
    }

    // ── AI Humanizer ─────────────────────────────────────────────────────────
    public partial class AIHumanizerPage
    {
        private async void Humanize_Click(object s, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(InputText.Text)) return;
            OutputText.Text = "Humanizing…";
            var prompt = $"Humanize this AI-generated text. Make it sound natural and professional:\n\n{InputText.Text}";
            OutputText.Text = await AIService.ChatAsync(prompt,
                "You rewrite AI-generated text to sound natural, human, and authentic. Remove all robotic patterns, vary sentence structure, eliminate cliche AI phrases, and make the text read like a knowledgeable human wrote it.", agentType: "humanizer");
        }

        private void ClearInput_Click(object s, RoutedEventArgs e) { InputText.Text = ""; OutputText.Text = ""; }

        private void CopyOutput_Click(object s, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(OutputText.Text)) Clipboard.SetText(OutputText.Text);
        }

        private void ExportOutput_Click(object s, RoutedEventArgs e)
            => ExportService.ExportTextReport("Humanized_Text", OutputText.Text);
    }

    // ── Document Creator ─────────────────────────────────────────────────────
    public partial class DocumentCreatorPage
    {
        private void DocType_Changed(object s, SelectionChangedEventArgs e) { }

        private async void Generate_Click(object s, RoutedEventArgs e)
        {
            var docType = (DocTypeList.SelectedItem as ListBoxItem)?.Content?.ToString() ?? "Letter";
            if (string.IsNullOrWhiteSpace(YourName.Text)) { MessageBox.Show("Your name is required."); return; }

            GeneratedDoc.Text = "Generating document…";
            var prompt = $@"Generate a professional, complete {docType} with these details:
From: {YourName.Text}
To/Recipient: {Recipient.Text}
Details/Context: {Details.Text}
Date: {DateTime.Today:MMMM d, yyyy}

Create a complete, ready-to-use document with proper formatting, all necessary sections, and professional language. Use [PLACEHOLDER] for any information the user still needs to fill in. Include a clear subject line, body, and professional closing.";

            GeneratedDoc.Text = await AIService.ChatAsync(prompt,
                "You are a professional document drafter. You create complete, properly formatted legal and business documents.", agentType: "document");
        }

        private void CopyDoc_Click(object s, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(GeneratedDoc.Text)) Clipboard.SetText(GeneratedDoc.Text);
        }

        private void ExportDoc_Click(object s, RoutedEventArgs e)
            => ExportService.ExportTextReport("Generated_Document", GeneratedDoc.Text);
    }

    // ── Reports ──────────────────────────────────────────────────────────────
    public partial class ReportsPage
    {
        protected override void OnInitialized(EventArgs e) { base.OnInitialized(e); Loaded += (_, _) => LoadReportsFull(); }

        private void LoadReportsFull()
        {
            var entries = App.Database.GetLedgerEntries();
            var entities = App.Database.GetEntities();

            RptIncome.Text = entries.Where(x => x.Category is "Income" or "Investment Income").Sum(x => x.Debit).ToString("C2");
            RptExpenses.Text = entries.Where(x => x.Category == "Business Expense").Sum(x => x.Credit).ToString("C2");
            var net = entries.Sum(x => x.Debit - x.Credit);
            RptNet.Text = net.ToString("C2");
            RptCount.Text = entries.Count.ToString("N0");

            CategoryGrid.ItemsSource = entries
                .GroupBy(x => x.Category)
                .Select(g => new
                {
                    Category = g.Key,
                    Count = g.Count(),
                    TotalDebit = g.Sum(x => x.Debit),
                    TotalCredit = g.Sum(x => x.Credit),
                    Net = g.Sum(x => x.Debit - x.Credit),
                })
                .OrderByDescending(x => x.TotalDebit).ToList();

            EntitySummaryGrid.ItemsSource = entities.Select(en => new
            {
                en.Name, en.Type,
                Balance = entries.Where(x => x.EntityId == en.Id).Sum(x => x.Debit - x.Credit),
            }).ToList();
        }

        private void Refresh_Click(object s, RoutedEventArgs e) => LoadReportsFull();
    }

    // ── Settings ─────────────────────────────────────────────────────────────
    public partial class SettingsPage
    {
        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);
            Loaded += (_, _) =>
            {
                var s = App.Settings.Current;
                ClaudeKey.Text = s.ClaudeApiKey;
                OpenAIKey.Text = s.OpenAIApiKey;
                OllamaHost.Text = s.OllamaHost;
                OllamaModel.Text = s.OllamaModel;
                ExportPath.Text = s.ExportPath;
                foreach (ComboBoxItem item in AIProvider.Items)
                    if (item.Tag?.ToString() == s.AIProvider) { AIProvider.SelectedItem = item; break; }
                UpdatePanels(s.AIProvider);
            };
        }

        private void Provider_Changed(object s, SelectionChangedEventArgs e)
        {
            if (AIProvider.SelectedItem is ComboBoxItem item)
                UpdatePanels(item.Tag?.ToString() ?? "Claude");
        }

        private void UpdatePanels(string provider)
        {
            ClaudePanel.Visibility = provider == "Claude" ? Visibility.Visible : Visibility.Collapsed;
            OpenAIPanel.Visibility = provider == "OpenAI" ? Visibility.Visible : Visibility.Collapsed;
            OllamaPanel.Visibility = provider == "Ollama" ? Visibility.Visible : Visibility.Collapsed;
        }

        private void SaveSettings_Click(object s, RoutedEventArgs e)
        {
            var settings = App.Settings.Current;
            settings.ClaudeApiKey = ClaudeKey.Text.Trim();
            settings.OpenAIApiKey = OpenAIKey.Text.Trim();
            settings.OllamaHost = OllamaHost.Text.Trim();
            settings.OllamaModel = OllamaModel.Text.Trim();
            settings.ExportPath = ExportPath.Text.Trim();
            if (AIProvider.SelectedItem is ComboBoxItem item)
                settings.AIProvider = item.Tag?.ToString() ?? "Claude";
            App.Settings.Save();
            SaveStatus.Text = "✓ Settings saved successfully.";
        }

        private void BrowseExport_Click(object s, RoutedEventArgs e)
        {
            var dlg = new System.Windows.Forms.FolderBrowserDialog { Description = "Select export folder" };
            if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                ExportPath.Text = dlg.SelectedPath;
        }

        private void Hyperlink_Navigate(object s, RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri) { UseShellExecute = true });
            e.Handled = true;
        }
    }
}
