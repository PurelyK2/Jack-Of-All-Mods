using HarmonyLib;
using JAM.Modifiers;
using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using TownOfUs.Utilities;
using JAM.Options.Modifiers;

namespace JAM.Patches;

[HarmonyPriority(Priority.Last)]
[HarmonyPatch(typeof(ShipStatus), "CalculateLightRadius")]
public static class VisionPatch
{
    public static void Postfix(ShipStatus __instance, NetworkedPlayerInfo player, ref float __result)
    {
        if (MiscUtils.CurrentGamemode() == TouGamemode.HideAndSeek)
        { 
            return;
        }
        if (player == null || player.IsDead)
        {
            __result = __instance.MaxLightRadius;
            return;
        }

        if(player.Object.HasModifier<BlindModifier>(null))
        {
            __result *= (100f - OptionGroupSingleton<BlindOptions>.Instance.BlindAmount) / 100f;
        }

        if(player.Object.HasModifier<HyperfocusModifier>() && Minigame.Instance != null)
        {
            __result *= 0f;
        }
    }
}
