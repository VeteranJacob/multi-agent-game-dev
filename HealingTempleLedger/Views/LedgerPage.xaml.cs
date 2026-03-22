using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using HealingTempleLedger.Models;
using HealingTempleLedger.Services;

namespace HealingTempleLedger.Views;

public partial class LedgerPage : Page
{
    private ObservableCollection<LedgerEntry> _all = new();
    private int _editingId = 0;

    public LedgerPage()
    {
        InitializeComponent();
        EntryDate.SelectedDate = DateTime.Today;
        Loaded += (_, _) => Refresh();
    }

    private void Refresh()
    {
        var entries = App.Database.GetLedgerEntries();
        _all = new ObservableCollection<LedgerEntry>(entries);

        // Populate account dropdown
        var accounts = App.Database.GetAccounts()
            .Select(a => new { Display = $"{a.Code} — {a.Name}", Code = a.Code })
            .ToList();
        EntryAccount.ItemsSource = accounts;

        ApplyFilter();
        UpdateTotals(entries);
    }

    private void ApplyFilter()
    {
        var cat = (CategoryFilter.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "All Categories";
        var search = SearchBox.Text.Trim().ToLowerInvariant();

        var filtered = _all.AsEnumerable();
        if (cat != "All Categories") filtered = filtered.Where(e => e.Category == cat);
        if (!string.IsNullOrEmpty(search))
            filtered = filtered.Where(e =>
                e.Description.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                e.AccountCode.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                e.Reference.Contains(search, StringComparison.OrdinalIgnoreCase));

        LedgerGrid.ItemsSource = filtered.ToList();
        UpdateTotals(filtered.ToList());
    }

    private void UpdateTotals(IEnumerable<LedgerEntry> entries)
    {
        var list = entries.ToList();
        var td = list.Sum(e => e.Debit);
        var tc = list.Sum(e => e.Credit);
        var net = td - tc;
        TotalDebits.Text = td.ToString("C2");
        TotalCredits.Text = tc.ToString("C2");
        NetBalance.Text = net.ToString("C2");
        NetBalance.Foreground = net >= 0
            ? (System.Windows.Media.Brush)FindResource("SuccessBrush")
            : (System.Windows.Media.Brush)FindResource("WarnBrush");
        EntryCount.Text = list.Count.ToString("N0");
    }

    private void Filter_Changed(object s, object e) => ApplyFilter();

    private void NewEntry_Click(object s, RoutedEventArgs e)
    {
        _editingId = 0;
        ClearForm();
        EditPanel.Visibility = Visibility.Visible;
        EntryDesc.Focus();
    }

    private void SaveEntry_Click(object s, RoutedEventArgs e)
    {
        if (!ValidateForm(out var entry)) return;
        entry.Id = _editingId;
        App.Database.SaveLedgerEntry(entry);
        EditPanel.Visibility = Visibility.Collapsed;
        Refresh();
    }

    private bool ValidateForm(out LedgerEntry entry)
    {
        entry = new LedgerEntry();
        if (string.IsNullOrWhiteSpace(EntryDesc.Text))
        { MessageBox.Show("Description is required.", "Validation"); return false; }
        if (!decimal.TryParse(EntryDebit.Text, out var debit))
        { MessageBox.Show("Invalid debit amount.", "Validation"); return false; }
        if (!decimal.TryParse(EntryCredit.Text, out var credit))
        { MessageBox.Show("Invalid credit amount.", "Validation"); return false; }

        entry.Date = EntryDate.SelectedDate ?? DateTime.Today;
        entry.Description = EntryDesc.Text.Trim();
        entry.Category = (EntryCategory.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "Other";
        entry.Debit = debit;
        entry.Credit = credit;
        entry.AccountCode = EntryAccount.SelectedValue?.ToString() ?? "";
        entry.Reference = EntryRef.Text.Trim();
        return true;
    }

    private void CancelEdit_Click(object s, RoutedEventArgs e)
    {
        EditPanel.Visibility = Visibility.Collapsed;
        _editingId = 0;
    }

    private void EditEntry_Click(object s, RoutedEventArgs e)
    {
        if (s is Button btn && btn.Tag is int id)
        {
            var entry = _all.FirstOrDefault(x => x.Id == id);
            if (entry == null) return;
            _editingId = id;
            EntryDate.SelectedDate = entry.Date;
            EntryDesc.Text = entry.Description;
            EntryDebit.Text = entry.Debit.ToString("F2");
            EntryCredit.Text = entry.Credit.ToString("F2");
            EntryRef.Text = entry.Reference;

            // Match category item
            foreach (ComboBoxItem item in EntryCategory.Items)
                if (item.Content?.ToString() == entry.Category) { EntryCategory.SelectedItem = item; break; }

            EditPanel.Visibility = Visibility.Visible;
        }
    }

    private void DeleteEntry_Click(object s, RoutedEventArgs e)
    {
        if (s is Button btn && btn.Tag is int id)
        {
            if (MessageBox.Show("Delete this entry?", "Confirm", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                App.Database.DeleteLedgerEntry(id);
                Refresh();
            }
        }
    }

    private void LedgerGrid_SelectionChanged(object s, SelectionChangedEventArgs e) { }

    private void ExportCsv_Click(object s, RoutedEventArgs e)
    {
        var csv = ExportService.ExportLedgerToCsv(_all.ToList());
        ExportService.SaveCsv(csv, "LedgerExport");
    }

    private void ExportExcel_Click(object s, RoutedEventArgs e)
        => ExportService.ExportLedgerToExcel(_all.ToList());

    private void ImportCsv_Click(object s, RoutedEventArgs e)
    {
        var dlg = new Microsoft.Win32.OpenFileDialog
        { Filter = "CSV files (*.csv)|*.csv", Title = "Import Ledger CSV" };
        if (dlg.ShowDialog() != true) return;
        try
        {
            var lines = System.IO.File.ReadAllLines(dlg.FileName);
            int imported = 0;
            foreach (var line in lines.Skip(1)) // skip header
            {
                var parts = line.Split(',');
                if (parts.Length < 5) continue;
                if (!DateTime.TryParse(parts[0], out var date)) continue;
                if (!decimal.TryParse(parts[3], out var debit)) continue;
                if (!decimal.TryParse(parts[4], out var credit)) continue;
                App.Database.SaveLedgerEntry(new LedgerEntry
                {
                    Date = date, Description = parts[1].Trim('"'),
                    Category = parts[2], Debit = debit, Credit = credit,
                });
                imported++;
            }
            MessageBox.Show($"Imported {imported} entries.", "Import Complete");
            Refresh();
        }
        catch (Exception ex) { MessageBox.Show($"Import failed: {ex.Message}", "Error"); }
    }

    private void ClearForm()
    {
        EntryDate.SelectedDate = DateTime.Today;
        EntryDesc.Text = "";
        EntryDebit.Text = "0.00";
        EntryCredit.Text = "0.00";
        EntryRef.Text = "";
        if (EntryCategory.Items.Count > 0) EntryCategory.SelectedIndex = 0;
    }
}
