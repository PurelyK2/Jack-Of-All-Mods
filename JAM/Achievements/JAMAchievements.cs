using JAM.Assets;
using MiraAPI.Utilities.Assets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using TownOfUs;
using TownOfUs.Achievements;
using TownOfUs.Assets;
using UnityEngine;
using static Il2CppSystem.Reflection.RuntimePropertyInfo;
using static TMPro.SpriteAssetUtilities.TexturePacker_JsonArray;
using static UnityEngine.GraphicsBuffer;

namespace JAM.Achievements;

internal static class JamAchievementGroups
{
    private static readonly Assembly MuAssembly = typeof(Plugin).Assembly;

    public static readonly AchievementTabSpec Tab = new(
        MuAssembly, "Jack Of All Mods", "Jack Of All Mods", "general",
        TabLocaleKey: "JAM.Achievements.Tab",
        LocalePrefix: "JAM.Achievement",
        TabIcon: JamRoleIcons.JackOfAll, ModIcon: JamAssets.GossipOverhear,
        TabColor: Colors.Zombie);
}

public static class JamAchievements
{
    private static AchievementSpec Define(string name, string description, LoadableAsset<Sprite> icon,
        LoadableAsset<Sprite>? subIcon = null, string? subIconTooltip = null, int rarity = 0,
        bool hidden = false, int progressGoal = 0,
        AchievementPersistence persistence = AchievementPersistence.ThroughoutSessions,
        bool toastOnGameEnd = false, string? categoryOverride = null, string? idOverride = null,
        bool hideProgress = false, string[]? tags = null, [CallerMemberName] string key = "")
        => new(key, name, description, icon, subIcon, rarity, hidden, progressGoal, persistence,
            toastOnGameEnd, categoryOverride, idOverride, HideProgress: hideProgress, Tags: tags,
            SubIconTooltip: subIconTooltip);

    // Mimic
    public static AchievementSpec DIYSwooper = Define("DIY Swooper", "Kill As A Snoop", JamRoleIcons.Snoop);
    public static AchievementSpec ThereCanOnlyBeOne = Define("There Can Only Be One", "Kill Someone Whose Role You're Mimicking", TouModifierIcons.Colorblind);

    // Jack Of All
    public static AchievementSpec DIYMayor = Define("DIY Mayor", "Get 2 Bonus Votes From Tasks As Jack Of All", JamRoleIcons.JackOfAll);
    public static AchievementSpec Unkillable = Define("Unkillable", "Get Bait + Celebrety As Jack Of All", TouModifierIcons.Bait);

    // Deceiver
    public static AchievementSpec GetBaited = Define("Get Baited", "Be Guessed As Medic When Deceiver", JamRoleIcons.Deceiver);
    public static AchievementSpec Deceived = Define("Deceived", "Deceive 3 People", JamRoleIcons.Deceiver);
    public static AchievementSpec Psych = Define("Psych!", "Kill The Person You Shielded As Deceiver", JamRoleIcons.Deceiver);

    // Vacillator
    public static AchievementSpec IShotTheSheriff = Define("I Shot The Sheriff", "Misfire On A Crewmate As Vacillator", TouModifierIcons.Colorblind);
    public static AchievementSpec DespicableMe = Define("Despicable Me", "Misfire On An Evil As Vacillator", TouModifierIcons.Colorblind);

    // Forbearing
    public static AchievementSpec FineIllDoItMyself = Define("Fine, I'll Do It Myself", "Have 0 Kill Cooldown As Forbearing", TouRoleIcons.Jackal);
    public static AchievementSpec Boring = Define("BORING!", "Have Your Cooldown Reduced As Forbearing", TouRoleIcons.Jackal);

    // Zombie Leader
    public static AchievementSpec Apocolypse = Define("Apocolypse", "Win As Zombie Leader", JamRoleIcons.ZombieLeader);
    public static AchievementSpec Arise = Define("ARISE!", "Personally Revive 3 People As Zombies In 1 Game", JamRoleIcons.ZombieLeader, persistence: AchievementPersistence.ThroughoutRounds);
    public static AchievementSpec Infected = Define("Infected", "Become A Zombie", JamRoleIcons.Zombie);

    // Projector
    public static AchievementSpec Poof = Define("Poof!", "Kill While Projected", JamRoleIcons.Projector);
    public static AchievementSpec OldFashioned = Define("Old-Fashioned", "Kill 3 Players Normally In A Single Game", JamRoleIcons.Projector);

    // Mortician
    public static AchievementSpec CaughtRedHanded = Define("Caught Red-Handed", "Perform An Autopsy On Someone Who Was Killed With Bait This Round", JamRoleIcons.Mortician);
    public static AchievementSpec UselessDegree = Define("Useless Degree", "Die Round 1 As Mortician", JamRoleIcons.Mortician);

    // Professor
    public static AchievementSpec PeerReviewed = Define("Peer Reviewed", "Get Killed By Someone You Retrained", JamRoleIcons.Professor);

    // Micromanager
    public static AchievementSpec EmployeeOfTheMonth = Define("Employee Of The Month", "Complete All Tasks As Micromanager", JamRoleIcons.Micromanager);
    
    public static readonly AchievementSpec[] All =
    [
    /*
        DIYSwooper, ThereCanOnlyBeOne,
        DIYMayor, Unkillable,
        GetBaited, Deceived, Psych,
        DespicableMe, IShotTheSheriff,
        FineIllDoItMyself, Boring,
        Apocolypse, Arise, Infected,
        Poof, OldFashioned, 
        CaughtRedHanded, UselessDegree
    */
    ];
}
