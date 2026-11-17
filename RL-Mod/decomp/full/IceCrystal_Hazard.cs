using System.Collections;
using System.Runtime.CompilerServices;
using FMODUnity;
using UnityEngine;
using UnityEngine.Events;

public class IceCrystal_Hazard : Hazard, IPointHazard, IHazard, IRootObj, IBodyOnEnterHitResponse, IHitResponse, IBodyOnStayHitResponse
{
	public const float ICE_CRYSTAL_MIN_RADIUS = 3.5f;

	public const float ICE_CRYSTAL_MAX_RADIUS = 5f;

	[SerializeField]
	public UnityEvent TriggeredEvent;

	[SerializeField]
	private GameObject m_hitEffects;

	[SerializeField]
	private StudioEventEmitter m_breakEventEmitter;

	[SerializeField]
	private StudioEventEmitter m_lifetimeEventEmitter;

	[SerializeField]
	private StudioEventEmitter m_growEventEmitter;

	private IHitboxController m_hbController;

	private CircleCollider2D m_weaponCollider;

	private CircleCollider2D m_bodyCollider;

	private bool m_isGrowing;

	private float m_initialScale;

	private bool m_isPaused;

	protected override void Awake()
	{
		base.Awake();
		m_hbController = GetComponentInChildren<IHitboxController>();
		m_initialScale = base.transform.localScale.x;
	}

	private IEnumerator Start()
	{
		while (!m_hbController.IsInitialized)
		{
			yield return null;
		}
		m_hbController.SetHitboxActiveState(HitboxType.Weapon, active: false);
		m_weaponCollider = m_hbController.GetCollider(HitboxType.Weapon) as CircleCollider2D;
		m_bodyCollider = m_hbController.GetCollider(HitboxType.Body) as CircleCollider2D;
		if (!m_isGrowing)
		{
			StartCoroutine(InitialGrowIceCrystalCoroutine());
		}
	}

	private void OnEnable()
	{
		if (m_hbController.IsInitialized && !m_isGrowing)
		{
			StartCoroutine(InitialGrowIceCrystalCoroutine());
		}
	}

	private IEnumerator InitialGrowIceCrystalCoroutine()
	{
		m_isPaused = false;
		base.transform.localScale = new Vector3(m_initialScale, m_initialScale, 1f);
		float delay = Time.time + 0.75f;
		while (Time.time < delay)
		{
			yield return null;
		}
		yield return GrowIceCrystalCoroutine();
	}

	private IEnumerator GrowIceCrystalCoroutine()
	{
		m_isGrowing = true;
		base.Animator.speed = 1f;
		base.transform.localScale = new Vector3(m_initialScale, m_initialScale, 1f);
		m_lifetimeEventEmitter.SetParameter("hazard_snowflake_active", 1f, ignoreseekspeed: true);
		bool weaponHitboxDisabled = true;
		m_hbController.SetHitboxActiveState(HitboxType.Weapon, active: false);
		m_hbController.SetHitboxActiveState(HitboxType.Body, active: true);
		m_weaponCollider.radius = 3.5f;
		m_bodyCollider.radius = 3.5f;
		base.Animator.SetBool("Active", value: true);
		while (!base.Animator.GetCurrentAnimatorStateInfo(0).IsName("Grow"))
		{
			yield return null;
		}
		float growthDuration = m_initialScale / 0.3f;
		float totalGrowthDuration = Time.time + growthDuration;
		float speed = base.Animator.GetCurrentAnimatorStateInfo(0).length / growthDuration;
		base.Animator.speed = speed;
		float damageDelay = Time.time + 0.65f;
		while (Time.time < totalGrowthDuration)
		{
			float num = 1f - (totalGrowthDuration - Time.time) / growthDuration;
			float radius = 1.5f * num + 3.5f;
			m_weaponCollider.radius = radius;
			m_bodyCollider.radius = radius;
			if (Time.time >= damageDelay && weaponHitboxDisabled)
			{
				weaponHitboxDisabled = false;
				m_hbController.SetHitboxActiveState(HitboxType.Weapon, active: true);
			}
			m_growEventEmitter.SetParameter("hazard_snowflake_size", num);
			yield return null;
		}
		m_isGrowing = false;
	}

	public void PlayGrowSound()
	{
		m_growEventEmitter.Play();
	}

	public void BodyOnStayHitResponse(IHitboxController otherHBController)
	{
		BodyOnEnterHitResponse(otherHBController);
	}

	public void BodyOnEnterHitResponse(IHitboxController otherHBController)
	{
		StopAllCoroutines();
		m_isGrowing = false;
		StartCoroutine(ShatterIceCrystalCoroutine());
	}

	private IEnumerator ShatterIceCrystalCoroutine()
	{
		m_hbController.SetHitboxActiveState(HitboxType.Weapon, active: false);
		m_hbController.SetHitboxActiveState(HitboxType.Body, active: false);
		base.Animator.SetBool("Active", value: false);
		m_breakEventEmitter.Play();
		m_lifetimeEventEmitter.SetParameter("hazard_snowflake_active", 0f, ignoreseekspeed: true);
		float regrowDelay = Time.time + 2.75f;
		while (Time.time < regrowDelay)
		{
			yield return null;
		}
		StartCoroutine(GrowIceCrystalCoroutine());
	}

	public void SetIceCrystalPaused(bool pause)
	{
		if (m_isPaused != pause)
		{
			m_isPaused = pause;
			if (pause)
			{
				BodyOnEnterHitResponse(null);
				StopAllCoroutines();
			}
			else
			{
				StartCoroutine(GrowIceCrystalCoroutine());
			}
		}
	}

	public override void ResetHazard()
	{
		if ((bool)m_weaponCollider)
		{
			m_hbController.SetHitboxActiveState(HitboxType.Weapon, active: false);
			m_hbController.SetHitboxActiveState(HitboxType.Body, active: true);
			m_weaponCollider.radius = 3.5f;
			m_bodyCollider.radius = 3.5f;
		}
		base.Animator.SetBool("Active", value: false);
		base.transform.localScale = new Vector3(m_initialScale, m_initialScale, 1f);
		m_isGrowing = false;
		base.Animator.speed = 1f;
		m_hitEffects.SetActive(value: false);
	}

	[SpecialName]
	GameObject IRootObj.get_gameObject()
	{
		return base.gameObject;
	}
}
