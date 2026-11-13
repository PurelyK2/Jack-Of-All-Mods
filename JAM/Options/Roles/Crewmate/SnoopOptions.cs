using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.GameOptions.OptionTypes;
using MiraAPI.Utilities;
using JAM.Roles.Crewmate;
using JAM.Roles.Neutral;
using TownOfUs.Extensions;
using TownOfUs.Modules.Localization;

namespace JAM.Options.Roles.Crewmate;

public sealed class SnoopOptions : AbstractOptionGroup<SnoopRole>
{
    public override string GroupName => "Snoop Options";
    
    [ModdedNumberOption("Sneak Cooldown", 0f, 45f, 2.5f, MiraNumberSuffixes.Seconds)]
    public float SneakCooldown { get; set; } = 25f;

    [ModdedNumberOption("Sneak Duration", 10f, 120f, 5f, MiraNumberSuffixes.Seconds)]
    public float SneakDuration { get; set; } = 30f;

    [ModdedToggleOption("Snoop Can Vent")]
    public bool SnoopCanVent { get; set; } = false;
}
