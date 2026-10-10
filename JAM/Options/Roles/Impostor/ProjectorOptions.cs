using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.Utilities;
using JAM.Roles.Impostor;

namespace JAM.Options.Roles.Impostor;

public sealed class ProjectorOptions : AbstractRoleOptionGroup<ProjectorRole>
{
    public override string GroupName => "Projector";

    [ModdedNumberOption("Ability Cooldown", 5f, 60f, 2.5f, MiraNumberSuffixes.Seconds)]
    public float ProjectCd { get; set; } = 25f;

    [ModdedNumberOption("Ability Duration", 5f, 60f, 2.5f, MiraNumberSuffixes.Seconds)]
    public float ProjectDuration { get; set; } = 20f;
    [ModdedToggleOption("Can Kill While Projected")]
    public bool ProjectKill { get; set; } = false;

    [ModdedNumberOption("Ghost Speed Multiplier", 0.5f, 2.5f, 0.1f, MiraNumberSuffixes.Multiplier)]
    public float GhostSpeed { get; set; } = 1f;
}
