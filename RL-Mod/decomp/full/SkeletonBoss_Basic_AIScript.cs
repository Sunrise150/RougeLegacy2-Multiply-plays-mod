using System;
using System.Collections;
using RLAudio;
using UnityEngine;

public class SkeletonBoss_Basic_AIScript : BaseAIScript
{
	public UnityEvent_GameObject JumpEvent;

	protected int m_modeShiftIndex;

	protected float[] m_modeShiftLevels_Basic = new float[1] { 0.65f };

	protected float[] m_modeShiftLevels_Advanced_Skeleton_A = new float[2] { 0.66f, 0.33f };

	protected float[] m_modeShiftLevels_Advanced_Skeleton_B = new float[1] { 0.5f };

	private EnemyModeShiftEventArgs m_modeShiftEventArgs;

	private bool m_isModeShifting;

	protected float m_throwBone_TellIntro_AnimationSpeed = 0.85f;

	protected float m_throwBone_TellHold_AnimationSpeed = 1.3f;

	protected const float m_throwBone_TellIntroAndHold_Delay = 0.65f;

	protected float m_throwBone_AttackIntro_AnimationSpeed = 0.8f;

	protected float m_throwBone_AttackIntro_Delay;

	protected float m_throwBone_AttackHold_AnimationSpeed = 0.8f;

	protected const float m_throwBone_AttackHold_Delay = 0.75f;

	protected float m_throwBone_Exit_AnimationSpeed = 0.65f;

	protected float m_throwBone_Exit_Delay = 0.15f;

	protected int m_BoneNear_Angle = 83;

	protected int m_FarBoneAngle = 70;

	protected int m_miniBoss_Bone_ThrowAmount = 6;

	protected Vector2 m_miniBoss_Bone_ThrowAngle = new Vector2(55f, 85f);

	protected float m_throwBone_Exit_ForceIdle = 0.15f;

	protected float m_throwBone_AttackCD;

	protected float m_throwRib_TellIntro_AnimationSpeed = 0.75f;

	protected float m_throwRib_TellHold_AnimationSpeed = 1.35f;

	protected const float m_throwRib_TellIntroAndHold_Delay = 0.85f;

	protected float m_throwRib_AttackIntro_AnimationSpeed = 1.1f;

	protected float m_throwRib_AttackIntro_Delay;

	protected float m_throwRib_AttackHold_AnimationSpeed = 1.1f;

	protected float m_throwRib_AttackHold_Delay = 1.25f;

	protected float m_throwRib_Exit_AnimationSpeed = 1f;

	protected float m_throwRib_Exit_Delay;

	protected int m_throwRib_Angle;

	protected float m_throwRib_secondRibSpeedMod = 1.35f;

	protected float m_throwRib_Exit_ForceIdle = 0.15f;

	protected float m_throwRib_Exit_AttackCD = 2f;

	protected float m_miniBoss_Rib_ThrowAmount = 1f;

	protected float m_miniBoss_Rib_ThrowPower = 1f;

	protected Vector2 m_miniBoss_Rib_ThrowHeight = new Vector2(6f, 6f);

	protected float m_miniBoss_Rib_LoopDelay = 0.9f;

	protected float m_jump_Tell_Delay = 0.4f;

	protected float m_jump_Tell_AnimationSpeed = 0.75f;

	protected Vector2 m_jump_Power = new Vector2(0f, 27f);

	protected int m_miniBoss_Bone_JumpAmount = 5;

	protected float m_jump_Exit_Delay;

	protected float m_jump_Exit_ForceIdle = 0.15f;

	protected float m_jump_Exit_AttackCD;

	protected Vector2 m_jumpAndThrowBone_ProjectileSpawnDelay = new Vector2(0f, 0f);

	protected float m_jumpAndThrowBone_Exit_ForceIdle = 0.15f;

	protected float m_jumpAndThrowBone_Exit_AttackCD = 1.25f;

	protected Vector2 m_miniBoss_Land_ThrowAngle = new Vector2(25f, 30f);

	protected Vector2 m_miniBoss_Land_ThrowPower = new Vector2(0.75f, 1f);

