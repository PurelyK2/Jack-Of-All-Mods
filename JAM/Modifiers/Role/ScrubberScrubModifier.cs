using HarmonyLib;
using JAM.Assets;
using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.Modifiers;
using Reactor.Networking.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TownOfUs.Modifiers;
using TownOfUs.Modifiers.Game;
using TownOfUs.Modules.Wiki;
using TownOfUs.Utilities;

namespace JAM.Modifiers;

public sealed class ScrubberScrubModifier : BaseModifier
{
    public override string ModifierName => "Scrubber Scrub Modifier";
    public override bool HideOnUi => true;

    [RegisterEvent(0)]
    public static void OnRoundStartEvent(RoundStartEvent @event)
    {
        foreach(PlayerControl player in MiraAPI.Utilities.Helpers.GetAlivePlayers().Where(p => p.HasModifier<ScrubberScrubModifier>()))
        {
            if(player.AmOwner)
            {
                foreach (BaseModifier modifier in player.GetModifiers<BaseModifier>())
                {
                    if (modifier.HideOnUi) continue;
                    if (modifier is not TouGameModifier) continue;
                    if (modifier is TouGameModifier gameModifier && (gameModifier.GetAssignmentChance() == 0 || gameModifier.GetAmountPerGame() == 0)) continue;
                    if (modifier is BaseShieldModifier) continue;
                    if (modifier is AllianceGameModifier) continue;

                    player.RpcRemoveModifier(modifier.UniqueId);
                }
                MiraAPI.Utilities.Helpers.CreateAndShowNotification("Your Modifiers Have Been Scrubbed!", UnityEngine.Color.white, null, JamRoleIcons.Scrubber.LoadAsset());
                player.RpcAddModifier<ScrubbedModifier>();
            }
        }
    }
}
public sealed class ScrubbedModifier : TouGameModifier, IWikiDiscoverable
{
    public override string ModifierName => "Scrubbed";
    public override bool HideOnUi => true;
    public bool IsHiddenFromList => true;

    public override int GetAssignmentChance()
    {
        return 0;
    }
}
