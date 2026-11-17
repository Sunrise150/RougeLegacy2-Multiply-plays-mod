using System.Collections;
using RLAudio;
using Sigtrap.Relays;
using UnityEngine;

public class TraitorBoss_Expert_AIScript : TraitorBoss_Basic_AIScript
{
	private const string SHOUT_TELL_INTRO = "OmniSpellCast_Tell_Intro";

	private const string SHOUT_TELL_HOLD = "OmniSpellCast_Tell_Hold";

	private const string SHOUT_ATTACK_INTRO = "OmniSpellCast_Attack_Intro";

	private const string SHOUT_ATTACK_HOLD = "OmniSpellCast_Attack_Hold";

	private const string SHOUT_EXIT = "OmniSpellCast_Exit";

	private float m_shout_TellIntro_AnimSpeed = 1f;

	private float m_shout_TellHold_AnimSpeed = 1f;

	private float m_shout_TellIntroAndHold_Delay = 1.5f;

	private float m_shout_AttackIntro_AnimSpeed = 1f;

	private float m_shout_AttackIntro_Delay;

	private float m_shout_AttackHold_AnimSpeed = 1f;

	private float m_shout_AttackHold_Delay;

	private float m_shout_Exit_AnimSpeed = 0.65f;

	private float m_shout_Exit_Delay;

	private float m_shout_Exit_IdleDuration = 0.35f;

	private float m_shout_AttackCD = 12f;

	private float m_shoutAttackTimeToExplosion;

	private bool m_shout_stopMovingWhileAttacking = true;

	private float m_shoutAttackDelaySecondExplosion = 0.65f;

	private int m_shoutAttackDelaySecondExplosionAngleAdd = 10;

	private Projectile_RL m_shoutWarningProjectile;

	private Relay m_shoutAttackWarningAppearedRelay = new Relay();

	private Relay m_shoutAttackExplodedRelay = new Relay();

	protected const string STAFFTHROW_TELL_INTRO = "SpellCast_Attack_Intro";

	protected const string STAFFTHROW_TELL_HOLD = "StaffForward_Tell_Hold";

	protected const string STAFFTHROW_ATTACK_INTRO = "StaffForward_Attack_Intro";

	protected const string STAFFTHROW_ATTACK_HOLD = "SpellCast_Attack_Hold";

	protected const string STAFFTHROW_EXIT = "SpellCast_Exit";

	protected float m_staffThrow_BeamAttack_Duration = 0.75f;

	protected float m_staffThrow_BeamAttack_Variant_Duration = 1f;

	protected float m_staffThrow_AttackSpeed = 28f;

	protected float m_staffThrow_AttackDuration_EndDelay = 1f;

	protected float m_staffThrow_TellIntro_AnimSpeed = 1.5f;

	protected float m_staffThrow_TellHold_AnimSpeed = 1f;

	protected float m_staffThrow_TellIntroAndHold_Delay = 0.9f;

	protected float m_staffThrow_AttackIntro_AnimSpeed = 1.2f;

	protected float m_staffThrow_AttackIntro_Delay;

	protected float m_staffThrow_AttackHold_AnimSpeed = 1.2f;

	protected float m_staffThrow_AttackHold_Delay;

	protected float m_staffThrow_Exit_AnimSpeed = 1f;

	protected float m_staffThrow_Exit_Delay;

	protected float m_staffThrow_Exit_IdleDuration = 0.15f;

	protected float m_staffThrow_AttackCD = 12f;

	protected int STAFF_FRONT_POS_INDEX = 1;

	private Projectile_RL m_staffBeamProjectile;

	private Projectile_RL m_staffBeamWarningProjectile;

	protected override int m_magma_ProjectilesFired => 1;

	protected override float m_magma_ProjectileDelay => 0.15f;

	protected virtual int m_shout_fireballsAtEnd => 18;

	public IRelayLink ShoutAttackWarningAppearedRelay => m_shoutAttackWarningAppearedRelay.link;

	public IRelayLink ShoutAttackExplodedRelay => m_shoutAttackExplodedRelay.link;

	[CloseLogic]
	[MediumLogic]
	[FarLogic]
	[ExpertEnemy]
	[MinibossEnemy]
	public IEnumerator CustomMagmaCombo()
	{
		StopAndFaceTarget();
		base.EnemyController.LockFlip = true;
		yield return SetWeaponGeo(m_weaponGeoController.Staff, DEFAULT_WEAPON_SWAP_DELAY);
		CDGHelper.RandomPlusMinus();
		yield return Single_Jump();
		float seconds = Random.Range(m_magma_JumpDelay.x, m_magma_JumpDelay.y);
		yield return Wait(seconds);
		yield return Default_Animation("SpellCast_Attack_Intro", base.m_magma_AttackIntro_AnimationSpeed, base.m_magma_AttackIntro_Delay);
		yield return Default_Animation("SpellCast_Attack_Hold", base.m_magma_AttackHold_AnimationSpeed, base.m_magma_AttackHold_Delay, waitUntilAnimationComplete: false);
		for (int i = 0; i < m_magma_ProjectilesFired; i++)
		{
			TraitorBoss_Expert_AIScript aiScript = this;
			_ = m_magma_angleAdder;
			aiScript.FireProjectile("TraitorMagmaProjectile", 0, matchFacing: true, 90f);
			this.FireProjectile("TraitorMagmaProjectile", 0, matchFacing: true, 90 + m_magma_angleAdder);
			this.FireProjectile("TraitorMagmaProjectile", 0, matchFacing: true, 90 + m_magma_angleAdder * -1);
			this.FireProjectile("TraitorMagmaProjectile", 0, matchFacing: true, 90 + m_magma_angleAdder * 2);
			this.FireProjectile("TraitorMagmaProjectile", 0, matchFacing: true, 90 + m_magma_angleAdder * -2);
			if (m_magma_ProjectileDelay != 0f)
			{
				yield return Wait(m_magma_ProjectileDelay);
			}
		}
		yield return Default_Animation("SpellCast_Exit", base.m_magma_Exit_AnimationSpeed, base.m_magma_Exit_Delay);
		base.Animator.SetTrigger("Change_Ability_Anim");
		base.EnemyController.LockFlip = false;
		yield return Single_WaitUntilGrounded();
		HideAllWeaponGeos();
		yield return Default_Attack_Cooldown(m_magma_Exit_ForceIdle, m_magma_Exit_AttackCD);
		base.EnemyController.ResetBaseValues();
	}

