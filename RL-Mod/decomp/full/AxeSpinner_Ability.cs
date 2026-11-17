using System.Collections;

public class AxeSpinner_Ability : Sword_Ability
{
	private bool m_isSpinning;

	private bool m_invulnDamageBuffApplied;

	private bool m_attackCooldownSpeedModApplied;

	protected override float TellIntroAnimSpeed => 10f;

	protected override float TellIntroAnimExitDelay => 0f;

	protected override float TellAnimSpeed => 1.15f;

	protected override float TellAnimExitDelay => 0f;

	protected override float AttackIntroAnimSpeed => 1.15f;

	protected override float AttackIntroAnimExitDelay => 0f;

	protected override float AttackAnimSpeed => 1.25f;

	protected override float AttackAnimExitDelay => 0f;

	protected override float ExitAnimSpeed => 1.4f;

	protected override float ExitAnimExitDelay => 0f;

	protected override void OnEnterExitLogic()
	{
		if ((bool)m_firedProjectile)
		{
			m_firedProjectile.FlagForDestruction();
		}
		base.OnEnterExitLogic();
	}

	protected override IEnumerator ChangeAnim(float duration)
	{
		if (base.CurrentAbilityAnimState == AbilityAnimState.Attack)
		{
			while (m_isSpinning)
			{
				yield return null;
			}
		}
		yield return base.ChangeAnim(duration);
	}

	protected override void OnEnterAttackLogic()
	{
		m_isSpinning = true;
		m_abilityController.BroadcastAbilityCastEvents(base.CastAbilityType);
		int level = SaveManager.PlayerSaveData.GetRelic(RelicType.AttackCooldown).Level;
		m_abilityController.PlayerController.MovementSpeedMod += -0.3f * (float)level;
		m_attackCooldownSpeedModApplied = true;
		FireProjectile();
	}

	protected override void FireProjectile()
	{
		m_invulnDamageBuffApplied = false;
		if (m_abilityController.PlayerController.IsInvincible && SaveManager.PlayerSaveData.GetRelic(RelicType.InvulnDamageBuff).Level > 0)
		{
			m_invulnDamageBuffApplied = true;
		}
		base.FireProjectile();
	}

	public override void StopAbility(bool abilityInterrupted)
	{
		m_isSpinning = false;
		StartCooldownTimer();
		if (m_attackCooldownSpeedModApplied)
		{
			int level = SaveManager.PlayerSaveData.GetRelic(RelicType.AttackCooldown).Level;
			m_abilityController.PlayerController.MovementSpeedMod -= -0.3f * (float)level;
			m_attackCooldownSpeedModApplied = false;
		}
		base.StopAbility(abilityInterrupted);
	}

	protected override void Update()
	{
		base.Update();
		if (!base.AbilityActive)
		{
			return;
		}
		if (m_isSpinning && m_invulnDamageBuffApplied && (bool)m_firedProjectile && !m_abilityController.PlayerController.IsInvincible)
		{
			float num = 1f * (float)SaveManager.PlayerSaveData.GetRelic(RelicType.InvulnDamageBuff).Level;
			if (m_firedProjectile.DamageMod > 0f)
			{
				m_firedProjectile.DamageMod -= num;
			}
			m_invulnDamageBuffApplied = false;
		}
		if (Rewired_RL.Player.GetButtonDown(m_abilityController.GetAbilityInputString(base.CastAbilityType)) && m_isSpinning)
		{
			m_isSpinning = false;
			CancelChangeAnimCoroutine();
			m_animator.Play("AxeGrounded_Exit");
		}
	}
}
