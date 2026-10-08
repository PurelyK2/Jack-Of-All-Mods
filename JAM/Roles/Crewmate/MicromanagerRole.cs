using AmongUs.GameOptions;
using HarmonyLib;
using Il2CppSystem.Text;
using JAM.Assets;
using JAM.Options.Roles.Crewmate;
using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using MiraAPI.Patches.Stubs;
using MiraAPI.Roles;
using MiraAPI.Utilities;
using MiraAPI.Utilities.Assets;
using Reactor.Networking.Attributes;
using Reactor.Utilities;
using System.Collections;
using System.Text.RegularExpressions;
using TownOfUs;
using TownOfUs.Assets;
using TownOfUs.Events;
using TownOfUs.Modifiers;
using TownOfUs.Modifiers.Game;
using TownOfUs.Modules;
using TownOfUs.Modules.Wiki;
using TownOfUs.Options;
using TownOfUs.Patches;
using TownOfUs.Roles;
using TownOfUs.Roles.Neutral;
using TownOfUs.Utilities;
using TownOfUs.Utilities.Appearances;
using UnityEngine;
using UnityEngine.UI;

namespace JAM.Roles.Crewmate;

public sealed class MicromanagerRole(IntPtr cppPtr) : CrewmateGhostRole(cppPtr), IWikiDiscoverable, ITownOfUsRole, IGhostRole
{
    public string RoleName => "Micromanager";
    public bool Setup { get; set; }
    public bool Caught { get; set; }
    public bool Faded { get; set; }
    public bool IsDraftable => false;
    public float managedTaskProgression = 0f;

    private MicromanagerOptions opts = OptionGroupSingleton<MicromanagerOptions>.Instance;
    public Color RoleColor => Colors.Micromanager;
    public ModdedRoleTeams Team => ModdedRoleTeams.Crewmate;
    public RoleAlignment RoleAlignment => RoleAlignment.CrewmateAfterlife;

    public CustomRoleConfiguration Configuration => new(this)
    {
        IconTmp = TmpSpriteUtils.CreateSpriteAsset(JamRoleIcons.Micromanager.LoadAsset(), "Micromanager", 1.55f),
        Icon = JamRoleIcons.Micromanager,
        OptionsScreenshot = TouBanners.CrewmateRoleBanner,
        TasksCountForProgress = false,
        HideSettings = false,
        CanUseVent = false,
        GetsVentData = true,
        ShowInFreeplay = true,
    };

    private bool _tasksAdded;

    public string RoleDescription => "You must Manage everyone's tasks!";
    public string RoleLongDescription => "Manage the Crew's tasks and complete their tasks for them.";
    public string RoleMedDescriptionLocale => "Complete tasks for other players.";
    public string GetAdvancedDescription()
    {
        return
            "The Micromanager is a Crewmate Afterlife that can help the Crew complete their Tasks" +
            "by completing your own set of extra Tasks." +
            MiscUtils.AppendOptionsText(GetType());
    }

    [HarmonyPatch(typeof(GhostRoleEvents), nameof(GhostRoleEvents.CoSetGhostwalkers))]
    public static class GhostwalkerPatch
    {
        public static void Postfix(PlayerControl? exiled)
        {
            var microData = MiscUtils.GetAssignData((RoleTypes)RoleId.Get<MicromanagerRole>());
            Warning($"MicromanagerData={microData}");

            if (CustomRoleUtils.GetActiveRoles().OfType<MicromanagerRole>().Count() < microData.Count)
            {
                var isSkipped = microData.Chance < 100 && HashRandom.Next(101) > microData.Chance;
                Warning($"MicromanagerRoleChance={microData.Chance}");
                if (!isSkipped)
                {
                    var deadCrew = PlayerControl.AllPlayerControls.ToArray().Where(x =>
                        (x.Data.IsDead || x == exiled) && x.GetRoleWhenAlive().IsCrewmate() && !x.HasModifier<AllianceGameModifier>() &&
                        x.CanGetGhostRole() &&
                        x.Data.Role).ToList();
                        Warning($"MicromanagerDeadCrew={deadCrew}");


                    if (deadCrew.Count > 0)
                    {
                        deadCrew.Shuffle();

                        var player = deadCrew.TakeFirst();
                        Warning($"MicromanagerPlayer={player}");

                        player?.RpcChangeRole(RoleId.Get<MicromanagerRole>());
                    }
                }
            }
        }
    }

