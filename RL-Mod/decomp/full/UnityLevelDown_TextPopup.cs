using System.Collections;
using UnityEngine;

public class UnityLevelDown_TextPopup : TextPopupObj
{
	protected override IEnumerator SpawnEffectCoroutine()
	{
		float speed = 0.35f;
		yield return TweenManager.TweenBy_UnscaledTime(base.transform, speed, Ease.Quad.EaseOut, "localPosition.y", 1).TweenCoroutine;
		float startTime = Time.unscaledTime;
		float waitTime = 0.5f;
		while (startTime + waitTime > Time.unscaledTime)
		{
			yield return null;
		}
		TweenManager.TweenBy_UnscaledTime(base.transform, speed, Ease.Quad.EaseIn, "localPosition.y", 1);
		yield return TweenManager.TweenTo_UnscaledTime(m_tmpText, speed / 2f, Ease.None, "delay", speed / 2f, "alpha", 0).TweenCoroutine;
		base.gameObject.SetActive(value: false);
	}
}
