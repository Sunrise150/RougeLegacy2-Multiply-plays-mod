using TMPro;
using UnityEngine;

public class NextPatchTimer : MonoBehaviour
{
	[SerializeField]
	private TMP_Text m_patchTimerText;

	private void Start()
	{
		m_patchTimerText.text = "TBD";
	}
}
