using JAM.Modifiers;
using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.GameOptions.OptionTypes;
using MiraAPI.Utilities;

namespace JAM.Options.Modifiers;

public sealed class ConcealedOptions : AbstractOptionGroup<JamConcealedModifier>
{
    public override string GroupName => "Concealed Options";
    
    [ModdedNumberOption("Concealed Count", 0f, 5f, 1f)]
    public float ConcealedCount { get; set; } = 1f;

    [ModdedNumberOption("Concealed Chance", 0f, 100f, 10f, MiraNumberSuffixes.Percent)]
    public float ConcealedChance { get; set; } = 50f;

    [ModdedNumberOption("Concealed Opacity", 0f, 1f, 0.05f)]
    public float ConcealedOpacity { get; set; } = 0.25f;

    [ModdedToggleOption("Conceal Name")]
    public bool ConcealName { get; set; } = false;
}
