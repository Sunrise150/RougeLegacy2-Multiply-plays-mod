using UnityEngine;

public class PointHazardSpawnController : HazardSpawnControllerBase
{
	[SerializeField]
	[Range(0f, 100f)]
	private float m_radius = 9f;

	[SerializeField]
	private bool m_overrideRotationSpeed;

	[SerializeField]
	private float m_rotationSpeedOverride = 1f;

	[SerializeField]
	private bool m_overrideExpansionDuration;

	[SerializeField]
	private float m_expansionDurationOverride = 1f;

	public override HazardCategory Category => HazardCategory.Point;

	public float Radius
	{
		get
		{
			return m_radius;
		}
		set
		{
			if (!Application.isPlaying)
			{
				m_radius = value;
			}
		}
	}

	public bool OverrideExpansionDuration => m_overrideExpansionDuration;

	public bool OverrideRotationSpeed => m_overrideRotationSpeed;

	protected override void Reset()
	{
		if (base.Hazard != null && base.Hazard.gameObject != null)
		{
			base.Hazard.gameObject.SetActive(value: false);
		}
		base.Hazard = null;
	}

	protected override void Spawn()
	{
		if (base.Type == HazardType.None)
		{
			return;
		}
		CreateHazard();
		if (m_hazardArgs == null)
		{
			float num = Radius;
			if (Hazard_EV.POINT_HAZARD_RADIUS_MULTIPLIER.ContainsKey(base.Room.AppearanceBiomeType))
			{
				if (Hazard_EV.POINT_HAZARD_RADIUS_MULTIPLIER[base.Room.AppearanceBiomeType] > 0f)
				{
					num *= Hazard_EV.POINT_HAZARD_RADIUS_MULTIPLIER[base.Room.AppearanceBiomeType];
				}
				else
				{
					Debug.LogFormat("<color=red>| {0} | The {1} Biome's entry in Hazard_EV's POINT_HAZARD_RADIUS_MULTIPLIER table has an invalid value. It must be greater than 0.</color>", this, base.Room.AppearanceBiomeType);
				}
			}
			if (base.transform.localScale.x != 1f && base.transform.localScale.x != 0f)
			{
				num *= 1f / base.transform.localScale.x;
			}
			float rotationSpeed = 75f;
			if (OverrideRotationSpeed)
			{
				rotationSpeed = m_rotationSpeedOverride;
			}
			else if (base.Type != HazardType.Windmill)
			{
				rotationSpeed = 100f;
			}
			float expansionDuration = 0f;
			if (base.Type != HazardType.Windmill)
			{
				expansionDuration = 2.5f;
				if (OverrideExpansionDuration)
				{
					expansionDuration = m_expansionDurationOverride;
				}
			}
			m_hazardArgs = new PointHazardArgs(base.InitialState, num, rotationSpeed, expansionDuration);
		}
		base.Hazard.Initialize(m_hazardArgs);
	}
}
