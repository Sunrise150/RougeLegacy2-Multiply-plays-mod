using System.Collections;

public class TraitorBoss_Miniboss_AIScript : TraitorBoss_Basic_AIScript
{
	protected override bool is_Gregory => true;

	[CloseLogic]
	[MediumLogic]
	[FarLogic]
	[ExpertEnemy]
	[MinibossEnemy]
	public IEnumerator CustomAxeCombo()
	{
		StopAndFaceTarget();
		base.EnemyController.LockFlip = true;
		Single_Enable_KnockbackDefense();
		yield return SetWeaponGeo(m_weaponGeoController.Axe, DEFAULT_WEAPON_SWAP_DELAY);
		yield return Default_TellIntroAndLoop("AxeGrounded_Tell_Intro", m_axe_TellIntro_AnimSpeed, "AxeGrounded_Tell_Hold", m_axe_TellHold_AnimSpeed, m_axe_TellIntroAndHold_Delay);
		yield return Default_Animation("AxeAirborne_Attack_Hold", m_axe_AttackHold_AnimSpeed, m_axe_AttackHold_Delay, waitUntilAnimationComplete: false);
		m_axeSpinProjectile = this.FireProjectile("TraitorAxeSpinProjectile", 1, matchFacing: false);
		yield return Single_Jump();
		yield return Single_WaitUntilGrounded();
		base.EnemyController.LockFlip = false;
		StopAndFaceTarget();
		base.EnemyController.LockFlip = true;
		yield return Single_Jump();
		yield return Single_WaitUntilGrounded();
		base.EnemyController.LockFlip = false;
		StopAndFaceTarget();
		base.EnemyController.LockFlip = true;
		if (is_Advanced)
		{
			base.EnemyController.LockFlip = false;
			StopAndFaceTarget();
			base.EnemyController.LockFlip = true;
			yield return Single_Jump(randomizeJump: false, lowJump: true);
			yield return Single_WaitUntilGrounded();
		}
		StopProjectile(ref m_axeSpinProjectile);
		Single_Disable_KnockbackDefense();
		yield return Default_Animation("AxeAirborne_Exit", m_axe_Exit_AnimSpeed, m_axe_Exit_Delay);
		base.Animator.SetTrigger("Change_Ability_Anim");
		base.EnemyController.LockFlip = false;
		HideAllWeaponGeos();
		base.EnemyController.ResetBaseValues();
		yield return Default_Attack_Cooldown(m_axe_Exit_IdleDuration, m_axe_AttackCD);
	}
}
