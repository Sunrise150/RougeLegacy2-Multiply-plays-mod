using System.Collections;
using UnityEngine;

public class Eggplant_Basic_AIScript : BaseAIScript
{
	private const string HIGH_SHOT_TELL_INTRO = "HighShot_Tell_Intro";

	private const string HIGH_SHOT_TELL_HOLD = "HighShot_Tell_Hold";

	private const string HIGH_SHOT_ATTACK_INTRO = "HighShot_Attack_Intro";

	private const string HIGH_SHOT_ATTACK_HOLD = "HighShot_Attack_Hold";

	private const string HIGH_SHOT_EXIT = "HighShot_Exit";

	private const string FIREWORKS_PROJECTILE_NAME = "EggplantFireworksProjectile";

	protected float m_highShot_TellIntro_AnimationSpeed = 0.85f;

	protected float m_highShot_TellHold_AnimationSpeed = 0.85f;

	protected const float m_highShot_TellIntroAndHold_Delay = 2.5f;

	protected float m_highShot_AttackIntro_AnimationSpeed = 1.1f;

	protected float m_highShot_AttackIntro_Delay;

	protected float m_highShot_AttackHold_AnimationSpeed = 1.1f;

	protected float m_highShot_AttackHold_Delay = 0.65f;

	protected float m_highShot_Exit_AnimationSpeed = 1f;

	protected float m_highShot_Exit_Delay;

	protected float m_highShot_Exit_ForceIdle = 2f;

	protected float m_highShot_Exit_AttackCD = 10f;

	private const string LOW_SHOT_TELL_INTRO = "LowShot_Tell_Intro";

	private const string LOW_SHOT_TELL_HOLD = "LowShot_Tell_Hold";

	private const string LOW_SHOT_ATTACK_INTRO = "LowShot_Attack_Intro";

	private const string LOW_SHOT_ATTACK_HOLD = "LowShot_Attack_Hold";

	private const string LOW_SHOT_EXIT = "LowShot_Exit";

	protected float m_lowShot_TellIntro_AnimationSpeed = 0.85f;

	protected float m_lowShot_TellHold_AnimationSpeed = 0.85f;

	protected const float m_lowShot_TellIntroAndHold_Delay = 2.5f;

	protected float m_lowShot_AttackIntro_AnimationSpeed = 1.1f;

	protected float m_lowShot_AttackIntro_Delay;

	protected float m_lowShot_AttackHold_AnimationSpeed = 1.1f;

	protected float m_lowShot_AttackHold_Delay = 0.65f;

	protected float m_lowShot_Exit_AnimationSpeed = 1f;

	protected float m_lowShot_Exit_Delay;

	protected float m_lowShot_Exit_ForceIdle = 2f;

	protected float m_lowShot_Exit_AttackCD = 10f;

	private const string DUAL_SHOT_TELL_INTRO = "MegaShot_Tell_Intro";

	private const string DUAL_SHOT_TELL_HOLD = "MegaShot_Tell_Hold";

	private const string DUAL_SHOT_ATTACK_INTRO = "MegaShot_Attack_Intro";

	private const string DUAL_SHOT_ATTACK_HOLD = "MegaShot_Attack_Hold";

	private const string DUAL_SHOT_EXIT = "MegaShot_Exit";

	protected float m_dualShot_TellIntro_AnimationSpeed = 0.85f;

	protected float m_dualShot_TellHold_AnimationSpeed = 0.85f;

	protected const float m_dualShot_TellIntroAndHold_Delay = 2.5f;

	protected float m_dualShot_AttackIntro_AnimationSpeed = 1.1f;

	protected float m_dualShot_AttackIntro_Delay;

	protected float m_dualShot_AttackHold_AnimationSpeed = 1.1f;

	protected float m_dualShot_AttackHold_Delay = 0.65f;

	protected float m_dualShot_Exit_AnimationSpeed = 1f;

	protected float m_dualShot_Exit_Delay;

	protected float m_dualShot_Exit_ForceIdle = 2f;

	protected float m_dualShot_Exit_AttackCD = 10f;

	protected override Vector2 IdleDuration => new Vector2(0.55f, 1.75f);

	protected override Vector2 WalkTowardsDuration => new Vector2(0.55f, 1.25f);

	protected override Vector2 WalkAwayDuration => new Vector2(0.55f, 1.25f);

	protected override void InitializeProjectileNameArray()
	{
		m_projectileNameArray = new string[1] { "EggplantFireworksProjectile" };
	}

