using HarmonyLib;
using Il2CppSystem.Web.Util;
using JAM.Assets;
using JAM.Options.Modifiers;
using JAM.Patches.WinConditions;
using JAM.Roles.Crewmate;
using JAM.Roles.Neutral;
using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.GameEnd;
using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using MiraAPI.Networking;
using MiraAPI.Roles;
using MiraAPI.Translation;
using MiraAPI.Utilities;
using MiraAPI.Utilities.Assets;
using Reactor.Networking.Attributes;
using Reactor.Networking.Rpc;
using Reactor.Utilities.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using TownOfUs.Extensions;
using TownOfUs.GameOver;
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

public sealed class MasonModifier : AllianceGameModifier, IAssignableTargets, IContinuesGame
{
    int IAssignableTargets.Priority { get; set; } = 3;

    public static string Keyword = "";
    public override ModifierUiConfiguration Configuration => new(Colors.Mason, TmpSpriteUtils.CreateSpriteAsset(JamModifierIcons.Mason.LoadAsset(), "Masons", 1.45f));
    public override string IdPart => "Masons";
    public override string ModifierName => "Masons";
    public override string IntroInfo => "You Are A Mason";
    public override string Symbol => "♣";
    public override bool GetsPunished => false;

    public override string GetDescription()
    {
        string description = "You Are A Mason!\nMasons Win With Eachother, Not With Their Own Factions";

        if (Keyword != "" && PlayerControl.LocalPlayer.HasModifier<MasonModifier>())
        {
            description += $"\n{Colors.Mason.ToTextColor()}Keyword: <b>" + Keyword + "</b></color>";
        }

        return description;
    }
    public override int CustomAmount => (int)OptionGroupSingleton<MasonOptions>.Instance.MasonChance > 0 ? (int)OptionGroupSingleton<MasonOptions>.Instance.MasonAmount : 0;
    public override int CustomChance => (int)OptionGroupSingleton<MasonOptions>.Instance.MasonChance;

    public string GetAdvancedDescription()
    {
        return "The Masons Win Together, Not With Their Own Factions " + MiscUtils.AppendOptionsText(GetType());
    }
    public string RoleMedDescriptionLocale() => "Don't Know Each Other, Win Together";

    public override LoadableAsset<Sprite>? ModifierIcon => JamModifierIcons.Mason;

    public override ModifierFaction FactionType => ModifierFaction.Alliance;
    public override Color FreeplayFileColor => Colors.Mason;

    public List<CustomButtonWikiDescription> Abilities { get; } = [];

    public bool ContinuesGame => (Player.Data.Role.GetRoleAlignment() == RoleAlignment.CrewmatePower && TownOfUs.Utilities.MiscUtils.KillersAliveCount > 0) || Player.Data.Role.GetRoleAlignment() == RoleAlignment.CrewmateKilling || Player.Data.Role.GetRoleAlignment() == RoleAlignment.NeutralKilling || Player.Data.Role.IsImpostor();

    public override int GetAssignmentChance()
    {
        return (int)Mathf.Pow(Mathf.Log10(CustomChance) / (float)CustomAmount, 10f);
    }

    public override int GetAmountPerGame()
    {
        return CustomAmount;
    }
    public void AssignTargets()
    {
        if (!PlayerControl.LocalPlayer.IsHost()) return;

        MasonKeywords.Shuffle();
        MasonFunnies.Shuffle();

        if(UnityEngine.Random.Range(1, 101) <= 25)
        {
            MasonModifier.RpcSendKeyword(PlayerControl.LocalPlayer, MiraAPI.Roles.CustomRoleManager.AllRoles[UnityEngine.Random.Range(1, MiraAPI.Roles.CustomRoleManager.AllRoles.Count())].GetRoleName());
        }
        else if (UnityEngine.Random.Range(1, 101) == 1)
        {
            MasonModifier.RpcSendKeyword(PlayerControl.LocalPlayer, MasonFunnies[0]);
        }
        else MasonModifier.RpcSendKeyword(PlayerControl.LocalPlayer, MasonKeywords[0]);
    }
    public override void FixedUpdate()
    {
        if (CheckMasonWinCon())
        {
            Info("Masons Should Win!");
            CustomGameOver.Trigger<MasonWinCondition>(PlayerControl.AllPlayerControls.ToArray().Where(p => p.HasModifier<MasonModifier>()).Select(p => p.Data));
        }
    }
    public override void OnDeath(DeathReason reason)
    {
        base.OnDeath(reason);

        if (OptionGroupSingleton<MasonOptions>.Instance.MasonsDisband && MiraAPI.Utilities.Helpers.GetAlivePlayers().Count(p => p.HasModifier<MasonModifier>()) == 1)
        {
            DisbandMasons();
        }
    }

    public static bool CheckMasonWinCon()
    {
        float percentMasons = (float)MiraAPI.Utilities.Helpers.GetAlivePlayers().Count(p => p.HasModifier<MasonModifier>()) / (float)MiraAPI.Utilities.Helpers.GetAlivePlayers().Count(p => p.Data.Role.GetRoleAlignment() != RoleAlignment.NeutralBenign);

        if (percentMasons > 0.5f)
        {
            return true;
        }
        return false;
    }


