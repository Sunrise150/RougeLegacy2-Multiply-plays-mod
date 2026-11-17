using System;
using System.Collections;
using RL_Windows;
using UnityEngine;

public class DriftHouseInteriorController : MonoBehaviour
{
	private Room m_room;

	private float m_regenTick;

	private InsightObjectiveCompleteHUDEventArgs m_insightEventArgs;

	private void Awake()
	{
		m_room = GetComponent<Room>();
		m_room.PlayerEnterRelay.AddListener(OnPlayerEnterRoom);
		m_insightEventArgs = new InsightObjectiveCompleteHUDEventArgs(InsightType.ScarChallenges_Complete, discovered: true);
	}

	private void OnDestroy()
	{
		m_room.PlayerEnterRelay.RemoveListener(OnPlayerEnterRoom);
	}

	private void FixedUpdate()
	{
		if (Time.time < m_regenTick + 0.05f)
		{
			return;
		}
		m_regenTick = Time.time;
		if (!PlayerManager.IsInstantiated)
		{
			return;
		}
		PlayerController playerController = PlayerManager.GetPlayerController();
		float currentMana = playerController.CurrentMana;
		float num = playerController.ActualMaxMana;
		if (currentMana < num)
		{
			float num2 = 100f * Time.fixedDeltaTime;
			if (currentMana + num2 > num)
			{
				playerController.SetMana(num, additive: false, runEvents: true);
			}
			else
			{
				playerController.SetMana(100f * Time.fixedDeltaTime, additive: true, runEvents: true);
			}
		}
		if (playerController.CurrentHealth < (float)playerController.ActualMaxHealth)
		{
			playerController.SetHealth(100f * Time.fixedDeltaTime, additive: true, runEvents: true);
		}
		if (playerController.CurrentArmor < playerController.ActualArmor)
		{
			playerController.CurrentArmor += Mathf.CeilToInt(100f * Time.fixedDeltaTime);
			playerController.SetHealth(0f, additive: true, runEvents: true);
		}
	}

	private void OnPlayerEnterRoom(object sender, EventArgs args)
	{
		if (PlayerManager.IsInstantiated)
		{
			PlayerController playerController = PlayerManager.GetPlayerController();
			playerController.ResetHealth();
			playerController.ResetMana();
		}
		if (WindowManager.GetIsWindowLoaded(WindowID.Jukebox))
		{
			(WindowManager.GetWindowController(WindowID.Jukebox) as JukeboxOmniUIWindowController).JukeboxSpectrum.StopSpectrum();
		}
		Debug.Log("trophy count: " + ChallengeManager.GetTotalTrophyCount());
		StartCoroutine(ResetEggplantHealthCoroutine());
		if (SaveManager.PlayerSaveData.GetInsightState(InsightType.ScarChallenges_Complete) < InsightState.ResolvedButNotViewed && (SaveManager.ModeSaveData.HasBronzeSisyphusTrophy || SaveManager.ModeSaveData.HasSilverSisyphusTrophy || SaveManager.ModeSaveData.HasGoldSisyphusTrophy))
		{
			SaveManager.PlayerSaveData.SetInsightState(InsightType.ScarChallenges_Complete, InsightState.ResolvedButNotViewed, forceOverride: false);
			m_insightEventArgs.Initialize(InsightType.ScarChallenges_Complete, discovered: false);
			Messenger<UIMessenger, UIEvent>.Broadcast(UIEvent.DisplayObjectiveCompleteHUD, this, m_insightEventArgs);
		}
		if (SaveManager.PlayerSaveData.GetInsightState(InsightType.ScarChallenges_Complete) < InsightState.DiscoveredButNotViewed && ChallengeManager.GetTotalTrophiesEarned(includeTutorialPurified: false) >= ChallengeManager.GetTotalTrophyCount() - 1)
		{
			SaveManager.PlayerSaveData.SetInsightState(InsightType.ScarChallenges_Complete, InsightState.DiscoveredButNotViewed, forceOverride: false);
			m_insightEventArgs.Initialize(InsightType.ScarChallenges_Complete, discovered: true);
			Messenger<UIMessenger, UIEvent>.Broadcast(UIEvent.DisplayObjectiveCompleteHUD, this, m_insightEventArgs);
		}
	}

	private IEnumerator ResetEggplantHealthCoroutine()
	{
		yield return null;
		EnemySpawnController[] enemySpawnControllers = m_room.SpawnControllerManager.EnemySpawnControllers;
		foreach (EnemySpawnController enemySpawnController in enemySpawnControllers)
		{
			if ((bool)enemySpawnController.EnemyInstance)
			{
				enemySpawnController.EnemyInstance.ResetHealth();
			}
		}
	}
}
