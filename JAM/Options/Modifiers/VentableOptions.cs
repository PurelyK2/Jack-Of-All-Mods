using JAM.Modifiers.Game.Universal;
using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.GameOptions.OptionTypes;
using MiraAPI.Utilities;
using JAM.Roles.Crewmate;
using JAM.Roles.Neutral;
using TownOfUs.Extensions;
using TownOfUs.Modules.Localization;

namespace JAM.Options.Modifiers.Game.Universal;

public sealed class VentableOptions : AbstractOptionGroup<VentableModifier>
{
    public override string GroupName => "Ventable Options";
    
    [ModdedNumberOption("Ventable Count", 0f, 5f, 1f)]
    public float VentableCount { get; set; } = 1f;

    [ModdedNumberOption("Ventable Chance", 0f, 100f, 10f, MiraNumberSuffixes.Percent)]
    public float VentableChance { get; set; } = 50f;
}
