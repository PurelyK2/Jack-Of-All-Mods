using AmongUs.GameOptions;
using HarmonyLib;
using Hazel;
using Il2CppInterop.Runtime.Attributes;
using InnerNet;
using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.GameOptions;
using MiraAPI.Hud;
using MiraAPI.Modifiers;
using MiraAPI.Roles;
using MiraAPI.Utilities;
using MiraAPI.Utilities.Assets;
using System.Xml.Linq;
using TownOfUs;
using TownOfUs.Assets;
using TownOfUs.Buttons.Crewmate;
using TownOfUs.Events.Impostor;
using TownOfUs.Extensions;
using TownOfUs.Modifiers;
using TownOfUs.Modifiers.Game.Assailant;
using TownOfUs.Modules;
using TownOfUs.Modules.Wiki;
using TownOfUs.Patches;
using TownOfUs.Roles;
using TownOfUs.Roles.Crewmate;
using TownOfUs.Roles.Neutral;
using TownOfUs.Utilities;
using UnityEngine;
using Reactor.Networking.Attributes;
using JAM;
using JAM.Assets;
using JAM.Options.Roles.Crewmate;
using JAM.Options.Roles.Impostor;
using JAM.Modifiers.Role;
using JAM.Modifiers.Hidden;

//Note: This Role Was Suggested By: ‧₊˚✧ 𝒥𝒶𝓎 :3 ✧˚₊‧ (Discord)
public sealed class DeceiverRole(IntPtr cppPtr) : ImpostorRole(cppPtr), ITownOfUsRole, IWikiDiscoverable, ICrewVariant
{
    public RoleBehaviour randomizedRole;
    public static bool confuseRole;
    public static bool hasGameStarted;

    public RoleAlignment RoleAlignment => RoleAlignment.ImpostorConcealing;
    public string RoleName => "Deceiver";

    public string RoleDescription => "You Seem Innocent To Others...";
    public string RoleMedDescription => "Hide Your True Alignment And Role From Others";
    public string RoleLongDescription => "Appear As A Random, Not-In-Play Crewmate Role To Those Collecting Information From You.";
	public string GetAdvancedDescription()
	{
		return RoleLongDescription + MiscUtils.AppendOptionsText(base.GetType());
	}

    [HideFromIl2Cpp]
    public List<CustomButtonWikiDescription> Abilities
    {
        get
        {
            return new List<CustomButtonWikiDescription>
            {
                new("Deceive", "Give Someone A Fake Shield That Does Nothing", TouCrewAssets.MedicSprite),
            };
        }
    }

    public Color RoleColor => TownOfUsColors.Impostor;

    public ModdedRoleTeams Team => ModdedRoleTeams.Impostor;

    public CustomRoleConfiguration Configuration => new(this)
    {
        IconTmp = TmpSpriteUtils.CreateSpriteAsset(JamRoleIcons.Deceiver.LoadAsset(), "JackOfAllMods.Roles.Impostor.Deceiver", 1.45f),
        UseVanillaKillButton = true,
        IntroSound = TouAudio.HackedSound,
        Icon = JamRoleIcons.Deceiver
    };

    public RoleBehaviour CrewVariant => DestroyableSingleton<RoleManager>.Instance.GetRole((RoleTypes)RoleId.Get<SeerRole>());

    public override void OnRoleSet()
    {
        base.OnRoleSet();
		Player.AddModifier<DeceiverModifier>();
    }

    [HarmonyPatch(typeof(IntroScenePatches), "ShowTeamPatchPostfix", [typeof(IntroCutscene)])]
    public static class DeceiverWaitsPatch
    {
        public static void Postfix()
        {
            hasGameStarted = true;
            ReConfuse();
        }
    }

    [RegisterEvent(0)]
    public static void OnRoundStart(RoundStartEvent @event)
    {
        confuseRole = false;
        foreach(PlayerControl player in Helpers.GetAlivePlayers().Where(p => p.Data.Role is DeceiverRole))
        {
            if(player.Data.Role is DeceiverRole deceiver)
                deceiver.randomizedRole = RandomizeRole();
        }
        confuseRole = true;
    }
    [RegisterEvent(0)]
    public static void OnDeathEvent(AfterMurderEvent @event)
    {
        if(PlayerControl.LocalPlayer.Data.IsDead)
        {
            confuseRole = false;
            hasGameStarted = false;
        }
    }

    [MethodRpc((uint)JAMRpcCalls.DeceiverShield)]
    public static void RpcDeceiverShield(PlayerControl deceiver, PlayerControl target)
    {
        if(PlayerControl.LocalPlayer == deceiver || PlayerControl.LocalPlayer == target)
        {
            target.AddModifier<DeceiverMedicShield>();
        }
    }

    #region Deceiver Deceive Exceptions
    [HarmonyPatch(typeof(EndGameResult), "Create", [ typeof(MessageReader) ])]
    public static class DeceiverDoesntWinWithCrewPatch
    {
        public static void Prefix()
        {
            confuseRole = false;
        }
        public static void Postfix()
        {
            ReConfuse();
        }
    }

    [HarmonyPatch(typeof(AssassinModifier), "ClickGuess", [typeof(PlayerVoteArea), typeof(MeetingHud)])]
	public static class AssassinGuessDeceiverPatch
	{
		public static void Prefix()
		{
			confuseRole = false;
		}
		public static void Postfix()
        {
            ReConfuse();
        }
    }

