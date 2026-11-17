using System.Collections;
using RLAudio;
using UnityEngine;

public class FlyingBurst_Miniboss_AIScript : FlyingBurst_Basic_AIScript
{
	protected float m_huntAttack_TellIntro_AnimationSpeed = 1f;

	protected float m_huntAttack_TellHold_AnimationSpeed = 2f;

	protected float m_huntAttack_Tell_Delay = 1.15f;

	protected float m_huntAttack_AttackIntro_AnimationSpeed = 1f;

	protected float m_huntAttack_AttackIntro_Delay;

	protected float m_huntAttack_AttackHold_AnimationSpeed = 1.25f;

	protected float m_huntAttack_AttackHold_Delay = 0.4f;

	protected float m_huntAttack_Exit_AnimationSpeed = 1f;

	protected float m_huntAttack_Exit_Delay;

	protected float m_huntAttack_Exit_ForceIdle = 0.15f;

	protected float m_huntAttack_Exit_AttackCD;

	protected float m_chaseAttack_TellHold_Delay = 1.4f;

	protected float m_chaseAttack_AttackHold_Delay = 0.4f;

	protected float m_chaseAttack_Exit_AnimationSpeed = 1f;

	protected float m_chaseAttack_Exit_Delay;

	protected float m_chaseAttack_Exit_ForceIdle = 0.15f;

	protected float m_chaseAttack_Exit_AttackCD;

	protected override int ShootPatternTotalLoops => 4;

	protected override float ShootPatternTotalLoopsDelay => 1f;

	protected override float MeleeFireSpread => 150f;

	protected override float BasicFireSpread => 40f;

	protected override Vector2 FireballRandSpeedMod => new Vector2(0.25f, 1.5f);

	protected override int NumFireballsMelee => 8;

	protected override int NumFireballsBasic => 2;

	protected virtual int m_huntAttack_NumFireballsBasic => 1;

	protected virtual float m_huntAttack_BasicFireSpread => 50f;

	[CloseLogic]
	[MediumLogic]
	[FarLogic]
	[MinibossEnemy]
	public IEnumerator HuntAttack()
	{
		base.EnemyController.AlwaysFacing = false;
		Vector3 absoluteSpawnPositionAtIndex = GetAbsoluteSpawnPositionAtIndex(3, matchFacing: false);
		float startingAngle = CDGHelper.VectorToAngle(base.EnemyController.TargetController.Midpoint - absoluteSpawnPositionAtIndex);
		int angleAdder = 45;
		AudioManager.PlayOneShotAttached(null, "event:/SFX/Enemies/Sfx_enemy_flyingBurst_multishot_charge", base.EnemyController.gameObject);
		EffectManager.PlayEffect(base.EnemyController.gameObject, null, "FlyingBurstLanternChargeUp_Effect", absoluteSpawnPositionAtIndex, m_huntAttack_Tell_Delay, EffectStopType.Gracefully).transform.SetParent(base.EnemyController.Pivot.transform, worldPositionStays: true);
		yield return Wait(m_huntAttack_Tell_Delay);
		AudioManager.PlayOneShot(null, "event:/SFX/Enemies/vo_flyingBurst_agro", base.EnemyController.Midpoint);
		AudioManager.PlayOneShot(null, "event:/SFX/Enemies/sfx_enemy_flyingBurstBoss_multishot_shoot", base.EnemyController.Midpoint);
		absoluteSpawnPositionAtIndex = GetAbsoluteSpawnPositionAtIndex(3, matchFacing: false);
		EffectManager.PlayEffect(base.EnemyController.gameObject, null, "FlyingBurstLanternRelease_Effect", absoluteSpawnPositionAtIndex, 0f, EffectStopType.Gracefully);
		this.FireProjectile("FlyingBurstBlueBoltMinibossProjectile", 3, matchFacing: false, startingAngle + 0f);
		this.FireProjectile("FlyingBurstBlueBoltMinibossProjectile", 3, matchFacing: false, startingAngle + (float)angleAdder);
		this.FireProjectile("FlyingBurstBlueBoltMinibossProjectile", 3, matchFacing: false, startingAngle + (float)(angleAdder * 2));
		this.FireProjectile("FlyingBurstBlueBoltMinibossProjectile", 3, matchFacing: false, startingAngle + (float)(angleAdder * 3));
		this.FireProjectile("FlyingBurstBlueBoltMinibossProjectile", 3, matchFacing: false, startingAngle + (float)(angleAdder * 4));
		this.FireProjectile("FlyingBurstBlueBoltMinibossProjectile", 3, matchFacing: false, startingAngle + (float)(angleAdder * 5));
		this.FireProjectile("FlyingBurstBlueBoltMinibossProjectile", 3, matchFacing: false, startingAngle + (float)(angleAdder * 6));
		this.FireProjectile("FlyingBurstBlueBoltMinibossProjectile", 3, matchFacing: false, startingAngle + (float)(angleAdder * 7));
		if (m_huntAttack_AttackHold_Delay > 0f)
		{
			yield return Wait(m_huntAttack_AttackHold_Delay);
		}
		base.EnemyController.AlwaysFacing = true;
		yield return Default_Attack_Cooldown(m_huntAttack_Exit_ForceIdle, m_huntAttack_Exit_AttackCD);
	}

