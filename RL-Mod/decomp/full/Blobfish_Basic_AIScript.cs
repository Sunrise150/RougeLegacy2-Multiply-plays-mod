using System;
using System.Collections;
using RLAudio;
using UnityEngine;

public class Blobfish_Basic_AIScript : BaseAIScript
{
	[SerializeField]
	private SpriteRenderer m_aimIndicator;

	private Action<object, CharacterHitEventArgs> m_onHitReenableGravity;

	protected const string JUMP_ATTACK_TELL_INTRO = "Jump_Tell_Intro";

	protected const string JUMP_ATTACK_TELL_HOLD = "Jump_Tell_Hold";

	protected const string JUMP_ATTACK_ATTACK_INTRO = "Jump_Attack_Intro";

	protected const string JUMP_ATTACK_ATTACK_HOLD = "Jump_Attack_Hold";

	protected const string JUMP_ATTACK_EXIT = "Jump_Exit";

	protected const string WATER_PROJECTILE = "BlobFishWaterProjectile";

	protected const string TAIL_PROJECTILE = "BlobFishTailProjectile";

	protected const string TAIL_LONG_PROJECTILE = "BlobFishTailLongProjectile";

	protected Vector2 m_jump_Direction;

	protected float m_jump_Attack_Intro_AnimationSpeed = 1f;

	protected float m_jump_Attack_Intro_Delay;

	protected float m_jump_Attack_Hold_AnimationSpeed = 1f;

	protected float m_jump_Attack_Hold_Delay;

	protected float m_jump_Exit_AnimationSpeed = 1f;

	protected float m_jump_Exit_Delay;

	protected float m_jump_Exit_AttackCD;

	protected const string GROUND_SLAM_TELL_INTRO = "Flop_Tell_Intro";

	protected const string GROUND_SLAM_TELL_HOLD = "Flop_Tell_Hold";

	protected const string GROUND_SLAM_ATTACK_INTRO = "Flop_Attack_Intro";

	protected const string GROUND_SLAM_ATTACK_HOLD = "Flop_Attack_Hold";

	protected const string GROUND_SLAM_EXIT = "Flop_Exit";

	protected const string SPLASH_PROJECTILE = "BlobFishWaterSlamProjectile";

	protected Vector2 GroundSlam_Land_ThrowAngle = new Vector2(15f, 7f);

	protected Vector2 GroundSlam_Land_ThrowPower = new Vector2(0.75f, 1f);

	private Coroutine m_groundSlamBlinkCoroutine;

	protected override Vector2 IdleDuration => new Vector2(0.25f, 0.7f);

	protected override Vector2 WalkTowardsDuration => new Vector2(0.45f, 0.85f);

	protected override Vector2 WalkAwayDuration => new Vector2(0.45f, 0.85f);

	protected virtual bool HasTail => false;

	protected virtual float m_jump_Tell_Delay => 0.3f;

	protected virtual float m_jump_Tell_AnimationSpeed => 1f;

	protected virtual float m_jump_Exit_ForceIdle => 0.15f;

	protected virtual bool m_jumpLandingShootsProjectiles => true;

	protected virtual bool m_jumpLandingShootsManyProjectiles => true;

	protected virtual int m_jumpLandingProjectileAngle => 80;

	protected virtual int m_jumpLandingProjectileAngleMany => 70;

	protected virtual float m_jumpProjectileFireDelay => 0.1f;

	protected virtual float m_aim_Indicator_Offset => 2f;

	protected virtual Vector2Int m_jump_Attack_Min_And_Max_Angle => new Vector2Int(65, 125);

	protected virtual float m_jump_Power => 27.5f;

	protected virtual float GroundSlam_Jump_Power => 27.5f;

	protected virtual float GroundSlam_Land_Power => -50f;

	protected virtual float GroundSlam_TellIntro_AnimSpeed => 1f;

	protected virtual float GroundSlam_TellHold_AnimSpeed => 1.75f;

	protected virtual float GroundSlam_TellHold_Duration => 2f;

	protected virtual float GroundSlam_AttackIntro_AnimSpeed => 2f;

	protected virtual float GroundSlam_AttackIntro_Delay => 0f;

	protected virtual float GroundSlam_AttackHold_AnimSpeed => 1f;

	protected virtual float GroundSlam_AttackHold_Delay => 0f;

	protected virtual float GroundSlam_Exit_AnimSpeed => 1.25f;

	protected virtual float GroundSlam_Exit_Duration => 0.2f;

	protected virtual float GroundSlam_Exit_ForceIdle => 0.15f;

	protected virtual float GroundSlam_Exit_AttackCD => 2.75f;

	protected float GroundSlam_Land_Delay => 0.5f;

