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

namespace JAM.Roles.Neutral;

public sealed class VacillatorRole(IntPtr cppPtr) : NeutralRole(cppPtr), ITownOfUsRole, IWikiDiscoverable, IDoomable, IUnlovable
{
    public bool winsWithCrew = true;

    public int SafeShotsLeft = (int)OptionGroupSingleton<VacillatorOptions>.Instance.SafeShots;
    public string RoleName => "Vacillator";
    public string RoleDescription => "Decide Who You Want To Win With";
    public string RoleMedDescription => "Kill Players To Decide Who You Win With!";
    public string RoleLongDescription
    {
        get
        {
            return "Select A Team To Win With.\nCurrent Alignment: " + (winsWithCrew ? "Crewmate" : "Non-Crew") + "\n\nShots Until Team Is Forced: " + SafeShotsLeft;
        }
    }
    public string GetAdvancedDescription() { return RoleMedDescription + TownOfUs.Utilities.MiscUtils.AppendOptionsText(base.GetType()); }
    public DoomableType DoomHintType => DoomableType.Perception;
    public RoleAlignment RoleAlignment => RoleAlignment.NeutralOutlier;
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
        return DestroyableSingleton<RoleManager>.Instance.GetRole(RoleTypes.Crewmate).DidWin(gameOverReason) == winsWithCrew;
    }
}
