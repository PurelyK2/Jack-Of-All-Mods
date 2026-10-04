using AmongUs.GameOptions;
using UnityEngine;
using Il2CppInterop.Runtime.Attributes;
using MiraAPI.Hud;
using MiraAPI.GameOptions;
using MiraAPI.Utilities.Assets;
using MiraAPI.Patches.Stubs;
using MiraAPI.Roles;
using TownOfUs.Buttons.Impostor;
using TownOfUs;
using TownOfUs.Modules.Wiki;
using TownOfUs.Utilities;
using TownOfUs.Assets;
using TownOfUs.Roles;
using TownOfUs.Roles.Crewmate;
using TownOfUs.Extensions;
using JAM.Options.Roles.Impostor;
using JAM.Assets;

namespace JAM.Roles.Impostor;

public sealed class RevenantRole(IntPtr cppPtr) : ImpostorRole(cppPtr), ITownOfUsRole, IWikiDiscoverable, IDoomable
{
    // public RoleBehaviour CrewVariant => RoleManager.Instance.GetRole((RoleTypes)RoleId.Get<HunterRole>());
    public Color RoleColor => TownOfUsColors.Impostor;
    public string RoleName => "Revenant";
    public ModdedRoleTeams Team => ModdedRoleTeams.Impostor;
    public RoleAlignment RoleAlignment => RoleAlignment.ImpostorKilling;
    public DoomableType DoomHintType => DoomableType.Perception;

    public string RoleDescription => "Use your ability to turn into a ghost!";
    public string RoleMedDescriptionLocale => "Use your abiliy to turn into a ghost.";
    public string RoleLongDescription => "You can Detatch to temporirly turn into a ghost";
    public string GetAdvancedDescription()
    {
        return
            $"The Revenant is a Impostor Killing that can use their Detatch ability to temporarily turn into a ghost." +
            MiscUtils.AppendOptionsText(GetType());
    }

    public CustomRoleConfiguration Configuration => new(this)
    {
        IconTmp = TmpSpriteUtils.CreateSpriteAsset(JamRoleIcons.Revenant.LoadAsset(), "Revenant", 1.45f),
        OptionsScreenshot = TouBanners.ImpostorRoleBanner,
        Icon = JamRoleIcons.Revenant
    };

    public override void Initialize(PlayerControl player)
    {
        RoleBehaviourStubs.Initialize(this, player);
    }

    [HideFromIl2Cpp]
    public List<CustomButtonWikiDescription> Abilities
    {
        get
        {
            return
            [
                new($"Detatch", $"Detatch Changes the Appearance of every player making them gray and with  you can see everyone's names",
                    JamAssets.DetachButton),
            ];
        }
    }
}
