using AmongUs.GameOptions;
using HarmonyLib;
using JAM.Assets;
using JAM.Modifiers.Hidden;
using JAM.Options.Roles.Neutral;
using JAM.Patches.WinConditions;
using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.Events.Vanilla.Meeting;
using MiraAPI.GameEnd;
using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using MiraAPI.Patches.Stubs;
using MiraAPI.Roles;
using MiraAPI.Utilities;
using MiraAPI.Utilities.Assets;
using Reactor.Utilities;
using TownOfUs.Assets;
using TownOfUs.Extensions;
using TownOfUs.Interfaces;
using TownOfUs.Modules;
using TownOfUs.Modules.Wiki;
using TownOfUs.Networking;
using TownOfUs.Roles;
using TownOfUs.Roles.Crewmate;
using TownOfUs.Roles.Neutral;
using TownOfUs.Utilities;
using UnityEngine;

namespace JAM.Roles.Neutral;

public class ZombieRole(IntPtr cppPtr) : NeutralRole(cppPtr), ITownOfUsRole, IWikiDiscoverable, IUnguessable
{
    public bool HasImpostorVision => true;
    public RoleAlignment RoleAlignment => RoleAlignment.NeutralEvil;
    public string RoleName => "Zombie";
    public string RoleDescription => "THE APOCOLYPSE HAS BEGUN!";
    public string RoleMedDescription => "Convert Dead Players Into Zombies.";
    public string RoleLongDescription => "Convert Dead Players Into Zombies.";
    
    public string GetAdvancedDescription() { return RoleLongDescription + MiscUtils.AppendOptionsText(base.GetType()); }

    public Color RoleColor => Colors.Zombie;
    public ModdedRoleTeams Team => ModdedRoleTeams.Custom;
    public RoleBehaviour AppearAs => (RoleBehaviour)RoleId.Get<ZombieRole>();
    public bool IsGuessable => false;
    public new bool IsDraftable => false;
    public CustomRoleConfiguration Configuration => new(this)
    {
        IconTmp = TmpSpriteUtils.CreateSpriteAsset(JamRoleIcons.Zombie.LoadAsset(), "JackOfAllMods.Roles.Neutral.Zombie", 1.45f),
        Icon = JamRoleIcons.Zombie,
        HideSettings = true,
        CanModifyChance = false,
        DefaultChance = 0,
        DefaultRoleCount = 0,
        MaxRoleCount = 0,
        TasksCountForProgress = false,
        GhostRole = (RoleTypes)RoleId.Get<NeutralGhostRole>()
    };

    public List<CustomButtonWikiDescription> Abilities
    {
        get
        {
            return 
            [
				new($"Revive", "Revive Dead Bodies turning them into a loyal Zombie servant", 
                    TouRoleIcons.Altruist),
            ];
        }
    }

    public override void Initialize(PlayerControl player)
    {
        RoleBehaviourStubs.Initialize(this, player);

        foreach(BaseModifier modifier in Player.GetModifiers<BaseModifier>().Where(m => !m.HideOnUi))
        {
            Player.RemoveModifier(modifier);
        }

        Player.RpcAddModifier<ZombieRevealedModifier>();
        Player.RemainingEmergencies = 0;
    }

    public bool WinConditionMet()
    {
        if (Helpers.GetAlivePlayers().FirstOrDefault(p => p.GetRoleWhenAlive() is ZombieLeaderRole)?.GetRoleWhenAlive() is ZombieLeaderRole zombieLeader)
        {
            return zombieLeader.WinConditionMet();
        }
        return false;
    }

