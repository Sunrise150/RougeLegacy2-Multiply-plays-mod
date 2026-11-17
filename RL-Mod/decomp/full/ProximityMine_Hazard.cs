using System;
using System.Collections;
using System.Runtime.CompilerServices;
using FMODUnity;
using UnityEngine;

public class ProximityMine_Hazard : Hazard, IPointHazard, IHazard, IRootObj, IHasProjectileNameArray, ITerrainOnEnterHitResponse, IHitResponse
{
	private enum ProximityMineState
	{
		Idle,
		Activated,
		CoolingDown
	}

	[SerializeField]
	private StudioEventEmitter m_triggerAudioEventEmitter;

	[SerializeField]
	private StudioEventEmitter m_explodeAudioEventEmitter;

	private const string EXPLOSION_PROJECTILE_NAME = "ProximityMineExplosionProjectile";

	private const string WARNING_PROJECTILE_NAME = "ProximityMineWarningProjectile";

	private float m_explosionRadius;

	private Projectile_RL m_warningProjectile;

	private IHitboxController m_hbController;

	private ProximityMineState m_state;

	private WaitRL_Yield m_waitYield;

	[NonSerialized]
	private string[] m_projectileNameArray;

	public string[] ProjectileNameArray
	{
		get
		{
			if (m_projectileNameArray == null)
			{
				m_projectileNameArray = new string[2] { "ProximityMineExplosionProjectile", "ProximityMineWarningProjectile" };
			}
			return m_projectileNameArray;
		}
	}

	protected override void Awake()
	{
		base.Awake();
		m_waitYield = new WaitRL_Yield(0f);
		m_hbController = GetComponentInChildren<IHitboxController>();
	}

	public override void Initialize(HazardArgs hazardArgs)
	{
		base.Initialize(hazardArgs);
		if (hazardArgs is PointHazardArgs pointHazardArgs)
		{
			m_explosionRadius = pointHazardArgs.Radius * 2f;
		}
		else
		{
			Debug.LogFormat("<color=red>| {0} | Failed to cast hazardArgs as PointHazardArgs. If you see this message please bug it on Pivotal.</color>");
		}
		(m_hbController.GetCollider(HitboxType.Terrain) as CircleCollider2D).radius = 5f;
		ResetHazard();
	}

	private IEnumerator FireProjectile()
	{
		m_state = ProximityMineState.Activated;
		base.Animator.SetBool("Active", value: true);
		m_triggerAudioEventEmitter.Play();
		m_warningProjectile = ProjectileManager.FireProjectile(base.gameObject, "ProximityMineWarningProjectile", Vector2.zero, matchFacing: false, 0f, 0f);
		m_warningProjectile.transform.localScale = new Vector3(m_explosionRadius, m_explosionRadius, m_warningProjectile.transform.localScale.z);
		m_waitYield.CreateNew(2.25f);
		yield return m_waitYield;
		m_warningProjectile.FlagForDestruction();
		m_warningProjectile = null;
		Projectile_RL projectile_RL = ProjectileManager.FireProjectile(base.gameObject, "ProximityMineExplosionProjectile", Vector2.zero, matchFacing: false, 0f, 0f);
		projectile_RL.transform.localScale = new Vector3(m_explosionRadius, m_explosionRadius, projectile_RL.transform.localScale.z);
		base.Animator.SetBool("Attacking", value: true);
		m_explodeAudioEventEmitter.Play();
		m_waitYield.CreateNew(projectile_RL.Lifespan);
		yield return m_waitYield;
		m_state = ProximityMineState.CoolingDown;
		base.Animator.SetBool("Attacking", value: false);
		m_waitYield.CreateNew(7.5f);
		yield return m_waitYield;
		ResetHazard();
	}

	public void TerrainOnEnterHitResponse(IHitboxController otherHBController)
	{
		if (m_state == ProximityMineState.Idle)
		{
			StartCoroutine(FireProjectile());
		}
	}

	public override void ResetHazard()
	{
		m_state = ProximityMineState.Idle;
		base.Animator.SetBool("Active", value: false);
		base.Animator.SetBool("Attacking", value: false);
		if ((bool)m_warningProjectile && m_warningProjectile.isActiveAndEnabled)
		{
			m_warningProjectile.gameObject.SetActive(value: false);
		}
		m_warningProjectile = null;
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		if ((bool)m_warningProjectile && m_warningProjectile.isActiveAndEnabled)
		{
			m_warningProjectile.gameObject.SetActive(value: false);
		}
		m_warningProjectile = null;
		if (!GameManager.IsApplicationClosing && (bool)base.Animator)
		{
			base.Animator.WriteDefaultValues();
		}
	}

	[SpecialName]
	GameObject IRootObj.get_gameObject()
	{
		return base.gameObject;
	}
}
