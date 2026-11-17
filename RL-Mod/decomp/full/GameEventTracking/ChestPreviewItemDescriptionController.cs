using System.Collections;
using TMPro;
using UnityEngine;

namespace GameEventTracking;

public class ChestPreviewItemDescriptionController : MonoBehaviour
{
	[SerializeField]
	private CanvasGroup m_itemDescriptionCanvasGroup;

	[SerializeField]
	private TextMeshProUGUI m_itemNameText;

	[SerializeField]
	private TextMeshProUGUI m_itemRarityText;

	[SerializeField]
	[Range(0.1f, 1f)]
	private float m_fadeTimeMultiplier = 0.5f;

	private void OnEnable()
	{
		m_itemDescriptionCanvasGroup.alpha = 0f;
	}

	private IEnumerator AnimateTextPosition(float time, float distance)
	{
		yield return TweenManager.TweenBy_UnscaledTime(m_itemDescriptionCanvasGroup.transform, time, Ease.Elastic.EaseOut, "position.y", distance).TweenCoroutine;
	}

	private IEnumerator FadeText(float time)
	{
		float timeStart = Time.unscaledTime;
		m_itemDescriptionCanvasGroup.alpha = 0f;
		while (Time.unscaledTime - timeStart < time)
		{
			m_itemDescriptionCanvasGroup.alpha = (Time.unscaledTime - timeStart) / time;
			yield return null;
		}
		m_itemDescriptionCanvasGroup.alpha = 1f;
	}

	public void Initialise(string name, string rarity, Color rarityTextColor, float popDistance, float popTime)
	{
		m_itemNameText.text = name;
		m_itemRarityText.text = rarity;
		m_itemRarityText.color = rarityTextColor;
		StartCoroutine(AnimateTextPosition(popTime, popDistance));
		StartCoroutine(FadeText(m_fadeTimeMultiplier * popTime));
	}
}
