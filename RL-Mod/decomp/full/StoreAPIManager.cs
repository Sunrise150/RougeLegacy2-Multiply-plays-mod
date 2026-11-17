using System;
using System.Collections;
using Steamworks;
using UnityEngine;

public class StoreAPIManager : MonoBehaviour
{
	public enum StoreInitState
	{
		LoggingIn,
		Succeeded,
		Failed
	}

	private interface IStoreAPI
	{
		StoreType StoreType { get; }

		float InitTimeout { get; }

		IEnumerator Init();

		void GetInitFailureMessage(out string title, out string description);

		string GetUserID();

		void GiveAchievement(AchievementType achievementType);

		void GiveAllUnlockedAchievements();
	}

	private class NoStore : IStoreAPI
	{
		public StoreType StoreType => StoreType.None;

		public float InitTimeout => 0f;

		public IEnumerator Init()
		{
			InitState = StoreInitState.Succeeded;
			yield break;
		}

		public void GetInitFailureMessage(out string title, out string description)
		{
			throw new Exception("This should never get called");
		}

		public string GetUserID()
		{
			return string.Empty;
		}

		public void GiveAchievement(AchievementType achievementType)
		{
		}

		public void GiveAllUnlockedAchievements()
		{
		}
	}

	private class SteamStore : IStoreAPI
	{
		private Callback<UserStatsReceived_t> m_userStatsRecievedCallback;

		private Callback<UserStatsStored_t> m_userStatsStoredCallback;

		private Callback<UserAchievementStored_t> m_userAchievementStoredCallback;

		private bool m_statsReceieved;

		private const ulong GAME_ID = 1253920uL;

		public StoreType StoreType => StoreType.Steam;

		public float InitTimeout => 3f;

		public IEnumerator Init()
		{
			while (!SteamManager.Initialized)
			{
				yield return null;
			}
			m_userStatsRecievedCallback = Callback<UserStatsReceived_t>.Create(OnStatsReceived);
			m_userStatsStoredCallback = Callback<UserStatsStored_t>.Create(OnStatsStored);
			m_userAchievementStoredCallback = Callback<UserAchievementStored_t>.Create(OnUserAchievementStored);
			if (!SteamUserStats.RequestCurrentStats())
			{
				Debug.Log("RequestCurrentStats failed");
			}
			InitState = StoreInitState.Succeeded;
		}

		private void OnStatsReceived(UserStatsReceived_t data)
		{
			if (data.m_nGameID == 1253920)
			{
				if (data.m_eResult == EResult.k_EResultOK)
				{
					m_statsReceieved = true;
					Debug.Log("Steam Stats Received");
				}
				else
				{
					Debug.LogFormat("RequestCurrentStats failed with error code: {0}", data.m_eResult);
				}
			}
		}

		private void OnStatsStored(UserStatsStored_t data)
		{
			if (data.m_nGameID == 1253920 && data.m_eResult != EResult.k_EResultOK)
			{
				Debug.LogFormat("StoreStats callback failed with error code {0}", data.m_eResult);
			}
		}

		private void OnUserAchievementStored(UserAchievementStored_t data)
		{
		}

		public void GetInitFailureMessage(out string title, out string description)
		{
			title = LocalizationManager.GetString("LOC_ID_CONFIRM_MENU_STEAM_FAIL_TITLE_1", isFemale: false);
			description = LocalizationManager.GetString("LOC_ID_CONFIRM_MENU_STEAM_FAIL_DESCRIPTION_1", isFemale: false);
		}

		public string GetUserID()
		{
			return SteamUser.GetSteamID().ToString();
		}

		public void GiveAchievement(AchievementType achievementType)
		{
			if (SteamManager.Initialized && m_statsReceieved)
			{
				SteamUserStats.SetAchievement(achievementType.ToString());
				if (!SteamUserStats.StoreStats())
				{
					Debug.Log("Failed to StoreStats");
				}
			}
		}

		public void GiveAllUnlockedAchievements()
		{
			if (!SteamManager.Initialized || !m_statsReceieved)
			{
				return;
			}
			bool flag = false;
			AchievementType[] typeArray = AchievementType_RL.TypeArray;
			for (int i = 0; i < typeArray.Length; i++)
			{
				AchievementType achievementType = typeArray[i];
				if (achievementType != 0 && AchievementLibrary.GetAchievementData(achievementType) != null && SaveManager.ModeSaveData.GetAchievementUnlocked(achievementType))
				{
					SteamUserStats.SetAchievement(achievementType.ToString());
					flag = true;
				}
			}
			if (flag && !SteamUserStats.StoreStats())
			{
				Debug.Log("Failed to StoreStats");
			}
		}
	}

	private static IStoreAPI m_storeInstance;

	public static StoreInitState InitState { get; private set; }

	public static StoreAPIManager Instance { get; private set; }

	private void Awake()
	{
		Instance = this;
		StartCoroutine(InitializeStoreAPI());
	}

	private IEnumerator InitializeStoreAPI()
	{
		m_storeInstance = new SteamStore();
		yield return m_storeInstance.Init();
	}

	public static string GetPlatformDirectoryName()
	{
		return "Steam";
	}

	public static float GetInitTimeout()
	{
		return m_storeInstance.InitTimeout;
	}

	public static void GetInitFailureMessage(out string title, out string description)
	{
		m_storeInstance.GetInitFailureMessage(out title, out description);
	}

	public static string GetUserIDString()
	{
		return m_storeInstance.GetUserID();
	}

	public static void GiveAchievement(AchievementType achievementType, StoreType storeMask)
	{
		if (AchievementLibrary.GetAchievementData(achievementType) != null && !SaveManager.ModeSaveData.GetAchievementUnlocked(achievementType))
		{
			SaveManager.ModeSaveData.SetAchievementUnlocked(achievementType, unlocked: true);
			m_storeInstance.GiveAchievement(achievementType);
			CheckForAllAchievementsAchievement();
		}
	}

	public static void GiveAllUnlockedAchievements()
	{
		m_storeInstance.GiveAllUnlockedAchievements();
		CheckForAllAchievementsAchievement();
	}

	private static void CheckForAllAchievementsAchievement()
	{
		bool flag = true;
		AchievementType[] typeArray = AchievementType_RL.TypeArray;
		foreach (AchievementType achievementType in typeArray)
		{
			if (achievementType != 0 && achievementType != AchievementType.AllAchievements && AchievementLibrary.GetAchievementData(achievementType) != null && !SaveManager.ModeSaveData.GetAchievementUnlocked(achievementType))
			{
				flag = false;
				break;
			}
		}
		if (flag)
		{
			SaveManager.ModeSaveData.SetAchievementUnlocked(AchievementType.AllAchievements, unlocked: true);
			m_storeInstance.GiveAchievement(AchievementType.AllAchievements);
		}
	}
}
