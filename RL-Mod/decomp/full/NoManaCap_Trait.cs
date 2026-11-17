using System.Runtime.CompilerServices;
using Rewired;
using UnityEngine;

public class NoManaCap_Trait : BaseTrait, IDamageObj
{
	private PlayerController m_playerController;

	private float m_damageTickStartTime;

	public override TraitType TraitType => TraitType.NoManaCap;

	public float BaseDamage => (float)m_playerController.ActualMaxHealth * 0.02f;

	public float ActualDamage => BaseDamage;

	public float ActualCritChance => 0f;

	public float ActualCritDamage => 0f;

	public Vector2 ExternalKnockbackMod => Vector2.zero;

	public float BaseKnockbackStrength { get; set; }

	public float ActualKnockbackStrength => 0f;

	public float BaseStunStrength { get; set; }

	public float ActualStunStrength => BaseStunStrength;

	public StrikeType StrikeType => StrikeType.Blunt;

	public bool IsDotDamage => true;

	public StatusEffectType[] StatusEffectTypes => null;

	public float[] StatusEffectDurations => null;

	private void Start()
	{
		m_playerController = PlayerManager.GetPlayerController();
		m_damageTickStartTime = Time.time;
	}

	private void Update()
	{
		if (!(m_playerController != null) || !(m_playerController.CurrentMana > (float)m_playerController.ActualMaxMana) || !ReInput.isReady)
		{
			return;
		}
		if (RewiredMapController.CurrentGameInputMode == GameInputMode.Game && RewiredMapController.IsCurrentMapEnabled)
		{
			if (Time.time > m_damageTickStartTime + 1f)
			{
				m_damageTickStartTime = Time.time;
				m_playerController.DisableArmor = true;
				m_playerController.CharacterHitResponse.StartHitResponse(m_playerController.gameObject, this);
				m_playerController.DisableArmor = false;
			}
		}
		else
		{
			m_damageTickStartTime = Time.time;
		}
	}

	[SpecialName]
	GameObject IDamageObj.get_gameObject()
	{
		return base.gameObject;
	}
}
