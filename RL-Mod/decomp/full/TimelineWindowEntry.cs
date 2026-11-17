using TMPro;
using UnityEngine;

public class TimelineWindowEntry : MonoBehaviour
{
	[SerializeField]
	private TMP_Text m_titleText;

	[SerializeField]
	private TMP_Text m_descriptionText;

	public BurdenType BurdenType { get; private set; }

	public void SetBurdenType(BurdenType burdenType)
	{
		BurdenType = burdenType;
		BurdenData burdenData = BurdenLibrary.GetBurdenData(burdenType);
		m_titleText.text = LocalizationManager.GetString(burdenData.Title, isFemale: false);
		m_descriptionText.text = LocalizationManager.GetString(burdenData.Description, isFemale: false);
	}
}
