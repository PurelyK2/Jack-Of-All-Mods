using AmongUs.GameOptions;
using Il2CppInterop.Runtime.Attributes;
using JAM.Assets;
using JAM.Modifiers;
using JAM.Modifiers.Role;
using JAM.Options.Roles.Impostor;
using JAM.Options.Roles.Neutral;
using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using MiraAPI.Patches.Stubs;
using MiraAPI.Roles;
using MiraAPI.Utilities;
using MiraAPI.Utilities.Assets;
using Reactor.Networking.Attributes;
using TownOfUs;
using TownOfUs.Assets;
using TownOfUs.Extensions;
using TownOfUs.Modifiers;
using TownOfUs.Modifiers.Crewmate;
using TownOfUs.Modules;
using TownOfUs.Modules.Components;
using TownOfUs.Modules.Wiki;
using TownOfUs.Networking;
using TownOfUs.Roles;
using TownOfUs.Roles.Neutral;
using TownOfUs.Utilities;
using UnityEngine;

namespace JAM.Roles.Impostor;

public sealed class ScrubberRole(IntPtr cppPtr) : ImpostorRole(cppPtr), ITownOfUsRole, IWikiDiscoverable, IDoomable
{
    public DoomableType DoomHintType => DoomableType.Fearmonger;
    public string LocaleKey => "Scrubber";
    public string RoleName => "Scrubber";
    public string RoleDescription => "No Modifiers For You!";
    public string RoleMedDescription => RoleDescription;
    public string RoleLongDescription => "Remove Modifiers From Crewmates To Make The Impostors' Lives Easier!";
    public string GetAdvancedDescription() { return RoleLongDescription + MiscUtils.AppendOptionsText(base.GetType()); }

    [HideFromIl2Cpp]
    public List<CustomButtonWikiDescription> Abilities
    {
        get
        {
            return new List<CustomButtonWikiDescription>
            {
				new("Cleanse", "Cleanse A Player Of All Their Modifiers", JamRoleIcons.Scrubber),
            };
        }
    }

    public Color RoleColor => TownOfUsColors.Impostor;
    public ModdedRoleTeams Team => ModdedRoleTeams.Impostor;
    public RoleAlignment RoleAlignment => RoleAlignment.ImpostorSupport;

    public CustomRoleConfiguration Configuration => new(this)
    {
        IconTmp = TmpSpriteUtils.CreateSpriteAsset(JamRoleIcons.Scrubber.LoadAsset(), "JackOfAllMods.Roles.Impostor.Scrubber", 1.45f),
        IntroSound = TouAudio.JanitorCleanSound,
        Icon = JamRoleIcons.Scrubber
    };

    #region Meeting Stuff
    private MeetingMenu? meetingMenu;
    private NetworkedPlayerInfo? selectedPlr;
    public override void OnMeetingStart()
    {
        if (OptionGroupSingleton<ScrubberOptions>.Instance.ScrubsInRounds) return;

        meetingMenu = new MeetingMenu(
            Player.Data.Role,
            Click,
            MeetingAbilityType.Toggle,
            JamRoleIcons.Scrubber,
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

        if (selectedPlr != null && !selectedPlr.IsDead)
            ScrubPlayerModifiers(selectedPlr);

        selectedPlr = null;
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
        return voteArea.AmDead || voteArea.GetPlayer().AmOwner || voteArea.GetPlayer().HasModifier<ScrubbedModifier>();
    }
    #endregion

    void ScrubPlayerModifiers(NetworkedPlayerInfo netPlayer)
    {
        if (netPlayer.IsDead) return;

        PlayerControl player = netPlayer.Role.Player;

        player.RpcAddModifier<ScrubberScrubModifier>();
    }
}
