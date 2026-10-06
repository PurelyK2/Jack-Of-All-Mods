using HarmonyLib;
using Hazel;
using Il2CppSystem.Web.Util;
using JAM.Assets;
using JAM.Modifiers;
using JAM.Modifiers.Crewmate;
using JAM.Modifiers.Game.Universal;
using JAM.Options.Roles.Neutral;
using JAM.Roles.Neutral;
using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.Events.Vanilla.Meeting.Voting;
using MiraAPI.Events.Vanilla.Player;
using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using MiraAPI.Modifiers.Types;
using MiraAPI.Utilities.Assets;
using Rewired;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TownOfUs.Assets;
using TownOfUs.Events;
using TownOfUs.Modifiers;
using TownOfUs.Modifiers.Game;
using TownOfUs.Modifiers.Game.Assailant;
using TownOfUs.Modifiers.Game.Crewmate;
using TownOfUs.Modifiers.Game.Universal;
using TownOfUs.Modules.RainbowMod;
using TownOfUs.Options.Roles.Crewmate;
using TownOfUs.Roles;
using TownOfUs.Utilities;
using UnityEngine;

namespace JAM.Modifiers.Role;

public sealed class BountyTargetModifier : BaseModifier
{
    int buttonsLeft;
    public override string ModifierName => "Bounty Target";

    public override string GetDescription()
    {
        return "You Are The Bounty Hunter Target...\nGood Luck!";
    }
    public override bool HideOnUi => false;

    public override void OnActivate()
    {
        base.OnActivate();

        if (Player.HasModifier<BountySparedModifier>())
        {
            ModifierComponent?.RemoveModifier(this);
            return;
        }

        if (Player.Data.IsDead || !MiraAPI.Utilities.Helpers.GetAlivePlayers().Any(p => p.Data.Role is BountyHunterRole))
        {
            ModifierComponent?.RemoveModifier(this);
            return;
        }

        if (Player.AmOwner)
        {
            MiraAPI.Utilities.Helpers.CreateAndShowNotification("A Bounty Has Been Placed On You...", Colors.BountyHunter, new UnityEngine.Vector3(0f, 1f, -20f), null, JamRoleIcons.BountyHunter.LoadAsset());
        }
        else
        {
            MiraAPI.Utilities.Helpers.CreateAndShowNotification("A Bounty Has Been Placed On " + Player.Data.PlayerName + "'s Head.\nKill Them To Get A Reward!", Colors.BountyHunter, new UnityEngine.Vector3(0f, 1f, -20f), null, JamRoleIcons.BountyHunter.LoadAsset());
            
            if(OptionGroupSingleton<BountyHunterOptions>.Instance.TargetArrow)
                Player.AddModifier<BountyArrowModifier>(PlayerControl.LocalPlayer, Player.Data.Color, 0f);
        }

        Player.AddModifier<BountySparedModifier>();

        buttonsLeft = Player.RemainingEmergencies;
        Player.RemainingEmergencies = 0;
    }
    public override void OnDeactivate()
    {
        Player.RemainingEmergencies = buttonsLeft;

        base.OnDeactivate();
    }

    public void Update()
    {
        if (!MiraAPI.Utilities.Helpers.GetAlivePlayers().Any(p => p.Data.Role is BountyHunterRole))
        {
            MiraAPI.Utilities.Helpers.CreateAndShowNotification("The Bounty Hunter Has Died, They Can No Longer Give A Reward...", Colors.BountyHunter, new UnityEngine.Vector3(0f, 1f, -20f), null, JamRoleIcons.BountyHunter.LoadAsset());
        
            ModifierComponent?.RemoveModifier(this);
        }
    }

    public override void OnMeetingStart()
    {
        base.OnMeetingStart();
        ModifierComponent?.RemoveModifier(this);
    }

    static void GivePlayerBonus(PlayerControl player, PlayerControl target)
    {
        if (!player.AmOwner)
        {
            MiraAPI.Utilities.Helpers.CreateAndShowNotification("The Bounty Has Been Claimed...", Colors.BountyHunter, new UnityEngine.Vector3(0f, 1f, -20f), null, JamRoleIcons.BountyHunter.LoadAsset());
        }
        
        if (player.AmOwner)
            player.AddModifier<BountyRewardModifier>();
    }

