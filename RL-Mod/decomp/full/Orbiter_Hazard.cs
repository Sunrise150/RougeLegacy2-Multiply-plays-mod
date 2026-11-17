using System.Collections;
using System.Runtime.CompilerServices;
using UnityEngine;

public class Orbiter_Hazard : Hazard, IPointHazard, IHazard, IRootObj
{
	[SerializeField]
	private SpriteRenderer m_orbiterChain;

	[SerializeField]
	private GameObject m_orbiterBall;

	[SerializeField]
	private SpriteRenderer m_orbiterRadius;

	private bool m_rotateClockwise = true;

	private float m_orbiterRotationSpeed = 100f;

	private float m_orbiterExpansionDuration = 2.5f;

	private Vector3 m_initialRotation;

	private Vector3 m_initialPosition;

	private float m_initialRadius;

	private float m_currentRotation;

	private float m_orbiterExpansionTimer;

	private float m_expansionPercent;

	private bool m_colorTrailApplied;

	private IHitboxController m_hbController;

	public override float BaseDamage => 30f;

	public float ExpansionPercent
	{
		get
		{
			return m_expansionPercent;
		}
		private set
		{
			m_expansionPercent = value;
		}
	}

	public float CurrentRotation
	{
		get
		{
			return m_currentRotation;
		}
		private set
		{
			m_currentRotation = value;
		}
	}

	public float InitialRotation => m_initialRotation.z;

	public GameObject OrbiterBall => m_orbiterBall;

	protected override void Awake()
	{
		base.Awake();
		m_hbController = GetComponentInChildren<IHitboxController>();
	}

	private void OnEnable()
	{
		if (!m_hbController.IsInitialized)
		{
			StartCoroutine(InitializeHBControllerCoroutine());
		}
		else
		{
			StartCoroutine(ActivateWeaponHitboxCoroutine());
		}
		ResetHazard();
		if (TraitManager.IsTraitActive(TraitType.ColorTrails))
		{
			ApplyColorTrails();
		}
	}

	private void ApplyColorTrails()
	{
		if (!m_colorTrailApplied)
		{
			Debug.Log("Applying color trails to orbiter");
			BaseEffect baseEffect = EffectManager.PlayEffect(m_orbiterBall, null, "ColorTrails_Trait_Effect", m_orbiterBall.transform.position, 0f, EffectStopType.Gracefully);
			TrailRenderer componentInChildren = baseEffect.GetComponentInChildren<TrailRenderer>();
			componentInChildren.startColor = Random.ColorHSV(0f, 1f, 1f, 1f, 0.5f, 1f);
			componentInChildren.endColor = Random.ColorHSV(0f, 1f, 1f, 1f, 0.5f, 1f);
			componentInChildren.widthMultiplier = 1.5f;
			componentInChildren.time = 18f;
			baseEffect.transform.SetParent(m_orbiterBall.gameObject.transform, worldPositionStays: true);
			m_colorTrailApplied = true;
		}
	}

	private IEnumerator InitializeHBControllerCoroutine()
	{
		while (!m_hbController.IsInitialized)
		{
			yield return null;
		}
		m_hbController.SetHitboxActiveState(HitboxType.Weapon, active: false);
		yield return ActivateWeaponHitboxCoroutine();
	}

	private IEnumerator ActivateWeaponHitboxCoroutine()
	{
		float startTime = Time.time;
		while (Time.time < startTime + 0.25f)
		{
			yield return null;
		}
		m_hbController.SetHitboxActiveState(HitboxType.Weapon, active: true);
	}

	private void Update()
	{
		if (m_rotateClockwise)
		{
			CurrentRotation -= m_orbiterRotationSpeed * Time.deltaTime;
		}
		else
		{
			CurrentRotation += m_orbiterRotationSpeed * Time.deltaTime;
		}
		CurrentRotation = CDGHelper.WrapAngleDegrees(CurrentRotation, convertToHalfRadius: false);
		Vector3 localEulerAngles = m_pivot.transform.localEulerAngles;
		localEulerAngles.z = CurrentRotation;
		m_pivot.transform.localEulerAngles = localEulerAngles;
		if (m_orbiterExpansionTimer > 0f)
		{
			m_orbiterExpansionTimer -= Time.deltaTime;
			if (m_orbiterExpansionTimer < 0f)
			{
				m_orbiterExpansionTimer = 0f;
			}
		}
		ExpansionPercent = 1f;
		if (m_orbiterExpansionDuration != 0f)
		{
			ExpansionPercent = Mathf.Clamp(1f - m_orbiterExpansionTimer / m_orbiterExpansionDuration, 0f, 1f);
			m_orbiterChain.size = new Vector2(m_orbiterChain.size.x, m_initialRadius * ExpansionPercent);
		}
		Vector3 localPosition = m_initialRadius * ExpansionPercent * Vector3.up;
		localPosition.z = m_initialPosition.z;
		OrbiterBall.transform.localPosition = localPosition;
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
			SetRadius(pointHazardArgs.Radius);
			SetRotationSpeed(pointHazardArgs.RotationSpeed);
			SetExpansionDuration(pointHazardArgs.ExpansionDuration);
		}
		else
		{
			Debug.LogFormat("<color=red>| {0} | Failed to cast hazardArgs as PointHazardArgs. If you see this message please bug it on Pivotal.</color>");
		}
	}

	public override void ResetHazard()
	{
		CurrentRotation = 0f;
		m_pivot.transform.localEulerAngles = m_initialRotation;
		m_orbiterExpansionTimer = m_orbiterExpansionDuration;
		Vector3 localPosition = m_initialRadius * 0f * Vector3.up;
		localPosition.z = m_initialPosition.z;
		OrbiterBall.transform.localPosition = localPosition;
		m_orbiterChain.size = new Vector2(m_orbiterChain.size.x, m_initialRadius * 0f);
		if (m_hbController.IsInitialized)
		{
			m_hbController.SetHitboxActiveState(HitboxType.Weapon, active: false);
		}
	}

	private void SetExpansionDuration(float expansionDuration)
	{
		m_orbiterExpansionTimer = expansionDuration;
		m_orbiterExpansionDuration = expansionDuration;
	}

	private void SetRadius(float radius)
	{
		m_initialPosition = OrbiterBall.transform.localPosition;
		m_initialRotation = m_pivot.transform.localEulerAngles;
		float num = m_orbiterRadius.bounds.size.x / 2f;
		float num2 = radius / num;
		m_orbiterRadius.gameObject.transform.localScale = new Vector3(num2, num2, num2);
		Color color = m_orbiterRadius.color;
		color.a = 0.35f;
		m_orbiterRadius.color = color;
		m_initialRadius = radius;
	}

	private void SetRotationSpeed(float rotationSpeed)
	{
		m_orbiterRotationSpeed = rotationSpeed;
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
