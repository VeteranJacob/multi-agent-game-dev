using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace HealingTempleLedger.Models;

// ── Ledger ────────────────────────────────────────────────────────────────────

public class LedgerEntry : INotifyPropertyChanged
{
    private int _id;
    private DateTime _date = DateTime.Today;
    private string _description = string.Empty;
    private string _category = "Income";
    private decimal _debit;
    private decimal _credit;
    private string _entityId = string.Empty;
    private string _accountCode = string.Empty;
    private string _reference = string.Empty;
    private bool _reconciled;

    public int Id             { get => _id; set => Set(ref _id, value); }
    public DateTime Date      { get => _date; set => Set(ref _date, value); }
    public string Description { get => _description; set => Set(ref _description, value); }
    public string Category    { get => _category; set => Set(ref _category, value); }
    public decimal Debit      { get => _debit; set => Set(ref _debit, value); }
    public decimal Credit     { get => _credit; set => Set(ref _credit, value); }
    public string EntityId    { get => _entityId; set => Set(ref _entityId, value); }
    public string AccountCode { get => _accountCode; set => Set(ref _accountCode, value); }
    public string Reference   { get => _reference; set => Set(ref _reference, value); }
    public bool Reconciled    { get => _reconciled; set => Set(ref _reconciled, value); }

    public decimal Net => Debit - Credit;

    public event PropertyChangedEventHandler? PropertyChanged;
    private void Set<T>(ref T field, T value, [CallerMemberName] string? name = null)
    {
        if (!Equals(field, value)) { field = value; PropertyChanged?.Invoke(this, new(name)); }
    }
}

public class JournalEntry
{
    public int Id { get; set; }
    public DateTime Date { get; set; } = DateTime.Today;
    public string Reference { get; set; } = string.Empty;
    public string Memo { get; set; } = string.Empty;
    public List<JournalLine> Lines { get; set; } = new();
    public decimal TotalDebit => Lines.Sum(l => l.Debit);
    public decimal TotalCredit => Lines.Sum(l => l.Credit);
    public bool IsBalanced => TotalDebit == TotalCredit;
}

public class JournalLine
{
    public int Id { get; set; }
    public int JournalEntryId { get; set; }
    public string AccountCode { get; set; } = string.Empty;
    public string AccountName { get; set; } = string.Empty;
    public decimal Debit { get; set; }
    public decimal Credit { get; set; }
    public string Description { get; set; } = string.Empty;
}

public class Account
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;   // Asset, Liability, Equity, Revenue, Expense
    public string SubType { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public string EntityId { get; set; } = string.Empty;
    public decimal Balance { get; set; }
}

