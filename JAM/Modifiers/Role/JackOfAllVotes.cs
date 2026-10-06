using JAM.Options.Modifiers.UniversalModifierOptions;
using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TownOfUs.Interfaces;
using UnityEngine;

namespace JAM.Modifiers.Role;

internal class JackOfAllVotes : BaseModifier, IContinuesGame
{
    public int NumVotes = 1;
    public override string ModifierName => "Extra Votes!";

    public bool ContinuesGame => true;

    public override string GetDescription()
    {
        return "You Have +" + NumVotes + " Votes In Meetings! (can stack)";
    }
}
