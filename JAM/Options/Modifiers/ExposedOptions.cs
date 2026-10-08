using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.GameOptions.OptionTypes;
using MiraAPI.Utilities;
using JAM.Modifiers;

namespace JAM.Options.Modifiers;

public sealed class ExposedOptions : AbstractOptionGroup<ExposedModifier>
{
    public override string GroupName => "Exposed Options";

    [ModdedNumberOption("Exposed Count", 0f, 15f, 1f)]
    public float ExposedAmount { get; set; } = 1f;

    [ModdedNumberOption("Exposed Chance", 0f, 100f, 10f, MiraNumberSuffixes.Percent)]
    public float ExposedChance { get; set; } = 50f;
}
