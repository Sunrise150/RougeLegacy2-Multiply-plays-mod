using System;
using UnityEngine;

[Serializable]
public class RelicObj
{
	private int m_level;

	private int m_intValue;

	private float m_floatValue;

	private bool m_wasSeen;

	public bool IsFreeRelic;

	private RelicModType m_relicModType;

	[NonSerialized]
	public bool CountsBackwards;

	private static RelicChangedEventArgs m_relicChangedArgs_STATIC = new RelicChangedEventArgs(RelicType.None);

	[NonSerialized]
	private Action<object, HealthChangeEventArgs> m_onPlayerHitLowHealth_nonSerialized;

	[NonSerialized]
	private Action<object, HealthChangeEventArgs> m_onPlayerHitMaxHealth_nonSerialized;

	private bool m_lowHealthStatBonusApplied;

	private bool m_maxHealthStatBonusApplied;

	[NonSerialized]
	private RelicChangedEventArgs m_relicChangedEventArgs;

	public bool WasSeen
	{
		get
		{
			return m_wasSeen;
		}
		private set
		{
			m_wasSeen = value;
		}
	}

	public RelicModType RelicModType => m_relicModType;

	public int Level => m_level;

	public int IntValue => m_intValue;

	public float FloatValue => m_floatValue;

	public RelicType RelicType { get; private set; }

	public void SetRelicMod(RelicModType relicMod)
	{
		m_relicModType = relicMod;
	}

	public void DebugForceLevel(int value)
	{
		m_level = value;
	}

	public void SetLevel(int value, bool additive, bool broadcast = true)
	{
		int level = m_level;
		if (additive)
		{
			m_level += value;
		}
		else
		{
			m_level = value;
		}
		m_level = Mathf.Clamp(m_level, 0, 99999);
		if (m_level > 0 && !WasSeen && !ChallengeManager.IsInChallenge)
		{
			WasSeen = true;
		}
		if (PlayerManager.IsInstantiated)
		{
			if (m_level > level)
			{
				ApplyRelic(m_level - level);
			}
			else if (m_level == 0)
			{
				StopRelic();
			}
		}
		if (broadcast)
		{
			m_relicChangedArgs_STATIC.Initialize(RelicType);
			Messenger<GameMessenger, GameEvent>.Broadcast(GameEvent.RelicLevelChanged, null, m_relicChangedArgs_STATIC);
		}
	}

	public void SetIntValue(int value, bool additive, bool broadcast = true)
	{
		if (additive)
		{
			m_intValue += value;
		}
		else
		{
			m_intValue = value;
		}
		if (broadcast)
		{
			m_relicChangedArgs_STATIC.Initialize(RelicType);
			Messenger<GameMessenger, GameEvent>.Broadcast(GameEvent.RelicStatsChanged, null, m_relicChangedArgs_STATIC);
		}
	}

	public void SetFloatValue(float value, bool additive, bool broadcast = true)
	{
		if (additive)
		{
			m_floatValue += value;
		}
		else
		{
			m_floatValue = value;
		}
		if (broadcast)
		{
			m_relicChangedArgs_STATIC.Initialize(RelicType);
			Messenger<GameMessenger, GameEvent>.Broadcast(GameEvent.RelicStatsChanged, null, m_relicChangedArgs_STATIC);
		}
	}

	public void ApplyRelic(int levelChange)
	{
		if (m_onPlayerHitLowHealth_nonSerialized == null)
		{
			m_onPlayerHitLowHealth_nonSerialized = OnPlayerHitLowHealth;
		}
		if (m_onPlayerHitMaxHealth_nonSerialized == null)
		{
			m_onPlayerHitMaxHealth_nonSerialized = OnPlayerHitMaxHealth;
		}
		PlayerController playerController = PlayerManager.GetPlayerController();
		switch (RelicType)
		{
		case RelicType.GodMode:
			playerController.StatusEffectController.StartStatusEffect(StatusEffectType.Player_GodMode, 25f, null);
			break;
		case RelicType.TakeNoDamage:
			m_level += levelChange * 3 - levelChange;
			break;
		case RelicType.LowHealthStatBonus:
			playerController.HealthChangeRelay.AddListener(m_onPlayerHitLowHealth_nonSerialized);
			break;
		case RelicType.MaxHealthStatBonus:
			playerController.HealthChangeRelay.AddListener(m_onPlayerHitMaxHealth_nonSerialized);
			break;
		case RelicType.ManaDamageReduction:
			CountsBackwards = true;
			break;
		case RelicType.AttackExhaust:
			playerController.InitializeExhaustMods();
			break;
		case RelicType.OnHitAreaDamage:
			if (playerController.isActiveAndEnabled)
			{
				playerController.StartOnHitAreaDamageTimer();
			}
			break;
		case RelicType.NoAttackDamageBonus:
			if (playerController.isActiveAndEnabled)
			{
				playerController.StartNoAttackDamageBonusTimer();
			}
			break;
		case RelicType.SpinKickLeavesCaltrops:
			if (playerController.isActiveAndEnabled)
			{
				playerController.StartSpinKicksDropCaltropsTimer();
			}
			break;
		}
	}

