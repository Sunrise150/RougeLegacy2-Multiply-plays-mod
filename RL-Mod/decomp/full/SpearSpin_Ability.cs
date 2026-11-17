using System;
using Sigtrap.Relays;
using UnityEngine;

public class SpearSpin_Ability : BaseAbility_RL, ITalent, IAbility
{
	private bool m_hitProjectilesWhileActive;

	private float m_spearSpinSuccessStartTime;

	private float m_spearSpinStartTime;

	private Action<Projectile_RL, GameObject> m_onCollision;

	public Relay<bool> SpinCompleteRelay = new Relay<bool>();

	public Relay HitProjectileRelay = new Relay();

	private ForceManaRegenEventArgs m_regenEventArgs = new ForceManaRegenEventArgs(0f, usePlayerRegenMods: false);

	protected override float TellIntroAnimSpeed => 2f;

	protected override float TellIntroAnimExitDelay => 0f;

	protected override float TellAnimSpeed => 2f;

	protected override float TellAnimExitDelay => 0f;

	protected override float AttackIntroAnimSpeed => 1.1f;

	protected override float AttackIntroAnimExitDelay => 0f;

	protected override float AttackAnimSpeed => 1.25f;

	protected override float AttackAnimExitDelay => 0f;

	protected override float ExitAnimSpeed => 2f;

	protected override float ExitAnimExitDelay => 0f;

	protected override void Awake()
	{
		base.Awake();
		m_onCollision = OnCollision;
	}

	protected override void FireProjectile()
	{
	}

	public override void PreCastAbility()
	{
		m_abilityController.PlayerController.PauseGravity(stopVelocity: true, lockPlayerControls: true);
		m_abilityController.PlayerController.IsSpearSpinning = true;
		base.PreCastAbility();
		m_hitProjectilesWhileActive = false;
		m_spearSpinStartTime = Time.time;
		base.FireProjectile();
		if ((bool)m_firedProjectile)
		{
			m_firedProjectile.OnCollisionRelay.AddListener(m_onCollision);
		}
	}

	protected void OnCollision(Projectile_RL projectile, GameObject colliderObj)
	{
		if (colliderObj.CompareTag("EnemyProjectile"))
		{
			m_hitProjectilesWhileActive = true;
			HitProjectileRelay.Dispatch();
			EffectManager.SetEffectParams("SlowTime_Effect", "m_timeScaleValue", 0.05f);
			EffectManager.PlayEffect(m_abilityController.PlayerController.gameObject, m_abilityController.PlayerController.Animator, "SlowTime_Effect", Vector3.zero, 0.175f, EffectStopType.Gracefully);
			RegenMana(colliderObj);
			EndCooldownTimer(displayCooldownResetText: false);
			m_spearSpinSuccessStartTime = Time.time;
		}
	}

	public override void StartCooldownTimer()
	{
		if (!(m_spearSpinStartTime < m_spearSpinSuccessStartTime + 0.05f))
		{
			base.StartCooldownTimer();
		}
	}

	private void RegenMana(GameObject colliderObj)
	{
		m_regenEventArgs.Initialise(15f, usePlayerRegenMods: false);
		EffectManager.PlayEffect(colliderObj, null, "ManaRegenBurst_Effect", Vector3.zero, 0f, EffectStopType.Gracefully);
		Messenger<GameMessenger, GameEvent>.Broadcast(GameEvent.PlayerForceManaRegen, this, m_regenEventArgs);
	}

	public override void StopAbility(bool abilityInterrupted)
	{
		SpinCompleteRelay.Dispatch(m_hitProjectilesWhileActive);
		m_abilityController.PlayerController.ResumeGravity();
		m_abilityController.PlayerController.IsSpearSpinning = false;
		if ((bool)m_firedProjectile)
		{
			m_firedProjectile.OnCollisionRelay.RemoveListener(m_onCollision);
			m_firedProjectile.FlagForDestruction();
			m_firedProjectile = null;
		}
		base.StopAbility(abilityInterrupted);
	}
}
