using JAM.Assets;
using JAM.Options.Modifiers;
using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using MiraAPI.Utilities;
using MiraAPI.Utilities.Assets;
using Reactor.Networking.Attributes;
using TownOfUs.Extensions;
using TownOfUs.Modifiers;
using TownOfUs.Modifiers.Game;
using TownOfUs.Utilities;
using UnityEngine;

namespace JAM.Modifiers;

public sealed class RivalModifier : AllianceGameModifier, IAssignableTargets
{
    public static Dictionary<PlayerControl, int> RivalScores = new Dictionary<PlayerControl, int>();

    public override string ModifierName => "Rival";
    public override string IntroInfo => "You Are A Rival";
    public override string GetDescription()
    {
        if(!PlayerControl.LocalPlayer.HasModifier<RivalModifier>())
            return IntroInfo;

        string RivalDesc = IntroInfo + "\nComplete More Tasks Than " + (RivalScores.Keys.FirstOrDefault(p => p.HasModifier<RivalModifier>() && !p.AmOwner)?.Data?.PlayerName ?? "Your Rival") + " To Win!";
        
        if(RivalScores.Keys.Any(p => p.AmOwner))
        {
            RivalDesc += "\nCurrent Score: " + RivalScores[RivalScores.Keys.FirstOrDefault(p => p.AmOwner) ?? RivalScores.Keys.First()];
        }
        return RivalDesc;
    }
    public string GetAdvancedDescription()
    {
        return "Complete More Tasks Than Your Rival To Win!\n(Tasks Only Count So Long As Both Of You Are Alive)" + MiscUtils.AppendOptionsText(GetType());
    }
    public override LoadableAsset<Sprite>? ModifierIcon => JamModifierIcons.Rivalry;
    public override ModifierUiConfiguration Configuration => new(Colors.Rivals, TmpSpriteUtils.CreateSpriteAsset(JamModifierIcons.Rivalry.LoadAsset(), "Rivals", 1.45f));
    public override int GetAssignmentChance()
    {
        return CustomChance;
    }
    public override int GetAmountPerGame()
    {
        return CustomAmount;
    }
    public override int CustomAmount => CustomChance == 0 ? 0 : 2;
    public override int CustomChance => (int)OptionGroupSingleton<RivalOptions>.Instance.RivalChance;
    public override string Symbol => (RivalScores.ContainsKey(Player) ? RivalScores[Player] : 0).ToString();

    public override bool? DidWin(GameOverReason reason)
    {
        PlayerControl? rival = PlayerControl.AllPlayerControls.ToArray().FirstOrDefault(p => p.HasModifier<RivalModifier>() && p != Player);

        if (rival == null) return true;

        return RivalScores[Player] > RivalScores[rival];
    }

    public override bool DoesTasks => false;
    int IAssignableTargets.Priority { get; set; } = 5;
    public void AssignTargets()
    {
        if (!PlayerControl.LocalPlayer.IsHost()) return;

        foreach (PlayerControl player in PlayerControl.AllPlayerControls.ToArray().Where(p => p.HasModifier<RivalModifier>()))
        {
            player.RpcRemoveModifier<RivalModifier>();
        }

        if (CustomChance < UnityEngine.Random.Range(1, 101)) return;

        List<PlayerControl> PossiblePlayers = MiraAPI.Utilities.Helpers.GetAlivePlayers().Where(p => !p.HasModifier<AllianceGameModifier>() && p.IsCrewmate()).ToList();

        if (PossiblePlayers.Count < 3)
        {
            return;
        }

        if (PlayerControl.AllPlayerControls.ToArray().Any(p => p.HasModifier<RivalModifier>()))
        {
            Error("Error - Rivals Already Spawned");
            return;
        }
        
        PossiblePlayers.Shuffle();
        PlayerControl randPlayer = PossiblePlayers[0];

        randPlayer.RpcAddModifier<RivalModifier>();

        PossiblePlayers.Remove(randPlayer);

        PossiblePlayers.Shuffle();
        PlayerControl randPlayer2 = PossiblePlayers[0];

        randPlayer2.RpcAddModifier<RivalModifier>();
    }

    [MethodRpc((uint)JAMRpcCalls.SetRivalScore)]
    public static void RpcSetRivalScore(PlayerControl rival, uint score)
    {
        foreach(PlayerControl player in RivalScores.Keys)
        {
            if(player.Data.IsDead)
            {
                Info($"Failed To Update Rival Scores - {rival.Data.PlayerName} is dead");
            }
        }

        if(!RivalScores.ContainsKey(rival))
        {
            Error("Rivals Don't Contain " + rival.Data.PlayerName);
            return;
        }

        RivalScores[rival] = (int)score;
    }
}
