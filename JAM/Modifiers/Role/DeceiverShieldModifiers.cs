using HarmonyLib;
using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using MiraAPI.Modifiers.Types;
using MiraAPI.Translation;
using MiraAPI.Utilities.Assets;
using Reactor.Utilities.Extensions;
using System.Reflection;
using System.Reflection.Emit;
using TownOfUs.Assets;
using TownOfUs.Modifiers;
using TownOfUs.Modifiers.Crewmate;
using TownOfUs.Modules.Anims;
using TownOfUs.Options.Maps;
using TownOfUs.Options.Roles.Crewmate;
using TownOfUs.Utilities;
using UnityEngine;
using UnityEngine.UIElements;

namespace JAM.Modifiers.Role;

public sealed class DeceiverMedicShield : TimedModifier, IAnimated
{
    public override string ModifierName => MiraLocaleManager.Get("TouMedicShield", "Medic");
    public string ShieldDescription => MiraLocaleManager.Get("TouMedicShieldDescription", "");
    public override LoadableAsset<Sprite>? ModifierIcon => TouRoleIcons.Medic;
    public override bool HideOnUi => false;
    public override Color FreeplayFileColor => new Color32(100, 220, 100, byte.MaxValue);
    public GameObject Shield { get; set; }

    public static (string good, string bad) DeceiverShieldIcon => new("+", "±");

    public bool IsVisible
    {
        get
        {
            return DeceiverShieldIconMaker.ShouldSeeShield(Player) || (PlayerControl.LocalPlayer.HasDied() && PlayerControl.LocalPlayer.DiedOtherRound()) || PlayerControl.LocalPlayer.IsImpostorAligned();
        }
    }
    public override string GetDescription()
    {
        return (!this.HideOnUi) ? this.ShieldDescription : string.Empty;
    }
    public void SetVisible() { }

    public override float Duration => 0.001f;
    public override bool AutoStart => false;

    bool IAnimated.IsVisible { get; set; } = true;

    public override void OnActivate()
    {
        Shield = AnimStore.SpawnAnimBody(base.Player, TouAssets.MedicShield.LoadAsset(), false, -1.1f, -0.1f, 1.5f);
    }
    public override void OnDeath(DeathReason reason)
    {
        base.OnDeath(reason);
        Shield?.SetActive(false);
        ModifierComponent?.RemoveModifier(this);
    }
    public override void OnDeactivate()
    {
        if (Shield != null)
        {
            Shield.Destroy();
        }
    }
    public override void Update()
    {
        Shield.SetActive(IsVisible);

        DeceiverRole.confuseRole = false;
        if(!MiraAPI.Utilities.Helpers.GetAlivePlayers().Any(p => p.Data.Role is DeceiverRole))
        {
            ModifierComponent.RemoveModifier(this);
        }
        DeceiverRole.ReConfuse();
    }

    [RegisterEvent(0)]
    public static void RemoveShieldBeforeKill(BeforeMurderEvent @event)
    {
        if(@event.Target.HasModifier<DeceiverMedicShield>())
        {
            @event.Target.RemoveModifier<DeceiverMedicShield>();
        }
    }
}

[HarmonyPatch(typeof(PlayerRoleTextExtensions), "UpdateProtectionSymbols", [typeof(string), typeof(PlayerControl), typeof(DataVisibility)])]
public static class  DeceiverShieldIconMaker
{
    public static void Postfix(ref string __result, PlayerControl player)
    {
        if (player == null) return;

        string deceiverIcon = DeceiverShieldIconMaker.AddDeceiverIcon(player);

        if (!string.IsNullOrEmpty(deceiverIcon))
        {
            __result += deceiverIcon;
        }
    }

    public static string AddDeceiverIcon(PlayerControl player)
    {
        // Ensure player and data exist safely
        if (player == null || player.Data == null || player.Data.Disconnected) return "";

        if (player.HasModifier<DeceiverMedicShield>())
        {
            if (PlayerControl.LocalPlayer.HasDied() || PlayerControl.LocalPlayer.IsImpostorAligned())
            {
                Info("Showing Evils Shield");
                return "<color=#006600> " + DeceiverMedicShield.DeceiverShieldIcon.Item2 + "</color>";
            }
            else if (ShouldSeeShield(player))
            {
                return "<color=#006600> " + DeceiverMedicShield.DeceiverShieldIcon.Item1 + "</color>";
            }

            return "";
        }

        return "";
    }

    public static bool ShouldSeeShield(PlayerControl player)
    {
        bool commsActive = false;
        switch ((ExpandedMapNames)GameOptionsManager.Instance.currentGameOptions.MapId)
        {
            case ExpandedMapNames.MiraHq:
            case ExpandedMapNames.Fungle:
                var hqComms = ShipStatus.Instance.Systems[SystemTypes.Comms]
                    .Cast<HqHudSystemType>();

                commsActive = hqComms.IsActive;
                break;

            default:
                var hudComms = ShipStatus.Instance.Systems[SystemTypes.Comms]
                    .Cast<HudOverrideSystemType>();

                commsActive = hudComms.IsActive;
                break;
        }
        if (commsActive && TownOfUsMapOptions.IsCamoCommsOn())
        {
            return false;
        }

        var showShielded = OptionGroupSingleton<MedicOptions>.Instance.ShowShielded;
        var showShieldedEveryone = showShielded == MedicOption.Everyone;
        var showShieldedSelf = player.AmOwner &&
                               showShielded is MedicOption.Shielded or MedicOption.ShieldedAndMedic;
        return showShieldedSelf || showShieldedEveryone;
    }
}