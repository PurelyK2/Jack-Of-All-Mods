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

[BepInAutoPlugin("com.JackOfAllMods.mod", "JackOfAllMods", "0.2.0")]
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

// =============== FIXES ===============
/*
 * Added Fake Shield Ability To Deceiver
 * Bounty Target No Longer Shows Up In Win/Loss Screen
 * Removed Zombie Alliance Modifier
 * Bounty Reward Modifier No Longer Shows Up In The Wiki
 * Made Bounty Target Modifier Hidden
 * Zombies No Longer Win With Crew Without The Alliance Modifier
 * Fixed Role Option Description Text (RoleMedDescription) For Roles
 * Changed Micromanager To Use An RPC Call Instead Of A Modifier
 * Made This Mod's Neutrals Win/Lose Correctly When Dead
 * Added Concealed Modifier
 * Gave Bounty Target A Speed Boost
 * Adjusted Micromanager To Work Better
 * Changed Micromanager Role Assignment To Be Aligned With Others
*/