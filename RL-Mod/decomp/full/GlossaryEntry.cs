using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GlossaryEntry : MonoBehaviour
{
	[SerializeField]
	protected TMP_Text m_descriptionText;

	[SerializeField]
	protected TMP_Text m_titleText;

	[SerializeField]
	protected Image m_icon;

	public virtual void Initialize()
	{
	}
}
