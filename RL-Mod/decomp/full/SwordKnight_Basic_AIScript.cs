using System;
using System.Collections;
using UnityEngine;

public class SwordKnight_Basic_AIScript : BaseAIScript
{
	protected float m_slash_TellIntro_AnimationSpeed = 0.5f;

	protected float m_slash_TellHold_AnimationSpeed = 0.5f;

	protected float m_slash_AttackIntro_AnimationSpeed = 2f;

	protected float m_slash_AttackIntro_Delay;

	protected float m_slash_AttackHold_AnimationSpeed = 1f;

	protected float m_slash_AttackHold_Delay = 0.5f;

	protected float m_slash_Exit_AnimationSpeed = 0.7f;

	protected float m_slash_Exit_Delay = 0.15f;

	protected float m_slash_AttackCD = 1f;

	protected float m_cricket_TellIntro_AnimationSpeed = 0.75f;

	protected float m_cricket_TellHold_AnimationSpeed = 0.65f;

	protected float m_cricket_TellIntroAndHold_Delay = 0.9f;

	protected float m_cricket_AttackIntro_AnimationSpeed = 1f;

	protected float m_cricket_AttackIntro_Delay;

	protected float m_cricket_AttackHold_AnimationSpeed = 1f;

	protected float m_cricket_AttackHold_Delay = 0.25f;

	protected float m_cricket_Exit_AnimationSpeed = 0.45f;

	protected float m_cricket_Exit_Delay = 0.15f;

	private Action<MonoBehaviour, EventArgs> m_onPlayerHit;

	private const string CHEST_BUMP_TELL_INTRO = "ChestBump_Tell_Intro";

	private const string CHEST_BUMP_TELL_HOLD = "ChestBump_Tell_Hold";

	private const string CHEST_BUMP_ATTACK_INTRO = "ChestBump_Attack_Intro";

	private const string CHEST_BUMP_ATTACK_HOLD = "ChestBump_Attack_Hold";

	private const string CHEST_BUMP_EXIT = "ChestBump_Exit";

	protected virtual float m_slash_TellIntroAndHold_Delay => 0.85f;

	protected virtual float m_slash_Exit_ForceIdle => 0.25f;

	protected virtual float m_slash_Attack_Speed => 13f;

	protected virtual float m_slash_Attack_Duration => 0.175f;

	protected virtual float m_cricket_Exit_ForceIdle => 0.4f;

	protected virtual float m_cricket_AttackCD => 3.5f;

	protected virtual int m_cricket_Attack_ProjectileAmount => 1;

	protected virtual float m_cricket_Attack_ProjectileDelay => 1.65f;

	protected virtual float m_chestBumpTellHoldDelay => 0f;

	protected virtual float m_chestBumpAttackHoldDelay => 0f;

	protected override void InitializeProjectileNameArray()
	{
		m_projectileNameArray = new string[4] { "SwordKnightSwordSlashProjectile", "SwordKnightBounceMinibossProjectile", "SwordKnightVoidProjectile", "SwordKnightVoidMinibossProjectile" };
	}

	private void Awake()
	{
		m_onPlayerHit = OnPlayerHit;
	}

	private void OnEnable()
	{
		Messenger<GameMessenger, GameEvent>.AddListener(GameEvent.PlayerHit, m_onPlayerHit);
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		Messenger<GameMessenger, GameEvent>.RemoveListener(GameEvent.PlayerHit, m_onPlayerHit);
	}

	private void OnPlayerHit(object sender, EventArgs args)
	{
		if (base.IsInitialized && (base.LogicController.CurrentLogicBlockName == "Idle" || base.LogicController.CurrentLogicBlockName == "WalkTowards" || base.LogicController.CurrentLogicBlockName == "WalkAway") && args is CharacterHitEventArgs characterHitEventArgs && characterHitEventArgs.Attacker.Equals(base.EnemyController))
		{
			base.LogicController.StopAllLogic(resetCurrentState: true);
			base.LogicController.ForceExecuteLogicBlockName_OnceOnly = "ChestBump";
		}
	}

	[CloseLogic]
	[MediumLogic]
	[FarLogic]
	[RestLogic]
	[BasicEnemy]
	[AdvancedEnemy]
	[ExpertEnemy]
	[MinibossEnemy]
	public IEnumerator SlashAttack()
	{
		StopAndFaceTarget();
		base.EnemyController.LockFlip = true;
		float dashSpeed = m_slash_Attack_Speed;
		if (!base.EnemyController.IsTargetToMyRight)
		{
			dashSpeed = 0f - dashSpeed;
		}
		yield return Default_TellIntroAndLoop("Slash_Tell_Intro", m_slash_TellIntro_AnimationSpeed, "Slash_Tell_Hold", m_slash_TellHold_AnimationSpeed, m_slash_TellIntroAndHold_Delay);
		SetAttackingWithContactDamage(enable: true);
		yield return Default_Animation("Slash_Attack_Intro", m_slash_AttackIntro_AnimationSpeed, m_slash_AttackIntro_Delay);
		this.FireProjectile("SwordKnightSwordSlashProjectile", 0);
		yield return Default_Animation("Slash_Attack_Hold", m_slash_AttackHold_AnimationSpeed, 0f, waitUntilAnimationComplete: false);
		if (m_slash_Attack_Duration > 0f)
		{
			SetVelocityX(dashSpeed, additive: false);
			if (m_slash_Attack_Duration > 0f)
			{
				yield return Wait(m_slash_Attack_Duration);
			}
			SetVelocityX(0f, additive: false);
		}
		if (m_slash_AttackHold_Delay - m_slash_Attack_Duration > 0f)
		{
			yield return Wait(m_slash_AttackHold_Delay - m_slash_Attack_Duration);
		}
		SetAttackingWithContactDamage(enable: false);
		yield return Default_Animation("Slash_Exit", m_slash_Exit_AnimationSpeed, m_slash_Exit_Delay);
		base.EnemyController.LockFlip = false;
		yield return Default_Attack_Cooldown(m_slash_Exit_ForceIdle, m_slash_AttackCD);
	}

