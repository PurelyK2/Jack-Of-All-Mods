using JAM.Assets;
using JAM.Options.Modifiers;
using MiraAPI.Events;
using MiraAPI.GameOptions;
using MiraAPI.Hud;
using MiraAPI.Modifiers;
using MiraAPI.Utilities;
using MiraAPI.Utilities.Assets;
using TownOfUs.Buttons.Impostor;
using TownOfUs.Events.TouEvents;
using TownOfUs.Interfaces;
using TownOfUs.Modifiers;
using TownOfUs.Modifiers.Game;
using TownOfUs.Modifiers.Game.Universal;
using TownOfUs.Modifiers.Impostor;
using TownOfUs.Modules;
using TownOfUs.Modules.Wiki;
using TownOfUs.Utilities;
using UnityEngine;

namespace JAM.Modifiers;

 
public sealed class UnstableModifier : TouGameModifier, IWikiDiscoverable
{
     
    public bool isUnstable { get; set; }

     
    public override string ModifierName => "Unstable";

     
    public override string IntroInfo => "You are unstable";

     
    public override bool HideFromGuessing => true;


     
    public override string GetDescription()
    {
        int minTPTime = (int)OptionGroupSingleton<UnstableOptions>.Instance.UnstableMinCooldown;
        int maxTPTime = Mathf.Max(minTPTime, (int)OptionGroupSingleton<UnstableOptions>.Instance.UnstableMaxCooldown);

        if(minTPTime == maxTPTime)
        {
            return "Randomly Teleport With Other Players Every " + minTPTime + " Seconds";
        }
        return "Randomly Teleport With Other Players Every " + minTPTime + " - " + maxTPTime + " Seconds";
    }
     
    public string GetAdvancedDescription()
    {
        return "Randomly Teleport With Other Players Throughout The Round" + MiscUtils.AppendOptionsText(base.GetType());
    }

    public override ModifierUiConfiguration Configuration
    {
        get
        {
            return new ModifierUiConfiguration(Colors.Unstable, TmpSpriteUtils.CreateSpriteAsset(JamModifierIcons.Blind.LoadAsset(), "TouMira.Modifier.Game.Universal.Blind", 1.45f));
        }
    }
     
    public override ModifierFaction FactionType => ModifierFaction.UniversalPassive;

     
    public override int GetAssignmentChance()
    {
        return CustomChance;
    }
     
    public override int GetAmountPerGame()
    {
        return CustomAmount;
    }
     
    public override float IntroSize => 5f;
     
    public override bool HideOnUi => false;
     
    public override LoadableAsset<Sprite> ModifierIcon => JamModifierIcons.Unstable;
     
    public override int CustomAmount => (int)OptionGroupSingleton<UnstableOptions>.Instance.UnstableCount;
     
    public override int CustomChance => (int)OptionGroupSingleton<UnstableOptions>.Instance.UnstableChance;

     
    public override void OnMeetingStart()
    {
        if(base.Player.HasDied() && base.Player.AmOwner)
        {
            base.Player.RemoveModifier<UnstableModifier>();
            return;
        }
        isUnstable = false;
    }

    float tpTimer;

     
    public override void Update()
    {
        base.Update();

        if(!Player.AmOwner) return;

        if(tpTimer <= 0)
        {
            RandomlyTeleport();
            tpTimer = ResetTPTimer();
        }
        else
        {
            tpTimer -= Time.deltaTime;
        }
    }

     
    void RandomlyTeleport()
    {
        if(!(MeetingHud.Instance || ExileController.Instance))
        {
            System.Collections.Generic.List<PlayerControl> playersList = Helpers.GetAlivePlayers().Where(pl => pl != Player && !pl.HasModifier<ImmovableModifier>()).ToList();
            if(playersList.Count > 0)
            {
                playersList.Shuffle();
                
                PlayerControl randPlayer = playersList[UnityEngine.Random.Range(0, playersList.Count)];

                RpcTransport(Player, Player.PlayerId, randPlayer.PlayerId);
            
                Info("Transported In An Unstable Way");
            }
        }
    }
    static float ResetTPTimer()
    {
        float minTPTime = OptionGroupSingleton<UnstableOptions>.Instance.UnstableMinCooldown;
        float maxTPTime = OptionGroupSingleton<UnstableOptions>.Instance.UnstableMaxCooldown;

        return UnityEngine.Random.Range(minTPTime, maxTPTime);
    }

     
    public override void OnDeath(DeathReason reason)
    {
        base.OnDeath(reason);

        Player.RemoveModifier(this);
    }

