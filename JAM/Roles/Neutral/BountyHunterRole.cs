using AmongUs.GameOptions;
using JAM.Assets;
using JAM.Modifiers.Role;
using JAM.Options.Roles.Neutral;
using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using MiraAPI.Roles;
using MiraAPI.Utilities;
using MiraAPI.Utilities.Assets;
using Reactor.Networking.Attributes;
using TownOfUs.Assets;
using TownOfUs.Extensions;
using TownOfUs.Modifiers.Crewmate;
using TownOfUs.Modules;
using TownOfUs.Modules.Wiki;
using TownOfUs.Roles;
using TownOfUs.Roles.Neutral;
using TownOfUs.Utilities;
using UnityEngine;

namespace JAM.Roles.Neutral;

// IAmBoogarz gave the role idea
public sealed class BountyHunterRole(IntPtr cppPtr) : NeutralRole(cppPtr), ITownOfUsRole, IWikiDiscoverable, IDoomable
{
    public string RoleName => "Bounty Hunter";
    public DoomableType DoomHintType => DoomableType.Relentless;
    public string RoleDescription => "Chose Who Dies";
    public string RoleMedDescription => "Put A Target On Someone. If Anyone Kills Them, They Get A Reward.";
    public string RoleLongDescription => "Pick A Person To Be Targeted Next Round And Get Others To Kill Them";
    public string GetAdvancedDescription() { return "During The Meeting, Pick A Person To Be Targeted Next Round. If The Targeted Person Is Killed, Their Killer Gets A Predetermined \"Reward\".\n" + TownOfUs.Utilities.MiscUtils.AppendOptionsText(base.GetType()); }
    public RoleAlignment RoleAlignment => RoleAlignment.NeutralOutlier;
    public ModdedRoleTeams Team => ModdedRoleTeams.Custom;

    public CustomRoleConfiguration Configuration => new(this)
    {
        IconTmp = TmpSpriteUtils.CreateSpriteAsset(JamRoleIcons.BountyHunter.LoadAsset(), "JackOfAllMods.Roles.Neutral.BountyHunter", 1.45f),
        IntroSound = TouAudio.SinisterIntro,
        Icon = JamRoleIcons.BountyHunter,
        GhostRole = (RoleTypes)RoleId.Get<NeutralGhostRole>()
    };

    public Color RoleColor => Colors.BountyHunter;

    #region Meeting Stuff
    private MeetingMenu? meetingMenu;
    private NetworkedPlayerInfo? selectedPlr;
    public override void OnMeetingStart()
    {
        meetingMenu = new MeetingMenu(
            Player.Data.Role,
            Click,
            MeetingAbilityType.Toggle,
            JamAssets.BountyTarget,
            TouAssets.Guess,
            IsExempt,
            Color.white)
        {
            Position = new Vector3(-0.40f, 0f, -3f)
        };

        var meeting = MeetingHud.Instance;
        if (Player.AmOwner && meeting != null)
        {
            meetingMenu.GenButtons(meeting,
                Player.AmOwner && !Player.HasDied() && !Player.HasModifier<JailedModifier>());
            if (selectedPlr != null)
            {
                meetingMenu.Actives[selectedPlr.PlayerId] = true;
            }
        }
    }

    public void OnVotingComplete()
    {
        if (Player.AmOwner)
        {
            meetingMenu?.HideButtons();
        }

        if(selectedPlr != null && !selectedPlr.IsDead)
            AssignBountyTarget(selectedPlr);
    }

    public void Click(PlayerVoteArea voteArea, MeetingHud __)
    {
        var player = GameData.Instance.GetPlayerById(voteArea.PlayerId);

        if (selectedPlr == player)
        {
            selectedPlr = null;
            meetingMenu?.Actives[voteArea.PlayerId] = false;
            return;
        }

        if (selectedPlr != null)
        {
            meetingMenu?.Actives[selectedPlr.PlayerId] = false;
            selectedPlr = null;
        }

        meetingMenu?.Actives[voteArea.PlayerId] = true;
        selectedPlr = player;
    }

    private bool IsExempt(PlayerVoteArea voteArea)
    {
        return voteArea.AmDead || voteArea.GetPlayer().AmOwner;
    }
    #endregion

    static void AssignBountyTarget(NetworkedPlayerInfo player)
    {
        PlayerControl? targetedPlayer = PlayerControl.AllPlayerControls.ToArray().First(p => p.Data.PlayerId == player?.PlayerId);

        if(targetedPlayer == null)
        {
            Error("No Bounty Target");
            return;
        }

        targetedPlayer.RpcAddModifier<BountyTargetModifier>();
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

    public int NumBountiesCollected;
    public bool WinConditionMet()
    {
        Info(NumBountiesCollected >= (int)OptionGroupSingleton<BountyHunterOptions>.Instance.BountiesToWin);

        return NumBountiesCollected >= (int)OptionGroupSingleton<BountyHunterOptions>.Instance.BountiesToWin;
    }

    [MethodRpc((uint) JAMRpcCalls.BountyCollected)]
    public void RpcAddBountyCollected()
    {
        NumBountiesCollected++;
    }

    public override bool DidWin(GameOverReason gameOverReason)
    {
        return WinConditionMet();
    }
}
