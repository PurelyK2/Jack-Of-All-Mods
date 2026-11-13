using HarmonyLib;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using JAM.Roles.Neutral;
using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Meeting;
using MiraAPI.Events.Vanilla.Meeting.Voting;
using MiraAPI.Utilities;
using MiraAPI.Voting;
using Reactor.Utilities;
using System.Collections;
using TownOfUs.Modules;
using UnityEngine;

namespace JAM.Patches;

public static class ZombiePatches
{
    static bool VotingForZombies = false;
    [RegisterEvent(0)]
    public static void HandleVoteEvent(HandleVoteEvent @event)
    {
        VotingForZombies = false;
        var voter = TownOfUs.Utilities.MiscUtils.PlayerById(@event.Player.PlayerId);

        if(@event.VoteData.Owner.Data.Role is ZombieLeaderRole)
        {
            @event.VoteData.SetRemainingVotes(0);
            foreach (PlayerControl player in PlayerControl.AllPlayerControls.ToArray().Where(p => p.GetRoleWhenAlive() is ZombieRole))
            {
                @event.VoteData.VoteForPlayer(@event.TargetId);
            }
            @event.Cancel();
        }
    }

    // Make Zombie Votes At The End Of The Vote List
    [HarmonyPatch(typeof(MiraAPI.Voting.VotingUtils), "CalculateVotes")]
    public static class ZombieVoteOrder
    {
        public static void Postfix(ref MeetingHud __instance, ref List<CustomVote> __result)
        {
            List<byte> zombieLeaders = PlayerControl.AllPlayerControls.ToArray().Where(p => p?.Data?.Role is ZombieLeaderRole).Select(p => p?.PlayerId ?? 0).ToList();

            int firstMatchIndex = __result.FindIndex(item => zombieLeaders.Any(id => id == item.Voter));

            __result = __result.Select((item, index) => new { item, index }).OrderBy(item => zombieLeaders.Any(id => id == item.item.Voter && item.index != firstMatchIndex)).Select(x => x.item).ToList();
        }
    }

    [HarmonyPatch(typeof(MeetingHud), "BloopAVoteIcon")]
    public static class BloopAZombieVote
    {
        public static void Postfix(NetworkedPlayerInfo voterPlayer, Transform parent)
        {
            var icon = FindLatestVoteIcon(parent);
            if (icon != null)
            {
                if(voterPlayer.Role is ZombieLeaderRole)
                {
                    if (VotingForZombies)
                    {
                        PlayerMaterial.SetColors(Palette.DisabledGrey, icon);
                    }
                    else VotingForZombies = true;
                }
            }
        }
        private static SpriteRenderer? FindLatestVoteIcon(Transform parent)
        {
            SpriteRenderer? latest = null;
            for (var i = 0; i < parent.childCount; i++)
            {
                var child = parent.GetChild(i);
                if (child.name != "playerVote(Clone)")
                {
                    continue;
                }

                var rend = child.GetComponent<SpriteRenderer>();
                if (rend != null)
                {
                    latest = rend;
                }
            }

            return latest;
        }
    }

    [HarmonyPatch(typeof(ChatController), "AddChat", [typeof(PlayerControl), typeof(string), typeof(bool)])]
    public static class ZombiesChatPatch
    {
        public static void Prefix(ref PlayerControl sourcePlayer)
        {
            if (sourcePlayer.Data.Role is ZombieRole)
            {
                sourcePlayer.Data.IsDead = true;
            }
            if (PlayerControl.LocalPlayer.Data.Role is ZombieRole)
            {
                PlayerControl.LocalPlayer.Data.IsDead = true;
            }
        }

        //Only Do Postfix If You Somehow Send A Chat Outside Of A Meeting
        public static void Postfix(ref PlayerControl sourcePlayer)
        {
            if (sourcePlayer.Data.Role is ZombieRole && MeetingHud.Instance == null)
            {
                sourcePlayer.Data.IsDead = false;
            }
            if (PlayerControl.LocalPlayer.Data.Role is ZombieRole && MeetingHud.Instance == null)
            {
                PlayerControl.LocalPlayer.Data.IsDead = false;
            }
        }
    }

    // Disables Hiding Chat On Zombie Death
    [HarmonyPatch(typeof(TownOfUs.Events.GhostRoleEvents), "CoRemoveChat")]
    public static class DontHideChatFromZombies
    {
        public static bool Prefix()
        {
            if (PlayerControl.LocalPlayer.Data.Role is ZombieRole) return false;
            return true;
        }
    }

    // Zombie Vitals
    [HarmonyPatch(typeof(VitalsPanel), "SetAlive")]
    public static class ZombieVitals
    {
        public static void Postfix(ref VitalsPanel __instance)
        {
            if (__instance.PlayerInfo.Role is ZombieRole)
            {
                __instance.Cardio.SetDead();
            }
        }
    }
}
