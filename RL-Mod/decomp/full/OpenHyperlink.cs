using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class OpenHyperlink : MonoBehaviour, IPointerClickHandler, IEventSystemHandler
{
	private TMP_Text m_text;

	private void Awake()
	{
		m_text = GetComponent<TMP_Text>();
	}

	public void OnPointerClick(PointerEventData eventData)
	{
		int num = TMP_TextUtilities.FindIntersectingLink(m_text, Input.mousePosition, CameraController.UICamera);
		if (num != -1)
		{
			TMP_LinkInfo tMP_LinkInfo = m_text.textInfo.linkInfo[num];
			Application.OpenURL(tMP_LinkInfo.GetLinkID());
		}
	}
}
