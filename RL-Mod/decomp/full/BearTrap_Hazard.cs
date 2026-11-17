using System;
using System.Collections;
using UnityEngine;

public class BearTrap_Hazard : Hazard, ILevelConsumer
{
	private const float m_extensionDelay = 0.425f;

	private const float m_retractionDelay = 0.45f;

	private bool m_extendSpikes;

	private bool m_retractSpikes;

	public override float BaseDamage => 30f;

	private void OnEnable()
	{
		BaseRoom componentInParent = GetComponentInParent<BaseRoom>();
		if (componentInParent != null)
		{
			componentInParent.PlayerExitRelay.AddListener(OnPlayerExitRoom);
		}
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		BaseRoom componentInParent = GetComponentInParent<BaseRoom>();
		if (componentInParent != null)
		{
			componentInParent.PlayerExitRelay.RemoveListener(OnPlayerExitRoom);
		}
	}

	public void ExtendSpike()
	{
		m_retractSpikes = false;
		if (!m_extendSpikes)
		{
			m_extendSpikes = true;
			StopAllCoroutines();
			StartCoroutine(ExtensionCoroutine());
		}
	}

	private IEnumerator ExtensionCoroutine()
	{
		float extensionTimer = Time.time + 0.425f;
		while (extensionTimer > Time.time)
		{
			yield return null;
		}
		base.Animator.SetBool("SpikesOut", value: true);
		StartCoroutine(PerformExtensionCheck());
	}

	public override void Initialize(HazardArgs hazardArgs)
	{
		base.InitialState = hazardArgs.InitialState;
	}

	private IEnumerator PerformExtensionCheck()
	{
		while (!m_retractSpikes)
		{
			yield return null;
		}
		StartCoroutine(RetractionCoroutine());
	}

	private IEnumerator RetractionCoroutine()
	{
		m_extendSpikes = false;
		float retractionTimer = Time.time + 0.45f;
		while (retractionTimer > Time.time)
		{
			yield return null;
		}
		base.Animator.SetBool("SpikesOut", value: false);
	}

	public void RetractSpike()
	{
		m_retractSpikes = true;
	}

	private void OnPlayerExitRoom(object sender, EventArgs args)
	{
		ResetHazard();
	}

	public override void ResetHazard()
	{
		base.Animator.SetBool("SpikesOut", value: false);
		base.Animator.SetTrigger("Reset");
		m_extendSpikes = false;
	}
}
