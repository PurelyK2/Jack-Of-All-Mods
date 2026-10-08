using MiraAPI.Events;
using MiraAPI.GameOptions;
using TownOfUs.Events.TouEvents;
using TownOfUs.Utilities.Appearances;
using UnityEngine;
using TownOfUs.Modifiers;
using JAM.Options.Roles.Impostor;
using JAM.Roles.Impostor;
namespace JAM.Modifiers.Hidden;

public sealed class CamouflagerCamoModifier : ConcealedModifier, IVisualAppearance
{
    public override string ModifierName => "CamouflagerCamo";
    public override float Duration => OptionGroupSingleton<CamouflagerOptions>.Instance.CamoDuration;
    public override bool AutoStart => true;
    public bool VisualPriority => true;
    public override bool HideOnUi => true;
    public override bool VisibleToOthers => true;

    public VisualAppearance GetVisualAppearance()
    {
        var appearance = Player.GetDefaultAppearance();
        appearance.Speed = 1f;
        appearance.Size = new Vector3(0.7f, 0.7f, 1f);
        appearance.ColorId = Player.Data.DefaultOutfit.ColorId;
        appearance.HatId = "hat_NoHat";
        appearance.SkinId = "skin_None";
        appearance.VisorId = "visor_EmptyVisor";
        appearance.PetId = "pet_EmptyPet";
        if (PlayerControl.LocalPlayer.Data.Role is CamouflagerRole && OptionGroupSingleton<CamouflagerOptions>.Instance.CamoVision)
        {
            appearance.PlayerName = Player.Data.PlayerName;
            appearance.NameVisible = true;
        }
        else
        {
            appearance.PlayerName = string.Empty;
            appearance.NameVisible = false;
        }
        appearance.PlayerMaterialColor = Color.grey;
        return appearance;
    }

    public override void OnActivate()
    {
        Player.RawSetAppearance(this);

        var touAbilityEvent = new TouAbilityEvent(AbilityType.VenererCamoAbility, Player);
        MiraEventManager.InvokeEvent(touAbilityEvent);
    }

    public override void OnDeactivate()
    {
        Player?.ResetAppearance();
    }
}
