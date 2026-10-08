using Il2CppInterop.Runtime.Attributes;
using MiraAPI.Roles;
using MiraAPI.Utilities.Assets;
using TownOfUs.Assets;
using TownOfUs.Extensions;
using TownOfUs.Modules.Wiki;
using TownOfUs.Roles;
using TownOfUs.Utilities;
using UnityEngine;

namespace JAM.Roles.Crewmate;

public sealed class SnoopRole(IntPtr cppPtr) : CrewmateRole(cppPtr), ITownOfUsRole, IWikiDiscoverable, IDoomable
{

    public DoomableType DoomHintType => DoomableType.Trickster;
    public string RoleName => "Snoop";
    public string RoleDescription => "Hide in plain sight and find the impostors";
    public string RoleMedDescription => RoleDescription;
    public string RoleLongDescription => RoleDescription;

    public string GetAdvancedDescription() { return RoleLongDescription + MiscUtils.AppendOptionsText(base.GetType()); }


    [HideFromIl2Cpp]
    public List<CustomButtonWikiDescription> Abilities
    {
        get
        {
            return new List<CustomButtonWikiDescription>
            {
				new("Sneak", "Hide in plain sight for an amount of time", TouRoleIcons.Chameleon),
            };
        }
    }


    public Color RoleColor => Colors.Snoop;

    public ModdedRoleTeams Team => ModdedRoleTeams.Crewmate;

    public RoleAlignment RoleAlignment => RoleAlignment.CrewmateInvestigative;


    public CustomRoleConfiguration Configuration => new(this)
    {
        IconTmp = TmpSpriteUtils.CreateSpriteAsset(TouRoleIcons.Chameleon.LoadAsset(), "JackOfAllMods.Roles.Crewmate.Snoop", 1.45f),
        IntroSound = TouAudio.SwooperActivateSound,
        Icon = TouRoleIcons.Chameleon
    };
}
