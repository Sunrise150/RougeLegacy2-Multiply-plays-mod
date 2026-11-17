using System;
using SceneManagement_RL;
using TMPro;
using UnityEngine;

public class RunStatisticsHUDController : MonoBehaviour
{
	[SerializeField]
	private GameObject m_panel;

	[SerializeField]
	private TextMeshProUGUI m_durationText;

	[SerializeField]
	private TextMeshProUGUI m_expCountText;

	[SerializeField]
	private TextMeshProUGUI m_chestCountText;

	[SerializeField]
	private TextMeshProUGUI m_blueprintCountText;

	[SerializeField]
	private TextMeshProUGUI m_runeCountText;

	[SerializeField]
	private bool m_isEnabledInEditor = true;

	private bool m_isSubscribed;

	private bool m_isPaused;

	private float m_elapsedTime;

	private int m_chestCount;

	private int m_blueprintCount;

	private int m_runeCount;

	private SceneID m_currentScene;

	private Action<MonoBehaviour, EventArgs> m_onChestOpened;

	private Action<MonoBehaviour, EventArgs> m_onSpecialItemDropped;

	private bool IsRunning => m_currentScene == SceneID.World;

	private void Awake()
	{
		m_onChestOpened = OnChestOpened;
		m_onSpecialItemDropped = OnSpecialItemDropped;
	}

	private void Start()
	{
		if (Application.isEditor)
		{
			m_currentScene = SceneLoadingUtility.GetSceneID(SceneLoadingUtility.ActiveScene.name);
			if (m_isEnabledInEditor)
			{
				SubscribeToEvents();
			}
			if (!m_isEnabledInEditor || m_currentScene != SceneID.World)
			{
				m_panel.SetActive(value: false);
			}
		}
		else
		{
			m_panel.SetActive(value: false);
		}
	}

	private void OnDestroy()
	{
		if (Application.isPlaying && this != null && m_isSubscribed)
		{
			UnsubscribeFromEvents();
			m_isSubscribed = false;
		}
	}

	private void SubscribeToEvents()
	{
		m_isSubscribed = true;
		SceneLoader_RL.SceneLoadingEndRelay.AddListener(OnSceneLoaded);
		Messenger<GameMessenger, GameEvent>.AddListener(GameEvent.ChestOpened, m_onChestOpened);
		Messenger<GameMessenger, GameEvent>.AddListener(GameEvent.SpecialItemsDropped, m_onSpecialItemDropped);
	}

	private void UnsubscribeFromEvents()
	{
		SceneLoader_RL.SceneLoadingEndRelay.RemoveListener(OnSceneLoaded);
		Messenger<GameMessenger, GameEvent>.RemoveListener(GameEvent.ChestOpened, m_onChestOpened);
		Messenger<GameMessenger, GameEvent>.RemoveListener(GameEvent.SpecialItemsDropped, m_onSpecialItemDropped);
	}

	private void OnSceneLoaded(string sceneName)
	{
		m_currentScene = SceneLoadingUtility.GetSceneID(sceneName);
		m_elapsedTime = 0f;
		m_chestCount = 0;
		m_blueprintCount = 0;
		m_runeCount = 0;
		m_durationText.text = "Duration = 0";
		m_chestCountText.text = "Chest Count = 0";
		m_blueprintCountText.text = "Blueprint Count = 0";
		m_runeCountText.text = "Rune Count = 0";
		m_expCountText.text = "EXP = 0";
		m_panel.SetActive(m_currentScene == SceneID.World);
	}

	private void OnSpecialItemDropped(MonoBehaviour sender, EventArgs eventArgs)
	{
		switch ((eventArgs as SpecialItemDroppedEventArgs).SpecialItemDrop.SpecialItemType)
		{
		case SpecialItemType.Blueprint:
			m_blueprintCount++;
			m_blueprintCountText.text = $"Blueprint Count = {m_chestCount}";
			break;
		case SpecialItemType.Rune:
			m_runeCount++;
			m_runeCountText.text = $"Rune Count = {m_chestCount}";
			break;
		}
	}

	private void OnChestOpened(MonoBehaviour sender, EventArgs eventArgs)
	{
		m_chestCount++;
		m_chestCountText.text = $"Chest Count = {m_chestCount}";
	}

	private void Update()
	{
		if (!IsRunning)
		{
			return;
		}
		m_isPaused = Time.timeScale == 0f;
		if (!m_isPaused)
		{
			m_elapsedTime += Time.deltaTime;
			TimeSpan timeSpan = TimeSpan.FromSeconds(m_elapsedTime);
			m_durationText.text = string.Format("Duration = {0}", timeSpan.ToString("mm\\:ss"));
			if (SaveManager.IsInitialized)
			{
				int runAccumulatedXP = SaveManager.PlayerSaveData.RunAccumulatedXP;
				m_expCountText.text = $"EXP = {runAccumulatedXP}";
			}
		}
	}
}
