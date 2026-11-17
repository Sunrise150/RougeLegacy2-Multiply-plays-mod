using System;
using System.Collections;
using UnityEngine;

public class TopShotHazard_Basic_AIScript : BaseAIScript
{
	[SerializeField]
	private bool m_hookToOneWays;

	private int m_triggerShotMask;

	private bool m_aimFollowTarget = true;

	private float m_headAimAngle;

	private float m_reaimStartTime;

	private Action<MonoBehaviour, EventArgs> m_onPlayerEnterRoom;

	private Action<object, EventArgs> m_onResetPosition;

	private Action<object, EventArgs> m_onPositionedForSummoning;

	private const float RANGE_WIDTH = 18f;

	protected const string SINGLE_SHOT_TELL_INTRO = "SingleShot_Tell_Intro";

	protected const string SINGLE_SHOT_TELL_HOLD = "SingleShot_Tell_Hold";

	protected const string SINGLE_SHOT_ATTACK_INTRO = "SingleShot_Attack_Intro";

	protected const string SINGLE_SHOT_ATTACK_HOLD = "SingleShot_Attack_Hold";

	protected const string SINGLE_SHOT_EXIT = "SingleShot_Exit";

	protected const string CONTINUOUS_SHOT_TELL_INTRO = "ContinuousShot_Tell_Intro";

	protected const string CONTINUOUS_SHOT_TELL_HOLD = "ContinuousShot_Tell_Hold";

	protected const string CONTINUOUS_SHOT_ATTACK_INTRO = "ContinuousShot_Attack_Intro";

	protected const string CONTINUOUS_SHOT_ATTACK_HOLD = "ContinuousShot_Attack_Hold";

	protected const string CONTINUOUS_SHOT_EXIT = "ContinuousShot_Exit";

	protected const string SINGLE_SHOT_PROJECTILE = "TopShotBoltProjectile";

	protected const string SINGLE_SHOT_PROJECTILE_EXPLOSION = "TopShotBoltExplosionProjectile";

	protected const string SINGLE_SHOT_PROJECTILE_MINIBOSS = "TopShotBoltMinibossProjectile";

	protected float m_fireBullet_TellIntro_AnimationSpeed = 1f;

	protected float m_fireBullet_TellHold_AnimationSpeed = 1f;

	protected const float m_fireBullet_TellIntroAndHold_Delay = 0.6f;

	protected float m_fireBullet_AttackIntro_AnimationSpeed = 1f;

	protected float m_fireBullet_AttackIntro_Delay;

	protected float m_fireBullet_AttackHold_AnimationSpeed = 1f;

	protected const float m_fireBullet_AttackHold_Delay = 0f;

	protected float m_fireBullet_Exit_AnimationSpeed = 0.65f;

	protected float m_fireBullet_Exit_Delay = 0.15f;

	protected float m_spreadShot_TellIntro_AnimationSpeed = 1f;

	protected float m_spreadShot_TellHold_AnimationSpeed = 1f;

	protected const float m_spreadShot_TellIntroAndHold_Delay = 0.6f;

	protected float m_spreadShot_AttackIntro_AnimationSpeed = 1f;

	protected float m_spreadShot_AttackIntro_Delay;

	protected float m_spreadShot_AttackHold_AnimationSpeed = 1f;

	protected const float m_spreadShot_AttackHold_Delay = 0f;

	protected float m_spreadShot_Exit_AnimationSpeed = 0.65f;

	protected float m_spreadShot_Exit_Delay = 0.15f;

	protected virtual float m_hookToWall_MaxDistance => 20f;

	protected virtual float SHOT_TRIGGER_WIDTH => 8f;

	protected virtual bool m_snapToFloor => false;

	protected override Vector2 IdleDuration => new Vector2(0.25f, 0.7f);

	protected override Vector2 WalkTowardsDuration => new Vector2(0.45f, 0.85f);

	protected override Vector2 WalkAwayDuration => new Vector2(0.45f, 0.85f);

	protected virtual float m_fireBullet_Exit_ForceIdle => 0f;

	protected virtual float m_fireBullet_Exit_AttackCD => 0.15f;

	protected virtual bool m_fireBullet_TargetPlayer => true;

	protected virtual float m_fireBullet_InitialAngle => 90f;

	protected virtual float m_fireBullet_SpreadAngle => 15f;