    public bool CanBeClicked
    {
        get
        {
        return TaskStage is GhostTaskStage.Clickable;
        // || TaskStage is GhostTaskStage.CompletedTasks;
        }
        set
        {
            // Left Alone
        }
    }

    public GhostTaskStage TaskStage { get; private set; } = GhostTaskStage.Unclickable;
    public bool GhostActive => Setup && !Caught;
    public bool CanCatch()
    {
        if (opts.MicromanagerCanBeClickedBy == MicromanagerRoleClickableType.ImpsOnly && !PlayerControl.LocalPlayer.IsImpostorAligned())
        {
            return false;
        }

        if (opts.MicromanagerCanBeClickedBy == MicromanagerRoleClickableType.NonCrew && PlayerControl.LocalPlayer.IsCrewmate())
        {
            return false;
        }

        return true;
    }

    public void Spawn()
    {
        Setup = true;

        if (HudManagerPatches.CamouflageCommsEnabled)
        {
            Player.SetCamouflage(false);
        }

        var text = $"Setup MicromanagerRole '{Player.Data.PlayerName}'";
        MiscUtils.LogInfo(TownOfUsEventHandlers.LogLevel.Error, text);

        Player.gameObject.layer = LayerMask.NameToLayer("Players");

        Player.gameObject.GetComponent<PassiveButton>().OnClick = new Button.ButtonClickedEvent();
        Player.gameObject.GetComponent<PassiveButton>().OnClick.AddListener((Action)(() => Player.OnClick()));
        Player.gameObject.GetComponent<BoxCollider2D>().enabled = true;

        var collider = Player.Collider;
        collider.enabled = true;

        MiscUtils.LogInfo(TownOfUsEventHandlers.LogLevel.Error,
        $"Micromanager Spawn: Player.Collider enabled={collider.enabled}");

        if (Player.AmOwner)
        {
            if ((GhostwalkerVentMode)OptionGroupSingleton<GameMechanicOptions>.Instance.GhostwalkerVentSpawn.Value is
                GhostwalkerVentMode.Crewmates or GhostwalkerVentMode.All)
            {
                Player.VentAtRandomVent();
            }
            else
            {
                Player.SpawnAtRandomVent();
            }
            Player.MyPhysics.ResetMoveState();

            HudManager.Instance.SetHudActive(false);
            HudManager.Instance.SetHudActive(true);
            HudManager.Instance.AbilityButton.SetDisabled();
            HudManagerPatches.ResetZoom();
        }
    }

    public void FadeUpdate()
    {
        if (!Caught && Setup)
        {
            Player.GhostFade();
            Faded = true;
        }
        else if (Faded)
        {
            Player.ResetAppearance();
            Player.cosmetics.ToggleNameVisible(true);

            Player.cosmetics.currentBodySprite.BodySprite.color = Color.white;
            Player.gameObject.layer = LayerMask.NameToLayer("Ghost");
            Player.MyPhysics.ResetMoveState();

            Faded = false;

            // Message($"MicromanagerRole.FadeUpdate UnFaded");
        }
    }

    public void FixedUpdate()
    {
        if (!Player || Player.Data.Role is not MicromanagerRole || MeetingHud.Instance)
        {
            return;
        }
        FadeUpdate();

        if (!_tasksAdded && Player.myTasks != null && Player.myTasks.Count > 0)
        {
            _tasksAdded = AddExtraTasks();
        }

        CheckTaskRequirements();
    }

