using JAM.Modifiers.Role;
using JAM.Options.Roles.Crewmate;
using JAM.Roles.Crewmate;
using JAM.Roles.Neutral;
using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Player;
using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using TownOfUs.Modules;

namespace JAM.Events.Crewmate
{
	public static class JackOfAllEvents
	{
		[RegisterEvent(0)]
		public static void CompleteTaskEvent(CompleteTaskEvent @event)
		{
			Info("Completed Task");
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
			}
		}

        [RegisterEvent(0)]
        public static void HandleVoteEvent(MiraAPI.Events.Vanilla.Meeting.Voting.HandleVoteEvent @event)
        {
            if (@event.Player.HasModifier<JackOfAllVotes>())
            {
                @event.VoteData.SetRemainingVotes(0);
                for (int i = 0; i < @event.Player.GetModifier<JackOfAllVotes>().NumVotes + 1; i++)
                {
                    @event.VoteData.VoteForPlayer(@event.TargetId);
                }
                @event.Cancel();
            }
        }
    }
}
