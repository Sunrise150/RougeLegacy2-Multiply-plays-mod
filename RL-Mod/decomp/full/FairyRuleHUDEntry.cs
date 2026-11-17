using TMPro;
using UnityEngine;

public class FairyRuleHUDEntry : MonoBehaviour
{
	[SerializeField]
	private TextMeshProUGUI m_text;

	[SerializeField]
	private Color m_defaultColor = Color.white;

	[SerializeField]
	private Color m_passedColor = Color.green;

	[SerializeField]
	private Color m_failedColor = Color.red;

	public void Initialise(string text)
	{
		m_text.color = m_defaultColor;
		SetText(text);
	}

	public void SetText(string text)
	{
		if (!string.IsNullOrEmpty(text))
		{
			m_text.text = text;
		}
		else
		{
			m_text.text = "Text was not set";
		}
	}

	public void SetState(FairyRoomState state)
	{
		switch (state)
		{
		case FairyRoomState.Passed:
			m_text.fontStyle = FontStyles.Normal;
			m_text.color = m_passedColor;
			break;
		case FairyRoomState.Failed:
			m_text.fontStyle = FontStyles.Strikethrough;
			m_text.color = m_failedColor;
			break;
		default:
			m_text.fontStyle = FontStyles.Normal;
			m_text.color = m_defaultColor;
			break;
		}
	}
}
