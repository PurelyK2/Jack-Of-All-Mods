using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.GameOptions.OptionTypes;
using MiraAPI.Utilities;
using JAM.Modifiers;

namespace JAM.Options.Modifiers;

public sealed class RivalOptions : AbstractOptionGroup<RivalModifier>
{
    public override string GroupName => "Rivals Options";

    [ModdedNumberOption("Rivals Chance", 0f, 100f, 5f, MiraNumberSuffixes.Percent)]
    public float RivalChance { get; set; } = 50f;
}
