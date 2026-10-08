using HarmonyLib;
using InnerNet;
using AmongUs.GameOptions;

namespace JAM.Patches;

[HarmonyPatch]
public static class MorticianPatch
{
    public static readonly Dictionary<byte, RoleTypes> RoleBeforeDeath = new();

    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.FixedUpdate))]
    [HarmonyPostfix]
    public static void CacheRoles()
    {
        if (AmongUsClient.Instance.GameState != InnerNetClient.GameStates.Started)
        {
            return;
        }

        foreach (var player in PlayerControl.AllPlayerControls)
        {
            if (!player ||
                player.Data == null ||
                player.Data.IsDead ||
                player.Data.Role == null)
            {
                continue;
            }

            RoleBeforeDeath[player.PlayerId] = player.Data.Role.Role;
        }
    }

    [HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.CoStartGame))]
    [HarmonyPostfix]
    public static void ResetCache()
    {
        RoleBeforeDeath.Clear();
    }

    public static RoleBehaviour? ResolveRoleBeforeDeath(byte playerId)
    {
        if (!RoleBeforeDeath.TryGetValue(playerId, out var roleType))
        {
            return null;
        }

        if (RoleManager.Instance == null)
        {
            return null;
        }

        return RoleManager.Instance.GetRole(roleType);
    }
}
