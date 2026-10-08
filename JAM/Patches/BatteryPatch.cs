using HarmonyLib;
using System.Collections.Generic;
using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using TownOfUs.Utilities;
using TownOfUs.Options;
using TownOfUs.Roles.Crewmate;
using UnityEngine;
using Object = UnityEngine.Object;
using JAM.Modifiers;
using TMPro;


namespace JAM.Patches;

[HarmonyPatch(typeof(MapCountOverlay), nameof(MapCountOverlay.Update))]
public static class DisableAdminComms
{
    [HarmonyPostfix]
    public static void Postfix(MapCountOverlay __instance)
    {
        if (!PlayerControl.LocalPlayer.HasModifier<BatteryModifier>())
        {
            return;
        }
        __instance.SabotageText?.gameObject.SetActive(false);
        __instance.BackgroundColor.SetColor(Color.green);

        HashSet<int> hashSet = new HashSet<int>();
        for (int i = 0; i < __instance.CountAreas.Length; i++)
        {
            CounterArea counterArea = __instance.CountAreas[i];
            if (counterArea.DetectiveExclusiveLocation)
            {
                continue;
            }

            if (ShipStatus.Instance.FastRooms.TryGetValue(counterArea.RoomType, out var value) && value.roomArea != null)
            {
                int num = value.roomArea.OverlapCollider(__instance.filter, __instance.buffer);
                int num2 = 0;
                for (int j = 0; j < num; j++)
                {
                    Collider2D val = __instance.buffer[j];
                    if (val.CompareTag("DeadBody") && __instance.includeDeadBodies)
                    {
                        DeadBody component = val.GetComponent<DeadBody>();
                        if (component != null && hashSet.Add(component.ParentId))
                        {
                            num2++;
                        }
                    }
                    else if (!val.isTrigger)
                    {
                        PlayerControl component2 = val.GetComponent<PlayerControl>();
                        if (component2 != null && component2.Data != null && !component2.Data.Disconnected && !component2.Data.IsDead && (__instance.showLivePlayerPosition || !component2.AmOwner) && hashSet.Add(component2.PlayerId))
                        {
                            num2++;
                        }
                    }
                }
                counterArea.UpdateCount(num2);
            }
            else
            {
                Warning("Couldn't find counter for:" + counterArea.RoomType);
            }
        }
    }
}


[HarmonyPatch(typeof(VitalsMinigame), nameof(VitalsMinigame.Update))]
public static class DisableVitalComms
{
    [HarmonyPostfix]
    public static void Postfix(VitalsMinigame __instance)
    {
        if (!PlayerControl.LocalPlayer.HasModifier<BatteryModifier>())
        {
            return;
        }
        __instance.SabText?.gameObject.SetActive(false);
        for (int i = 0; i < __instance.vitals.Length; i++)
        {
            __instance.vitals[i].gameObject.SetActive(true);
        }

        // for (int j = 0; j < __instance.vitals.Length; j++)
        // {
        //     VitalsPanel vitalsPanel = __instance.vitals[j];
        //     if (!vitalsPanel.PlayerInfo.IsDead && vitalsPanel.PlayerInfo.Disconnected && !vitalsPanel.IsDiscon)
        //     {
        //         vitalsPanel.SetDisconnected();
        //     }
        //     else if (vitalsPanel.PlayerInfo.IsDead && !vitalsPanel.IsDead && !vitalsPanel.IsDiscon)
        //     {
        //         vitalsPanel.SetDead();
        //     }
        // }
    }
}

[HarmonyPatch(typeof(PlanetSurveillanceMinigame), nameof(PlanetSurveillanceMinigame.Update))]
public static class DisablePolusCamsComms
{
    [HarmonyPostfix]
    public static void Postfix(PlanetSurveillanceMinigame __instance)
    {
        if (!PlayerControl.LocalPlayer.HasModifier<BatteryModifier>())
        {
            return;
        }
        __instance.SabText?.gameObject.SetActive(false);
        __instance.ViewPort.sharedMaterial = __instance.DefaultMaterial;
        __instance.ViewPort.material.SetTexture("_MainTex", (Texture)(object)__instance.texture);
    }
}

[HarmonyPatch(typeof(SurveillanceMinigame), nameof(SurveillanceMinigame.Update))]
public static class DisableSkeldCamsComms
{
    [HarmonyPostfix]
    public static void Postfix(SurveillanceMinigame __instance)
    {
        if (!PlayerControl.LocalPlayer.HasModifier<BatteryModifier>())
        {
            return;
        }
            for (int i = 0; i < __instance.ViewPorts.Length; i++)
            {
                __instance.ViewPorts[i].sharedMaterial = __instance.DefaultMaterial;
                __instance.ViewPorts[i].material.SetTexture("_MainTex", __instance.textures[i]);
                __instance.SabText[i].gameObject.SetActive(false);
            }
    }
}

[HarmonyPatch(typeof(SecurityLogGame), nameof(SecurityLogGame.Update))]
public static class DisableLogsComms
{
    [HarmonyPostfix]
    public static void Postfix(SecurityLogGame __instance)
    {
        if (!PlayerControl.LocalPlayer.HasModifier<BatteryModifier>())
        {
            return;
        }

        __instance.SabText.gameObject.SetActive(false);
        __instance.RefreshScreen();
    }
}

//DEBUGGING CODE

// [HarmonyPatch(typeof(Minigame), nameof(Minigame.Begin))]
// public static class UtilityDebugPatch
// {
//     [HarmonyPostfix]
//     public static void Postfix(Minigame __instance)
//     {
//         Error($"[BatteryDebug] Minigame opened: {__instance.GetType().FullName}");

//         LogChildren(__instance.transform, 0);
//     }

//     private static void LogChildren(Transform parent, int depth)
//     {
//         if (parent == null)
//         {
//             return;
//         }

//         var indent = new string(' ', depth * 2);

//         Error($"[BatteryDebug] {indent}{parent.name} | Active={parent.gameObject.activeSelf}");

//         for (var i = 0; i < parent.childCount; i++)
//         {
//             LogChildren(parent.GetChild(i), depth + 1);
//         }
//     }
// }

// [HarmonyPatch(typeof(MapCountOverlay), nameof(MapCountOverlay.OnEnable))]
// public static class AdminDebugPatch
// {
//     [HarmonyPostfix]
//     public static void Postfix(MapCountOverlay __instance)
//     {
//         Error("[BatteryDebug] MapCountOverlay enabled");

//         LogChildren(__instance.transform, 0);
//     }

//     private static void LogChildren(Transform parent, int depth)
//     {
//         if (parent == null)
//         {
//             return;
//         }

//         var indent = new string(' ', depth * 2);

//         Error($"[BatteryDebug] {indent}{parent.name} | Active={parent.gameObject.activeSelf}");

//         for (var i = 0; i < parent.childCount; i++)
//         {
//             LogChildren(parent.GetChild(i), depth + 1);
//         }
//     }
// }
