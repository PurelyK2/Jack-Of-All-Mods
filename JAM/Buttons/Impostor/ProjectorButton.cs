using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using MiraAPI.Keybinds;
using MiraAPI.Utilities;
using MiraAPI.Utilities.Assets;
using TownOfUs;
using TownOfUs.Utilities;
using TownOfUs.Assets;
using TownOfUs.Modifiers;
using TownOfUs.Roles.Impostor;
using TownOfUs.Buttons;
using TownOfUs.Options.Maps;
using UnityEngine;
using UnityEngine.UI;
using JAM.Options.Roles.Impostor;
using JAM.Modifiers.Hidden;
using JAM.Roles.Impostor;
using JAM.Assets;

namespace JAM.Buttons.Impostor;

public sealed class ProjectorButton : TownOfUsRoleButton<ProjectorRole>, ILegacyCapable
{
    public override string Name => "Project";
    public override BaseKeybind Keybind => Keybinds.SecondaryAction;
    public override Color TextOutlineColor => TownOfUsColors.Medium;
    public override LoadableAsset<Sprite> Sprite => LegacyAssets.IsLegacy ? JamAssets.ProjectButton : JamAssets.ProjectButton;
    public override float Cooldown => Math.Clamp(OptionGroupSingleton<ProjectorOptions>.Instance.ProjectCd + MapCooldown, 0.001f, 120f);
    public override float EffectDuration => OptionGroupSingleton<ProjectorOptions>.Instance.ProjectDuration;

    public override bool ZeroIsInfinite { get; set; } = true;

    public override void ClickHandler()
    {
        if (!CanUse())
        {
            return;
        }

        OnClick();
        Button?.SetDisabled();
        if (EffectActive)
        {
            Timer = Cooldown;
            EffectActive = false;
        }
        else if (HasEffect)
        {
            EffectActive = true;
            Timer = EffectDuration;
        }
        else
        {
            Timer = Cooldown;
        }
    }

    protected override void OnClick()
    {
        if (EffectActive)
        {
            if (Role.Spirit != null)
            {
                ProjectorRole.RpcRemoveMediumSpirit(PlayerControl.LocalPlayer, Role.Spirit);
            }
            return;
        }
    }

    public override void OnEffectEnd()
    {
        if (Role.Spirit == null)
        {
            return;
        }
        ProjectorRole.RpcRemoveMediumSpirit(PlayerControl.LocalPlayer, Role.Spirit);
    }

    public override bool CanUse()
    {
        if (HudManager.Instance.Chat.IsOpenOrOpening || MeetingHud.Instance)
        {
            return false;
        }

        if (PlayerControl.LocalPlayer.GetModifiers<DisabledModifier>().Any(x => !x.CanUseAbilities))
        {
            return false;
        }

        return ((Timer <= 0 && !EffectActive) || (EffectActive && Timer <= EffectDuration - OptionGroupSingleton<ProjectorOptions>.Instance.ProjectDuration - 1f));
    }

}
