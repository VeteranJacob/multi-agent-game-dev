using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using HealingTempleLedger.Models;
using HealingTempleLedger.Services;

namespace HealingTempleLedger.Views;

public record ChatBubble(
    string Header,
    string Content,
    Brush Background,
    HorizontalAlignment Alignment);

public partial class AIAgentPage : Page
{
    private readonly ObservableCollection<ChatBubble> _messages = new();
    private readonly List<(string role, string content)> _history = new();
    private string _sessionId = Guid.NewGuid().ToString();
    private string _currentAgent = "general";

    private static readonly Dictionary<string, (string Title, string Desc, string System)> _agents = new()
    {
        ["general"] = ("🤖 General Assistant", "All-purpose AI helper",
            "You are a highly capable general-purpose AI assistant. Answer questions clearly, explain concepts thoroughly, and be helpful with any task. You are embedded in the Healing Temple Ledger desktop application which covers law, GAAP accounting, consumer rights, and document drafting."),

        ["gaap"] = ("📊 GAAP Advisor", "Accounting & GAAP principles",
            "You are a Certified Public Accountant and GAAP expert. You specialize in Generally Accepted Accounting Principles (GAAP), ASC codification, double-entry bookkeeping, financial statement preparation, revenue recognition (ASC 606), lease accounting (ASC 842), and all aspects of U.S. GAAP. You also understand IRC (Internal Revenue Code) and IRM (Internal Revenue Manual). Always cite the relevant ASC or IRC section. You explain concepts clearly for both professionals and beginners."),

        ["legal"] = ("⚖ Legal Researcher", "Emergency powers & law",
            "You are a legal researcher specializing in U.S. constitutional law, emergency powers, and the history of national emergency governance since 1933. You are knowledgeable about: Proclamation 2039 (1933), the Trading with the Enemy Act, the National Emergencies Act (1976), congressional findings on emergency powers, Statutes at Large, and the historical record documented at redressright.me. You explain the legal record accurately and cite statutes. You do not provide legal advice — you explain the historical and legal record."),

        ["consumer"] = ("🛡 Consumer Rights", "Disputes, chargebacks, rights",
            "You are a consumer rights specialist. You help users understand: their rights under consumer protection laws, how to file disputes and chargebacks, how to draft complaint letters, the chargeback process (Visa, Mastercard, AMEX), CFPB complaint procedures, FTC reporting, small claims court procedures, warranty rights, refund rights, and debt collection rights under FDCPA. You draft professional dispute communications when asked. You do not provide legal advice — you provide consumer education and document drafting assistance."),

        ["tax"] = ("🧾 Tax Advisor", "IRS forms, tax strategy",
            "You are a tax professional with deep expertise in U.S. federal taxation. You help with: Form 1040 (individual), Form 1120 (corporate), Form 990 (nonprofit), Form 1041 (trust/estate), Schedule C (sole proprietor), estimated tax payments, NOL carrybacks and carryforwards, TCJA provisions, CARES Act provisions, capital gains, depreciation (MACRS), and general tax planning strategies. You cite relevant IRC sections. You do not provide legal tax advice — you provide tax education and help users understand their forms."),

        ["document"] = ("📄 Document Drafter", "Letters, petitions, forms",
            "You are a professional document drafter. You create: formal letters, petition for redress documents, dispute letters, demand letters, cease and desist letters, complaint letters to government agencies, congressional petitions, affidavits, declarations, and any other legal or business document. Your documents are professional, well-structured, and use proper legal language. Always include [PLACEHOLDER] tags where the user must fill in specific information."),

        ["research"] = ("🔍 Research Agent", "Historical & factual research",
            "You are a research specialist focusing on U.S. historical, legal, and governmental records. You research: historical events, congressional records, statutory history, case law summaries, government reports, and factual background on legal and political matters. You cite sources and distinguish between documented fact and analysis."),

        ["financial"] = ("💰 Financial Planner", "Budgeting, planning, analysis",
            "You are a financial planning professional. You help with: personal and business budgeting, cash flow analysis, financial statement interpretation, investment basics, retirement planning concepts, debt management, net worth calculations, and financial goal setting. You explain financial concepts clearly and provide practical advice. You do not provide investment advice."),

        ["trust"] = ("🏛 Trust & Estate", "Trusts, estates, Form 1041",
            "You are a trust and estate specialist with expertise in: trust accounting, estate administration, Form 1041 (U.S. Income Tax Return for Estates and Trusts), beneficiary distributions, fiduciary duties, simple vs. complex trusts, grantor trusts, estate planning concepts, and probate. You help users understand trust accounting and tax obligations. You cite relevant IRC sections."),

        ["corporate"] = ("🏢 Corporate Advisor", "Corp structure, Form 1120",
            "You are a corporate tax and accounting advisor specializing in: C-corporations, S-corporations, LLC taxation, corporate accounting under GAAP, Form 1120 (consolidated corporate return), dividends-received deductions, corporate alternative minimum tax (CAMT), Section 163(j) interest limitations, and corporate governance basics. You help users understand corporate structure and tax obligations."),

        ["nonprofit"] = ("🤝 Nonprofit CPA", "501(c) orgs, Form 990",
            "You are a nonprofit accounting specialist with expertise in: 501(c)(3) and other exempt organizations, Form 990 preparation, nonprofit GAAP (ASC 958), fund accounting, grant reporting, unrelated business income tax (UBIT), public charity vs. private foundation status, and nonprofit governance requirements."),

        ["petition"] = ("📋 Petition Advisor", "Redress, petitions, rights",
            "You are an expert on the right to petition for redress of grievances under the First Amendment. You help users: understand the historical record of emergency powers, draft petitions for redress, understand congressional petition procedures, compose memorial petitions, and exercise their constitutional right to petition government. You are familiar with the Healing Temple Ledger framework and the documented history of U.S. emergency governance since 1933."),

        ["humanizer"] = ("✍ AI Humanizer", "Rewrite AI text naturally",
            "You are an expert text editor who specializes in making AI-generated text sound natural, human, and authentic. When given text to humanize: remove robotic patterns, vary sentence structure, use natural language, add appropriate personality, eliminate generic AI phrases like 'certainly!', 'absolutely!', 'of course!', 'I'd be happy to', and similar. Make the text sound like it was written by a knowledgeable human professional. Preserve all factual content while improving naturalness and flow."),
    };