    public void Clicked()
    {
        var text = $"Clicked MicromanagerRole: '{Player.Data.PlayerName}'";
        MiscUtils.LogInfo(TownOfUsEventHandlers.LogLevel.Message, text);

        Caught = true;
        Player.Exiled();

        if (Player.AmOwner)
        {
            HudManager.Instance.AbilityButton.SetEnabled();
        }

        GameHistory.PlayerStats[Player.PlayerId].DiedThisRound = false;
    }

    public void CompleteRandomCrewTask()
    {
        if (!Player.AmOwner)
        {
            return;
        }

        var crewPlayers = PlayerControl.AllPlayerControls.ToArray().Where(x => 
            x && x != Player && x.Data != null && x.IsCrewmate() && !x.HasModifier<AllianceGameModifier>() &&
                opts.MicromanagerManagesCrewTasks switch
                {
                    MicromanagerRoleCrewType.AliveCrew => !x.Data.IsDead,
                    MicromanagerRoleCrewType.DeadCrew => x.Data.IsDead,
                    MicromanagerRoleCrewType.AnyCrew => true,
                    _ => false
                }).Where(x => x.myTasks != null && x.myTasks.ToArray().Any(task => task.TryCast<NormalPlayerTask>() != null && !task.IsComplete)).ToList();

        if (crewPlayers.Count == 0)
        {
            var warn = Helpers.CreateAndShowNotification(
            $"There are no more tasks available to manage.",
            Color.white,
            new Vector3(0f, 1f, -20f),
            spr: JamRoleIcons.Micromanager.LoadAsset());

            warn.AdjustNotification();
            return;
        }

        var randomCrew = crewPlayers[UnityEngine.Random.Range(0, crewPlayers.Count)];

        if (randomCrew.myTasks == null || randomCrew.myTasks.Count == 0)
        {
            return;
        }

        var tasks = randomCrew.myTasks.ToArray().Where(x => x.TryCast<NormalPlayerTask>() != null && !x.IsComplete).ToList();

        if (tasks.Count == 0)
        {
            return;
        }

        tasks.Shuffle();

        var randomTask = tasks[0];

        HudManager.Instance.ShowTaskComplete();

        var sb = new StringBuilder();
        randomTask.AppendTaskText(sb);

        var pattern = @" \(.*?\)";
        var playerText = randomCrew.Data.PlayerName;
        var taskText = Regex.Replace(sb.ToString(), pattern, string.Empty);
        var microColor = Colors.Micromanager;

        var notif = Helpers.CreateAndShowNotification(
            $"<b>{microColor.ToTextColor()}You completed </color>{TownOfUsColors.Crewmate.ToTextColor()}{playerText}'s</color> {TownOfUsColors.Doomsayer.ToTextColor()}{taskText}</color>. </color></b>",
            Color.white,
            new Vector3(0f, 1f, -20f),
            spr: JamRoleIcons.Micromanager.LoadAsset());

        notif.AdjustNotification();


        RpcCompleteTask(randomCrew, randomTask.Id, taskText);
        var targetMessage =
            $"<b>{microColor.ToTextColor()}The Micromanager completed the</color> {TownOfUsColors.Doomsayer.ToTextColor()}{taskText}</color>{microColor.ToTextColor()}task for you.</color></b>";
    }

    public override void Initialize(PlayerControl player)
    {
        RoleBehaviourStubs.Initialize(this, player);
    
        if (!Player.HasModifier<BasicGhostModifier>())
        {
            Player.AddModifier<BasicGhostModifier>();
        }
        if (TutorialManager.InstanceExists)
        {
            Setup = true;

            if (HudManagerPatches.CamouflageCommsEnabled)
            {
                Player.SetCamouflage(false);
            }

            Coroutines.Start(SetTutorialCollider(Player));

            if (Player.AmOwner)
            {
                Player.MyPhysics.ResetMoveState();
                HudManager.Instance.SetHudActive(false);
                HudManager.Instance.SetHudActive(true);
                HudManager.Instance.AbilityButton.SetDisabled();
                HudManagerPatches.ResetZoom();
            }
        }

        MiscUtils.AdjustGhostTasks(player);
    }

