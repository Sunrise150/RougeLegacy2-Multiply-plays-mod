using System;
using System.Collections;
using UnityEngine;

public class RaycastTurret_Hazard : Hazard, IHasProjectileNameArray, IMultiHazardConsumer
{
	[SerializeField]
	private string m_projectileName;

	private const int SHOT_TRIGGER_WIDTH = 2;

	private float m_shotDelayTimer;

	private int m_triggerShotMask;

	private Coroutine m_initialDelayCoroutine;

	private WaitForSeconds m_initialDelay;

	private float m_onEnterTime;

	private bool m_needsReloading;

	[NonSerialized]
	private string[] m_projectileNameArray;

	public string[] ProjectileNameArray
	{
		get
		{
			if (m_projectileNameArray == null)
			{
				m_projectileNameArray = new string[1] { m_projectileName };
			}
			return m_projectileNameArray;
		}
	}

	protected virtual Vector2 FireOffset => Vector2.zero;

	protected virtual bool AllTurretsFire => true;

	protected virtual float InitializationDelay => 1f;

	protected virtual float FireDelay => 0.5f;

	protected virtual float DelayBetweenShots => 4f;

	protected virtual float DetectedRange => 25f;

	public Multi_Hazard MultiHazard { get; set; }

	private bool IsShotWithinRange
	{
		get
		{
			PlayerController playerController = PlayerManager.GetPlayerController();
			float z = base.transform.localEulerAngles.z;
			z = (int)CDGHelper.WrapAngleDegrees(z, convertToHalfRadius: true);
			Vector2 thePoint = playerController.Midpoint;
			thePoint = CDGHelper.RotatedPoint(thePoint, base.transform.position, 90f - z);
			return new Rect(base.transform.position.x - 1f, base.transform.position.y, 2f, DetectedRange).Contains(thePoint);
		}
	}

	protected override void Awake()
	{
		base.Awake();
		m_triggerShotMask = 256;
		m_initialDelay = new WaitForSeconds(FireDelay);
	}

	private void OnEnable()
	{
		m_onEnterTime = Time.time;
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		m_shotDelayTimer = 0f;
	}

	private void FixedUpdate()
	{
		if (Time.time < m_onEnterTime + InitializationDelay)
		{
			return;
		}
		if (m_shotDelayTimer <= 0f)
		{
			if (!PlayerManager.IsInstantiated)
			{
				return;
			}
			if (m_needsReloading)
			{
				base.Animator.SetTrigger("Reload");
				m_needsReloading = false;
			}
			PlayerController playerController = PlayerManager.GetPlayerController();
			float num = 0.5f;
			float z = base.transform.localEulerAngles.z;
			Vector3 position = base.transform.position;
			position.y += num;
			position = CDGHelper.RotatedPoint(position, base.transform.position, z - 90f);
			position.z = base.transform.position.z;
			if (!IsShotWithinRange)
			{
				return;
			}
			Vector2 direction = CDGHelper.VectorBetweenPts(position, playerController.Midpoint);
			float magnitude = direction.magnitude;
			direction.Normalize();
			if ((bool)Physics2D.Raycast(position, direction, magnitude, m_triggerShotMask))
			{
				return;
			}
			if (AllTurretsFire)
			{
				if (!MultiHazard || MultiHazard.Hazards == null)
				{
					return;
				}
				{
					foreach (IHazard hazard in MultiHazard.Hazards)
					{
						(hazard as RaycastTurret_Hazard).FireRaycastArrow();
					}
					return;
				}
			}
			FireRaycastArrow();
		}
		else
		{
			m_shotDelayTimer -= Time.deltaTime;
		}
	}

	public void FireRaycastArrow()
	{
		if (m_initialDelayCoroutine == null)
		{
			m_initialDelayCoroutine = StartCoroutine(FireAfterInitialDelay());
		}
	}

	private IEnumerator FireAfterInitialDelay()
	{
		base.Animator.SetTrigger("Tell");
		yield return m_initialDelay;
		base.Animator.SetTrigger("Fire");
		m_needsReloading = true;
		ProjectileManager.FireProjectile(base.gameObject, m_projectileName, FireOffset, matchFacing: false, base.transform.localEulerAngles.z);
		m_shotDelayTimer = DelayBetweenShots;
		m_initialDelayCoroutine = null;
	}

	public override void ResetHazard()
	{
		m_initialDelayCoroutine = null;
		m_needsReloading = false;
	}
}
