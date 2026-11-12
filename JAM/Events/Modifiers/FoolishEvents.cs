using HarmonyLib;
using Innersloth.Assets;
using JAM.Modifiers;
using JAM.Modifiers.Role;
using JAM.Options.Modifiers;
using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Meeting;
using MiraAPI.Events.Vanilla.Meeting.Voting;
using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using MiraAPI.Voting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JAM.Events.Modifiers;

public static class FoolishEvents
{
    [HarmonyPatch(typeof(PlayerVoteData), "VoteForPlayer")]
    public static class FoolishVote
    {
        [HarmonyPriority(Priority.Last)]
        public static void Postfix(ref PlayerVoteData __instance, byte playerId)
        {
            if (!__instance.Owner.HasModifier<FoolishModifier>()) return;

            Info("Voting Foolishly");

            int currVotes = __instance.Votes.Count;
            for(int i = 0; i < currVotes; i++)
            {
                __instance.Votes.Add(new CustomVote(__instance.Owner.PlayerId, playerId));
            }
        }
    }
}
