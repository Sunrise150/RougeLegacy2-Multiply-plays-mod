using System.Collections;
using UnityEngine;

public class Interact_TextPopup : TextPopupObj
{
	protected override IEnumerator SpawnEffectCoroutine()
	{
		Vector3 localScale = base.transform.localScale;
		base.transform.localScale = localScale * 0.5f;
		yield return TweenManager.TweenTo(base.transform, 0.15f, Ease.Back.EaseOutLarge, "localScale.x", localScale.x, "localScale.y", localScale.y, "localScale.z", localScale.z).TweenCoroutine;
	}
}
