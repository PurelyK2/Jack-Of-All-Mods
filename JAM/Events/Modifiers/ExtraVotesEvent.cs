using HarmonyLib;
using JAM.Modifiers.Role;
using MiraAPI.Events;
using MiraAPI.Modifiers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JAM.Events.Modifiers;

public sealed class ExtraVotesEvents
{
    [RegisterEvent(0)]
    public static void HandleVoteEvent(MiraAPI.Events.Vanilla.Meeting.Voting.HandleVoteEvent @event)
    {
        if (@event.Player.HasModifier<ExtraVotesModifier>())
        {
            @event.VoteData.SetRemainingVotes(0);
            for (int i = 0; i < @event.Player.GetModifier<ExtraVotesModifier>().NumVotes + 1; i++)
            {
                @event.VoteData.VoteForPlayer(@event.TargetId);
            }
            @event.Cancel();
        }
    }

    [HarmonyPatch(typeof(MiraAPI.Modifiers.ModifierExtensions), "RpcAddModifier", [typeof(PlayerControl), typeof(uint), typeof(object[])])]
    public static class AddVotesModifierFix
    {
        public static bool Prefix(ref PlayerControl target, uint typeId)
        {
            if(typeId == ModifierManager.GetModifierTypeId(typeof(ExtraVotesModifier)))
            {
                if(target.HasModifier<ExtraVotesModifier>())
                {
                    (target.GetModifier<ExtraVotesModifier>() as ExtraVotesModifier)?.NumVotes += 1;
                    return false;
                }
            }
            return true;
        }
    }
}