	[CloseLogic]
	[MediumLogic]
	[FarLogic]
	[BasicEnemy]
	[AdvancedEnemy]
	[ExpertEnemy]
	[MinibossEnemy]
	public IEnumerator High_Shot_Attack()
	{
		SetVelocityX(0f, additive: false);
		yield return Default_TellIntroAndLoop("HighShot_Tell_Intro", m_highShot_TellIntro_AnimationSpeed, "HighShot_Tell_Hold", m_highShot_TellHold_AnimationSpeed, 2.5f);
		yield return Default_Animation("HighShot_Attack_Intro", m_highShot_AttackIntro_AnimationSpeed, m_highShot_AttackIntro_Delay);
		yield return Default_Animation("HighShot_Attack_Hold", m_highShot_AttackHold_AnimationSpeed, 0f, waitUntilAnimationComplete: false);
		this.FireProjectile("EggplantFireworksProjectile", 1);
		SetAttackingWithContactDamage(enable: false);
		yield return Wait(m_highShot_AttackHold_Delay);
		yield return Default_Animation("HighShot_Exit", m_highShot_Exit_AnimationSpeed, m_highShot_Exit_Delay);
		yield return this.ChangeAnimationState(BaseAIScript.NEUTRAL_STATE);
		yield return Default_Attack_Cooldown(m_highShot_Exit_ForceIdle, m_highShot_Exit_AttackCD);
	}

	[CloseLogic]
	[MediumLogic]
	[FarLogic]
	[BasicEnemy]
	[AdvancedEnemy]
	[ExpertEnemy]
	[MinibossEnemy]
	public IEnumerator Low_Shot_Attack()
	{
		SetVelocityX(0f, additive: false);
		yield return Default_TellIntroAndLoop("LowShot_Tell_Intro", m_lowShot_TellIntro_AnimationSpeed, "LowShot_Tell_Hold", m_lowShot_TellHold_AnimationSpeed, 2.5f);
		yield return Default_Animation("LowShot_Attack_Intro", m_lowShot_AttackIntro_AnimationSpeed, m_lowShot_AttackIntro_Delay);
		yield return Default_Animation("LowShot_Attack_Hold", m_lowShot_AttackHold_AnimationSpeed, 0f, waitUntilAnimationComplete: false);
		this.FireProjectile("EggplantFireworksProjectile", 2);
		SetAttackingWithContactDamage(enable: false);
		yield return Wait(m_lowShot_AttackHold_Delay);
		yield return Default_Animation("LowShot_Exit", m_lowShot_Exit_AnimationSpeed, m_lowShot_Exit_Delay);
		yield return this.ChangeAnimationState(BaseAIScript.NEUTRAL_STATE);
		yield return Default_Attack_Cooldown(m_lowShot_Exit_ForceIdle, m_lowShot_Exit_AttackCD);
	}

	[CloseLogic]
	[MediumLogic]
	[FarLogic]
	[BasicEnemy]
	[AdvancedEnemy]
	[ExpertEnemy]
	[MinibossEnemy]
	public IEnumerator Dual_Shot_Attack()
	{
		SetVelocityX(0f, additive: false);
		yield return Default_TellIntroAndLoop("MegaShot_Tell_Intro", m_dualShot_TellIntro_AnimationSpeed, "MegaShot_Tell_Hold", m_dualShot_TellHold_AnimationSpeed, 2.5f);
		yield return Default_Animation("MegaShot_Attack_Intro", m_dualShot_AttackIntro_AnimationSpeed, m_dualShot_AttackIntro_Delay);
		yield return Default_Animation("MegaShot_Attack_Hold", m_dualShot_AttackHold_AnimationSpeed, 0f, waitUntilAnimationComplete: false);
		this.FireProjectile("EggplantFireworksProjectile", 1);
		this.FireProjectile("EggplantFireworksProjectile", 2);
		SetAttackingWithContactDamage(enable: false);
		yield return Wait(m_dualShot_AttackHold_Delay);
		yield return Default_Animation("MegaShot_Exit", m_dualShot_Exit_AnimationSpeed, m_dualShot_Exit_Delay);
		yield return this.ChangeAnimationState(BaseAIScript.NEUTRAL_STATE);
		yield return Default_Attack_Cooldown(m_dualShot_Exit_ForceIdle, m_dualShot_Exit_AttackCD);
	}
}
