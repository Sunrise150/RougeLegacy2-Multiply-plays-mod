using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class AIMessage_AIExtension
{
	private static Dictionary<float, WaitForSeconds> m_waitForSecondsTable;

	public static IEnumerator ToDo(this BaseAIScript aiScript, string message, float waitTime = 0f)
	{
		_ = Application.isEditor;
		yield break;
	}

	public static IEnumerator DisplayMessage(this BaseAIScript aiScript, string message, float waitTime = 0f)
	{
		aiScript.DisplayMessage(message);
		if (waitTime < 0f)
		{
			waitTime = 0f;
		}
		if (m_waitForSecondsTable == null)
		{
			m_waitForSecondsTable = new Dictionary<float, WaitForSeconds>();
		}
		if (waitTime > 0f)
		{
			if (!m_waitForSecondsTable.ContainsKey(waitTime))
			{
				m_waitForSecondsTable.Add(waitTime, new WaitForSeconds(waitTime));
			}
			yield return m_waitForSecondsTable[waitTime];
		}
	}

	public static void ToDo(this BaseAIScript aiScript, string message)
	{
		_ = Application.isEditor;
	}

	public static void DisplayMessage(this BaseAIScript aiScript, string message)
	{
		Vector2 absPos = new Vector2(aiScript.EnemyController.Midpoint.x, aiScript.EnemyController.VisualBounds.max.y) + new Vector2(0f, 1f);
		TextPopupManager.DisplayTextAtAbsPos(TextPopupType.EquipmentOreCollected, message, absPos);
	}
}
