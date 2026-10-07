using AmongUs.GameOptions;
using MiraAPI.Roles;
using MiraAPI.Translation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TownOfUs.Modifiers;
using TownOfUs.Modules;
using TownOfUs.Roles.Crewmate;

namespace JAM.Modifiers;

public sealed class BountyRevealModifier : BaseRevealModifier
{
    RoleBehaviour? role;
    public override string ModifierName => "Role Revealed";

    public override string GetDescription()
    {
        return "Your Role Is Revealed To Everyone";
    }
    public override bool HideOnUi => false;
    public override bool AutoStart => true;
    public override ChangeRoleResult ChangeRoleResult => ChangeRoleResult.Nothing;
    public override RoleBehaviour? ShownRole => role;
    public override bool RevealRole => true;
    public override bool Visible => true;

    public override void OnActivate()
    {
        base.OnActivate();
        role = Player.GetRoleWhenAlive();
        ShownRole = role;

        SetNewInfo(true, null);
    }
}