	protected float m_dance_AttackIntro_AnimationSpeed = 1.1f;

	protected float m_dance_AttackIntro_Delay = 1f;

	protected float m_dance_AttackHold_Delay = 1f;

	protected float m_dance_Exit_ForceIdle = 1f;

	protected float m_dance_Exit_AttackCD = 5f;

	protected const string MODE_SHIFT_TELL_INTRO = "ModeShift_Intro";

	protected const string MODE_SHIFT_ATTACK_INTRO = "ModeShift_Scream_Intro";

	protected const string MODE_SHIFT_ATTACK_HOLD = "ModeShift_Scream_Hold";

	protected const string MODE_SHIFT_EXIT = "ModeShift_Scream_Exit";

	protected float m_modeShift_Tell_Intro_AnimSpeed = 1f;

	protected float m_modeShift_TellIntro_Delay = 0.5f;

	protected float m_modeShift_Attack_Intro_AnimSpeed = 1f;

	protected float m_modeShift_Attack_Intro_Delay;

	protected float m_modeShift_Attack_Hold_AnimSpeed = 1f;

	protected float m_modeShift_Attack_Hold_Delay = 2f;

	protected float m_modeShift_Exit_AnimSpeed = 1f;

	protected float m_modeShift_Exit_Delay;

	protected bool m_modeShiftSummons_Appeared;

	[NonSerialized]
	public bool ModeShiftComplete_SpawnSecondBoss;

	protected const string SPAWN_IDLE = "Intro_Idle";

	protected const string SPAWN_INTRO = "Intro";

	protected float m_spawn_Idle_AnimSpeed = 1f;

	protected float m_spawn_Idle_Delay;

	protected float m_spawn_Intro_AnimSpeed = 1f;

	protected float m_spawn_Intro_Delay;

	protected const string DEATH_INTRO = "Death_Intro";

	protected const string DEATH_HOLD = "Death_Loop";

	protected float m_death_Intro_AnimSpeed = 1f;

	protected float m_death_Intro_Delay;

	protected float m_death_Hold_AnimSpeed = 1f;

	protected float m_death_Hold_Delay = 4.5f;

	protected override Vector2 IdleDuration => new Vector2(0.25f, 0.7f);

	protected override Vector2 WalkTowardsDuration => new Vector2(0.45f, 0.85f);

	protected override Vector2 WalkAwayDuration => new Vector2(0.45f, 0.85f);

	protected virtual bool m_jump_Land_Spawn_Curse => false;

	protected virtual float m_modeShift_Damage_Mod => 0.1f;

	protected override void InitializeProjectileNameArray()
	{
		m_projectileNameArray = new string[7] { "SkeletonBoneProjectile", "SkeletonBoneMinibossProjectile", "SkeletonRibProjectile", "SkeletonBoneBigProjectile", "SkeletonBossCurseProjectile", "SkeletonCannonBallProjectile", "SkeletonCannonBallWarningProjectile" };
	}

	[CloseLogic]
	[MediumLogic]
	[FarLogic]
	[BasicEnemy]
	[AdvancedEnemy]
	[ExpertEnemy]
	[MinibossEnemy]
	public IEnumerator Throw_Bone_Near_Attack()
	{
		StopAndFaceTarget();
		base.EnemyController.AlwaysFacing = false;
		yield return Single_Attack_Throw_Bone(m_BoneNear_Angle);
		SetAttackingWithContactDamage(enable: false);
		base.EnemyController.AlwaysFacing = true;
		yield return Default_Attack_Cooldown(m_throwBone_Exit_ForceIdle, m_throwBone_AttackCD);
	}

	[CloseLogic]
	[MediumLogic]
	[FarLogic]
	[BasicEnemy]
	[AdvancedEnemy]
	[ExpertEnemy]
	[MinibossEnemy]
	public IEnumerator Throw_Bone_Far_Attack()
	{
		StopAndFaceTarget();
		base.EnemyController.AlwaysFacing = false;
		yield return Single_Attack_Throw_Bone(m_FarBoneAngle);
		SetAttackingWithContactDamage(enable: false);
		base.EnemyController.AlwaysFacing = true;
		yield return Default_Attack_Cooldown(m_throwBone_Exit_ForceIdle, m_throwBone_AttackCD);
	}

