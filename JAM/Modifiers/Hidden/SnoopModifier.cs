using HarmonyLib;
using JAM.Options.Roles.Crewmate;
using MiraAPI.Events;
using MiraAPI.GameOptions;
using MiraAPI.Hud;
using MiraAPI.Modifiers;
using TownOfUs.Assets;
using TownOfUs.Buttons.Impostor;
using TownOfUs.Events.TouEvents;
using TownOfUs.Modifiers;
using TownOfUs.Modules.Localization;
using TownOfUs.Options;
using TownOfUs.Patches;
using TownOfUs.Utilities;
using TownOfUs.Utilities.Appearances;
using UnityEngine;

namespace JAM.Modifiers.Hidden;

public sealed class StealthySwoopModifier : ConcealedModifier, IVisualAppearance
{
        public override string ModifierName => "Stealthy";
        public override float Duration => OptionGroupSingleton<StealthyOptions>.Instance.SneakDuration;
        public override bool HideOnUi => true;
        public override bool AutoStart => true;
        public override bool VisibleToOthers => false;

        public bool VisualPriority => true;

        public VisualAppearance GetVisualAppearance()
    {
        Color playerColor = (Player.AmOwner || (PlayerControl.LocalPlayer.DiedOtherRound() && OptionGroupSingleton<GeneralOptions>.Instance.TheDeadKnow)) ? new Color(0f, 0f, 0f, 0.1f) : Color.clear;
        return new VisualAppearance(Player.GetDefaultModifiedAppearance(), TownOfUsAppearances.Swooper)
        {
            HatId = "hat_NoHat",
            SkinId = "skin_None",
            VisorId = "visor_EmptyVisor",
            PlayerName = string.Empty,
            PetId = "pet_EmptyPet",
            RendererColor = playerColor,
            NameColor = new Color?(Color.clear),
            ColorBlindTextColor = Color.clear
        };
    }
        public override void OnMeetingStart()
    {
        Player.RpcRemoveModifier(this.GetType());
    }

        public override void OnActivate()
    {
        if (base.Player.AmOwner)
        {
            TouAudio.PlaySound(TouAudio.SwooperActivateSound, 1f);
            SwooperSwoopButton instance = CustomButtonSingleton<SwooperSwoopButton>.Instance;
            instance.OverrideSprite(TouImpAssets.UnswoopSprite.LoadAsset());
            instance.OverrideName(TouLocale.Get("TouRoleSwooperUnswoop", "Unswoop"));
        }
        base.Player.RawSetAppearance(this);
        base.Player.cosmetics.ToggleNameVisible(false);
        MiraEventManager.InvokeEvent<TouAbilityEvent>(new TouAbilityEvent(AbilityType.SwooperSwoop, base.Player, null, null));
    }

        public override void OnDeactivate()
    {
        base.Player.ResetAppearance(false, false);
        base.Player.cosmetics.ToggleNameVisible(true);
        if (base.Player.AmOwner)
        {
            SwooperSwoopButton instance = CustomButtonSingleton<SwooperSwoopButton>.Instance;
            instance.OverrideSprite(TouImpAssets.SwoopSprite.LoadAsset());
            instance.OverrideName(TouLocale.Get("TouRoleSwooperSwoop", "Swoop"));
            if (!MeetingHud.Instance)
            {
                TouAudio.PlaySound(TouAudio.SwooperDeactivateSound, 1f);
            }
        }
        if (HudManagerPatches.CamouflageCommsEnabled)
        {
            base.Player.cosmetics.ToggleNameVisible(false);
        }
        MiraEventManager.InvokeEvent<TouAbilityEvent>(new TouAbilityEvent(AbilityType.SwooperUnswoop, base.Player, null, null));
    }

    [HarmonyPatch(typeof(PlayerControl), "MixUpOutfit", new Type[] {typeof(NetworkedPlayerInfo.PlayerOutfit)})]
    public static class MushroomMixupDoesntAffectSnoop
    {
        public static bool Prefix(ref PlayerControl __instance)
        {
            if (__instance.HasModifier<StealthySwoopModifier>()) return false;
            return true;
        }
    }
}
