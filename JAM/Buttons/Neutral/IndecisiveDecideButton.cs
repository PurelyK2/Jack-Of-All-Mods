using AmongUs.GameOptions;
using JAM.Modules;
using JAM.Options.Roles.Neutral;
using JAM.Roles.Neutral;
using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using MiraAPI.Roles;
using MiraAPI.Utilities.Assets;
using TownOfUs.Assets;
using TownOfUs.Buttons;
using TownOfUs.Modifiers.Game.Alliance;
using TownOfUs.Utilities;
using UnityEngine;
using static JAM.Modules.IndeciveDecideMinigame;

namespace JAM.Buttons.Neutral;

public sealed class IndecisiveDecideButton : TownOfUsRoleButton<IndecisiveRole>
{
    public override float Cooldown => 0;
    public override string Name => "DECIDE";
    public override LoadableAsset<Sprite> Sprite => TouRoleIcons.Agent;

    public override bool Enabled(RoleBehaviour? role)
    {
        return base.Enabled(role) && OptionGroupSingleton<IndecisiveOptions>.Instance.RoleStyle == IndecisiveOptions.IndecisiveStyle.Decider;
    }

    protected override void OnClick()
    {
        if (!Minigame.Instance)
        {
            var decisionMenu = IndeciveDecideMinigame.Create();
            decisionMenu.Open(
                choice =>
                {
                    switch(choice)
                    {
                        case IndecisiveChoice.Crewmate:
                            Role.Player.RpcChangeRole((ushort)RoleTypes.Crewmate);
                            break;
                        case IndecisiveChoice.Impostor:
                            Role.Player.RpcAddModifier<EgotistModifier>();
                            Role.Player.RpcChangeRole((ushort)RoleTypes.Crewmate);
                            break;
                    }
                    decisionMenu.Close();
                }
            );
        }
    }
}
