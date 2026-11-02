using JAM.Assets;
using JAM.Options.Modifiers;
using MiraAPI.GameOptions;
using MiraAPI.Utilities.Assets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TownOfUs.Modifiers;
using TownOfUs.Modifiers.Game;
using TownOfUs.Utilities;
using UnityEngine;

namespace JAM.Modifiers;

public sealed class FoolishModifier : UniversalGameModifier
{
    public static float votingTimeLeft = 0f;

    public override void Update()
    {
        base.Update();

        if(votingTimeLeft > 0f)
        {
            votingTimeLeft -= Time.deltaTime;
        }
    }

    public override string ModifierName => "Foolish";

    public override string GetDescription()
    {
        return "Vote Quickly To Get Double Votes";
    }

    public string GetAdvancedDescription()
    {
        return GetDescription() + MiscUtils.AppendOptionsText(base.GetType());
    }

    public override ModifierFaction FactionType => ModifierFaction.UniversalVisibility;

    public override int GetAssignmentChance()
    {
        return (int)OptionGroupSingleton<FoolishOptions>.Instance.FoolishChance;
    }

    public override ModifierUiConfiguration Configuration
    {
        get
        {
            return new ModifierUiConfiguration(Colors.Foolish, TmpSpriteUtils.CreateSpriteAsset(JamModifierIcons.Foolish.LoadAsset(), "TouMira.Modifier.Game.Universal.Foolish", 1.45f));
        }
    }

    public override bool HideOnUi => false;

    public override LoadableAsset<Sprite> ModifierIcon => JamModifierIcons.Foolish;

    public override int GetAmountPerGame()
    {
        return CustomAmount;
    }

    public override int CustomAmount => (int)OptionGroupSingleton<FoolishOptions>.Instance.FoolishAmount;

    public override int CustomChance => (int)OptionGroupSingleton<FoolishOptions>.Instance.FoolishChance;
}
