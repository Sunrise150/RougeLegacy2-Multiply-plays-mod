using System.Collections;
using UnityEngine;

public class CharacterHit_TextPopup : TextPopupObj
{
	private static bool m_goRight_STATIC;

	protected override IEnumerator SpawnEffectCoroutine()
	{
		float globalDelay = m_animationClipDuration + Time.time;
		float randomX = ((!m_goRight_STATIC) ? Random.Range(-0.5f, -2f) : Random.Range(0.5f, 2f));
		m_goRight_STATIC = !m_goRight_STATIC;
		float startingX = base.transform.localPosition.x;
		float xPos;
		while (Time.time < globalDelay)
		{
			yield return null;
			xPos = (1f - (globalDelay - Time.time) / m_animationClipDuration) * randomX + startingX;
			base.transform.SetLocalPositionX(xPos);
		}
		xPos = randomX + startingX;
		base.transform.SetLocalPositionX(xPos);
		base.gameObject.SetActive(value: false);
	}
}
