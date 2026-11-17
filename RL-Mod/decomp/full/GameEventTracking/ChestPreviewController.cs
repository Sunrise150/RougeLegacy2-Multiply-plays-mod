using System;
using UnityEngine;
using UnityEngine.UI;

namespace GameEventTracking;

public class ChestPreviewController : MonoBehaviour
{
	[Serializable]
	private class RarityColors
	{
		public Color Uncommon = Color.blue;

		public Color Rare = Color.yellow;

		public Color Unique = Color.magenta;
	}

	[SerializeField]
	private ChestPreviewItemDescriptionController m_itemDescriptionPrefab;

	[SerializeField]
	private float m_itemDescriptionPopDistance = 50f;

	[SerializeField]
	private float m_popStartYPosition = -50f;

	[SerializeField]
	private float m_popDistanceDecrement = 10f;

	[SerializeField]
	private float m_itemDescriptionPopTime = 0.5f;

	[SerializeField]
	private GameObject m_coins;

	[SerializeField]
	private Image m_image;

	[SerializeField]
	private RarityColors m_colors;

	public GameObject GameObject => base.gameObject;

	private void OnEnable()
	{
		m_coins.SetActive(value: false);
	}

	private string GetNameText(ISpecialItemData specialItemData)
	{
		string result = "";
		if (specialItemData is RuneTrackerData { RuneType: var runeType })
		{
			result = runeType.ToString();
		}
		else if (specialItemData is BlueprintTrackerData blueprintTrackerData)
		{
			result = $"{blueprintTrackerData.EquipmentType} {blueprintTrackerData.EquipmentCategory}";
		}
		return result;
	}

	public void Initialise(ChestTrackerData saveData)
	{
		m_image.sprite = AssetPreviewManager.GetPreviewImage(saveData.ChestType);
		if (saveData.ContainsGold)
		{
			m_coins.SetActive(value: true);
		}
	}
}
