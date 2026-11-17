using UnityEngine;
using UnityEngine.UI;

public class InvincibilityTimerUIController : MonoBehaviour
{
	[SerializeField]
	private Slider m_slider;

	[SerializeField]
	private Text m_text;

	[SerializeField]
	private GameObject m_indicator;

	[SerializeField]
	private bool m_isOn;
}
