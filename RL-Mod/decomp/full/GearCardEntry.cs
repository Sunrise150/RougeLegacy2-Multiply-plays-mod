using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GearCardEntry : MonoBehaviour
{
	public enum GearCardEntryType
	{
		None,
		Equipment,
		Rune,
		Heirloom,
		Relic
	}

	[SerializeField]
	private TMP_Text m_descriptionText;

	[SerializeField]
	private TMP_Text m_titleText;

	[SerializeField]
	private Image m_icon;

	public GearCardEntryType GearCardType { get; private set; }

	public void SetAsEquipment(EquipmentCategoryType category, EquipmentType equipType)
	{
		EquipmentData equipmentData = EquipmentLibrary.GetEquipmentData(category, equipType);
		string text = LocalizationManager.GetString(equipmentData.Title, isFemale: false);
		if (string.IsNullOrEmpty(text) || text.Contains("LOC_ID"))
		{
			text = Equipment_EV.GetFormattedEquipmentName(category, equipType);
		}
		int upgradeLevel = EquipmentManager.GetUpgradeLevel(category, equipType);
		if (upgradeLevel > 0)
		{
			text = text + " +" + upgradeLevel;
		}
		m_titleText.text = text;
		m_descriptionText.text = LocalizationManager.GetString(equipmentData.Description, isFemale: false);
		m_icon.sprite = IconLibrary.GetEquipmentIcon(category, equipType);
		GearCardType = GearCardEntryType.Equipment;
	}

	public void SetAsRune(RuneType runeType)
	{
		RuneData runeData = RuneLibrary.GetRuneData(runeType);
		RuneObj rune = RuneManager.GetRune(runeType);
		m_titleText.text = LocalizationManager.GetString(runeData.Title, isFemale: false);
		if (rune.EquippedLevel > 0)
		{
			TMP_Text titleText = m_titleText;
			titleText.text = titleText.text + " +" + rune.EquippedLevel;
		}
		m_descriptionText.text = LocalizationManager.GetString(runeData.Description, isFemale: false);
		m_icon.sprite = IconLibrary.GetRuneIcon(runeType);
		GearCardType = GearCardEntryType.Rune;
	}

	public void SetAsHeirloom(HeirloomType heirloomType)
	{
		HeirloomData heirloomData = HeirloomLibrary.GetHeirloomData(heirloomType);
		m_titleText.text = LocalizationManager.GetString(heirloomData.TitleLocID, isFemale: false);
		m_descriptionText.text = LocalizationManager.GetString(heirloomData.DescriptionLocID, isFemale: false);
		m_icon.sprite = IconLibrary.GetHeirloomSprite(heirloomType);
		GearCardType = GearCardEntryType.Heirloom;
	}

	public void SetAsRelic(RelicType relicType)
	{
		RelicData relicData = RelicLibrary.GetRelicData(relicType);
		RelicObj relic = SaveManager.PlayerSaveData.GetRelic(relicType);
		m_titleText.text = LocalizationManager.GetString(relicData.Title, isFemale: false);
		m_descriptionText.text = LocalizationManager.GetString(relicData.Description, isFemale: false, out var isFormatString);
		if (relic != null)
		{
			if (isFormatString)
			{
				float altValue;
				float relicFormatString = Relic_EV.GetRelicFormatString(relicType, relic.Level, out altValue);
				m_descriptionText.text = string.Format(m_descriptionText.text, relicFormatString.ToCIString(), altValue.ToCIString());
			}
			if (relic.Level > 1)
			{
				m_titleText.text += $" (x{relic.Level})";
			}
			float costAmount = relicData.CostAmount;
			float relicCostMod = SkillTreeLogicHelper.GetRelicCostMod();
			int num = Mathf.RoundToInt(Mathf.Clamp((costAmount - relicCostMod) * (float)relic.Level, 0f, float.MaxValue) * 100f);
			string arg = string.Format(LocalizationManager.GetString("LOC_ID_GENERAL_UI_PERCENT_1", isFemale: false), -num);
			m_titleText.text += $" • <sprite=\"IconGlyph_Spritesheet_Texture\" name=\"Resolve_Icon\"> {arg}";
		}
		m_icon.sprite = IconLibrary.GetRelicSprite(relicType, getLargeIcon: false);
		GearCardType = GearCardEntryType.Relic;
	}

	public void SetAsUnity(EquipmentSetBonusType unityType, float bonusGain)
	{
		if (m_icon.transform.parent.gameObject.activeSelf)
		{
			m_icon.transform.parent.gameObject.SetActive(value: false);
		}
		if (m_descriptionText.gameObject.activeSelf)
		{
			m_descriptionText.gameObject.SetActive(value: false);
		}
		Vector3 localPosition = m_titleText.transform.parent.gameObject.transform.localPosition;
		localPosition.x = 40f;
		m_titleText.transform.parent.gameObject.transform.localPosition = localPosition;
		string arg = ((!CDGHelper.IsPercent(bonusGain)) ? bonusGain.ToString() : string.Format(LocalizationManager.GetString("LOC_ID_GENERAL_UI_PERCENT_1", isFemale: false), bonusGain * 100f));
		m_titleText.text = $"+{arg} ";
		m_titleText.text += LocalizationManager.GetString(EquipmentSetLibrary.GetEquipmentSetBonusLocID(unityType), isFemale: false);
	}
}
