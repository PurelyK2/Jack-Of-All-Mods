using AmongUs.GameOptions;
using HarmonyLib;
using JAM.Modifiers.Hidden;
using JAM.Modifiers.Role;
using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.Modifiers;
using TownOfUs.Achievements;
using TownOfUs.Modules;
using TownOfUs.Roles.Crewmate;
using static JAM.Achievements.JamAchievements;
using static TownOfUs.Achievements.AchievementsAPIHelper;

namespace JAM.Achievements;

public static class ImpAchievementChecks
{
    // Deceiver Kill Checks
    [RegisterEvent(0)]
    public static void DeceiverKillEvent(BeforeMurderEvent @event)
    {
        DeceiverRole.confuseRole = false;
        if (!(@event.Source.AmOwner && @event.Source.Data.Role is DeceiverRole))
        {
            Info("Not Deceiver, Skip Checks");
            DeceiverRole.ReConfuse();
            return;
        }

        if(@event.Target.HasModifier<DeceiverMedicShield>() && !IsUnlocked(Psych))
        {
            Unlock(Psych);
        }
        
        DeceiverRole.ReConfuse();
    }

    // Deceiver Give Shield Achievement Check
    public static void ProgressDeceiverShields()
    {
        if(GetCurrentValue(Deceived) < 3)
        {
            SetValue(Deceived, GetCurrentValue(Deceived) + 1);
        }
        else if(!IsUnlocked(Deceived))
        {
            Unlock(Deceived);
        }
    }

    // Deceiver Guessed Achievement Check
    [HarmonyPatch(typeof(GameHistory), "RpcSetMisguessSummary")]
    public static class MisguessDeceiverPatch
    {
        public static void Postfix(byte victimId, uint guessId, bool isRole)
        {
            DeceiverRole.confuseRole = false;

            PlayerControl victim = TownOfUs.Utilities.MiscUtils.PlayerById(victimId);
            RoleBehaviour role = RoleManager.Instance.GetRole((RoleTypes)guessId);

            if(victim.AmOwner && victim.Data.Role is DeceiverRole && role is MedicRole && !IsUnlocked(GetBaited))
            {
                Unlock(GetBaited);
            }

            DeceiverRole.ReConfuse();
        }
    }
}