	[CloseLogic]
	[MediumLogic]
	[FarLogic]
	[BasicEnemy]
	[AdvancedEnemy]
	[ExpertEnemy]
	[MinibossEnemy]
	public IEnumerator ShoutAttack()
	{
		this.ToDo("SHOUT ATTACK");
		if (m_shout_stopMovingWhileAttacking)
		{
			base.EnemyController.FlyingMovementType = FlyingMovementType.Override;
			StopAndFaceTarget();
			base.EnemyController.LockFlip = true;
		}
		m_shoutWarningProjectile = this.FireProjectile("TraitorBossShoutWarningProjectile", 2, matchFacing: false);
		yield return Default_TellIntroAndLoop("OmniSpellCast_Tell_Intro", m_shout_TellIntro_AnimSpeed, "OmniSpellCast_Tell_Hold", m_shout_TellHold_AnimSpeed, m_shout_TellIntroAndHold_Delay);
		SetAttackingWithContactDamage(enable: true);
		yield return Default_Animation("OmniSpellCast_Attack_Intro", m_shout_AttackIntro_AnimSpeed, m_shout_AttackIntro_Delay);
		m_shoutWarningProjectile.transform.position = base.EnemyController.Midpoint;
		m_shoutWarningProjectile.transform.SetParent(base.EnemyController.transform, worldPositionStays: true);
		m_shoutAttackWarningAppearedRelay.Dispatch();
		StopProjectile(ref m_shoutWarningProjectile);
		m_shoutAttackExplodedRelay.Dispatch();
		this.FireProjectile("TraitorBossShoutExplosionProjectile", 2, matchFacing: false);
		yield return Default_Animation("OmniSpellCast_Attack_Hold", m_shout_AttackHold_AnimSpeed, m_shout_AttackHold_Delay);
		SetAttackingWithContactDamage(enable: false);
		yield return Default_Animation("OmniSpellCast_Exit", m_shout_Exit_AnimSpeed, m_shout_Exit_Delay);
		base.EnemyController.LockFlip = false;
		base.EnemyController.FlyingMovementType = FlyingMovementType.Stop;
		yield return Default_Attack_Cooldown(m_shout_Exit_IdleDuration, m_shout_AttackCD);
	}

	[CloseLogic]
	[MediumLogic]
	[FarLogic]
	[ExpertEnemy]
	[MinibossEnemy]
	public IEnumerator Staff_Throw()
	{
		StopAndFaceTarget();
		base.EnemyController.LockFlip = true;
		m_staffBeamWarningProjectile = this.FireProjectile("TraitorWarningForwardBeamProjectile", STAFF_FRONT_POS_INDEX);
		AudioManager.PlayOneShot(null, "event:/SFX/Enemies/Spells/sfx_enemy_spell_gravityWell_prep", base.EnemyController.Midpoint);
		yield return Default_Animation("SpellCast_Attack_Intro", m_staffThrow_TellIntro_AnimSpeed, m_staffThrow_TellIntroAndHold_Delay);
		yield return Default_Animation("StaffForward_Attack_Intro", m_staffThrow_AttackIntro_AnimSpeed, m_staffThrow_AttackIntro_Delay);
		yield return Default_Animation("SpellCast_Attack_Hold", m_staffThrow_AttackHold_AnimSpeed, m_staffThrow_AttackHold_Delay, waitUntilAnimationComplete: false);
		StopProjectile(ref m_staffBeamWarningProjectile);
		m_staffBeamProjectile = this.FireProjectile("TraitorForwardBeamProjectile", STAFF_FRONT_POS_INDEX);
		if (m_staffThrow_BeamAttack_Duration > 0f)
		{
			yield return Wait(m_staffThrow_BeamAttack_Duration);
		}
		StopProjectile(ref m_staffBeamProjectile);
		SetAttackingWithContactDamage(enable: false);
		yield return Wait(m_staffThrow_AttackDuration_EndDelay);
		yield return Default_Animation("SpellCast_Exit", m_staffThrow_Exit_AnimSpeed, m_staffThrow_Exit_Delay);
		base.EnemyController.LockFlip = false;
		base.EnemyController.ResetBaseValues();
		yield return Default_Attack_Cooldown(m_staffThrow_Exit_IdleDuration, m_staffThrow_AttackCD);
	}

	public override void OnLBCompleteOrCancelled()
	{
		base.OnLBCompleteOrCancelled();
		StopProjectile(ref m_staffBeamWarningProjectile);
		StopProjectile(ref m_staffBeamProjectile);
		StopProjectile(ref m_shoutWarningProjectile);
	}
}