    private static IEnumerator SetTutorialCollider(PlayerControl player)
    {
        yield return new WaitForSeconds(0.01f);
        player.gameObject.layer = LayerMask.NameToLayer("Players");

        player.gameObject.GetComponent<PassiveButton>().OnClick = new Button.ButtonClickedEvent();
        player.gameObject.GetComponent<PassiveButton>().OnClick.AddListener((Action)(() => player.OnClick()));
        player.gameObject.GetComponent<BoxCollider2D>().enabled = true;
    }

    private bool AddExtraTasks()
    {
        if (Player?.Data?.Tasks == null || Player.Data.Tasks.Count == 0)
            return false;

        var tasks = Player.Data.Tasks;

        var commonTasks = ShipStatus.Instance.CommonTasks;
        var shortTasks  = ShipStatus.Instance.ShortTasks;
        var longTasks   = ShipStatus.Instance.LongTasks;

        var toAdd = new List<NormalPlayerTask>();

        for (int i = 0; i < (int)opts.ExtraCommonTasks; i++)
            toAdd.Add(commonTasks[UnityEngine.Random.Range(0, commonTasks.Length)]);

        for (int i = 0; i < (int)opts.ExtraShortTasks; i++)
            toAdd.Add(shortTasks[UnityEngine.Random.Range(0, shortTasks.Length)]);

        for (int i = 0; i < (int)opts.ExtraLongTasks; i++)
            toAdd.Add(longTasks[UnityEngine.Random.Range(0, longTasks.Length)]);

        foreach (var prefab in toAdd)
        {
            var task = Instantiate(prefab, Player.transform);

            task.Id = (uint)tasks.Count;
            task.Owner = Player;
            task.Initialize();

            Player.myTasks.Add(task);
            tasks.Add(CreateTaskInfo(task.Id));
        }
        return true;
    }

    private static NetworkedPlayerInfo.TaskInfo CreateTaskInfo(uint id)
    {
        var ptr = Il2CppInterop.Runtime.IL2CPP.il2cpp_object_new(
            Il2CppInterop.Runtime.Il2CppClassPointerStore<NetworkedPlayerInfo.TaskInfo>.NativeClassPtr);

        var taskInfo = new NetworkedPlayerInfo.TaskInfo(ptr);

        taskInfo.Id = id;
        taskInfo.Complete = false;

        return taskInfo;
    }

    public override void Deinitialize(PlayerControl targetPlayer)
    {
        RoleBehaviourStubs.Deinitialize(this, targetPlayer);
        if (TutorialManager.InstanceExists)
        {
            Player.ResetAppearance();
            Player.cosmetics.ToggleNameVisible(true);

            Player.cosmetics.currentBodySprite.BodySprite.color = Color.white;
            Player.gameObject.layer = LayerMask.NameToLayer("Ghost");
            Player.MyPhysics.ResetMoveState();

            Faded = false;
        }
    }

    public override bool CanUse(IUsable console)
    {
        var validUsable = console.TryCast<Console>() ||
                          console.TryCast<DoorConsole>() ||
                          console.TryCast<OpenDoorConsole>() ||
                          console.TryCast<DeconControl>() ||
                          console.TryCast<PlatformConsole>() ||
                          console.TryCast<Ladder>() ||
                          console.TryCast<ZiplineConsole>();

        return GhostActive && validUsable;
    }

    public void CheckTaskRequirements()
    {
        UpdateTaskStage(silent: false, forceRecalculate: false);
    }

