using System.Collections;

public class Generic_TextPopup : TextPopupObj
{
	protected override IEnumerator SpawnEffectCoroutine()
	{
		float speed = 0.35f;
		yield return TweenManager.TweenBy(base.transform, speed, Ease.Quad.EaseOut, "localPosition.y", 2).TweenCoroutine;
		TweenManager.TweenBy(base.transform, speed, Ease.Quad.EaseIn, "localPosition.y", -1);
		yield return TweenManager.TweenTo(m_tmpText, speed / 2f, Ease.None, "delay", speed / 2f, "alpha", 0).TweenCoroutine;
		base.gameObject.SetActive(value: false);
	}
}
