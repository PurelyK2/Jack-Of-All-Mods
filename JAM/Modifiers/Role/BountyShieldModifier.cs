using MiraAPI.GameOptions;
using MiraAPI.LocalSettings;
using MiraAPI.Modifiers;
using MiraAPI.Utilities.Assets;
using Reactor.Utilities.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TownOfUs;
using TownOfUs.Assets;
using TownOfUs.Modifiers;
using TownOfUs.Modules.Anims;
using TownOfUs.Options;
using TownOfUs.Patches;
using TownOfUs.Roles.Other;
using TownOfUs.Utilities.Appearances;
using UnityEngine;

namespace JAM.Modifiers;

public sealed class BountyShieldModifier : BaseShieldModifier, IAnimated
{
    public bool collectedThisRound = true;

    public override string ModifierName => "Shield Reward";
    public override LoadableAsset<Sprite>? ModifierIcon => TouModifierIcons.FirstRoundShield;
    public override Color FreeplayFileColor => Color.white;

    public GameObject BountyShield { get; set; }
    public bool IsVisible { get; set; } = false;

    public void Update()
    {
        IsVisible = Player.AmOwner;
    }

    public override string GetDescription()
    {
        return "A Shield Gained As A Reward! Have It Until The End Of The Next Round.";
    }

    public override void OnActivate()
    {
        BountyShield = AnimStore.SpawnAnimBody(Player, TouAssets.FirstRoundShield.LoadAsset(), false, -1.1f, -0.225f, 1.5f)!;
    }

    public override void OnDeath(DeathReason reason)
    {
        base.OnDeath(reason);

        BountyShield?.SetActive(false);
        ModifierComponent!.RemoveModifier(this);
    }

    public override void OnDeactivate()
    {
        if (BountyShield)
        {
            BountyShield.Destroy();
        }
    }

    public override void OnMeetingStart()
    {
        base.OnMeetingStart();

        if(!collectedThisRound)
        {
            ModifierComponent.RemoveModifier(this);
        }
        else
        {
            collectedThisRound = false;
        }
    }
}
