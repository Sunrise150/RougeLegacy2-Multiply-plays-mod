using System.Collections;
using UnityEngine;

public class PressurePlate_HazardTrigger : Multi_Hazard, ITerrainOnStayHitResponse, IHitResponse, ITerrainOnExitHitResponse
{
	[SerializeField]
	private GameObject m_visuals;

	[SerializeField]
	private SpriteRenderer m_triggerSprite;

	private WaitRL_Yield m_waitYield;

	private bool m_plateTriggered;

	private bool m_isShooting;

	private BoxCollider2D m_triggerCollider;

	private IHitboxController m_hbController;

	private Vector3 m_spawnPosition;

	protected override void Awake()
	{
		base.Awake();
		m_waitYield = new WaitRL_Yield(0f);
		m_hbController = GetComponentInChildren<IHitboxController>();
	}

	private IEnumerator InitializeHitboxes(float width)
	{
		while (!m_hbController.IsInitialized)
		{
			yield return null;
		}
		m_hbController.RepeatHitDuration = 0f;
		m_triggerCollider = m_hbController.GetCollider(HitboxType.Terrain) as BoxCollider2D;
		m_triggerCollider.size = new Vector2(width, m_triggerCollider.size.y);
		m_triggerCollider.gameObject.transform.position = m_spawnPosition;
	}

	public override void Initialize(PivotPoint pivot, int width, HazardArgs hazardArgs)
	{
		base.Initialize(pivot, width, hazardArgs);
		m_spawnPosition = base.transform.position;
		switch (pivot)
		{
		case PivotPoint.Left:
			m_spawnPosition += 0.5f * (float)width * base.transform.right;
			break;
		case PivotPoint.Right:
			m_spawnPosition -= 0.5f * (float)width * base.transform.right;
			break;
		}
		m_visuals.transform.position = m_spawnPosition;
		m_triggerSprite.size = new Vector2(width, m_triggerSprite.size.y);
		if (m_triggerCollider == null)
		{
			StartCoroutine(InitializeHitboxes(width));
			return;
		}
		m_triggerCollider.size = new Vector2(width, m_triggerCollider.size.y);
		m_triggerCollider.gameObject.transform.position = m_spawnPosition;
	}

	public void TerrainOnStayHitResponse(IHitboxController otherHBController)
	{
		if (m_plateTriggered || !PlayerManager.GetPlayerController().IsGrounded || m_isShooting)
		{
			return;
		}
		if (m_hazards != null)
		{
			foreach (PressurePlate_Hazard hazard in m_hazards)
			{
				hazard.Animator.SetTrigger("ButtonDown");
			}
		}
		m_plateTriggered = true;
	}

	public void TerrainOnExitHitResponse(IHitboxController otherHBController)
	{
		if (m_plateTriggered && !m_isShooting)
		{
			StopAllCoroutines();
			if (base.gameObject.activeInHierarchy)
			{
				StartCoroutine(TriggerPressurePlate());
			}
		}
		m_plateTriggered = false;
	}

	private IEnumerator TriggerPressurePlate()
	{
		m_isShooting = true;
		foreach (PressurePlate_Hazard hazard in m_hazards)
		{
			hazard.Animator.SetTrigger("Open");
		}
		m_waitYield.CreateNew(0.2f);
		yield return m_waitYield;
		foreach (PressurePlate_Hazard hazard2 in m_hazards)
		{
			hazard2.Shoot();
		}
		m_waitYield.CreateNew(0.6f);
		yield return m_waitYield;
		foreach (PressurePlate_Hazard hazard3 in m_hazards)
		{
			hazard3.StopShooting();
		}
		m_waitYield.CreateNew(0.25f);
		yield return m_waitYield;
		foreach (PressurePlate_Hazard hazard4 in m_hazards)
		{
			hazard4.Animator.SetTrigger("Close");
		}
		m_isShooting = false;
	}

	public override void ResetHazard()
	{
		if (m_hazards != null && m_isShooting)
		{
			foreach (PressurePlate_Hazard hazard in m_hazards)
			{
				hazard.StopShooting();
				hazard.Animator.ResetTrigger("Close");
				hazard.Animator.ResetTrigger("Open");
				hazard.Animator.ResetTrigger("ButtonDown");
			}
		}
		m_plateTriggered = false;
		m_isShooting = false;
		base.ResetHazard();
	}
}
