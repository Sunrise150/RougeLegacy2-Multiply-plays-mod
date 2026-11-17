using System;
using System.Collections;
using MoreMountains.CorgiEngine;
using UnityEngine;

public class Scythe_Ability : BaseAbility_RL, IAttack, IAbility
{
	[SerializeField]
	private string m_secondProjectileName;

	[Space(10f)]
	[SerializeField]
	private ParticleSystem m_scytheParticlePartSysPrefab;

	private float m_storedFallMultiplier;

	private bool m_performedFirstAttack;

	private bool m_continueDashing;

	private bool m_partSysAdded;

	private Transform m_weaponTransform;

	private ParticleSystem m_scythePartSys;

	private Action<MonoBehaviour, EventArgs> m_stopScythePartSystem;

	private bool m_oneWayCollisionDisabled;

	public override float AirMovementMod => 0f;

	public override float MovementMod => 0f;

	public override string ProjectileName
	{
		get
		{
			if (!m_performedFirstAttack)
			{
				return base.ProjectileName;
			}
			return m_secondProjectileName;
		}
	}

	protected override float TellIntroAnimSpeed
	{
		get
		{
			if (m_performedFirstAttack)
			{
				return 0.8f;
			}
			return 0.8f;
		}
	}

	protected override float TellIntroAnimExitDelay
	{
		get
		{
			if (m_performedFirstAttack)
			{
				return 0f;
			}
			return 0f;
		}
	}

	protected override float TellAnimSpeed
	{
		get
		{
			if (m_performedFirstAttack)
			{
				return 1f;
			}
			return 1f;
		}
	}

	protected override float TellAnimExitDelay
	{
		get
		{
			if (m_performedFirstAttack)
			{
				return 0f;
			}
			return 0f;
		}
	}

	protected override float AttackIntroAnimSpeed
	{
		get
		{
			if (m_performedFirstAttack)
			{
				return 1f;
			}
			return 1f;
		}
	}

	protected override float AttackIntroAnimExitDelay
	{
		get
		{
			if (m_performedFirstAttack)
			{
				return 0f;
			}
			return 0f;
		}
	}

	protected override float AttackAnimSpeed
	{
		get
		{
			if (m_performedFirstAttack)
			{
				return 1f;
			}
			return 1f;
		}
	}

	protected override float AttackAnimExitDelay
	{
		get
		{
			if (m_performedFirstAttack)
			{
				return 0f;
			}
			return 0.075f;
		}
	}

	protected override float ExitAnimSpeed
	{
		get
		{
			if (m_performedFirstAttack)
			{
				return 1f;
			}
			return 1f;
		}
	}

	protected override float ExitAnimExitDelay
	{
		get
		{
			if (m_performedFirstAttack)
			{
				return 0f;
			}
			return 0f;
		}
	}

	protected override void InitializeProjectileNameArray()
	{
		m_projectileNameArray = new string[2] { m_projectileName, m_secondProjectileName };
	}

	protected override void Awake()
	{
		base.Awake();
		m_stopScythePartSystem = StopScythePartSystem;
	}

	public override void Initialize(CastAbility_RL abilityController, CastAbilityType castAbilityType)
	{
		base.Initialize(abilityController, castAbilityType);
		if (!m_partSysAdded)
		{
			m_weaponTransform = abilityController.PlayerController.Visuals.transform.FindDeep("weapon_l");
			if ((bool)m_weaponTransform)
			{
				m_scythePartSys = UnityEngine.Object.Instantiate(m_scytheParticlePartSysPrefab);
				m_scythePartSys.transform.position = m_weaponTransform.transform.position;
				m_scythePartSys.transform.SetParent(m_weaponTransform, worldPositionStays: false);
				m_scythePartSys.Clear(withChildren: true);
				m_scythePartSys.transform.localScale = new Vector3(1f, 1f, 1f);
				m_scythePartSys.transform.localPosition = Vector3.zero;
				m_partSysAdded = true;
				Messenger<GameMessenger, GameEvent>.AddListener(GameEvent.PlayerDeath, m_stopScythePartSystem);
				Messenger<GameMessenger, GameEvent>.AddListener(GameEvent.PlayerEnterParade, m_stopScythePartSystem);
			}
		}
	}

	public override void OnPreDestroy()
	{
		if (m_partSysAdded)
		{
			m_scythePartSys.transform.SetParent(null);
		}
		base.OnPreDestroy();
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		if (m_partSysAdded)
		{
			UnityEngine.Object.Destroy(m_scythePartSys.gameObject);
			m_partSysAdded = false;
			Messenger<GameMessenger, GameEvent>.RemoveListener(GameEvent.PlayerDeath, m_stopScythePartSystem);
			Messenger<GameMessenger, GameEvent>.RemoveListener(GameEvent.PlayerEnterParade, m_stopScythePartSystem);
		}
		m_weaponTransform = null;
	}

