using System;
using System.Collections;
using UnityEngine;

public class EnemyInvulnWindowStatusEffect : BaseStatusEffect
{
	private static string[] m_projectileNameArray = new string[1] { "StatusEffectInvulnWindowProjectile" };

	private Action<object, CharacterHitEventArgs> m_onEnemyHit;

	private bool m_countdownStarted;

	public override string[] ProjectileNameArray => m_projectileNameArray;

	public override StatusEffectType StatusEffectType => StatusEffectType.Enemy_InvulnWindow;

	public override float StartingDurationOverride => float.MaxValue;

	protected override void Awake()
	{
		base.Awake();
		m_onEnemyHit = OnEnemyHit;
	}

	protected override IEnumerator StartEffectCoroutine(IDamageObj caster, bool justCasted)
	{
		m_charController.StatusBarController.ApplyUIEffect(StatusBarEntryType.InvulnWindow);
		m_charController.CharacterHitResponse.OnCharacterHitRelay.AddListener(m_onEnemyHit);
		m_countdownStarted = false;
		while (!m_countdownStarted)
		{
			yield return null;
		}
		float duration = Time.time + 5f;
		m_charController.StatusBarController.ApplyUIEffect(StatusBarEntryType.InvulnWindow, 5f);
		float fireDelay = Time.time + 1.15f;
		while (Time.time < duration)
		{
			if (Time.time >= fireDelay)
			{
				float num = CDGHelper.VectorToAngle(PlayerManager.GetPlayerController().Midpoint - m_charController.Midpoint);
				UnityEngine.Random.Range(StatusEffect_EV.ENEMY_INVULNWINDOW_PROJECTILE_OFFSET.x, StatusEffect_EV.ENEMY_INVULNWINDOW_PROJECTILE_OFFSET.y);
				ProjectileManager.FireProjectile(m_charController.gameObject, "StatusEffectInvulnWindowProjectile", m_charController.Midpoint, matchFacing: false, num, 1f, useAbsPos: true);
				num += 40f;
				float num2 = UnityEngine.Random.Range(StatusEffect_EV.ENEMY_INVULNWINDOW_PROJECTILE_OFFSET.x, StatusEffect_EV.ENEMY_INVULNWINDOW_PROJECTILE_OFFSET.y);
				ProjectileManager.FireProjectile(m_charController.gameObject, "StatusEffectInvulnWindowProjectile", m_charController.Midpoint, matchFacing: false, num + num2, 1f, useAbsPos: true);
				UnityEngine.Random.Range(StatusEffect_EV.ENEMY_INVULNWINDOW_PROJECTILE_OFFSET.x, StatusEffect_EV.ENEMY_INVULNWINDOW_PROJECTILE_OFFSET.y);
				num += 40f;
				num2 = UnityEngine.Random.Range(StatusEffect_EV.ENEMY_INVULNWINDOW_PROJECTILE_OFFSET.x, StatusEffect_EV.ENEMY_INVULNWINDOW_PROJECTILE_OFFSET.y);
				ProjectileManager.FireProjectile(m_charController.gameObject, "StatusEffectInvulnWindowProjectile", m_charController.Midpoint, matchFacing: false, num + num2, 1f, useAbsPos: true);
				num += 40f;
				num2 = UnityEngine.Random.Range(StatusEffect_EV.ENEMY_INVULNWINDOW_PROJECTILE_OFFSET.x, StatusEffect_EV.ENEMY_INVULNWINDOW_PROJECTILE_OFFSET.y);
				ProjectileManager.FireProjectile(m_charController.gameObject, "StatusEffectInvulnWindowProjectile", m_charController.Midpoint, matchFacing: false, num + num2, 1f, useAbsPos: true);
				num += 40f;
				num2 = UnityEngine.Random.Range(StatusEffect_EV.ENEMY_INVULNWINDOW_PROJECTILE_OFFSET.x, StatusEffect_EV.ENEMY_INVULNWINDOW_PROJECTILE_OFFSET.y);
				ProjectileManager.FireProjectile(m_charController.gameObject, "StatusEffectInvulnWindowProjectile", m_charController.Midpoint, matchFacing: false, num + num2, 1f, useAbsPos: true);
				num += 40f;
				num2 = UnityEngine.Random.Range(StatusEffect_EV.ENEMY_INVULNWINDOW_PROJECTILE_OFFSET.x, StatusEffect_EV.ENEMY_INVULNWINDOW_PROJECTILE_OFFSET.y);
				ProjectileManager.FireProjectile(m_charController.gameObject, "StatusEffectInvulnWindowProjectile", m_charController.Midpoint, matchFacing: false, num + num2, 1f, useAbsPos: true);
				num += 40f;
				num2 = UnityEngine.Random.Range(StatusEffect_EV.ENEMY_INVULNWINDOW_PROJECTILE_OFFSET.x, StatusEffect_EV.ENEMY_INVULNWINDOW_PROJECTILE_OFFSET.y);
				ProjectileManager.FireProjectile(m_charController.gameObject, "StatusEffectInvulnWindowProjectile", m_charController.Midpoint, matchFacing: false, num + num2, 1f, useAbsPos: true);
				num += 40f;
				num2 = UnityEngine.Random.Range(StatusEffect_EV.ENEMY_INVULNWINDOW_PROJECTILE_OFFSET.x, StatusEffect_EV.ENEMY_INVULNWINDOW_PROJECTILE_OFFSET.y);
				ProjectileManager.FireProjectile(m_charController.gameObject, "StatusEffectInvulnWindowProjectile", m_charController.Midpoint, matchFacing: false, num + num2, 1f, useAbsPos: true);
				fireDelay = Time.time + 1.25f;
			}
			yield return null;
		}
		StopEffect();
	}