    private void UpdateTaskStage(bool silent, bool forceRecalculate)
    {
        if (Caught || !Player)
        {
            return;
        }

        GetTaskCounts(Player, out var completedTasks, out var totalTasks);
        var tasksRemaining = totalTasks - completedTasks;
        var clickableAt = (int)opts.NumTasksLeftBeforeClickable;

        GhostTaskStage newStage;

        if (tasksRemaining <= clickableAt)
        {
            newStage = GhostTaskStage.Clickable;
        }
        else
        {
            newStage = GhostTaskStage.Unclickable;
        }

        if (!forceRecalculate)
        {
            if (TaskStage is not GhostTaskStage.Clickable && newStage is GhostTaskStage.Clickable)
                // || (!CompletedAllTasks && newStage is GhostTaskStage.CompletedTasks))
            {
                TaskStage = newStage;
                HandleStageChange(newStage, silent);
            }
            else
            {
                var text = $"Micromanager Stage for '{Player.Data.PlayerName}': " +
                        $"{TaskStage.ToDisplayString()} - ({completedTasks} / {totalTasks})";

                MiscUtils.LogInfo(TownOfUsEventHandlers.LogLevel.Error, text);
            }
        }
        else
        {
            TaskStage = newStage;
            HandleStageChange(newStage, silent);
        }
    }

    private void HandleStageChange(GhostTaskStage stage, bool silent)
    {
        var text = $"Micromanager Stage for '{Player.Data.PlayerName}': {stage.ToDisplayString()}";
        MiscUtils.LogInfo(TownOfUsEventHandlers.LogLevel.Error, text);

        if (stage is GhostTaskStage.Clickable)
        {
            if (Player.AmOwner && !silent)
            {
                var notif1 = Helpers.CreateAndShowNotification(
                    $"<b>{Colors.Micromanager.ToTextColor()}{"You are now clickable by players!"}</b></color>",
                    Color.white,
                    new Vector3(0f, 1f, -20f), spr: JamRoleIcons.Micromanager.LoadAsset());
                notif1.AdjustNotification();
            }
        }
    }

    private static void GetTaskCounts(PlayerControl player, out int completed, out int total)
    {
        completed = 0;
        total = 0;

        if (player == null || player.Data == null)
        {
            return;
        }

        if (player.myTasks != null && player.myTasks.Count > 0)
        {
            var tasks = player.myTasks.ToArray().Where(x => !PlayerTask.TaskIsEmergency(x) && !x.TryCast<ImportantTextTask>());
            foreach (var t in tasks)
            {
                total++;
                var taskInfo = player.Data.FindTaskById(t.Id);
                var isComplete = taskInfo != null ? taskInfo.Complete : t.IsComplete;
                if (isComplete)
                {
                    completed++;
                }
            }

            return;
        }

        foreach (var info in player.Data.Tasks)
        {
            total++;
            if (info.Complete)
            {
                completed++;
            }
        }
    }

    [MethodRpc((uint) JAMRpcCalls.MicromanageTask)]
    public static void RpcCompleteTask(PlayerControl player, uint taskId, string taskName)
    {
        if (player.AmOwner)
        {
            player.RpcCompleteTask(taskId);

            Helpers.CreateAndShowNotification(
                $"<b>{Colors.Micromanager.ToTextColor()}Your {TownOfUsColors.Doomsayer.ToTextColor()}{taskName}</color> Task Was Completed By The {Colors.Micromanager.ToTextColor()}Micromanager</color>. </color></b>",
                Color.white,
                new Vector3(0f, 1f, -20f),
                spr: JamRoleIcons.Micromanager.LoadAsset());
        }
    }

    //Stop Micromanager From Happening On Death
    [HarmonyPatch(typeof(TouRoleManagerPatches), "GhostRoleSetup")]
    public static class DontMicromanageOnDeathPatch
    {
        public static void Postfix(ref List<RoleTypes> ___CrewmateGhostRolePool)
        {
            ___CrewmateGhostRolePool.RemoveAll(x => x == (RoleTypes)RoleId.Get<MicromanagerRole>());
        }
    }
}