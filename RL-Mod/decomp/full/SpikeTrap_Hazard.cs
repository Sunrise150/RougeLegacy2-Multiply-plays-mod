using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class SpikeTrap_Hazard : Hazard
{
	[SerializeField]
	public UnityEvent TriggeredEvent;

	private const float m_extensionDelay = 0.425f;

	private const float m_retractionDelay = 0.45f;

	private IHitboxController m_hbController;

	private bool m_extendSpikes;

	private bool m_retractSpikes;

	public override float BaseDamage => 30f;

	protected override void Awake()
	{
		base.Awake();
		m_hbController = GetComponentInChildren<IHitboxController>();
	}

	private IEnumerator Start()
	{
		while (!m_hbController.IsInitialized)
		{
			yield return null;
		}
		m_hbController.SetHitboxActiveState(HitboxType.Weapon, active: false);
	}

	public void ExtendSpike()
	{
		if (TraitManager.IsTraitActive(TraitType.DisableSpikeTraps))
		{
			return;
		}
		m_retractSpikes = false;
		if (!m_extendSpikes)
		{
			m_extendSpikes = true;
			StopAllCoroutines();
			if (TriggeredEvent != null)
			{
				TriggeredEvent.Invoke();
			}
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
		base.Animator.Play("PopOutSpikes_Extend");
		yield return null;
		StartCoroutine(PerformExtensionCheck());
	}

	private IEnumerator PerformExtensionCheck()
	{
		while (base.Animator.GetCurrentAnimatorStateInfo(0).normalizedTime < 0.5f)
		{
			yield return null;
		}
		m_hbController.SetHitboxActiveState(HitboxType.Weapon, active: true);
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
		m_hbController.SetHitboxActiveState(HitboxType.Weapon, active: false);
		base.Animator.SetBool("SpikesOut", value: false);
		base.Animator.Play("PopOutSpikes_Retract");
		yield return null;
	}

	public void RetractSpike()
	{
		m_retractSpikes = true;
	}

	public override void ResetHazard()
	{
		base.Animator.SetBool("SpikesOut", value: false);
		base.Animator.Play("PopOutSpikes_Idle");
		m_extendSpikes = false;
		m_hbController.SetHitboxActiveState(HitboxType.Weapon, active: false);
	}
}
