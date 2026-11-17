using System.Collections;
using UnityEngine;

public class DisclaimerPositionController : MonoBehaviour
{
	[SerializeField]
	private GameObject m_disclaimerPositionObj;

	private bool m_isFirstObj;

	public void OnEnable()
	{
		if ((bool)m_disclaimerPositionObj)
		{
			if (m_disclaimerPositionObj.activeSelf)
			{
				m_isFirstObj = true;
				m_disclaimerPositionObj.SetActive(value: false);
			}
			base.transform.position = m_disclaimerPositionObj.transform.position;
		}
		if (m_isFirstObj)
		{
			StartCoroutine(UpdatePositionHack());
		}
	}

	private IEnumerator UpdatePositionHack()
	{
		yield return null;
		base.transform.position = m_disclaimerPositionObj.transform.position;
	}
}
