using AmongUs.GameOptions;
using HarmonyLib;
using Hazel;
using JAM.Assets;
using JAM.Modifiers.Hidden;
using JAM.Options.Roles.Impostor;
using JAM.Roles.Neutral;
using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Meeting;
using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using MiraAPI.Roles;
using TownOfUs.Interfaces;
using TownOfUs.Modifiers;
using TownOfUs.Roles;
using TownOfUs.Roles.Crewmate;
using TownOfUs.Utilities;

namespace JAM.Modifiers.Role;

internal class MimicCacheModifier : BaseModifier
{
    static bool showAsMimic = false;

    public override string ModifierName => "Mimic Cache Modifier";
    public override bool HideOnUi => true;

    public override void OnDeath(DeathReason reason)
    {
        Player.RpcChangeRole(RoleId.Get<MimicRole>(), false);

        base.OnDeath(reason);

        ModifierComponent.RemoveModifier(this);
    }

    [RegisterEvent(0)]
    public static void OnEjectionEvent(EjectionEvent @event)
    {
        if (PlayerControl.LocalPlayer.HasModifier<MimicCacheModifier>())
        {
            PlayerControl.LocalPlayer.RpcChangeRole(RoleId.Get<MimicRole>(), false);
        }
    }
    [HarmonyPatch(typeof(EndGameResult), "Create", [typeof(MessageReader)])]
    public static class ReturnToMimicAtEnd
    {
        public static void Prefix()
        {
            foreach(PlayerControl player in PlayerControl.AllPlayerControls)
            {
                if (player.HasModifier<MimicCacheModifier>())
                    player.RpcChangeRole(RoleId.Get<MimicRole>(), false);
            }
        }
    }

    //Remove On True Role Change
    [HarmonyPatch(typeof(TownOfUs.Utilities.Extensions), "RpcChangeRole", [typeof(PlayerControl), typeof(ushort), typeof(bool)])]
    public static class TrulyChangeRolePatch
    {
        public static void Prefix(PlayerControl player, bool recordRole)
        {
            if(player.HasModifier<MimicCacheModifier>() && recordRole)
            {
                player.RpcRemoveModifier<MimicCacheModifier>();
            }
        }
    }

    [HarmonyPatch(typeof(LogicGameFlowNormal), nameof(LogicGameFlowNormal.CheckEndCriteria))]
    public static class DeceiverDoesntDeceiveTheGamePatch
    {
        [HarmonyPriority(801)]
        public static void Prefix()
        {
            showAsMimic = true;
        }
        public static void Postfix()
        {
            showAsMimic = false;
        }
    }

    [HarmonyPatch(typeof(NetworkedPlayerInfo), "Role", MethodType.Getter)]
    public static class GetMimicRolePatch
    {
        public static void Postfix(ref RoleBehaviour __result, ref NetworkedPlayerInfo __instance)
        {
            if((showAsMimic || !__result.Player.AmOwner) && __result.Player.HasModifier<MimicCacheModifier>())
            {
                __result = DestroyableSingleton<RoleManager>.Instance.GetRole((RoleTypes)RoleId.Get<MimicRole>());
            }
        }
    }

    [HarmonyPatch(typeof(TownOfUs.Utilities.Extensions), "IsImpostorAligned", [typeof(PlayerControl)])]
    public static class MimicIsntImpPatch
    {
        public static void Postfix(PlayerControl player, ref bool __result)
        {
            if (player.HasModifier<MimicCacheModifier>()) __result = false;
        }
    }
}