	protected virtual float m_fireBullet_AdditionalSpreadBullets => 1f;

	protected virtual float m_fireBullet_ShotLoop => 1f;

	protected virtual float m_fireBullet_ShotLoopDelay => 0.5f;

	protected virtual float m_spreadShot_Exit_ForceIdle => 0f;

	protected virtual float m_spreadShot_Exit_AttackCD => 0.15f;

	protected virtual bool m_spreadShot_TargetPlayer => true;

	protected virtual float m_spreadShot_InitialAngle => 90f;

	protected virtual float m_spreadShot_SpreadAngle => 15f;

	protected virtual float m_spreadShot_AdditionalSpreadBullets => 0f;

	protected virtual float m_spreadShot_ShotLoop => 2f;

	protected virtual float m_spreadShot_ShotLoopDelay => 0.35f;

	protected override void InitializeProjectileNameArray()
	{
		m_projectileNameArray = new string[3] { "TopShotBoltProjectile", "TopShotBoltExplosionProjectile", "TopShotBoltMinibossProjectile" };
	}

	private void Awake()
	{
		m_onResetPosition = OnResetPosition;
		m_onPositionedForSummoning = OnPositionedForSummoning;
	}

	public override void Initialize(EnemyController enemyController)
	{
		base.Initialize(enemyController);
		m_onPlayerEnterRoom = OnPlayerEnterRoom;
		base.EnemyController.ControllerCorgi.DefaultParameters.Gravity = 0f;
		base.LogicController.ExecuteLogicInAir = true;
		base.EnemyController.PreserveRotationWhenDeactivated = true;
		base.EnemyController.ForceDisableSummonOffset = true;
		base.LogicController.IsInRange = IsInRange;
		HookToWall();
		m_triggerShotMask = 256;
		base.EnemyController.OnResetPositionRelay.AddListener(m_onResetPosition);
		base.EnemyController.OnPositionedForSummoningRelay.AddListener(m_onPositionedForSummoning);
		m_aimFollowTarget = false;
		m_headAimAngle = -90f;
	}

	private void OnResetPosition(object sender, EventArgs args)
	{
		Messenger<GameMessenger, GameEvent>.AddListener(GameEvent.PlayerEnterRoom, m_onPlayerEnterRoom);
	}

	private void OnPositionedForSummoning(object sender, EventArgs args)
	{
		HookToWall();
	}

	private void OnPlayerEnterRoom(object sender, EventArgs args)
	{
		HookToWall();
		Messenger<GameMessenger, GameEvent>.RemoveListener(GameEvent.PlayerEnterRoom, m_onPlayerEnterRoom);
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		m_aimFollowTarget = false;
		m_headAimAngle = -90f;
	}

	public override void OnEnemyActivated()
	{
		m_reaimStartTime = Time.time;
		m_aimFollowTarget = true;
		base.OnEnemyActivated();
	}

	public override void Unpause()
	{
		m_reaimStartTime = Time.time;
		base.Unpause();
	}

	private void OnDestroy()
	{
		if ((bool)base.EnemyController)
		{
			base.EnemyController.OnResetPositionRelay.RemoveListener(m_onResetPosition);
			base.EnemyController.OnPositionedForSummoningRelay.RemoveListener(m_onPositionedForSummoning);
		}
	}

