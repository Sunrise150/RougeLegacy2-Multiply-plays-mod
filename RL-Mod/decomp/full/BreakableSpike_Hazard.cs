using System.Collections;
using Sigtrap.Relays;
using UnityEngine;
using UnityEngine.Events;

public class BreakableSpike_Hazard : Hazard, IBodyOnEnterHitResponse, IHitResponse, IBodyOnStayHitResponse, IEffectTriggerEvent_OnDeath
{
	[SerializeField]
	private bool m_isTallSpike;

	[SerializeField]
	private UnityEvent ExtendEvent;

	[SerializeField]
	private UnityEvent CollisionActivatedEvent;

	[SerializeField]
	private UnityEvent DestroyedEvent;

	[SerializeField]
	private BoxCollider2D m_hudCollider;

	private bool m_destroyed;

	private IHitboxController m_hbController;

	private BlinkPulseEffect m_blinkPulseEffect;

	private Relay<GameObject> m_onDeathEffectTriggerEventRelay = new Relay<GameObject>();

	public IRelayLink<GameObject> OnDeathEffectTriggerRelay => m_onDeathEffectTriggerEventRelay.link;

	public override float BaseDamage => 30f;

	protected override void Awake()
	{
		base.Awake();
		m_hbController = GetComponentInChildren<IHitboxController>();
		m_blinkPulseEffect = GetComponent<BlinkPulseEffect>();
	}

	private void OnEnable()
	{
		if (m_isTallSpike)
		{
			StartCoroutine(ExtendCoroutine(0.2f));
		}
		else
		{
			StartCoroutine(ExtendCoroutine(0.2f));
		}
		m_hudCollider.enabled = true;
	}

	private IEnumerator ExtendCoroutine(float extendDelay)
	{
		float startTime = Time.time;
		while (Time.time < startTime + extendDelay)
		{
			yield return null;
		}
		m_destroyed = false;
		m_hbController.SetHitboxActiveState(HitboxType.Body, active: true);
		if (ExtendEvent != null)
		{
			ExtendEvent.Invoke();
		}
		base.Animator.SetBool("Extend", value: true);
		if (m_isTallSpike)
		{
			while (Time.time < startTime + extendDelay + 1.3f)
			{
				yield return null;
			}
		}
		else
		{
			while (Time.time < startTime + extendDelay + 1f)
			{
				yield return null;
			}
		}
		if (CollisionActivatedEvent != null)
		{
			CollisionActivatedEvent.Invoke();
		}
		m_blinkPulseEffect.BlinkOnHitTint = new Color(1f, 0f, 0f, 0.8f);
		m_blinkPulseEffect.StartSingleBlinkEffect();
		m_hbController.SetHitboxActiveState(HitboxType.Weapon, active: true);
	}

	public void BodyOnEnterHitResponse(IHitboxController otherHBController)
	{
		if (!m_destroyed)
		{
			if (DestroyedEvent != null)
			{
				DestroyedEvent.Invoke();
			}
			StopAllCoroutines();
			m_hbController.SetHitboxActiveState(HitboxType.Weapon, active: false);
			m_hbController.SetHitboxActiveState(HitboxType.Body, active: false);
			m_blinkPulseEffect.BlinkOnHitTint = new Color(1f, 1f, 1f, 0.8f);
			m_blinkPulseEffect.StartSingleBlinkEffect();
			base.Animator.SetBool("Extend", value: false);
			base.Animator.Play("Retract");
			m_onDeathEffectTriggerEventRelay.Dispatch(otherHBController.RootGameObject);
			m_hudCollider.enabled = false;
			m_destroyed = true;
			if (m_isTallSpike)
			{
				StartCoroutine(ExtendCoroutine(6.5f));
			}
		}
	}

	public void BodyOnStayHitResponse(IHitboxController otherHBController)
	{
		BodyOnEnterHitResponse(otherHBController);
	}

	public override void ResetHazard()
	{
		m_destroyed = false;
		m_hbController.SetHitboxActiveState(HitboxType.Body, active: false);
		m_hbController.SetHitboxActiveState(HitboxType.Weapon, active: false);
		base.Animator.SetBool("Extend", value: false);
		if (base.gameObject.activeInHierarchy)
		{
			base.Animator.Update(0f);
			base.Animator.Update(0f);
		}
	}
}
