using System.Collections;
using UnityEngine;

public class MaxHealthChange_TextPopup : TextPopupObj
{
	protected override IEnumerator SpawnEffectCoroutine()
	{
		float speed = 0.35f;
		base.transform.SetLocalPositionX(base.transform.localPosition.x - 50f);
		yield return TweenManager.TweenBy(base.transform, speed, Ease.Quad.EaseOut, "localPosition.x", 50).TweenCoroutine;
		float timeDelay = Time.time + 1f;
		while (Time.time < timeDelay)
		{
			yield return null;
		}
		TweenManager.TweenBy(base.transform, speed, Ease.Quad.EaseIn, "localPosition.x", 100);
		yield return TweenManager.TweenTo(m_tmpText, speed / 2f, Ease.None, "delay", speed / 2f, "alpha", 0).TweenCoroutine;
		base.gameObject.SetActive(value: false);
	}
}
