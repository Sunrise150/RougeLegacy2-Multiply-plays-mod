using System.Collections;
using UnityEngine;

public class PlayerRankUp_TextPopup : TextPopupObj
{
	protected override IEnumerator SpawnEffectCoroutine()
	{
		float speed = 0.35f;
		yield return TweenManager.TweenBy_UnscaledTime(base.transform, speed, Ease.Quad.EaseOut, "localPosition.y", 0.5f).TweenCoroutine;
		float startTime = Time.unscaledTime;
		while (Time.unscaledTime < startTime + 0.75f)
		{
			yield return null;
		}
		TweenManager.TweenBy_UnscaledTime(base.transform, speed, Ease.Quad.EaseIn, "localPosition.y", 0.5f);
		yield return TweenManager.TweenTo_UnscaledTime(m_tmpText, speed, Ease.None, "alpha", 0).TweenCoroutine;
		base.gameObject.SetActive(value: false);
	}
}