	private void HookToWall()
	{
		LayerMask layerMask = 256;
		if (m_hookToOneWays)
		{
			layerMask = (int)layerMask | 0x800;
		}
		Vector2 direction = ((!m_snapToFloor) ? Vector2.up : Vector2.down);
		RaycastHit2D raycastHit2D = Physics2D.Raycast(base.EnemyController.Midpoint, direction, m_hookToWall_MaxDistance, layerMask);
		bool flag = (bool)raycastHit2D.collider && raycastHit2D.collider.CompareTag("Hazard");
		bool flag2 = (bool)raycastHit2D.collider && raycastHit2D.collider.CompareTag("OneWay");
		base.EnemyController.transform.localEulerAngles = Vector3.zero;
		if ((bool)raycastHit2D && !flag && !flag2)
		{
			Vector3 localEulerAngles = base.EnemyController.Pivot.transform.localEulerAngles;
			localEulerAngles.z = ((!m_snapToFloor) ? 180 : 0);
			base.EnemyController.Pivot.transform.localEulerAngles = localEulerAngles;
			Vector3 position = base.EnemyController.transform.position;
			position.y = raycastHit2D.point.y;
			if (!m_snapToFloor)
			{
				position.y -= base.EnemyController.VisualBounds.size.y;
			}
			base.EnemyController.transform.position = position;
			float num = 270f - CDGHelper.VectorToAngle(raycastHit2D.normal);
			Vector3 zero = Vector3.zero;
			zero.z = 0f - num;
			base.EnemyController.transform.localEulerAngles = zero;
		}
		else
		{
			EnemySpawnController enemySpawnController = base.EnemyController.EnemySpawnController;
			if ((bool)enemySpawnController && !m_snapToFloor)
			{
				enemySpawnController.RemoveListeners();
				base.EnemyController.gameObject.SetActive(value: false);
				enemySpawnController.Override = false;
				enemySpawnController.SetEnemy(EnemyType.Eyeball, base.EnemyController.EnemyRank, base.EnemyController.Level);
				enemySpawnController.InitializeEnemyInstance();
			}
		}
		Vector3 relativeSpawnPositionAtIndex = GetRelativeSpawnPositionAtIndex(2, matchFacing: false);
		relativeSpawnPositionAtIndex.z = -1f;
		base.EnemyController.StatusBarController.gameObject.transform.localPosition = relativeSpawnPositionAtIndex;
		base.EnemyController.ControllerCorgi.SetRaysParameters();
	}

	private bool IsInRange(float rangeRadius)
	{
		Rect rect = default(Rect);
		rect.x = base.EnemyController.Midpoint.x - 9f;
		rect.y = base.EnemyController.Midpoint.y;
		if (!m_snapToFloor)
		{
			rect.y -= rangeRadius;
		}
		rect.width = 18f;
		rect.height = rangeRadius;
		Vector2 vector = PlayerManager.GetPlayerController().Midpoint;
		if (rect.Contains(vector))
		{
			float distance = CDGHelper.DistanceBetweenPts(base.EnemyController.Midpoint, vector);
			Vector2 direction = CDGHelper.VectorBetweenPts(base.EnemyController.Midpoint, vector);
			direction.Normalize();
			if (!Physics2D.Raycast(base.EnemyController.Midpoint, direction, distance, m_triggerShotMask))
			{
				return true;
			}
		}
		return false;
	}