    #region Transport
    public static void RpcTransport(PlayerControl transporter, byte player1, byte player2)
    {
        var t1 = GetTarget(player1);
        var t2 = GetTarget(player2);

        var play1 = MiscUtils.PlayerById(player1)!;
        var play2 = MiscUtils.PlayerById(player2)!;

        if (play1.TryGetModifier<ShyModifier>(out var shy))
        {
            shy.OnRoundStart();
        }

        if (play2.TryGetModifier<ShyModifier>(out var shy2))
        {
            shy2.OnRoundStart();
        }

        if (t1.TryCast<DeadBody>())
        {
            PreCheckUndertaker(t1.TryCast<DeadBody>()!);
        }

        if (t2.TryCast<DeadBody>())
        {
            PreCheckUndertaker(t2.TryCast<DeadBody>()!);
        }

        var positions = GetAdjustedPositions(t1, t2);
        if (t1.TryCast<PlayerControl>() != null && t2.TryCast<DeadBody>() != null)
        {
            positions.Item1 = play1.Collider.bounds.center;
        }

        if (t2.TryCast<PlayerControl>() != null && t1.TryCast<DeadBody>() != null)
        {
            positions.Item2 = play2.Collider.bounds.center;
        }

        Transport(t1, positions.Item2);
        Transport(t2, positions.Item1);
        var touAbilityEvent = new TouAbilityEvent(AbilityType.TransporterTransport, transporter, t1, t2);
        MiraEventManager.InvokeEvent(touAbilityEvent);

        if (play1.AmOwner && t1 is PlayerControl || play2.AmOwner && t2 is PlayerControl)
        {
            var notif1 = Helpers.CreateAndShowNotification(
                $"<b>{Colors.Unstable.ToTextColor()}You have been transported</color></b>", Color.white,
                new Vector3(0f, 1f, -20f), spr: JamModifierIcons.Unstable.LoadAsset());

            notif1.AdjustNotification();

            if (Minigame.Instance)
            {
                Minigame.Instance.Close();
                Minigame.Instance.Close();
            }
        }

        MonoBehaviour? GetTarget(byte id)
        {
            var data = GameData.Instance.GetPlayerById(id);
            if (!data)
            {
                return null;
            }

            var stoned = MiscUtils.GetFreshStonedPlayerById(id);
            if (stoned != null)
            {
                return stoned;
            }

            var body = Helpers.GetBodyById(id);
            if (data.IsDead && body)
            {
                return body;
            }

            var pc = data.Object;
            if (!pc)
            {
                return null;
            }

            if (pc.HasModifier<NoTransportModifier>())
            {
                return null;
            }

            if (pc.GetModifiers<BaseModifier>().Any(x => x is IUntransportable))
            {
                return null;
            }

            if (pc.moveable || pc.inVent || (pc.TryGetModifier<DisabledModifier>(out var mod) &&
                                             (!mod.IsConsideredAlive || !mod.CanBeInteractedWith)))
            {
                if (pc.inVent)
                {
                    pc.MyPhysics.ExitAllVents();
                }

                return pc;
            }

            return null;
        }

        void PreCheckUndertaker(DeadBody body)
        {
            var mods = ModifierUtils.GetActiveModifiers<DragModifier>();

            foreach (var mod in mods)
            {
                if (mod.BodyId == body.ParentId)
                {
                    var dragMod = mod.Player.GetModifier<DragModifier>()!;
                    var dropPos = body.transform.position;
                    dropPos.z = dropPos.y / 1000f;
                    dragMod.DeadBody!.transform.position = dropPos;

                    var touAbilityEvent2 = new TouAbilityEvent(AbilityType.UndertakerDrop, mod.Player, dragMod.DeadBody);
                    MiraEventManager.InvokeEvent(touAbilityEvent2);

                    if (mod.Player.AmOwner)
                    {
                        CustomButtonSingleton<UndertakerDragDropButton>.Instance.SetDrag();
                    }

                    mod.Player.RemoveModifier(dragMod);
                }
            }
        }

        (Vector2, Vector2) GetAdjustedPositions(MonoBehaviour transportable, MonoBehaviour transportable2)
        {
            // assign dummy values so it doesnt error about returning unassigned variables
            Vector2 TP1Position = transportable.gameObject.transform.position;
            Vector2 TP2Position = transportable2.gameObject.transform.position;

            if (transportable.TryCast<DeadBody>() == null && transportable2.TryCast<DeadBody>() == null)
            {
                Error($"type: {transportable.GetIl2CppType().Name}");
                var TP1 = transportable.TryCast<PlayerControl>()!;
                var stoned1 = transportable.TryCast<StonedPlayer>();
                if (stoned1 == null)
                {
                    TP1Position = TP1.GetTruePosition();
                    TP1Position = new Vector2(TP1Position.x, TP1Position.y + 0.3636f);
                }

                var TP2 = transportable2.TryCast<PlayerControl>()!;
                var stoned2 = transportable2.TryCast<StonedPlayer>();
                if (stoned2 == null)
                {
                    TP2Position = TP2.GetTruePosition();
                    TP2Position = new Vector2(TP2Position.x, TP2Position.y + 0.3636f);
                }

                if (TP1 && TP1.HasModifier<MiniModifier>() || stoned1 != null && stoned1.IsMiniPlayer)
                {
                    TP1Position = new Vector2(TP1Position.x, TP1Position.y + 0.2233912f * 0.75f);
                    TP2Position = new Vector2(TP2Position.x, TP2Position.y - 0.2233912f * 0.75f);
                }
                if (TP2 && TP2.HasModifier<MiniModifier>() || stoned2 != null && stoned2.IsMiniPlayer)
                {
                    TP1Position = new Vector2(TP1Position.x, TP1Position.y - 0.2233912f * 0.75f);
                    TP2Position = new Vector2(TP2Position.x, TP2Position.y + 0.2233912f * 0.75f);
                }
            }
            else if (transportable.TryCast<DeadBody>() != null && transportable2.TryCast<DeadBody>() == null)
            {
                var Player1Body = transportable.TryCast<DeadBody>()!;
                TP1Position = Player1Body.TruePosition;
                TP1Position = new Vector2(TP1Position.x, TP1Position.y + 0.3636f);

                var TP2 = transportable2.TryCast<PlayerControl>()!;
                TP2Position = TP2.GetTruePosition();
                TP2Position = new Vector2(TP2Position.x, TP2Position.y + 0.3636f);

                if (TP2.HasModifier<MiniModifier>())
                {
                    TP1Position = new Vector2(TP1Position.x, TP1Position.y - 0.2233912f * 0.75f);
                    TP2Position = new Vector2(TP2Position.x, TP2Position.y + 0.2233912f * 0.75f);
                }
            }
            else if (transportable.TryCast<DeadBody>() == null && transportable2.TryCast<DeadBody>() != null)
            {
                var TP1 = transportable.TryCast<PlayerControl>()!;
                TP1Position = TP1.GetTruePosition();
                TP1Position = new Vector2(TP1Position.x, TP1Position.y + 0.3636f);

                var Player2Body = transportable2.TryCast<DeadBody>()!;
                TP2Position = Player2Body.TruePosition;
                TP2Position = new Vector2(TP2Position.x, TP2Position.y + 0.3636f);
                if (TP1.HasModifier<MiniModifier>())
                {
                    TP1Position = new Vector2(TP1Position.x, TP1Position.y + 0.2233912f * 0.75f);
                    TP2Position = new Vector2(TP2Position.x, TP2Position.y - 0.2233912f * 0.75f);
                }
            }
            else if (transportable.TryCast<DeadBody>() != null && transportable2.TryCast<DeadBody>() != null)
            {
                TP1Position = transportable.TryCast<DeadBody>()!.TruePosition;
                TP2Position = transportable2.TryCast<DeadBody>()!.TruePosition;
            }

            return (TP1Position, TP2Position);
        }
    }

