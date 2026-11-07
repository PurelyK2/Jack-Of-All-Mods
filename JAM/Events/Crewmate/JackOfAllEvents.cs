using JAM.Achievements;
using JAM.Modifiers.Role;
using JAM.Options.Roles.Crewmate;
using JAM.Roles.Crewmate;
using JetBrains.Annotations;
using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.Events.Vanilla.Player;
using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using TownOfUs.Modules;

namespace JAM.Events.Crewmate;

public static class JackOfAllEvents
{
	[RegisterEvent(0)]
	public static void CompleteTaskEvent(CompleteTaskEvent @event)
	{
		if (!@event.Player.AmOwner) return;

		if(@event.Player.GetRoleWhenAlive() is JackOfAllRole jackOfAllRole)
		{
			if(jackOfAllRole.NumTasksUntilMod <= 1)
			{
				JackOfAllRole.CheckAddModifier(@event.Player);
				jackOfAllRole.NumTasksUntilMod = (int)OptionGroupSingleton<JackOfAllOptions>.Instance.TasksPerMod;
			}
			else
			{
				jackOfAllRole.NumTasksUntilMod -= 1;
			}

			CrewAchievementChecks.CheckJoaAchievements(@event.Player);
		}
	}

	[RegisterEvent(0)]
	public static void OnGameStartEvent(RoundStartEvent @event)
	{
		if (!@event.TriggeredByIntro) return;

		if(PlayerControl.LocalPlayer.Data.Role is JackOfAllRole)
		{
            try
            {
                JackOfAllRole.GiveRandModifiers((int)OptionGroupSingleton<JackOfAllOptions>.Instance.NumModifiers - PlayerControl.LocalPlayer.GetModifiers<BaseModifier>().Count(), PlayerControl.LocalPlayer);
            }
            catch (System.Exception e)
            {
                Fatal(e);
            }
        }
	}
}
