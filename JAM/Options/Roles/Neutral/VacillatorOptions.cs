using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.GameOptions.OptionTypes;
using MiraAPI.Utilities;
using JAM.Roles.Crewmate;
using JAM.Roles.Neutral;
using TownOfUs.Extensions;
using TownOfUs.Modules.Localization;

namespace JAM.Options.Roles.Neutral;

public sealed class VacillatorOptions : AbstractOptionGroup<VacillatorRole>
{
    public override string GroupName => "Indecisive Options";

    [ModdedNumberOption("Shots Before Fixed Team", 1f, 5f, 1f)]
    public float SafeShots { get; set; } = 1;
}
