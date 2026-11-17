using System;
using System.Collections;
using UnityEngine;

public class SuperHealer_Trait : BaseTrait
{
	private PlayerController m_playerController;

	private MaxHealthChangeEventArgs m_healthChangeArgs;

	private Action<MonoBehaviour, EventArgs> m_onPlayerHit;

	public override TraitType TraitType => TraitType.SuperHealer;

	protected override void Awake()
	{
		base.Awake();
		m_playerController = PlayerManager.GetPlayerController();
		m_healthChangeArgs = new MaxHealthChangeEventArgs(0f, 0f);
		m_onPlayerHit = OnPlayerHit;
	}

	private void OnEnable()
	{
		Messenger<GameMessenger, GameEvent>.AddListener(GameEvent.PlayerHit, m_onPlayerHit);
	}

	private void OnDisable()
	{
		Messenger<GameMessenger, GameEvent>.RemoveListener(GameEvent.PlayerHit, m_onPlayerHit);
	}

	private void OnPlayerHit(MonoBehaviour sender, EventArgs args)
	{
		if (!m_playerController.TakesNoDamage && !((args as CharacterHitEventArgs).DamageTaken <= 0f) && (!PlayerManager.IsInstantiated || PlayerManager.GetCurrentPlayerRoom().BiomeType != BiomeType.HubTown))
		{
			int actualMaxHealth = m_playerController.ActualMaxHealth;
			SaveManager.PlayerSaveData.TemporaryMaxHealthMods -= 0.0625f;
			m_playerController.InitializeHealthMods();
			int actualMaxHealth2 = m_playerController.ActualMaxHealth;
			m_healthChangeArgs.Initialise(actualMaxHealth2, actualMaxHealth);
			Messenger<GameMessenger, GameEvent>.Broadcast(GameEvent.PlayerMaxHealthChange, this, m_healthChangeArgs);
			StopAllCoroutines();
			StartCoroutine(DelayHealthRegen());
		}
	}

	private IEnumerator DelayHealthRegen()
	{
		float delay = Time.time + 2.5f;
		while (Time.time < delay)
		{
			yield return null;
		}
		while (m_playerController.CurrentHealthAsInt < m_playerController.ActualMaxHealth)
		{
			int num = Mathf.CeilToInt(0.05f * (float)m_playerController.ActualMaxHealth);
			m_playerController.SetHealth(num, additive: true, runEvents: true);
			string text = string.Format(LocalizationManager.GetString("LOC_ID_STATUS_EFFECT_HEALTH_RESTORE_1", SaveManager.PlayerSaveData.CurrentCharacter.IsFemale), num);
			TextPopupManager.DisplayTextDefaultPos(TextPopupType.HPGained, text, m_playerController, attachToSource: true);
			float regenTick = Time.time + 0.25f;
			while (Time.time < regenTick)
			{
				yield return null;
			}
		}
	}
}
