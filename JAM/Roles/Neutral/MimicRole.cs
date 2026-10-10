using AmongUs.GameOptions;
using JAM.Assets;
using MiraAPI.Roles;
using MiraAPI.Utilities.Assets;
using TownOfUs.Assets;
using TownOfUs.Extensions;
using TownOfUs.Interfaces;
using TownOfUs.Modules.Wiki;
using TownOfUs.Roles;
using TownOfUs.Roles.Neutral;
using UnityEngine;

namespace JAM.Roles.Neutral;

public sealed class MimicRole(IntPtr cppPtr) : NeutralRole(cppPtr), ITownOfUsRole, IWikiDiscoverable, IDoomable
{
    public string RoleName => "Mimic";
    public RoleAlignment RoleAlignment => RoleAlignment.NeutralKilling;
    public Color RoleColor => Colors.Mimic;
    public ModdedRoleTeams Team => ModdedRoleTeams.Custom;
    public DoomableType DoomHintType => DoomableType.Trickster;

    public CustomRoleConfiguration Configuration => new(this)
    {
        IntroSound = TouAudio.DetectiveIntroSound,
        Icon = JamRoleIcons.Mimic,
        IconTmp = TmpSpriteUtils.CreateSpriteAsset(JamRoleIcons.Mimic.LoadAsset(), "JackOfAllMods.Roles.Neutral.Mimic", 1.45f),
        GhostRole = (RoleTypes)RoleId.Get<NeutralGhostRole>()
    };
}
