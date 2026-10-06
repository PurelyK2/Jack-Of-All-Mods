using MiraAPI.Events;
using MiraAPI.GameOptions;
using MiraAPI.Hud;
using MiraAPI.Modifiers;
using MiraAPI.Utilities;
using TownOfUs.Buttons.Impostor;
using TownOfUs.Events.TouEvents;
using TownOfUs.Modifiers.Game.Universal;
using TownOfUs.Options.Modifiers.Universal;
using TownOfUs.Options.Roles.Impostor;
using TownOfUs.Utilities;
using UnityEngine;
using JAM.Options.Modifiers;

namespace JAM.Modifiers.Hidden;

public sealed class ShackledDragModifier(byte bodyId) : BaseModifier
{
    public override string ModifierName => "Shackle";
    public override bool HideOnUi => true;

    public byte BodyId { get; } = bodyId;
    public float SpeedFactor { get; set; } = OptionGroupSingleton<ShackledOptions>.Instance.ShackledSpeedMultiplier;
    public DeadBody? DeadBody { get; } = Helpers.GetBodyById(bodyId);

    private float _remainingDuration;

    public override bool? CanVent()
    {
        return OptionGroupSingleton<ShackledOptions>.Instance.CanVentWithBody ? null : false;
    }

    public override void OnActivate()
    {
        if (Player != null)
        {
            _remainingDuration = OptionGroupSingleton<ShackledOptions>.Instance.ShackledDuration;
            SpeedFactor = OptionGroupSingleton<ShackledOptions>.Instance.ShackledSpeedMultiplier;
            if (OptionGroupSingleton<ShackledOptions>.Instance.SizeAffectedSpeed)
            {
                var dragged = MiscUtils.PlayerById(DeadBody!.ParentId)!;
                if (dragged.HasModifier<GiantModifier>())
                {
                    SpeedFactor *= OptionGroupSingleton<GiantOptions>.Instance.GiantSpeed;
                }
                else if (dragged.HasModifier<MiniModifier>())
                {
                    SpeedFactor *= OptionGroupSingleton<MiniOptions>.Instance.MiniSpeed;
                }
            }
        }
    }

    public override void OnDeactivate()
    {
        if (Player.AmOwner)
        {
            // CustomButtonSingleton<UndertakerDragDropButton>.Instance.SetDrag();
            // I think this forces the game to drag the body before the modifier is removed, so the body can drop it properly.
        }
        // I think I'll need to make my own nullsAbilityEvent for the shackled dropping, but I also want to drop it after a specified amount of time instead of on a button press.
        var touAbilityEvent = new TouAbilityEvent(AbilityType.UndertakerDrop, Player, DeadBody);
        MiraEventManager.InvokeEvent(touAbilityEvent);

        if (DeadBody == null)
        {
            return;
        }

        var dropPos = DeadBody.transform.position;
        dropPos.z = dropPos.y / 1000f;
        DeadBody.transform.position = dropPos;
    }

    public override void OnMeetingStart()
    {
        Player.RemoveModifier(this);
    }

    public override void OnDeath(DeathReason reason)
    {
        Player.RemoveModifier(this);
    }

    private bool CanReachBody()
    {
        if (DeadBody == null || DeadBody.Reported)
        {
            return false;
        }

        return !PhysicsHelpers.AnythingBetween(Player.Collider, Player.Collider.bounds.center,
            DeadBody.TruePosition, Constants.ShipAndAllObjectsMask, false);
    }

    public override void Update()
    {
        if (DeadBody == null || !DeadBody.myCollider.enabled)
        {
            Player.RemoveModifier(this);
            return;
        }

        if (CanReachBody())
        {
            _remainingDuration -= Time.deltaTime;

            if (_remainingDuration <= 0f)
            {
                Player.RemoveModifier(this);
                return;
            }
        }

        var targetPos = Player.transform.position;
        targetPos.z = targetPos.y / 1000f;
        
        if (Player.inVent)
        {
            DeadBody.transform.position = targetPos;
        }
        else
        {
            DeadBody.transform.position = Vector3.Lerp(DeadBody.transform.position, targetPos, 5f * Time.deltaTime);
        }
    }
}
