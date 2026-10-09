using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.GameOptions.OptionTypes;
using MiraAPI.Utilities;
using JAM.Roles.Crewmate;
using JAM.Roles.Neutral;
using TownOfUs.Extensions;
using TownOfUs.Modules.Localization;

namespace JAM.Options.Roles.Neutral;

public sealed class BountyHunterOptions : AbstractOptionGroup<BountyHunterRole>
{
    public override string GroupName => "Bounty Hunter Options";

    [ModdedNumberOption("Bounties To Win", 1f, 5f, 1f)]
    public float BountiesToWin { get; set; } = 3;

    [ModdedToggleOption("Arrow To Bounty Target")]
    public bool TargetArrow { get; set; } = true;

    [ModdedNumberOption("Hunted Player Grace Period", 0f, 30f, 5f, MiraNumberSuffixes.Seconds)]
    public float HuntedGracePeriod { get; set; } = 15;

    //Reward Stuff
    [ModdedNumberOption("Faction Modifier Weight", 0f, 100f, 5f, MiraNumberSuffixes.Percent)]
    public float RandFactMod { get; set; } = 10f;
    [ModdedNumberOption("Universal Modifier Weight", 0f, 100f, 5f, MiraNumberSuffixes.Percent)]
    public float RandUnivMod { get; set; } = 10f;
    /*
    [ModdedNumberOption("Lower Cooldowns Weight", 0f, 100f, 5f, MiraNumberSuffixes.Percent)]
    public float lowerCooldowns { get; set; } = 10f;
    */
    [ModdedNumberOption("Allow Venting Weight", 0f, 100f, 5f, MiraNumberSuffixes.Percent)]
    public float GiveVentable { get; set; } = 10f;
    [ModdedNumberOption("Extra Vote Weight", 0f, 100f, 5f, MiraNumberSuffixes.Percent)]
    public float GiveExtraVote { get; set; } = 10f;
    [ModdedNumberOption("Reveal Role Weight (Crew Only)", 0f, 100f, 5f, MiraNumberSuffixes.Percent)]
    public float RevealCKRole { get; set; } = 10f;
    [ModdedNumberOption("Double Shot Weight", 0f, 100f, 5f, MiraNumberSuffixes.Percent)]
    public float GiveDblShot { get; set; } = 10f;
    [ModdedNumberOption("Temporary Shield Weight", 0f, 100f, 5f, MiraNumberSuffixes.Percent)]
    public float ShieldNextRound { get; set; } = 10f;

    //Hunting Period
    [ModdedToggleOption("Hunting Time Is Limited")]
    public bool LimitedHuntingTime { get; set; } = false;

    [ModdedNumberOption("Bounty Hunting Timeframe", 30f, 120f, 5f, MiraNumberSuffixes.Seconds)]
    public float BountyTimeframe { get; set; } = 30;
}