    public override bool DidWin(GameOverReason gameOverReason)
    {
        if (Helpers.GetAlivePlayers().FirstOrDefault(p => p.GetRoleWhenAlive() is ZombieLeaderRole)?.GetRoleWhenAlive() is ZombieLeaderRole zombieLeader)
        {
            return zombieLeader.DidWin(gameOverReason);
        }
        return false;
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

    public void Update()
    {
        if(Player == null || Player.Data.IsDead) return;

        if(!MiraAPI.Utilities.Helpers.GetAlivePlayers().Any(p => p.GetRoleWhenAlive() is ZombieLeaderRole))
        {
            Player.RpcSpecialMurder(Player, true, true, true, false, false, false, false, false, "Leaderless");
        }
    }

    [HarmonyPatch(typeof(TouRoleUtils), "CanGetGhostRole", [typeof(PlayerControl)])]
    [HarmonyPostfix]
    public static void NoZombieSpectre(ref PlayerControl __instance, ref bool __result)
    {
        if (__instance.Data.Role is ZombieRole)
            __result = false;
    }

    [RegisterEvent(0)]
    public static void StartOfMeetingEvent(StartMeetingEvent @event)
    {
        foreach (PlayerControl player in Helpers.GetAlivePlayers().Where(p => p.Data.Role is ZombieRole))
        {
            player.RpcSpecialMurder(player, true, true, true, false, false, false, false, false, "Undead");
        }
    }
    [RegisterEvent(0)]
    public static void StartOfRoundEvent(RoundStartEvent @event)
    {
        if (@event.TriggeredByIntro) return;

        Info("Respawning Zombies");
        foreach (PlayerControl player in PlayerControl.AllPlayerControls.ToArray().Where(p => p.GetRoleWhenAlive() is ZombieRole))
        {
            Info("Respawning " + player.Data.PlayerName);
            player.RpcFullRevive(false, player.GetTruePosition(), RoleId.Get<ZombieRole>(), false);
        }
    }

    [HarmonyPatch(typeof(MiscTouRpcs), "RpcFullRevive", [ typeof(PlayerControl), typeof(bool), typeof(Vector2), typeof(ushort), typeof(ushort), typeof(bool) ])]
    [HarmonyPostfix]
    public static void SoundAndVisualOnRevive(ref PlayerControl player)
    {
        if(player.AmOwner && player.Data.Role is ZombieRole)
        {
            Coroutines.Start(MiscUtils.CoFlash(Colors.Zombie));
            TouAudio.PlaySound(TouAudio.AltruistReviveSound);
        }
    }
}

public sealed class ZombieLeaderRole(IntPtr cppPtr) : NeutralRole(cppPtr), ITownOfUsRole, IWikiDiscoverable, IDoomable, IUnguessable, ICrewVariant, IContinuesGame
{
    public static bool hasZombies = false;
    public bool HasImpostorVision => true;
    public RoleAlignment RoleAlignment => RoleAlignment.NeutralEvil;
    public DoomableType DoomHintType => DoomableType.Death;
    public string RoleName => "Zombie Leader";
    public string RoleDescription => "START AN APOCOLYPSE";
    public string RoleLongDescription => "Convert Dead Players Into Zombies!";
    
    public string GetAdvancedDescription() { return RoleLongDescription + MiscUtils.AppendOptionsText(base.GetType()); }

    public List<CustomButtonWikiDescription> Abilities
    {
        get
        {
            return 
            [
				new($"Revive", "Revive Dead Bodies turning them into a loyal Zombie servant", 
                    TouRoleIcons.Altruist),
            ];
        }
    }

    public Color RoleColor => Colors.Zombie;
    public ModdedRoleTeams Team => ModdedRoleTeams.Custom;
    public RoleBehaviour AppearAs => (RoleBehaviour)RoleId.Get<ZombieLeaderRole>();
    public bool IsGuessable => Helpers.GetAlivePlayers().Count > 3;
    public CustomRoleConfiguration Configuration => new(this)
    {
        IconTmp = TmpSpriteUtils.CreateSpriteAsset(JamRoleIcons.ZombieLeader.LoadAsset(), "JackOfAllMods.Roles.Neutral.ZombieLeader", 1.45f),
        IntroSound = TouAudio.ScreamIntro,
        Icon = JamRoleIcons.ZombieLeader,
        GhostRole = (RoleTypes)RoleId.Get<NeutralGhostRole>()
    };

    public RoleBehaviour CrewVariant => (RoleBehaviour)RoleId.Get<AltruistRole>();

