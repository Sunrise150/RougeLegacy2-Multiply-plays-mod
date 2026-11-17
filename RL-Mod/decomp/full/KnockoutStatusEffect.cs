using System.Collections;
using MoreMountains.CorgiEngine;
using UnityEngine;

public class KnockoutStatusEffect : BaseStatusEffect
{
	private static string[] m_projectileNameArray = new string[1] { "KnockoutStatusEffectProjectile" };

	private Projectile_RL m_knockbackProjectile;

	private bool m_isKnockedBack;

	private bool m_oneWayCollisionDisabled;

	public override string[] ProjectileNameArray => m_projectileNameArray;

	public override StatusEffectType StatusEffectType => StatusEffectType.Enemy_Knockout;

	public override float StartingDurationOverride => 0.6f;

	protected override IEnumerator StartEffectCoroutine(IDamageObj caster, bool justCasted)
	{
		m_statusEffectController.StartStatusEffect(StatusEffectType.Enemy_DeathDelay, base.Duration, caster);
		m_statusEffectController.StartStatusEffect(StatusEffectType.Enemy_Phased, base.Duration, caster);
		yield return null;
		if ((bool)m_knockbackProjectile && !m_knockbackProjectile.IsFreePoolObj)
		{
			m_knockbackProjectile.FlagForDestruction();
			m_knockbackProjectile = null;
		}
		PlayerController playerController = PlayerManager.GetPlayerController();
		int num = 0;
		if (m_charController.StatusEffectController.HasStatusEffect(StatusEffectType.Enemy_Combo))
		{
			BaseStatusEffect statusEffect = m_charController.StatusEffectController.GetStatusEffect(StatusEffectType.Enemy_Combo);
			num += statusEffect.TimesStacked;
			m_charController.StatusEffectController.StopStatusEffect(StatusEffectType.Enemy_Combo, interrupted: true);
		}
		if (playerController.StatusEffectController.HasStatusEffect(StatusEffectType.Player_Combo))
		{
			BaseStatusEffect statusEffect2 = playerController.StatusEffectController.GetStatusEffect(StatusEffectType.Player_Combo);
			num += statusEffect2.TimesStacked;
			Invoke("RemovePlayerCombo", 0f);
		}
		Vector2 offset = m_charController.Midpoint - m_charController.transform.position;
		float num2 = m_charController.BaseScaleToOffsetWith / m_charController.transform.localScale.x;
		offset *= num2;
		m_knockbackProjectile = ProjectileManager.FireProjectile(m_charController.gameObject, "KnockoutStatusEffectProjectile", offset, matchFacing: false);
		m_knockbackProjectile.Magic = playerController.ActualMagic;
		m_knockbackProjectile.Strength = playerController.ActualStrength;
		m_knockbackProjectile.StrengthScale = m_knockbackProjectile.ProjectileData.StrengthScale;
		m_knockbackProjectile.MagicScale = m_knockbackProjectile.ProjectileData.MagicScale;
		if (m_knockbackProjectile.ActualDamage > 0f)
		{
			m_knockbackProjectile.DamageMod = (float)num * 0.3f * m_knockbackProjectile.ActualDamage / m_knockbackProjectile.ActualDamage;
		}
		m_knockbackProjectile.GetComponent<OnProjectileDeathProjectileLogic>().enabled = true;
		EnemyController enemyController = m_charController as EnemyController;
		Vector2 vector = (Vector2)caster.gameObject.transform.position + caster.ExternalKnockbackMod * 32f;
		Vector2 pushbackVelocity = (vector - (Vector2)enemyController.Midpoint).normalized;
		pushbackVelocity *= 33f;
		float explosionDelayTime = Time.time + 0.1f;
		Projectile_RL projectile_RL = caster as Projectile_RL;
		m_isKnockedBack = false;
		if ((bool)projectile_RL && projectile_RL.ActualKnockbackStrength > enemyController.ActualKnockbackDefense)
		{
			if (enemyController.CurrentHealth > 0f)
			{
				enemyController.LogicController.StopAllLogic(resetCurrentState: true);
				enemyController.ConditionState = CharacterStates.CharacterConditions.Stunned;
				if (AnimatorUtility.HasParameter(enemyController.Animator, "Stunned"))
				{
					enemyController.Animator.SetBool("Stunned", value: true);
				}
				else
				{
					enemyController.Animator.Play("Neutral");
				}
			}
			m_isKnockedBack = true;
			if (!enemyController.ControllerCorgi.DisableOneWayCollision)
			{
				m_oneWayCollisionDisabled = true;
				enemyController.ControllerCorgi.DisableOneWayCollision = true;
			}
			float oneWayCollisionDuration = Time.time + 0.25f;
			while (Time.time < base.EndTime)
			{
				if (m_oneWayCollisionDisabled && Time.time > oneWayCollisionDuration)
				{
					m_oneWayCollisionDisabled = false;
					enemyController.ControllerCorgi.DisableOneWayCollision = false;
				}
				m_charController.SetVelocity(pushbackVelocity.x, pushbackVelocity.y, additive: false);
				if ((enemyController.TouchingBottomRoomEdge || enemyController.TouchingLeftRoomEdge || enemyController.TouchingRightRoomEdge || enemyController.TouchingTopRoomEdge || m_charController.ControllerCorgi.ContactList.Count > 0) && Time.time > explosionDelayTime)
				{
					StopEffect();
					yield break;
				}
				yield return null;
			}
		}
		StopEffect();
	}

	private void RemovePlayerCombo()
	{
		PlayerManager.GetPlayerController().StatusEffectController.StopStatusEffect(StatusEffectType.Player_Combo, interrupted: true);
	}

	public override void StopEffect(bool interrupted = false)
	{
		base.StopEffect(interrupted);
		EnemyController enemyController = m_charController as EnemyController;
		enemyController.ConditionState = CharacterStates.CharacterConditions.Normal;
		if (AnimatorUtility.HasParameter(enemyController.Animator, "Stunned"))
		{
			enemyController.Animator.SetBool("Stunned", value: false);
		}
		if ((bool)m_knockbackProjectile && !m_knockbackProjectile.IsFreePoolObj)
		{
			if (interrupted)
			{
				m_knockbackProjectile.gameObject.SetActive(value: false);
			}
			else
			{
				m_knockbackProjectile.transform.SetParent(null, worldPositionStays: true);
				m_knockbackProjectile.FlagForDestruction();
			}
			m_knockbackProjectile = null;
			if (m_isKnockedBack)
			{
				m_charController.SetVelocity(0f, 0f, additive: false);
				m_isKnockedBack = false;
			}
		}
		if (m_oneWayCollisionDisabled)
		{
			m_oneWayCollisionDisabled = false;
			enemyController.ControllerCorgi.DisableOneWayCollision = false;
		}
		m_statusEffectController.StopStatusEffect(StatusEffectType.Enemy_Phased, interrupted);
		m_statusEffectController.StopStatusEffect(StatusEffectType.Enemy_DeathDelay, interrupted);
	}
}
