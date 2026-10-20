using AmongUs.GameOptions;
using HarmonyLib;
using Hazel;
using JAM.Assets;
using JAM.Modifiers.Hidden;
using JAM.Options.Roles.Impostor;
using JAM.Roles.Neutral;
using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.Events.Vanilla.Meeting;
using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using MiraAPI.Roles;
using MiraAPI.Translation;
using TownOfUs.Assets;
using TownOfUs.Events;
using TownOfUs.Extensions;
using TownOfUs.Interfaces;
using TownOfUs.Modifiers;
using TownOfUs.Modifiers.Crewmate;
using TownOfUs.Modifiers.Game;
using TownOfUs.Modules;
using TownOfUs.Modules.Wiki;
using TownOfUs.Options;
using TownOfUs.Options.Roles.Crewmate;
using TownOfUs.Roles;
using TownOfUs.Roles.Crewmate;
using TownOfUs.Roles.Neutral;
using TownOfUs.Utilities;

namespace JAM.Modifiers.Role;

public sealed class MimicCacheModifier : BaseModifier, ICachedRole
{
    public override string ModifierName => "Mimic Cache Modifier";
    public override bool HideOnUi => true;

    public bool ShowCurrentRoleFirst => true;

    public bool Visible => Player.AmOwner || PlayerControl.LocalPlayer.HasDied() ||
                           FairyRole.FairySeesRoleVisibilityFlag(Player);

    public CacheRoleGuess GuessMode => CacheRoleGuess.ActiveOrCachedRole;

    public RoleBehaviour CachedRole => RoleManager.Instance.GetRole((RoleTypes)RoleId.Get<MimicRole>());
    public string CachedRoleName => "M";

    public override void OnActivate()
    {
        Player.AddModifier<MimicAllianceModifier>();

        base.OnActivate();
    }

    [RegisterEvent(0)]
    public static void OnEjectionEvent(EjectionEvent @event)
    {
        if(PlayerControl.LocalPlayer.HasModifier<MimicCacheModifier>())
        {
            PlayerControl.LocalPlayer.RpcChangeRole(RoleId.Get<MimicRole>(), false);
        }
    }

    //Remove On True Role Change
    [HarmonyPatch(typeof(TownOfUs.Utilities.Extensions), "RpcChangeRole", [typeof(PlayerControl), typeof(ushort), typeof(bool)])]
    public static class TrulyChangeRolePatch
    {
        public static void Prefix(PlayerControl player, bool recordRole)
        {
            if (player?.HasModifier<MimicCacheModifier>() == true && recordRole)
            {
                player.RpcRemoveModifier<MimicCacheModifier>();
            }
        }
    }
}

public sealed class MimicAllianceModifier : AllianceGameModifier, IWikiDiscoverable, IContinuesGame
{
    public override string ModifierName => "Mimic Alliance Modifier";
    public bool IsHiddenFromList => true;
    public override bool GetsPunished => false;
    public override AlliedFaction TrueFactionType => AlliedFaction.NeutralKiller;
    public override bool HideOnUi => true;
    public override string Symbol => "";

    public bool ContinuesGame => (DestroyableSingleton<RoleManager>.Instance.GetRole((RoleTypes)RoleId.Get<MimicRole>()) as IContinuesGame)?.ContinuesGame == true;

    public override int GetAssignmentChance()
    {
        return 0;
    }

    public override void OnDeath(DeathReason reason)
    {
        Player?.RpcChangeRole(RoleId.Get<MimicRole>(), false);
        ModifierComponent?.RemoveModifier(this);

        base.OnDeath(reason);
    }

    [RegisterEvent(0)]
    public void OnGameEndEvent(GameEndEvent @event)
    {
        ModifierComponent?.RemoveModifier(this);
        Player?.RpcChangeRole(RoleId.Get<MimicRole>(), false);
    }
}