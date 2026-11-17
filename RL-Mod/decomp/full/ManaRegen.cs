using System;
using System.Collections;
using UnityEngine;

public class ManaRegen : MonoBehaviour
{
	private PlayerController m_playerController;

	private WaitRL_Yield m_regenDelayWaitYield;

	private float m_manaToAddThisUpdate;

	private bool m_enemyOnHitManaWasAdded;

	private Action<MonoBehaviour, EventArgs> m_onForceRegen;

	private Action<MonoBehaviour, EventArgs> m_onPlayerHealthChange;

	private Action<MonoBehaviour, EventArgs> m_onPlayerBlocked;

	private Action<MonoBehaviour, EventArgs> m_onPlayerHit;

	private Action<MonoBehaviour, EventArgs> m_onEnemyHit;

	private Action<MonoBehaviour, EventArgs> m_onEnemyKilled;

	private Action<MonoBehaviour, EventArgs> m_onItemCollected;

	private Action<MonoBehaviour, EventArgs> m_onBreakableDestroyed;

	private Action<ManaChangeEventArgs> m_onManaChange;

	public bool IsManaRegenDelayed { get; private set; }

	private ManaRegenType RegenType => m_playerController.CharacterClass.ClassData.PassiveData.ManaRegenType;

	private void Awake()
	{
		m_playerController = GetComponent<PlayerController>();
		m_regenDelayWaitYield = new WaitRL_Yield(0f);
		m_onForceRegen = OnForceRegen;
		m_onPlayerHealthChange = OnPlayerHealthChange;
		m_onPlayerBlocked = OnPlayerBlocked;
		m_onPlayerHit = OnPlayerHit;
		m_onEnemyHit = OnEnemyHit;
		m_onEnemyKilled = OnEnemyKilled;
		m_onItemCollected = OnItemCollected;
		m_onBreakableDestroyed = OnBreakableDestroyed;
		m_onManaChange = OnManaChange;
	}

	private void OnEnable()
	{
		Messenger<GameMessenger, GameEvent>.AddListener(GameEvent.PlayerForceManaRegen, m_onForceRegen);
		Messenger<GameMessenger, GameEvent>.AddListener(GameEvent.PlayerHealthChange, m_onPlayerHealthChange);
		Messenger<GameMessenger, GameEvent>.AddListener(GameEvent.PlayerBlocked, m_onPlayerBlocked);
		Messenger<GameMessenger, GameEvent>.AddListener(GameEvent.PlayerHit, m_onPlayerHit);
		Messenger<GameMessenger, GameEvent>.AddListener(GameEvent.EnemyHit, m_onEnemyHit);
		Messenger<GameMessenger, GameEvent>.AddListener(GameEvent.EnemyDeath, m_onEnemyKilled);
		Messenger<GameMessenger, GameEvent>.AddListener(GameEvent.ItemCollected, m_onItemCollected);
		Messenger<GameMessenger, GameEvent>.AddListener(GameEvent.BreakableDestroyed, m_onBreakableDestroyed);
		if ((bool)m_playerController && m_playerController.ManaChangeRelay != null)
		{
			m_playerController.ManaChangeRelay.AddListener(m_onManaChange);
		}
	}

	private void OnDisable()
	{
		Messenger<GameMessenger, GameEvent>.RemoveListener(GameEvent.PlayerForceManaRegen, m_onForceRegen);
		Messenger<GameMessenger, GameEvent>.RemoveListener(GameEvent.PlayerHealthChange, m_onPlayerHealthChange);
		Messenger<GameMessenger, GameEvent>.RemoveListener(GameEvent.PlayerBlocked, m_onPlayerBlocked);
		Messenger<GameMessenger, GameEvent>.RemoveListener(GameEvent.PlayerHit, m_onPlayerHit);
		Messenger<GameMessenger, GameEvent>.RemoveListener(GameEvent.EnemyHit, m_onEnemyHit);
		Messenger<GameMessenger, GameEvent>.RemoveListener(GameEvent.EnemyDeath, m_onEnemyKilled);
		Messenger<GameMessenger, GameEvent>.RemoveListener(GameEvent.ItemCollected, m_onItemCollected);
		Messenger<GameMessenger, GameEvent>.RemoveListener(GameEvent.BreakableDestroyed, m_onBreakableDestroyed);
		if ((bool)m_playerController && m_playerController.ManaChangeRelay != null)
		{
			m_playerController.ManaChangeRelay.RemoveListener(m_onManaChange);
		}
	}

