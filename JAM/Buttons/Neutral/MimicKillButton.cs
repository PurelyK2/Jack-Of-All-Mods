using MiraAPI.GameOptions;
using MiraAPI.Keybinds;
using MiraAPI.Networking;
using MiraAPI.Utilities.Assets;
using Reactor.Utilities;
using JAM.Options.Roles.Neutral;
using JAM.Roles.Neutral;
using TownOfUs.Assets;
using TownOfUs.Buttons;
using TownOfUs.Options.Modifiers.Alliance;
using TownOfUs.Utilities;
using UnityEngine;
using MiraAPI.Modifiers;
using JAM.Modifiers.Role;
using Reactor.Utilities.Extensions;

namespace JAM.Buttons.Neutral;

public sealed class MimicKillButton : TownOfUsTargetButton<PlayerControl>, IDiseaseableButton,
    IKillButton
{
    public override string Name => TranslationController.Instance.GetStringWithDefault(StringNames.KillLabel, "Kill");
    public override BaseKeybind Keybind => Keybinds.PrimaryAction;
    public override Color TextOutlineColor => Colors.Mimic;
    public override float Cooldown => GameManager.Instance.LogicOptions.GetKillCooldown();
    public override LoadableAsset<Sprite> Sprite => TouAssets.KillSprite;

    public override bool Enabled(RoleBehaviour? role)
    {
        return PlayerControl.LocalPlayer.Data.Role is MimicRole || (
                PlayerControl.LocalPlayer.HasModifier<MimicCacheModifier>()
                && !PlayerControl.LocalPlayer.IsImpostor()
                && PlayerControl.LocalPlayer.Data.Role.GetRoleAlignment() != TownOfUs.Roles.RoleAlignment.NeutralKilling
                && PlayerControl.LocalPlayer.Data.Role.GetRoleAlignment() != TownOfUs.Roles.RoleAlignment.CrewmateKilling);
    }

    public void SetDiseasedTimer(float multiplier)
    {
        SetTimer(Cooldown * multiplier);
    }

    public override PlayerControl? GetTarget()
    {
        if (!OptionGroupSingleton<LoversOptions>.Instance.LoversKillEachOther && PlayerControl.LocalPlayer.IsLover())
        {
            return PlayerControl.LocalPlayer.GetClosestLivingPlayer(true, Distance, false, x => !x.IsLover());
        }

        return PlayerControl.LocalPlayer.GetClosestLivingPlayer(true, Distance);
    }

    protected override void OnClick()
    {
        if (Target == null)
        {
            Error("Mimic Kill: Target is null");
            return;
        }

        PlayerControl.LocalPlayer.RpcCustomMurder(Target);
    }

    public override void SetOutline(bool active)
    {
        if (base.Target != null && !PlayerControl.LocalPlayer.HasDied())
        {
            Target.cosmetics.currentBodySprite.BodySprite.SetOutline(active ? Colors.Mimic : null);
        }
    }
}
