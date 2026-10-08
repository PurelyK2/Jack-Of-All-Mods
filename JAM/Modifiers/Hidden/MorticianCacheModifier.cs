using MiraAPI.Modifiers;
using MiraAPI.Utilities;
using TownOfUs.Modules;
using TownOfUs.Utilities;
using TownOfUs.Modifiers;
using TownOfUs.Modifiers.Impostor.Herbalist;
using UnityEngine;
using JAM.Roles.Crewmate;
using JAM.Events.Crewmate;
using JAM.Assets;

namespace JAM.Modifiers.Hidden;

public sealed class MorticianCacheModifier : BaseModifier
{
    private MeetingMenu? _meetingMenu;

    public override string ModifierName => "Mortician";
    public override bool HideOnUi => true;
    private MeetingHud? _currentMeeting;
    private bool _usedThisMeeting;

    public override void OnActivate()
    {
        if (!Player.AmOwner)
        {
            return;
        }

        _meetingMenu = new MeetingMenu(Player.Data.Role, Click, MeetingAbilityType.Click,
            JamAssets.MorticianAbility, exemption: IsExempt, activeColor: Color.white, hoverColor: new Color(0.8f, 0.8f, 0.8f))
        {
            Position = new Vector3(-0.40f, 0f, -3f)
        };
    }

    public void Click(PlayerVoteArea voteArea, MeetingHud meeting)
    {   
        if (_usedThisMeeting ||
            !Player.AmOwner ||
            Player.Data.Role is not MorticianRole mortician ||
            mortician.AbilityUses <= 0)
        {
            return;
        }

        var target = GameData.Instance.GetPlayerById(voteArea.PlayerId)?.Object;

        if (target == null ||
            voteArea.PlayerId == Player.PlayerId ||
            !voteArea.AmDead ||
            (target.HasModifier<BaseRevealModifier>() &&
            !target.HasModifier<HerbalistExposedModifier>()))
        {
            return;
        }

        target!.RpcAddModifier<MorticianRevealModifier>();

        MorticianEvents.RpcMorticianAutopsyNotification(Player, target.PlayerId);

        mortician.AbilityUses--;
        _usedThisMeeting = true;
        _meetingMenu?.HideButtons();
    }

    public override void OnMeetingStart()
    {
        var meeting = MeetingHud.Instance;

        if (meeting == null)
        {
            return;
        }

        if (_currentMeeting != meeting)
        {
            _currentMeeting = meeting;
            _usedThisMeeting = false;
        }

        if (!Player.AmOwner || _meetingMenu == null)
        {
            return;
        }

        if (Player.Data.Role is not MorticianRole mortician)
        {
            return;
        }

        var usable = mortician.AbilityUses > 0 && !Player.HasDied();

        _meetingMenu.GenButtons(meeting, usable);
    }

    public void OnVotingComplete()
    {
        _meetingMenu?.HideButtons();
    }

    public override void OnDeactivate()
    {
        _meetingMenu?.Dispose();
        _meetingMenu = null;
    }

    private bool IsExempt(PlayerVoteArea voteArea)
    {
        var player = GameData.Instance.GetPlayerById(voteArea.PlayerId);

        if (player == null || player.Object == null || voteArea.PlayerId == Player.PlayerId || !voteArea.AmDead)
        {
            return true;
        }

        // if (player == null || player.Object == null || voteArea.PlayerId == Player.PlayerId ||
        //     player.Object.Data.Disconnected || !voteArea.AmDead)
        // {
        //     return true;
        // }

        var target = player.Object;

        //Makes roles that are already revealed Exempt from being revealed again
        if (target.HasModifier<BaseRevealModifier>() &&
            !target.HasModifier<HerbalistExposedModifier>())
        {
            return true;
        }

        return false;
    }
}