	[CloseLogic]
	[MediumLogic]
	[FarLogic]
	[BasicEnemy]
	[AdvancedEnemy]
	[ExpertEnemy]
	[MinibossEnemy]
	public IEnumerator Throw_Rib_Attack()
	{
		StopAndFaceTarget();
		base.EnemyController.AlwaysFacing = false;
		yield return Single_Attack_Throw_Rib();
		SetAttackingWithContactDamage(enable: false);
		base.EnemyController.AlwaysFacing = true;
		yield return Default_Attack_Cooldown(m_throwRib_Exit_ForceIdle, m_throwRib_Exit_AttackCD);
	}

	[CloseLogic]
	[MediumLogic]
	[FarLogic]
	[BasicEnemy]
	[AdvancedEnemy]
	[ExpertEnemy]
	[MinibossEnemy]
	public IEnumerator Dance_Attack()
	{
		StopAndFaceTarget();
		base.EnemyController.AlwaysFacing = false;
		SetAttackingWithContactDamage(enable: true);
		yield return Default_Animation("Victory", m_dance_AttackIntro_AnimationSpeed, m_dance_AttackIntro_Delay, waitUntilAnimationComplete: false);
		float y = base.EnemyController.Room.Bounds.max.y + 10f;
		float num = 5f;
		Vector2 vector = new Vector2(PlayerManager.GetPlayerController().Midpoint.x, y);
		Vector2 pos3;
		Vector2 pos2;
		Vector2 pos;
		Vector2 pos4 = (pos3 = (pos2 = (pos = vector)));
		pos4.y = y;
		pos3.x += num;
		pos2.x += 0f - num;
		pos.x += num * 2f;
		vector.x += (0f - num) * 2f;
		pos3.y -= 2f;
		pos2.y -= 2f;
		pos4.y -= 4f;
		AudioManager.PlayOneShot(this, "event:/SFX/Enemies/sfx_enemy_skeletonBoss_portal_multiple", PlayerManager.GetPlayerController().Midpoint);
		this.FireProjectileAbsPos("SkeletonCannonBallWarningProjectile", pos4, matchFacing: false, 270f);
		this.FireProjectileAbsPos("SkeletonCannonBallWarningProjectile", pos3, matchFacing: false, 270f);
		this.FireProjectileAbsPos("SkeletonCannonBallWarningProjectile", pos2, matchFacing: false, 270f);
		this.FireProjectileAbsPos("SkeletonCannonBallWarningProjectile", pos, matchFacing: false, 270f);
		this.FireProjectileAbsPos("SkeletonCannonBallWarningProjectile", vector, matchFacing: false, 270f);
		Projectile_RL projectile_RL = this.FireProjectileAbsPos("SkeletonCannonBallProjectile", pos4, matchFacing: true, -90f);
		AudioManager.PlayOneShotAttached(this, "event:/SFX/Enemies/sfx_enemy_skeletonBoss_cannon_projectiles_loop", projectile_RL.gameObject);
		this.FireProjectileAbsPos("SkeletonCannonBallProjectile", pos3, matchFacing: true, -90f);
		this.FireProjectileAbsPos("SkeletonCannonBallProjectile", pos2, matchFacing: true, -90f);
		this.FireProjectileAbsPos("SkeletonCannonBallProjectile", pos, matchFacing: true, -90f);
		this.FireProjectileAbsPos("SkeletonCannonBallProjectile", vector, matchFacing: true, -90f);
		yield return Wait(m_dance_AttackHold_Delay);
		yield return this.ChangeAnimationState(BaseAIScript.NEUTRAL_STATE);
		base.EnemyController.AlwaysFacing = true;
		SetAttackingWithContactDamage(enable: false);
		yield return Default_Attack_Cooldown(m_dance_Exit_ForceIdle, m_dance_Exit_AttackCD);
	}

