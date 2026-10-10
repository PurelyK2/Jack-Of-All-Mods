using JAM.Modifiers.Hidden;
using MiraAPI.GameOptions;
using MiraAPI.Keybinds;
using MiraAPI.Modifiers;
using MiraAPI.Utilities.Assets;
using Reactor.Utilities;
using JAM.Options.Roles.Crewmate;
using JAM.Roles.Crewmate;
using TownOfUs.Assets;
using TownOfUs.Buttons;
using TownOfUs.Utilities;
using UnityEngine;
using MiraAPI.Utilities;
using JAM.Assets;

namespace JAM.Buttons.Crewmate;

public sealed class OverhearButton : TownOfUsRoleButton<GossipRole, PlayerControl>
{
    public override string Name => "OVERHEAR";
    public override BaseKeybind Keybind => Keybinds.PrimaryAction;
    public override Color TextOutlineColor => Colors.Gossip;
    public override float Cooldown => OptionGroupSingleton<GossipOptions>.Instance.GossipCooldown;
    public override LoadableAsset<Sprite> Sprite => Assets.JamAssets.GossipOverhear;

    public override void CreateButton(Transform parent)
    {
        base.CreateButton(parent);
        Coroutines.Start(MiscUtils.CoMoveButtonIndex(this, false));
    }

    public override PlayerControl? GetTarget()
    {
        return PlayerControl.LocalPlayer.GetClosestLivingPlayer(true, Distance);
    }

    protected override void OnClick()
    {
        if (Target == null)
        {
            Error("Gossip Overhear: Target is null");
            return;
        }

        foreach (PlayerControl player in PlayerControl.AllPlayerControls)
        {
            if (player.HasModifier<GossipOverhearModifier>())
            {
                player.RemoveModifier<GossipOverhearModifier>();
            }
        }


        if (OptionGroupSingleton<GossipOptions>.Instance.ShowGossip)
            Target.RpcAddModifier(typeof(GossipOverhearModifier), new object[] { string.Join("|", GossipOverhearModifier.GenerateGossipRoles(Target).Select(r => r.GetRoleName()).ToList()) });
        else
        {
            GossipOverhearModifier gossipModifier = new GossipOverhearModifier(GossipOverhearModifier.GenerateGossipRoles(Target));
            Target.AddModifier(gossipModifier);
        }

        string notifyString = "You are overhearing " + Target.Data.PlayerName + ".\nYou will " + (OptionGroupSingleton<GossipOptions>.Instance.ShowGossip ? "tell everyone" : "learn") + " something about them next meeting.";
        MiraAPI.Utilities.Helpers.CreateAndShowNotification(notifyString, Colors.Gossip, new Vector3(0f, 1f, -20f), null, JamAssets.GossipOverhear.LoadAsset());
    }
}
