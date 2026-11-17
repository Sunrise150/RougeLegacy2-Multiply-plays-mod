using System;
using UnityEngine;

public class MagicWand_Ability : BaseAbility_RL, IAttack, IAbility
{
	[SerializeField]
	private GameObject m_aimIndicator;

	[Header("Strike Projectile")]
	[SerializeField]
	private string m_strikeProjectileName;

	[SerializeField]
	private Vector2 m_strikeProjectileOffset;

	private Projectile_RL m_strikeProjectile;

	private float m_aimIndicatorStartingScale;

	private float m_manaSpentForCrit;

	private Action<MonoBehaviour, EventArgs> m_onPlayerScaleChanged;

	private Action<MonoBehaviour, EventArgs> m_onPlayerManaChange;

	private Action<MonoBehaviour, EventArgs> m_onPlayerDeath;

	protected override float TellIntroAnimSpeed => 5f;

	protected override float TellIntroAnimExitDelay => 0f;

	protected override float TellAnimSpeed => 5f;

	protected override float TellAnimExitDelay => 0f;

	protected override float AttackIntroAnimSpeed => 5f;

	protected override float AttackIntroAnimExitDelay => 0.1f;

	protected override float AttackAnimSpeed => 1f;

	protected override float AttackAnimExitDelay => 0.175f;

	protected override float ExitAnimSpeed => 1f;

	protected override float ExitAnimExitDelay => 0f;

	protected virtual Vector2 ProjectileIndicatorPosition => new Vector2(7.2f, 1f);

	protected override void InitializeProjectileNameArray()
	{
		m_projectileNameArray = new string[2] { m_projectileName, m_strikeProjectileName };
	}

	protected override void Awake()
	{
		m_onPlayerScaleChanged = OnPlayerScaleChanged;
		m_onPlayerDeath = OnPlayerDeath;
		m_onPlayerManaChange = OnPlayerManaChange;
		Messenger<GameMessenger, GameEvent>.AddListener(GameEvent.PlayerScaleChanged, m_onPlayerScaleChanged);
		Messenger<GameMessenger, GameEvent>.AddListener(GameEvent.PlayerManaChange, m_onPlayerManaChange);
		Messenger<GameMessenger, GameEvent>.AddListener(GameEvent.PlayerDeath, m_onPlayerDeath);
		base.Awake();
	}

	public override void OnPreDestroy()
	{
		m_aimIndicator.transform.SetParent(null);
		base.OnPreDestroy();
	}

	private void OnPlayerDeath(object sender, EventArgs args)
	{
		m_aimIndicator.SetActive(value: false);
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		UnityEngine.Object.Destroy(m_aimIndicator);
		Messenger<GameMessenger, GameEvent>.RemoveListener(GameEvent.PlayerScaleChanged, m_onPlayerScaleChanged);
		Messenger<GameMessenger, GameEvent>.RemoveListener(GameEvent.PlayerManaChange, m_onPlayerManaChange);
		Messenger<GameMessenger, GameEvent>.RemoveListener(GameEvent.PlayerDeath, m_onPlayerDeath);
		m_aimIndicator = null;
	}

	private void OnPlayerScaleChanged(object sender, EventArgs args)
	{
		InitializeAimIndicator();
	}

	private void OnPlayerManaChange(object sender, EventArgs args)
	{
		ManaChangeEventArgs manaChangeEventArgs = args as ManaChangeEventArgs;
		float num = manaChangeEventArgs.PrevManaValue - manaChangeEventArgs.NewManaValue;
		bool flag = (float)manaChangeEventArgs.Player.ActualMaxMana == manaChangeEventArgs.NewManaValue;
		bool flag2 = m_abilityController.PlayerController.CastAbility.AbilityInProgress(CastAbilityType.Spell) || m_abilityController.PlayerController.CastAbility.AbilityInProgress(CastAbilityType.Talent);
		if (num > 0f && !flag && flag2)
		{
			m_manaSpentForCrit += num;
			if (m_manaSpentForCrit >= 50f)
			{
				m_manaSpentForCrit = 0f;
				m_abilityController.PlayerController.StatusEffectController.StartStatusEffect(StatusEffectType.Player_FreeCrit, 3f, null);
			}
		}
	}

	public override void Initialize(CastAbility_RL abilityController, CastAbilityType castAbilityType)
	{
		base.Initialize(abilityController, castAbilityType);
		m_aimIndicatorStartingScale = m_aimIndicator.transform.localScale.x;
		m_aimIndicator.transform.SetParent(m_abilityController.PlayerController.Visuals.transform, worldPositionStays: true);
		m_manaSpentForCrit = 0f;
		InitializeAimIndicator();
	}

	public override void Reinitialize()
	{
		base.Reinitialize();
		if (!m_aimIndicator.activeSelf)
		{
			m_aimIndicator.SetActive(value: true);
		}
	}

	private void InitializeAimIndicator()
	{
		float num = m_abilityController.PlayerController.BaseScaleToOffsetWith / m_abilityController.PlayerController.transform.lossyScale.x;
		Vector2 projectileIndicatorPosition = ProjectileIndicatorPosition;
		if (!m_abilityController.PlayerController.IsFacingRight)
		{
			projectileIndicatorPosition.x = 0f - ProjectileIndicatorPosition.x;
		}
		m_aimIndicator.transform.position = m_abilityController.PlayerController.transform.position + (Vector3)projectileIndicatorPosition;
		num += m_aimIndicatorStartingScale * num - num;
		m_aimIndicator.transform.localScale = new Vector3(num, num, num);
	}

	public override void PreCastAbility()
	{
		InitializeAimIndicator();
		if (m_abilityController.PlayerController.StatusEffectController.HasStatusEffect(StatusEffectType.Player_FreeCrit))
		{
			m_animator.SetBool("MagicWand_ForceCrit", value: true);
		}
		base.PreCastAbility();
	}

	protected override void FireProjectile()
	{
		if (!string.IsNullOrEmpty(ProjectileName))
		{
			Vector2 projectileOffset = ProjectileOffset;
			if (!m_abilityController.PlayerController.IsFacingRight)
			{
				projectileOffset.x = 0f - projectileOffset.x;
			}
			projectileOffset += (Vector2)m_abilityController.PlayerController.transform.localPosition;
			m_firedProjectile = ProjectileManager.FireProjectile(m_abilityController.gameObject, ProjectileName, projectileOffset, matchFacing: true, 0f, 1f, useAbsPos: true);
			m_abilityController.InitializeProjectile(m_firedProjectile);
			ApplyAbilityCosts();
		}
		m_strikeProjectile = ProjectileManager.FireProjectile(m_abilityController.gameObject, m_strikeProjectileName, m_strikeProjectileOffset);
		m_abilityController.InitializeProjectile(m_strikeProjectile);
	}

	public override void StopAbility(bool abilityInterrupted)
	{
		base.StopAbility(abilityInterrupted);
		m_animator.SetBool("MagicWand_ForceCrit", value: false);
		if ((bool)m_strikeProjectile)
		{
			m_strikeProjectile.FlagForDestruction();
			m_strikeProjectile = null;
		}
	}

	protected override void PlayFreeCritReleaseAudio()
	{
	}
}