    [RegisterEvent(0)]
    public static void PlayerDeathEventHandler(BeforeMurderEvent @event)
    {
        Info("Noticed Player Death");
        if (@event.Target.HasModifier<BountyTargetModifier>())
            GivePlayerBonus(@event.Source, @event.Target);
    }

    [HarmonyPatch(typeof(RoleBehaviour), "CanVent")]
    [HarmonyPrefix]
    public static bool DontVentAsTarget(ref RoleBehaviour __instance)
    {
        if(__instance.Player.HasModifier<BountyTargetModifier>())
        {
            MiraAPI.Utilities.Helpers.CreateAndShowNotification("Don't Even Think About It...", Color.red, null, JamRoleIcons.BountyHunter.LoadAsset());
            return false;
        }

        return true;
    }

    [HarmonyPatch(typeof(EndGameResult), "Create", [typeof(MessageReader)])]
    [HarmonyPrefix]
    public static void RemoveOnGameEnd()
    {
        MiraAPI.Utilities.Helpers.GetAlivePlayers().ForEach(p => {
            if(p.HasModifier<BountyTargetModifier>())
                p.RemoveModifier<BountyTargetModifier>();
        });
    }
}
public sealed class BountyArrowModifier(PlayerControl owner, Color color, float update) : ArrowTargetModifier(owner, color, update)
{
    public override string ModifierName => "Bounty Arrow";

    public override void OnActivate()
    {
        base.OnActivate();

        if (Arrow == null)
        {
            return;
        }

        var spr = Arrow.gameObject.GetComponent<SpriteRenderer>();
        var r = Arrow.gameObject.AddComponent<BasicRainbowBehaviour>();

        r.AddRend(spr, Player.cosmetics.ColorId);
    }

    public override void OnMeetingStart()
    {
        base.OnMeetingStart();

        ModifierComponent?.RemoveModifier(this);
    }

    public override void OnDeath(DeathReason reason)
    {
        TouAudio.PlaySound(TouAudio.TrackerDeactivateSound);

        base.OnDeath(reason);
    }

    public override void Update()
    {
        if(Player.Data.IsDead)
        {
            ModifierComponent?.RemoveModifier(this);
        }
    }
}
public sealed class BountyRewardModifier : BaseModifier
{
    public override string ModifierName => "Bounty Reward Modifier";
    public override bool HideOnUi => true;

    public override void OnActivate()
    {
        base.OnActivate();

        DeceiverRole.confuseRole = false;
        GiveRandomBounty();
        DeceiverRole.confuseRole = true;
    }

