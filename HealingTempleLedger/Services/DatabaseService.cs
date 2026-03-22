using System.IO;
using Microsoft.Data.Sqlite;
using Newtonsoft.Json;
using HealingTempleLedger.Models;

namespace HealingTempleLedger.Services;

public class DatabaseService
{
    private readonly string _dbPath;
    private readonly string _connectionString;

    public DatabaseService()
    {
        var appData = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "Healing Temple Ledger");
        Directory.CreateDirectory(appData);
        _dbPath = Path.Combine(appData, "healingtemple.db");
        _connectionString = $"Data Source={_dbPath}";
    }

    private SqliteConnection OpenConnection()
    {
        var conn = new SqliteConnection(_connectionString);
        conn.Open();
        conn.Execute("PRAGMA journal_mode=WAL;");
        conn.Execute("PRAGMA foreign_keys=ON;");
        return conn;
    }

    public void Initialize()
    {
        using var conn = OpenConnection();
        conn.Execute(@"
            CREATE TABLE IF NOT EXISTS ledger_entries (
                id INTEGER PRIMARY KEY AUTOINCREMENT,
                date TEXT NOT NULL,
                description TEXT NOT NULL,
                category TEXT NOT NULL,
                debit REAL DEFAULT 0,
                credit REAL DEFAULT 0,
                entity_id TEXT DEFAULT '',
                account_code TEXT DEFAULT '',
                reference TEXT DEFAULT '',
                reconciled INTEGER DEFAULT 0,
                created_at TEXT DEFAULT (datetime('now'))
            );

            CREATE TABLE IF NOT EXISTS journal_entries (
                id INTEGER PRIMARY KEY AUTOINCREMENT,
                date TEXT NOT NULL,
                reference TEXT NOT NULL,
                memo TEXT,
                created_at TEXT DEFAULT (datetime('now'))
            );

            CREATE TABLE IF NOT EXISTS journal_lines (
                id INTEGER PRIMARY KEY AUTOINCREMENT,
                journal_entry_id INTEGER NOT NULL,
                account_code TEXT NOT NULL,
                account_name TEXT NOT NULL,
                debit REAL DEFAULT 0,
                credit REAL DEFAULT 0,
                description TEXT,
                FOREIGN KEY (journal_entry_id) REFERENCES journal_entries(id) ON DELETE CASCADE
            );

            CREATE TABLE IF NOT EXISTS accounts (
                id INTEGER PRIMARY KEY AUTOINCREMENT,
                code TEXT NOT NULL,
                name TEXT NOT NULL,
                type TEXT NOT NULL,
                sub_type TEXT DEFAULT '',
                is_active INTEGER DEFAULT 1,
                entity_id TEXT DEFAULT ''
            );

            CREATE TABLE IF NOT EXISTS entities (
                id TEXT PRIMARY KEY,
                name TEXT NOT NULL,
                type TEXT NOT NULL,
                ein TEXT DEFAULT '',
                address TEXT DEFAULT '',
                state TEXT DEFAULT '',
                fiscal_year_end TEXT DEFAULT '12/31',
                is_parent INTEGER DEFAULT 0,
                parent_id TEXT DEFAULT '',
                created_at TEXT DEFAULT (datetime('now'))
            );

            CREATE TABLE IF NOT EXISTS tax_forms_1040 (
                id INTEGER PRIMARY KEY AUTOINCREMENT,
                tax_year INTEGER NOT NULL,
                data_json TEXT NOT NULL,
                last_saved TEXT DEFAULT (datetime('now'))
            );

            CREATE TABLE IF NOT EXISTS tax_forms_1120 (
                id INTEGER PRIMARY KEY AUTOINCREMENT,
                tax_year INTEGER NOT NULL,
                data_json TEXT NOT NULL,
                last_saved TEXT DEFAULT (datetime('now'))
            );

            CREATE TABLE IF NOT EXISTS tax_forms_990 (
                id INTEGER PRIMARY KEY AUTOINCREMENT,
                tax_year INTEGER NOT NULL,
                data_json TEXT NOT NULL,
                last_saved TEXT DEFAULT (datetime('now'))
            );

            CREATE TABLE IF NOT EXISTS tax_forms_1041 (
                id INTEGER PRIMARY KEY AUTOINCREMENT,
                tax_year INTEGER NOT NULL,
                data_json TEXT NOT NULL,
                last_saved TEXT DEFAULT (datetime('now'))
            );

            CREATE TABLE IF NOT EXISTS schedule_c (
                id INTEGER PRIMARY KEY AUTOINCREMENT,
                tax_year INTEGER NOT NULL,
                data_json TEXT NOT NULL,
                last_saved TEXT DEFAULT (datetime('now'))
            );

            CREATE TABLE IF NOT EXISTS chat_messages (
                id INTEGER PRIMARY KEY AUTOINCREMENT,
                session_id TEXT NOT NULL,
                role TEXT NOT NULL,
                content TEXT NOT NULL,
                timestamp TEXT DEFAULT (datetime('now')),
                agent_type TEXT DEFAULT 'general'
            );

            CREATE TABLE IF NOT EXISTS app_settings (
                key TEXT PRIMARY KEY,
                value TEXT NOT NULL
            );
        ");

        // Seed default chart of accounts if empty
        var count = (long)(conn.ExecuteScalar("SELECT COUNT(*) FROM accounts") ?? 0L);
        if (count == 0) SeedDefaultAccounts(conn);
    }

    private void SeedDefaultAccounts(SqliteConnection conn)
    {
        var defaults = new[]
        {
            ("1000","Cash and Cash Equivalents","Asset","Current Asset"),
            ("1100","Accounts Receivable","Asset","Current Asset"),
            ("1200","Inventory","Asset","Current Asset"),
            ("1300","Prepaid Expenses","Asset","Current Asset"),
            ("1500","Property, Plant & Equipment","Asset","Fixed Asset"),
            ("1600","Accumulated Depreciation","Asset","Contra Asset"),
            ("2000","Accounts Payable","Liability","Current Liability"),
            ("2100","Accrued Liabilities","Liability","Current Liability"),
            ("2200","Notes Payable (Short-Term)","Liability","Current Liability"),
            ("2500","Long-Term Debt","Liability","Long-Term Liability"),
            ("2600","Deferred Revenue","Liability","Current Liability"),
            ("3000","Common Stock","Equity","Paid-In Capital"),
            ("3100","Retained Earnings","Equity","Retained Earnings"),
            ("3200","Additional Paid-In Capital","Equity","Paid-In Capital"),
            ("4000","Revenue","Revenue","Operating Revenue"),
            ("4100","Service Revenue","Revenue","Operating Revenue"),
            ("4200","Product Sales","Revenue","Operating Revenue"),
            ("4300","Other Revenue","Revenue","Other Revenue"),
            ("5000","Cost of Goods Sold","Expense","Cost of Sales"),
            ("6000","Salaries & Wages","Expense","Operating Expense"),
            ("6100","Rent Expense","Expense","Operating Expense"),
            ("6200","Utilities","Expense","Operating Expense"),
            ("6300","Office Supplies","Expense","Operating Expense"),
            ("6400","Insurance","Expense","Operating Expense"),
            ("6500","Depreciation Expense","Expense","Operating Expense"),
            ("6600","Interest Expense","Expense","Financial Expense"),
            ("6700","Professional Fees","Expense","Operating Expense"),
            ("6800","Marketing & Advertising","Expense","Operating Expense"),
            ("6900","Other Expenses","Expense","Operating Expense"),
            ("7000","Income Tax Expense","Expense","Tax Expense"),
        };

        foreach (var (code, name, type, sub) in defaults)
        {
            conn.Execute(
                "INSERT INTO accounts (code, name, type, sub_type) VALUES (@c,@n,@t,@s)",
                new { c = code, n = name, t = type, s = sub });
        }
    }

    // ── Ledger Entries ────────────────────────────────────────────────────────

    public List<LedgerEntry> GetLedgerEntries(string? entityId = null)
    {
        using var conn = OpenConnection();
        var sql = entityId == null
            ? "SELECT * FROM ledger_entries ORDER BY date DESC, id DESC"
            : "SELECT * FROM ledger_entries WHERE entity_id=@e ORDER BY date DESC, id DESC";
        using var cmd = conn.CreateCommand();
        cmd.CommandText = sql;
        if (entityId != null) cmd.Parameters.AddWithValue("@e", entityId);
        var results = new List<LedgerEntry>();
        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            results.Add(new LedgerEntry
            {
                Id = reader.GetInt32(0),
                Date = DateTime.Parse(reader.GetString(1)),
                Description = reader.GetString(2),
                Category = reader.GetString(3),
                Debit = (decimal)reader.GetDouble(4),
                Credit = (decimal)reader.GetDouble(5),
                EntityId = reader.GetString(6),
                AccountCode = reader.GetString(7),
                Reference = reader.GetString(8),
                Reconciled = reader.GetInt32(9) == 1,
            });
        }
        return results;
    }

    public int SaveLedgerEntry(LedgerEntry e)
    {
        using var conn = OpenConnection();
        if (e.Id == 0)
        {
            conn.Execute(@"INSERT INTO ledger_entries
                (date,description,category,debit,credit,entity_id,account_code,reference,reconciled)
                VALUES (@d,@ds,@cat,@db,@cr,@eid,@ac,@ref,@rec)",
                new { d = e.Date.ToString("yyyy-MM-dd"), ds = e.Description, cat = e.Category,
                      db = (double)e.Debit, cr = (double)e.Credit, eid = e.EntityId,
                      ac = e.AccountCode, ref_ = e.Reference, rec = e.Reconciled ? 1 : 0 });
            return (int)(long)(conn.ExecuteScalar("SELECT last_insert_rowid()") ?? 0);
        }
        else
        {
            conn.Execute(@"UPDATE ledger_entries SET date=@d,description=@ds,category=@cat,
                debit=@db,credit=@cr,entity_id=@eid,account_code=@ac,reference=@ref,reconciled=@rec
                WHERE id=@id",
                new { d = e.Date.ToString("yyyy-MM-dd"), ds = e.Description, cat = e.Category,
                      db = (double)e.Debit, cr = (double)e.Credit, eid = e.EntityId,
                      ac = e.AccountCode, @ref = e.Reference, rec = e.Reconciled ? 1 : 0, id = e.Id });
            return e.Id;
        }
    }

    public void DeleteLedgerEntry(int id)
    {
        using var conn = OpenConnection();
        conn.Execute("DELETE FROM ledger_entries WHERE id=@id", new { id });
    }

    // ── Accounts ──────────────────────────────────────────────────────────────

    public List<Account> GetAccounts()
    {
        using var conn = OpenConnection();
        var results = new List<Account>();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT * FROM accounts WHERE is_active=1 ORDER BY code";
        using var r = cmd.ExecuteReader();
        while (r.Read())
            results.Add(new Account { Id=r.GetInt32(0), Code=r.GetString(1), Name=r.GetString(2),
                Type=r.GetString(3), SubType=r.GetString(4), IsActive=r.GetInt32(5)==1, EntityId=r.GetString(6) });
        return results;
    }

    public void SaveAccount(Account a)
    {
        using var conn = OpenConnection();
        if (a.Id == 0)
            conn.Execute("INSERT INTO accounts (code,name,type,sub_type,entity_id) VALUES (@c,@n,@t,@s,@e)",
                new { c=a.Code, n=a.Name, t=a.Type, s=a.SubType, e=a.EntityId });
        else
            conn.Execute("UPDATE accounts SET code=@c,name=@n,type=@t,sub_type=@s WHERE id=@id",
                new { c=a.Code, n=a.Name, t=a.Type, s=a.SubType, id=a.Id });
    }

    // ── Entities ──────────────────────────────────────────────────────────────

    public List<Entity> GetEntities()
    {
        using var conn = OpenConnection();
        var results = new List<Entity>();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT * FROM entities ORDER BY name";
        using var r = cmd.ExecuteReader();
        while (r.Read())
            results.Add(new Entity { Id=r.GetString(0), Name=r.GetString(1), Type=r.GetString(2),
                EIN=r.GetString(3), Address=r.GetString(4), State=r.GetString(5),
                FiscalYearEnd=r.GetString(6), IsParent=r.GetInt32(7)==1, ParentId=r.GetString(8) });
        return results;
    }

    public void SaveEntity(Entity e)
    {
        using var conn = OpenConnection();
        conn.Execute(@"INSERT OR REPLACE INTO entities
            (id,name,type,ein,address,state,fiscal_year_end,is_parent,parent_id)
            VALUES (@id,@n,@t,@ein,@addr,@st,@fy,@ip,@pid)",
            new { id=e.Id, n=e.Name, t=e.Type, ein=e.EIN, addr=e.Address,
                  st=e.State, fy=e.FiscalYearEnd, ip=e.IsParent?1:0, pid=e.ParentId });
    }

    public void DeleteEntity(string id)
    {
        using var conn = OpenConnection();
        conn.Execute("DELETE FROM entities WHERE id=@id", new { id });
    }

    // ── Tax Forms ─────────────────────────────────────────────────────────────

    public void SaveTaxForm<T>(string table, int taxYear, T form)
    {
        using var conn = OpenConnection();
        var json = JsonConvert.SerializeObject(form);
        var existing = conn.ExecuteScalar($"SELECT id FROM {table} WHERE tax_year=@y", new { y = taxYear });
        if (existing == null)
            conn.Execute($"INSERT INTO {table} (tax_year, data_json) VALUES (@y, @j)", new { y = taxYear, j = json });
        else
            conn.Execute($"UPDATE {table} SET data_json=@j, last_saved=datetime('now') WHERE tax_year=@y", new { j = json, y = taxYear });
    }

    public T? LoadTaxForm<T>(string table, int taxYear)
    {
        using var conn = OpenConnection();
        var json = conn.ExecuteScalar($"SELECT data_json FROM {table} WHERE tax_year=@y ORDER BY id DESC LIMIT 1", new { y = taxYear }) as string;
        return json == null ? default : JsonConvert.DeserializeObject<T>(json);
    }

    public List<int> GetSavedTaxYears(string table)
    {
        using var conn = OpenConnection();
        var results = new List<int>();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = $"SELECT DISTINCT tax_year FROM {table} ORDER BY tax_year DESC";
        using var r = cmd.ExecuteReader();
        while (r.Read()) results.Add(r.GetInt32(0));
        return results;
    }

    // ── Chat ──────────────────────────────────────────────────────────────────

    public void SaveChatMessage(ChatMessage m)
    {
        using var conn = OpenConnection();
        conn.Execute("INSERT INTO chat_messages (session_id,role,content,agent_type) VALUES (@s,@r,@c,@a)",
            new { s=m.SessionId, r=m.Role, c=m.Content, a=m.AgentType });
    }

    public List<ChatMessage> GetChatHistory(string sessionId, int limit = 50)
    {
        using var conn = OpenConnection();
        var results = new List<ChatMessage>();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT * FROM chat_messages WHERE session_id=@s ORDER BY id DESC LIMIT @l";
        cmd.Parameters.AddWithValue("@s", sessionId);
        cmd.Parameters.AddWithValue("@l", limit);
        using var r = cmd.ExecuteReader();
        while (r.Read())
            results.Add(new ChatMessage { Id=r.GetInt32(0), SessionId=r.GetString(1),
                Role=r.GetString(2), Content=r.GetString(3),
                Timestamp=DateTime.Parse(r.GetString(4)), AgentType=r.GetString(5) });
        results.Reverse();
        return results;
    }

    // ── Settings ──────────────────────────────────────────────────────────────

    public void SetSetting(string key, string value)
    {
        using var conn = OpenConnection();
        conn.Execute("INSERT OR REPLACE INTO app_settings (key, value) VALUES (@k, @v)", new { k = key, v = value });
    }

    public string? GetSetting(string key)
    {
        using var conn = OpenConnection();
        return conn.ExecuteScalar("SELECT value FROM app_settings WHERE key=@k", new { k = key }) as string;
    }
}

// ── Micro-ORM helpers ─────────────────────────────────────────────────────────
internal static class SqliteExtensions
{
    public static int Execute(this SqliteConnection conn, string sql, object? param = null)
    {
        using var cmd = BuildCommand(conn, sql, param);
        return cmd.ExecuteNonQuery();
    }

    public static object? ExecuteScalar(this SqliteConnection conn, string sql, object? param = null)
    {
        using var cmd = BuildCommand(conn, sql, param);
        return cmd.ExecuteScalar();
    }

    private static SqliteCommand BuildCommand(SqliteConnection conn, string sql, object? param)
    {
        var cmd = conn.CreateCommand();
        cmd.CommandText = sql;
        if (param != null)
            foreach (var prop in param.GetType().GetProperties())
            {
                var name = prop.Name == "ref_" ? "@ref" : $"@{prop.Name}";
                cmd.Parameters.AddWithValue(name, prop.GetValue(param) ?? DBNull.Value);
            }
        return cmd;
    }
}
