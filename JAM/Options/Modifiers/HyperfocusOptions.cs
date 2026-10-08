using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.GameOptions.OptionTypes;
using MiraAPI.Utilities;
using JAM.Modifiers;

namespace JAM.Options.Modifiers;

public sealed class HyperfocusOptions : AbstractOptionGroup<HyperfocusModifier>
{
    public override string GroupName => "Hyperfocus Options";

    [ModdedNumberOption("Hyperfocus Count", 0f, 5f, 1f)]
    public float HyperfocusCount { get; set; } = 1f;
    
    [ModdedNumberOption("Hyperfocus Chance", 0f, 100f, 10f, MiraNumberSuffixes.Percent)]
    public float HyperfocusChance { get; set; } = 50f;
}
