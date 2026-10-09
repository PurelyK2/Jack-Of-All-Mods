using AmongUs.GameOptions;
using JAM.Assets;
using JAM.Options.Roles.Neutral;
using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using MiraAPI.Roles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TownOfUs.Assets;
using TownOfUs.Extensions;
using TownOfUs.Interfaces;
using TownOfUs.Modifiers.Game.Alliance;
using TownOfUs.Modules.Wiki;
using TownOfUs.Roles;
using TownOfUs.Roles.Neutral;
using UnityEngine;
using static JAM.Modules.IndeciveDecideMinigame;

namespace JAM.Roles.Neutral;

public sealed class IndecisiveRole(IntPtr cppPtr) : NeutralRole(cppPtr), ITownOfUsRole, IWikiDiscoverable, IDoomable, IUnlovable
{
    public int SafeShotsLeft = (int)OptionGroupSingleton<IndecisiveOptions>.Instance.SafeShots;
    public string RoleName => "Indecisive";
    public string RoleDescription => "Pick Your Team";
    public string RoleMedDescription => "Decide Who You Want To Win With";
    public string RoleLongDescription
    {
        get
        {
            string descAddOn = OptionGroupSingleton<IndecisiveOptions>.Instance.RoleStyle == IndecisiveOptions.IndecisiveStyle.Decider
                        ? "You Cannot Win Before Selecting" : "Current Alignment: " + (Player.HasModifier<EgotistModifier>() ? "Non-Crew" : "Crewmate") + "\nShots Until Team Is Forced: " + SafeShotsLeft;
            return "Select A Team To Win With.\n" + descAddOn;
        }
    }
    public string GetAdvancedDescription() { return RoleLongDescription + TownOfUs.Utilities.MiscUtils.AppendOptionsText(base.GetType()); }
    public DoomableType DoomHintType => DoomableType.Perception;
    public RoleAlignment RoleAlignment => OptionGroupSingleton<IndecisiveOptions>.Instance.RoleStyle == IndecisiveOptions.IndecisiveStyle.Decider ? RoleAlignment.NeutralBenign : RoleAlignment.NeutralOutlier;
    public Color RoleColor => Color.grey;
    public bool IsUnlovable => true;
    public ModdedRoleTeams Team => ModdedRoleTeams.Custom;
    public CustomRoleConfiguration Configuration => new(this)
    {
        IntroSound = TouAudio.OtherIntroSound,
        Icon = TouModifierIcons.Colorblind,
        GhostRole = (RoleTypes)RoleId.Get<NeutralGhostRole>()
    };

    public override bool DidWin(GameOverReason gameOverReason)
    {
        return OptionGroupSingleton<IndecisiveOptions>.Instance.RoleStyle == IndecisiveOptions.IndecisiveStyle.Killer && DestroyableSingleton<RoleManager>.Instance.GetRole(RoleTypes.Crewmate).DidWin(gameOverReason);
    }
}