	[CloseLogic]
	[MediumLogic]
	[FarLogic]
	[BasicEnemy]
	[AdvancedEnemy]
	[ExpertEnemy]
	[MinibossEnemy]
	public IEnumerator Jump_Attack()
	{
		StopAndFaceTarget();
		base.EnemyController.AlwaysFacing = false;
		SetAttackingWithContactDamage(enable: true);
		yield return Default_Animation("Jump_Tell", m_jump_Tell_AnimationSpeed, m_jump_Tell_Delay);
		yield return Single_Action_Jump();
		yield return WaitUntilIsGrounded();
		SetAttackingWithContactDamage(enable: false);
		SetVelocityX(0f, additive: false);
		for (int i = 0; i < 2; i++)
		{
			float speedMod = UnityEngine.Random.Range(m_miniBoss_Land_ThrowPower.x, m_miniBoss_Land_ThrowPower.y);
			int num = UnityEngine.Random.Range((int)m_miniBoss_Land_ThrowAngle.x, (int)m_miniBoss_Land_ThrowAngle.y);
			this.FireProjectile("SkeletonBoneMinibossProjectile", 1, matchFacing: true, num, speedMod);
			this.FireProjectile("SkeletonBoneMinibossProjectile", 1, matchFacing: true, 180 - num, speedMod);
			this.FireProjectile("SkeletonBoneMinibossProjectile", 2, matchFacing: true, num, speedMod);
			this.FireProjectile("SkeletonBoneMinibossProjectile", 2, matchFacing: true, 180 - num, speedMod);
			this.FireProjectile("SkeletonBoneMinibossProjectile", 2, matchFacing: true, num, speedMod);
			this.FireProjectile("SkeletonBoneMinibossProjectile", 2, matchFacing: true, 180 - num, speedMod);
		}
		if (m_jump_Land_Spawn_Curse)
		{
			this.FireProjectile("SkeletonBossCurseProjectile", 2, matchFacing: true, 65f);
			this.FireProjectile("SkeletonBossCurseProjectile", 2, matchFacing: true, 115f);
		}
		if (m_jump_Exit_Delay > 0f)
		{
			yield return Wait(m_jump_Exit_Delay);
		}
		base.EnemyController.AlwaysFacing = true;
		yield return Default_Attack_Cooldown(m_jump_Exit_ForceIdle, m_jump_Exit_AttackCD);
	}

	[CloseLogic]
	[MediumLogic]
	[FarLogic]
	[BasicEnemy]
	[AdvancedEnemy]
	[ExpertEnemy]
	[MinibossEnemy]
	public IEnumerator Jump_And_Throw_Bone_Attack()
	{
		StopAndFaceTarget();
		base.EnemyController.AlwaysFacing = false;
		SetAttackingWithContactDamage(enable: true);
		yield return Default_Animation("Jump_Tell", m_jump_Tell_AnimationSpeed, m_jump_Tell_Delay);
		yield return Single_Action_Jump();
		float num = UnityEngine.Random.Range(m_jumpAndThrowBone_ProjectileSpawnDelay.x, m_jumpAndThrowBone_ProjectileSpawnDelay.y);
		if (num > 0f)
		{
			yield return Wait(num);
		}
		if (UnityEngine.Random.Range(0, 2) == 0)
		{
			yield return Single_Attack_Throw_Bone(m_BoneNear_Angle);
		}
		else
		{
			yield return Single_Attack_Throw_Bone(m_FarBoneAngle);
		}
		yield return WaitUntilIsGrounded();
		SetAttackingWithContactDamage(enable: false);
		if (m_jump_Exit_Delay > 0f)
		{
			yield return Wait(m_jump_Exit_Delay);
		}
		base.EnemyController.AlwaysFacing = true;
		yield return Default_Attack_Cooldown(m_jumpAndThrowBone_Exit_ForceIdle, m_jumpAndThrowBone_Exit_AttackCD);
	}

