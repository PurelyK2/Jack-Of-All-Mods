using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.GameOptions.OptionTypes;
using MiraAPI.Utilities;
using JAM.Modifiers.Universal;

namespace JAM.Options.Modifiers;

public sealed class BatteryOptions : AbstractOptionGroup<BatteryModifier>
{
    public override string GroupName => "Battery Options";

    [ModdedNumberOption("Battery Count", 0f, 15f, 1f)]
    public float BatteryAmount { get; set; } = 1f;

    [ModdedNumberOption("Battery Chance", 0f, 100f, 10f, MiraNumberSuffixes.Percent)]
    public float BatteryChance { get; set; } = 50f;
}