public class Entity
{
    public string Id { get; set; } = Guid.NewGuid().ToString()[..8];
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = "Corporation"; // Corporation, LLC, Trust, Nonprofit, Sole Prop
    public string EIN { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public string FiscalYearEnd { get; set; } = "12/31";
    public bool IsParent { get; set; }
    public string ParentId { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.Now;
}

// ── Tax Forms ─────────────────────────────────────────────────────────────────

public class TaxForm1040
{
    public int Id { get; set; }
    public int TaxYear { get; set; } = DateTime.Today.Year - 1;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string SSN { get; set; } = string.Empty;
    public string FilingStatus { get; set; } = "Single";
    public string Address { get; set; } = string.Empty;

    // Income
    public decimal WagesW2 { get; set; }
    public decimal TaxExemptInterest { get; set; }
    public decimal TaxableInterest { get; set; }
    public decimal QualifiedDividends { get; set; }
    public decimal OrdinaryDividends { get; set; }
    public decimal IRADistributions { get; set; }
    public decimal TaxableIRA { get; set; }
    public decimal PensionsAnnuities { get; set; }
    public decimal TaxablePension { get; set; }
    public decimal SocialSecurityBenefits { get; set; }
    public decimal TaxableSS { get; set; }
    public decimal CapitalGainLoss { get; set; }
    public decimal OtherIncome { get; set; }

    // Adjustments
    public decimal EducatorExpenses { get; set; }
    public decimal HSADeduction { get; set; }
    public decimal SelfEmploymentTaxHalf { get; set; }
    public decimal SelfEmployedHealthInsurance { get; set; }
    public decimal IRADeduction { get; set; }
    public decimal StudentLoanInterest { get; set; }

    // Deductions
    public string DeductionType { get; set; } = "Standard";
    public decimal MedicalExpenses { get; set; }
    public decimal SALTDeduction { get; set; }
    public decimal MortgageInterest { get; set; }
    public decimal CharitableContributions { get; set; }

    // Tax & Payments
    public decimal ChildTaxCredit { get; set; }
    public decimal OtherCredits { get; set; }
    public decimal FederalTaxWithheld { get; set; }
    public decimal EstimatedTaxPayments { get; set; }
    public decimal EarnedIncomeCredit { get; set; }

    // Computed
    public decimal TotalIncome => WagesW2 + TaxableInterest + OrdinaryDividends + TaxableIRA
        + TaxablePension + TaxableSS + CapitalGainLoss + OtherIncome;
    public decimal AdjustedGrossIncome => TotalIncome - EducatorExpenses - HSADeduction
        - SelfEmploymentTaxHalf - SelfEmployedHealthInsurance - IRADeduction - StudentLoanInterest;
    public decimal StandardDeductionAmount(string status, int year) =>
        (status, year) switch
        {
            ("Single", 2024) => 14600m,
            ("Married Filing Jointly", 2024) => 29200m,
            ("Head of Household", 2024) => 21900m,
            ("Single", 2023) => 13850m,
            ("Married Filing Jointly", 2023) => 27700m,
            ("Head of Household", 2023) => 20800m,
            _ => 14600m
        };
    public DateTime LastSaved { get; set; } = DateTime.Now;
}

public class TaxForm1120
{
    public int Id { get; set; }
    public int TaxYear { get; set; } = DateTime.Today.Year - 1;
    public string CorporationName { get; set; } = string.Empty;
    public string EIN { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public DateTime TaxYearBegin { get; set; }
    public DateTime TaxYearEnd { get; set; }

    // Income
    public decimal GrossReceipts { get; set; }
    public decimal ReturnsAllowances { get; set; }
    public decimal CostOfGoodsSold { get; set; }
    public decimal Dividends { get; set; }
    public decimal Interest { get; set; }
    public decimal GrossRents { get; set; }
    public decimal CapitalGain { get; set; }
    public decimal OtherIncome { get; set; }

    // Deductions
    public decimal CompensationOfficers { get; set; }
    public decimal SalariesWages { get; set; }
    public decimal RepairsMaintenance { get; set; }
    public decimal Rents { get; set; }
    public decimal TaxesLicenses { get; set; }
    public decimal InterestExpense { get; set; }
    public decimal CharitableContributions { get; set; }
    public decimal Depreciation { get; set; }
    public decimal OtherDeductions { get; set; }

    // NOL
    public decimal NOLDeduction { get; set; }
    public decimal SpecialDeductions { get; set; }

    // Tax
    public decimal EstimatedTaxPayments { get; set; }

    public decimal TotalIncome => GrossReceipts - ReturnsAllowances - CostOfGoodsSold
        + Dividends + Interest + GrossRents + CapitalGain + OtherIncome;
    public decimal TotalDeductions => CompensationOfficers + SalariesWages + RepairsMaintenance
        + Rents + TaxesLicenses + InterestExpense + CharitableContributions + Depreciation + OtherDeductions;
    public decimal TaxableIncome => Math.Max(0, TotalIncome - TotalDeductions - NOLDeduction - SpecialDeductions);
    public decimal ComputedTax(int year) => year >= 2018 ? TaxableIncome * 0.21m
        : TaxableIncome <= 50000 ? TaxableIncome * 0.15m
        : TaxableIncome <= 75000 ? 7500 + (TaxableIncome - 50000) * 0.25m
        : TaxableIncome <= 100000 ? 13750 + (TaxableIncome - 75000) * 0.34m
        : TaxableIncome * 0.35m;
}

public class TaxForm990
{
    public int Id { get; set; }
    public int TaxYear { get; set; } = DateTime.Today.Year - 1;
    public string OrganizationName { get; set; } = string.Empty;
    public string EIN { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string TaxExemptStatus { get; set; } = "501(c)(3)";
    public string FormType { get; set; } = "Form 990 (Full)";

    public decimal ContributionsGrants { get; set; }
    public decimal ProgramServiceRevenue { get; set; }
    public decimal MembershipDues { get; set; }
    public decimal InvestmentIncome { get; set; }
    public decimal FundraisingEvents { get; set; }
    public decimal OtherRevenue { get; set; }

    public decimal GrantsSimilar { get; set; }
    public decimal BenefitsToMembers { get; set; }
    public decimal SalariesCompensation { get; set; }
    public decimal ProfessionalFees { get; set; }
    public decimal OfficeExpenses { get; set; }
    public decimal Occupancy { get; set; }
    public decimal Travel { get; set; }
    public decimal OtherExpenses { get; set; }

    public decimal BeginNetAssets { get; set; }
    public decimal EndNetAssets { get; set; }

    public decimal TotalRevenue => ContributionsGrants + ProgramServiceRevenue + MembershipDues
        + InvestmentIncome + FundraisingEvents + OtherRevenue;
    public decimal TotalExpenses => GrantsSimilar + BenefitsToMembers + SalariesCompensation
        + ProfessionalFees + OfficeExpenses + Occupancy + Travel + OtherExpenses;
}

public class TaxForm1041
{
    public int Id { get; set; }
    public int TaxYear { get; set; } = DateTime.Today.Year - 1;
    public string TrustEstateName { get; set; } = string.Empty;
    public string EIN { get; set; } = string.Empty;
    public string FiduciaryName { get; set; } = string.Empty;
    public string FiduciaryTitle { get; set; } = "Trustee";
    public string EntityType { get; set; } = "Simple Trust";
    public DateTime DateCreated { get; set; }

    public decimal InterestIncome { get; set; }
    public decimal OrdinaryDividends { get; set; }
    public decimal QualifiedDividends { get; set; }
    public decimal BusinessIncomeLoss { get; set; }
    public decimal CapitalGainLoss { get; set; }
    public decimal RentsRoyalties { get; set; }
    public decimal OtherIncome { get; set; }

    public decimal InterestExpense { get; set; }
    public decimal Taxes { get; set; }
    public decimal FiduciaryFees { get; set; }
    public decimal CharitableDeduction { get; set; }
    public decimal AttorneyFees { get; set; }
    public decimal OtherDeductions { get; set; }

    public decimal EstimatedTaxPayments { get; set; }

    public decimal TotalIncome => InterestIncome + OrdinaryDividends + BusinessIncomeLoss
        + CapitalGainLoss + RentsRoyalties + OtherIncome;
}

public class ScheduleC
{
    public int Id { get; set; }
    public int TaxYear { get; set; } = DateTime.Today.Year - 1;
    public string ProprietorName { get; set; } = string.Empty;
    public string SSN { get; set; } = string.Empty;
    public string BusinessName { get; set; } = string.Empty;
    public string BusinessCode { get; set; } = string.Empty;
    public string EIN { get; set; } = string.Empty;
    public string AccountingMethod { get; set; } = "Cash";

    public decimal GrossReceipts { get; set; }
    public decimal ReturnsAllowances { get; set; }
    public decimal CostOfGoodsSold { get; set; }
    public decimal OtherIncome { get; set; }

    public decimal Advertising { get; set; }
    public decimal CarTruckExpenses { get; set; }
    public decimal CommissionsFees { get; set; }
    public decimal ContractLabor { get; set; }
    public decimal Depreciation { get; set; }
    public decimal EmployeeBenefits { get; set; }
    public decimal Insurance { get; set; }
    public decimal MortgageInterest { get; set; }
    public decimal OtherInterest { get; set; }
    public decimal LegalProfessional { get; set; }
    public decimal OfficeExpense { get; set; }
    public decimal Rents { get; set; }
    public decimal RepairsMaintenance { get; set; }
    public decimal Supplies { get; set; }
    public decimal TaxesLicenses { get; set; }
    public decimal Travel { get; set; }
    public decimal Meals { get; set; }
    public decimal Utilities { get; set; }
    public decimal Wages { get; set; }
    public decimal OtherExpenses { get; set; }
    public decimal HomeOfficeDeduction { get; set; }

    public decimal GrossIncome => GrossReceipts - ReturnsAllowances - CostOfGoodsSold + OtherIncome;
    public decimal TotalExpenses => Advertising + CarTruckExpenses + CommissionsFees + ContractLabor
        + Depreciation + EmployeeBenefits + Insurance + MortgageInterest + OtherInterest
        + LegalProfessional + OfficeExpense + Rents + RepairsMaintenance + Supplies
        + TaxesLicenses + Travel + (Meals * 0.5m) + Utilities + Wages + OtherExpenses;
    public decimal NetProfitLoss => GrossIncome - TotalExpenses - HomeOfficeDeduction;
}

// ── AI / Chat ─────────────────────────────────────────────────────────────────

public class ChatMessage
{
    public int Id { get; set; }
    public string SessionId { get; set; } = string.Empty;
    public string Role { get; set; } = "user"; // user | assistant | system
    public string Content { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; } = DateTime.Now;
    public string AgentType { get; set; } = "general";
}

// ── Settings ──────────────────────────────────────────────────────────────────

public class AppSettings
{
    public string ClaudeApiKey { get; set; } = string.Empty;
    public string OllamaHost { get; set; } = "http://localhost:11434";
    public string OllamaModel { get; set; } = "phi3:mini";
    public string AIProvider { get; set; } = "Claude";  // Claude | Ollama | OpenAI
    public string OpenAIApiKey { get; set; } = string.Empty;
    public string Theme { get; set; } = "Dark";
    public bool AutoSave { get; set; } = true;
    public int AutoSaveIntervalSeconds { get; set; } = 5;
    public string DefaultEntity { get; set; } = string.Empty;
    public string ExportPath { get; set; } = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
}
