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

public sealed class ConcealedOptions : AbstractOptionGroup<ConcealedModifier>
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
