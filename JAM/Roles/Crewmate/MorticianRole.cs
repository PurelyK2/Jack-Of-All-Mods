using UnityEngine;
using MiraAPI.Modifiers;
using MiraAPI.Patches.Stubs;
using MiraAPI.Roles;
using MiraAPI.Utilities.Assets;
using MiraAPI.GameOptions;
using TownOfUs.Utilities;
using TownOfUs.Modules.Wiki;
using TownOfUs.Extensions;
using TownOfUs.Assets;
using TownOfUs.Roles;
using JAM.Options.Roles.Crewmate;
using JAM.Modifiers.Hidden;
using JAM.Assets;

namespace JAM.Roles.Crewmate;

public sealed class MorticianRole(IntPtr cppPtr) : CrewmateRole(cppPtr), ITownOfUsRole, IWikiDiscoverable, IDoomable
{
    public DoomableType DoomHintType => DoomableType.Perception;
    public string RoleName => "Mortician";
    public Color RoleColor => Colors.Mortician;
    public ModdedRoleTeams Team => ModdedRoleTeams.Crewmate;
    public RoleAlignment RoleAlignment => RoleAlignment.CrewmateSupport;
    public int AbilityUses { get; set; } 
    public float TaskProgress { get; set; }

    public CustomRoleConfiguration Configuration => new(this)
    {
        IconTmp = TmpSpriteUtils.CreateSpriteAsset(JamRoleIcons.Mortician.LoadAsset(), "Mortician", 1.45f),
        Icon = JamRoleIcons.Mortician,
        OptionsScreenshot = TouBanners.CrewmateRoleBanner,
        IntroSound = TouAudio.ScientistIntroSound
    };

    public string RoleDescription => "Reveal the dead to gain extra info!";
    public string RoleMedDescriptionLocale => "Reveal dead players during the Meeting!";
    public string RoleLongDescription => 
        $"Reveal dead roles to everyone, and report bodies to figure out the killer.\n" +
        $"Autopsy Uses: <color=#{ColorUtility.ToHtmlStringRGBA(Colors.Mortician)}>{AbilityUses}</color>";
    public string GetAdvancedDescription()
    {
        return
            "The Mortician is a Crewmate Support that can perform an autopsy during the meeting " +
            "to reveal a dead players role to everyone. Reporting bodies also gives the Mortician " +
            "the Killers Role." + 
            MiscUtils.AppendOptionsText(GetType());
    }

    public override void Initialize(PlayerControl player)
    {
        RoleBehaviourStubs.Initialize(this, player);

        AbilityUses = (int)OptionGroupSingleton<MorticianOptions>.Instance.InitialAbilityUses;
        TaskProgress = 0;

        if (!player.HasModifier<MorticianCacheModifier>())
        {
            player.AddModifier<MorticianCacheModifier>();
        }
    }
}
