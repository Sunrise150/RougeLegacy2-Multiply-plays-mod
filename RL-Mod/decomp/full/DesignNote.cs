using UnityEngine;

public class DesignNote : MonoBehaviour
{
	[SerializeField]
	private string m_text;

	[SerializeField]
	private Color m_textColour = Color.yellow;

	public string Text
	{
		get
		{
			return m_text;
		}
		set
		{
			m_text = value;
		}
	}

	public Color TextColour
	{
		get
		{
			return m_textColour;
		}
		set
		{
			m_textColour = value;
		}
	}
}
