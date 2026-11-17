using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HubTownController : MonoBehaviour
{
	[Header("Shops")]
	[SerializeField]
	private SkillTreeShop m_skillTree;

	private PolygonCollider2D m_collider;

	private Coroutine m_refreshTownCoroutine;

	private bool m_firstTimeEntering = true;

	private BaseRoom m_room;

	private HubTownEnteredEventArgs m_hubtownEnteredArgs;

	private ObjectiveCompleteHUDEventArgs m_traitsDisplayHUDArgs;

	private bool m_traitsDisplayed;

	private Action<MonoBehaviour, EventArgs> m_skillTreeWindowClosed;

	private Action<MonoBehaviour, EventArgs> m_onExitHubTown;

	private Action<MonoBehaviour, EventArgs> m_onBounce;

	private Action<object, RoomViaDoorEventArgs> m_onPlayerEnterRoom;

	private float m_regenTick;

	private const int SPINKICK_ACHIEVEMENT_CONDITION_COUNT = 11;

	private bool m_hasStartedSpinkickAchievement;

	private HashSet<SpinkickAchievementObj> m_spinKickAchievementObjs = new HashSet<SpinkickAchievementObj>();

	private void Awake()
	{
		m_room = GetComponent<Room>();
		m_hubtownEnteredArgs = new HubTownEnteredEventArgs(this);
		m_traitsDisplayHUDArgs = new ObjectiveCompleteHUDEventArgs(ObjectiveCompleteHUDType.Traits);
		m_skillTreeWindowClosed = SkillTreeWindowClosed;
		m_onExitHubTown = OnExitHubTown;
		m_onPlayerEnterRoom = OnPlayerEnterRoom;
		m_onBounce = OnBounce;
	}

	private void OnEnable()
	{
		Messenger<UIMessenger, UIEvent>.AddListener(UIEvent.SkillTree_Closed, m_skillTreeWindowClosed);
		Messenger<GameMessenger, GameEvent>.AddListener(GameEvent.PlayerExitHubTown, m_onExitHubTown);
		Messenger<GameMessenger, GameEvent>.AddListener(GameEvent.PlayerDownstrikeBounce, m_onBounce);
		m_room.PlayerEnterRelay.AddListener(m_onPlayerEnterRoom);
	}

	private void OnDisable()
	{
		Messenger<UIMessenger, UIEvent>.RemoveListener(UIEvent.SkillTree_Closed, m_skillTreeWindowClosed);
		Messenger<GameMessenger, GameEvent>.RemoveListener(GameEvent.PlayerExitHubTown, m_onExitHubTown);
		Messenger<GameMessenger, GameEvent>.RemoveListener(GameEvent.PlayerDownstrikeBounce, m_onBounce);
		m_room.PlayerEnterRelay.RemoveListener(m_onPlayerEnterRoom);
	}

	private void OnPlayerEnterRoom(object sender, RoomViaDoorEventArgs e)
	{
		SaveManager.PlayerSaveData.InCastle = false;
		SaveManager.PlayerSaveData.InHubTown = true;
		if (m_refreshTownCoroutine == null)
		{
			m_refreshTownCoroutine = StartCoroutine(RefreshTown());
			return;
		}
		Debug.LogFormat("<color=red>| {0} | Refresh Town Coroutine is already running, but should not be.</color>", this);
	}

	private void SkillTreeWindowClosed(object sender, EventArgs eventArgs)
	{
		if (m_refreshTownCoroutine == null)
		{
			m_refreshTownCoroutine = StartCoroutine(RefreshTown());
			return;
		}
		Debug.LogFormat("<color=red>| {0} | Refresh Town Coroutine is already running, but should not be.</color>", this);
	}

	private IEnumerator RefreshTown()
	{
		if (PlayerManager.IsInstantiated)
		{
			PlayerManager.GetPlayerController().ResetCharacter();
		}
		if (m_firstTimeEntering)
		{
			while (!PlayerManager.IsInstantiated)
			{
				yield return null;
			}
			BaseRoom room = PlayerManager.GetCurrentPlayerRoom();
			while (!room.CinemachineCamera.IsActiveVirtualCamera)
			{
				yield return null;
			}
			if (!SaveManager.PlayerSaveData.FirstTimeGoldReceived && SaveManager.PlayerSaveData.GoldCollected < 200 && SaveManager.PlayerSaveData.TimesDied <= 1)
			{
				SaveManager.PlayerSaveData.FirstTimeGoldReceived = true;
				SaveManager.PlayerSaveData.GoldCollected = 200;
			}
			if (!SaveManager.PlayerSaveData.GoldilocksReceived)
			{
				CalculateGoldilocksLimit();
				SaveManager.PlayerSaveData.GoldilocksReceived = true;
			}
			m_skillTree.OpenSkillTree(animate: false);
		}
		else
		{
			if (!m_traitsDisplayed)
			{
				m_traitsDisplayed = true;
				if (TraitLibrary.GetTraitData(SaveManager.PlayerSaveData.CurrentCharacter.TraitOne) != null || TraitLibrary.GetTraitData(SaveManager.PlayerSaveData.CurrentCharacter.TraitTwo) != null)
				{
					Messenger<UIMessenger, UIEvent>.Broadcast(UIEvent.DisplayObjectiveCompleteHUD, this, m_traitsDisplayHUDArgs);
				}
			}
			Messenger<GameMessenger, GameEvent>.Broadcast(GameEvent.PlayerEnterHubTown, this, m_hubtownEnteredArgs);
		}
		Messenger<GameMessenger, GameEvent>.Broadcast(GameEvent.GoldSavedChanged, this, null);
		Messenger<GameMessenger, GameEvent>.Broadcast(GameEvent.GoldChanged, this, null);
		Messenger<GameMessenger, GameEvent>.Broadcast(GameEvent.RuneOreChanged, this, null);
		Messenger<GameMessenger, GameEvent>.Broadcast(GameEvent.EquipmentOreChanged, this, null);
		Messenger<GameMessenger, GameEvent>.Broadcast(GameEvent.SoulChanged, this, null);
		m_firstTimeEntering = false;
		m_refreshTownCoroutine = null;
	}

	private void OnExitHubTown(MonoBehaviour sender, EventArgs eventArgs)
	{
		if (SaveManager.PlayerSaveData.CastleLockState != 0)
		{
			SaveManager.PlayerSaveData.TimesCastleLocked++;
		}
		else
		{
			SaveManager.PlayerSaveData.TimesCastleLocked = 0;
		}
		PlayerController playerController = PlayerManager.GetPlayerController();
		playerController.ResetHealth();
		playerController.ResetMana();
		playerController.ResetAllAbilityAmmo();
		playerController.ResetAbilityCooldowns();
	}

	private void CalculateGoldilocksLimit()
	{
		int num = int.MaxValue;
		SkillTreeType skillTreeType = SkillTreeType.None;
		SkillTreeType[] typeArray = SkillTreeType_RL.TypeArray;
		foreach (SkillTreeType skillTreeType2 in typeArray)
		{
			SkillTreeObj skillTreeObj = SkillTreeManager.GetSkillTreeObj(skillTreeType2);
			if (skillTreeObj != null && !skillTreeObj.IsLevelLocked && !skillTreeObj.IsLocked && !skillTreeObj.IsSoulLocked)
			{
				int goldCostWithLevelAppreciation = skillTreeObj.GoldCostWithLevelAppreciation;
				if (goldCostWithLevelAppreciation < num)
				{
					num = goldCostWithLevelAppreciation;
					skillTreeType = skillTreeType2;
				}
			}
		}
		int goldCollectedIncludingBank = SaveManager.PlayerSaveData.GoldCollectedIncludingBank;
		if (skillTreeType != 0 && goldCollectedIncludingBank < num)
		{
			int goldilocksLimit = Economy_EV.GetGoldilocksLimit();
			if (goldCollectedIncludingBank + goldilocksLimit >= num)
			{
				SaveManager.PlayerSaveData.GoldCollected += num - goldCollectedIncludingBank;
				Debug.Log("<color=yellow>Awarded goldilocks bonus of " + (num - goldCollectedIncludingBank) + " gold.</color>");
			}
		}
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
		if (m_hasStartedSpinkickAchievement && (playerController.IsGrounded || playerController.CharacterFlight.IsFlying || playerController.CharacterFlight.IsAssistFlying))
		{
			m_hasStartedSpinkickAchievement = false;
		}
	}

	private void OnBounce(object sender, EventArgs args)
	{
		if (!m_hasStartedSpinkickAchievement)
		{
			m_hasStartedSpinkickAchievement = true;
			m_spinKickAchievementObjs.Clear();
		}
		if (!(args is PlayerDownstrikeEventArgs playerDownstrikeEventArgs))
		{
			return;
		}
		SpinkickAchievementObj component = playerDownstrikeEventArgs.CollidedObj.GetRoot().GetComponent<SpinkickAchievementObj>();
		if ((bool)component)
		{
			m_spinKickAchievementObjs.Add(component);
			if (m_spinKickAchievementObjs.Count >= 11)
			{
				StoreAPIManager.GiveAchievement(AchievementType.HubtownSpinKick, StoreType.All);
			}
		}
	}
}
