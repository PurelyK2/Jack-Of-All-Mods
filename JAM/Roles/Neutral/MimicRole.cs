using AmongUs.GameOptions;
using HarmonyLib;
using Hazel;
using JAM.Assets;
using JAM.Modifiers.Role;
using JAM.Options.OptionTypes;
using JAM.Options.Roles.Neutral;
using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using MiraAPI.Roles;
using MiraAPI.Utilities;
using MiraAPI.Utilities.Assets;
using Reactor.Networking.Attributes;
using Reactor.Networking.Rpc;
using TownOfUs.Assets;
using TownOfUs.Extensions;
using TownOfUs.Interfaces;
using TownOfUs.Modules.Wiki;
using TownOfUs.Roles;
using TownOfUs.Roles.Crewmate;
using TownOfUs.Roles.Neutral;
using TownOfUs.Utilities;
using UnityEngine;

namespace JAM.Roles.Neutral;

public sealed class MimicRole(IntPtr cppPtr) : NeutralRole(cppPtr), ITownOfUsRole, IWikiDiscoverable, IDoomable, IContinuesGame
{
    public string RoleName => "Mimic";
    public RoleAlignment RoleAlignment => RoleAlignment.NeutralKilling;
    public Color RoleColor => Colors.Mimic;
    public ModdedRoleTeams Team => ModdedRoleTeams.Custom;
    public DoomableType DoomHintType => DoomableType.Trickster;
    public string RoleDescription => "Copy-Pasta!";
    public string RoleMedDescription => "Each Round, Pick A Role To Mimic For That Round.";
    public string RoleLongDescription => "Each Round, Pick A Role To Mimic For That Round.";
    public string GetAdvancedDescription() { return RoleLongDescription + TownOfUs.Utilities.MiscUtils.AppendOptionsText(base.GetType()); }

    public CustomRoleConfiguration Configuration => new(this)
    {
        IntroSound = TouAudio.DetectiveIntroSound,
        Icon = JamRoleIcons.Mimic,
        IconTmp = TmpSpriteUtils.CreateSpriteAsset(JamRoleIcons.Mimic.LoadAsset(), "JackOfAllMods.Roles.Neutral.Mimic", 1.45f),
        GhostRole = (RoleTypes)RoleId.Get<NeutralGhostRole>()
    };

    public override void OnRoleSet()
    {
        if(!Player.HasModifier<MimicAllianceModifier>())
            Player.AddModifier<MimicAllianceModifier>();

        base.OnRoleSet();
    }

    public static List<RoleBehaviour> GetValidMimicRoles()
    {
        List<RoleBehaviour> roleList = DestroyableSingleton<RoleManager>.Instance.AllRoles.ToArray().Where(delegate (RoleBehaviour r)
        {
            if (OptionGroupSingleton<MimicOptions>.Instance.BlacklistedRoles.rolesList.Any(role => role.Role == r.Role))
                return false;

            return IsValidMimicRole(r);
        }).ToList();

        return roleList;
    }

    public static bool IsValidMimicRole(RoleBehaviour role)
    {
        RoleManager.RoleAssignmentData roleData = CustomRoleUtils.GetAssignData(role.Role);

        if (roleData.Count == 0 || roleData.Chance == 0) return false; // Only If It Can Currenlty Be In The Game
        if (!CustomRoleUtils.CanSpawnOnCurrentMode(role)) return false; // Only If It Can Spawn On The Current Mode
        if (role is IGhostRole) return false; // No Ghost Roles
        if (role is IAssignableTargets) return false; // Not A Targeting One (eg. Exe or Fairy)
        if (role is IDoubleDraftRole) return false; // For Things Like Recruiter And Vampire

        if (role.IsImpostor()) return false; // Stupid Fix Right Now So Mimic At Least Can Be Played

        if (role.GetRoleAlignment() == RoleAlignment.NeutralOutlier) return false; // Removing Chef, Inquiz, etc.
        if (role.GetRoleAlignment() == RoleAlignment.CrewmatePower) return false; // No Crewmate Power
        if (role.GetRoleAlignment() == RoleAlignment.CrewmateSupport) return false; // No Crewmate Support
        if (role.GetRoleAlignment() == RoleAlignment.NeutralEvil) return false; // No Neutral Evils

        // Explicitly Blacklisted Roles
        if (role is ZombieLeaderRole
            || role is MercenaryRole
            || role is MimicRole
            || role is AltruistRole
            || role is AmnesiacRole
            || role is ForbearingRole
            || role is PoliticianRole) return false;


        return true; //Will Be Ok Here
    }

    public bool ContinuesGame => MiraAPI.Utilities.Helpers.GetAlivePlayers().Count > 2;

    public override bool CanUse(IUsable usable)
    {
        if (!GameManager.Instance.LogicUsables.CanUse(usable, Player))
        {
            return false;
        }

        var console = usable.TryCast<Console>()!;
        return console == null || console.AllowImpostor;
    }
}
