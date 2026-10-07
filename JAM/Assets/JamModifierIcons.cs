using MiraAPI.Utilities.Assets;
using UnityEngine;

namespace JAM.Assets;

public static class JamModifierIcons
{
    private const string ShortPath = "JAM.Resources.ModifierIcons";

    // K2 Modifiers
    public static LoadableAsset<Sprite> Blind { get; } = new LoadableResourceAsset($"{ShortPath}.Blind.png", 200);
    public static LoadableAsset<Sprite> Rivalry { get; } = new LoadableResourceAsset($"{ShortPath}.Rivalry.png", 200);
    public static LoadableAsset<Sprite> Unstable { get; } = new LoadableResourceAsset($"{ShortPath}.Unstable.png", 200);
    public static LoadableAsset<Sprite> Ventable { get; } = new LoadableResourceAsset($"{ShortPath}.Ventable.png", 200);
    public static LoadableAsset<Sprite> Concealed { get; } = new LoadableResourceAsset($"{ShortPath}.Concealed.png", 200);

    // Nulls Modifiers
    public static LoadableAsset<Sprite> Battery { get; } = new LoadableResourceAsset($"{ShortPath}.Battery.png", 200);
    public static LoadableAsset<Sprite> Exposed { get; } = new LoadableResourceAsset($"{ShortPath}.Exposed.png", 200);
    public static LoadableAsset<Sprite> Shackled { get; } = new LoadableResourceAsset($"{ShortPath}.Shackled.png", 200);
}
