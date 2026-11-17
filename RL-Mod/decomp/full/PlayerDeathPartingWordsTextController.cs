using TMPro;
using UnityEngine;

public class PlayerDeathPartingWordsTextController : MonoBehaviour
{
	[SerializeField]
	private TMP_Text m_partingWordsText;

	private int m_partingWordsIndex;

	private string[] m_partingWordsLocIDArray = new string[32]
	{
		"LOC_ID_TUTORIAL_HINTS_BOSS_CD_1", "LOC_ID_TUTORIAL_HINTS_EQUIPMENT_1", "LOC_ID_TUTORIAL_HINTS_UPGRADING_EQUIPMENT_1", "LOC_ID_TUTORIAL_HINTS_UNITY_EXPLANATION_1", "LOC_ID_TUTORIAL_HINTS_FLAMEBARRIER_ROTATION_1", "LOC_ID_TUTORIAL_HINTS_SPINKICK_SIDE_RANGE_1", "LOC_ID_TUTORIAL_HINTS_MAGIC_DAMAGE_MEAT_1", "LOC_ID_TUTORIAL_HINTS_DASH_RECOVERY_1", "LOC_ID_TUTORIAL_HINTS_ENEMY_LEVELING_1", "LOC_ID_TUTORIAL_HINTS_DODGE_PRACTICE_1",
		"LOC_ID_TUTORIAL_HINTS_LOOK_FOR_CHESTS_1", "LOC_ID_TUTORIAL_HINTS_FINDING_RUNE_ORE_1", "LOC_ID_TUTORIAL_HINTS_TELEPORTER_INVINCIBILITY_1", "LOC_ID_TUTORIAL_HINTS_FINDING_EQUIPMENT_ORE_1", "LOC_ID_TUTORIAL_HINTS_BOSS_WEAKNESSES_1", "LOC_ID_TUTORIAL_HINTS_HEIRLOOM_EXPLANATIONS_1", "LOC_ID_TUTORIAL_HINTS_EXPLORE_1", "LOC_ID_TUTORIAL_HINTS_SWORD_DASH_ATTACK_1", "LOC_ID_TUTORIAL_HINTS_SWORD_BACK_STEP_1", "LOC_ID_TUTORIAL_HINTS_SHIELD_BLOCK_REMINDER_1",
		"LOC_ID_TUTORIAL_HINTS_SHIELD_HOLD_BLOCK_1", "LOC_ID_TUTORIAL_HINTS_SHIELD_PERFECT_BLOCK_1", "LOC_ID_TUTORIAL_HINTS_SHIELD_DASH_BLOCK_1", "LOC_ID_TUTORIAL_HINTS_SHIELD_BLOCK_VULNERABLE_1", "LOC_ID_TUTORIAL_HINTS_CHAKRAM_BOUNCE_1", "LOC_ID_TUTORIAL_HINTS_CHAKRAM_MULTIPLE_WEAPONS_1", "LOC_ID_TUTORIAL_HINTS_BOW_PERFECT_SHOT_1", "LOC_ID_TUTORIAL_HINTS_BOW_AIR_TIME_1", "LOC_ID_TUTORIAL_HINTS_BOW_ARC_1", "LOC_ID_TUTORIAL_HINTS_CREATE_PLATFORM_STATUS_EFFECT_1",
		"LOC_ID_TUTORIAL_HINTS_CREATE_PLATFORM_BLOCKS_PROJECTILES_1", "LOC_ID_TUTORIAL_HINTS_CREATE_PLATFORM_SIDE_WALLS_1"
	};

	public TMP_Text Text => m_partingWordsText;

	public void UpdateMessage(bool isVictory)
	{
		if (!isVictory)
		{
			string text;
			if (m_partingWordsLocIDArray != null && m_partingWordsLocIDArray.Length != 0)
			{
				if (PlayerManager.GetCurrentPlayerRoom().SpecialRoomType == SpecialRoomType.Heirloom && (SaveManager.PlayerSaveData.CurrentCharacter.TraitOne == TraitType.CantAttack || SaveManager.PlayerSaveData.CurrentCharacter.TraitTwo == TraitType.CantAttack) && (PlayerManager.GetCurrentPlayerRoom() as Room).GridPointManager.RoomMetaData.RoomPath.Contains("Levels Arena Challenge"))
				{
					text = LocalizationManager.GetString("LOC_ID_ARENA_MENU_ARENA_DEATH_1", SaveManager.PlayerSaveData.CurrentCharacter.IsFemale);
				}
				else
				{
					m_partingWordsIndex = Random.Range(0, m_partingWordsLocIDArray.Length);
					text = LocalizationManager.GetString(m_partingWordsLocIDArray[m_partingWordsIndex], SaveManager.PlayerSaveData.CurrentCharacter.IsFemale);
				}
			}
			else
			{
				text = "A tip to help you out next time";
			}
			m_partingWordsText.text = text;
		}
		else
		{
			m_partingWordsText.text = LocalizationManager.GetString("LOC_ID_ENDING_VICTORY_DEATH_HINT_TEXT_OVERRIDE_1", SaveManager.PlayerSaveData.CurrentCharacter.IsFemale);
		}
	}

	public void CycleUpdateMessage(bool cycleUp)
	{
		if (cycleUp)
		{
			m_partingWordsIndex++;
		}
		else
		{
			m_partingWordsIndex--;
		}
		if (m_partingWordsIndex < -1)
		{
			m_partingWordsIndex = m_partingWordsLocIDArray.Length - 1;
		}
		else if (m_partingWordsIndex >= m_partingWordsLocIDArray.Length)
		{
			m_partingWordsIndex = -1;
		}
		if (m_partingWordsIndex == -1)
		{
			m_partingWordsText.text = LocalizationManager.GetString("LOC_ID_ENDING_VICTORY_DEATH_HINT_TEXT_OVERRIDE_1", SaveManager.PlayerSaveData.CurrentCharacter.IsFemale);
		}
		else
		{
			m_partingWordsText.text = LocalizationManager.GetString(m_partingWordsLocIDArray[m_partingWordsIndex], SaveManager.PlayerSaveData.CurrentCharacter.IsFemale);
		}
	}
}
