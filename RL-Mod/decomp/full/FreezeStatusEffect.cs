using System;
using System.Collections;
using MoreMountains.CorgiEngine;
using UnityEngine;

public class FreezeStatusEffect : BaseStatusEffect
{
	private const string MULTIPLY_COLOR = "#9CE6FF";

	private const string ADD_COLOR = "#7382FF";

	private Color m_multiplyColor;

	private Color m_addColor;

	private float m_shakeAmountMin = 0.025f;

	private float m_shakeAmountMax = 0.025f;

	private float m_shakeInterval = 0.05f;

	private float m_shakeDuration = 0.5f;

	private bool m_shakeStarted;

	private Coroutine m_shakeCoroutine;

	private Vector3 m_storedVisualsPos;

	private bool m_hasGravity;

	private float m_unbreakableFreezeTime;

	private float m_storedKnockback;

	private Action<object, CharacterHitEventArgs> m_onFrozenEnemyHit;

	public override StatusEffectType StatusEffectType => StatusEffectType.Enemy_Freeze;

	public override float StartingDurationOverride => 3f;

	public override void Initialize(StatusEffectController statusEffectController, BaseCharacterController charController)
	{
		base.Initialize(statusEffectController, charController);
		ColorUtility.TryParseHtmlString("#9CE6FF", out m_multiplyColor);
		ColorUtility.TryParseHtmlString("#7382FF", out m_addColor);
		base.AppliesTint = true;
		m_onFrozenEnemyHit = OnFrozenEnemyHit;
	}

	public override void StartEffect(float duration, IDamageObj caster)
	{
		base.StartEffect(duration, caster);
	}

	protected override IEnumerator StartEffectCoroutine(IDamageObj caster, bool justCasted)
	{
		m_shakeStarted = false;
		if (m_shakeCoroutine != null)
		{
			StopCoroutine(m_shakeCoroutine);
		}
		m_charController.StatusBarController.ApplyUIEffect(StatusBarEntryType.Freeze, base.Duration);
		m_unbreakableFreezeTime = Time.time + 0.25f;
		m_storedKnockback = m_charController.BaseKnockbackDefense;
		m_charController.BaseKnockbackDefense = m_storedKnockback + 0f;
		m_hasGravity = m_charController.ControllerCorgi.IsGravityActive;
		m_charController.CharacterCorgi.Freeze();
		m_charController.Animator.enabled = false;
		m_charController.HitboxController.SetHitboxActiveState(HitboxType.Weapon, active: false);
		EnemyController enemyController = m_charController as EnemyController;
		if ((bool)enemyController)
		{
			if (enemyController.IsBoss)
			{
				base.Duration *= 0.15f;
			}
			enemyController.LogicController.Pause();
		}
		foreach (Renderer item in m_charController.RendererArray)
		{
			item.GetPropertyBlock(BaseStatusEffect.m_matBlockHelper_STATIC);
			BaseStatusEffect.m_matBlockHelper_STATIC.SetColor(ShaderID_RL._MultiplyColor, m_multiplyColor);
			BaseStatusEffect.m_matBlockHelper_STATIC.SetColor(ShaderID_RL._AddColor, m_addColor);
			item.SetPropertyBlock(BaseStatusEffect.m_matBlockHelper_STATIC);
		}
		yield return null;
		m_charController.CharacterHitResponse.OnCharacterHitRelay.AddListener(m_onFrozenEnemyHit);
		while (Time.time < base.EndTime)
		{
			if (!m_shakeStarted && Time.time > base.EndTime - m_shakeDuration)
			{
				m_shakeStarted = true;
				m_shakeCoroutine = StartCoroutine(ShakeCoroutine());
			}
			yield return null;
		}
		m_statusEffectController.StartStatusEffect(StatusEffectType.Enemy_FreezeImmunity, 0f, null);
		EffectManager.PlayEffect(m_charController.gameObject, m_charController.Animator, "FreezeThawParticles_Effect", m_charController.Midpoint, 0f, EffectStopType.Gracefully);
		StopEffect();
	}

	private IEnumerator ShakeCoroutine()
	{
		m_storedVisualsPos = m_charController.Visuals.transform.localPosition;
		float startTime = Time.time;
		while (true)
		{
			if (Time.time > startTime + m_shakeInterval)
			{
				startTime = Time.time;
				Vector3 storedVisualsPos = m_storedVisualsPos;
				float num = UnityEngine.Random.Range(m_shakeAmountMin, m_shakeAmountMax);
				num *= (float)CDGHelper.RandomPlusMinus();
				float num2 = UnityEngine.Random.Range(m_shakeAmountMin, m_shakeAmountMax);
				num2 *= (float)CDGHelper.RandomPlusMinus();
				storedVisualsPos.x += num;
				storedVisualsPos.y += num2;
				m_charController.Visuals.transform.localPosition = storedVisualsPos;
			}
			yield return null;
		}
	}

	public override void StopEffect(bool interrupted = false)
	{
		base.StopEffect(interrupted);
		if (m_shakeCoroutine != null)
		{
			StopCoroutine(m_shakeCoroutine);
		}
		if (m_shakeStarted)
		{
			m_shakeStarted = false;
			m_charController.Visuals.transform.localPosition = m_storedVisualsPos;
		}
		if (m_charController.ConditionState == CharacterStates.CharacterConditions.Frozen)
		{
			m_charController.CharacterCorgi.UnFreeze();
			m_charController.ControllerCorgi.GravityActive(m_hasGravity);
			EnemyController enemyController = m_charController as EnemyController;
			if ((bool)enemyController)
			{
				enemyController.LogicController.Unpause();
			}
		}
		m_charController.HitboxController.SetHitboxActiveState(HitboxType.Weapon, active: true);
		m_charController.Animator.enabled = true;
		m_charController.BaseKnockbackDefense = m_storedKnockback;
		m_charController.CharacterHitResponse.OnCharacterHitRelay.RemoveListener(m_onFrozenEnemyHit);
	}

	private void OnFrozenEnemyHit(object sender, EventArgs args)
	{
		if (Time.time > m_unbreakableFreezeTime)
		{
			StopEffect();
			EffectManager.PlayEffect(m_charController.gameObject, m_charController.Animator, "FreezeThawParticles_Effect", m_charController.Midpoint, 0f, EffectStopType.Gracefully);
		}
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		if ((bool)m_charController && m_charController.IsInitialized)
		{
			m_charController.CharacterHitResponse.OnCharacterHitRelay.RemoveListener(m_onFrozenEnemyHit);
		}
	}
}
