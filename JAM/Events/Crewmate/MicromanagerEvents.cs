using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Player;
using MiraAPI.GameOptions;
using JAM.Roles.Crewmate;
using JAM.Options.Roles.Crewmate;

namespace JAM.Events.Crewmate;
public static class MicromanagerEvents
{
    [RegisterEvent]
    public static void CompleteTaskEvent(CompleteTaskEvent @event)
    {
        if (@event.Player.Data.Role is not MicromanagerRole micromanagerRole)
        {
            return;
        }

        if (!@event.Player.AmOwner || micromanagerRole.Caught)
        {
            return;
        }

        // micromanagerRole.CheckTaskRequirements();
        ++micromanagerRole.managedTaskProgression;

        if (micromanagerRole.managedTaskProgression >=
            OptionGroupSingleton<MicromanagerOptions>.Instance.NumTasksPerManagedTask)
        {
            micromanagerRole.managedTaskProgression = 0f;

            micromanagerRole.CompleteRandomCrewTask();
        }
    }
}