	public override void StopEffect(bool interrupted = false)
	{
		base.StopEffect(interrupted);
		m_charController.CharacterHitResponse.OnCharacterHitRelay.RemoveListener(m_onEnemyHit);
		m_statusEffectController.RemoveStatusEffectInvulnStack(StatusEffectType);
		(m_charController as EnemyController).DisableDeath = false;
		m_countdownStarted = false;
		if (GameManager.IsApplicationClosing || !m_charController || m_charController.StatusEffectController.HasStatusEffect(StatusEffectType.Enemy_Invuln) || BaseStatusEffect.m_matBlockHelper_STATIC == null || m_charController.RendererArray == null)
		{
			return;
		}
		foreach (Renderer item in m_charController.RendererArray)
		{
			if ((bool)item && item.sharedMaterial.HasProperty(ShaderID_RL._ShieldToggle))
			{
				item.GetPropertyBlock(BaseStatusEffect.m_matBlockHelper_STATIC);
				BaseStatusEffect.m_matBlockHelper_STATIC.SetInt(ShaderID_RL._ShieldToggle, 0);
				item.SetPropertyBlock(BaseStatusEffect.m_matBlockHelper_STATIC);
			}
		}
	}

	private void OnEnemyHit(object sender, CharacterHitEventArgs args)
	{
		if (m_countdownStarted)
		{
			return;
		}
		float b = (float)m_charController.ActualMaxHealth * 0.01f;
		b = Mathf.Max(1f, b);
		if (!(m_charController.CurrentHealth <= b))
		{
			return;
		}
		m_countdownStarted = true;
		(m_charController as EnemyController).DisableDeath = true;
		m_statusEffectController.AddStatusEffectInvulnStack(StatusEffectType);
		m_charController.CharacterHitResponse.OnCharacterHitRelay.RemoveListener(m_onEnemyHit);
		foreach (Renderer item in m_charController.RendererArray)
		{
			if (item.sharedMaterial.HasProperty(ShaderID_RL._ShieldToggle))
			{
				item.GetPropertyBlock(BaseStatusEffect.m_matBlockHelper_STATIC);
				BaseStatusEffect.m_matBlockHelper_STATIC.SetInt(ShaderID_RL._ShieldToggle, 1);
				item.SetPropertyBlock(BaseStatusEffect.m_matBlockHelper_STATIC);
			}
		}
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		if ((bool)m_charController)
		{
			m_charController.CharacterHitResponse.OnCharacterHitRelay.RemoveListener(m_onEnemyHit);
		}
	}
}
