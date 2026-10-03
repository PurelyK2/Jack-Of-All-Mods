using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.GameOptions.OptionTypes;
using MiraAPI.Utilities;
using JAM.Roles.Crewmate;
using JAM.Roles.Neutral;
using TownOfUs.Extensions;
using TownOfUs.Modules.Localization;

namespace JAM.Options.Roles.Crewmate;

public sealed class InformantOptions : AbstractOptionGroup<InformantRole>
{
    public override string GroupName => "Informant";
    
    [ModdedNumberOption("Investigate Cooldown", 0f, 35f, 2.5f, MiraNumberSuffixes.Seconds)]
    public float InformantCooldown { get; set; } = 15f;
    
    [ModdedNumberOption("Investigate Roles Count", 0f, 15f, 1f)]
    public float InformantRoles { get; set; } = 8f;

    [ModdedToggleOption("Shares Info")]
    public bool ShareInfo { get; set; } = true;

    [ModdedNumberOption("Crew Role Weight", 25f, 75f, 5f, MiraNumberSuffixes.Percent)]
    public float CrewWeight { get; set; } = 50;
}
