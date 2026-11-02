using JAM.Assets;
using JAM.Modifiers;
using JAM.Options.Roles.Impostor;
using JAM.Roles.Impostor;
using MiraAPI.GameOptions;
using MiraAPI.Keybinds;
using MiraAPI.Modifiers;
using MiraAPI.Utilities.Assets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TownOfUs.Buttons;
using TownOfUs.Utilities;
using UnityEngine;

namespace JAM.Buttons.Impostor;

public sealed class ScrubberScrubButton : TownOfUsRoleButton<ScrubberRole, PlayerControl>
{
    public override float Cooldown => GameManager.Instance.LogicOptions.GetKillCooldown();
    public override BaseKeybind? Keybind => Keybinds.PrimaryAction;
    public override LoadableAsset<Sprite> Sprite => JamRoleIcons.Scrubber;
    public override PlayerControl? GetTarget()
    {
        return Role.Player.GetClosestLivingPlayer(true, Distance);
    }
    public override bool Enabled(RoleBehaviour? role)
    {
        return base.Enabled(role) && OptionGroupSingleton<ScrubberOptions>.Instance.ScrubsInRounds;
    }
    protected override void OnClick()
    {
        if (Target == null)
        {
            Error("Scrubber Error: Target Is Null");
            return;
        }

        Target.RpcAddModifier<ScrubberScrubModifier>();
    }
}
