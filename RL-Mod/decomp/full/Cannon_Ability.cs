using System.Collections;
using Rewired;
using UnityEngine;

public class Cannon_Ability : AimedAbilityFast_RL, IAttack, IAbility
{
	[Header("Shoot Values")]
	[SerializeField]
	private string m_shootTellIntroName;

	[SerializeField]
	protected AbilityData m_shootAbilityData;

	[SerializeField]
	protected string m_shootProjectileName;

	[SerializeField]
	protected Vector2 m_shootProjectileOffset;

	private float MeleeTellIntroAnimSpeed = 3f;

	private float MeleeTellIntroAnimExitDelay;

	private float MeleeTellAnimSpeed = 1.5f;

	private float MeleeTellAnimExitDelay;

	private float MeleeAttackIntroAnimSpeed = 1.35f;

	private float MeleeAttackIntroAnimExitDelay;

	private float MeleeAttackAnimSpeed = 1.25f;

	private float MeleeAttackAnimExitDelay;

	private float MeleeExitAnimSpeed = 1.25f;

	private float MeleeExitAnimExitDelay;

	private float ShootTellIntroAnimSpeed = 3f;

	private float ShootTellIntroAnimExitDelay;

	private float ShootTellAnimSpeed = 1.5f;

	private float ShootTellAnimExitDelay;

	private float ShootAttackIntroAnimSpeed = 1.25f;

	private float ShootAttackIntroAnimExitDelay;

	private float ShootAttackAnimSpeed = 1.25f;

	private float ShootAttackAnimExitDelay;

	private float ShootExitAnimSpeed = 1.25f;

	private float ShootExitAnimExitDelay;

	private bool m_shootButtonHeld;

	private bool m_shootButtonReleased;

	public bool IsShooting { get; private set; }

	public override Vector2 PushbackAmount
	{
		get
		{
			if (!IsShooting)
			{
				return Vector2.zero;
			}
			return base.PushbackAmount;
		}
	}

	protected override float GravityReduction => 1f;

	protected override float TellIntroAnimSpeed
	{
		get
		{
			if (IsShooting)
			{
				return ShootTellIntroAnimSpeed;
			}
			return MeleeTellIntroAnimSpeed;
		}
	}

	protected override float TellIntroAnimExitDelay
	{
		get
		{
			if (IsShooting)
			{
				return ShootTellIntroAnimExitDelay;
			}
			return MeleeTellIntroAnimExitDelay;
		}
	}

	protected override float TellAnimSpeed
	{
		get
		{
			if (IsShooting)
			{
				return ShootTellAnimSpeed;
			}
			return MeleeTellAnimSpeed;
		}
	}

	protected override float TellAnimExitDelay
	{
		get
		{
			if (IsShooting)
			{
				return ShootTellAnimExitDelay;
			}
			return MeleeTellAnimExitDelay;
		}
	}

	protected override float AttackIntroAnimSpeed
	{
		get
		{
			if (IsShooting)
			{
				return ShootAttackIntroAnimSpeed;
			}
			return MeleeAttackIntroAnimSpeed;
		}
	}

	protected override float AttackIntroAnimExitDelay
	{
		get
		{
			if (IsShooting)
			{
				return ShootAttackIntroAnimExitDelay;
			}
			return MeleeAttackIntroAnimExitDelay;
		}
	}

	protected override float AttackAnimSpeed
	{
		get
		{
			if (IsShooting)
			{
				return ShootAttackAnimSpeed;
			}
			return MeleeAttackAnimSpeed;
		}
	}

	protected override float AttackAnimExitDelay
	{
		get
		{
			if (IsShooting)
			{
				return ShootAttackAnimExitDelay;
			}
			return MeleeAttackAnimExitDelay;
		}
	}

	protected override float ExitAnimSpeed
	{
		get
		{
			if (IsShooting)
			{
				return ShootExitAnimSpeed;
			}
			return MeleeExitAnimSpeed;
		}
	}

	protected override float ExitAnimExitDelay
	{
		get
		{
			if (IsShooting)
			{
				return ShootExitAnimExitDelay;
			}
			return MeleeExitAnimExitDelay;
		}
	}

	public override bool HasAttackFlipCheck
	{
		get
		{
			if (IsShooting)
			{
				return false;
			}
			return base.HasAttackFlipCheck;
		}
	}

	public override string ProjectileName
	{
		get
		{
			if (IsShooting)
			{
				return m_shootProjectileName;
			}
			return base.ProjectileName;
		}
	}

	public override Vector2 ProjectileOffset
	{
		get
		{
			if (IsShooting)
			{
				return m_shootProjectileOffset;
			}
			return base.ProjectileOffset;
		}
	}

	public override AbilityData AbilityData
	{
		get
		{
			if (IsShooting)
			{
				return m_shootAbilityData;
			}
			return base.AbilityData;
		}
	}

