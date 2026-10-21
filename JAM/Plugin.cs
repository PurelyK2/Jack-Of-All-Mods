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

[BepInAutoPlugin("com.JackOfAllMods.mod", "JackOfAllMods", "1.2")]
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

            JamRoleIcons.Professor.LoadAsset();
        }
        catch(Exception e)
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
    MicromanageTask = 2,
    BountyCollected = 3,
    Project = 4,
    RemoveProjection = 5,
    AssignSpirit = 6,
    SyncRolesList = 7
}

// Concealed looks odd... (icon in-game)

// MIMIC STILL SEES IMPS!?! (Did A "Fix" for now that stops imp cards from being in selections)

// Make Scrubber Work In Rounds

// Exposed is broken

// =============== FIXES ===============
/*
 * Changed Scrubber Icon
 * Added Keybind To Projector Kill Button
 * Fixed Scrubber Scrubbing To Only Scrub Certain Modifiers
*/