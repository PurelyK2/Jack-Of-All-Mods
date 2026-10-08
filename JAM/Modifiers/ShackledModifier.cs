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

public sealed class ShackledModifier : UniversalGameModifier, IWikiDiscoverable
{
    public override ModifierUiConfiguration Configuration => new(Colors.Shackled, TmpSpriteUtils.CreateSpriteAsset(JamModifierIcons.Shackled.LoadAsset(), "Shackled", 1.45f));
    public override string IdPart => "Shackled";
    public override string ModifierName => "Shackled";
    public override string IntroInfo => "Shackle your Killer!";

    public override string GetDescription()
    {
        return "Shackle your Killer forcing them to drag your dead body";
    }

    public string GetAdvancedDescription()
    {
        return "The Shackled Modifier is a Universal Postmortem modifier that causes " +
        "your killer to drag your dead body along with them for a specified duration." +
        MiscUtils.AppendOptionsText(GetType());
    }
    public string RoleMedDescriptionLocale => "Shackle your killer, Chaining them to your dead body !";

    public override LoadableAsset<Sprite>? ModifierIcon => JamModifierIcons.Shackled;

    public override ModifierFaction FactionType => ModifierFaction.UniversalPostmortem;
    public override Color FreeplayFileColor => Colors.Shackled;

    public List<CustomButtonWikiDescription> Abilities { get; } = [];

    public override int GetAssignmentChance()
    {
        return (int)OptionGroupSingleton<ShackledOptions>.Instance.ShackledChance;
    }

    public override int GetAmountPerGame()
    {
        return (int)OptionGroupSingleton<ShackledOptions>.Instance.ShackledAmount;
    }
}
