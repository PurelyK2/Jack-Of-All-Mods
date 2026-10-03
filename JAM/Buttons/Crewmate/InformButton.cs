using JAM.Modifiers.Crewmate;
using MiraAPI.GameOptions;
using MiraAPI.Keybinds;
using MiraAPI.Modifiers;
using MiraAPI.Utilities.Assets;
using Reactor.Utilities;
using JAM.Options.Roles.Crewmate;
using JAM.Roles.Crewmate;
using TownOfUs.Assets;
using TownOfUs.Buttons;
using TownOfUs.Extensions;
using TownOfUs.Modules;
using TownOfUs.Utilities;
using UnityEngine;
using MiraAPI.Roles;
using MiraAPI.Utilities;

namespace JAM.Buttons.Crewmate;

public sealed class InformButton : TownOfUsRoleButton<InformantRole, PlayerControl>
{
    public override string Name => "INVESTIGATE";
    public override BaseKeybind Keybind => Keybinds.PrimaryAction;
    public override Color TextOutlineColor => Colors.Informant;
    public override float Cooldown => OptionGroupSingleton<InformantOptions>.Instance.InformantCooldown;
    public override LoadableAsset<Sprite> Sprite => TouRoleIcons.Forensic;

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
            Error("Informant Investigate: Target is null");
            return;
        }

        foreach (PlayerControl player in PlayerControl.AllPlayerControls)
        {
            if (player.HasModifier<InformantInfoModifier>())
            {
                player.RemoveModifier<InformantInfoModifier>();
            }
        }


        if (OptionGroupSingleton<InformantOptions>.Instance.ShareInfo)
            Target.RpcAddModifier(typeof(InformantInfoModifier), new object[] { string.Join("|", InformantInfoModifier.GenerateInfoRoles(Target).Select(r => r.GetRoleName()).ToList()) });
        else
        {
            InformantInfoModifier InformantModifier = new InformantInfoModifier(InformantInfoModifier.GenerateInfoRoles(Target));
            Target.AddModifier(InformantModifier);
        }

        string notifyString = "You are investigating " + Target.Data.PlayerName + ".\nYou will " + (OptionGroupSingleton<InformantOptions>.Instance.ShareInfo ? "tell everyone" : "learn") + " something about them next meeting.";
        MiraAPI.Utilities.Helpers.CreateAndShowNotification(notifyString, Colors.Informant, new Vector3(0f, 1f, -20f), null, TouRoleIcons.Forensic.LoadAsset());
    }
}