    public override void Initialize(PlayerControl player)
    {
        RoleBehaviourStubs.Initialize(this, player);

        Player.RpcAddModifier<ZombieLeaderRevealedModifier>();
    }
    float timer;
    public void Update()
    {
        if (Player == null || Player.Data.IsDead) return;

        List<DeadBody> bodiesInRange = Helpers.GetNearestDeadBodies(Player.transform.position, ShipStatus.Instance.MaxLightRadius * 100, Helpers.CreateFilter(Constants.NotShipMask));
        bodiesInRange.RemoveAll(b => !(MiscUtils.PlayerById(b.ParentId).GetRoleWhenAlive() is ZombieRole));

        if(bodiesInRange.Count > 0)
        {
            for(int i = bodiesInRange.Count - 1; i >= 0; i--)
            {
                var body = bodiesInRange[i];

                if(Helpers.GetAlivePlayers().Any(p => p.PlayerId == body.ParentId))
                {
                    body.ClearBody();
                }
            }

            if(timer <= 0)
            {
                PlayerControl player = MiscUtils.PlayerById(bodiesInRange[0].ParentId);


                ReviveUtilities.RevivePlayer(
                    reviver: Player,
                    revived: MiscUtils.PlayerById(bodiesInRange[0].ParentId),
                    position: bodiesInRange[0].TruePosition,
                    roleWhenAlive: DestroyableSingleton<RoleManager>.Instance.GetRole((RoleTypes)RoleId.Get<ZombieRole>()),
                    flashColor: RoleColor,
                    revivedOwnerNotificationText: "You Are Now A Zombie",
                    reviverOwnerNotificationText: "You Have Successfully Revived A Player... Kinda",
                    notificationIcon: JamRoleIcons.Zombie.LoadAsset()
                );

                timer = OptionGroupSingleton<ZombieOptions>.Instance.ZombieReviveTimer;
            }
            else
            {
                timer -= Time.deltaTime;
            }
        }
        else
        {
            timer = OptionGroupSingleton<ZombieOptions>.Instance.ZombieReviveTimer;
        }

        if(!Player.Data.IsDead)
        {
            int numNonZombies = Helpers.GetAlivePlayers().Count(p => p.Data.Role is not ZombieRole && p.Data.Role is not ZombieLeaderRole);
            IEnumerable<NetworkedPlayerInfo> zombies = PlayerControl.AllPlayerControls.ToArray().Where(p => p.Data.Role is ZombieRole || p.Data.Role is ZombieLeaderRole).Select(p => p.Data);

            if(numNonZombies < zombies.Count() && MiscUtils.KillersAliveCount == 0)
            {
                Info("Zombies Should Win!");
                CustomGameOver.Trigger<ZombieGameOver>(zombies);
            }
        }
    }

    public bool WinConditionMet()
    {
        if (Player.Data.IsDead) return false;

        int numNonZombies = Helpers.GetAlivePlayers().Count(p => p.Data.Role is not ZombieRole && p.Data.Role is not ZombieLeaderRole);
        IEnumerable<NetworkedPlayerInfo> zombies = PlayerControl.AllPlayerControls.ToArray().Where(p => p.Data.Role is ZombieRole || p.Data.Role is ZombieLeaderRole).Select(p => p.Data);

        return numNonZombies < zombies.Count() && MiscUtils.KillersAliveCount == 0;
    }


    public override bool DidWin(GameOverReason gameOverReason)
    {
        return WinConditionMet() && gameOverReason != GameOverReason.CrewmatesByTask && gameOverReason != GameOverReason.CrewmatesByVote;
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

    public bool ContinuesGame
    {
        get
        {
            bool killersAlive = TownOfUs.Utilities.MiscUtils.KillersAliveCount > 0;
            bool canGetDeadBody = Helpers.GetNearestDeadBodies(Player.transform.position, ShipStatus.Instance.MaxLightRadius * 100, Helpers.CreateFilter(Constants.NotShipMask)).Count > 0;

            return killersAlive || hasZombies || canGetDeadBody
                || Helpers.GetAlivePlayers().Any(p => p.Data.Role is SurvivorRole);
        }
    }
}
