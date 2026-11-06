using HarmonyLib;
using Il2CppSystem.Web.Util;
using JAM.Assets;
using JAM.Options.Modifiers;
using JAM.Patches.WinConditions;
using JAM.Roles.Crewmate;
using JAM.Roles.Neutral;
using MiraAPI.GameEnd;
using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using MiraAPI.Roles;
using MiraAPI.Translation;
using MiraAPI.Utilities;
using MiraAPI.Utilities.Assets;
using Reactor.Networking.Attributes;
using Reactor.Utilities.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TownOfUs.Extensions;
using TownOfUs.Interfaces;
using TownOfUs.Modifiers;
using TownOfUs.Modifiers.Game;
using TownOfUs.Modifiers.Game.Alliance;
using TownOfUs.Modifiers.Neutral;
using TownOfUs.Modules.Wiki;
using TownOfUs.Options;
using TownOfUs.Options.Modifiers;
using TownOfUs.Options.Modifiers.Alliance;
using TownOfUs.Roles;
using TownOfUs.Roles.Other;
using TownOfUs.Utilities;
using UnityEngine;

namespace JAM.Modifiers;

public sealed class MasonModifier : AllianceGameModifier, IAssignableTargets
{
    int IAssignableTargets.Priority { get; set; } = 5;

    public static string Keyword = "";
    public override ModifierUiConfiguration Configuration => new(Colors.Mason, TmpSpriteUtils.CreateSpriteAsset(JamModifierIcons.Mason.LoadAsset(), "Masons", 1.45f));
    public override string IdPart => "Masons";
    public override string ModifierName => "Masons";
    public override string IntroInfo => "You Are A Mason";
    public override string Symbol => "♣";

    public override string GetDescription()
    {
        string description = "You Are A Mason!\nMasons Win With Eachother, Not With Their Own Factions";

        if (Keyword != "" && PlayerControl.LocalPlayer.HasModifier<MasonModifier>())
        {
            description += $"\n{Colors.Mason.ToTextColor()}Keyword: <b>" + Keyword + "</b></color>";
        }

        return description;
    }
    public override int CustomAmount => GetAmountPerGame();
    public override int CustomChance => GetAssignmentChance();

    public string GetAdvancedDescription()
    {
        return "The Masons Win Together, Not With Their Own Factions " + MiscUtils.AppendOptionsText(GetType());
    }
    public string RoleMedDescriptionLocale() => "Don't Know Each Other, Win Together";

    public override LoadableAsset<Sprite>? ModifierIcon => JamModifierIcons.Mason;

    public override ModifierFaction FactionType => ModifierFaction.Alliance;
    public override Color FreeplayFileColor => Colors.Mason;

    public List<CustomButtonWikiDescription> Abilities { get; } = [];

    public override int GetAssignmentChance()
    {
        return (int)OptionGroupSingleton<MasonOptions>.Instance.MasonChance;
    }

    public override int GetAmountPerGame()
    {
        return GetAssignmentChance() > 0 ? (int)OptionGroupSingleton<MasonOptions>.Instance.MasonAmount : 0;
    }
    public void AssignTargets()
    {
        if (!PlayerControl.LocalPlayer.IsHost()) return;

        uint keywordIndex = (uint)UnityEngine.Random.Range(0, MasonKeywords.Count);
        MasonModifier.RpcSendKeyword(PlayerControl.LocalPlayer, keywordIndex);
    }
    public override void FixedUpdate()
    {
        if (!Player.Data.IsDead && CheckMasonWinCon())
        {
            Info("Masons Should Win!");
            CustomGameOver.Trigger<MasonWinCondition>(PlayerControl.AllPlayerControls.ToArray().Where(p => p.HasModifier<MasonModifier>()).Select(p => p.Data));
        }
    }

    static bool CheckMasonWinCon()
    {

        float percentMasons = (float)MiraAPI.Utilities.Helpers.GetAlivePlayers().Count(p => p.HasModifier<MasonModifier>()) / (float)MiraAPI.Utilities.Helpers.GetAlivePlayers().Count;

        if (percentMasons > 0.5f)
        {
            return true;
        }
        return false;
    }


    [MethodRpc((uint)JAMRpcCalls.SendKeyword)]
    public static void RpcSendKeyword(PlayerControl player, uint keywordId) // Required PlayerControl to work
    {
        string keyword;

        try
        {
            keyword = MasonKeywords[(int)keywordId];
        }
        catch
        {
            keyword = MasonKeywords[0];
        }

        MasonModifier.Keyword = keyword;
    }

    public override bool? DidWin(GameOverReason reason)
    {
        return CheckMasonWinCon();
    }

    static List<string> MasonKeywords = new() {
        // Sensible Words (Easy To Use, Hard To Decide If they're Just Using It Or Are A Mason)
        "Fly",
        "Cluster",
        "Death",
        "Hurt",
        "Join",
        "Trust",
        "Enemy",
        "Claim",
        "Flop",
        "Alone",
        "Hide",
        "Tricky",
        "Scared",
        "Ability",
        "Vital",
        "Fresh",
        "Ally",
        "Sidebar",
        "Chat",
        "Role",
        "Camera",
        "Map",
        "Evil",
        "Task",
        "Wait",

        // Somewhat Difficult Words (Can Find Ways To Use, And Some People May Just Use Them Normally)
        "Friend",
        "Buddy",
        "Help",
        "Murder",
        "Lonesome",
        "Cause",
        "Ship",
        "Wonderful",
        "Obvious",
        "Team",
        "Rant",
        "Revive",
        "Foot",
        "Love",
        "Cautious",

        // Hard Words (Will likely tell others you're a mason, but are hard to use without revealing yourself)
        "Jump",
        "Sneak",
        "Inquiry",
        "Oblivious",
        "Monster",
        "Mason",

        // Near Impossible To Use (You'll Have To Be Clever To Use These)
        "Creature",
        "Peculiar",
        "Hippopotomonstrosesquipedaliophobia" // I feel Bad If Someone Gets This, But It's Too Funny
    };
}

[HarmonyPatch(typeof(TownOfUs.Utilities.PlayerRoleTextExtensions), "UpdateAllianceSymbols", [typeof(string), typeof(PlayerControl), typeof(DataVisibility)])]
public static class MasonIconPatch
{
    public static void Postfix(PlayerControl player, ref string __result)
    {
        if (!player.HasModifier<MasonModifier>()) return;

        if(PlayerControl.LocalPlayer.HasDied() && PlayerControl.LocalPlayer.DiedOtherRound())
        {
            __result += $"{Colors.Mason.ToTextColor()} ♣</color>";
        }
    }
}