using HarmonyLib;
using JAM.Assets;
using JAM.Options.Modifiers.Game.Universal;
using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using MiraAPI.Roles;
using MiraAPI.Utilities.Assets;
using TownOfUs.Assets;
using TownOfUs.Modifiers;
using TownOfUs.Modifiers.Game;
using TownOfUs.Modifiers.Game.Universal;
using TownOfUs.Modules.Wiki;
using TownOfUs.Utilities;
using TownOfUs.Utilities.Appearances;
using UnityEngine;

namespace JAM.Modifiers.Game.Universal;

 
public sealed class ConcealedModifier : UniversalGameModifier, IWikiDiscoverable
{
     
    public override string ModifierName => "Concealed";
     
    public override string IntroInfo => "You Are Harder To See";

     
    public override string GetDescription()
    {
        return "You Are Slightly Less Opaque";
    }
     
    public string GetAdvancedDescription()
    {
        return GetDescription() + MiscUtils.AppendOptionsText(GetType());
    }
     
    public override ModifierFaction FactionType => ModifierFaction.UniversalVisibility;

     
    public override int GetAssignmentChance()
    {
        return (int)OptionGroupSingleton<ConcealedOptions>.Instance.ConcealedChance;
    }
     
    public override bool IsModifierValidOn(RoleBehaviour role)
    {
        return !role.CanVent && base.IsModifierValidOn(role);
    }

    public override ModifierUiConfiguration Configuration
    {
        get
        {
            return new ModifierUiConfiguration(Colors.Concealed, TmpSpriteUtils.CreateSpriteAsset(JamModifierIcons.Concealed.LoadAsset(), "TouMira.Modifier.Game.Universal.Concealed", 1.45f));
        }
    }

     
    public override float IntroSize => 3f;
     
    public override bool HideOnUi => false;
     
    public override LoadableAsset<Sprite> ModifierIcon => JamModifierIcons.Concealed;
     
    public override int GetAmountPerGame()
    {
        return CustomAmount;
    }
     
    public override int CustomAmount => (int)OptionGroupSingleton<ConcealedOptions>.Instance.ConcealedCount;
     
    public override int CustomChance => (int)OptionGroupSingleton<ConcealedOptions>.Instance.ConcealedChance;

    public override void Update()
    {
        base.Update();

        if (!Player.Data.IsDead)
        {
            ShyModifier.SetVisibility(Player, OptionGroupSingleton<ConcealedOptions>.Instance.ConcealedOpacity);
        }
        else
        {
            ShyModifier.SetVisibility(Player, 1, OptionGroupSingleton<ConcealedOptions>.Instance.ConcealName);
        }
    }
}
