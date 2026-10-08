using MiraAPI.GameOptions;
using MiraAPI.Keybinds;
using MiraAPI.Modifiers;
using MiraAPI.Utilities.Assets;
using JAM.Options.Roles.Crewmate;
using JAM.Roles.Crewmate;
using TownOfUs.Assets;
using TownOfUs.Buttons;
using TownOfUs.Modifiers;
using UnityEngine;
using JAM.Modifiers.Hidden;

namespace JAM.Buttons.Crewmate;

public sealed class SnoopSwoopButton : TownOfUsRoleButton<SnoopRole>
{
	public override Color TextOutlineColor => Colors.Snoop;
	public override string Name => "Snoop";
	public override BaseKeybind Keybind => Keybinds.PrimaryAction;
	public override float Cooldown => Math.Clamp(OptionGroupSingleton<StealthyOptions>.Instance.SneakCooldown + TownOfUsButton.MapCooldown, 5f, 120f);

	public override float EffectDuration => OptionGroupSingleton<StealthyOptions>.Instance.SneakDuration;

    /* 
	public override int MaxUses => (int)OptionGroupSingleton<StealthyOptions>.Instance.MaxSneaks;
	*/

	public override LoadableAsset<Sprite> Sprite => TouCrewAssets.CrewSwoopSprite;
	public override bool ZeroIsInfinite => true;
    public override void ClickHandler()
    {
        if (!CanUse())
        {
            return;
        }

        if (EffectActive)
        {
            Timer = Cooldown;
            EffectActive = false;
            Button?.SetDisabled();
            OnEffectEnd();
            return;
        }

        OnClick();
        Button?.SetDisabled();

        if (HasEffect)
        {
            EffectActive = true;
            Timer = EffectDuration;
        }
        else
        {
            Timer = Cooldown;
        }
    }
	public override bool CanUse()
	{
		if (DestroyableSingleton<HudManager>.Instance.Chat.IsOpenOrOpening || MeetingHud.Instance)
		{
			return false;
		}
		return !PlayerControl.LocalPlayer.GetModifiers<DisabledModifier>(null).Any((DisabledModifier x) => !x.CanUseAbilities) && ((Timer <= 0f && !EffectActive && (!LimitedUses || UsesLeft > 0)) || (EffectActive && Timer <= EffectDuration - 1f));
	}

	protected override void OnClick()
	{
		if(EffectActive)
		{
			ResetCooldownAndOrEffect();
			OnEffectEnd();
			EffectActive = false;
			Timer = 0.001f;
			return;
		}

		EffectActive = true;
		PlayerControl.LocalPlayer.RpcAddModifier<StealthySwoopModifier>();

		int usesLeft = UsesLeft;
		UsesLeft = usesLeft - 1;
		if (LimitedUses && !EffectActive)
		{
			if (Button == null)
			{
				return;
			}
			Button.SetUsesRemaining(UsesLeft);
		}
	}

    public override void OnEffectEnd()
    {
		Timer = Cooldown;
		EffectActive = false;
		if(PlayerControl.LocalPlayer.HasModifier<StealthySwoopModifier>())
			PlayerControl.LocalPlayer.RpcRemoveModifier<StealthySwoopModifier>();
    }
}