	public void StopRelic()
	{
		if (m_onPlayerHitLowHealth_nonSerialized == null)
		{
			m_onPlayerHitLowHealth_nonSerialized = OnPlayerHitLowHealth;
		}
		if (m_onPlayerHitMaxHealth_nonSerialized == null)
		{
			m_onPlayerHitMaxHealth_nonSerialized = OnPlayerHitMaxHealth;
		}
		PlayerController playerController = PlayerManager.GetPlayerController();
		switch (RelicType)
		{
		case RelicType.GodMode:
			playerController.StatusEffectController.StopStatusEffect(StatusEffectType.Player_GodMode, interrupted: true);
			break;
		case RelicType.LowHealthStatBonus:
			m_lowHealthStatBonusApplied = false;
			playerController.HealthChangeRelay.RemoveListener(m_onPlayerHitLowHealth_nonSerialized);
			break;
		case RelicType.MaxHealthStatBonus:
			m_maxHealthStatBonusApplied = false;
			playerController.HealthChangeRelay.RemoveListener(m_onPlayerHitMaxHealth_nonSerialized);
			break;
		case RelicType.OnHitAreaDamage:
			playerController.StopOnHitAreaDamageTimer();
			break;
		case RelicType.NoAttackDamageBonus:
			playerController.StopNoAttackDamageBonusTimer();
			break;
		case RelicType.SpinKickLeavesCaltrops:
			playerController.StopSpinKicksDropCaltropsTimer();
			break;
		}
	}

	private void OnPlayerHitLowHealth(object sender, EventArgs args)
	{
		PlayerController playerController = PlayerManager.GetPlayerController();
		bool lowHealthStatBonusApplied = m_lowHealthStatBonusApplied;
		if (!m_lowHealthStatBonusApplied)
		{
			if ((float)playerController.CurrentHealthAsInt <= 0.5f * (float)playerController.ActualMaxHealth)
			{
				m_lowHealthStatBonusApplied = true;
			}
		}
		else if (m_lowHealthStatBonusApplied && (float)playerController.CurrentHealthAsInt > 0.5f * (float)playerController.ActualMaxHealth)
		{
			m_lowHealthStatBonusApplied = false;
		}
		if (lowHealthStatBonusApplied != m_lowHealthStatBonusApplied)
		{
			playerController.InitializeStrengthMods();
			playerController.InitializeMagicMods();
		}
	}

	private void OnPlayerHitMaxHealth(object sender, EventArgs args)
	{
		PlayerController playerController = PlayerManager.GetPlayerController();
		bool maxHealthStatBonusApplied = m_maxHealthStatBonusApplied;
		if (!m_maxHealthStatBonusApplied)
		{
			if ((float)playerController.CurrentHealthAsInt >= 0.5f * (float)playerController.ActualMaxHealth)
			{
				m_maxHealthStatBonusApplied = true;
			}
		}
		else if (m_maxHealthStatBonusApplied && (float)playerController.CurrentHealthAsInt < 0.5f * (float)playerController.ActualMaxHealth)
		{
			m_maxHealthStatBonusApplied = false;
		}
		if (maxHealthStatBonusApplied != m_maxHealthStatBonusApplied)
		{
			playerController.InitializeStrengthMods();
			playerController.InitializeMagicMods();
		}
	}

	public RelicObj(RelicType relicType)
	{
		RelicType = relicType;
	}
}