	[CloseLogic]
	[MediumLogic]
	[FarLogic]
	[BasicEnemy]
	[AdvancedEnemy]
	[ExpertEnemy]
	[MinibossEnemy]
	public IEnumerator Jump_And_Throw_Rib_Attack()
	{
		StopAndFaceTarget();
		base.EnemyController.AlwaysFacing = false;
		SetAttackingWithContactDamage(enable: true);
		yield return Default_Animation("Jump_Tell", m_jump_Tell_AnimationSpeed, m_jump_Tell_Delay);
		yield return Single_Action_Jump();
		float num = UnityEngine.Random.Range(m_jumpAndThrowBone_ProjectileSpawnDelay.x, m_jumpAndThrowBone_ProjectileSpawnDelay.y);
		if (num > 0f)
		{
			yield return Wait(num);
		}
		yield return Single_Attack_Throw_Rib();
		yield return WaitUntilIsGrounded();
		SetAttackingWithContactDamage(enable: false);
		if (m_jump_Exit_Delay > 0f)
		{
			yield return Wait(m_jump_Exit_Delay);
		}
		base.EnemyController.AlwaysFacing = true;
		yield return Default_Attack_Cooldown(m_jumpAndThrowBone_Exit_ForceIdle, m_jumpAndThrowBone_Exit_AttackCD);
	}

	public IEnumerator Single_Attack_Throw_Bone(int throwAngle)
	{
		yield return Default_TellIntroAndLoop("ThrowBone_Tell_Intro", m_throwBone_TellIntro_AnimationSpeed, "ThrowBone_Tell_Hold", m_throwBone_TellHold_AnimationSpeed, 0.65f);
		yield return Default_Animation("ThrowBone_Attack_Intro", m_throwBone_AttackIntro_AnimationSpeed, m_throwBone_AttackIntro_Delay);
		for (int i = 0; i < m_miniBoss_Bone_ThrowAmount; i++)
		{
			int num = 5 + i;
			if (num >= 9)
			{
				num = 5;
			}
			throwAngle = UnityEngine.Random.Range((int)m_miniBoss_Bone_ThrowAngle.x, (int)m_miniBoss_Bone_ThrowAngle.y);
			this.FireProjectile("SkeletonBoneMinibossProjectile", num, matchFacing: true, throwAngle);
		}
		yield return Default_Animation("ThrowBone_Attack_Hold", m_throwBone_AttackHold_AnimationSpeed, 0f, waitUntilAnimationComplete: false);
		Wait(0.75f);
		yield return Default_Animation("ThrowBone_Exit", m_throwBone_Exit_AnimationSpeed, m_throwBone_Exit_Delay);
		yield return this.ChangeAnimationState(BaseAIScript.NEUTRAL_STATE);
	}

	public IEnumerator Single_Attack_Throw_Rib()
	{
		yield return Default_TellIntroAndLoop("ChestBone_Tell_Intro", m_throwRib_TellIntro_AnimationSpeed, "ChestBone_Tell_Hold", m_throwRib_TellHold_AnimationSpeed, 0.85f);
		yield return Default_Animation("ChestBone_Attack_Intro", m_throwRib_AttackIntro_AnimationSpeed, m_throwRib_AttackIntro_Delay);
		yield return Default_Animation("ChestBone_Attack_Hold", m_throwRib_AttackHold_AnimationSpeed, 0f, waitUntilAnimationComplete: false);
		for (int i = 0; (float)i < m_miniBoss_Rib_ThrowAmount; i++)
		{
			float miniBoss_Rib_ThrowPower = m_miniBoss_Rib_ThrowPower;
			UnityEngine.Random.Range(m_miniBoss_Rib_ThrowHeight.x, m_miniBoss_Rib_ThrowHeight.y);
			Projectile_RL projectile_RL = this.FireProjectile("SkeletonBoneBigProjectile", 4, matchFacing: true, m_throwRib_Angle, miniBoss_Rib_ThrowPower);
			projectile_RL.RotationSpeed = projectile_RL.InitialRotationSpeed;
			if (m_miniBoss_Rib_LoopDelay > 0f)
			{
				yield return Wait(m_miniBoss_Rib_LoopDelay);
			}
		}
		yield return Wait(m_throwRib_AttackHold_Delay);
		yield return Default_Animation("ChestBone_Exit", m_throwRib_Exit_AnimationSpeed, m_throwRib_Exit_Delay);
		yield return this.ChangeAnimationState(BaseAIScript.NEUTRAL_STATE);
	}

