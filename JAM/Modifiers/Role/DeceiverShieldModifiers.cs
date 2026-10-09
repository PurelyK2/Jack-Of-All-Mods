using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using MiraAPI.Translation;
using MiraAPI.Utilities.Assets;
using Reactor.Utilities.Extensions;
using TownOfUs.Assets;
using TownOfUs.Modifiers;
using TownOfUs.Modules.Anims;
using TownOfUs.Options.Maps;
using TownOfUs.Options.Roles.Crewmate;
using TownOfUs.Utilities;
using UnityEngine;

namespace JAM.Modifiers.Role;

public sealed class DeceiverMedicShield : BaseShieldModifier, IAnimated
{
    public override string ModifierName => MiraLocaleManager.Get("TouMedicShield", "Medic");
    public override string ShieldDescription => MiraLocaleManager.Get("TouMedicShieldDescription", "");
    public override LoadableAsset<Sprite>? ModifierIcon => TouRoleIcons.Medic;
    public override bool HideOnUi => false;
    public override Color FreeplayFileColor => new Color32(100, 220, 100, byte.MaxValue);
    public GameObject Shield { get; set; }
    public override bool VisibleSymbol
    {
        get
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
            var showShieldedSelf = Player.AmOwner &&
                                   showShielded is MedicOption.Shielded or MedicOption.ShieldedAndMedic;
            return showShieldedSelf || showShieldedEveryone;
        }
    }
    public void SetVisible() { }
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
        Shield.SetActive(!Player.Data.IsDead);

        DeceiverRole.confuseRole = false;
        if(!MiraAPI.Utilities.Helpers.GetAlivePlayers().Any(p => p.Data.Role is DeceiverRole))
        {
            ModifierComponent.RemoveModifier(this);
        }
        DeceiverRole.ReConfuse();
    }
}