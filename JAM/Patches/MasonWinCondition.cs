using System.Runtime.CompilerServices;
using JAM.Modifiers;
using JAM.Roles.Neutral;
using MiraAPI.GameEnd;
using MiraAPI.Modifiers;
using MiraAPI.Utilities;
using TMPro;
using TownOfUs.Modules;
using UnityEngine;

namespace JAM.Patches.WinConditions;

public sealed class MasonWinCondition : CustomGameOver
{
	public override bool VerifyCondition(PlayerControl playerControl, NetworkedPlayerInfo[] winners)
    {
		float percentMasons = (float)Helpers.GetAlivePlayers().Count(p => p.HasModifier<MasonModifier>()) / (float)Helpers.GetAlivePlayers().Count;

		Info(percentMasons);

        if (percentMasons > 0.5f)
        {
            Info("Masons Should Win!");
            return true;
        }
		return false;
    }

	public override void AfterEndGameSetup(EndGameManager endGameManager)
	{
		endGameManager.BackgroundBar.material.SetColor(ShaderID.Color, Colors.Mason);
		TextMeshPro text = UnityEngine.Object.Instantiate<TextMeshPro>(endGameManager.WinText);
		text.text = "Masons Win!";
		text.color = Colors.Mason;
		DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(17, 2);
		defaultInterpolatedStringHandler.AppendLiteral("<color=#");
		defaultInterpolatedStringHandler.AppendFormatted(Colors.Mason);
		defaultInterpolatedStringHandler.AppendLiteral(">");
        defaultInterpolatedStringHandler.AppendLiteral("Masons Win!</color>");
        GameHistory.WinningFaction = defaultInterpolatedStringHandler.ToStringAndClear();
		Vector3 pos = endGameManager.WinText.transform.localPosition;
		pos.y = 1.5f;
		pos += Vector3.down * 0.15f;
		text.transform.localScale = new Vector3(1f, 1f, 1f);
		text.transform.position = pos;
		text.text = "<size=4>" + text.text + "</size>";
	}
}