	[CloseLogic]
	[MediumLogic]
	[FarLogic]
	[RestLogic]
	[BasicEnemy]
	[AdvancedEnemy]
	[ExpertEnemy]
	[MinibossEnemy]
	public IEnumerator CricketAttack()
	{
		StopAndFaceTarget();
		base.EnemyController.LockFlip = true;
		yield return Default_TellIntroAndLoop("GroundShot_Tell_Intro", m_cricket_TellIntro_AnimationSpeed, "GroundShot_Tell_Hold", m_cricket_TellHold_AnimationSpeed, m_cricket_TellIntroAndHold_Delay);
		SetAttackingWithContactDamage(enable: true);
		yield return Default_Animation("GroundShot_Attack_Intro", m_cricket_AttackIntro_AnimationSpeed, m_cricket_AttackIntro_Delay);
		yield return Default_Animation("GroundShot_Attack_Hold", m_cricket_AttackHold_AnimationSpeed, 0f, waitUntilAnimationComplete: false);
		for (int i = 1; i < m_cricket_Attack_ProjectileAmount + 1; i++)
		{
			if (base.LogicController.EnemyLogicType == EnemyLogicType.Miniboss)
			{
				switch (UnityEngine.Random.Range(0, 4))
				{
				case 0:
					this.FireProjectile("SwordKnightBounceMinibossProjectile", 6);
					this.FireProjectile("SwordKnightBounceMinibossProjectile", 5);
					this.FireProjectile("SwordKnightBounceMinibossProjectile", 4);
					this.FireProjectile("SwordKnightBounceMinibossProjectile", 3);
					break;
				case 1:
					this.FireProjectile("SwordKnightBounceMinibossProjectile", 5);
					this.FireProjectile("SwordKnightBounceMinibossProjectile", 4);
					this.FireProjectile("SwordKnightBounceMinibossProjectile", 3);
					this.FireProjectile("SwordKnightBounceMinibossProjectile", 2);
					break;
				case 2:
					this.FireProjectile("SwordKnightVoidMinibossProjectile", 0);
					break;
				case 3:
					this.FireProjectile("SwordKnightBounceMinibossProjectile", 6);
					this.FireProjectile("SwordKnightBounceMinibossProjectile", 4);
					this.FireProjectile("SwordKnightBounceMinibossProjectile", 2);
					break;
				}
			}
			else
			{
				this.FireProjectile("SwordKnightVoidProjectile", 0);
			}
			if (m_cricket_Attack_ProjectileAmount > 1 && m_cricket_Attack_ProjectileDelay > 0f)
			{
				yield return Wait(m_cricket_Attack_ProjectileDelay);
			}
		}
		yield return Wait(m_cricket_AttackHold_Delay);
		SetAttackingWithContactDamage(enable: false);
		yield return Default_Animation("GroundShot_Exit", m_cricket_Exit_AnimationSpeed, m_cricket_Exit_Delay);
		base.EnemyController.LockFlip = false;
		yield return Default_Attack_Cooldown(m_cricket_Exit_ForceIdle, m_cricket_AttackCD);
	}

	[CloseLogic]
	[MediumLogic]
	[FarLogic]
	[RestLogic]
	[WanderLogic]
	[BasicEnemy]
	[AdvancedEnemy]
	[ExpertEnemy]
	[MinibossEnemy]
	public IEnumerator ChestBump()
	{
		base.EnemyController.LockFlip = true;
		base.EnemyController.SetVelocityX(0f, additive: false);
		yield return Default_Animation("ChestBump_Tell_Intro", 10f, 0f);
		yield return Default_Animation("ChestBump_Tell_Hold", 10f, m_chestBumpTellHoldDelay);
		yield return Default_Animation("ChestBump_Attack_Intro", 1f, 0f);
		yield return Default_Animation("ChestBump_Attack_Hold", 1f, m_chestBumpAttackHoldDelay);
		yield return Default_Animation("ChestBump_Exit", 1f, 0f);
		yield return Default_Animation(BaseAIScript.NEUTRAL_STATE, 1f, 0f);
		base.EnemyController.LockFlip = false;
	}

	public override void OnLBCompleteOrCancelled()
	{
		base.EnemyController.LockFlip = false;
		base.OnLBCompleteOrCancelled();
	}
}