    [MethodRpc((uint)JAMRpcCalls.SendKeyword)]
    public static void RpcSendKeyword(PlayerControl player, string keyword) // Required PlayerControl to work
    {
        MasonModifier.Keyword = keyword;
    }

    public override bool? DidWin(GameOverReason reason)
    {
        bool otherModifiersWin = false;

        foreach (AllianceGameModifier modifier in Player.GetModifiers<AllianceGameModifier>())
        {
            if (modifier is MasonModifier) continue;

            if (modifier.DidWin(reason) == true)
            {
                otherModifiersWin = true;
            }
        }

        return CheckMasonWinCon() || otherModifiersWin;
    }

    [MethodRpc((uint)JAMRpcCalls.DisbandMasons)]
    void DisbandMasons()
    {
        if (PlayerControl.LocalPlayer.HasModifier<MasonModifier>())
            MiraAPI.Utilities.Helpers.CreateAndShowNotification("The Masons Have Disbanded", Colors.Mason, spr: JamModifierIcons.Mason);

        foreach (PlayerControl mason in PlayerControl.AllPlayerControls.ToArray().Where(p => p.HasModifier<MasonModifier>()))
        {
            mason.RemoveModifier<MasonModifier>();
        }
    }
    public override bool DoesTasks => false;

    public override bool IsModifierValidOn(RoleBehaviour role)
    {
        return base.IsModifierValidOn(role) || (role is IAssignableTargets);
    }

    static List<string> MasonKeywords = new() {
        // Sensible Words (Easy To Use, Hard To Decide If they're Just Using It Or Are A Mason)
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
        "Fly",
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
        "Chill",
        "Mention",
        "Color",

        // Hard Words (Will likely tell others you're a mason, but are hard to use without revealing yourself)
        "Sneak",
        "Inquiry",
        "Oblivious",
        "Monster",
        "Mason",

        // Near Impossible To Use (You'll Have To Be Clever To Use These)
        "Creature",
        "Peculiar",
    };
    static List<string> MasonFunnies = new() {
        "Hippopotomonstrosesquipedaliophobia",
        "The Entire Declaration Of Independence",
        "'MURICA!",
        "I Want To Flatter K2",
        "Supercalifragilisticexpialidocious"
    };

    [RegisterEvent(0)]
    public static void MakeMoreMasons(RoundStartEvent @event)
    {
        if (!@event.TriggeredByIntro || !PlayerControl.LocalPlayer.IsHost()) return;

        List<PlayerControl> killers = PlayerControl.AllPlayerControls.ToArray().Where(p => p.IsImpostor() || p.Data.Role.GetRoleAlignment() == RoleAlignment.NeutralKilling || p.Data.Role.GetRoleAlignment() == RoleAlignment.CrewmateKilling).ToList();

        if (!killers.Any(p => p.HasModifier<MasonModifier>()) && PlayerControl.AllPlayerControls.ToArray().Count(p => p.HasModifier<MasonModifier>()) >= 1)
        {
            killers.Shuffle();
            PlayerControl newMason = killers[0];
            newMason.RpcAddModifier<MasonModifier>();
        }
        else if(PlayerControl.AllPlayerControls.ToArray().Count(p => p.HasModifier<MasonModifier>()) == 1)
        {
            List<PlayerControl> possibleMasons = PlayerControl.AllPlayerControls.ToArray().Where(p => !p.HasModifier<MasonModifier>()).ToList();
            possibleMasons.Shuffle();
            PlayerControl newMason = possibleMasons[0];
            newMason.RpcAddModifier<MasonModifier>();
        }
    }
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

[HarmonyPatch(typeof(TownOfUs.Utilities.Extensions), "GetClosestLivingPlayer")]
public static class MasonsKillAll
{
    static bool checkingClosestLiving = false;
    public static void Prefix(PlayerControl playerControl, ref bool includeImpostors)
    {
        checkingClosestLiving = true;
        if (playerControl.HasModifier<MasonModifier>()) includeImpostors = true;
    }
    public static void Postfix()
    {
        checkingClosestLiving = false;
    }

    [HarmonyPatch(typeof(TownOfUs.Utilities.Extensions), "IsImpostorAligned")]
    public static class ImpsKillMasons
    {
        public static void Postfix(ref PlayerControl player, ref bool __result)
        {
            if(checkingClosestLiving && player.HasModifier<MasonModifier>() && MiraAPI.Utilities.Helpers.GetAlivePlayers().Count < 5)
            {
                __result = false;
            }
        }
    }
}

[HarmonyPatch(typeof(UnsafeCustomRpc), "UnsafeSend")]
public static class EndMasonWinners
{
    public static void Prefix(object data)
    {
        if(data is GameOverData goData && goData.Winners.All(p => p.Role.Player.HasModifier<MasonModifier>()))
        {
            goData.Winners.AddRange(PlayerControl.AllPlayerControls.ToArray().Where(p => p.HasModifier<MasonModifier>() && !goData.Winners.Contains(p.Data)).Select(p => p.Data));
        }
    }
}