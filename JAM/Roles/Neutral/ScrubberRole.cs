using AmongUs.GameOptions;
using Il2CppInterop.Runtime.Attributes;
using JAM.Assets;
using JAM.Modifiers;
using JAM;
using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.Modifiers;
using MiraAPI.Patches.Stubs;
using MiraAPI.Roles;
using MiraAPI.Utilities.Assets;
using Reactor.Networking.Attributes;
using TownOfUs.Assets;
using TownOfUs.Extensions;
using TownOfUs.Modifiers;
using TownOfUs.Modules;
using TownOfUs.Modules.Components;
using TownOfUs.Modules.Wiki;
using TownOfUs.Networking;
using TownOfUs.Roles;
using TownOfUs.Roles.Neutral;
using TownOfUs.Utilities;
using UnityEngine;

namespace JAM.Roles.Neutral;

public sealed class ScrubberRole(IntPtr cppPtr) : NeutralRole(cppPtr), ITownOfUsRole, IWikiDiscoverable, IDoomable
{
    public bool didWin { get; set; }

    public DoomableType DoomHintType => DoomableType.Fearmonger;
    public string LocaleKey => "Scrubber";
    public string RoleName => "Scrubber";
    public string RoleDescription => "Cleanse the land of modifiers to win";
    public string RoleMedDescription => RoleDescription;
    public string RoleLongDescription => RoleDescription;

    public string GetAdvancedDescription() { return RoleLongDescription + MiscUtils.AppendOptionsText(base.GetType()); }

    [HideFromIl2Cpp]
    public List<CustomButtonWikiDescription> Abilities
    {
        get
        {
            return new List<CustomButtonWikiDescription>
            {
				new("Cleanse", "Cleanse A Player Of All Their Modifiers", TouRoleIcons.Amnesiac),
            };
        }
    }

    public Color RoleColor => Colors.Scrubber;
    public ModdedRoleTeams Team => ModdedRoleTeams.Custom;
    public RoleAlignment RoleAlignment => RoleAlignment.NeutralOutlier;

    public CustomRoleConfiguration Configuration => new(this)
    {
        IconTmp = TmpSpriteUtils.CreateSpriteAsset(JamRoleIcons.Scrubber.LoadAsset(), "JackOfAllMods.Roles.Neutral.Scrubber", 1.45f),
        IntroSound = TouAudio.JanitorCleanSound,
        Icon = JamRoleIcons.Scrubber,
        GhostRole = (RoleTypes)RoleId.Get<NeutralGhostRole>()
    };
    public void OnRoundStart()
    {
        if (Player == null || Player.Data.IsDead || didWin || !Player.AmOwner) return;

        didWin = !ModifierUtils.GetPlayersWithModifier<BaseModifier>().Where(p => !p.Data.IsDead && p.Data.Role is not ScrubberRole).Any(p => p.GetModifiers<BaseModifier>().Any(m => !m.HideOnUi));

        if (didWin)
        {
            MiraAPI.Utilities.Helpers.CreateAndShowNotification("The world has been cleansed of impurities, the only thing left to cleanse is yourself...", RoleColor, new Vector3(0f, 1f, -20f), null, JamRoleIcons.Scrubber.LoadAsset());
        
            Player.RpcPlayerExile();
            Player.AddModifier<BasicGhostModifier>();
        }
    }
    public bool WinConditionMet()
    {
        return didWin;
    }

    public bool MetWinCon => didWin;

    public override bool DidWin(GameOverReason gameOverReason)
    {
        return WinConditionMet();
    }

        public override bool CanUse(IUsable usable)
    {
        if (!GameManager.Instance.LogicUsables.CanUse(usable, Player))
        {
            return false;
        }

        var console = usable.TryCast<Console>()!;
        return console == null || console.AllowImpostor;
    }



    [RegisterEvent(0)]
    public static void RoundStartHandler(RoundStartEvent @event)
    {
        if(@event.TriggeredByIntro) return;

        if(PlayerControl.LocalPlayer.Data.Role is ScrubberRole scrubber)
            scrubber.OnRoundStart();
    }

    [MethodRpc((uint) JAMRpcCalls.ScrubModifiers)]
    public static void RpcScrubModifiers(PlayerControl scrubber, PlayerControl scrubbedPlayer)
    {
        foreach (BaseModifier modifier in scrubbedPlayer.GetModifiers<BaseModifier>().Where(m => !m.HideOnUi))
        {
            scrubbedPlayer.RemoveModifier(modifier);
        }

        if (scrubbedPlayer.AmOwner)
        {
            MiraAPI.Utilities.Helpers.CreateAndShowNotification("Your Modifiers Have Been Scrubbed", Colors.Scrubber, new Vector3(0f, 1f, -20f), null, JamRoleIcons.Scrubber.LoadAsset());
        }
        if (scrubber.AmOwner)
        {
            MiraAPI.Utilities.Helpers.CreateAndShowNotification("You Have Successfully Scrubbed " + scrubbedPlayer.Data.PlayerName + "'s Modifiers", Colors.Scrubber, new Vector3(0f, 1f, -20f), null, JamRoleIcons.Scrubber.LoadAsset());
        }
    }
}
