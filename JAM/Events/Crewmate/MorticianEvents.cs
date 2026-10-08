using Reactor.Networking.Rpc;
using UnityEngine;
using Reactor.Networking.Attributes;
using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.Events.Vanilla.Player;
using MiraAPI.Utilities;
using MiraAPI.GameOptions;
using TownOfUs.Utilities;
using TownOfUs.Roles;
using TownOfUs.Modules;
using MiraAPI.Events.Vanilla.Meeting;
using AmongUs.GameOptions;
using JAM.Options.Roles.Crewmate;
using JAM.Roles.Crewmate;
using JAM.Patches;
using JAM.Assets;

namespace JAM.Events.Crewmate;

public static class MorticianEvents
{
    private static readonly Dictionary<byte, byte> KillerByVictim = new();
    private static readonly Dictionary<byte, RoleTypes> KillerRoleByVictim = new();

    [RegisterEvent]
    public static void CompleteTaskEvent(CompleteTaskEvent @event)
    {
        if (@event.Player.Data.Role is not MorticianRole mortician)
        {
            return;
        }

        if (!@event.Player.AmOwner)
        {
            return;
        }
        var opts = OptionGroupSingleton<MorticianOptions>.Instance;
        ++mortician.TaskProgress;

        if (mortician.TaskProgress >= opts.TasksPerAbilityUse)
        {
            mortician.TaskProgress = 0;
            ++mortician.AbilityUses;
        }
    }

    [RegisterEvent]
    public static void AfterMurderEventHandler(AfterMurderEvent @event)
    {
        var source = @event.Source;
        var target = @event.Target;

        if (source == null || target == null)
        {
            return;
        }

        KillerByVictim[target.PlayerId] = source.PlayerId;

        var killerRole = source.Data.Role is IGhostRole
            ? source.Data.Role
            : source.GetRoleWhenAlive();

        if (killerRole != null)
        {
            KillerRoleByVictim[target.PlayerId] = killerRole.Role;
        }
    }

    [MethodRpc(9103)]
    public static void RpcMorticianAutopsyNotification(PlayerControl player, byte targetId)
    {
        var target = GameData.Instance.GetPlayerById(targetId)?.Object;

        if (target == null)
        {
            return;
        }

        var notification = Helpers.CreateAndShowNotification(
            $"<b>The <color=#{ColorUtility.ToHtmlStringRGBA(Colors.Mortician)}>Mortician</color> performed an Autopsy on {target.Data.PlayerName}</b>",
            Color.white,
            new Vector3(0f, 2f, -20f),
            spr: JamAssets.MorticianAbility.LoadAsset());

        notification.AdjustNotification();
        notification.alphaTimer = 5f;
    }

    [MethodRpc(9104)]
    public static void RpcMorticianAutopsyChat(PlayerControl morticianPlayer,
        byte victimId, byte killerId, ushort killerRoleType)
    {
        if (PlayerControl.LocalPlayer != morticianPlayer)
        {
            return;
        }

        var victim = GameData.Instance.GetPlayerById(victimId)?.Object;
        var killer = GameData.Instance.GetPlayerById(killerId)?.Object;
        var killerRole = RoleManager.Instance.GetRole((RoleTypes)killerRoleType);

        if (victim == null || killer == null || killerRole == null)
        {
            return;
        }

        var victimRole = MorticianPatch.ResolveRoleBeforeDeath(victim.PlayerId);

        if (victimRole == null)
        {
            return;
        }

        var title =
            $"<color=#{ColorUtility.ToHtmlStringRGBA(Colors.Mortician)}>Mortician</color>";

        var message =
            $"Autopsy Results:\n" +
            $"{victim.Data.PlayerName} was a #{victimRole.GetRoleName()}.\n" +
            $"Killed by a #{killerRole.GetRoleName()}.";

        MiscUtils.AddFakeChat(
            morticianPlayer.Data,
            title,
            message,
            false,
            true);
    }

    [RegisterEvent]
    public static void ReportBodyEventHandler(ReportBodyEvent @event)
    {
        if (!@event.Reporter.AmOwner ||
            @event.Reporter.Data.Role is not MorticianRole)
        {
            return;
        }

        var victim = @event.Target?.Object;

        if (victim == null)
        {
            return;
        }

        if (!KillerByVictim.TryGetValue(victim.PlayerId, out var killerId) ||
            !KillerRoleByVictim.TryGetValue(victim.PlayerId, out var killerRoleType))
        {
            return;
        }
        var options = OptionGroupSingleton<MorticianOptions>.Instance;
        if(options.GetAutopsyChat == true)
        {
            RpcMorticianAutopsyChat(@event.Reporter, victim.PlayerId, killerId, (ushort)killerRoleType);
        }
    }
}
