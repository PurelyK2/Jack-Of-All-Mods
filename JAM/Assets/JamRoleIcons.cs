using MiraAPI.Utilities.Assets;
using UnityEngine;

namespace JAM.Assets;

public static class JamRoleIcons
{
    // THIS FILE SHOULD ONLY HOLD ROLE ICONS

    private const string ShortPath = "JAM.Resources.RoleIcons";

    // K2 Roles
    public static LoadableAsset<Sprite> Mimic { get; } = new LoadableResourceAsset($"{ShortPath}.Mimic.png", 200);
    public static LoadableAsset<Sprite> Zombie { get; } = new LoadableResourceAsset($"{ShortPath}.Zombie.png", 200);
    public static LoadableAsset<Sprite> ZombieLeader { get; } = new LoadableResourceAsset($"{ShortPath}.ZombieLeader.png", 200);
    public static LoadableAsset<Sprite> TimeKeeper { get; } = new LoadableResourceAsset($"{ShortPath}.TimeKeeper.png", 200);
    public static LoadableAsset<Sprite> JackOfAll { get; } = new LoadableResourceAsset($"{ShortPath}.JackOfAll.png", 200);
    public static LoadableAsset<Sprite> Deceiver { get; } = new LoadableResourceAsset($"{ShortPath}.Deceiver.png", 200);
    public static LoadableAsset<Sprite> BountyHunter { get; } = new LoadableResourceAsset($"{ShortPath}.BountyHunter.png", 200);
    public static LoadableAsset<Sprite> Gossip { get; } = new LoadableResourceAsset($"{ShortPath}.Gossip.png", 200);
    public static LoadableAsset<Sprite> Scrubber { get; } = new LoadableResourceAsset($"{ShortPath}.Scrubber.png", 200);
    // Nulls Roles
    public static LoadableAsset<Sprite> Micromanager { get; } = new LoadableResourceAsset($"{ShortPath}.Micromanager.png", 200);
    public static LoadableAsset<Sprite> Workaholic { get; } = new LoadableResourceAsset($"{ShortPath}.Workaholic.png", 200);
    public static LoadableAsset<Sprite> Mortician { get; } = new LoadableResourceAsset($"{ShortPath}.Mortician.png", 200);
    public static LoadableAsset<Sprite> Camouflager { get; } = new LoadableResourceAsset($"{ShortPath}.Camouflager.png", 200);
    public static LoadableAsset<Sprite> Projector { get; } = new LoadableResourceAsset($"{ShortPath}.Projector.png", 200);
}
