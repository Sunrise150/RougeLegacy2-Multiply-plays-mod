using System.Collections;
using MoreMountains.CorgiEngine;
using UnityEngine;
using UnityEngine.Events;

public class FlameBarrier_Ability : BaseAbility_RL, ISpell, IAbility, IPersistentAbility
{
	[SerializeField]
	[ReadOnlyOnPlay]
	private int m_numberOfFireballs = 8;

	[SerializeField]
	private float m_radius;

	[SerializeField]
	private float m_rotationSpeed = 360f;

	[SerializeField]
	private UnityEvent m_stopCastingEvent;

	private Projectile_RL[] m_projectileArray;

	private float m_rotationDiff;

	private GameObject m_carouselGO;

	private float m_ticRateStartTime;

	private sbyte m_rotationDirection;

	public bool IsPersistentActive { get; private set; }

	protected override float TellIntroAnimSpeed => 3f;

	protected override float TellIntroAnimExitDelay => 0f;

	protected override float TellAnimSpeed => 3f;

	protected override float TellAnimExitDelay => 0f;

	protected override float AttackIntroAnimSpeed => 2f;

	protected override float AttackIntroAnimExitDelay => 0f;

	protected override float AttackAnimSpeed => 2f;

	protected override float AttackAnimExitDelay => 0.25f;

	protected override float ExitAnimSpeed => 1f;

	protected override float ExitAnimExitDelay => 0f;

	protected override void Awake()
	{
		base.Awake();
		m_projectileArray = new Projectile_RL[m_numberOfFireballs];
		m_rotationDiff = 360f / (float)m_numberOfFireballs;
	}

	public override void Initialize(CastAbility_RL abilityController, CastAbilityType castAbilityType)
	{
		base.Initialize(abilityController, castAbilityType);
		m_carouselGO = new GameObject();
		m_carouselGO.name = "CarouselGO";
		m_carouselGO.transform.SetParent(m_abilityController.PlayerController.transform);
		ReinitializeCarousel();
	}

	public override void PreCastAbility()
	{
		ReinitializeCarousel();
		if (IsPersistentActive)
		{
			StopFlameBarrier();
			StopAbility(abilityInterrupted: true);
		}
		else
		{
			m_hasAnimation = false;
			IsPersistentActive = true;
			base.PreCastAbility();
		}
	}

	private void ReinitializeCarousel()
	{
		Vector3 vector = Vector2.zero;
		float num = m_abilityController.PlayerController.transform.lossyScale.x / m_abilityController.PlayerController.BaseScaleToOffsetWith;
		Vector2 vector2 = ProjectileOffset * num;
		vector.x += vector2.x;
		vector.y += vector2.y;
		m_carouselGO.transform.position = vector + m_abilityController.PlayerController.transform.localPosition;
		float num2 = 1f / m_abilityController.PlayerController.transform.lossyScale.x;
		m_carouselGO.transform.localScale = new Vector3(num2, num2, 1f);
	}

	public override IEnumerator CastAbility()
	{
		if (IsPersistentActive)
		{
			yield return base.CastAbility();
		}
	}

	protected override void OnEnterAttackLogic()
	{
		m_abilityController.BroadcastAbilityCastEvents(base.CastAbilityType);
		FireProjectile();
	}

	protected override void FireProjectile()
	{
		if (!IsPersistentActive || ProjectileName == null)
		{
			return;
		}
		Projectile_RL[] projectileArray = m_projectileArray;
		foreach (Projectile_RL projectile_RL in projectileArray)
		{
			if ((bool)projectile_RL)
			{
				projectile_RL.gameObject.SetActive(value: false);
				projectile_RL.transform.SetParent(null);
			}
		}
		Vector2 thePoint = new Vector2(m_radius, 0f);
		for (int j = 0; j < m_numberOfFireballs; j++)
		{
			float theRotation = (float)j * m_rotationDiff;
			Vector2 vector = CDGHelper.RotatedPoint(thePoint, Vector2.zero, theRotation);
			m_projectileArray[j] = ProjectileManager.FireProjectile(m_abilityController.gameObject, ProjectileName, vector, matchFacing: false);
			m_projectileArray[j].transform.SetParent(m_carouselGO.transform, worldPositionStays: false);
			m_projectileArray[j].transform.localPosition = vector;
			m_abilityController.InitializeProjectile(m_projectileArray[j]);
		}
		ApplyAbilityCosts();
		m_carouselGO.transform.eulerAngles = Vector3.zero;
		if (m_abilityController.PlayerController.IsFacingRight)
		{
			m_rotationDirection = -1;
		}
		else
		{
			m_rotationDirection = 1;
		}
	}

	protected override void ApplyAbilityCosts()
	{
		if ((base.MaxAmmo > 0 && base.CurrentAmmo <= 0) || (base.ActualCost > 0 && m_abilityController.PlayerController.CurrentManaAsInt <= 0))
		{
			StopAbility(abilityInterrupted: true);
			StopFlameBarrier();
		}
		else
		{
			m_ticRateStartTime = Time.time;
			base.ApplyAbilityCosts();
		}
	}

	private void StopFlameBarrier()
	{
		IsPersistentActive = false;
		m_hasAnimation = true;
		if (m_stopCastingEvent != null)
		{
			m_stopCastingEvent.Invoke();
		}
		Projectile_RL[] projectileArray = m_projectileArray;
		foreach (Projectile_RL projectile_RL in projectileArray)
		{
			if ((bool)projectile_RL && projectile_RL.isActiveAndEnabled)
			{
				projectile_RL.FlagForDestruction();
			}
		}
		StartCooldownTimer();
	}

	public void StopPersistentAbility()
	{
		StopFlameBarrier();
	}

	protected override void Update()
	{
		base.Update();
		if (!IsPersistentActive)
		{
			return;
		}
		if ((bool)m_abilityController && m_abilityController.PlayerController.ConditionState == CharacterStates.CharacterConditions.Stunned && Rewired_RL.Player.GetButtonDown(m_abilityController.GetAbilityInputString(base.CastAbilityType)))
		{
			StopPersistentAbility();
		}
		if ((bool)m_projectileArray[0] && m_projectileArray[0].isActiveAndEnabled)
		{
			Vector3 eulerAngles = m_carouselGO.transform.eulerAngles;
			eulerAngles.z += m_rotationSpeed * Time.deltaTime * (float)m_rotationDirection;
			m_carouselGO.transform.eulerAngles = eulerAngles;
			Projectile_RL[] projectileArray = m_projectileArray;
			for (int i = 0; i < projectileArray.Length; i++)
			{
				projectileArray[i].transform.eulerAngles = Vector3.zero;
			}
			if (Time.time >= m_ticRateStartTime + 0.5f)
			{
				ApplyAbilityCosts();
			}
		}
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		if (GameManager.IsApplicationClosing)
		{
			return;
		}
		bool flag = m_carouselGO;
		Projectile_RL[] projectileArray = m_projectileArray;
		foreach (Projectile_RL projectile_RL in projectileArray)
		{
			if ((bool)projectile_RL && flag && projectile_RL.transform.parent == m_carouselGO.transform)
			{
				projectile_RL.gameObject.SetActive(value: false);
				projectile_RL.transform.SetParent(null);
			}
		}
		if (flag)
		{
			Object.Destroy(m_carouselGO);
		}
	}
}
