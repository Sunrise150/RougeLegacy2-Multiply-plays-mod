using System.Collections;
using UnityEngine;

public class XPGain_TextPopup : TextPopupObj
{
	public bool FastPlay { get; set; }

	protected override IEnumerator SpawnEffectCoroutine()
	{
		float speed = 0.25f;
		Vector3 localScale = base.transform.localScale;
		base.transform.localScale *= 0.75f;
		m_tmpText.alpha = 1f;
		TweenManager.TweenBy(base.transform, speed, Ease.Quad.EaseOut, "localPosition.y", 0.5f);
		yield return TweenManager.TweenTo(base.transform, speed, Ease.Back.EaseOut, "localScale.x", localScale.x, "localScale.y", localScale.y, "localScale.z", localScale.z).TweenCoroutine;
		float startTime = Time.time;
		while (Time.time < startTime + 0.25f)
		{
			yield return null;
		}
		yield return TweenManager.TweenTo(m_tmpText, speed, Ease.None, "alpha", 0).TweenCoroutine;
		base.gameObject.SetActive(value: false);
	}

	public override void ResetValues()
	{
		FastPlay = false;
		base.ResetValues();
	}
}
