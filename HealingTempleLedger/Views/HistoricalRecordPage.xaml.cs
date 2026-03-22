using System.Windows;
using System.Windows.Controls;
using HealingTempleLedger.Services;

namespace HealingTempleLedger.Views;

public partial class HistoricalRecordPage : Page
{
    public HistoricalRecordPage()
    {
        InitializeComponent();
        Loaded += (_, _) => LoadContent();
    }

    private void LoadContent()
    {
        TimelineList.ItemsSource = new[]
        {
            new { Year = "1933", Title = "Emergency Banking Act — Proclamation 2039",
                  Proclamation = "Proclamation 2039 · March 9, 1933",
                  Description = "President Franklin D. Roosevelt declared a national emergency and amended the Trading with the Enemy Act (originally aimed at foreign enemies) to apply domestically. This began the activation of extraordinary statutory powers over all U.S. citizens and property." },
            new { Year = "1950", Title = "Korean Conflict — Proclamation 2914",
                  Proclamation = "Proclamation 2914 · December 16, 1950",
                  Description = "President Harry S. Truman declared a national emergency during the Korean conflict, further expanding the scope of executive powers available under existing statutes. This emergency layered additional authority on top of the 1933 framework." },
            new { Year = "1970", Title = "Postal Strike Emergency",
                  Proclamation = "Proclamation 3972 · March 27, 1970",
                  Description = "President Richard Nixon declared a national emergency in response to the U.S. Postal Service strike, utilizing the broad authority accumulated over decades of continuous emergency governance." },
            new { Year = "1971", Title = "International Monetary Crisis",
                  Proclamation = "Proclamation 4074 · August 15, 1971",
                  Description = "President Nixon declared a second emergency responding to the international monetary and balance-of-payments crisis. This emergency suspended the convertibility of the dollar to gold, fundamentally altering the U.S. monetary system." },
            new { Year = "1973", Title = "Senate Special Committee Investigation",
                  Proclamation = "Senate Report 93-549 · 1973",
                  Description = "Congress formed a Special Committee on the Termination of the National Emergency. The committee found that 470 statutes were activated, and documented that these powers confer enough authority to 'rule the country without reference to normal constitutional processes.' Despite finding no present necessity, Congress preserved the framework." },
            new { Year = "1976", Title = "National Emergencies Act",
                  Proclamation = "90 Stat. 1255 · September 14, 1976",
                  Description = "Congress passed the National Emergencies Act. Critically: it did NOT revoke the 1933, 1950, 1970, or 1971 proclamations. It preserved all prior emergency actions under its Savings Clause (§502(a)). Termination was conditional on presidential affirmative action — not automatic. The emergency regime was regularized, not dismantled." },
            new { Year = "1976+", Title = "Continuous Emergency Governance",
                  Proclamation = "Present Day",
                  Description = "Subsequent presidents have issued new national emergency declarations and renewed existing ones. The administrative and statutory framework enabled by emergency powers has continued operating. The condition identified by Congress in 1973 — governance enabled without reference to normal constitutional processes — was never fully dismantled." },
        };

        PowersList.ItemsSource = new[]
        {
            "Seize property and organize and control the means of production",
            "Seize commodities and assign military forces abroad",
            "Institute martial law and seize and control all transportation",
            "Seize and control all communications (including internet infrastructure)",
            "Regulate all private enterprise and restrict travel",
            "Control the lives of American citizens in numerous specific ways",
            "All powers ordinarily exercised by Congress — delegated to the Executive",
        };

        NEAList.ItemsSource = new[]
        {
            new { Title = "I. The NEA Did NOT Return All Emergency Powers to Dormancy",
                  Content = "The statute conditions termination on affirmative presidential action and Congressional notice. It does not self-terminate emergency powers. Termination was conditional, not automatic.",
                  Citation = "National Emergencies Act, Title I, §101(a), 90 Stat. 1255 (1976)" },
            new { Title = "II. The Act Explicitly Preserved Prior Emergency Actions",
                  Content = "The Savings Clause states: 'The termination of a national emergency shall not affect any action taken or proceeding pending not finally concluded or determined on the date of termination.' All actions, structures, delegations, and legal consequences remained valid.",
                  Citation = "NEA §502(a), 90 Stat. 1265 — Savings Clause (Controlling)" },
            new { Title = "III. The Emergency Proclamations Were NOT Rendered Ineffective",
                  Content = "The proclamations were never revoked. The statutory delegations they activated were never repealed. The Act preserved all effects and consequences. Congress acknowledged the regime and left it intact.",
                  Citation = "Controlling statutory text confirmed in 90 Stat. 1255–1265" },
            new { Title = "IV. Congressional Admissions Contradict Claims of Dormancy",
                  Content = "The Senate Special Committee expressly stated: 'This vast range of powers... confer enough authority to rule the country without reference to normal constitutional processes.' Despite this, Congress preserved the regime and did not restore constitutional governance.",
                  Citation = "Senate Report 93-549, Special Committee on National Emergencies (1973)" },
            new { Title = "V. Statement of Criminal Conspiracy — Elements Documented",
                  Content = "Based on the official record: (1) A combination of Congress and the Executive agreed to operate outside constitutional processes; (2) The objective was governance without normal constitutional constraints — Congress's own description; (3) Congress had actual knowledge via formal committee findings; (4) Overt acts include 470 statutes activated, Trading with the Enemy Act amended domestically, emergency powers preserved after knowledge.",
                  Citation = "18 U.S.C. §371 — Conspiracy framework applied to documented record" },
        };
    }

    private void Export_Click(object s, RoutedEventArgs e)
    {
        var content = @"REDRESSRIGHT — HISTORICAL RECORD OF U.S. EMERGENCY POWERS
================================================================

SINCE MARCH 9, 1933, the United States has operated under declared national emergencies
activating hundreds of federal statutes granting extraordinary powers.

KEY PROCLAMATIONS:
- Proclamation 2039 (1933): Emergency Banking Act — Trading with the Enemy Act applied domestically
- Proclamation 2914 (1950): Korean Conflict — further executive power expansion
- Proclamation 3972 (1970): Postal Strike Emergency
- Proclamation 4074 (1971): International Monetary Crisis — dollar convertibility suspended

KEY STATUTORY TEXT:
National Emergencies Act, Title I, §101(a), 90 Stat. 1255 (1976):
Termination was CONDITIONAL — not automatic. Required affirmative presidential action.

NEA §502(a), 90 Stat. 1265 (Savings Clause):
'The termination of a national emergency shall not affect any action taken...'
All prior emergency actions, delegations, and legal effects were PRESERVED.

CONGRESSIONAL FINDING (Senate Report 93-549, 1973):
'This vast range of powers, taken together, confer enough authority to rule the country
without reference to normal constitutional processes.'

Despite finding NO present necessity, Congress preserved the framework.

Source: redressright.me — Official Historical Record
Est. 1933 — Present
";
        ExportService.ExportTextReport("HealingTempleLedger_Historical_Record", content);
    }
}
