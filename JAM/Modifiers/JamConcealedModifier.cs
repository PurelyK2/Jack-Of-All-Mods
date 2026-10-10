using HarmonyLib;
using JAM.Assets;
using JAM.Options.Modifiers;
using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using MiraAPI.Utilities.Assets;
using TownOfUs.Modifiers;
using TownOfUs.Modifiers.Game;
using TownOfUs.Modifiers.Game.Universal;
using TownOfUs.Modules.Wiki;
using TownOfUs.Options.Maps;
using TownOfUs.Utilities;
using UnityEngine;

namespace JAM.Modifiers;

 
public sealed class JamConcealedModifier : UniversalGameModifier, IWikiDiscoverable
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
        return base.IsModifierValidOn(role) && !role.Player.HasModifier<ShyModifier>();
    }
    [HarmonyPatch(typeof(ShyModifier), nameof(ShyModifier.IsModifierValidOn))]
    public static class MakeShyExclusive
    {
        public static void Postfix(RoleBehaviour role, ref bool __result)
        {
            if (role.Player.HasModifier<ConcealedModifier>())
            {
                __result = false;
            }
        }
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

        bool commsActive;

        switch ((ExpandedMapNames)GameOptionsManager.Instance.currentGameOptions.MapId)
        {
            case ExpandedMapNames.MiraHq:
            case ExpandedMapNames.Fungle:
                var hqComms = ShipStatus.Instance.Systems[SystemTypes.Comms]
                    .Cast<HqHudSystemType>();

                commsActive = hqComms.IsActive;
                break;

            default:
                var hudComms = ShipStatus.Instance.Systems[SystemTypes.Comms]
                    .Cast<HudOverrideSystemType>();

                commsActive = hudComms.IsActive;
                break;
        }

        if (!Player.Data.IsDead && !(commsActive && TownOfUsMapOptions.IsCamoCommsOn()) && !Player.HasModifier<CamouflagerCamoModifier>())
        {
            ShyModifier.SetVisibility(Player, OptionGroupSingleton<ConcealedOptions>.Instance.ConcealedOpacity / 100f);
        }
    }
}
