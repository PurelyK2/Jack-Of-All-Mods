using JAM.Assets;
using JAM.Options.Modifiers;
using MiraAPI.GameOptions;
using MiraAPI.Utilities.Assets;
using TownOfUs.Modifiers;
using TownOfUs.Modifiers.Game;
using TownOfUs.Modules.Wiki;
using TownOfUs.Utilities;
using UnityEngine;

namespace JAM.Modifiers;

 
public sealed class BlindModifier : TouGameModifier, IWikiDiscoverable
{
     
    public override string ModifierName => "Blind";
    public override string IntroInfo => "Your vision is reduced by " + (int)OptionGroupSingleton<BlindOptions>.Instance.BlindAmount + "%";
    public override bool HideFromGuessing => true;
    public override string GetDescription()
    {
        return IntroInfo;
    }
    public override ModifierUiConfiguration Configuration
    {
        get
        {
            return new ModifierUiConfiguration(Colors.Blind, TmpSpriteUtils.CreateSpriteAsset(JamModifierIcons.Blind.LoadAsset(), "TouMira.Modifier.Game.Universal.Blind", 1.45f));
        }
    }
     
    public string GetAdvancedDescription()
    {
        return GetDescription() + MiscUtils.AppendOptionsText(base.GetType());
    }
     
    public override ModifierFaction FactionType => ModifierFaction.UniversalUtility;

     
    public override int GetAssignmentChance()
    {
        return (int)OptionGroupSingleton<BlindOptions>.Instance.BlindChance;
    }
     
    public override float IntroSize => 3f;
     
    public override bool HideOnUi => false;
     
    public override LoadableAsset<Sprite> ModifierIcon => JamModifierIcons.Blind;
     
    public override int GetAmountPerGame()
    {
        return CustomAmount;
    }
     
    public override int CustomAmount => (int)OptionGroupSingleton<BlindOptions>.Instance.BlindCount;
     
    public override int CustomChance => (int)OptionGroupSingleton<BlindOptions>.Instance.BlindChance;
}
