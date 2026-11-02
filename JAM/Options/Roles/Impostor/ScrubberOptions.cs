using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.GameOptions.OptionTypes;
using MiraAPI.Utilities;
using JAM.Roles.Crewmate;
using TownOfUs.Extensions;
using TownOfUs.Modules.Localization;
using JAM.Roles.Impostor;

namespace JAM.Options.Roles.Impostor;

public sealed class ScrubberOptions : AbstractOptionGroup<ScrubberRole>
{
    public override string GroupName => "Cleanser Options";

    [ModdedNumberOption("Scrub Cooldown", 5f, 60f, 5f, MiraNumberSuffixes.Seconds)]
    public float ScrubCooldown { get; set; } = 30f;

    [ModdedNumberOption("Scrub Delay", 0f, 10f, 0.5f, MiraNumberSuffixes.Seconds)]
    public float ScrubDelay { get; set; } = 5f;

    [ModdedToggleOption("Scrubber Scrubs In Rounds")]
    public bool ScrubsInRounds { get; set; } = true;
}
