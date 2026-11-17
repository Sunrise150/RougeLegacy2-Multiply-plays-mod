using System.Collections;
using UnityEngine;

public class TimedCrumblePlatform : CrumblePlatform
{
	private WaitRL_Yield m_crumbleDelayYield;

	private Coroutine m_crackedAnimCoroutine;

	protected override void Awake()
	{
		base.Awake();
		m_crumbleDelayYield = new WaitRL_Yield(0f);
	}

	protected override IEnumerator StartCrumbleCoroutine()
	{
		if (m_crackedAnimCoroutine != null)
		{
			StopCoroutine(m_crackedAnimCoroutine);
		}
		m_crackedAnimCoroutine = StartCoroutine(CrackedAnimCoroutine(0.25f));
		m_crumbleDelayYield.CreateNew(1f);
		yield return m_crumbleDelayYield;
		if (m_crackedAnimCoroutine != null)
		{
			StopCoroutine(m_crackedAnimCoroutine);
		}
		m_platformCollider.gameObject.layer = LayerMask.NameToLayer("Ignore Raycast");
		m_terrainCollider.gameObject.layer = LayerMask.NameToLayer("Ignore Raycast");
		m_animator.SetTrigger("Crumble");
		if (m_reformCoroutine != null)
		{
			StopCoroutine(m_reformCoroutine);
			m_reformCoroutine = null;
		}
		m_reformCoroutine = StartCoroutine(ReformCoroutine(1f));
	}

	private IEnumerator CrackedAnimCoroutine(float animInterval)
	{
		float startTime = Time.time;
		while (true)
		{
			if (Time.time > startTime + animInterval)
			{
				startTime = Time.time;
				AnimatorStateInfo currentAnimatorStateInfo = m_animator.GetCurrentAnimatorStateInfo(0);
				m_animator.Play(currentAnimatorStateInfo.shortNameHash, 0, 0f);
			}
			yield return null;
		}
	}

	public override void ResetCrumble()
	{
		base.ResetCrumble();
		if (m_crackedAnimCoroutine != null)
		{
			StopCoroutine(m_crackedAnimCoroutine);
		}
	}
}