	public IEnumerator Single_Action_Jump()
	{
		if (!base.EnemyController.IsFacingRight)
		{
			SetVelocityX(m_jump_Power.x, additive: true);
		}
		else
		{
			SetVelocityX(0f - m_jump_Power.x, additive: true);
		}
		SetVelocityY(m_jump_Power.y, additive: false);
		yield return this.ChangeAnimationState("JumpUp");
		yield return Wait(0.05f);
	}

	[CloseLogic]
	[MediumLogic]
	[FarLogic]
	[BasicEnemy]
	[AdvancedEnemy]
	[ExpertEnemy]
	[MinibossEnemy]
	public IEnumerator Mode_Shift()
	{
		m_isModeShifting = true;
		this.ToDo("Mode_Shift " + m_modeShiftIndex);
		RemoveStatusEffects(includeCommanderBuffs: false);
		base.EnemyController.StatusEffectController.ImmuneToAllStatusEffects = true;
		yield return base.DeathAnim();
		SetVelocity(0f, 0f, additive: false);
		base.EnemyController.LockFlip = true;
		base.EnemyController.ModeshiftDamageMod = m_modeShift_Damage_Mod;
		yield return Default_Animation("ModeShift_Intro", m_modeShift_Tell_Intro_AnimSpeed, m_modeShift_TellIntro_Delay);
		yield return Default_Animation("ModeShift_Scream_Intro", m_modeShift_Attack_Intro_AnimSpeed, m_modeShift_Attack_Intro_Delay);
		yield return Default_Animation("ModeShift_Scream_Hold", m_modeShift_Attack_Hold_AnimSpeed, m_modeShift_Attack_Hold_Delay);
		ModeShiftComplete_SpawnSecondBoss = true;
		SummonModeShiftBosses(forceSummon: false);
		yield return Default_Animation("ModeShift_Scream_Exit", m_modeShift_Exit_AnimSpeed, m_modeShift_Exit_Delay);
		yield return Default_Animation(BaseAIScript.NEUTRAL_STATE, 1f, 1f, waitUntilAnimationComplete: false);
		base.EnemyController.ModeshiftDamageMod = 1f;
		base.EnemyController.LockFlip = false;
		base.EnemyController.StatusEffectController.ImmuneToAllStatusEffects = false;
		m_isModeShifting = false;
	}

	protected void SummonModeShiftBosses(bool forceSummon)
	{
		if (base.EnemyController.EnemyRank < EnemyRank.Advanced || (!forceSummon && m_modeShiftIndex <= 1 && base.EnemyController.EnemyType != EnemyType.SkeletonBossB))
		{
			return;
		}
		for (int i = 0; i < 2; i++)
		{
			Vector3 vector = i switch
			{
				0 => GetAbsoluteSpawnPositionAtIndex(9, matchFacing: false), 
				1 => GetAbsoluteSpawnPositionAtIndex(10, matchFacing: false), 
				_ => GetAbsoluteSpawnPositionAtIndex(11, matchFacing: false), 
			};
			if (IsCollidingAtSpawnPoint(vector))
			{
				vector.x = base.EnemyController.Midpoint.x;
			}
			EnemyManager.SummonEnemy(base.EnemyController, EnemyType.Skeleton, EnemyRank.Miniboss, vector, useAbsPos: true).DisableCulling = true;
		}
		m_modeShiftSummons_Appeared = true;
	}

	public override IEnumerator SpawnAnim()
	{
		base.EnemyController.Visuals.SetActive(value: false);
		yield return Wait(1.5f);
		yield return this.ChangeAnimationState("Fall");
		Vector3 position = base.EnemyController.transform.position;
		position.y += 18f;
		base.EnemyController.transform.position = position;
		base.EnemyController.Visuals.SetActive(value: true);
		yield return WaitUntilIsGrounded();
		yield return Wait(0.5f);
		yield return Default_Animation("Intro_Idle", m_spawn_Idle_AnimSpeed, m_spawn_Idle_Delay);
		yield return Default_Animation("Intro", m_spawn_Intro_AnimSpeed, m_spawn_Intro_Delay);
		yield return Default_Animation(BaseAIScript.NEUTRAL_STATE, 1f, 1f, waitUntilAnimationComplete: false);
	}