    void GiveRandomBounty()
    {
        if (!Player.AmOwner)
        {
            ModifierComponent?.RemoveModifier(this);
            return;
        }

        MiraAPI.Utilities.Helpers.CreateAndShowNotification("You killed the Bounty Hunter Target", Colors.BountyHunter, new UnityEngine.Vector3(0f, 1f, -20f), null, JamRoleIcons.BountyHunter.LoadAsset());
        BountyHunterOptions opts = OptionGroupSingleton<BountyHunterOptions>.Instance;

        RoleAlignment thisFaction = Player.Data.Role.GetRoleAlignment();
        bool canVent = Player.HasModifier<VentableModifier>() || Player.Data.Role.CanVent;
        bool canGetDouble = !Player.HasModifier<DoubleShotModifier>() && Player.HasModifier<AssassinModifier>();

        List<RewardType> rewards = new List<RewardType>();

        // Random Good Modifier From Your Faction
        if(Player.Data.Role.IsCrewmate())
            for (int i = 0; i < (int)opts.RandFactMod; i++)
            {
                rewards.Add(RewardType.GoodFactMod);
            }

        // Give Random Good Universal Modifier
        for (int i = 0; i < (int)opts.RandUnivMod; i++)
        {
            rewards.Add(RewardType.GoodUnivMod);
        }

        // Universal Decreased Cooldowns (Like Mage Energize?)
        /*
        for (int i = 0; i < (int)opts.lowerCooldowns; i++)
        {
            rewards.Add(RewardType.LowerCooldown);
        }
        */

        // Give Ventable (if can't vent)
        if(!canVent)
            for (int i = 0; i < (int)opts.GiveVentable; i++)
            {
                rewards.Add(RewardType.GiveVentable);
            }

        // Extra Vote (JOA Extra Vote Modifier?)
        for (int i = 0; i < (int)opts.GiveExtraVote; i++)
        {
            rewards.Add(RewardType.ExtraVote);
        }

        // Reveal Role (If Crew)
        if(Player.Data.Role.IsCrewmate())
            for (int i = 0; i < (int)opts.RevealCKRole; i++)
            {
                rewards.Add(RewardType.RevealRole);
            }

        // Double Shot (If you have assassin and no double shot)
        if(canGetDouble)
            for (int i = 0; i < (int)opts.GiveDblShot; i++)
            {
                rewards.Add(RewardType.DoubleShot);
            }

        // Shield until end of next round
        for (int i = 0; i < (int)opts.ShieldNextRound; i++)
        {
            rewards.Add(RewardType.GiveShield);
        }

        if(rewards.Count == 0)
        {
            Error("No Rewards Available");
            MiraAPI.Utilities.Helpers.CreateAndShowNotification("The Bounty Hunter Is Too Poor To Give Handouts", Colors.BountyHunter, new UnityEngine.Vector3(0f, 1f, -20f), null, JamRoleIcons.BountyHunter.LoadAsset());
            return;
        }


        rewards.Shuffle();
        RewardType thisReward = rewards[0];

        List<BaseModifier> modifiers = MiscUtils.AllModifiers.ToList();
        List<Type> possibleModifiers = new List<Type>();

        switch (thisReward)
        {
            case RewardType.GoodFactMod:
                possibleModifiers.Add(typeof(BaitModifier));
                possibleModifiers.Add(typeof(CelebrityModifier));
                possibleModifiers.Add(typeof(FrostyModifier));
                possibleModifiers.Add(typeof(InvestigatorModifier));
                possibleModifiers.Add(typeof(MultitaskerModifier));
                possibleModifiers.Add(typeof(NoisemakerModifier));
                possibleModifiers.Add(typeof(OperativeModifier));
                possibleModifiers.Add(typeof(ScientistModifier));
                possibleModifiers.Add(typeof(ScoutModifier));
                possibleModifiers.Add(typeof(SpyModifier));
                possibleModifiers.Add(typeof(TorchModifier));

                possibleModifiers.RemoveAll(m => Player.HasModifier(m));

                if(possibleModifiers.Count == 0)
                {
                    Error("No Possible Good Crewmate Modifiers To Give");
                    return;
                }

                possibleModifiers.Shuffle();
                Player.RpcAddModifier(possibleModifiers[0]);
                MiraAPI.Utilities.Helpers.CreateAndShowNotification("You Got A Random Crewmate Modifier", Colors.BountyHunter, new UnityEngine.Vector3(0f, 1f, -20f), null, JamRoleIcons.BountyHunter.LoadAsset());
                break;
            case RewardType.GoodUnivMod:
                possibleModifiers.Add(typeof(ButtonBarryModifier));
                possibleModifiers.Add(typeof(TiebreakerModifier));
                possibleModifiers.Add(typeof(ImmovableModifier));
                possibleModifiers.Add(typeof(RadarModifier));
                possibleModifiers.Add(typeof(ShyModifier));
                possibleModifiers.Add(typeof(SixthSenseModifier));
                possibleModifiers.Add(typeof(SleuthModifier));

                possibleModifiers.RemoveAll(m => Player.HasModifier(m));

                if (possibleModifiers.Count == 0)
                {
                    Error("No Possible Good Universal Modifiers To Give");
                    return;
                }

                possibleModifiers.Shuffle();
                Player.RpcAddModifier(possibleModifiers[0]);
                MiraAPI.Utilities.Helpers.CreateAndShowNotification("You Got A Random Universal Modifier", Colors.BountyHunter, new UnityEngine.Vector3(0f, 1f, -20f), null, JamRoleIcons.BountyHunter.LoadAsset());
                break;
            case RewardType.LowerCooldown:
                //Not Accessable Atm because not implemented
                MiraAPI.Utilities.Helpers.CreateAndShowNotification("Your Cooldowns Are Decreased", Colors.BountyHunter, new UnityEngine.Vector3(0f, 1f, -20f), null, JamRoleIcons.BountyHunter.LoadAsset());
                break;
            case RewardType.GiveVentable:
                Player.RpcAddModifier<VentableModifier>();
                MiraAPI.Utilities.Helpers.CreateAndShowNotification("You Can Now Vent", Colors.BountyHunter, new UnityEngine.Vector3(0f, 1f, -20f), null, JamRoleIcons.BountyHunter.LoadAsset());
                break;
            case RewardType.ExtraVote:
                if(!Player.HasModifier<JackOfAllVotes>())
                {
                    Player.RpcAddModifier<JackOfAllVotes>();
                }
                else
                {
                    JackOfAllVotes votes = Player.GetModifier<JackOfAllVotes>();
                    votes.NumVotes++;
                }
                MiraAPI.Utilities.Helpers.CreateAndShowNotification("You Have An Extra Vote", Colors.BountyHunter, new UnityEngine.Vector3(0f, 1f, -20f), null, JamRoleIcons.BountyHunter.LoadAsset());
                break;
            case RewardType.RevealRole:
                Player.RpcAddModifier<BountyRevealModifier>();
                MiraAPI.Utilities.Helpers.CreateAndShowNotification("Your Role Has Been Revealed To Everyone", Colors.BountyHunter, new UnityEngine.Vector3(0f, 1f, -20f), null, JamRoleIcons.BountyHunter.LoadAsset());
                break;
            case RewardType.DoubleShot:
                Player.RpcAddModifier<DoubleShotModifier>();
                MiraAPI.Utilities.Helpers.CreateAndShowNotification("You Know Have Double Shot", Colors.BountyHunter, new UnityEngine.Vector3(0f, 1f, -20f), null, JamRoleIcons.BountyHunter.LoadAsset());
                break;
            case RewardType.GiveShield:
                Player.RpcAddModifier<BountyShieldModifier>();
                MiraAPI.Utilities.Helpers.CreateAndShowNotification("You Have A Temporary Shield", Colors.BountyHunter, new UnityEngine.Vector3(0f, 1f, -20f), null, JamRoleIcons.BountyHunter.LoadAsset());
                break;
        }

        try
        {
            foreach (BountyHunterRole bountyHunter in MiraAPI.Utilities.Helpers.GetAlivePlayers().Where(p => p.Data.Role is BountyHunterRole).Select(p => p.Data.Role as BountyHunterRole))
            {
                if (bountyHunter != null)
                {
                    bountyHunter.NumBountiesCollected++;
                }
            }
        }
        catch (Exception e)
        {
            Error(e);
        }
    }