	private void OnForceRegen(object sender, EventArgs args)
	{
		ForceManaRegenEventArgs forceManaRegenEventArgs = args as ForceManaRegenEventArgs;
		RegenPlayerManaFlat((int)forceManaRegenEventArgs.RegenAmount, forceManaRegenEventArgs.UsePlayerRegenMods);
	}

	private void OnManaChange(ManaChangeEventArgs args)
	{
		if (args.NewManaValue < args.PrevManaValue)
		{
			StartCoroutine(DelayManaRegen());
		}
	}

	private IEnumerator DelayManaRegen()
	{
		IsManaRegenDelayed = true;
		m_regenDelayWaitYield.CreateNew(2f);
		yield return m_regenDelayWaitYield;
		IsManaRegenDelayed = false;
	}

	private void OnPlayerHealthChange(MonoBehaviour sender, EventArgs args)
	{
		if (RegenType == ManaRegenType.OnPlayerHit)
		{
			HealthChangeEventArgs healthChangeEventArgs = args as HealthChangeEventArgs;
			if (healthChangeEventArgs.NewHealthValue < healthChangeEventArgs.PrevHealthValue)
			{
				float num = EquipmentManager.Get_EquipmentSet_BonusTypeStatGain(EquipmentSetBonusType.ManaChargeInjury);
				RegenPlayerMana(0.1f + num, applyManaRegenMods: true);
			}
		}
	}

	private void OnPlayerHit(MonoBehaviour sender, EventArgs args)
	{
		if (TraitManager.IsTraitActive(TraitType.ManaFromHurt))
		{
			CharacterHitEventArgs obj = args as CharacterHitEventArgs;
			RegenPlayerManaFlat((int)((float)PlayerManager.GetPlayerController().ActualMaxMana * 0.5f), applyManaRegenMods: false);
			EffectManager.PlayEffect(obj.Attacker.gameObject, null, "ManaRegenBurst_Effect", Vector3.zero, 0f, EffectStopType.Gracefully);
		}
	}

	private void OnPlayerBlocked(MonoBehaviour sender, EventArgs args)
	{
		if (RegenType == ManaRegenType.OnPlayerBlock)
		{
			RegenPlayerMana(0.1f, applyManaRegenMods: true);
		}
	}

	private void OnEnemyHit(MonoBehaviour sender, EventArgs args)
	{
		if (RegenType != ManaRegenType.OnEnemyHit || TraitManager.IsTraitActive(TraitType.ManaFromHurt))
		{
			return;
		}
		CharacterHitEventArgs characterHitEventArgs = args as CharacterHitEventArgs;
		EnemyController enemyController = characterHitEventArgs.Victim as EnemyController;
		if (!enemyController)
		{
			return;
		}
		int num = 0;
		int num2 = 0;
		bool flag = SaveManager.PlayerSaveData.CurrentCharacter.ClassType == ClassType.AstroClass;
		if (characterHitEventArgs.Attacker.gameObject.CompareTag("PlayerProjectile"))
		{
			if (enemyController.DisableHPMPBonuses || enemyController.TakesNoDamage)
			{
				enemyController.StatusEffectController.StopStatusEffect(StatusEffectType.Enemy_ManaBurn, interrupted: true);
				return;
			}
			if (!m_enemyOnHitManaWasAdded)
			{
				m_enemyOnHitManaWasAdded = true;
				num += (int)(characterHitEventArgs.Attacker as Projectile_RL).ManaGainPerHit;
			}
			if (flag)
			{
				num2++;
			}
		}
		else if (flag && characterHitEventArgs.Attacker.gameObject.CompareTag("EnemyStatusEffect"))
		{
			num2++;
		}
		Projectile_RL projectile_RL = characterHitEventArgs.Attacker as Projectile_RL;
		bool flag2 = (bool)projectile_RL && projectile_RL.CastAbilityType == CastAbilityType.Talent;
		if (num + num2 > 0 && RegenPlayerManaFlat(num, !flag2) + RegenPlayerManaFlat(num2, applyManaRegenMods: false) > 0)
		{
			EffectManager.PlayEffect(enemyController.gameObject, null, "ManaRegenBurst_Effect", Vector3.zero, 0f, EffectStopType.Gracefully);
		}
	}

