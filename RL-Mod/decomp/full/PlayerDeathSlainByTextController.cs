using GameEventTracking;
using TMPro;
using UnityEngine;

public class PlayerDeathSlainByTextController : MonoBehaviour
{
	[SerializeField]
	private TMP_Text m_slainByText;

	public TMP_Text Text => m_slainByText;

	public void UpdateMessage(bool isVictory)
	{
		string slainBy = GameEventTrackerManager.EnemyEventTracker.GetSlainBy();
		string localizedPlayerName = LocalizationManager.GetLocalizedPlayerName(SaveManager.PlayerSaveData.CurrentCharacter);
		string playerKiller = GameEventTrackerManager.EnemyEventTracker.GetPlayerKiller();
		if (!isVictory)
		{
			string text = string.Format(slainBy, localizedPlayerName, playerKiller);
			m_slainByText.text = text;
		}
		else
		{
			string @string = LocalizationManager.GetString("LOC_ID_ENDING_VICTORY_DEATH_SLAIN_TEXT_OVERRIDE_1", SaveManager.PlayerSaveData.CurrentCharacter.IsFemale);
			m_slainByText.text = string.Format(@string, localizedPlayerName);
		}
	}
}