	[CloseLogic]
	[MediumLogic]
	[FarLogic]
	[WanderLogic]
	[BasicEnemy]
	[AdvancedEnemy]
	[ExpertEnemy]
	[MinibossEnemy]
	public IEnumerator ShootFireball()
	{
		string tellIntroName = "SingleShot_Tell_Intro";
		string tellHoldName = "SingleShot_Tell_Hold";
		string attackIntro = "SingleShot_Attack_Intro";
		string attackHold = "SingleShot_Attack_Hold";
		string attackExit = "SingleShot_Exit";
		if (m_fireBullet_ShotLoop > 1f)
		{
			tellIntroName = "ContinuousShot_Tell_Intro";
			tellHoldName = "ContinuousShot_Tell_Hold";
			attackIntro = "ContinuousShot_Attack_Intro";
			attackHold = "ContinuousShot_Attack_Hold";
			attackExit = "ContinuousShot_Exit";
		}
		yield return Default_TellIntroAndLoop(tellIntroName, m_fireBullet_TellIntro_AnimationSpeed, tellHoldName, m_fireBullet_TellHold_AnimationSpeed, 0.6f);
		yield return Default_Animation(attackIntro, m_fireBullet_AttackIntro_AnimationSpeed, m_fireBullet_AttackIntro_Delay);
		yield return Default_Animation(attackHold, m_fireBullet_AttackHold_AnimationSpeed, 0f, waitUntilAnimationComplete: false);
		float initialFireAngle = ((!m_fireBullet_TargetPlayer) ? m_fireBullet_InitialAngle : CDGHelper.VectorToAngle(base.EnemyController.TargetController.Midpoint - base.EnemyController.Midpoint));
		if (base.EnemyController.Pivot.transform.localEulerAngles.z == 180f && !m_fireBullet_TargetPlayer)
		{
			initialFireAngle = 0f - initialFireAngle;
		}
		int projectileSpawnPoint = 1;
		if (base.LogicController.EnemyLogicType == EnemyLogicType.Miniboss)
		{
			projectileSpawnPoint = 0;
		}
		for (int i = 0; (float)i < m_fireBullet_ShotLoop; i++)
		{
			EffectManager.PlayEffect(base.EnemyController.gameObject, base.Animator, "PoisonProjectileHit_Effect", base.EnemyController.Midpoint, 0f, EffectStopType.Gracefully).transform.localEulerAngles = Vector3.forward * (-90f + m_headAimAngle);
			if (m_fireBullet_TargetPlayer)
			{
				initialFireAngle = CDGHelper.VectorToAngle(base.EnemyController.TargetController.Midpoint - base.EnemyController.Midpoint);
			}
			Vector2 thePoint = GetAbsoluteSpawnPositionAtIndex(projectileSpawnPoint, matchFacing: false);
			thePoint = CDGHelper.RotatedPoint(thePoint, base.EnemyController.Midpoint, 90f + m_headAimAngle);
			if (base.LogicController.EnemyLogicType == EnemyLogicType.Miniboss)
			{
				this.FireProjectileAbsPos("TopShotBoltMinibossProjectile", thePoint, matchFacing: false, initialFireAngle);
			}
			else
			{
				this.FireProjectileAbsPos("TopShotBoltProjectile", thePoint, matchFacing: false, initialFireAngle);
			}
			for (int j = 1; (float)j < m_fireBullet_AdditionalSpreadBullets + 1f; j++)
			{
				if (base.LogicController.EnemyLogicType == EnemyLogicType.Miniboss)
				{
					this.FireProjectileAbsPos("TopShotBoltMinibossProjectile", thePoint, matchFacing: false, initialFireAngle + m_fireBullet_SpreadAngle * (float)j);
					this.FireProjectileAbsPos("TopShotBoltMinibossProjectile", thePoint, matchFacing: false, initialFireAngle - m_fireBullet_SpreadAngle * (float)j);
				}
				else
				{
					this.FireProjectileAbsPos("TopShotBoltProjectile", thePoint, matchFacing: false, initialFireAngle + m_fireBullet_SpreadAngle * (float)j);
					this.FireProjectileAbsPos("TopShotBoltProjectile", thePoint, matchFacing: false, initialFireAngle - m_fireBullet_SpreadAngle * (float)j);
				}
			}
			if (m_fireBullet_ShotLoopDelay > 0f)
			{
				yield return Wait(m_fireBullet_ShotLoopDelay);
			}
		}
		yield return Default_Animation(attackExit, m_fireBullet_Exit_AnimationSpeed, m_fireBullet_Exit_Delay);
		yield return this.ChangeAnimationState(BaseAIScript.NEUTRAL_STATE);
		yield return Default_Attack_Cooldown(m_fireBullet_Exit_ForceIdle, m_fireBullet_Exit_AttackCD);
	}

