using AmongUs.GameOptions;
using JAM.Assets;
using JAM.Options.Roles.Neutral;
using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using MiraAPI.Roles;
using MiraAPI.Utilities;
using MiraAPI.Utilities.Assets;
using TownOfUs.Assets;
using TownOfUs.Extensions;
using TownOfUs.Modules;
using TownOfUs.Modules.Wiki;
using TownOfUs.Modifiers.Game.Assailant;
using TownOfUs.Roles;
using TownOfUs.Roles.Crewmate;
using TownOfUs.Roles.Neutral;
using TownOfUs.Utilities;
using UnityEngine;

namespace JAM.Roles.Neutral;

public sealed class TimeKeeperRole(IntPtr cppPtr) : NeutralRole(cppPtr), ITownOfUsRole, IWikiDiscoverable, IDoomable, ICrewVariant
{
    public int numMeetingsSkipped = -1;

    public string RoleName => "Time Keeper";
    public string LocaleKey => "Time Keeper";
    public DoomableType DoomHintType => DoomableType.Fearmonger;
    public string RoleDescription => "You are patient, but irritated...";
    public string RoleMedDescription => "Decrease Your Cooldowns Whenever A Meeting Is Skipped Or Tied";
    public string RoleLongDescription => "Decrease Your Cooldowns Whenever A Meeting Is Skipped Or Tied.";
    public string GetAdvancedDescription() { return RoleLongDescription + MiscUtils.AppendOptionsText(base.GetType()); }

    public Color RoleColor => Colors.TimeKeeper;
    public ModdedRoleTeams Team => ModdedRoleTeams.Custom;
    public RoleAlignment RoleAlignment => RoleAlignment.NeutralKilling;
    public CustomRoleConfiguration Configuration => new(this)
    {
        IconTmp = TmpSpriteUtils.CreateSpriteAsset(JamRoleIcons.TimeKeeper.LoadAsset(), "JackOfAllMods.Roles.Neutral.TimeKeeper", 1.45f),
        IntroSound = TouAudio.SinisterIntro,
        Icon = JamRoleIcons.TimeKeeper,
        CanUseVent = true,
        GhostRole = (RoleTypes)RoleId.Get<NeutralGhostRole>()
    };
    public RoleBehaviour CrewVariant => RoleManager.Instance.GetRole((RoleTypes)RoleId.Get<SheriffRole>());
    public override void OnVotingComplete()
    {
        if (MeetingHud.Instance.exiledPlayer == null)
        {
            if(!Player.HasModifier<AssassinModifier>())
            {
                Player.RpcAddModifier<AssassinModifier>();
            }
            numMeetingsSkipped++;

            if (!OptionGroupSingleton<TimeKeeperOptions>.Instance.TimeNotif) return;

            if(numMeetingsSkipped < 0)
            {
                numMeetingsSkipped++;

                Helpers.CreateAndShowNotification(
                    "There is no decision, a killer has awoken...",
                    RoleColor,
                    new Vector3(0f, 1f, -20f),
                    null,
                    TouRoleIcons.Jackal.LoadAsset()
                );
            }
            else if(OptionGroupSingleton<TimeKeeperOptions>.Instance.AlwaysDecreaseCd)
            {
                numMeetingsSkipped++;

                Helpers.CreateAndShowNotification(
                    "The Killer Is Getting Tired Of Waiting...",
                    RoleColor,
                    new Vector3(0f, 1f, -20f),
                    null,
                    TouRoleIcons.Jackal.LoadAsset()
                );
            }
        }
    }

    public bool WinConditionMet()
    {
        if (Player.HasDied())
        {
            return false;
        }

        var aliveCount = Helpers.GetAlivePlayers().Count;
        var killersAlive = MiscUtils.KillersAliveCount;

        return aliveCount <= 2 && killersAlive == 1;
    }
    public override bool DidWin(GameOverReason gameOverReason)
    {
        return WinConditionMet();
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
