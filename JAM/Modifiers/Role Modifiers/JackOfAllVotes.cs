using JAM.Options.Modifiers.UniversalModifierOptions;
using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using MiraAPI.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TownOfUs.Interfaces;
using TownOfUs.Roles;
using TownOfUs.Utilities;
using UnityEngine;

namespace JAM.Modifiers.Crewmate;

internal class JackOfAllVotes : BaseModifier, IContinuesGame
{
    public int NumVotes = 1;
    public override string ModifierName => "Extra Votes!";

    public bool ContinuesGame //Same As Tiebreaker
    {
        get
        {
            if (Player.HasDied() || Player.IsImpostorAligned())
            {
                return false;
            }
            if ((!Player.IsCrewmate() || Helpers.GetAlivePlayers().Count(x => x.IsCrewmate()) == 1) &&
                Player.Data.Role is ITownOfUsRole touRole &&
                touRole.RoleAlignment is not RoleAlignment.NeutralKilling && Helpers.GetAlivePlayers().Count < 4 &&
                Helpers.GetAlivePlayers().Count > 1)
            {
                return touRole.CanModifierContinueGame(this);
            }

            return false;
        }
    }

    public override string GetDescription()
    {
        return "You Have +" + NumVotes + " Votes In Meetings! (can stack)";
    }
}