	protected override void InitializeProjectileNameArray()
	{
		m_projectileNameArray = new string[4] { "BlobFishWaterProjectile", "BlobFishTailProjectile", "BlobFishTailLongProjectile", "BlobFishWaterSlamProjectile" };
	}

	private void Awake()
	{
		m_onHitReenableGravity = OnHitReenableGravity;
	}

	public override void Initialize(EnemyController enemyController)
	{
		base.Initialize(enemyController);
		HideAimIndicator();
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		HideAimIndicator();
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
		SetRandomJumpDirection();
		base.Animator.SetBool("BellyFlop", value: false);
		yield return Default_Animation("Jump_Tell_Intro", m_jump_Tell_AnimationSpeed, m_jump_Tell_Delay);
		yield return Default_Animation("Jump_Tell_Hold", m_jump_Tell_AnimationSpeed, m_jump_Tell_Delay);
		SetAttackingWithContactDamage(enable: true);
		Jump(m_jump_Direction.x, m_jump_Direction.y);
		HideAimIndicator();
		yield return Wait(0.05f);
		if (HasTail)
		{
			this.RunPersistentCoroutine(FireJumpProjectiles(m_jumpProjectileFireDelay));
		}
		yield return Default_Animation("Jump_Attack_Intro", m_jump_Attack_Intro_AnimationSpeed, m_jump_Attack_Intro_Delay);
		yield return Default_Animation("Jump_Attack_Hold", m_jump_Attack_Hold_AnimationSpeed, m_jump_Attack_Hold_Delay);
		yield return Wait(0.05f);
		yield return WaitUntilIsGrounded();
		SetVelocity(Vector2.zero, additive: false);
		base.EnemyController.JumpHorizontalVelocity = 0f;
		if (m_jumpLandingShootsProjectiles)
		{
			this.FireProjectile("BlobFishWaterProjectile", 2, matchFacing: true, m_jumpLandingProjectileAngle);
			this.FireProjectile("BlobFishWaterProjectile", 1, matchFacing: true, 180 - m_jumpLandingProjectileAngle);
			if (m_jumpLandingShootsManyProjectiles)
			{
				this.FireProjectile("BlobFishWaterProjectile", 2, matchFacing: true, m_jumpLandingProjectileAngleMany);
				this.FireProjectile("BlobFishWaterProjectile", 1, matchFacing: true, 180 - m_jumpLandingProjectileAngleMany);
			}
		}
		yield return Default_Animation("Jump_Exit", m_jump_Exit_AnimationSpeed, m_jump_Exit_Delay);
		SetAttackingWithContactDamage(enable: false);
		if (m_jump_Exit_Delay > 0f)
		{
			Wait(m_jump_Exit_Delay);
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
	public IEnumerator Ground_Slam()
	{
		base.EnemyController.AlwaysFacing = false;
		SetVelocityX(0f, additive: false);
		float totalDuration = GroundSlam_TellHold_Duration * 0.3f;
		float blinkDuration = GroundSlam_TellHold_Duration * 0.7f;
		base.Animator.SetBool("BellyFlop", value: true);
		yield return Default_TellIntroAndLoop("Jump_Tell_Intro", GroundSlam_TellIntro_AnimSpeed, "Jump_Tell_Hold", GroundSlam_TellHold_AnimSpeed, totalDuration);
		Color red = Color.red;
		red.a = 0.5f;
		m_groundSlamBlinkCoroutine = this.RunPersistentCoroutine(GroundSlamBlinkCoroutine(blinkDuration, red));
		yield return Wait(blinkDuration);
		SetAttackingWithContactDamage(enable: true);
		Jump(0f, GroundSlam_Jump_Power);
		yield return Wait(0.05f);
		if (HasTail)
		{
			this.RunPersistentCoroutine(FireJumpProjectiles(m_jumpProjectileFireDelay));
		}
		yield return Default_Animation("Jump_Attack_Intro", 1f, 0f);
		yield return Default_Animation("Jump_Attack_Hold", 1f, 0f);
		yield return Wait(0.05f);
		while (base.EnemyController.Velocity.y > 0f)
		{
			yield return null;
		}
		base.EnemyController.SetVelocity(0f, 0f, additive: false);
		base.EnemyController.ControllerCorgi.GravityActive(state: false);
		base.EnemyController.CharacterHitResponse.OnCharacterHitRelay.AddListener(m_onHitReenableGravity);
		yield return Default_Animation("Flop_Attack_Intro", GroundSlam_AttackIntro_AnimSpeed, GroundSlam_AttackIntro_Delay);
		yield return Default_Animation("Flop_Attack_Hold", GroundSlam_AttackHold_AnimSpeed, GroundSlam_AttackHold_Delay, waitUntilAnimationComplete: false);
		yield return Wait(GroundSlam_Land_Delay);
		base.EnemyController.ControllerCorgi.GravityActive(state: true);
		SetVelocityY(GroundSlam_Land_Power, additive: false);
		base.EnemyController.CharacterHitResponse.OnCharacterHitRelay.RemoveListener(m_onHitReenableGravity);
		yield return WaitUntilIsGrounded();
		this.StopPersistentCoroutine(m_groundSlamBlinkCoroutine);
		SetVelocity(Vector2.zero, additive: false);
		base.EnemyController.JumpHorizontalVelocity = 0f;
		for (int i = 0; i < 2; i++)
		{
			UnityEngine.Random.Range(GroundSlam_Land_ThrowPower.x, GroundSlam_Land_ThrowPower.y);
			float speedMod = 1f;
			int num = UnityEngine.Random.Range((int)GroundSlam_Land_ThrowAngle.x, (int)GroundSlam_Land_ThrowAngle.y);
			this.FireProjectile("BlobFishWaterSlamProjectile", 5, matchFacing: false, num, speedMod);
			this.FireProjectile("BlobFishWaterSlamProjectile", 5, matchFacing: false, 180 - num, speedMod);
			this.FireProjectile("BlobFishWaterSlamProjectile", 6, matchFacing: false, num, speedMod);
			this.FireProjectile("BlobFishWaterSlamProjectile", 6, matchFacing: false, 180 - num, speedMod);
		}
		yield return Default_Animation("Flop_Exit", GroundSlam_Exit_AnimSpeed, GroundSlam_Exit_Duration);
		SetAttackingWithContactDamage(enable: false);
		base.EnemyController.AlwaysFacing = true;
		yield return Default_Attack_Cooldown(GroundSlam_Exit_ForceIdle, GroundSlam_Exit_AttackCD);
	}

	private void OnHitReenableGravity(object sender, EventArgs args)
	{
		base.EnemyController.ControllerCorgi.GravityActive(state: true);
	}

	private IEnumerator GroundSlamBlinkCoroutine(float duration, Color color)
	{
		while (true)
		{
			float blinkDuration = Time.time + base.EnemyController.BlinkPulseEffect.SingleBlinkDuration * 2f;
			base.EnemyController.BlinkPulseEffect.StartSingleBlinkEffect(color);
			while (Time.time < blinkDuration)
			{
				yield return null;
			}
		}
	}

	private void Jump(float x, float y)
	{
		SetVelocityX(x, additive: true);
		base.EnemyController.JumpHorizontalVelocity = x;
		SetVelocityY(y, additive: false);
	}

	private void SetRandomJumpDirection()
	{
		Vector2 vector = CDGHelper.AngleToVector(UnityEngine.Random.Range(m_jump_Attack_Min_And_Max_Angle.x, m_jump_Attack_Min_And_Max_Angle.y + 1));
		m_jump_Direction = m_jump_Power * vector;
		DrawAimIndicator(vector);
	}

	private IEnumerator FireJumpProjectiles(float delayBetweenProjectiles)
	{
		float pitch = 0f;
		while (!base.EnemyController.IsGrounded)
		{
			float delay = Time.time + delayBetweenProjectiles;
			while (Time.time < delay || base.IsPaused)
			{
				yield return null;
			}
			Projectile_RL projectile_RL = ((base.EnemyController.EnemyRank >= EnemyRank.Expert) ? this.FireProjectile("BlobFishTailLongProjectile", Vector2.zero, matchFacing: false) : this.FireProjectile("BlobFishTailProjectile", Vector2.zero, matchFacing: false));
			projectile_RL.GetComponent<BubbleProjectileAudioController>().PlaySpawnAudio(pitch);
			pitch = Mathf.Min(pitch + 0.05f, 1f);
		}
	}

	private void DrawAimIndicator(Vector2 direction)
	{
		m_aimIndicator.transform.rotation = Quaternion.LookRotation(Vector3.forward, m_jump_Direction);
		Vector3 localEulerAngles = m_aimIndicator.transform.localEulerAngles;
		localEulerAngles.z += 90f;
		m_aimIndicator.transform.localEulerAngles = localEulerAngles;
		m_aimIndicator.transform.localPosition = m_aim_Indicator_Offset * direction;
		m_aimIndicator.gameObject.SetActive(value: true);
	}

	private void HideAimIndicator()
	{
		m_aimIndicator.gameObject.SetActive(value: false);
	}

	public override void OnLBCompleteOrCancelled()
	{
		base.EnemyController.CharacterHitResponse.OnCharacterHitRelay.RemoveListener(m_onHitReenableGravity);
		base.EnemyController.ControllerCorgi.GravityActive(state: true);
		this.StopPersistentCoroutine(m_groundSlamBlinkCoroutine);
		base.OnLBCompleteOrCancelled();
	}
}
