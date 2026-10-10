using MiraAPI.Keybinds;
using MiraAPI.Utilities.Assets;
using TownOfUs.Assets;
using UnityEngine;
using MiraAPI.Modifiers;
using TownOfUs.Modifiers;
using TownOfUs.Modules;
using TownOfUs.Utilities;
using Reactor.Utilities.Extensions;
using JAM.Modifiers;
using TownOfUs.Buttons;

namespace JAM.Buttons.Crewmate;

public sealed class VentableVentButton : TownOfUsTargetButton<Vent>
{
        public override string Name => "VENT";
        public override BaseKeybind Keybind => Keybinds.VentAction;
        public override Color TextOutlineColor => Colors.Ventable;
        public override float Cooldown => 0;
        public override LoadableAsset<Sprite> Sprite => TouNeutAssets.JuggVentSprite;

        public override bool Enabled(RoleBehaviour? role)
        {
            return !Disabled && role?.Player?.HasModifier<VentableModifier>() == true;
        }

        public override Vent GetTarget()
    {
        return DestroyableSingleton<HudManager>.Instance.ImpostorVentButton.currentTarget;
    }

        public override void SetOutline(bool active)
    {
        if (Target != null && !PlayerControl.LocalPlayer.HasDied())
        {
            Target.SetOutline(active, true, Colors.Ventable);
        }
    }

        public override bool CanUse()
    {
        
        if (TimeLordRewindSystem.IsRewinding)
        {
            return false;
        }
        if (PlayerControl.LocalPlayer.HasDied())
        {
            return false;
        }
        if (DestroyableSingleton<HudManager>.Instance.Chat.IsOpenOrOpening || MeetingHud.Instance)
        {
            return false;
        }
        if (PlayerControl.LocalPlayer.GetModifiers<DisabledModifier>(null).Any((DisabledModifier x) => !x.CanUseAbilities))
        {
            return false;
        }
        Vent newTarget = this.GetTarget();
        base.Target = (this.IsTargetValid(newTarget) ? newTarget : null);
        return (PlayerControl.LocalPlayer.inVent || (base.Timer <= 0f && base.Target != null)) && (!base.LimitedUses || base.UsesLeft > 0);
    }

        protected override void OnClick()
    {
        if(Target != null && !PlayerControl.LocalPlayer.inVent)
        {
			PlayerControl.LocalPlayer.MyPhysics.RpcEnterVent(Target.Id);
			Target.SetButtons(true);
        }
        else if(Target != null)
        {
			PlayerControl.LocalPlayer.MyPhysics.RpcExitVent(Target.Id);
			Target.SetButtons(false);
        }
    }
}
