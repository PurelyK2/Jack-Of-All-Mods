using JAM.Modifiers;
using JAM.Modifiers.Role;
using JAM.Options.Roles.Impostor;
using MiraAPI.GameOptions;
using MiraAPI.Keybinds;
using MiraAPI.Modifiers;
using MiraAPI.Utilities;
using MiraAPI.Utilities.Assets;
using TownOfUs;
using TownOfUs.Assets;
using TownOfUs.Buttons;
using TownOfUs.Utilities;
using UnityEngine;

namespace JAM.Buttons.Roles.Impostor;

public sealed class DeceiverShieldButton : TownOfUsRoleButton<DeceiverRole, PlayerControl>
{
    public override string Name => "DECEIVE";
    public override BaseKeybind Keybind => Keybinds.PrimaryAction;

    public override float Cooldown => Math.Clamp(TownOfUsTargetButton<PlayerControl>.MapCooldown, 0.001f, 120f);
    public override LoadableAsset<Sprite> Sprite => TouCrewAssets.MedicSprite;

    public override PlayerControl? GetTarget()
    {
        return PlayerControl.LocalPlayer.GetClosestLivingPlayer(true, this.Distance, false, null);
    }

    protected override void OnClick()
    {
        if (base.Target == null)
        {
            Error("Deceiver Shield: Target is null");
            return;
        }
        else if (MiraAPI.Utilities.Helpers.GetAlivePlayers().Any(p => p.HasModifier<DeceiverMedicShield>() || p.HasModifier<DeceiverWardenShield>()))
        {
            Error("There Is Already A Shielded Target: " + Helpers.GetAlivePlayers().FirstOrDefault(p => p.HasModifier<DeceiverMedicShield>() || p.HasModifier<DeceiverWardenShield>())?.Data.PlayerName);
            return;
        }

        DeceiverRole.RpcDeceiverShield(PlayerControl.LocalPlayer, Target, UnityEngine.Random.Range(0f, 1f) < 0.5f);
    }
    public override bool CanUse()
    {
        return base.CanUse() && !MiraAPI.Utilities.Helpers.GetAlivePlayers().Any(p => p.HasModifier<DeceiverMedicShield>() || p.HasModifier<DeceiverWardenShield>());
    }
}