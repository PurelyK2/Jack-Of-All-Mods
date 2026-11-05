using HarmonyLib;
using JAM.Modifiers.Role;
using JAM.Options.Roles.Neutral;
using JAM.Roles.Crewmate;
using JAM.Roles.Neutral;
using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using MiraAPI.Roles;
using static JAM.Achievements.JamAchievements;
using static TownOfUs.Achievements.AchievementsAPIHelper;

namespace JAM.Achievements;

public static class NeutAchievementChecks
{
    // Mimic
    [RegisterEvent(0)]
    public static void CheckDIYSwooper(AfterMurderEvent @event)
    {
        if (!(@event.Source.AmOwner && @event.Source.HasModifier<MimicAllianceModifier>()))
        {
            Info("Not Mimic, Skip Checks");
            return;
        }

        if (@event.Source.Data.Role is SnoopRole && !IsUnlocked(DIYSwooper))
        {
            Info("DIYSwooper");
            Unlock(DIYSwooper);
        }

        if (@event.Source.Data.Role.GetType() == @event.Target.Data.Role.GetType() && !IsUnlocked(ThereCanOnlyBeOne))
        {
            Info("ThereCanOnlyBeOne");
            Unlock(ThereCanOnlyBeOne);
        }
    }

    // Vacillator
    public static void MisKill(bool onCrew)
    {
        Error("MisKill: " + onCrew);
        if (onCrew && !IsUnlocked(IShotTheSheriff)) Unlock(IShotTheSheriff);
        if (!onCrew && !IsUnlocked(DespicableMe)) Unlock(DespicableMe);
    }

    // Forbearing
    public static void CheckForbearing(ForbearingRole role)
    {
        if(role.numMeetingsSkipped > -1 && !IsUnlocked(Boring))
        {
            Unlock(Boring);
        }

        if(OptionGroupSingleton<ForbearingOptions>.Instance.ForbearingCooldown - OptionGroupSingleton<ForbearingOptions>.Instance.CooldownDecrease * role.numMeetingsSkipped <= 0 && !IsUnlocked(FineIllDoItMyself))
        {
            Unlock(FineIllDoItMyself);
        }
    }

    // Zombie Leader
    public static void OnZombieLeaderRevive()
    {
        if(GetCurrentValue(Arise) < 3)
        {
            SetValue(Arise, GetCurrentValue(Arise) + 1, true);
        }
        else
        {
            Unlock(Arise);
        }
    }

    // Zombie
    [HarmonyPatch(typeof(TownOfUs.Networking.MiscTouRpcs), "RpcFullRevive")]
    public static class ZombieRevivePatch
    {
        public static void Postfix(ref PlayerControl player, ushort newRoleType)
        {
            if(player.AmOwner && newRoleType == RoleId.Get<ZombieRole>() && !IsUnlocked(Infected))
            {
                Unlock(Infected);
            }
        }
    }
}
