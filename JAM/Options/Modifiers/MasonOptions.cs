using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.GameOptions.OptionTypes;
using MiraAPI.Utilities;
using JAM.Modifiers;

namespace JAM.Options.Modifiers;

public sealed class MasonOptions : AbstractOptionGroup<MasonModifier>
{
    public override string GroupName => "Masons Options";

    [ModdedNumberOption("Max Mason Count", 2f, 5f, 1f)]
    public float MasonAmount { get; set; } = 1f;

    [ModdedNumberOption("Mason Chance", 0f, 100f, 10f, MiraNumberSuffixes.Percent)]
    public float MasonChance { get; set; } = 50f;
}
