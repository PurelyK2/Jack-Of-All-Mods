using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.GameOptions.OptionTypes;
using MiraAPI.Utilities;
using JAM.Modifiers.Universal;

namespace JAM.Options.Modifiers;

public sealed class ShackledOptions : AbstractOptionGroup<ShackledModifier>
{
    public override string GroupName => "Shackled Options";

    [ModdedNumberOption("Shackled Count", 0f, 15f, 1f)]
    public float ShackledAmount { get; set; } = 1f;

    [ModdedNumberOption("Shackled Chance", 0f, 100f, 10f, MiraNumberSuffixes.Percent)]
    public float ShackledChance { get; set; } = 50f;

    [ModdedNumberOption("Shackled Duration", 3f, 25f, 1f, MiraNumberSuffixes.Seconds)]
    public float ShackledDuration { get; set; } = 5f;

    [ModdedNumberOption("Shackled Speed", 0.25f, 1.5f, 0.25f, MiraNumberSuffixes.Multiplier)]
    public float ShackledSpeedMultiplier { get; set; } = 1f;

    [ModdedToggleOption("Shackled Speed Is Affected by Body Size")]
    public bool SizeAffectedSpeed { get; set; } = true;

    [ModdedToggleOption("Killer Can Vent with Body")]
    public bool CanVentWithBody { get; set; } = false;

}
