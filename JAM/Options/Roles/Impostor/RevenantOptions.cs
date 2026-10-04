using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.Utilities;
using JAM.Roles.Impostor;

namespace JAM.Options.Roles.Impostor;

public sealed class RevenantOptions : AbstractRoleOptionGroup<RevenantRole>
{
    public override string GroupName => "Revenant";

    [ModdedNumberOption("Ability Cooldown", 5f, 120f, 2.5f, MiraNumberSuffixes.Seconds)]
    public float DetachCd { get; set; } = 25f;

    [ModdedNumberOption("Ability Duration", 5f, 60f, 2.5f, MiraNumberSuffixes.Seconds)]
    public float DetachDuration { get; set; } = 20f;
    [ModdedToggleOption("Can Kill While Detached")]
    public bool DetachKill { get; set; } = false;

    [ModdedNumberOption("Ghost Speed Multiplier", 0.5f, 2.5f, 0.1f, MiraNumberSuffixes.Multiplier)]
    public float GhostSpeed { get; set; } = 1f;
}