    [HarmonyPatch(typeof(TownOfUs.Utilities.Extensions), "RpcChangeRole", [typeof(PlayerControl), typeof(ushort), typeof(bool)])]
    public static class DeceiverChangesRolePatch
    {
        public static void Prefix()
        {
            confuseRole = false;
        }
        public static void Postfix()
        {
            ReConfuse();
        }
    }
    
    [HarmonyPatch(typeof(LogicGameFlowNormal), nameof(LogicGameFlowNormal.CheckEndCriteria))]
	public static class DeceiverDoesntDeceiveTheGamePatch
    {
        [HarmonyPriority(801)]
        public static void Prefix()
        {
            confuseRole = false;
        }
        public static void Postfix()
        {
            ReConfuse();
        }
    }

    [HarmonyPatch(typeof(TownOfUs.Utilities.Extensions), "IsImpostorAligned", [typeof(PlayerControl)])]
    public static class DeceiverIsImpostorPatch
    {
        public static void Prefix()
        {
            confuseRole = false;
        }
        public static void Postfix()
        {
            ReConfuse();
        }
    }

    [HarmonyPatch(typeof(TraitorEvents), "RoundStartEventHandler", [typeof(RoundStartEvent)])]
    public static class TraitorPickedPatch
    {
        public static void Prefix()
        {
            confuseRole = false;
        }
        public static void Postfix()
        {
            ReConfuse();
        }
    }
    #endregion

    [HarmonyPatch(typeof(NetworkedPlayerInfo), "Role", MethodType.Getter)]
    public static class GetDeceiverRolePatch
    {
		public static void Postfix(ref RoleBehaviour __result, ref NetworkedPlayerInfo __instance)
		{
			if (__result?.Player == null || __instance?.PlayerId == null) return;

			if(__result.Player.HasModifier<DeceiverModifier>())
			{
				__result = DestroyableSingleton<RoleManager>.Instance.GetRole((RoleTypes)RoleId.Get<DeceiverRole>());
			}

			if (!confuseRole) return;
			if (__result is not DeceiverRole || __instance.IsDead) return;
			if(__instance.AmOwner
				&& (__instance.IsDead
                || __result.Player.HasModifier<BaseRevealModifier>())
                || (__result.GetRoleAlignment() == RoleAlignment.CrewmateKilling && !OptionGroupSingleton<DeceiverOptions>.Instance.DeceiveCrewKillers)) return;

			if(OptionGroupSingleton<DeceiverOptions>.Instance.DeceiverDisplayedAs == DeceiverOptions.DeceiverRoleDisplayed.Investigator)
				__result = DestroyableSingleton<RoleManager>.Instance.GetRole((RoleTypes)RoleId.Get<InvestigatorRole>());
			else
            {
                if(__result is DeceiverRole deceiver && deceiver.randomizedRole != null)
                    __result = deceiver.randomizedRole;
			}
		}
	}

    [RegisterEvent(0)]
	public static void EndGamePatch(BeforeGameEndEvent @event)
    {
        hasGameStarted = false;
        confuseRole = false;
	}

    [HarmonyPatch(typeof(AmongUsClient), "CoStartGame")]
    public static class DeceiverBreakingInsurance
    {
        public static void Prefix()
        {
            hasGameStarted = false;
            confuseRole = false;
        }
    }

    public static void ReConfuse()
    {
        if (!hasGameStarted) return;
        if (PlayerControl.LocalPlayer.Data.Role.IsImpostor() || PlayerControl.LocalPlayer.Data.IsDead || PlayerControl.LocalPlayer.Data.Role is IGhostRole) return;
        if (PlayerControl.LocalPlayer.Data.Role is SnitchRole || PlayerControl.LocalPlayer.Data.Role is InquisitorRole) return;

        confuseRole = true;
    }

    static RoleBehaviour RandomizeRole()
    {
        List<RoleBehaviour> allRoles = DestroyableSingleton<RoleManager>.Instance.AllRoles.ToArray().Where(r => r is not IGhostRole && r.IsCrewmate() && r.GetRoleAlignment() != RoleAlignment.CrewmateKilling).ToList();
        List<RoleBehaviour> crewRoles = new List<RoleBehaviour>();
        foreach (RoleBehaviour role in allRoles)
        {
            RoleManager.RoleAssignmentData roleData = CustomRoleUtils.GetAssignData(role.Role);
            if (roleData.Chance > 0 && roleData.Count > 0 && CustomRoleUtils.CanSpawnOnCurrentMode(role))
            {
                crewRoles.Add(role);
            }
        }

        crewRoles.RemoveAll(r => PlayerControl.AllPlayerControls.ToArray().Any(p => p.Data.Role.GetType() == r.GetType()));

        if (crewRoles.Count == 0)
        {
            return DestroyableSingleton<RoleManager>.Instance.GetRole((RoleTypes)RoleId.Get<InvestigatorRole>());
        }

        RoleBehaviour randomCrewRole = crewRoles[UnityEngine.Random.Range(0, crewRoles.Count)];

        if (randomCrewRole != null)
        {
            return randomCrewRole;
        }

        return DestroyableSingleton<RoleManager>.Instance.GetRole((RoleTypes)RoleId.Get<DeceiverRole>());
    }
}
