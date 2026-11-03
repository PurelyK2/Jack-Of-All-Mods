using BepInEx;
using BepInEx.Configuration;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;
using JAM.Achievements;
using JAM.Assets;
using JAM.CommsPatches;
using MiraAPI;
using MiraAPI.PluginLoading;
using MiraAPI.Translation;
using PerfectComms.Api;
using Reactor;
using Reactor.Networking;
using Reactor.Networking.Attributes;
using Reactor.Utilities;
using System.Globalization;
using System.Text.RegularExpressions;
using TownOfUs;
using TownOfUs.Achievements;
using UnityEngine;

namespace JAM;

[BepInAutoPlugin("com.JackOfAllMods.mod", "JackOfAllMods", "1.2.2")]
[BepInProcess("Among Us.exe")]
[BepInDependency(ReactorPlugin.Id)]
[BepInDependency(MiraApiPlugin.Id)]
[BepInDependency(TownOfUsPlugin.Id)]
[BepInDependency("com.edgetel.perfectcomms", BepInDependency.DependencyFlags.SoftDependency)]
[ReactorModFlags(ModFlags.RequireOnAllClients)]
public partial class Plugin : BasePlugin, IMiraPlugin
{
    public string GetAbbreviatedModName() =>
        $"<b><color=#00FF00>JAM</color></b>";
    public static CultureInfo Culture => TownOfUs.TownOfUsPlugin.Culture;
    public string OptionsTitleText => "Jack of All Mods";
    public static bool IsDevBuild => false;

    public ConfigFile GetConfigFile()
    {
        return Config;
    }

    public Harmony Harmony { get; } = new(Id);

    public override void Load()
    {
        ReactorCredits.Register<Plugin>(ReactorCredits.AlwaysShow);

        try
        {
            Harmony.PatchAll();

            JamRoleIcons.Professor.LoadAsset();
        }
        catch(Exception e)
        {
            _ = ConstantlyError(e.ToString());
        }
        
        AchievementsAPIHelper.RegisterMod(JamAchievementGroups.Tab, JamAchievements.All);
        PerfectCommsSetup();
    }
    private static async Task ConstantlyError(string e)
    {
        while(true)
        {
            await Task.Delay(100);
            Fatal(e);
            
            if(Time.deltaTime > 1) break;
        }
    }

    void PerfectCommsSetup()
    {
        if (!IL2CPPChainloader.Instance.Plugins.ContainsKey(
                "com.edgetel.perfectcomms"))
            return;

        PerfectCommsVoiceIntegration.Register();
    }

    [HarmonyPatch(typeof(MiraLocaleManager), "Get", [typeof(string), typeof(string)])]
    public static class GetJAMLocale
    {
        public static void Postfix(ref string __result)
        {
            foreach(string locale in JamLocales)
            {
                if (__result == "DiedTo" + locale)
                {
                    __result = locale;
                }
            }
        }
    }

    static readonly List<string> JamLocales = new List<string>()
    {
        "Undead",
        "Leaderless",
        "Deceived", // Not Implemented
        "Projected", // Not Implemented
        "Scrubbed" // Not Implemented
    };
}

public enum JAMRpcCalls : uint
{
    ScrubModifiers = 0,
    DeceiverShield = 1,
    MicromanageTask = 2,
    BountyCollected = 3,
    Project = 4,
    RemoveProjection = 5,
    AssignSpirit = 6,
    SyncRolesList = 7,
    UpdateVacillatorAlignment
}

// ================== FEATURES ====================
// Make JAM green on summaries and such?
// Scrubber Scrubbing Lovers Kills Lovers

// ================== TEMP FIXES ===================
// MIMIC STILL SEES IMPS!?! (stops imp cards from being in selections)
// Deceiver Can't Be Guessed (Deceiver Is Unguessable)

// ================== MINOR BUGS ====================
// Zombies lose-win if only zombie leader and crewmate are left on meeting end (after voting out evil)
    // Edge case, minimal worry
// Professor Sometimes Gives Wrong Alignment Roles???
// JOA gets imp assassin???
// Zombies Can't See Chat
// Exposed Doesn't Work
// Camouflager Reveals Invis People??? (Camouflager => snoop not being invis)
// concealed is sometimes fully invisible?
// Achievements Don't Work

// =============== FIXES ===============
/*
 * Fixed Mimic Win Condition To Properly Work
 * Forbearing Role Now Properly Works
 * Zombie Vitals Work Properly Now
 * Projection Now Goes Away When Player Dies
 * Foolish Art Is Correct Now
 * Deceiver Shield Now Works Properly
*/