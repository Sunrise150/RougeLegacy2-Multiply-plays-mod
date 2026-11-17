using System.Collections;
using UnityEngine;

public class Conveyor_Hazard : SingleLine_Multi_Hazard, ITerrainOnStayHitResponse, IHitResponse
{
	[SerializeField]
	private SpriteRenderer m_lineSprite;

	private bool m_moveRight = true;

	private IHitboxController m_hbController;

	protected override void Awake()
	{
		base.Awake();
		m_hbController = GetComponentInChildren<IHitboxController>();
	}

	public override void Initialize(PivotPoint pivot, int width, HazardArgs hazardArgs)
	{
		base.Initialize(pivot, width, hazardArgs);
		switch (hazardArgs.InitialState)
		{
		case StateID.One:
			m_moveRight = true;
			break;
		case StateID.Two:
			m_moveRight = false;
			break;
		case StateID.Random:
			if (CDGHelper.RandomPlusMinus() > 0)
			{
				m_moveRight = true;
			}
			else
			{
				m_moveRight = false;
			}
			break;
		}
		StartCoroutine(SetConveyorWidth(width));
	}

	private IEnumerator SetConveyorWidth(int width)
	{
		while (!m_hbController.IsInitialized)
		{
			yield return null;
		}
		m_hbController.RepeatHitDuration = 0f;
		BoxCollider2D obj = m_hbController.GetCollider(HitboxType.Terrain) as BoxCollider2D;
		Vector2 size = obj.size;
		size.x = width;
		obj.size = size;
		m_lineSprite.size = new Vector2(width, m_lineSprite.size.y);
	}

	public void TerrainOnStayHitResponse(IHitboxController otherHBController)
	{
		if (otherHBController == null || !otherHBController.RootGameObject.CompareTag("Player"))
		{
			return;
		}
		CorgiController_RL component = otherHBController.RootGameObject.GetComponent<CorgiController_RL>();
		if (component != null && component.State.IsGrounded)
		{
			Vector3 localPosition = component.gameObject.transform.localPosition;
			if (m_moveRight)
			{
				localPosition.x += 10f * Time.deltaTime;
			}
			else
			{
				localPosition.x -= 10f * Time.deltaTime;
			}
			component.gameObject.transform.localPosition = localPosition;
		}
	}

	public override void ResetHazard()
	{
	}
}