	private void OnEnemyKilled(MonoBehaviour sender, EventArgs args)
	{
		if (RegenType == ManaRegenType.OnEnemyKilled && (args == null || !(args is EnemyDeathEventArgs enemyDeathEventArgs) || !enemyDeathEventArgs.Victim.DisableHPMPBonuses))
		{
			RegenPlayerMana(0.05f, applyManaRegenMods: true);
		}
	}

	private void OnItemCollected(MonoBehaviour sender, EventArgs args)
	{
		if (RegenType == ManaRegenType.OnGoldCollected && Economy_EV.GetItemDropValue((args as ItemCollectedEventArgs).Item.ItemDropType) > 0)
		{
			RegenPlayerMana(0.1f, applyManaRegenMods: true);
		}
	}

	private void OnBreakableDestroyed(MonoBehaviour sender, EventArgs args)
	{
		if (TraitManager.IsTraitActive(TraitType.BreakPropsForMana))
		{
			RegenPlayerMana(0.01f, applyManaRegenMods: true);
		}
	}

	private void Update()
	{
		if (!TraitManager.IsTraitActive(TraitType.BonusMagicStrength))
		{
			return;
		}
		float num = m_playerController.ActualMaxMana;
		float currentMana = m_playerController.CurrentMana;
		float num2 = Mathf.CeilToInt(1f * num);
		if (currentMana < num2 && !IsManaRegenDelayed)
		{
			float num3 = Mathf.CeilToInt(0.75f * num * Time.deltaTime);
			if (num3 + currentMana > num2)
			{
				m_playerController.SetMana(num2, additive: false, runEvents: true);
			}
			else
			{
				m_playerController.SetMana(num3, additive: true, runEvents: true);
			}
		}
	}

	private void RegenPlayerMana(float regenPercent, bool applyManaRegenMods)
	{
		float num = regenPercent * (float)m_playerController.ActualMaxMana;
		if (applyManaRegenMods)
		{
			num *= 1f + m_playerController.ManaRegenMod;
		}
		m_manaToAddThisUpdate += num;
	}

	private int RegenPlayerManaFlat(int regenFlat, bool applyManaRegenMods)
	{
		if (applyManaRegenMods)
		{
			regenFlat = Mathf.CeilToInt((float)regenFlat * (1f + m_playerController.ManaRegenMod));
		}
		m_manaToAddThisUpdate += regenFlat;
		return regenFlat;
	}

	private void LateUpdate()
	{
		if (!m_playerController)
		{
			return;
		}
		if (m_manaToAddThisUpdate > 0f)
		{
			bool flag = TraitManager.IsTraitActive(TraitType.NoManaCap);
			if (m_playerController.CurrentManaAsInt < m_playerController.ActualMaxMana || flag)
			{
				m_manaToAddThisUpdate = Mathf.CeilToInt(m_manaToAddThisUpdate);
				m_playerController.SetMana(m_manaToAddThisUpdate, additive: true, runEvents: true);
				string text = ((m_playerController.CurrentManaAsInt < m_playerController.ActualMaxMana || flag) ? string.Format(LocalizationManager.GetString("LOC_ID_STATUS_EFFECT_MANA_RESTORE_1", SaveManager.PlayerSaveData.CurrentCharacter.IsFemale), (int)m_manaToAddThisUpdate) : LocalizationManager.GetString("LOC_ID_STATUS_EFFECT_MAX_MANA_1", SaveManager.PlayerSaveData.CurrentCharacter.IsFemale));
				TextPopupManager.DisplayTextDefaultPos(TextPopupType.MPGained, text, m_playerController, attachToSource: true);
			}
			m_manaToAddThisUpdate = 0f;
		}
		m_enemyOnHitManaWasAdded = false;
	}
}
