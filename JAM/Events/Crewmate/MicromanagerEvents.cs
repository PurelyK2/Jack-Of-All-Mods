using UnityEngine;
using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Player;
using MiraAPI.GameOptions;
using MiraAPI.Utilities;
using TownOfUs.Utilities;
using Reactor.Networking.Attributes;
using Reactor.Networking.Rpc;
using Reactor.Utilities;
using Reactor;
using JAM.Roles.Crewmate;
using JAM.Options.Roles.Crewmate;
using JAM.Assets;

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
