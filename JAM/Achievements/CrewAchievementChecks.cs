using JAM.Modifiers.Role;
using JAM.Roles.Crewmate;
using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.Modifiers;
using TownOfUs.Modifiers.Game.Crewmate;
using static JAM.Achievements.JamAchievements;
using static TownOfUs.Achievements.AchievementsAPIHelper;

namespace JAM.Achievements;

public static class CrewAchievementChecks
{
    // Jack Of All On Modifier Get Achievement Checks
    public static void CheckJoaAchievements(PlayerControl player)
    {
        List<BaseModifier> modifiers = player.GetModifiers<BaseModifier>().ToList();

        if(modifiers.Any(m => m is BaitModifier) && modifiers.Any(m => m is CelebrityModifier) && !IsUnlocked(Unkillable))
        {
            Unlock(Unkillable);
        }

        if(modifiers.FirstOrDefault(m => m is ExtraVotesModifier) is ExtraVotesModifier votes && votes.NumVotes > 1 && !IsUnlocked(DIYMayor))
        {
            Unlock(DIYMayor);
        }
    }
}
