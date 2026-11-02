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

namespace JAM.Buttons.Neutral;

public sealed class ForbearingKillButton : TownOfUsKillRoleButton<ForbearingRole, PlayerControl>, IDiseaseableButton,
    IKillButton
{
    public override string Name => TranslationController.Instance.GetStringWithDefault(StringNames.KillLabel, "Kill");
    public override BaseKeybind Keybind => Keybinds.PrimaryAction;
    public override Color TextOutlineColor => Colors.Forbearing;
    public override float Cooldown => Role != null ? Mathf.Max(0, OptionGroupSingleton<ForbearingOptions>.Instance.ForbearingCooldown - OptionGroupSingleton<ForbearingOptions>.Instance.CooldownDecrease * Role.numMeetingsSkipped) : OptionGroupSingleton<ForbearingOptions>.Instance.ForbearingCooldown;
    public override LoadableAsset<Sprite> Sprite => TouAssets.KillSprite;

    public override void CreateButton(Transform parent)
    {
        base.CreateButton(parent);
        Coroutines.Start(MiscUtils.CoMoveButtonIndex(this, false));
    }

    public override bool Enabled(RoleBehaviour? role)
    {
        return role is ForbearingRole && (Role.numMeetingsSkipped >= 0 || OptionGroupSingleton<ForbearingOptions>.Instance.StartWithKillButton);
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
            Error("Time Kill: Target is null");
            return;
        }

        PlayerControl.LocalPlayer.RpcCustomMurder(Target);
    }
}
