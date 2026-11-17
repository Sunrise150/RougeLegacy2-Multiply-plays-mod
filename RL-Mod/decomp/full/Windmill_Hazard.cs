using System.Runtime.CompilerServices;
using UnityEngine;

public class Windmill_Hazard : Hazard, IPointHazard, IHazard, IRootObj
{
	[SerializeField]
	private GameObject m_bladeTip;

	private bool m_rotateClockwise = true;

	private float m_rotationSpeed;

	private bool m_colorTrailApplied;

	public float CurrentRotation { get; private set; }

	private void OnEnable()
	{
		if (TraitManager.IsTraitActive(TraitType.ColorTrails))
		{
			ApplyColorTrails();
		}
	}

	private void ApplyColorTrails()
	{
		if (!m_colorTrailApplied)
		{
			Debug.Log("Applying color trails to windmill");
			BaseEffect baseEffect = EffectManager.PlayEffect(m_bladeTip, null, "ColorTrails_Trait_Effect", m_bladeTip.transform.position, 0f, EffectStopType.Gracefully);
			TrailRenderer componentInChildren = baseEffect.GetComponentInChildren<TrailRenderer>();
			componentInChildren.startColor = Random.ColorHSV(0f, 1f, 1f, 1f, 0.5f, 1f);
			componentInChildren.endColor = Random.ColorHSV(0f, 1f, 1f, 1f, 0.5f, 1f);
			componentInChildren.widthMultiplier = 1.5f;
			componentInChildren.time = 18f;
			baseEffect.transform.SetParent(m_bladeTip.gameObject.transform, worldPositionStays: true);
			m_colorTrailApplied = true;
		}
	}

	public override void Initialize(HazardArgs hazardArgs)
	{
		base.InitialState = hazardArgs.InitialState;
		switch (base.InitialState)
		{
		case StateID.One:
			m_rotateClockwise = true;
			break;
		case StateID.Two:
			m_rotateClockwise = false;
			break;
		case StateID.Random:
			if (CDGHelper.RandomPlusMinus() > 0)
			{
				m_rotateClockwise = true;
			}
			else
			{
				m_rotateClockwise = false;
			}
			break;
		}
		if (hazardArgs is PointHazardArgs pointHazardArgs)
		{
			m_rotationSpeed = pointHazardArgs.RotationSpeed;
		}
		else
		{
			Debug.LogFormat("<color=red>| {0} | Failed to cast hazardArgs as PointHazardArgs. If you see this message please bug it on Pivotal.</color>");
		}
	}

	private void Update()
	{
		if (m_rotateClockwise)
		{
			CurrentRotation -= m_rotationSpeed * Time.deltaTime;
		}
		else
		{
			CurrentRotation += m_rotationSpeed * Time.deltaTime;
		}
		CurrentRotation = CDGHelper.WrapAngleDegrees(CurrentRotation, convertToHalfRadius: false);
		Vector3 localEulerAngles = m_pivot.transform.localEulerAngles;
		localEulerAngles.z = CurrentRotation;
		m_pivot.transform.localEulerAngles = localEulerAngles;
	}

	public override void ResetHazard()
	{
		CurrentRotation = 0f;
		m_pivot.transform.localEulerAngles = Vector3.zero;
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		m_colorTrailApplied = false;
	}

	[SpecialName]
	GameObject IRootObj.get_gameObject()
	{
		return base.gameObject;
	}
}
