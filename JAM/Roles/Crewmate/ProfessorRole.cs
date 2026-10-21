using AmongUs.GameOptions;
using Il2CppInterop.Runtime.Attributes;
using JAM.Assets;
using JAM.Modifiers.Role;
using JAM.Options.Roles.Crewmate;
using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using MiraAPI.Roles;
using MiraAPI.Utilities;
using MiraAPI.Utilities.Assets;
using TownOfUs.Assets;
using TownOfUs.Extensions;
using TownOfUs.Modifiers.Crewmate;
using TownOfUs.Modules;
using TownOfUs.Modules.Wiki;
using TownOfUs.Roles;
using TownOfUs.Roles.Crewmate;
using TownOfUs.Utilities;
using UnityEngine;

namespace JAM.Roles.Crewmate;

public sealed class ProfessorRole(IntPtr cppPtr) : CrewmateRole(cppPtr), ITownOfUsRole, IWikiDiscoverable, IDoomable, ICrewVariant
{
    public int Uses = (int)OptionGroupSingleton<ProfessorOptions>.Instance.MaxUses;
    public int TasksUntilUse = (int)OptionGroupSingleton<ProfessorOptions>.Instance.TasksPerUse;

    public DoomableType DoomHintType => DoomableType.Protective;
    public string RoleName => "Professor";
    public string RoleDescription => "Train Players To Change Their Role";
    public string RoleMedDescription => "Select Players To Make Them Change Their Role";
    public string RoleLongDescription => "You Can Pick A Player In Meetings. They Will Be Able To Change Their Role The Next Round.\n\nUses Left: " + Uses;
    public RoleAlignment RoleAlignment => RoleAlignment.CrewmatePower;
    public Color RoleColor => Colors.Professor;
    public ModdedRoleTeams Team => ModdedRoleTeams.Crewmate;

    public string GetAdvancedDescription() { return "You Can Pick A Player In Meetings. They Will Be Able To Change Their Role The Next Round." + MiscUtils.AppendOptionsText(base.GetType()); }

    [HideFromIl2Cpp]
    public List<CustomButtonWikiDescription> Abilities
    {
        get
        {
            return new List<CustomButtonWikiDescription>
            {
				new("Train", "Select A Player To Let Them Change Their Role", JamRoleIcons.Professor),
            };
        }
    }
    public CustomRoleConfiguration Configuration => new(this)
    {
        IconTmp = TmpSpriteUtils.CreateSpriteAsset(JamRoleIcons.Professor.LoadAsset(), "JackOfAllMods.Roles.Crewmate.Professor", 1.45f),
        IntroSound = TouAudio.DetectiveIntroSound,
        Icon = JamRoleIcons.Professor
    };

    public RoleBehaviour CrewVariant => RoleManager.Instance.GetRole((RoleTypes)RoleId.Get<SwapperRole>());

    #region Meeting Stuff
    private MeetingMenu? meetingMenu;
    private NetworkedPlayerInfo? selectedPlr;
    public override void OnMeetingStart()
    {
        meetingMenu = new MeetingMenu(
            Player.Data.Role,
            Click,
            MeetingAbilityType.Toggle,
            JamRoleIcons.Professor,
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
            AssignChangeRoleModifier(selectedPlr);

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
        RoleBehaviour? role = voteArea.GetPlayer()?.Data.Role;
        return voteArea.AmDead || voteArea.GetPlayer()?.AmOwner == true || (role?.IsNeutral() == true && role?.DidWin(GameOverReason.CrewmatesByTask) == true && role?.DidWin(GameOverReason.ImpostorsByKill) == true);
    }
    #endregion

    void AssignChangeRoleModifier(NetworkedPlayerInfo player)
    {
        if (player.IsDead) return;

        Uses--;
        player.Role.Player.RpcAddModifier<ProfessorRetrainModifier>();
    }
}
