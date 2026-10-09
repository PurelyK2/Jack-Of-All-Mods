using BepInEx;
using BepInEx.Configuration;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;
using JAM.Assets;
using JAM.CommsPatches;
using MiraAPI;
using MiraAPI.PluginLoading;
using PerfectComms.Api;
using Reactor;
using Reactor.Networking;
using Reactor.Networking.Attributes;
using Reactor.Utilities;
using System.Globalization;
using TownOfUs;
using UnityEngine;

namespace JAM;

[BepInAutoPlugin("com.JackOfAllMods.mod", "JackOfAllMods", "1.0.1")]
[BepInProcess("Among Us.exe")]
[BepInDependency(ReactorPlugin.Id)]
[BepInDependency(MiraApiPlugin.Id)]
[BepInDependency(TownOfUsPlugin.Id)]
[BepInDependency("com.edgetel.perfectcomms", BepInDependency.DependencyFlags.SoftDependency)]
[ReactorModFlags(ModFlags.RequireOnAllClients)]
public partial class Plugin : BasePlugin, IMiraPlugin
{
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
        ReactorCredits.Register("Jack of All Mods", Version, IsDevBuild, ReactorCredits.AlwaysShow);

        try
        {
            Harmony.PatchAll();
        }
        catch(System.Exception e)
        {
            _ = ConstantlyError(e.ToString());
        }

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
}

public enum JAMRpcCalls : uint
{
    ScrubModifiers = 0,
    DeceiverShield = 1,
    MicromanageTask = 2
}

// Concealed looks odd... (icon in-game)

// =============== FIXES ===============
/*
 * Fixed Bounty Target Speed Modifier
 * Fixed Concealed Options Appearance
 * Micromanager Can No Longer Manager Alliance Modified Crew
 * Zombies Can No Longer Call Meetings
 * Zombie Leader Is Now Guessable Until Final 3
 * Made Concealed And Shy Mutually Exclusive
 * You Are No Longer Concealed During Camo Comms
 * Adjusted Wording In Bounty Hunter Options
 * Micromanager Task Completion Names Look Right
 * Changed Gossip Sprite
 * Slighty Brightened Bounty Hunter's Color
 * Deceiver Shield Goes Away When Deceiver Is Dead Now
 * Changed Bounty Hunter Sprite
 * Dead no longer get bounty hunting button
 * Removed Warden Shield From Deceiver Shields
 * Jack Of All with Assassin No Longer Continues Game When Crew Should Win
 * Fixed Shackled Modifier not working
 * Fixed a bunch of namespaces being wrong and causing errors
 * Fixed Concealed Modifier overlapping with TOUM Concealed Modifier
 * Bounty Now Correctly Goes Away When Bounty Hunter Dies
 * Bounty Target Can Now Optionally Have A Timed Period Where It Can Be Caught
 * Deceiver Shield Now Shows Up Correctly And Hides In Comms Correctly
 * Added "Indecisive" Role
 * Made 2 Versions Of Indecisive To Test Out (Toggle With In-Game Setting)
*/