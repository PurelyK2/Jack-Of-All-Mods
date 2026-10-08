using JAM.Assets;
using JAM.Options.Modifiers;
using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using MiraAPI.Utilities.Assets;
using TownOfUs.Modifiers;
using TownOfUs.Modifiers.Game;
using TownOfUs.Modules.Wiki;
using TownOfUs.Utilities;
using UnityEngine;

namespace JAM.Modifiers;

 
public sealed class VentableModifier : TouGameModifier, IWikiDiscoverable
{
     
    public override string ModifierName => "Ventable";

    public override bool HideFromGuessing => true;
    
     
    public override string IntroInfo => "You can vent!";

     
    public override string GetDescription()
    {
        return IntroInfo;
    }
     
    public string GetAdvancedDescription()
    {
        return GetDescription() + MiscUtils.AppendOptionsText(base.GetType());
    }
     
    public override ModifierFaction FactionType => ModifierFaction.UniversalUtility;

     
    public override int GetAssignmentChance()
    {
        return (int)OptionGroupSingleton<VentableOptions>.Instance.VentableChance;
    }
     
    public override bool IsModifierValidOn(RoleBehaviour role)
    {
        return !role.CanVent && base.IsModifierValidOn(role);
    }
     
    public override bool? CanVent()
    {
        return true;
    }

    public override ModifierUiConfiguration Configuration
    {
        get
        {
            return new ModifierUiConfiguration(Colors.Ventable, TmpSpriteUtils.CreateSpriteAsset(JamModifierIcons.Blind.LoadAsset(), "TouMira.Modifier.Game.Universal.Blind", 1.45f));
        }
    }

     
    public override float IntroSize => 3f;
     
    public override bool HideOnUi => false;
     
    public override LoadableAsset<Sprite> ModifierIcon => JamModifierIcons.Ventable;
     
    public override int GetAmountPerGame()
    {
        return CustomAmount;
    }
     
    public override int CustomAmount => (int)OptionGroupSingleton<VentableOptions>.Instance.VentableCount;
     
    public override int CustomChance => (int)OptionGroupSingleton<VentableOptions>.Instance.VentableChance;

    public override void Update()
    {
        base.Update();

        if(Player.Data.Role.CanVent)
        {
            Player.RpcRemoveModifier<VentableModifier>();
        }
    }
}
