using TMPro;
using UnityEngine;

public class TeleporterSubEntry : MonoBehaviour
{
	private static Color NOT_SELECTED_COLOR = new Color(0.4627451f, 33f / 85f, 38f / 85f);

	private static Color SELECTED_COLOR = Color.white;

	[SerializeField]
	private TMP_Text m_text;

	private LocalizationItem m_locItem;

	public int BiomeControllerIndex { get; private set; }

	public GlobalTeleporterType SubEntryType { get; private set; }

	public GridPointManager GridPointManager { get; private set; }

	public BiomeType BiomeType { get; private set; }

	private string GetLocID(GlobalTeleporterType subEntryType)
	{
		return subEntryType switch
		{
			GlobalTeleporterType.TransitionRoom => "LOC_ID_MAP_ICON_SUBENTRY_TRANSITION_TELEPORTER_1", 
			GlobalTeleporterType.BossEntrance => "LOC_ID_MAP_ICON_SUBENTRY_BOSS_TELEPORTER_1", 
			GlobalTeleporterType.SpecialRoom => "LOC_ID_MAP_ICON_SUBENTRY_SPECIAL_TELEPORTER_1", 
			GlobalTeleporterType.HeirloomRoom => "LOC_ID_MAP_ICON_SUBENTRY_HEIRLOOM_TELEPORTER_1", 
			_ => null, 
		};
	}

	public void SetSubEntryType(GlobalTeleporterType subEntryType, int biomeControllerIndex, BiomeType biomeType, GridPointManager gridPointManager)
	{
		if (!m_locItem)
		{
			m_locItem = GetComponent<LocalizationItem>();
		}
		SubEntryType = subEntryType;
		m_locItem.SetString(GetLocID(SubEntryType));
		BiomeControllerIndex = biomeControllerIndex;
		BiomeType = biomeType;
		GridPointManager = gridPointManager;
		SetSelected(selected: false);
	}

	public void SetSelected(bool selected)
	{
		if (selected)
		{
			m_text.color = SELECTED_COLOR;
		}
		else
		{
			m_text.color = NOT_SELECTED_COLOR;
		}
	}
}
