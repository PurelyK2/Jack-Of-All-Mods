using AmongUs.GameOptions;
using HarmonyLib;
using Il2CppSystem.Runtime.Remoting.Messaging;
using JAM.Assets;
using JAM.Modifiers;
using JAM.Options.Roles.Neutral;
using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using MiraAPI.Roles;
using Reactor.Networking.Attributes;
using Reactor.Utilities.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TownOfUs;
using TownOfUs.Assets;
using TownOfUs.Extensions;
using TownOfUs.Interfaces;
using TownOfUs.Modifiers;
using TownOfUs.Modifiers.Game.Alliance;
using TownOfUs.Modules;
using TownOfUs.Modules.Wiki;
using TownOfUs.Options;
using TownOfUs.Roles;
using TownOfUs.Roles.Neutral;
using TownOfUs.Utilities;
using UnityEngine;

namespace JAM.Roles.Neutral;

public sealed class VacillatorRole(IntPtr cppPtr) : NeutralRole(cppPtr), ITownOfUsRole, IWikiDiscoverable, IDoomable, IUnlovable, IContinuesGame
{
    public bool? winsWithCrew;

    [MethodRpc((uint)JAMRpcCalls.UpdateVacillatorAlignment)]
    public static void RPCUpdateVacillatorAlignment(PlayerControl player, bool crewmate)
    {
        (player.Data.Role as VacillatorRole)?.winsWithCrew = crewmate;
    }

    public bool HasImpostorVision => winsWithCrew == false;
    public int SafeShotsLeft = 1;
    public string RoleName => "Vacillator";
    public string RoleDescription => "Decide Who You Want To Win With";
    public string RoleMedDescription => "Kill Players To Decide Who You Win With!";
    public string RoleLongDescription
    {
        get
        {
            return "Select A Team To Win With.\nCurrent Alignment: " + (winsWithCrew == null ? "None" : (winsWithCrew == true ? "Crewmate" : "Non-Crew")) + "\n\nShots Until Team Is Forced: " + SafeShotsLeft;
        }
    }
    public string GetAdvancedDescription() { return RoleMedDescription + TownOfUs.Utilities.MiscUtils.AppendOptionsText(base.GetType()); }
    public DoomableType DoomHintType => DoomableType.Perception;
    public RoleAlignment RoleAlignment => RoleAlignment.NeutralOutlier;
    public Color RoleColor => Color.grey;
    public bool IsUnlovable => true;
    public ModdedRoleTeams Team => ModdedRoleTeams.Custom;
    public CustomRoleConfiguration Configuration => new(this)
    {
        IntroSound = TouAudio.OtherIntroSound,
        Icon = TouModifierIcons.Colorblind,
        GhostRole = (RoleTypes)RoleId.Get<NeutralGhostRole>(),
        TasksCountForProgress = false
    };

    public bool ContinuesGame => (winsWithCrew != true && MiraAPI.Utilities.Helpers.GetAlivePlayers().Count > 2) || TownOfUs.Utilities.MiscUtils.KillersAliveCount > 0;

    public override bool DidWin(GameOverReason gameOverReason)
    {
        return RoleManager.Instance.GetRole(RoleTypes.Crewmate).DidWin(gameOverReason) == winsWithCrew;
    }

    public override void UseAbility()
    {
        base.UseAbility();
    }
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

[HarmonyPatch(typeof(TownOfUs.Utilities.TouRoleUtils), "CanGetGhostRole", [typeof(PlayerControl)])]
public static class CantGetGhostRole
{
    public static void Postfix(ref bool __result, PlayerControl player)
    {
        if(player?.Data?.Role != null && player.Data.Role is VacillatorRole vacillator && vacillator.winsWithCrew != null)
        {
            __result = false;
        }
    }
}

[HarmonyPatch(typeof(TownOfUs.Modules.Components.HudManagerHelper), "GetRoleNameText", [typeof(PlayerControl), typeof(bool), typeof(PostmortemOptions), typeof(string), typeof(bool), typeof(bool), typeof(bool), typeof(bool), typeof(bool), typeof(bool), typeof(bool), typeof(bool), typeof(bool), typeof(bool)])]
public static class GetRoleNameTextPostfix
{
    public static void Postfix(PlayerControl player, ref ValueTuple<Color, string> __result)
    {
        if (player?.GetRoleWhenAlive() is VacillatorRole vacillator && (player.AmOwner || (PlayerControl.LocalPlayer.HasDied() && PlayerControl.LocalPlayer.DiedOtherRound()) || FairyRole.FairySeesRoleVisibilityFlag(player)))
        {
            if(vacillator.winsWithCrew == true)
            {
                __result.Item2 += $" ({TownOfUsColors.Crewmate.ToTextColor()}Crewmate</color>)";
            }
            else if (vacillator.winsWithCrew == false)
            {
                __result.Item2 += $" ({TownOfUsColors.Impostor.ToTextColor()}Other</color>)";
            }
        }
    }
}