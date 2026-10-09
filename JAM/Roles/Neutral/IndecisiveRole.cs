using AmongUs.GameOptions;
using JAM.Assets;
using MiraAPI.Roles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TownOfUs.Assets;
using TownOfUs.Extensions;
using TownOfUs.Interfaces;
using TownOfUs.Modules.Wiki;
using TownOfUs.Roles;
using TownOfUs.Roles.Neutral;
using UnityEngine;
using static JAM.Modules.IndeciveDecideMinigame;

namespace JAM.Roles.Neutral;

public sealed class IndecisiveRole(IntPtr cppPtr) : NeutralRole(cppPtr), ITownOfUsRole, IWikiDiscoverable, IDoomable, IUnlovable
{
    public string RoleName => "Indecisive";
    public string RoleDescription => "Pick Your Team";
    public string RoleMedDescription => "Decide Who You Want To Win With";
    public string RoleLongDescription => "Select A Team To Win With.\nYou Cannot Win Before Selecting";
    public string GetAdvancedDescription() { return RoleLongDescription + TownOfUs.Utilities.MiscUtils.AppendOptionsText(base.GetType()); }

    public DoomableType DoomHintType => DoomableType.Perception;

    public RoleAlignment RoleAlignment => RoleAlignment.NeutralBenign;

    public Color RoleColor => Color.grey;

    public ModdedRoleTeams Team => ModdedRoleTeams.Custom;

    public CustomRoleConfiguration Configuration => new(this)
    {
        IntroSound = TouAudio.OtherIntroSound,
        Icon = TouModifierIcons.Colorblind,
        GhostRole = (RoleTypes)RoleId.Get<NeutralGhostRole>()
    };

    public bool IsUnlovable => true;

    public bool WinConditionMet()
    {
        return false;
    }
    public override bool DidWin(GameOverReason gameOverReason)
    {
        return false;
    }
}