	[CloseLogic]
	[MediumLogic]
	[FarLogic]
	[MinibossEnemy]
	public IEnumerator ChaserAttack()
	{
		base.EnemyController.AlwaysFacing = false;
		Vector3 absoluteSpawnPositionAtIndex = GetAbsoluteSpawnPositionAtIndex(3, matchFacing: false);
		CDGHelper.VectorToAngle(base.EnemyController.TargetController.Midpoint - absoluteSpawnPositionAtIndex);
		int angleAdder = 72;
		AudioManager.PlayOneShot(null, "event:/SFX/Enemies/Sfx_enemy_flyingBurst_multishot_charge", base.EnemyController.Midpoint);
		EffectManager.PlayEffect(base.EnemyController.gameObject, null, "FlyingBurstCurseChargeUp_Effect", absoluteSpawnPositionAtIndex, m_chaseAttack_TellHold_Delay, EffectStopType.Gracefully).transform.SetParent(base.EnemyController.Pivot.transform, worldPositionStays: true);
		yield return Wait(m_chaseAttack_TellHold_Delay);
		AudioManager.PlayOneShot(null, "event:/SFX/Enemies/vo_flyingBurst_agro", base.EnemyController.Midpoint);
		AudioManager.PlayOneShot(null, "event:/SFX/Enemies/sfx_enemy_flyingBurst_multishot_shoot", base.EnemyController.Midpoint);
		absoluteSpawnPositionAtIndex = GetAbsoluteSpawnPositionAtIndex(3, matchFacing: false);
		EffectManager.PlayEffect(base.EnemyController.gameObject, null, "EnemyCurseProjectileSpawn_Effect", absoluteSpawnPositionAtIndex, 0f, EffectStopType.Gracefully);
		this.FireProjectile("FlyingBurstSlashBoltMinibossProjectile", 3, matchFacing: false);
		this.FireProjectile("FlyingBurstSlashBoltMinibossProjectile", 3, matchFacing: false, angleAdder);
		this.FireProjectile("FlyingBurstSlashBoltMinibossProjectile", 3, matchFacing: false, angleAdder * 2);
		this.FireProjectile("FlyingBurstSlashBoltMinibossProjectile", 3, matchFacing: false, angleAdder * 3);
		this.FireProjectile("FlyingBurstSlashBoltMinibossProjectile", 3, matchFacing: false, angleAdder * 4);
		if (m_chaseAttack_AttackHold_Delay > 0f)
		{
			yield return Wait(m_chaseAttack_AttackHold_Delay);
		}
		base.EnemyController.AlwaysFacing = true;
		yield return Default_Attack_Cooldown(m_chaseAttack_Exit_ForceIdle, m_chaseAttack_Exit_AttackCD);
	}
}