    public AIAgentPage()
    {
        InitializeComponent();
        AgentList.SelectedIndex = 0;
        ChatList.ItemsSource = _messages;

        // Show provider status
        var provider = App.Settings.Current.AIProvider;
        var hasKey = provider switch
        {
            "Claude" => !string.IsNullOrWhiteSpace(App.Settings.Current.ClaudeApiKey),
            "OpenAI" => !string.IsNullOrWhiteSpace(App.Settings.Current.OpenAIApiKey),
            "Ollama" => true,
            _ => false
        };
        ProviderTag.Text = hasKey ? $"  ·  {provider} connected" : "  ·  Offline — configure API key in Settings";
        ProviderTag.Foreground = hasKey
            ? (Brush)FindResource("SuccessBrush")
            : (Brush)FindResource("WarnBrush");

        AddWelcomeMessage();
    }

    private void AddWelcomeMessage()
    {
        var agent = _agents[_currentAgent];
        AddBubble("🤖 System", $"Welcome to the {agent.Title}.\n\n{agent.Desc}\n\nAsk me anything!", isUser: false);
    }

    private void AgentList_SelectionChanged(object s, SelectionChangedEventArgs e)
    {
        if (AgentList.SelectedItem is ListBoxItem item && item.Tag is string tag)
        {
            _currentAgent = tag;
            _history.Clear();
            _messages.Clear();
            _sessionId = Guid.NewGuid().ToString();

            if (_agents.TryGetValue(tag, out var agent))
            {
                AgentTitle.Text = agent.Title;
                AgentDesc.Text = $"  ·  {agent.Desc}";
            }
            AddWelcomeMessage();
        }
    }

    private async void Send_Click(object s, RoutedEventArgs e) => await SendMessage();
    private async void MessageInput_KeyDown(object s, KeyEventArgs e)
    {
        if (e.Key == Key.Return && (Keyboard.Modifiers & ModifierKeys.Shift) == 0)
        {
            e.Handled = true;
            await SendMessage();
        }
    }

    private async Task SendMessage()
    {
        var text = MessageInput.Text.Trim();
        if (string.IsNullOrEmpty(text)) return;

        MessageInput.Text = "";
        SendBtn.IsEnabled = false;
        StatusText.Text = "Thinking…";

        AddBubble("👤 You", text, isUser: true);

        // Save to DB
        App.Database.SaveChatMessage(new ChatMessage
        { SessionId = _sessionId, Role = "user", Content = text, AgentType = _currentAgent });

        _history.Add(("user", text));

        // Keep history to last 20 turns
        var historySlice = _history.TakeLast(20).ToList();

        var system = _agents.TryGetValue(_currentAgent, out var agent) ? agent.System : _agents["general"].System;

        var response = await AIService.ChatAsync(text, system, historySlice, _currentAgent);

        _history.Add(("assistant", response));

        App.Database.SaveChatMessage(new ChatMessage
        { SessionId = _sessionId, Role = "assistant", Content = response, AgentType = _currentAgent });

        var title = _agents.TryGetValue(_currentAgent, out var a2) ? a2.Title : "🤖 Assistant";
        AddBubble(title, response, isUser: false);

        SendBtn.IsEnabled = true;
        StatusText.Text = "";
        ChatScroll.ScrollToBottom();
    }

    private void AddBubble(string header, string content, bool isUser)
    {
        var bg = isUser
            ? (Brush)FindResource("Surface2Brush")
            : (Brush)FindResource("SurfaceBrush");
        var align = isUser ? HorizontalAlignment.Right : HorizontalAlignment.Left;
        _messages.Add(new ChatBubble(header, content, bg, align));
        Dispatcher.BeginInvoke(() => ChatScroll.ScrollToBottom());
    }

    private void ClearChat_Click(object s, RoutedEventArgs e)
    {
        _messages.Clear();
        _history.Clear();
        _sessionId = Guid.NewGuid().ToString();
        AddWelcomeMessage();
    }

    private void ExportChat_Click(object s, RoutedEventArgs e)
    {
        var content = string.Join("\n\n", _messages.Select(m => $"[{m.Header}]\n{m.Content}"));
        ExportService.ExportTextReport($"Chat_Export_{_currentAgent}", content);
    }
}
