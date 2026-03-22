using System.IO;
using System.Net.Http;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using HealingTempleLedger.Models;

namespace HealingTempleLedger.Services;

// ── Settings ──────────────────────────────────────────────────────────────────

public class SettingsService
{
    private readonly string _settingsPath;
    public AppSettings Current { get; private set; } = new();

    public SettingsService()
    {
        var dir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "Healing Temple Ledger");
        Directory.CreateDirectory(dir);
        _settingsPath = Path.Combine(dir, "htl_settings.json");
    }

    public void Load()
    {
        try
        {
            if (File.Exists(_settingsPath))
            {
                var json = File.ReadAllText(_settingsPath);
                Current = JsonConvert.DeserializeObject<AppSettings>(json) ?? new();
            }
        }
        catch { Current = new(); }
    }

    public void Save()
    {
        try
        {
            var json = JsonConvert.SerializeObject(Current, Formatting.Indented);
            File.WriteAllText(_settingsPath, json);
        }
        catch { /* swallow */ }
    }
}

// ── AI Service ────────────────────────────────────────────────────────────────

public class AIService
{
    private static readonly HttpClient _http = new() { Timeout = TimeSpan.FromSeconds(120) };

    /// <summary>
    /// Send a message to the configured AI provider and return the response.
    /// Falls back through providers: Claude → OpenAI → Ollama → built-in.
    /// </summary>
    public static async Task<string> ChatAsync(
        string userMessage,
        string systemPrompt,
        List<(string role, string content)>? history = null,
        string? agentType = null)
    {
        var settings = App.Settings.Current;

        try
        {
            return settings.AIProvider switch
            {
                "Claude" when !string.IsNullOrWhiteSpace(settings.ClaudeApiKey)
                    => await CallClaudeAsync(userMessage, systemPrompt, history, settings.ClaudeApiKey),
                "OpenAI" when !string.IsNullOrWhiteSpace(settings.OpenAIApiKey)
                    => await CallOpenAIAsync(userMessage, systemPrompt, history, settings.OpenAIApiKey),
                "Ollama"
                    => await CallOllamaAsync(userMessage, systemPrompt, history, settings.OllamaHost, settings.OllamaModel),
                _   => GetBuiltInResponse(userMessage, agentType)
            };
        }
        catch (Exception ex)
        {
            return $"[AI unavailable: {ex.Message}]\n\nI can still help — please configure your API key in Settings.";
        }
    }

    // ── Claude ────────────────────────────────────────────────────────────────

    private static async Task<string> CallClaudeAsync(
        string userMessage, string system,
        List<(string role, string content)>? history,
        string apiKey)
    {
        var messages = new List<object>();
        if (history != null)
            foreach (var (role, content) in history)
                messages.Add(new { role, content });
        messages.Add(new { role = "user", content = userMessage });

        var body = new
        {
            model = "claude-sonnet-4-20250514",
            max_tokens = 2048,
            system,
            messages
        };

        var req = new HttpRequestMessage(HttpMethod.Post, "https://api.anthropic.com/v1/messages");
        req.Headers.Add("x-api-key", apiKey);
        req.Headers.Add("anthropic-version", "2023-06-01");
        req.Content = new StringContent(JsonConvert.SerializeObject(body), Encoding.UTF8, "application/json");

        var resp = await _http.SendAsync(req);
        resp.EnsureSuccessStatusCode();
        var json = JObject.Parse(await resp.Content.ReadAsStringAsync());
        return json["content"]?[0]?["text"]?.ToString() ?? "[No response]";
    }

    // ── OpenAI ────────────────────────────────────────────────────────────────

    private static async Task<string> CallOpenAIAsync(
        string userMessage, string system,
        List<(string role, string content)>? history,
        string apiKey)
    {
        var messages = new List<object> { new { role = "system", content = system } };
        if (history != null)
            foreach (var (role, content) in history)
                messages.Add(new { role, content });
        messages.Add(new { role = "user", content = userMessage });

        var body = new { model = "gpt-4o-mini", messages, max_tokens = 2048 };

        var req = new HttpRequestMessage(HttpMethod.Post, "https://api.openai.com/v1/chat/completions");
        req.Headers.Add("Authorization", $"Bearer {apiKey}");
        req.Content = new StringContent(JsonConvert.SerializeObject(body), Encoding.UTF8, "application/json");

        var resp = await _http.SendAsync(req);
        resp.EnsureSuccessStatusCode();
        var json = JObject.Parse(await resp.Content.ReadAsStringAsync());
        return json["choices"]?[0]?["message"]?["content"]?.ToString() ?? "[No response]";
    }

    // ── Ollama ────────────────────────────────────────────────────────────────

    private static async Task<string> CallOllamaAsync(
        string userMessage, string system,
        List<(string role, string content)>? history,
        string host, string model)
    {
        var messages = new List<object> { new { role = "system", content = system } };
        if (history != null)
            foreach (var (role, content) in history)
                messages.Add(new { role, content });
        messages.Add(new { role = "user", content = userMessage });

        var body = new { model, messages, stream = false };
        var req = new HttpRequestMessage(HttpMethod.Post, $"{host}/api/chat");
        req.Content = new StringContent(JsonConvert.SerializeObject(body), Encoding.UTF8, "application/json");

        var resp = await _http.SendAsync(req);
        resp.EnsureSuccessStatusCode();
        var json = JObject.Parse(await resp.Content.ReadAsStringAsync());
        return json["message"]?["content"]?.ToString() ?? "[No response]";
    }

    // ── Built-in fallback ─────────────────────────────────────────────────────

    private static string GetBuiltInResponse(string message, string? agentType)
    {
        var msg = message.ToLowerInvariant();

        if (msg.Contains("gaap") || msg.Contains("accounting"))
            return "GAAP (Generally Accepted Accounting Principles) are the standard rules for financial reporting in the U.S. "
                 + "Key principles include: Revenue Recognition (ASC 606), Matching Principle, Materiality, "
                 + "Going Concern, and Full Disclosure. To get AI-powered GAAP explanations, please add your "
                 + "API key in Settings → AI Provider.";

        if (msg.Contains("emergency") || msg.Contains("1933") || msg.Contains("redress"))
            return "Healing Temple Ledger documents the official historical record showing that since March 9, 1933, "
                 + "the United States has operated under declared national emergencies activating hundreds of federal statutes. "
                 + "The National Emergencies Act (1976) did not automatically terminate these powers — termination was conditional. "
                 + "For detailed AI analysis, please configure your API key in Settings.";

        if (msg.Contains("dispute") || msg.Contains("refund") || msg.Contains("consumer"))
            return "For consumer disputes: (1) Document everything in writing, (2) Contact the merchant formally, "
                 + "(3) File a chargeback with your bank if within 60-120 days, (4) File with the FTC or CFPB, "
                 + "(5) Consider small claims court. For AI-drafted dispute letters, configure your API key in Settings.";

        return "I'm operating in offline mode. To enable full AI capabilities:\n\n"
             + "1. Go to Settings\n"
             + "2. Choose your AI provider (Claude, OpenAI, or Ollama)\n"
             + "3. Enter your API key\n\n"
             + "All features of this application — GAAPCLAW ledger, tax forms, legal documents — work without AI. "
             + "AI enhances them with explanations, drafting, and analysis.";
    }
}
