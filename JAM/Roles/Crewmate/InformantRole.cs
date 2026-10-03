using Il2CppInterop.Runtime.Attributes;
using JAM.Assets;
using JAM.Options.Roles.Crewmate;
using MiraAPI.GameOptions;
using MiraAPI.Roles;
using MiraAPI.Utilities;
using MiraAPI.Utilities.Assets;
using TownOfUs.Assets;
using TownOfUs.Extensions;
using TownOfUs.Modules;
using TownOfUs.Modules.Wiki;
using TownOfUs.Roles;
using TownOfUs.Utilities;
using UnityEngine;

namespace JAM.Roles.Crewmate;

public sealed class InformantRole(IntPtr cppPtr) : CrewmateRole(cppPtr), ITownOfUsRole, IWikiDiscoverable, IDoomable
{
    static bool OnlyOneType = true;

    public DoomableType DoomHintType => DoomableType.Insight;
    public string RoleName => "Informant";
    public string RoleDescription => "Obtain Hidden Information";
    public string RoleMedDescription => "Investigate players, then find out info about them in meetings.";
    public string RoleLongDescription => "Investigate players, then learn about them in the meeting.";

    public string GetAdvancedDescription() { return RoleLongDescription + MiscUtils.AppendOptionsText(base.GetType()); }

    [HideFromIl2Cpp]
    public List<CustomButtonWikiDescription> Abilities
    {
        get
        {
            return new List<CustomButtonWikiDescription>
            {
				new("Investigate", "Select A Player To Discover Information About In The Meeting", TouRoleIcons.Forensic),
            };
        }
    }
    
        public static void GenerateInfo(PlayerControl player, List<RoleBehaviour> randomRolesList)
    {
        string alertString = "Information Has Been Spread About " + player.Data.PlayerName + "! View Details In The Chat!";
        MiraAPI.Utilities.Helpers.CreateAndShowNotification(alertString, Colors.Informant, new Vector3(0f, 1f, -20f), null, TouRoleIcons.Forensic.LoadAsset());

        string InformantString = "";

        randomRolesList.Shuffle();

        foreach(string roleName in randomRolesList.Select(role => role.GetRoleName()))
        {
            InformantString += ", #" + roleName.Replace(" ", "-");
        }

        InformantString = InformantString.Substring(2);
        InformantString = player.Data.PlayerName + " is one of the following roles:\n" + InformantString;

        MiscUtils.AddFakeChat(player.Data, "Informant Information:", InformantString, false, true);
    }
    
        public Color RoleColor => Colors.Informant;
        public ModdedRoleTeams Team => ModdedRoleTeams.Crewmate;
        public RoleAlignment RoleAlignment => RoleAlignment.CrewmateInvestigative;

        public CustomRoleConfiguration Configuration => new(this)
    {
        IconTmp = TmpSpriteUtils.CreateSpriteAsset(JamRoleIcons.Informant.LoadAsset(), "JackOfAllMods.Roles.Crewmate.Informant", 1.45f),
        IntroSound = TouAudio.DetectiveIntroSound,
        Icon = JamRoleIcons.Informant
    };
}
