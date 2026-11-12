using JAM.Modifiers;
using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.Events.Vanilla.Player;
using MiraAPI.Modifiers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TownOfUs.Utilities;
using UnityEngine;

namespace JAM.Events.Modifiers;

public static class RivalEvents
{
    [RegisterEvent(0)]
    public static void OnGameStart(RoundStartEvent @event)
    {
        if (!@event.TriggeredByIntro) return;

        RivalModifier.RivalScores.Clear();

        foreach (PlayerControl player in PlayerControl.AllPlayerControls.ToArray().Where(p => p.HasModifier<RivalModifier>()))
        {
            RivalModifier.RivalScores.Add(player, 0);
        }
    }

    [RegisterEvent(0)]
    public static void OnKillEvent(AfterMurderEvent @event)
    {
        if(@event.Source.HasModifier<RivalModifier>() && @event.Source.IsImpostor())
        {
            AddOneScore(@event.Source);
        }
    }

    [RegisterEvent(0)]
    public static void OnTaskCompleteEvent(CompleteTaskEvent @event)
    {
        if (@event.Player.HasModifier<RivalModifier>() && @event.Player.IsCrewmate())
        {
            AddOneScore(@event.Player);
            AddExtraTask(@event.Player, @event.Task);
        }
    }

    private static void AddExtraTask(PlayerControl player, PlayerTask completedTask)
    {
        if (player?.Data?.Tasks == null || player.Data.Tasks.Count == 0) return;

        List<PlayerTask> tasks = player.myTasks.ToArray().ToList();

        List<NormalPlayerTask> commonTasks = ShipStatus.Instance.CommonTasks.Where(t => !tasks.Any(task => task.TaskType == t.TaskType)).ToList();
        List<NormalPlayerTask> shortTasks = ShipStatus.Instance.ShortTasks.Where(t => !tasks.Any(task => task.TaskType == t.TaskType)).ToList();
        List<NormalPlayerTask> longTasks = ShipStatus.Instance.LongTasks.Where(t => !tasks.Any(task => task.TaskType == t.TaskType)).ToList();

        NormalPlayerTask prefab = new NormalPlayerTask();

        if (ShipStatus.Instance.CommonTasks.Any(t => t.TaskType == completedTask.TaskType)) prefab = commonTasks[UnityEngine.Random.Range(0, commonTasks.Count)];
        else if (ShipStatus.Instance.ShortTasks.Any(t => t.TaskType == completedTask.TaskType)) prefab = shortTasks[UnityEngine.Random.Range(0, shortTasks.Count)];
        else prefab = longTasks[UnityEngine.Random.Range(0, longTasks.Count)];

        var task = NormalPlayerTask.Instantiate(prefab, player.transform);

        task.Id = (uint)tasks.Count;
        task.Owner = player;
        task.Initialize();

        player.myTasks.Add(task);
        player.myTasks.Remove(completedTask);

        //Failsafe For Bugs
        if (!player.myTasks.ToArray().Any(p => p.IsComplete)) return;

        foreach(PlayerTask thisTask in player.myTasks)
        {
            if(thisTask.IsComplete)
            {
                prefab = new NormalPlayerTask();

                if (ShipStatus.Instance.CommonTasks.Any(t => t.TaskType == thisTask.TaskType)) prefab = commonTasks[UnityEngine.Random.Range(0, commonTasks.Count)];
                else if (ShipStatus.Instance.ShortTasks.Any(t => t.TaskType == thisTask.TaskType)) prefab = shortTasks[UnityEngine.Random.Range(0, shortTasks.Count)];
                else prefab = longTasks[UnityEngine.Random.Range(0, longTasks.Count)];

                var task1 = NormalPlayerTask.Instantiate(prefab, player.transform);

                task1.Id = (uint)tasks.Count;
                task1.Owner = player;
                task1.Initialize();

                player.myTasks.Add(task1);
                player.myTasks.Remove(thisTask);
            }
        }
    }

    static void AddOneScore(PlayerControl rival)
    {
        if (!RivalModifier.RivalScores.ContainsKey(rival))
        {
            Error("Rivals Don't Contain " + rival.Data.PlayerName);
            return;
        }

        RivalModifier.RpcSetRivalScore(rival, (uint)(RivalModifier.RivalScores[rival] + 1));
    }
}
