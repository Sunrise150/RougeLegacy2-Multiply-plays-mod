using System.Collections;
using UnityEngine;

public class Wolf_Miniboss_AIScript : Wolf_Basic_AIScript
{
	protected const string STAFFTHROW_TELL_INTRO = "Laser_Tell_Intro";

	protected const string STAFFTHROW_TELL_HOLD = "Laser_Tell_Hold";

	protected const string STAFFTHROW_ATTACK_INTRO = "Laser_Attack_Intro";

	protected const string STAFFTHROW_ATTACK_HOLD = "Laser_Attack_Hold";

	protected const string STAFFTHROW_EXIT = "Laser_Attack_Exit";

	protected const string STAFFTHROW_PROJECTILE = "SpellSwordStaffForwardBoltProjectile";

	protected const string STAFFTHROW_BEAM_PROJECTILE = "WolfForwardBeamProjectile";

	protected const string STAFFTHROW_BEAM_WARNING_PROJECTILE = "WolfWarningForwardBeamProjectile";

	protected const int STAFF_FRONT_POS_INDEX = 2;

	protected float m_staffThrow_BeamAttack_Duration = 1.2f;

	protected float m_staffThrow_BeamAttack_Variant_Duration = 1f;

	protected float m_staffThrow_AttackSpeed = 28f;

	protected float m_staffThrow_AttackDuration_EndDelay = 1f;

	protected float m_staffThrow_TellIntro_AnimSpeed = 1.5f;

	protected float m_staffThrow_TellHold_AnimSpeed = 1f;

	protected float m_staffThrow_TellIntroAndHold_Delay = 1.1f;

	protected float m_staffThrow_AttackIntro_AnimSpeed = 1.2f;

	protected float m_staffThrow_AttackIntro_Delay;

	protected float m_staffThrow_AttackHold_AnimSpeed = 1.2f;

	protected float m_staffThrow_AttackHold_Delay;

	protected float m_staffThrow_Exit_AnimSpeed = 1f;

	protected float m_staffThrow_Exit_Delay;

	protected float m_staffThrow_Exit_IdleDuration = 0.15f;

	protected float m_staffThrow_AttackCD = 12f;

	private Projectile_RL m_staffBeamProjectile;

	private Projectile_RL m_staffBeamWarningProjectile;

	protected override Vector2 IdleDuration => new Vector2(0.25f, 0.25f);

	protected override Vector2 WalkTowardsDuration => new Vector2(0.6f, 1.15f);

	protected override Vector2 WalkAwayDuration => new Vector2(0.6f, 1.15f);

	protected override Vector2 m_dash_AttackHold_ForwardPower => new Vector2(28f, 18f);

	protected override Vector2 m_jumpPower => new Vector2(20f, 32f);

	protected override bool m_howl_Randomize_Howl => false;

	protected override bool m_howl_Spawn_Projectile => true;

	protected override bool m_howl_At_Start => false;

	[CloseLogic]
	[MediumLogic]
	[FarLogic]
	[BasicEnemy]
	[AdvancedEnemy]
	[ExpertEnemy]
	[MinibossEnemy]
	public IEnumerator Staff_Throw()
	{
		StopAndFaceTarget();
		base.EnemyController.LockFlip = true;
		m_staffBeamWarningProjectile = this.FireProjectile("WolfWarningForwardBeamProjectile", 2);
		yield return Default_TellIntroAndLoop("Laser_Tell_Intro", m_staffThrow_TellIntro_AnimSpeed, "Laser_Tell_Hold", m_staffThrow_TellHold_AnimSpeed, m_staffThrow_TellIntroAndHold_Delay);
		SetAttackingWithContactDamage(enable: true);
		yield return Default_Animation("Laser_Attack_Intro", m_staffThrow_AttackIntro_AnimSpeed, m_staffThrow_AttackIntro_Delay);
		yield return Default_Animation("Laser_Attack_Hold", m_staffThrow_AttackHold_AnimSpeed, m_staffThrow_AttackHold_Delay, waitUntilAnimationComplete: false);
		if ((bool)m_staffBeamWarningProjectile && m_staffBeamWarningProjectile.isActiveAndEnabled)
		{
			m_staffBeamWarningProjectile.FlagForDestruction();
			m_staffBeamWarningProjectile = null;
		}
		m_staffBeamProjectile = this.FireProjectile("WolfForwardBeamProjectile", 2);
		if (m_staffThrow_BeamAttack_Duration > 0f)
		{
			yield return Wait(m_staffThrow_BeamAttack_Duration);
		}
		if ((bool)m_staffBeamProjectile && m_staffBeamProjectile.isActiveAndEnabled)
		{
			m_staffBeamProjectile.FlagForDestruction();
			m_staffBeamProjectile = null;
		}
		SetAttackingWithContactDamage(enable: false);
		yield return Wait(m_staffThrow_AttackDuration_EndDelay);
		yield return Default_Animation("Laser_Attack_Exit", m_staffThrow_Exit_AnimSpeed, m_staffThrow_Exit_Delay);
		base.EnemyController.LockFlip = false;
		base.EnemyController.ResetBaseValues();
		yield return Default_Attack_Cooldown(m_staffThrow_Exit_IdleDuration, m_staffThrow_AttackCD);
	}
}
