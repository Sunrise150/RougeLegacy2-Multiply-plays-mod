using System.Collections.Generic;
using UnityEngine;

public class StatusEffectController : MonoBehaviour
{
	public static bool DISABLE_ADDING_STATUS_EFFECTS;

	private BaseCharacterController m_charController;

	private List<StatusEffectType> m_immunityList;

	private HashSet<StatusEffectType> m_invulnStatusEffectStacks;

	private Dictionary<StatusEffectType, BaseStatusEffect> m_statusEffectTable;

	public bool HasInvulnStack
	{
		get
		{
			if (IsInitialized)
			{
				return m_invulnStatusEffectStacks.Count > 0;
			}
			return false;
		}
	}

	public bool ImmuneToAllStatusEffects { get; set; }

	public bool IsInitialized { get; private set; }

	public bool HasAnyActiveStatusEffect(bool includeCommanderBuffs)
	{
		StatusEffectType[] typeArray = StatusEffectType_RL.TypeArray;
		foreach (StatusEffectType statusEffectType in typeArray)
		{
			if ((includeCommanderBuffs || StatusEffect_EV.COMMANDER_STATUS_EFFECT_ARRAY.IndexOf(statusEffectType) == -1) && HasStatusEffect(statusEffectType))
			{
				return true;
			}
		}
		return false;
	}

	public bool HasAnyActiveTintedStatusEffect()
	{
		StatusEffectType[] typeArray = StatusEffectType_RL.TypeArray;
		foreach (StatusEffectType key in typeArray)
		{
			if (m_statusEffectTable.TryGetValue(key, out var value) && value.IsPlaying && value.AppliesTint)
			{
				return true;
			}
		}
		return false;
	}

	public bool HasStatusEffect(StatusEffectType statusEffectType)
	{
		if (m_statusEffectTable.TryGetValue(statusEffectType, out var value))
		{
			return value.IsPlaying;
		}
		return false;
	}

	public BaseStatusEffect GetStatusEffect(StatusEffectType statusEffectType)
	{
		if (m_statusEffectTable.TryGetValue(statusEffectType, out var value))
		{
			return value;
		}
		return null;
	}

	public void Initialize(BaseCharacterController charController)
	{
		m_charController = charController;
		InitializeStatusEffects();
	}

	private void InitializeStatusEffects()
	{
		m_immunityList = new List<StatusEffectType>();
		BaseStatusEffect[] components = GetComponents<BaseStatusEffect>();
		m_statusEffectTable = new Dictionary<StatusEffectType, BaseStatusEffect>(components.Length);
		BaseStatusEffect[] array = components;
		foreach (BaseStatusEffect baseStatusEffect in array)
		{
			m_statusEffectTable.Add(baseStatusEffect.StatusEffectType, baseStatusEffect);
			baseStatusEffect.Initialize(this, m_charController);
		}
		m_invulnStatusEffectStacks = new HashSet<StatusEffectType>();
		IsInitialized = true;
	}

	private void OnDisable()
	{
		if (!GameManager.IsApplicationClosing)
		{
			StopAllStatusEffects(interrupted: true);
		}
	}

	public void StartStatusEffect(StatusEffectType statusEffectType, float duration, IDamageObj caster)
	{
		if (DISABLE_ADDING_STATUS_EFFECTS || statusEffectType == StatusEffectType.None || ImmuneToAllStatusEffects || m_immunityList.Contains(statusEffectType))
		{
			return;
		}
		if (m_statusEffectTable.TryGetValue(statusEffectType, out var value))
		{
			if (duration == 0f)
			{
				duration = value.StartingDurationOverride;
			}
			if (duration != 0f)
			{
				value.StartEffect(duration, caster);
			}
		}
		else
		{
			Debug.Log("Could not execute StatusEffect: " + statusEffectType.ToString() + " on " + base.name + ". Status Effect not found.  Make sure m_statusEffectTable has the effect added");
		}
	}

	public void StopStatusEffect(StatusEffectType statusEffectType, bool interrupted)
	{
		if (m_statusEffectTable.TryGetValue(statusEffectType, out var value) && value.IsPlaying)
		{
			value.StopEffect(interrupted);
		}
		if (m_invulnStatusEffectStacks.Contains(statusEffectType))
		{
			m_invulnStatusEffectStacks.Remove(statusEffectType);
		}
	}

	public void StopAllStatusEffects(bool interrupted)
	{
		StatusEffectType[] typeArray = StatusEffectType_RL.TypeArray;
		foreach (StatusEffectType key in typeArray)
		{
			if (m_statusEffectTable.TryGetValue(key, out var value) && value.IsPlaying)
			{
				value.StopEffect(interrupted);
			}
		}
		m_invulnStatusEffectStacks.Clear();
	}

	public void AddStatusEffectImmunity(StatusEffectType immunityType)
	{
		if (!m_immunityList.Contains(immunityType))
		{
			m_immunityList.Add(immunityType);
		}
	}

	public void RemoveStatusEffectImmunity(StatusEffectType immunityType)
	{
		m_immunityList.Remove(immunityType);
	}

	public void SetStatusEffectHidden(StatusEffectType statusEffectType, bool hide)
	{
		BaseStatusEffect statusEffect = GetStatusEffect(statusEffectType);
		if ((bool)statusEffect && (hide || (!hide && statusEffect.IsPlaying)))
		{
			statusEffect.SetIsHidden(hide);
		}
	}

	public void SetAllStatusEffectsHidden(bool hide)
	{
		foreach (KeyValuePair<StatusEffectType, BaseStatusEffect> item in m_statusEffectTable)
		{
			SetStatusEffectHidden(item.Key, hide);
		}
		if (hide)
		{
			m_charController.StatusBarController.SetCanvasVisible(visible: false);
		}
		else
		{
			m_charController.StatusBarController.SetCanvasVisible(visible: true);
		}
	}

	public void AddStatusEffectInvulnStack(StatusEffectType statusEffectType)
	{
		m_invulnStatusEffectStacks.Add(statusEffectType);
	}

	public void RemoveStatusEffectInvulnStack(StatusEffectType statusEffectType)
	{
		if (m_invulnStatusEffectStacks.Contains(statusEffectType))
		{
			m_invulnStatusEffectStacks.Remove(statusEffectType);
		}
	}
}
