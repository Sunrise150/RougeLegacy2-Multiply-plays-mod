using System.Collections.Generic;
using UnityEngine;

public class Heirloom_EV
{
	private static Dictionary<HeirloomType, HeirloomDialogueEntry> m_heirloomDialogueTable = new Dictionary<HeirloomType, HeirloomDialogueEntry>
	{
		{
			HeirloomType.UnlockAirDash,
			new HeirloomDialogueEntry
			{
				HeirloomDialogue = new HeirloomTextEntry[2]
				{
					new HeirloomTextEntry("LOC_ID_HEIRLOOM_TITLE_DASH_INTRO_1", "LOC_ID_HEIRLOOM_TEXT_DASH_INTRO_1"),
					new HeirloomTextEntry("LOC_ID_HEIRLOOM_TITLE_DASH_INTRO_REPEAT_1", "LOC_ID_HEIRLOOM_TEXT_DASH_INTRO_REPEAT_1")
				},
				HeirloomCompleteDialogue = new HeirloomTextEntry("LOC_ID_HEIRLOOM_TITLE_DASH_END_1", "LOC_ID_HEIRLOOM_TEXT_DASH_END_1"),
				RandomizeRepeatDialogues = true
			}
		},
		{
			HeirloomType.UnlockMemory,
			new HeirloomDialogueEntry
			{
				HeirloomDialogue = new HeirloomTextEntry[2]
				{
					new HeirloomTextEntry("LOC_ID_HEIRLOOM_TITLE_MEMORY_INTRO_1", "LOC_ID_HEIRLOOM_TEXT_MEMORY_INTRO_1"),
					new HeirloomTextEntry("LOC_ID_HEIRLOOM_TITLE_MEMORY_INTRO_REPEAT_1", "LOC_ID_HEIRLOOM_TEXT_MEMORY_INTRO_REPEAT_1")
				},
				HeirloomCompleteDialogue = new HeirloomTextEntry("LOC_ID_HEIRLOOM_TITLE_MEMORY_END_1", "LOC_ID_HEIRLOOM_TEXT_MEMORY_END_1"),
				RandomizeRepeatDialogues = true
			}
		},
		{
			HeirloomType.UnlockBouncableDownstrike,
			new HeirloomDialogueEntry
			{
				HeirloomDialogue = new HeirloomTextEntry[2]
				{
					new HeirloomTextEntry("LOC_ID_HEIRLOOM_TITLE_SPIN_KICK_INTRO_1", "LOC_ID_HEIRLOOM_TEXT_SPIN_KICK_INTRO_1"),
					new HeirloomTextEntry("LOC_ID_HEIRLOOM_TITLE_SPIN_KICK_INTRO_REPEAT_1", "LOC_ID_HEIRLOOM_TEXT_SPIN_KICK_INTRO_REPEAT_1")
				},
				HeirloomCompleteDialogue = new HeirloomTextEntry("LOC_ID_HEIRLOOM_TITLE_SPIN_KICK_END_1", "LOC_ID_HEIRLOOM_TEXT_SPIN_KICK_END_1"),
				RandomizeRepeatDialogues = true
			}
		},
		{
			HeirloomType.UnlockDoubleJump,
			new HeirloomDialogueEntry
			{
				HeirloomDialogue = new HeirloomTextEntry[2]
				{
					new HeirloomTextEntry("LOC_ID_HEIRLOOM_TITLE_DOUBLE_JUMP_INTRO_1", "LOC_ID_HEIRLOOM_TEXT_DOUBLE_JUMP_INTRO_1"),
					new HeirloomTextEntry("LOC_ID_HEIRLOOM_TITLE_DOUBLE_JUMP_REPEAT_1", "LOC_ID_HEIRLOOM_TEXT_DOUBLE_JUMP_REPEAT_1")
				},
				HeirloomCompleteDialogue = new HeirloomTextEntry("LOC_ID_HEIRLOOM_TITLE_DOUBLE_JUMP_END_1", "LOC_ID_HEIRLOOM_TEXT_DOUBLE_JUMP_END_1"),
				RandomizeRepeatDialogues = true
			}
		},
		{
			HeirloomType.UnlockVoidDash,
			new HeirloomDialogueEntry
			{
				HeirloomDialogue = new HeirloomTextEntry[2]
				{
					new HeirloomTextEntry("LOC_ID_HEIRLOOM_TITLE_VOID_DASH_INTRO_1", "LOC_ID_HEIRLOOM_TEXT_VOID_DASH_INTRO_1"),
					new HeirloomTextEntry("LOC_ID_HEIRLOOM_TITLE_VOID_DASH_INTRO_REPEAT_1", "LOC_ID_HEIRLOOM_TEXT_VOID_DASH_INTRO_REPEAT_1")
				},
				HeirloomCompleteDialogue = new HeirloomTextEntry("LOC_ID_HEIRLOOM_TITLE_VOID_DASH_END_1", "LOC_ID_HEIRLOOM_TEXT_VOID_DASH_END_1"),
				RandomizeRepeatDialogues = true
			}
		},
		{
			HeirloomType.UnlockEarthShift,
			new HeirloomDialogueEntry
			{
				HeirloomDialogue = new HeirloomTextEntry[2]
				{
					new HeirloomTextEntry("LOC_ID_NAME_EARTH_SHIFT_NAME_1", "LOC_ID_HEIRLOOM_TEXT_EARTH_SHIFT_INTRO_1"),
					new HeirloomTextEntry("LOC_ID_NAME_EARTH_SHIFT_NAME_1", "LOC_ID_HEIRLOOM_TEXT_EARTH_SHIFT_INTRO_REPEAT_1")
				},
				HeirloomCompleteDialogue = new HeirloomTextEntry("LOC_ID_NAME_EARTH_SHIFT_NAME_1", "LOC_ID_HEIRLOOM_TEXT_EARTH_SHIFT_END_1"),
				RandomizeRepeatDialogues = true
			}
		}
	};

