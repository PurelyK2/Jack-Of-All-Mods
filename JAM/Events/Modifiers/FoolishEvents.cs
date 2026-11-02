using HarmonyLib;
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
    [RegisterEvent(0)]
    public static void OnMeetingStart(StartMeetingEvent @event)
    {
        FoolishModifier.votingTimeLeft = OptionGroupSingleton<FoolishOptions>.Instance.FoolishTime + 5; // the 5 is for the initial animation
    }

    [HarmonyPatch(typeof(PlayerVoteData), "VoteForPlayer")]
    public static class FoolishVote
    {
        static bool repeating;
        public static void Postfix(ref PlayerVoteData __instance, byte playerId)
        {
            if(repeating) return;

            if(!__instance.Owner.HasModifier<FoolishModifier>())
            {
                return;
            }

            if (FoolishModifier.votingTimeLeft <= 0)
            {
                Info("Not In Voting Time");
                return;
            }

            repeating = true;
            __instance.VoteForPlayer(playerId);
            repeating = false;
        }
    }
}
