using System;
using System.Collections;
using System.Runtime.CompilerServices;
using RLAudio;
using UnityEngine;

public class ProximityProjectile_Hazard : Hazard, IPointHazard, IHazard, IRootObj, IHasProjectileNameArray, ITerrainOnEnterHitResponse, IHitResponse
{
	private enum ProximityMineState
	{
		Idle,
		Activated,
		CoolingDown
	}

	private const string PROJECTILE_NAME = "ProximityDashBoltProjectile";

	private const string SFX_TRIGGER_NAME = "event:/SFX/Enemies/sfx_hazard_voidMushroom_trigger";

	private const string SFX_EXPLODE_NAME = "event:/SFX/Enemies/sfx_hazard_voidMushroom_explode";

	private ProximityMineState m_state;

	private WaitRL_Yield m_waitYield;

	private IHitboxController m_hbController;

	[NonSerialized]
	private string[] m_projectileNameArray;

	public string[] ProjectileNameArray
	{
		get
		{
			if (m_projectileNameArray == null)
			{
				m_projectileNameArray = new string[1] { "ProximityDashBoltProjectile" };
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
		(m_hbController.GetCollider(HitboxType.Terrain) as CircleCollider2D).radius = 5f;
		ResetHazard();
	}

	private IEnumerator FireProjectile()
	{
		m_state = ProximityMineState.Activated;
		base.Animator.SetBool("Active", value: true);
		AudioManager.PlayOneShot(null, "event:/SFX/Enemies/sfx_hazard_voidMushroom_trigger", base.transform.position);
		m_waitYield.CreateNew(1.25f);
		yield return m_waitYield;
		float num = 0f;
		if (PlayerManager.IsInstantiated && !PlayerManager.IsDisposed)
		{
			num = CDGHelper.AngleBetweenPts(base.transform.position, PlayerManager.GetPlayerController().Midpoint);
		}
		AudioManager.PlayOneShot(null, "event:/SFX/Enemies/sfx_hazard_voidMushroom_explode", base.transform.position);
		for (int i = 0; i < 360; i += 36)
		{
			ProjectileManager.FireProjectile(base.gameObject, "ProximityDashBoltProjectile", Vector2.zero, matchFacing: false, num + (float)i);
		}
		base.Animator.SetBool("Attacking", value: true);
		m_waitYield.CreateNew(0.1f);
		yield return m_waitYield;
		m_state = ProximityMineState.CoolingDown;
		base.Animator.SetBool("Attacking", value: false);
		m_waitYield.CreateNew(8.5f);
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
	}

	[SpecialName]
	GameObject IRootObj.get_gameObject()
	{
		return base.gameObject;
	}
}
