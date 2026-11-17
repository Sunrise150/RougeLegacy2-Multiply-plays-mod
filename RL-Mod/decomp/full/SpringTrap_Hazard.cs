using System.Collections;
using UnityEngine;

public class SpringTrap_Hazard : Hazard
{
	private bool m_trapTriggered;

	public void BouncePlayer()
	{
		if (!m_trapTriggered && PlayerManager.IsInstantiated)
		{
			m_trapTriggered = true;
			StopAllCoroutines();
			StartCoroutine(SpringTrapCoroutine());
		}
	}

	private IEnumerator SpringTrapCoroutine()
	{
		base.Animator.SetBool("SpikesOut", value: true);
		if (PlayerManager.IsInstantiated)
		{
			PlayerManager.GetPlayerController().SetVelocityY(42f, additive: false);
		}
		float startTime = Time.time;
		while (Time.time < startTime + 0.25f)
		{
			yield return null;
		}
		base.Animator.SetBool("SpikesOut", value: false);
		m_trapTriggered = false;
	}

	public override void ResetHazard()
	{
		base.Animator.SetBool("SpikesOut", value: false);
		base.Animator.SetTrigger("Reset");
		m_trapTriggered = false;
	}
}