	[CloseLogic]
	[MediumLogic]
	[FarLogic]
	[WanderLogic]
	[BasicEnemy]
	[AdvancedEnemy]
	[ExpertEnemy]
	[MinibossEnemy]
	public IEnumerator SpreadShot()
	{
		string tellIntroName = "SingleShot_Tell_Intro";
		string tellHoldName = "SingleShot_Tell_Hold";
		string attackIntro = "SingleShot_Attack_Intro";
		string attackHold = "SingleShot_Attack_Hold";
		string attackExit = "SingleShot_Exit";
		if (m_spreadShot_ShotLoop > 1f)
		{
			tellIntroName = "ContinuousShot_Tell_Intro";
			tellHoldName = "ContinuousShot_Tell_Hold";
			attackIntro = "ContinuousShot_Attack_Intro";
			attackHold = "ContinuousShot_Attack_Hold";
			attackExit = "ContinuousShot_Exit";
		}
		yield return Default_TellIntroAndLoop(tellIntroName, m_spreadShot_TellIntro_AnimationSpeed, tellHoldName, m_spreadShot_TellHold_AnimationSpeed, 0.6f);
		yield return Default_Animation(attackIntro, m_spreadShot_AttackIntro_AnimationSpeed, m_spreadShot_AttackIntro_Delay);
		yield return Default_Animation(attackHold, m_spreadShot_AttackHold_AnimationSpeed, 0f, waitUntilAnimationComplete: false);
		float initialFireAngle = ((!m_spreadShot_TargetPlayer) ? m_spreadShot_InitialAngle : CDGHelper.VectorToAngle(base.EnemyController.TargetController.Midpoint - base.EnemyController.Midpoint));
		if (base.EnemyController.Pivot.transform.localEulerAngles.z == 180f && !m_spreadShot_TargetPlayer)
		{
			initialFireAngle = 0f - initialFireAngle;
		}
		int projectileSpawnPoint = 1;
		if (base.LogicController.EnemyLogicType == EnemyLogicType.Miniboss)
		{
			projectileSpawnPoint = 0;
		}
		for (int i = 0; (float)i < m_spreadShot_ShotLoop; i++)
		{
			EffectManager.PlayEffect(base.EnemyController.gameObject, base.Animator, "PoisonProjectileHit_Effect", base.EnemyController.Midpoint, 0f, EffectStopType.Gracefully).transform.localEulerAngles = Vector3.forward * (-90f + m_headAimAngle);
			if (m_spreadShot_TargetPlayer)
			{
				initialFireAngle = CDGHelper.VectorToAngle(base.EnemyController.TargetController.Midpoint - base.EnemyController.Midpoint);
			}
			Vector2 thePoint = GetAbsoluteSpawnPositionAtIndex(projectileSpawnPoint, matchFacing: false);
			thePoint = CDGHelper.RotatedPoint(thePoint, base.EnemyController.Midpoint, 90f + m_headAimAngle);
			if (base.LogicController.EnemyLogicType == EnemyLogicType.Miniboss)
			{
				this.FireProjectileAbsPos("TopShotBoltMinibossProjectile", thePoint, matchFacing: false, initialFireAngle);
			}
			else
			{
				this.FireProjectileAbsPos("TopShotBoltProjectile", thePoint, matchFacing: false, initialFireAngle);
			}
			for (int j = 0; (float)j < m_spreadShot_AdditionalSpreadBullets; j++)
			{
				if (base.LogicController.EnemyLogicType == EnemyLogicType.Miniboss)
				{
					this.FireProjectileAbsPos("TopShotBoltMinibossProjectile", thePoint, matchFacing: false, initialFireAngle + m_spreadShot_SpreadAngle * (float)j);
					this.FireProjectileAbsPos("TopShotBoltMinibossProjectile", thePoint, matchFacing: false, initialFireAngle - m_spreadShot_SpreadAngle * (float)j);
				}
				else
				{
					this.FireProjectileAbsPos("TopShotBoltProjectile", thePoint, matchFacing: false, initialFireAngle + m_spreadShot_SpreadAngle * (float)j);
					this.FireProjectileAbsPos("TopShotBoltProjectile", thePoint, matchFacing: false, initialFireAngle - m_spreadShot_SpreadAngle * (float)j);
				}
			}
			if (m_spreadShot_ShotLoopDelay > 0f)
			{
				yield return Wait(m_spreadShot_ShotLoopDelay);
			}
		}
		yield return Default_Animation(attackExit, m_spreadShot_Exit_AnimationSpeed, m_spreadShot_Exit_Delay);
		yield return this.ChangeAnimationState(BaseAIScript.NEUTRAL_STATE);
		yield return Default_Attack_Cooldown(m_spreadShot_Exit_ForceIdle, m_spreadShot_Exit_AttackCD);
	}

	private void FixedUpdate()
	{
		if (base.IsInitialized && !base.IsPaused && m_aimFollowTarget && (bool)base.EnemyController)
		{
			float num = CDGHelper.WrapAngleDegrees(CDGHelper.AngleBetweenPts(base.EnemyController.Midpoint, base.EnemyController.TargetController.Midpoint), convertToHalfRadius: true);
			float num2 = 1f;
			float num3 = m_reaimStartTime + num2;
			if (Time.time < num3)
			{
				m_headAimAngle = Mathf.Abs(Mathf.LerpAngle(m_headAimAngle, num, 1f - (num3 - Time.time / num2))) * -1f;
			}
			else
			{
				m_headAimAngle = num;
			}
			m_headAimAngle = Mathf.Clamp(m_headAimAngle, -135f, -45f);
			float t = (m_headAimAngle + 45f) / -90f;
			float value = Mathf.Lerp(1f, -1f, t);
			base.EnemyController.Animator.SetFloat("LookAngle", value);
		}
	}
}
