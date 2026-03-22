# Healing Temple Ledger

**App:** Healing Temple Ledger
**Platform:** Windows 10/11 64-bit  
**Framework:** C# / WPF / .NET 8  
**Version:** 1.0.0  

A fully local Windows desktop application embedding every feature from **redressright.me** and **redressright.me/GAAP.html**, plus a 13-agent AI system, complete GAAPCLAW accounting ledger, and all major IRS tax forms.

---

## Features

| Module | Description |
|---|---|
| 📜 Historical Record | Full U.S. emergency powers timeline 1933–present |
| 📋 Petition for Redress | Draft and export First Amendment petitions |
| 🏛 Congressional Petition | Memorial and Congressional Legislative Petitions |
| ⚡ Emergency Powers | Statutory analysis with full citation index |
| 📒 General Ledger | GAAP double-entry accrual ledger with CSV/Excel export |
| 📝 Journal Entries | Full journal entry view |
| 📋 Chart of Accounts | 30 default GAAP accounts, fully editable |
| 🏢 Entities | Multi-entity management (Corp, LLC, Trust, Nonprofit) |
| 📑 Financial Statements | Auto-calculated Income Statement & Balance Sheet |
| 👤 Form 1040 | U.S. Individual Income Tax Return |
| 🏢 Form 1120 | Corporate Tax Return (2016–2025 tax law) |
| 🤝 Form 990 | Nonprofit Return |
| 🏛 Form 1041 | Trust & Estate Return |
| 💼 Schedule C | Sole Proprietorship Profit/Loss |
| 🧮 Tax Estimator | Quarterly estimated payment calculator |
| 🤖 AI Agent Chat | 13 specialized AI agents |
| ✍ AI Humanizer | Make AI text sound natural |
| 📄 Document Creator | 12 document types (dispute letters, petitions, etc.) |
| 📊 Reports | Category and entity financial summaries |

---

## Installation — Method 1: Run the Installer EXE

1. Double-click `HealingTempleLedger_Setup_v1.0.0.exe`
2. Click through the installer wizard
3. A **desktop shortcut** and **Start Menu** entry are created automatically
4. Launch from Desktop or Start Menu

---

## Installation — Method 2: Build from Source

### Prerequisites

1. **Windows 10 or 11 (64-bit)**

2. **.NET 8 SDK**  
   Download: https://dotnet.microsoft.com/download/dotnet/8.0  
   Install the **SDK** (not Runtime-only)

3. **Verify installation:**
   ```
   dotnet --version
   ```
   Should show `8.x.x`

### Build Steps

```cmd
cd HealingTempleLedger
Build.bat
```

That's it. The script will:
- Restore NuGet packages
- Build in Release mode
- Publish a single self-contained `.exe` to `.\publish\`
- Create a Desktop shortcut automatically
- Build the NSIS installer if NSIS is installed

### Manual Build (without Build.bat)

```cmd
cd HealingTempleLedger
dotnet restore HealingTempleLedger\HealingTempleLedger.csproj
dotnet publish HealingTempleLedger\HealingTempleLedger.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -o publish
```

Run the app:
```cmd
publish\HealingTempleLedger.exe
```

---

## Enabling AI Features

The application works fully offline without an API key (ledger, tax forms, document library, historical record). To enable AI-powered features:

1. Open the app → **Settings** (bottom of left sidebar)
2. Choose your AI Provider:
   - **Claude (Anthropic)** — Get free key at https://console.anthropic.com/api-keys
   - **OpenAI (GPT-4)** — Get key at https://platform.openai.com/api-keys
   - **Ollama (Local)** — Install from https://ollama.com, run `ollama pull phi3:mini`
3. Paste your API key and click **Save Settings**

---

## Data Storage

All data is stored locally on your computer:

```
C:\Users\[YourName]\AppData\Roaming\HealingTempleLedger\
  healingtemple.db      ← SQLite database (ledger, tax forms, chat history)
  htl_htl_settings.json        ← Application settings and API key
```

No data is ever sent to any server. The application is fully local-first.

---

## Creating the Desktop Shortcut Manually

If the shortcut was not created automatically:

1. Navigate to the install folder (default: `C:\Program Files\HealingTempleLedger\`)
2. Right-click `HealingTempleLedger.exe` → **Send to** → **Desktop (create shortcut)**
3. Right-click the shortcut → **Properties** → **Change Icon** to use the app icon

---

## Building the Installer (Optional)

1. Install NSIS from https://nsis.sourceforge.io
2. Run: `makensis HealingTempleLedger_Installer.nsi`
3. Output: `HealingTempleLedger_Setup_v1.0.0.exe`

---

## Project Structure

```
HealingTempleLedger/
├── Build.bat                        ← One-click build script
├── HealingTempleLedger_Installer.nsi       ← NSIS installer script
├── HealingTempleLedger/
│   ├── HealingTempleLedger.csproj          ← Project file (.NET 8 WPF)
│   ├── App.xaml / App.xaml.cs       ← Application startup
│   ├── MainWindow.xaml / .cs        ← Navigation shell
│   ├── Themes/Dark.xaml             ← Full dark theme
│   ├── Models/Models.cs             ← All data models
│   ├── Services/
│   │   ├── DatabaseService.cs       ← SQLite persistence
│   │   ├── AIService.cs             ← Claude/OpenAI/Ollama
│   │   ├── ExportService.cs         ← CSV, Excel, text export
│   │   └── SettingsService.cs       ← Settings persistence
│   └── Views/                       ← All 21 pages
│       ├── DashboardPage.*
│       ├── HistoricalRecordPage.*
│       ├── PetitionRedressPage.*
│       ├── CongressionalPetitionPage.*
│       ├── EmergencyPowersPage.*
│       ├── LedgerPage.*
│       ├── JournalPage.*
│       ├── ChartOfAccountsPage.*
│       ├── EntitiesPage.*
│       ├── FinancialStatementsPage.*
│       ├── Form1040Page.*
│       ├── Form1120Page.*
│       ├── Form990Page.*
│       ├── Form1041Page.*
│       ├── ScheduleCPage.*
│       ├── TaxEstimatorPage.*
│       ├── AIAgentPage.*            ← 13 AI agents
│       ├── AIHumanizerPage.*
│       ├── DocumentCreatorPage.*
│       ├── ReportsPage.*
│       └── SettingsPage.*
```

---

## System Requirements

- Windows 10 version 1903+ or Windows 11 (64-bit)
- 200 MB disk space
- 4 GB RAM minimum (8 GB recommended for local AI with Ollama)
- Internet connection required only for Claude/OpenAI AI features
