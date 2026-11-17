using System.Collections;
using UnityEngine;

public class TextPopupTester : MonoBehaviour
{
	[SerializeField]
	private float m_initialDelay = 1f;

	[SerializeField]
	private float m_fireInterval = 1f;

	[SerializeField]
	private TextPopupType m_popupType;

	[SerializeField]
	private string m_text = "Please like and subscribe";

	private void OnEnable()
	{
		if (m_popupType != 0)
		{
			StartCoroutine(FireCoroutine());
		}
	}

	private IEnumerator FireCoroutine()
	{
		float delay2 = m_initialDelay + Time.time;
		while (Time.time < delay2)
		{
			yield return null;
		}
		while (true)
		{
			TextPopupObj textPopupObj = TextPopupManager.DisplayText(m_popupType, m_text, base.gameObject, base.transform.position, attachToSource: true);
			Debug.Log("TextPopup Type: " + m_popupType.ToString() + " uses the " + textPopupObj.name + " prefab.");
			delay2 = m_fireInterval + Time.time;
			while (Time.time < delay2)
			{
				yield return null;
			}
		}
	}
}
