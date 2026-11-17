using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TeleporterEntry : MonoBehaviour
{
	[SerializeField]
	private TeleporterSubEntry m_teleporterSubEntryPrefab;

	[SerializeField]
	private TMP_Text m_entryTitleText;

	[SerializeField]
	private Image m_entryIcon;

	[SerializeField]
	private RectTransform m_teleporterSubEntries;

	private LocalizationItem m_locItem;

	private List<TeleporterSubEntry> m_subEntriesList;

	public List<TeleporterSubEntry> SubEntriesList => m_subEntriesList;

	public BiomeType BiomeType { get; private set; }

	public void Initialize()
	{
		m_subEntriesList = new List<TeleporterSubEntry>();
		m_locItem = m_entryTitleText.GetComponent<LocalizationItem>();
	}

	public void SetBiomeType(BiomeType biomeType)
	{
		BiomeType = biomeType;
		BiomeData data = BiomeDataLibrary.GetData(biomeType);
		if ((bool)data)
		{
			m_locItem.SetString(data.BiomeNameLocID);
		}
		BiomeArtData artData = BiomeArtDataLibrary.GetArtData(biomeType);
		if ((bool)artData)
		{
			m_entryIcon.color = artData.Ferr2DBiomeArtData.MapColor;
		}
	}

	public void AddSubEntry(GlobalTeleporterType subEntryType, GridPointManager gridPointManager)
	{
		TeleporterSubEntry teleporterSubEntry = Object.Instantiate(m_teleporterSubEntryPrefab, m_teleporterSubEntries.transform.transform);
		teleporterSubEntry.SetSubEntryType(subEntryType, gridPointManager.BiomeControllerIndex, BiomeType, gridPointManager);
		teleporterSubEntry.gameObject.SetActive(value: false);
		m_subEntriesList.Add(teleporterSubEntry);
	}
}
