using JAM.Options.Roles.Neutral;
using JAM.Roles.Neutral;
using MiraAPI.GameOptions;
using MiraAPI.Keybinds;
using MiraAPI.Modifiers;
using MiraAPI.Roles;
using MiraAPI.Utilities;
using MiraAPI.Utilities.Assets;
using TownOfUs.Assets;
using TownOfUs.Buttons;
using TownOfUs.Modules;
using TownOfUs.Networking;
using TownOfUs.Modifiers;
using TownOfUs.Utilities;
using TownOfUs.Utilities.Appearances;
using UnityEngine;

namespace JAM.Buttons.Neutral;

public class ZombieReviveButton : TownOfUsButton
{
    public override string Name => "REVIVE";
    public override BaseKeybind Keybind => Keybinds.PrimaryAction;
    public override Color TextOutlineColor => Colors.Zombie;
    public override float Cooldown => OptionGroupSingleton<ZombieOptions>.Instance.ZombieReviveCd;
    public override bool ZeroIsInfinite { get; set; } = true;
    public override LoadableAsset<Sprite> Sprite => TouCrewAssets.ReviveSprite;

    public override bool Enabled(RoleBehaviour? role)
    {
        return role is ZombieRole || role is ZombieLeaderRole;
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
        return Helpers.GetNearestDeadBodies(PlayerControl.LocalPlayer.transform.position, ShipStatus.Instance.MaxLightRadius * 0.1f, Helpers.CreateFilter(Constants.NotShipMask)).Any(b => MiscUtils.PlayerById(b.ParentId).Data.Role is not ZombieRole);
    }

    // public override bool CanClick()
    // {
    //     return Helpers.GetNearestDeadBodies(PlayerControl.LocalPlayer.transform.position, ShipStatus.Instance.MaxLightRadius * 0.1f, Helpers.CreateFilter(Constants.NotShipMask)).Any(b => MiscUtils.PlayerById(b.ParentId).Data.Role is not ZombieRole);
    // }

    protected override void OnClick()
    {
        List<DeadBody> bodiesInRange = Helpers.GetNearestDeadBodies(PlayerControl.LocalPlayer.transform.position, ShipStatus.Instance.MaxLightRadius * 0.1f, Helpers.CreateFilter(Constants.NotShipMask)).Where(b => !(MiscUtils.PlayerById(b.ParentId).GetRoleWhenAlive() is ZombieLeaderRole)).ToList();

        if (bodiesInRange.Count > 0)
        {
            SetZombieRole(MiscUtils.PlayerById(bodiesInRange[0].ParentId), bodiesInRange[0]);
        }
    }

    public static void SetZombieRole(PlayerControl player, DeadBody body)
    {
        foreach (BaseModifier modifier in player.GetModifiers<BaseModifier>().Where(m => m is not IVisualAppearance))
        {
            player.RpcRemoveModifier(modifier.UniqueId);
        }

        player.RpcFullRevive(false, body.TruePosition, RoleId.Get<ZombieRole>(), player.Data.Role is not ZombieRole);

        ZombieLeaderRole.hasZombies = true;
    }
}
