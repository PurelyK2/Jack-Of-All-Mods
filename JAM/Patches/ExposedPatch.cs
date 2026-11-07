using HarmonyLib;
using JAM.Modifiers;
using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using TownOfUs.Options;
using TownOfUs.Roles.Crewmate;
using TownOfUs.Utilities;
using UnityEngine;
using Object = UnityEngine.Object;


namespace NullsMod.Patches;

[HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.BloopAVoteIcon))]
public static class ExposedPatch
{
    public static void Postfix(NetworkedPlayerInfo voterPlayer, Transform parent)
    {
        if (TownOfUs.Utilities.MiscUtils.PlayerById(voterPlayer.PlayerId)?.HasModifier<ExposedModifier>() != true) return;

        var icon = FindLatestVoteIcon(parent);
        if (icon != null)
        {
            PlayerMaterial.SetColors(voterPlayer.Color, icon);
        }
    }
    private static SpriteRenderer? FindLatestVoteIcon(Transform parent)
    {
        SpriteRenderer? latest = null;
        for (var i = 0; i < parent.childCount; i++)
        {
            var child = parent.GetChild(i);
            if (child.name != "playerVote(Clone)")
            {
                continue;
            }

            var rend = child.GetComponent<SpriteRenderer>();
            if (rend != null)
            {
                latest = rend;
            }
        }

        return latest;
    }
}