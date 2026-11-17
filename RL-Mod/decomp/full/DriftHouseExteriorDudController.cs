using UnityEngine;

public class DriftHouseExteriorDudController : BaseSpecialPropController
{
	[SerializeField]
	private GameObject m_closedShack;

	[SerializeField]
	private GameObject m_openShack;

	protected override void InitializePooledPropOnEnter()
	{
	}
}
