using System;
using System.Collections;
using System.Runtime.CompilerServices;
using UnityEngine;

public class VoidTrap_Hazard : Hazard, IPointHazard, IHazard, IRootObj, IHasProjectileNameArray, ITerrainOnEnterHitResponse, IHitResponse
{
	private enum VoidTrapState
	{
		Idle,
		Activated,
		Attacking,
		CoolingDown
	}

	[SerializeField]
	private SpriteRenderer m_renderer;

	[SerializeField]
	private CircleCollider2D m_triggerCollider;

	private Projectile_RL[] m_projectileArray;

	private GameObject m_carouselGO;

	private float m_rotationDiff;

	private float m_ringRadius;

	private VoidTrapState m_state;

	private WaitRL_Yield m_waitYield;

	[NonSerialized]
	private string[] m_projectileNameArray;

	public string[] ProjectileNameArray
	{
		get
		{
			if (m_projectileNameArray == null)
			{
				m_projectileNameArray = new string[1] { "VoidTrapHazardProjectile" };
			}
			return m_projectileNameArray;
		}
	}

	protected override void Awake()
	{
		base.Awake();
		m_projectileArray = new Projectile_RL[8];
		m_rotationDiff = 45f;
		m_carouselGO = new GameObject();
		m_carouselGO.name = "CarouselGO";
		m_carouselGO.transform.SetParent(base.transform);
		Vector3 position = m_carouselGO.transform.position;
		position.x += Hazard_EV.VOID_TRAP_CAROUSEL_OFFSET.x;
		position.y += Hazard_EV.VOID_TRAP_CAROUSEL_OFFSET.y;
		m_carouselGO.transform.position = position;
		m_waitYield = new WaitRL_Yield(0f);
	}

	public override void Initialize(HazardArgs hazardArgs)
	{
		base.Initialize(hazardArgs);
		if (hazardArgs is PointHazardArgs pointHazardArgs)
		{
			m_ringRadius = pointHazardArgs.Radius * 1.17f;
		}
		else
		{
			Debug.LogFormat("<color=red>| {0} | Failed to cast hazardArgs as PointHazardArgs. If you see this message please bug it on Pivotal.</color>");
		}
		m_triggerCollider.radius = 3f;
		ResetHazard();
	}

	private IEnumerator FireProjectile()
	{
		m_state = VoidTrapState.Activated;
		m_renderer.color = Color.red;
		m_waitYield.CreateNew(0.25f);
		yield return m_waitYield;
		m_state = VoidTrapState.Attacking;
		Vector2 thePoint = new Vector2(m_ringRadius, 0f);
		for (int i = 0; i < 8; i++)
		{
			float theRotation = (float)i * m_rotationDiff;
			Vector2 vector = CDGHelper.RotatedPoint(thePoint, Vector2.zero, theRotation);
			m_projectileArray[i] = ProjectileManager.FireProjectile(base.gameObject, "VoidTrapHazardProjectile", vector, matchFacing: false);
			m_projectileArray[i].transform.SetParent(m_carouselGO.transform, worldPositionStays: false);
			m_projectileArray[i].transform.localPosition = vector;
		}
		m_waitYield.CreateNew(5f);
		yield return m_waitYield;
		m_state = VoidTrapState.CoolingDown;
		m_renderer.color = Color.blue;
		StopCarousel();
		m_waitYield.CreateNew(10f);
		yield return m_waitYield;
		ResetHazard();
	}

	private void FixedUpdate()
	{
		if (m_state == VoidTrapState.Attacking && m_projectileArray[0] != null && m_projectileArray[0].isActiveAndEnabled)
		{
			Vector3 eulerAngles = m_carouselGO.transform.eulerAngles;
			eulerAngles.z += 360f * Time.deltaTime;
			m_carouselGO.transform.eulerAngles = eulerAngles;
			Projectile_RL[] projectileArray = m_projectileArray;
			for (int i = 0; i < projectileArray.Length; i++)
			{
				projectileArray[i].transform.eulerAngles = Vector3.zero;
			}
		}
	}

	public void TerrainOnEnterHitResponse(IHitboxController otherHBController)
	{
		if (m_state == VoidTrapState.Idle)
		{
			StartCoroutine(FireProjectile());
		}
	}

	private void StopCarousel()
	{
		Projectile_RL[] projectileArray = m_projectileArray;
		foreach (Projectile_RL projectile_RL in projectileArray)
		{
			if (projectile_RL != null && projectile_RL.isActiveAndEnabled)
			{
				projectile_RL.FlagForDestruction();
			}
		}
	}

	public override void ResetHazard()
	{
		if (m_state == VoidTrapState.Attacking)
		{
			StopCarousel();
		}
		m_state = VoidTrapState.Idle;
		m_renderer.color = Color.white;
	}

	[SpecialName]
	GameObject IRootObj.get_gameObject()
	{
		return base.gameObject;
	}
}
