using JAM.Roles.Neutral;
using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.GameOptions.OptionTypes;
using MiraAPI.Utilities;
using TownOfUs.Extensions;
using TownOfUs.Modules.Localization;

namespace JAM.Options.Roles.Neutral;
public sealed class ZombieOptions : AbstractOptionGroup<ZombieLeaderRole>
{
    public override string GroupName => "Zombie Options";

    [ModdedToggleOption("Zombies Are Revealed")]
    public bool ZombieShowsRole { get; set; } = true;

    [ModdedToggleOption("Zombie Leader Has Arrows To Dead")]
    public bool ZombieArrows { get; set; } = true;

    [ModdedNumberOption("Zombie Revive Cooldown", 0f, 15f, 1f, MiraNumberSuffixes.Seconds)]
    public float ZombieReviveCd { get; set; } = 1f;

    [ModdedNumberOption("Zombie Dead Duration", 0f, 120f, 5f, MiraNumberSuffixes.Seconds)]
    public float ZombieReviveTimer { get; set; } = 5f;

    [ModdedToggleOption("Zombie Leader Is Unlovable")]
    public bool ZombieCantLove { get; set; } = true;

    [ModdedNumberOption("Players Left When Unguessable", 0, 15, 1)]
    public int PlayersWhenUnguessable { get; set; } = 3;
}
