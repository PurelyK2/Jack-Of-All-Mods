using HarmonyLib;
using Il2CppSystem.Web.Util;
using JAM.Assets;
using JAM.Modifiers.Crewmate;
using JAM.Options.Roles.Crewmate;
using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using MiraAPI.Modifiers.Types;
using MiraAPI.Patches.Stubs;
using MiraAPI.Roles;
using MiraAPI.Utilities;
using MiraAPI.Utilities.Assets;
using TownOfUs.Assets;
using TownOfUs.Extensions;
using TownOfUs.Interfaces;
using TownOfUs.Modifiers;
using TownOfUs.Modifiers.Game;
using TownOfUs.Modifiers.Game.Alliance;
using TownOfUs.Modifiers.Game.Assailant;
using TownOfUs.Modifiers.Game.Impostor;
using TownOfUs.Modifiers.Game.Universal;
using TownOfUs.Modules;
using TownOfUs.Modules.Wiki;
using TownOfUs.Roles;
using TownOfUs.Utilities;
using UnityEngine;

namespace JAM.Roles.Crewmate;

public sealed class JackOfAllRole(IntPtr cppPtr) : CrewmateRole(cppPtr), ITownOfUsRole, IWikiDiscoverable, IDoomable, IContinuesGame
{
    public int NumTasksUntilMod = (int)OptionGroupSingleton<JackOfAllOptions>.Instance.TasksPerMod;

    public DoomableType DoomHintType => DoomableType.Perception;
    public string RoleName => "Jack Of All";
    public string RoleDescription => "Have a lot of modifiers";
    public string RoleMedDescription => RoleDescription;
    public string RoleLongDescription => RoleDescription;
    public RoleAlignment RoleAlignment => RoleAlignment.CrewmatePower;

    public Color RoleColor => Colors.JackOfAll;

    public ModdedRoleTeams Team => ModdedRoleTeams.Crewmate;

    public CustomRoleConfiguration Configuration => new(this)
    {
        IconTmp = TmpSpriteUtils.CreateSpriteAsset(JamRoleIcons.JackOfAll.LoadAsset(), "JackOfAllMods.Roles.Crewmate.JackOfAll", 1.45f),
        IntroSound = JamAudio.JackOfAllIntro,
        Icon = JamRoleIcons.JackOfAll
    };

    public bool ContinuesGame
    {
        get
        {
            if (Player.Data.IsDead) return false;

            //Assassin ONLY when there are killers
            if(Player.HasModifier<AssassinModifier>())
            {
                if (MiscUtils.RealKillersAliveCount > 0)
                {
                    return true;
                }

                return false;
            }

            //Tasks ONLY when there are killers
            if(!Player.AllTasksCompleted() && MiscUtils.RealKillersAliveCount > 0)
            {
                return true;
            }

            //Regular Check
            return Player.GetModifiers<BaseModifier>().Any(m => m is IContinuesGame gameHalt && gameHalt.ContinuesGame);
        }
    }

    public string GetAdvancedDescription() { return RoleLongDescription + MiscUtils.AppendOptionsText(base.GetType()); }

    public override void Initialize(PlayerControl player)
    {
        RoleBehaviourStubs.Initialize(this, player);

        if(player.AmOwner)
        {
            try
            {
                GiveRandModifiers((int)OptionGroupSingleton<JackOfAllOptions>.Instance.NumModifiers - player.GetModifiers<BaseModifier>().Count(), player);
            }
            catch(System.Exception e)
            {
                Fatal(e);
            }
        }
    }
    static void GiveRandModifiers(int count, PlayerControl player)
    {
        if(count < 1) return;

        try
        {
            List<BaseModifier> modifiers =
                    ModifierManager.Modifiers
                    .Where(m => m is GameModifier
                    && (m as GameModifier)?.GetAmountPerGame() > 0
                    && (m as GameModifier)?.GetAssignmentChance() > 0
                    && (m as GameModifier)?.CanSpawnOnCurrentMode() == true
                    && (!player.HasModifier(m.TypeId))
                    && m is not DeadlyQuotaModifier
                    && m is not CircumventModifier
                    && m is not AllianceGameModifier
                    && m.GetModifierFaction() != ModifierFaction.UniversalVisibility
                    && (m is not TelepathModifier || player.HasModifier<EgotistModifier>() || player.HasModifier<CrewpostorModifier>())
                    && (m is not DoubleShotModifier || player.HasModifier<AssassinModifier>())
                    && (m is not FirstDeadShield)
                    || m is JackOfAllVotes
                ).ToList();

            if(modifiers.Count == 0)
            {
			    Error("No modifiers to give");
                MiraAPI.Utilities.Helpers.CreateAndShowNotification("There Are No Modifiers Left To Give", Color.white, new Vector3(0f, 1f, -20f), null, null);

                return;
            }

            for(int i = 0; i < count; i++)
            {
                BaseModifier modifier;
                do
                {
                    if (modifiers.Count == 0)
                    {
                        Error("No modifiers to give");
                        MiraAPI.Utilities.Helpers.CreateAndShowNotification("There Are No Modifiers Left To Give", Color.white, new Vector3(0f, 1f, -20f), null, null);

                        return;
                    }

                    modifiers.Shuffle();
                    modifier = modifiers[0];
                    modifiers.Remove(modifier);
                } while (player.HasModifier(modifier.TypeId) && modifier is not JackOfAllVotes && modifiers.Count > 0);

                if (modifier is JackOfAllVotes)
                {
                    JackOfAllVotes? joav = player.GetModifier<JackOfAllVotes>();

                    if(joav == null)
                    {
                        player.RpcAddModifier<JackOfAllVotes>();
                    }
                    else
                    {
                        joav.NumVotes++;
                    }
                }
                else
                    player.RpcAddModifier(modifier.TypeId, Array.Empty<object>());
            }
        }
        catch(System.Exception e)
        {
			Fatal(e);
        }
    }

    public static void CheckAddModifier(PlayerControl player)
    {
        if(player.AmOwner && player.Data.Role is JackOfAllRole)
        {
			Info("Completed Task");
            GiveRandModifiers(1, player);
        }
    }

    [HarmonyPatch(typeof(MiscUtils), "GameHaltersAliveCount", MethodType.Getter)]
    public static class ModsDontContinueOnJack
    {
        public static void Postfix(ref int __result)
        {
            foreach(PlayerControl player in MiraAPI.Utilities.Helpers.GetAlivePlayers().Where(p => p.Data.Role is JackOfAllRole))
            {
                if(!(player is IContinuesGame contGame && contGame.ContinuesGame))
                {
                    if (player.GetModifiers<BaseModifier>().Any(m => m is IContinuesGame contGame && contGame.ContinuesGame))
                    {
                        __result--;
                    }
                }
            }
        }
    }
}
