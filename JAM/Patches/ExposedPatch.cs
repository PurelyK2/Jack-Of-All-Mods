using HarmonyLib;
using JAM.Modifiers;
using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using System.Reflection;
using System.Reflection.Emit;
using TownOfUs.Options;
using TownOfUs.Roles.Crewmate;
using TownOfUs.Utilities;
using UnityEngine;
using static Il2CppSystem.Xml.XmlWellFormedWriter.AttributeValueCache;
using Object = UnityEngine.Object;

namespace JAM.Patches;

[HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.BloopAVoteIcon))]
public static class ExposedPatch
{
    [HarmonyTranspiler]
    public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        MethodInfo hookMethod = AccessTools.Method(typeof(ExposedPatch), nameof(ExposedPatch.GetAnonymousVoteColor));
        
        FieldInfo targetField = AccessTools.Field(typeof(Palette), nameof(Palette.DisabledGrey));

        var matchAnyAccess = new CodeMatch(instruction =>
            (instruction.opcode == OpCodes.Ldsfld)
            && (FieldInfo)instruction.operand == targetField
        );

        var matcher = new CodeMatcher(instructions).Start().MatchStartForward(matchAnyAccess);

        if (matcher.IsValid)
        {
            Info("Found Patch Spot For Exposed Patch");
            matcher.SetAndAdvance(OpCodes.Call, AccessTools.Method(typeof(ExposedPatch), nameof(ExposedPatch.GetAnonymousVoteColor)));
        }
        else
        {
            Info("Couldn't Find Patch Spot For Exposed Patch");
        }

        return matcher.InstructionEnumeration();
    }

    public static Color GetAnonymousVoteColor(NetworkedPlayerInfo voterPlayer)
    {
        if(TownOfUs.Utilities.MiscUtils.PlayerById(voterPlayer.PlayerId)?.HasModifier<ExposedModifier>() == true)
        {
            return Palette.PlayerColors[voterPlayer.DefaultOutfit.ColorId];
        }
        else
        {
            return Palette.DisabledGrey;
        }
    }
}