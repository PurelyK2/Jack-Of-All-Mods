using MiraAPI.GameOptions;
using MiraAPI.Utilities.Assets;
using TownOfUs.Modules.Wiki;
using TownOfUs.Modifiers;
using TownOfUs.Utilities;
using TownOfUs.Modifiers.Game;
using UnityEngine;
using JAM.Options.Modifiers;
using JAM.Assets;

namespace JAM.Modifiers.Universal;

public sealed class BatteryModifier : UniversalGameModifier, IWikiDiscoverable
{
    public override ModifierUiConfiguration Configuration => new(Colors.Exposed, TmpSpriteUtils.CreateSpriteAsset(JamModifierIcons.Exposed.LoadAsset(), "Exposed", 1.45f));
    public override string IdPart => "Battery";
    public override string ModifierName => "Battery";
    public override string IntroInfo => "Use Utilities Whenever!";

    public override string GetDescription()
    {
        return "You can access Utilities during Comms Sabo";
    }

    public string GetAdvancedDescription()
    {
        return "The Battery is a Universal Utility modifier that lets you see " +
        "Utility(Admin table, Cameras, Vitals and Hq Logs) info during comms sabotage" +
        MiscUtils.AppendOptionsText(GetType());
    }
    public string RoleMedDescriptionLocale() => "Lets you use Utilities during Comms Disabled sabotage";

    public override LoadableAsset<Sprite>? ModifierIcon => JamModifierIcons.Battery;

    public override ModifierFaction FactionType => ModifierFaction.UniversalUtility;
    public override Color FreeplayFileColor => Colors.Battery;

    public List<CustomButtonWikiDescription> Abilities { get; } = [];

    public override int GetAssignmentChance()
    {
        return (int)OptionGroupSingleton<BatteryOptions>.Instance.BatteryChance;
    }

    public override int GetAmountPerGame()
    {
        return (int)OptionGroupSingleton<BatteryOptions>.Instance.BatteryAmount;
    }
}