	private void StopScythePartSystem(object sender, EventArgs args)
	{
		if (m_partSysAdded)
		{
			m_scythePartSys.Stop(withChildren: false, ParticleSystemStopBehavior.StopEmittingAndClear);
		}
	}

	public override void PreCastAbility()
	{
		base.PreCastAbility();
		m_storedFallMultiplier = m_abilityController.PlayerController.FallMultiplierOverride;
		m_abilityController.PlayerController.CharacterJump.ResetBrakeForce();
		m_abilityController.PlayerController.SetVelocity(0f, 0f, additive: false);
		m_abilityController.PlayerController.FallMultiplierOverride = 0f;
		m_abilityController.PlayerController.ControllerCorgi.StickWhenWalkingDownSlopes = false;
		m_performedFirstAttack = false;
		m_continueDashing = false;
	}

	protected override IEnumerator ChangeAnim(float duration)
	{
		if (base.CurrentAbilityAnimState == AbilityAnimState.Attack && !m_performedFirstAttack)
		{
			StartCoroutine(PushForward());
			m_performedFirstAttack = true;
			yield return base.ChangeAnim(duration);
		}
		else
		{
			yield return base.ChangeAnim(duration);
		}
	}

	protected IEnumerator PushForward()
	{
		float speed = 22f;
		m_continueDashing = true;
		if (m_abilityController.PlayerController.IsFacingRight)
		{
			m_abilityController.PlayerController.SetVelocityX(speed, additive: false);
		}
		else
		{
			m_abilityController.PlayerController.SetVelocityX(0f - speed, additive: false);
		}
		if (m_abilityController.PlayerController.IsGrounded)
		{
			m_abilityController.PlayerController.MovementState = CharacterStates.MovementStates.Idle;
		}
		if (!m_abilityController.PlayerController.ControllerCorgi.DisableOneWayCollision)
		{
			m_abilityController.PlayerController.ControllerCorgi.DisableOneWayCollision = true;
			m_oneWayCollisionDisabled = true;
		}
		while (m_continueDashing)
		{
			float num = speed;
			m_abilityController.PlayerController.DisableFriction = true;
			m_abilityController.PlayerController.DisableDoorBlock = true;
			if (m_abilityController.PlayerController.IsFacingRight)
			{
				m_abilityController.PlayerController.SetVelocity(num, 0f, additive: false);
			}
			else
			{
				m_abilityController.PlayerController.SetVelocity(0f - num, 0f, additive: false);
			}
			if (m_abilityController.PlayerController.IsGrounded)
			{
				m_abilityController.PlayerController.ConditionState = CharacterStates.CharacterConditions.DisableHorizontalMovement;
			}
			else
			{
				m_abilityController.PlayerController.ConditionState = CharacterStates.CharacterConditions.Normal;
			}
			yield return null;
		}
		m_abilityController.PlayerController.DisableDoorBlock = false;
		m_abilityController.PlayerController.DisableFriction = false;
		if (m_oneWayCollisionDisabled)
		{
			m_abilityController.PlayerController.ControllerCorgi.DisableOneWayCollision = false;
			m_oneWayCollisionDisabled = false;
		}
	}

	public override void StopAbility(bool abilityInterrupted)
	{
		m_abilityController.PlayerController.DisableFriction = false;
		m_abilityController.PlayerController.DisableDoorBlock = false;
		if (m_oneWayCollisionDisabled)
		{
			m_abilityController.PlayerController.ControllerCorgi.DisableOneWayCollision = false;
			m_oneWayCollisionDisabled = false;
		}
		m_abilityController.PlayerController.ControllerCorgi.StickWhenWalkingDownSlopes = true;
		if (m_abilityController.PlayerController.ConditionState != CharacterStates.CharacterConditions.Stunned)
		{
			m_abilityController.PlayerController.ConditionState = CharacterStates.CharacterConditions.Normal;
		}
		if ((bool)m_firedProjectile)
		{
			m_firedProjectile.FlagForDestruction();
			m_firedProjectile = null;
		}
		m_abilityController.PlayerController.FallMultiplierOverride = m_storedFallMultiplier;
		m_continueDashing = false;
		StartCooldownTimer();
		base.StopAbility(abilityInterrupted);
	}

	protected override void OnEnterExitLogic()
	{
		m_continueDashing = false;
		base.OnEnterExitLogic();
	}

	protected override void Update()
	{
		base.Update();
		if (!m_partSysAdded || !m_weaponTransform || !m_weaponTransform.hasChanged)
		{
			return;
		}
		if (m_weaponTransform.localScale.x < 0.1f)
		{
			if (m_scythePartSys.isEmitting)
			{
				m_scythePartSys.Stop(withChildren: false, ParticleSystemStopBehavior.StopEmitting);
			}
		}
		else if (!m_scythePartSys.isPlaying)
		{
			m_scythePartSys.Play();
		}
	}
}
