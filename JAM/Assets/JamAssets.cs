using MiraAPI.Utilities.Assets;
using UnityEngine;

namespace JAM.Assets;

public static class JamAssets
{
    private const string ShortPath = "JAM.Resources.Other";
    // K2 Buttons & Assets
    public static LoadableAsset<Sprite> BountyTarget { get; } = new LoadableResourceAsset($"{ShortPath}.Bounty Target.png", 200);
    public static LoadableAsset<Sprite> GossipOverhear { get; } = new LoadableResourceAsset($"{ShortPath}.GossipAbility.png", 200);
    // Nulls Buttons & Assets
    public static LoadableAsset<Sprite> CamouflagerButton { get; } = new LoadableResourceAsset($"{ShortPath}.CamouflagerButton.png", 200);
    public static LoadableAsset<Sprite> ProjectButton { get; } = new LoadableResourceAsset($"{ShortPath}.RecoilButton.png", 200);
    public static LoadableAsset<Sprite> RecoilButton { get; } = new LoadableResourceAsset($"{ShortPath}.RecoilButton.png", 200);

    public static LoadableAsset<Sprite> MorticianAbility { get; } = new LoadableResourceAsset($"{ShortPath}.MorticianAbility.png", 200);
}
