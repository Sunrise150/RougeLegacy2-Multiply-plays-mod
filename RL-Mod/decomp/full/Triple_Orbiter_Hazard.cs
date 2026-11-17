using System.Runtime.CompilerServices;
using UnityEngine;

public class Triple_Orbiter_Hazard : Hazard, IPointHazard, IHazard, IRootObj
{
	public const int NUM_ORBITERS = 3;

	private const float SPEED_MOD = 0.75f;

	private static readonly float[] m_angleOffsets = new float[3] { 0f, 120f, 240f };

	private IHazard[] m_orbiters = new IHazard[3];

	public override void Initialize(HazardArgs hazardArgs)
	{
		PointHazardArgs pointHazardArgs = hazardArgs as PointHazardArgs;
		StateID stateID = hazardArgs.InitialState;
		if (stateID == StateID.Random)
		{
			stateID = ((CDGHelper.RandomPlusMinus() <= 0) ? StateID.Two : StateID.One);
		}
		PointHazardArgs hazardArgs2 = new PointHazardArgs(stateID, pointHazardArgs.Radius, pointHazardArgs.RotationSpeed * 0.75f, pointHazardArgs.ExpansionDuration);
		for (int i = 0; i < m_orbiters.Length; i++)
		{
			m_orbiters[i] = HazardManager.GetHazard(HazardType.Orbiter);
			m_orbiters[i].gameObject.SetActive(value: true);
			m_orbiters[i].gameObject.transform.SetPositionAndRotation(base.transform.position, base.transform.rotation);
			m_orbiters[i].gameObject.transform.Rotate(0f, 0f, m_angleOffsets[i]);
			m_orbiters[i].Initialize(hazardArgs2);
			m_orbiters[i].ResetHazard();
			(m_orbiters[i] as Hazard).SetRoom(base.Room);
		}
	}

	public override void ResetHazard()
	{
		for (int i = 0; i < m_orbiters.Length; i++)
		{
			m_orbiters[i].ResetHazard();
		}
	}

	protected override void OnDisable()
	{
		for (int i = 0; i < m_orbiters.Length; i++)
		{
			if (m_orbiters[i] != null && (bool)m_orbiters[i].gameObject)
			{
				m_orbiters[i].gameObject.SetActive(value: false);
			}
		}
		base.OnDisable();
	}

	[SpecialName]
	GameObject IRootObj.get_gameObject()
	{
		return base.gameObject;
	}
}
