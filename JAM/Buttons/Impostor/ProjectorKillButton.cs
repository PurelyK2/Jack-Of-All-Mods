using HarmonyLib;
using JAM.Assets;
using JAM.Modifiers.Hidden;
using JAM.Options.Roles.Impostor;
using JAM.Roles.Impostor;
using MiraAPI.GameOptions;
using MiraAPI.Keybinds;
using MiraAPI.Modifiers;
using MiraAPI.Networking;
using MiraAPI.Utilities;
using MiraAPI.Utilities.Assets;
using System.Globalization;
using TownOfUs;
using TownOfUs.Assets;
using TownOfUs.Buttons;
using TownOfUs.Modifiers;
using TownOfUs.Modifiers.Game.Alliance;
using TownOfUs.Modules;
using TownOfUs.Modules.Components;
using TownOfUs.Modules.MedSpirit;
using TownOfUs.Options;
using TownOfUs.Options.Maps;
using TownOfUs.Options.Modifiers.Alliance;
using TownOfUs.Roles;
using TownOfUs.Roles.Impostor;
using TownOfUs.Utilities;
using UnityEngine;
using UnityEngine.UI;

namespace JAM.Buttons.Impostor;

public sealed class ProjectorKillButton : TownOfUsKillRoleButton<ProjectorRole, PlayerControl>
{
    public override float Cooldown => GameManager.Instance.LogicOptions.GetKillCooldown();
    public override BaseKeybind? Keybind => Keybinds.PrimaryAction;
    public override LoadableAsset<Sprite> Sprite => TouAssets.KillSprite;
    public override PlayerControl? GetTarget()
    {
        Vector2 playerPos = (Role.Spirit?.Rigidbody.position ?? PlayerControl.LocalPlayer.transform.position) + PlayerControl.LocalPlayer.Collider.offset;

        if (!OptionGroupSingleton<LoversOptions>.Instance.LoversKillEachOther && PlayerControl.LocalPlayer.IsLover())
        {
            return GetClosestLivingPlayerTo(playerPos, Distance, true);
        }
        return GetClosestLivingPlayerTo(playerPos, Distance);
    }

    protected override void OnClick()
    {
        if (Target == null)
        {
            Error("Projector Error: Target Is Null");
            return;
        }

        PlayerControl.LocalPlayer.RpcCustomMurder(Target);
        ProjectorRole.RpcRemoveMediumSpirit(PlayerControl.LocalPlayer, Role.Spirit);
        Role.Spirit = null;
    }

    PlayerControl? GetClosestLivingPlayerTo(
        Vector2 position,
        float distance,
        bool skipLovers = false)
    {
        PlayerControl? closestPlayer = null;

        List<PlayerControl> AlivePlayers = PlayerControl.AllPlayerControls.ToArray().Where(
                                player => !player.Data.Disconnected && !player.AmOwner
                                && ((player.TryGetModifier<DisabledModifier>(out var mod) && mod.IsConsideredAlive)
                                    || !player.HasModifier<DisabledModifier>()
                                ) && !player.Data.IsDead && !(player.HasModifier<LoverModifier>() && skipLovers)
                                && !player.IsImpostor()).ToList();

        AlivePlayers.Sort((p1, p2) =>
            (p1.GetTruePosition() - position).sqrMagnitude.CompareTo((p2.GetTruePosition() - position).sqrMagnitude)
        );

        if (AlivePlayers.Count > 0)
            closestPlayer = AlivePlayers[0];

        if (Vector2.Distance(closestPlayer?.GetTruePosition() ?? position + Vector2.up * distance * 2, position) > distance)
            closestPlayer = null;

        return closestPlayer;
    }

    protected override void FixedUpdate(PlayerControl playerControl)
    {
        if (Role.killButton == null)
            Role.killButton = this;

        base.FixedUpdate(playerControl);
    }

    public override bool CanUse()
    {
        Target = GetTarget();

        if (TimeLordRewindSystem.IsRewinding) return false;
        if (HudManager.Instance.Chat.IsOpenOrOpening || MeetingHud.Instance) return false;
        if (PlayerControl.LocalPlayer.GetModifiers<DisabledModifier>().Any(x => !x.CanUseAbilities)) return false;

        return Timer <= 0 && Target != null;
    }
}
