using System.Collections;
using UnityEngine;

public class Dummy_AIScript : BaseAIScript
{
	protected const string ARROW_ATTACK_ATTACK = "Stunned";

	protected const string ARROW_PROJECTILE = "SkeletonArcherBoltProjectile";

	private Coroutine m_blinkCoroutine;

	protected virtual float ArrowAttack_AnimSpeed => 1f;

	protected virtual float ArrowAttack_Duration => 0f;

	protected virtual float ArrowAttack_Exit_ForceIdle => 0.15f;

	protected virtual float ArrowAttack_Exit_AttackCD => 2.75f;

	protected virtual int ArrowAttack_NumProjectiles => 10;

	protected override void InitializeProjectileNameArray()
	{
		m_projectileNameArray = new string[1] { "SkeletonArcherBoltProjectile" };
	}

	[CloseLogic]
	[MediumLogic]
	[FarLogic]
	[MinibossEnemy]
	public IEnumerator Arrow_Attack()
	{
		base.EnemyController.AlwaysFacing = false;
		Color red = Color.red;
		red.a = 0.5f;
		m_blinkCoroutine = this.RunPersistentCoroutine(BlinkCoroutine(red));
		yield return Default_Animation("Stunned", ArrowAttack_AnimSpeed, ArrowAttack_Duration);
		yield return Wait(0.5f);
		float num = 180 / ArrowAttack_NumProjectiles;
		for (int i = 0; i < ArrowAttack_NumProjectiles; i++)
		{
			this.FireProjectile("SkeletonArcherBoltProjectile", 0, matchFacing: false, num * (float)i);
		}
		this.StopPersistentCoroutine(m_blinkCoroutine);
		SetAttackingWithContactDamage(enable: false);
		base.EnemyController.AlwaysFacing = true;
		yield return Default_Attack_Cooldown(ArrowAttack_Exit_ForceIdle, ArrowAttack_Exit_AttackCD);
	}

	private IEnumerator BlinkCoroutine(Color color)
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

	public override void OnLBCompleteOrCancelled()
	{
		this.StopPersistentCoroutine(m_blinkCoroutine);
		base.OnLBCompleteOrCancelled();
	}

	public override void ResetScript()
	{
		base.EnemyController.ControllerCorgi.GravityActive(state: false);
		base.ResetScript();
	}
}
