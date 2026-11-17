using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EnemyHUDController : MonoBehaviour
{
	public static string RegularEnemyLocIDOverride = "";

	public static string MinibossEnemyLocIDOverride = "";

	public static string BossEnemyLocIDOverride = "";

	private const float DISPLAY_DURATION = 1.5f;

	private const float HP_CHANGE_DURATION = 0.1f;

	private const int NUM_BLINKS = 4;

	private const float BLINK_DURATION = 0.15f;

	[SerializeField]
	private EnemyHUDType m_hudType;

	[SerializeField]
	private TMP_Text m_levelText;

	[SerializeField]
	private Image m_hpBar;

	[SerializeField]
	private Image m_hpLossBar;

	[SerializeField]
	private Image m_hpGainBar;

	[SerializeField]
	private TMP_Text m_enemyNameText;

	[SerializeField]
	private TMP_Text m_commanderText;

	[SerializeField]
	private CanvasGroup m_canvasGroup;

	private WaitRL_Yield m_waitYield;

	private float m_storedHUDAlpha;

	private Action<MonoBehaviour, EventArgs> m_onEnemyHealthChange;

	private Action<MonoBehaviour, EventArgs> m_onPlayerDeath;

	private Action<MonoBehaviour, EventArgs> m_onPause;

	private Action<MonoBehaviour, EventArgs> m_onUnpause;

	private static string GetGlobalLocIDOverride(EnemyHUDType hudType)
	{
		return hudType switch
		{
			EnemyHUDType.Regular => RegularEnemyLocIDOverride, 
			EnemyHUDType.Miniboss => MinibossEnemyLocIDOverride, 
			EnemyHUDType.Boss => BossEnemyLocIDOverride, 
			_ => "", 
		};
	}

	private void Awake()
	{
		m_onEnemyHealthChange = OnEnemyHealthChange;
		m_onPause = OnPause;
		m_onUnpause = OnUnpause;
		m_onPlayerDeath = OnPlayerDeath;
		Messenger<GameMessenger, GameEvent>.AddListener(GameEvent.EnemyHealthChange, m_onEnemyHealthChange);
		Messenger<GameMessenger, GameEvent>.AddListener(GameEvent.PlayerDeath, m_onPlayerDeath);
		Messenger<GameMessenger, GameEvent>.AddListener(GameEvent.PlayerFakedDeath, m_onPlayerDeath);
		Messenger<UIMessenger, UIEvent>.AddListener(UIEvent.PauseWindow_Opened, m_onPause);
		Messenger<UIMessenger, UIEvent>.AddListener(UIEvent.PauseWindow_Closed, m_onUnpause);
		Messenger<UIMessenger, UIEvent>.AddListener(UIEvent.HideEnemyHUD, m_onPlayerDeath);
		m_waitYield = new WaitRL_Yield(1.5f);
		base.gameObject.SetActive(value: false);
	}

	private void OnDestroy()
	{
		Messenger<GameMessenger, GameEvent>.RemoveListener(GameEvent.EnemyHealthChange, m_onEnemyHealthChange);
		Messenger<GameMessenger, GameEvent>.RemoveListener(GameEvent.PlayerDeath, m_onPlayerDeath);
		Messenger<GameMessenger, GameEvent>.RemoveListener(GameEvent.PlayerFakedDeath, m_onPlayerDeath);
		Messenger<UIMessenger, UIEvent>.RemoveListener(UIEvent.PauseWindow_Opened, m_onPause);
		Messenger<UIMessenger, UIEvent>.RemoveListener(UIEvent.PauseWindow_Closed, m_onUnpause);
		Messenger<UIMessenger, UIEvent>.RemoveListener(UIEvent.HideEnemyHUD, m_onPlayerDeath);
	}

	private void OnPlayerDeath(MonoBehaviour sender, EventArgs args)
	{
		StopAllCoroutines();
		base.gameObject.SetActive(value: false);
	}

	private void OnEnemyHealthChange(MonoBehaviour sender, EventArgs args)
	{
		if (TraitManager.IsTraitActive(TraitType.NoEnemyHealthBar))
		{
			StopAllCoroutines();
			base.gameObject.SetActive(value: false);
		}
		else
		{
			if (!base.transform.parent.gameObject.activeInHierarchy)
			{
				return;
			}
			EnemyController enemyController = sender as EnemyController;
			HealthChangeEventArgs healthChangeEventArgs = args as HealthChangeEventArgs;
			if (healthChangeEventArgs.NewHealthValue == healthChangeEventArgs.PrevHealthValue)
			{
				return;
			}
			StopAllCoroutines();
			base.gameObject.SetActive(value: false);
			m_hpGainBar.gameObject.SetActive(value: false);
			m_hpLossBar.gameObject.SetActive(value: false);
			if (0 == 0 && (m_hudType != EnemyHUDType.Boss || enemyController.IsBoss) && (m_hudType == EnemyHUDType.Boss || !enemyController.IsBoss) && (enemyController.IsBoss || ((enemyController.EnemyRank != EnemyRank.Miniboss || m_hudType == EnemyHUDType.Miniboss) && (enemyController.EnemyRank == EnemyRank.Miniboss || m_hudType != EnemyHUDType.Miniboss))))
			{
				string commanderText = null;
				if (enemyController.IsCommander)
				{
					commanderText = GetCommanderText(enemyController);
				}
				float newPercent = healthChangeEventArgs.NewHealthValue / (float)enemyController.ActualMaxHealth;
				float prevPercent = healthChangeEventArgs.PrevHealthValue / (float)enemyController.ActualMaxHealth;
				EnemyData enemyData = EnemyClassLibrary.GetEnemyClassData(enemyController.EnemyType).GetEnemyData(enemyController.EnemyRank);
				DisplayHealthBar(newPercent, prevPercent, enemyController.Level, enemyData.Title, enemyController.CurrentHealth <= 0f, commanderText);
			}
		}
	}

	private string GetCommanderText(EnemyController enemy)
	{
		string text = LocalizationManager.GetString("LOC_ID_INDEX_COMMANDER_BUFFS_COMMANDER_TITLE_1", isFemale: false);
		StatusEffectType[] cOMMANDER_STATUS_EFFECT_ARRAY = StatusEffect_EV.COMMANDER_STATUS_EFFECT_ARRAY;
		foreach (StatusEffectType statusEffectType in cOMMANDER_STATUS_EFFECT_ARRAY)
		{
			if (enemy.HasCommanderStatusEffect(statusEffectType) && StatusEffect_EV.STATUS_EFFECT_TITLE_LOC_IDS.TryGetValue(statusEffectType, out var value))
			{
				text = text + ", " + LocalizationManager.GetString(value, isFemale: false);
			}
		}
		return text.Replace(text, "(" + text + ")");
	}

	private void DisplayHealthBar(float newPercent, float prevPercent, int enemyLevel, string locID, bool isKilled, string commanderText)
	{
		enemyLevel = Mathf.FloorToInt((float)enemyLevel * 2.5f);
		string globalLocIDOverride = GetGlobalLocIDOverride(m_hudType);
		if (!string.IsNullOrEmpty(globalLocIDOverride))
		{
			locID = globalLocIDOverride;
		}
		base.gameObject.SetActive(value: true);
		m_canvasGroup.alpha = 1f;
		m_hpBar.fillAmount = newPercent;
		m_levelText.text = string.Format(LocalizationManager.GetString("LOC_ID_HUD_TITLE_CHARACTER_LEVEL_1", isFemale: false), enemyLevel);
		m_enemyNameText.text = LocalizationManager.GetString(locID, isFemale: false);
		if (!string.IsNullOrEmpty(commanderText))
		{
			if (!m_commanderText.gameObject.activeSelf)
			{
				m_commanderText.gameObject.SetActive(value: true);
			}
			m_commanderText.text = commanderText;
		}
		else if (m_commanderText.gameObject.activeSelf)
		{
			m_commanderText.gameObject.SetActive(value: false);
		}
		if (!isKilled)
		{
			StartCoroutine(HPGainLossAnimCoroutine(newPercent, prevPercent));
			StartCoroutine(TimedDisplayCoroutine());
		}
		else
		{
			StartCoroutine(BlinkDisplayCoroutine());
		}
	}

	private IEnumerator HPGainLossAnimCoroutine(float newFill, float prevFill)
	{
		if (prevFill > newFill)
		{
			m_hpLossBar.gameObject.SetActive(value: true);
			m_hpBar.fillAmount = newFill;
			m_hpLossBar.fillAmount = prevFill;
		}
		else
		{
			m_hpGainBar.gameObject.SetActive(value: true);
			m_hpBar.fillAmount = prevFill;
			m_hpGainBar.fillAmount = newFill;
		}
		float delay2 = Time.time + 0.1f;
		float fillAmount = newFill - prevFill;
		while (Time.time < delay2)
		{
			yield return null;
		}
		delay2 = Time.time + 0.1f;
		float startTime = Time.time;
		while (Time.time < delay2)
		{
			float num = (Time.time - startTime) / 0.1f;
			if (prevFill > newFill)
			{
				m_hpLossBar.fillAmount = prevFill + fillAmount * num;
			}
			else
			{
				m_hpBar.fillAmount = prevFill + fillAmount * num;
			}
			yield return null;
		}
		if (prevFill > newFill)
		{
			m_hpLossBar.fillAmount = newFill;
		}
		else
		{
			m_hpBar.fillAmount = newFill;
		}
	}

	private IEnumerator TimedDisplayCoroutine()
	{
		m_waitYield.CreateNew(1.5f);
		yield return m_waitYield;
		base.gameObject.SetActive(value: false);
	}

	private IEnumerator BlinkDisplayCoroutine()
	{
		for (int counter = 0; counter < 4; counter++)
		{
			m_canvasGroup.alpha = 0.25f;
			m_waitYield.CreateNew(0.075f);
			yield return m_waitYield;
			m_canvasGroup.alpha = 1f;
			m_waitYield.CreateNew(0.15f);
			yield return m_waitYield;
		}
		base.gameObject.SetActive(value: false);
	}

	private void OnPause(object sender, EventArgs eventArgs)
	{
		if ((bool)m_canvasGroup)
		{
			m_storedHUDAlpha = m_canvasGroup.alpha;
			m_canvasGroup.alpha = 0f;
		}
	}

	private void OnUnpause(object sender, EventArgs eventArgs)
	{
		if ((bool)m_canvasGroup)
		{
			m_canvasGroup.alpha = m_storedHUDAlpha;
		}
	}
}
