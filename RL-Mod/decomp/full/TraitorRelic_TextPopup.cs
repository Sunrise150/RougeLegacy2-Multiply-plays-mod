using System.Collections;
using UnityEngine;

public class TraitorRelic_TextPopup : TextPopupObj
{
	[SerializeField]
	private SpriteRenderer m_relicIcon;

	private RelicType m_relicType;

	public RelicType RelicType
	{
		get
		{
			return m_relicType;
		}
		set
		{
			m_relicType = value;
			Sprite relicSprite = IconLibrary.GetRelicSprite(m_relicType, getLargeIcon: true);
			m_relicIcon.sprite = relicSprite;
		}
	}

	protected override IEnumerator SpawnEffectCoroutine()
	{
		float speed = 0.35f;
		Color color = m_relicIcon.color;
		color.a = 0f;
		m_relicIcon.color = color;
		TweenManager.TweenBy(m_relicIcon, speed, Ease.None, "color.a", 1);
		yield return TweenManager.TweenBy(base.transform, speed, Ease.None, "localPosition.y", 0.5f).TweenCoroutine;
		yield return TweenManager.TweenBy(base.transform, 1f, Ease.Quad.EaseOut, "localPosition.y", 0.5f).TweenCoroutine;
		TweenManager.TweenBy(base.transform, speed, Ease.Quad.EaseIn, "localPosition.y", -0.25f);
		TweenManager.TweenTo(m_tmpText, speed / 2f, Ease.None, "delay", speed / 2f, "alpha", 0);
		yield return TweenManager.TweenTo(m_relicIcon, speed / 2f, Ease.None, "delay", speed / 2f, "color.a", 0).TweenCoroutine;
		base.gameObject.SetActive(value: false);
	}
}
