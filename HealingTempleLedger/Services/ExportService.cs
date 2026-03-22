using System.IO;
using System.Text;
using ClosedXML.Excel;
using HealingTempleLedger.Models;

namespace HealingTempleLedger.Services;

public static class ExportService
{
    // ── CSV ───────────────────────────────────────────────────────────────────

    public static string ExportLedgerToCsv(List<LedgerEntry> entries)
    {
        var sb = new StringBuilder();
        sb.AppendLine("Date,Description,Category,Debit,Credit,Net,AccountCode,Reference,Reconciled");
        foreach (var e in entries)
            sb.AppendLine($"{e.Date:yyyy-MM-dd},{CsvEscape(e.Description)},{e.Category}," +
                          $"{e.Debit:F2},{e.Credit:F2},{e.Net:F2},{e.AccountCode},{e.Reference},{e.Reconciled}");
        return sb.ToString();
    }

    public static void SaveCsv(string content, string fileName)
    {
        var path = GetSavePath(fileName, ".csv");
        if (path == null) return;
        File.WriteAllText(path, content, Encoding.UTF8);
        OpenFileLocation(path);
    }

    // ── Excel ─────────────────────────────────────────────────────────────────

    public static void ExportLedgerToExcel(List<LedgerEntry> entries)
    {
        var path = GetSavePath("LedgerExport", ".xlsx");
        if (path == null) return;

        using var wb = new XLWorkbook();
        var ws = wb.Worksheets.Add("General Ledger");

        // Header
        var headers = new[] { "Date","Description","Category","Debit","Credit","Net","Account","Reference","Reconciled" };
        for (int i = 0; i < headers.Length; i++)
        {
            ws.Cell(1, i + 1).Value = headers[i];
            ws.Cell(1, i + 1).Style.Font.Bold = true;
            ws.Cell(1, i + 1).Style.Fill.BackgroundColor = XLColor.FromHtml("#161920");
            ws.Cell(1, i + 1).Style.Font.FontColor = XLColor.FromHtml("#00E5A0");
        }

        // Data
        for (int row = 0; row < entries.Count; row++)
        {
            var e = entries[row];
            ws.Cell(row + 2, 1).Value = e.Date.ToString("yyyy-MM-dd");
            ws.Cell(row + 2, 2).Value = e.Description;
            ws.Cell(row + 2, 3).Value = e.Category;
            ws.Cell(row + 2, 4).Value = (double)e.Debit;
            ws.Cell(row + 2, 5).Value = (double)e.Credit;
            ws.Cell(row + 2, 6).Value = (double)e.Net;
            ws.Cell(row + 2, 7).Value = e.AccountCode;
            ws.Cell(row + 2, 8).Value = e.Reference;
            ws.Cell(row + 2, 9).Value = e.Reconciled ? "Yes" : "No";
        }

        // Summary row
        int sumRow = entries.Count + 2;
        ws.Cell(sumRow, 2).Value = "TOTALS";
        ws.Cell(sumRow, 4).FormulaA1 = $"=SUM(D2:D{sumRow - 1})";
        ws.Cell(sumRow, 5).FormulaA1 = $"=SUM(E2:E{sumRow - 1})";
        ws.Cell(sumRow, 6).FormulaA1 = $"=SUM(F2:F{sumRow - 1})";

        ws.Columns().AdjustToContents();

        wb.SaveAs(path);
        OpenFileLocation(path);
    }

    public static void ExportTaxForm1040ToExcel(TaxForm1040 form)
    {
        var path = GetSavePath($"Form1040_{form.TaxYear}", ".xlsx");
        if (path == null) return;

        using var wb = new XLWorkbook();
        var ws = wb.Worksheets.Add("Form 1040");
        WriteFormRow(ws, 1, "FORM 1040 - U.S. INDIVIDUAL INCOME TAX RETURN", "", bold: true);
        WriteFormRow(ws, 2, "Tax Year", form.TaxYear.ToString());
        WriteFormRow(ws, 3, "Taxpayer Name", $"{form.FirstName} {form.LastName}");
        WriteFormRow(ws, 4, "SSN", form.SSN);
        WriteFormRow(ws, 5, "Filing Status", form.FilingStatus);
        WriteFormRow(ws, 6, "", "");
        WriteFormRow(ws, 7, "INCOME", "", bold: true);
        WriteFormRow(ws, 8, "1a. W-2 Wages", form.WagesW2.ToString("C2"));
        WriteFormRow(ws, 9, "2b. Taxable Interest", form.TaxableInterest.ToString("C2"));
        WriteFormRow(ws, 10, "3b. Ordinary Dividends", form.OrdinaryDividends.ToString("C2"));
        WriteFormRow(ws, 11, "Capital Gain/Loss", form.CapitalGainLoss.ToString("C2"));
        WriteFormRow(ws, 12, "Total Income", form.TotalIncome.ToString("C2"), bold: true);
        WriteFormRow(ws, 13, "Adjusted Gross Income", form.AdjustedGrossIncome.ToString("C2"), bold: true);
        ws.Columns().AdjustToContents();
        wb.SaveAs(path);
        OpenFileLocation(path);
    }

    // ── Plain Text ────────────────────────────────────────────────────────────

    public static void ExportTextReport(string title, string content)
    {
        var path = GetSavePath(SanitizeFileName(title), ".txt");
        if (path == null) return;
        var full = $"{title}\n{"=".PadRight(title.Length, '=')}\nGenerated: {DateTime.Now}\n\n{content}";
        File.WriteAllText(path, full, Encoding.UTF8);
        OpenFileLocation(path);
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private static void WriteFormRow(IXLWorksheet ws, int row, string label, string value, bool bold = false)
    {
        ws.Cell(row, 1).Value = label;
        ws.Cell(row, 2).Value = value;
        if (bold) { ws.Cell(row, 1).Style.Font.Bold = true; ws.Cell(row, 2).Style.Font.Bold = true; }
    }

    private static string? GetSavePath(string baseName, string ext)
    {
        var dlg = new Microsoft.Win32.SaveFileDialog
        {
            FileName = $"{baseName}_{DateTime.Now:yyyyMMdd}",
            DefaultExt = ext,
            Filter = ext switch
            {
                ".csv"  => "CSV files (*.csv)|*.csv",
                ".xlsx" => "Excel files (*.xlsx)|*.xlsx",
                ".txt"  => "Text files (*.txt)|*.txt",
                _       => "All files (*.*)|*.*"
            },
            InitialDirectory = App.Settings.Current.ExportPath,
        };
        return dlg.ShowDialog() == true ? dlg.FileName : null;
    }

    private static void OpenFileLocation(string path)
    {
        try { System.Diagnostics.Process.Start("explorer.exe", $"/select,\"{path}\""); }
        catch { /* non-critical */ }
    }

    private static string CsvEscape(string s) =>
        s.Contains(',') || s.Contains('"') || s.Contains('\n')
            ? $"\"{s.Replace("\"", "\"\"")}\"" : s;

    private static string SanitizeFileName(string name) =>
        string.Concat(name.Where(c => !Path.GetInvalidFileNameChars().Contains(c)));
}