    public static void Transport(MonoBehaviour mono, Vector3 position)
    {
        var stoned = mono.TryCast<StonedPlayer>();
        var deadBody = mono.TryCast<DeadBody>();
        var player = mono.TryCast<PlayerControl>();
        if (stoned != null)
        {
            if (stoned.ProgressStage > StoneStage.Frozen)
            {
                return;
            }
        }
        else
        {
            if (player != null && player.HasModifier<ImmovableModifier>())
            {
                return;
            }

            if (deadBody != null &&
                MiscUtils.PlayerById(deadBody.ParentId)?.HasModifier<ImmovableModifier>() == true)
            {
                return;
            }
        }

        if (player != null)
        {
            player.MyPhysics.ResetMoveState();
            player.transform.position = position;
            player.NetTransform.SnapTo(position);
        }

        mono.transform.position = position;
        Collider2D cd = mono.GetComponent<Collider2D>();
        if (cd != null && deadBody != null)
        {
            mono.transform.position += cd.bounds.center - position;
        }

        var cnt = mono.TryCast<CustomNetworkTransform>();
        if (cnt != null)
        {
            cnt.SnapTo(position, (ushort)(cnt.lastSequenceId + 1));

            if (cnt.AmOwner && ModCompatibility.IsSubmerged())
            {
                ModCompatibility.ChangeFloor(cnt.myPlayer.GetTruePosition().y > -7);
                ModCompatibility.CheckOutOfBoundsElevator(cnt.myPlayer);
            }
        }

        if (player != null && player.AmOwner)
        {
            // If the transported player is a Puppeteer/Parasite controlling someone, snap camera to the victim instead
            MonoBehaviour? cameraTarget = null;

            if (player.Data?.Role is ITransportTrigger triggerRole)
            {
                cameraTarget = triggerRole.OnTransport();
            }

            MiscUtils.SnapPlayerCamera(cameraTarget ?? PlayerControl.LocalPlayer);
        }
    }
    #endregion
}