	public override string AbilityTellIntroName
	{
		get
		{
			if (IsShooting)
			{
				return m_shootTellIntroName;
			}
			return base.AbilityTellIntroName;
		}
	}

	protected override void InitializeProjectileNameArray()
	{
		m_projectileNameArray = new string[2] { m_projectileName, m_shootProjectileName };
	}

	public override void Initialize(CastAbility_RL abilityController, CastAbilityType castAbilityType)
	{
		base.Initialize(abilityController, castAbilityType);
		abilityController.Animator.SetBool("FryingPan_UsePirateAudio", value: true);
	}

	protected override void InitializeAimLine()
	{
		Projectile_RL projectile = ProjectileLibrary.GetProjectile(m_shootProjectileName);
		if ((bool)projectile)
		{
			m_projFallMultiplier = projectile.FallMultiplierOverride;
			m_projGravityKickInDelay = projectile.GravityKickInDelay;
			m_projSpeed = projectile.ProjectileData.Speed;
			m_projIsWeighted = projectile.IsWeighted;
		}
		if ((bool)m_aimLine)
		{
			Color startColor = new Color(1f, 1f, 1f, 0.25f);
			m_aimLine.startColor = startColor;
			m_aimLine.gameObject.SetActive(value: false);
		}
	}

	public override void PreCastAbility()
	{
		IsShooting = m_shootButtonHeld;
		m_shootButtonReleased = false;
		base.PreCastAbility();
		if (!IsShooting)
		{
			m_aimLine.gameObject.SetActive(value: false);
		}
	}

	protected override void OnEnterAttackLogic()
	{
		m_abilityController.BroadcastAbilityCastEvents(base.CastAbilityType);
		FireProjectile();
	}

	protected override void FireProjectile()
	{
		base.FireProjectile();
		if (!IsShooting)
		{
			return;
		}
		Vector3 localPosition = m_firedProjectile.transform.localPosition;
		Vector3 vector = localPosition;
		Vector3 lossyScale = base.transform.lossyScale;
		vector.x += 1.1f * lossyScale.x;
		vector = CDGHelper.RotatedPoint(vector, localPosition, m_unmoddedAngle);
		vector.z = localPosition.z;
		EffectManager.PlayEffect(m_firedProjectile.gameObject, null, "CannonBallMuzzle_Effect", vector, 0f, EffectStopType.Gracefully).transform.localScale = lossyScale;
		StartCooldownTimer();
		if (!m_firedProjectile)
		{
			return;
		}
		GameObject gameObject = m_firedProjectile.gameObject.FindObjectReference("CriticalEffects");
		if ((bool)gameObject)
		{
			if (m_firedProjectile.ActualCritChance >= 100f)
			{
				gameObject.SetActive(value: true);
			}
			else
			{
				gameObject.SetActive(value: false);
			}
		}
	}

	protected override void UpdateArrowAim(bool doNotUpdatePlayerAnims = false)
	{
		if (IsShooting)
		{
			base.UpdateArrowAim(doNotUpdatePlayerAnims);
		}
	}

	protected override IEnumerator ChangeAnim(float duration)
	{
		if (!m_shootButtonHeld)
		{
			if (duration <= 0f)
			{
				yield return null;
			}
			while (duration > 0f)
			{
				duration -= Time.deltaTime;
				yield return null;
			}
			m_animator.SetTrigger("Change_Ability_Anim");
			PerformTurnAnimCheck();
		}
		else
		{
			yield return base.ChangeAnim(duration);
		}
	}

	public override void StopAbility(bool abilityInterrupted)
	{
		base.StopAbility(abilityInterrupted);
		if (IsShooting)
		{
			IsShooting = false;
		}
	}

	protected override void Update()
	{
		if (ReInput.isReady && !GameManager.IsGamePaused && !m_shootButtonReleased && !Rewired_RL.Player.GetButton(m_abilityController.GetAbilityInputString(base.CastAbilityType)))
		{
			m_shootButtonReleased = true;
			m_shootButtonHeld = false;
			StartCooldownTimer();
		}
		if (!base.AbilityActive)
		{
			base.Update();
			return;
		}
		if (m_shootButtonHeld)
		{
			base.Update();
			return;
		}
		if (!m_shootButtonReleased && !IsShooting && base.CurrentAbilityAnimState > AbilityAnimState.Attack)
		{
			m_shootButtonHeld = true;
			m_abilityController.StartAbility(base.CastAbilityType);
		}
		if (base.DecreaseCooldownOverTime && base.CooldownTimer > 0f)
		{
			m_cooldownTimer -= Time.deltaTime;
			if (base.CooldownTimer <= 0f)
			{
				EndCooldownTimer();
			}
		}
		if (base.LockoutTimer > 0f)
		{
			m_lockoutTimer -= Time.deltaTime;
		}
		if (base.AbilityActive)
		{
			CriticalHitTimingDisplay();
		}
	}
}