    enum RewardType
    {
        GoodFactMod,
        GoodUnivMod,
        LowerCooldown,
        GiveVentable,
        ExtraVote,
        RevealRole,
        DoubleShot,
        GiveShield
    }

    [HarmonyPatch(typeof(Vent), "Use")]
    public static class NoBountyHidingInVentsPatch
    {
        public static bool Prefix()
        {
            if(PlayerControl.LocalPlayer.HasModifier<BountyTargetModifier>())
                MiraAPI.Utilities.Helpers.CreateAndShowNotification("Don't Even Think About it...", Colors.BountyHunter, new UnityEngine.Vector3(0f, 1f, -20f), null, JamRoleIcons.BountyHunter.LoadAsset());
            return !PlayerControl.LocalPlayer.HasModifier<BountyTargetModifier>();
        }
    }

    [RegisterEvent(0)]
    public static void OnEndGameEvent(GameEndEvent __)
    {
        foreach(PlayerControl player in MiraAPI.Utilities.Helpers.GetAlivePlayers().Where(p => p.HasModifier<BountyTargetModifier>()))
        {
            player.RpcRemoveModifier<BountyTargetModifier>();
        }
    }
}
public sealed class BountySparedModifier : BaseModifier
{
    public override string ModifierName => "Bounty Immune";
    public override bool HideOnUi => true;

    public override void OnDeath(DeathReason reason)
    {
        ModifierComponent?.RemoveModifier(this);
    }

    [RegisterEvent(0)]
    public static void OnVotingCompleteEvent(VotingCompleteEvent @event)
    {
        foreach (PlayerControl player in PlayerControl.AllPlayerControls.ToArray().Where(p => p.HasModifier<BountySparedModifier>()))
        {
            player.RemoveModifier<BountySparedModifier>();
        }
    }
}