	public override IEnumerator DeathAnim()
	{
		ModeShiftComplete_SpawnSecondBoss = true;
		yield return base.DeathAnim();
		RLTimeScale.SetTimeScale(TimeScaleType.Cutscene, 0.5f);
		yield return Default_Animation("Death_Intro", m_death_Intro_AnimSpeed, m_death_Intro_Delay);
		RLTimeScale.SetTimeScale(TimeScaleType.Cutscene, 1f);
		base.EnemyController.GetComponent<BossModeShiftController>().DissolveMaterials(m_death_Hold_Delay);
		yield return Default_Animation("Death_Loop", m_death_Hold_AnimSpeed, m_death_Hold_Delay);
	}

	public override void Initialize(EnemyController enemyController)
	{
		base.Initialize(enemyController);
		ModeShiftComplete_SpawnSecondBoss = false;
		base.LogicController.DisableLogicActivationByDistance = true;
		m_jump_Power = new Vector2(-11.5f, 45f);
		m_jump_Tell_Delay = 0.55f;
		m_jump_Exit_AttackCD = 6f;
		m_throwBone_AttackCD = 6f;
		m_throwRib_Exit_AttackCD = 6f;
		base.EnemyController.HealthChangeRelay.AddListener(OnBossHit);
	}

	private void OnBossHit(object sender, HealthChangeEventArgs args)
	{
		if (m_isModeShifting || base.EnemyController.IsDead || (base.EnemyController.EnemyType == EnemyType.SkeletonBossB && base.LogicController.EnemyLogicType == EnemyLogicType.Basic) || args.PrevHealthValue <= args.NewHealthValue)
		{
			return;
		}
		float[] array = m_modeShiftLevels_Basic;
		if (base.LogicController.EnemyLogicType == EnemyLogicType.Advanced)
		{
			array = ((base.EnemyController.EnemyType != EnemyType.SkeletonBossA) ? m_modeShiftLevels_Advanced_Skeleton_B : m_modeShiftLevels_Advanced_Skeleton_A);
		}
		if (m_modeShiftIndex >= array.Length)
		{
			return;
		}
		float num = array[m_modeShiftIndex] * (float)base.EnemyController.ActualMaxHealth;
		if (base.EnemyController.CurrentHealth <= num)
		{
			m_modeShiftIndex++;
			base.LogicController.StopAllLogic(resetCurrentState: true);
			base.LogicController.ForceExecuteLogicBlockName_OnceOnly = "Mode_Shift";
			if (m_modeShiftEventArgs == null)
			{
				m_modeShiftEventArgs = new EnemyModeShiftEventArgs(base.EnemyController);
			}
			Messenger<GameMessenger, GameEvent>.Broadcast(GameEvent.EnemyModeShift, this, m_modeShiftEventArgs);
		}
	}

	public override void ResetScript()
	{
		m_isModeShifting = false;
		m_modeShiftSummons_Appeared = false;
		m_modeShiftIndex = 0;
		base.EnemyController.StatusEffectController.ImmuneToAllStatusEffects = false;
		base.ResetScript();
	}

	public override void OnLBCompleteOrCancelled()
	{
		base.EnemyController.AlwaysFacing = true;
		base.OnLBCompleteOrCancelled();
	}

	public bool IsCollidingAtSpawnPoint(Vector3 spawnPoint)
	{
		int layerMask = (int)base.EnemyController.ControllerCorgi.PlatformMask & ~(int)base.EnemyController.ControllerCorgi.OneWayPlatformMask;
		if ((bool)Physics2D.OverlapBox(spawnPoint, Vector2.one, 0f, layerMask))
		{
			return true;
		}
		return false;
	}
}
