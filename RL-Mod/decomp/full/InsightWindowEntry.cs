using TMPro;
using UnityEngine;

public class InsightWindowEntry : MonoBehaviour
{
	[SerializeField]
	private TMP_Text m_titleText;

	[SerializeField]
	private TMP_Text m_subtitleText;

	[SerializeField]
	private TMP_Text m_descriptionText;

	public bool IsResolvedEntry { get; private set; }

	public InsightType InsightType { get; private set; }

	public void SetInsightType(InsightType insightType, bool isResolvedEntry)
	{
		IsResolvedEntry = isResolvedEntry;
		InsightType = insightType;
		if (Insight_EV.LocIDTable.ContainsKey(insightType))
		{
			InsightLocIDEntry insightLocIDEntry = Insight_EV.LocIDTable[insightType];
			m_titleText.text = LocalizationManager.GetString(insightLocIDEntry.TitleLocID, isFemale: false);
			m_subtitleText.text = LocalizationManager.GetString(insightLocIDEntry.SubTitleLocID, isFemale: false);
			if (!isResolvedEntry)
			{
				m_descriptionText.text = LocalizationManager.GetString(insightLocIDEntry.DiscoveredTextLocID, isFemale: false);
			}
			else
			{
				m_descriptionText.text = LocalizationManager.GetString(insightLocIDEntry.ResolvedTextLocID, isFemale: false);
			}
		}
	}
}
