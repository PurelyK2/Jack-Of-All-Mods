using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.GameOptions.OptionTypes;
using MiraAPI.Utilities;
using JAM.Roles.Crewmate;
using JAM.Roles.Neutral;
using TownOfUs.Extensions;
using TownOfUs.Modules.Localization;

namespace JAM.Options.Roles.Neutral;

public sealed class IndecisiveOptions : AbstractOptionGroup<IndecisiveRole>
{
    public enum IndecisiveStyle
    {
        Killer,
        Decider
    }

    public override string GroupName => "Indecisive Options";

    [ModdedEnumOption("Indecisive Role Style", typeof(IndecisiveStyle), ["Killer", "Decider"])]
    public IndecisiveStyle RoleStyle { get; set; } = IndecisiveStyle.Decider;

    [ModdedNumberOption("Shots Before Fixed Team", 1f, 5f, 1f)]
    public float SafeShots { get; set; } = 1;
}