	public static readonly Color CAVE_LANTERN_DARKNESS_COLOR_DIM = new Color(0.03f, 0.03f, 0.03f);

	public const float CAVE_LANTERN_DARKNESS_AMOUNT_DIM = 0.83f;

	public const float CAVE_LANTERN_DARKNESS_SOFTNESS_DIM = 0.13f;

	public static readonly Color CAVE_LANTERN_DARKNESS_COLOR_LIT = new Color(0.03f, 0.03f, 0.03f);

	public const float CAVE_LANTERN_DARKNESS_AMOUNT_LIT = 0.51f;

	public const float CAVE_LANTERN_DARKNESS_SOFTNESS_LIT = 0.31f;

	public const float CAVE_LANTERN_BURDEN_VIEW_MOD = 0.05f;

	public const float CAVE_LANTERN_BURDEN_VIEW_CAP = 0.25f;

	public static HeirloomDialogueEntry GetDialogueEntry(HeirloomType heirloomType)
	{
		if (m_heirloomDialogueTable.TryGetValue(heirloomType, out var value))
		{
			return value;
		}
		return null;
	}

	public static bool IsHeirloomLocked(HeirloomType heirloomType)
	{
		switch (heirloomType)
		{
		case HeirloomType.UnlockBouncableDownstrike:
			return SaveManager.PlayerSaveData.GetHeirloomLevel(HeirloomType.UnlockAirDash) <= 0;
		case HeirloomType.UnlockVoidDash:
			if (SaveManager.PlayerSaveData.GetHeirloomLevel(HeirloomType.UnlockAirDash) > 0)
			{
				return SaveManager.PlayerSaveData.GetHeirloomLevel(HeirloomType.UnlockBouncableDownstrike) <= 0;
			}
			return true;
		default:
			return false;
		}
	}

	public static string GetLockedHeirloomLocID(HeirloomType heirloomType, bool isRepeat)
	{
		switch (heirloomType)
		{
		case HeirloomType.UnlockBouncableDownstrike:
			if (!isRepeat)
			{
				return "LOC_ID_HEIRLOOM_TEXT_SPIN_KICK_NO_ABILITY_1";
			}
			return "LOC_ID_HEIRLOOM_TEXT_SPIN_KICK_NO_ABILITY_REPEAT_1";
		case HeirloomType.UnlockVoidDash:
			return "LOC_ID_HEIRLOOM_TEXT_VOID_DASH_NO_ABILITY_1";
		default:
			return null;
		}
	}
}
