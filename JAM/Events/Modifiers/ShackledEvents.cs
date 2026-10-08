using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.Modifiers;
using MiraAPI.Utilities;
using TownOfUs.Utilities;
using UnityEngine;
using JAM.Modifiers;
using JAM.Modifiers.Hidden;
using JAM.Assets;

namespace JAM.Events.Modifiers;

public static class ShackledEvents
{
    [RegisterEvent]
    public static void ShackledDeathEvent(AfterMurderEvent @event)
    {
        var source = @event.Source;
        var target = @event.Target;

        if (!target.HasModifier<ShackledModifier>() || !source.AmOwner || MeetingHud.Instance || source.HasDied())
        {
            return;
        }

        source.RpcAddModifier<ShackledDragModifier>(target.PlayerId);

        var text = "<player> was <modifier>, forcing you to drag their body!"
            .Replace("<player>", target.Data.PlayerName)
            .Replace(
                "<modifier>",
                $"{Colors.Shackled.ToTextColor()}Shackled</color>");

        var notif = Helpers.CreateAndShowNotification(
            $"<b>{text}</b>",
            Color.white,
            new Vector3(0f, 1f, -20f),
            spr: JamModifierIcons.Shackled.LoadAsset());

        notif?.AdjustNotification();
    }
}
