using TownOfUs.Modules;
using MiraAPI.Events;
using JAM.Roles.Neutral;
using HarmonyLib;
using MiraAPI.Events.Vanilla.Meeting.Voting;

namespace JAM.Patches;

public static class ZombiePatches
{
    [RegisterEvent(0)]
    public static void HandleVoteEvent(HandleVoteEvent @event)
    {
        ZombieLeaderRole? leader = @event.VoteData.Owner.Data.Role as ZombieLeaderRole;
        if (leader != null)
        {
            @event.VoteData.SetRemainingVotes(0);
            foreach(PlayerControl player in PlayerControl.AllPlayerControls.ToArray().Where(p => p.GetRoleWhenAlive() is ZombieRole || p.GetRoleWhenAlive() is ZombieLeaderRole))
            {
                @event.VoteData.VoteForPlayer(@event.TargetId);
            }
            @event.Cancel();
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
