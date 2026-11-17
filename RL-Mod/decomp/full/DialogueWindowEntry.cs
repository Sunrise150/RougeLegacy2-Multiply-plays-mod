using TMPro;
using UnityEngine;

public class DialogueWindowEntry : MonoBehaviour
{
	[SerializeField]
	private TMP_Text m_titleText;

	[SerializeField]
	private TMP_Text m_dialogueText;

	[SerializeField]
	private GameObject m_arrowObj;

	[SerializeField]
	private RectTransform m_rectTransform;

	[SerializeField]
	private CanvasGroup m_canvasGroup;

	[SerializeField]
	private Typewrite_RL m_typeWriter;

	public TMP_Text TitleText => m_titleText;

	public TMP_Text DialogueText => m_dialogueText;

	public GameObject ArrowObj => m_arrowObj;

	public RectTransform RectTransform => m_rectTransform;

	public CanvasGroup CanvasGroup => m_canvasGroup;

	public Typewrite_RL Typewrite => m_typeWriter;
}
