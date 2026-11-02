using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.GameOptions.OptionTypes;
using MiraAPI.Utilities;
using JAM.Modifiers;

namespace JAM.Options.Modifiers;

public sealed class FoolishOptions : AbstractOptionGroup<FoolishModifier>
{
    public override string GroupName => "Foolish Options";

    [ModdedNumberOption("Foolish Count", 0f, 15f, 1f)]
    public float FoolishAmount { get; set; } = 1f;

    [ModdedNumberOption("Foolish Chance", 0f, 100f, 10f, MiraNumberSuffixes.Percent)]
    public float FoolishChance { get; set; } = 50f;

    [ModdedNumberOption("Foolish Time", 5f, 60f, 2.5f, MiraNumberSuffixes.Seconds)]
    public float FoolishTime { get; set; } = 5f;
}
