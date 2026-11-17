using System;
using System.Collections;
using System.Collections.Generic;
using SceneManagement_RL;
using UnityEngine;

public class ChallengeTunnelController : MonoBehaviour, IRoomConsumer
{
	[SerializeField]
	private GameObject m_exitSpawnPoint;

	[SerializeField]
	private ChallengeTypeTunnelSpawnControllerDictionary m_tunnelSpawnControllers;

	[SerializeField]
	private PropSpawnController m_challengeNPCSpawner;

	private ChallengeTrophyRank m_prevTrophyRank;

	private Action<MonoBehaviour, EventArgs> m_onEnterChallenge;

	public BaseRoom Room { get; private set; }

	private void Awake()
	{
		m_onEnterChallenge = OnEnterChallenge;
	}

	private void OnEnable()
	{
		Messenger<UIMessenger, UIEvent>.AddListener(UIEvent.ChallengeNPC_EnterChallenge, m_onEnterChallenge);
	}

	private void OnDisable()
	{
		Messenger<UIMessenger, UIEvent>.RemoveListener(UIEvent.ChallengeNPC_EnterChallenge, m_onEnterChallenge);
	}

	public void SetRoom(BaseRoom room)
	{
		Room = room;
	}

	private void OnEnterChallenge(MonoBehaviour sender, EventArgs args)
	{
		ChallengeType challengeType = (args as ChallengeOmniUIDescriptionEventArgs).ChallengeType;
		EnterChallenge(challengeType, animateTransition: true);
	}

	private void EnterTunnel(TunnelSpawnController tunnelSpawnController, ChallengeType challengeType)
	{
		SaveManager.DisableSaving = true;
		List<Room> list;
		if (!GameUtility.IsInLevelEditor)
		{
			list = WorldBuilder.BiomeControllers[Room.BiomeType].RoomsConnectedByTunnel;
		}
		else
		{
			list = new List<Room>();
			foreach (BaseRoom room in OnPlayManager.RoomList)
			{
				list.Add(room as Room);
			}
		}
		foreach (Room item in list)
		{
			if (!(item == Room))
			{
				item.GridPointManager.SetRoomSeed(UnityEngine.Random.Range(0, int.MaxValue));
				RoomSaveData roomSaveData = SaveManager.StageSaveData.GetRoomSaveData(item.BiomeType, item.BiomeControllerIndex);
				if (!roomSaveData.IsNativeNull())
				{
					roomSaveData.Clear();
				}
			}
		}
		m_prevTrophyRank = ChallengeManager.GetChallengeTrophyRank(challengeType);
		ChallengeManager.ChallengeTunnelController = this;
		ChallengeManager.SetActiveChallenge(challengeType);
		ChallengeManager.SetupCharacter();
		PlayerManager.GetPlayerController().SetFacing(faceRight: true);
		GC.Collect();
		tunnelSpawnController.Tunnel.ForceEnterTunnel(animate: false);
	}

	public void EnterChallenge(ChallengeType challengeType, bool animateTransition)
	{
		if (challengeType == ChallengeType.Tutorial && ChallengeManager.GetTotalTrophiesEarned(includeTutorialPurified: true) >= ChallengeManager.GetTotalTrophyCount() - 1)
		{
			challengeType = ChallengeType.TutorialPurified;
		}
		if (m_tunnelSpawnControllers.TryGetValue(challengeType, out var tunnelSpawnController))
		{
			if (!tunnelSpawnController.IsNativeNull() && !tunnelSpawnController.Tunnel.IsNativeNull())
			{
				if (animateTransition)
				{
					SceneLoader_RL.RunTransitionWithLogic(delegate
					{
						EnterTunnel(tunnelSpawnController, challengeType);
					}, TransitionID.ScreenDistortion, cleanup: false);
				}
				else
				{
					EnterTunnel(tunnelSpawnController, challengeType);
				}
				return;
			}
			throw new Exception("There is no corresponding TunnelSpawnController for ChallengeType: " + challengeType.ToString() + " in ChallengeTunnelController.");
		}
		throw new Exception("There is no entry for ChallengeType: " + challengeType.ToString() + " in ChallengeTunnelController.");
	}

	public void ReturnToDriftHouse(bool broadcastExitEvent = true)
	{
		ChallengeType challengeType = ChallengeManager.ActiveChallenge.ChallengeType;
		if (!Room.IsNativeNull())
		{
			PlayerManager.GetCurrentPlayerRoom().PlayerExit(null);
			LocalTeleporterController.StopTeleportPlayer();
			Room.PlacePlayerInRoom(m_exitSpawnPoint.transform.localPosition);
		}
		else
		{
			LocalTeleporterController.StopTeleportPlayer();
			Debug.Log("<color=red>Cannot exit challenge. No entrance room found.</color>");
		}
		ChallengeManager.SetActiveChallenge(ChallengeType.None);
		ChallengeManager.RestoreCharacter(updateVisuals: true);
		Messenger<UIMessenger, UIEvent>.Broadcast(UIEvent.StopGlobalTimer, null, null);
		Messenger<UIMessenger, UIEvent>.Broadcast(UIEvent.HideGlobalTimer, null, null);
		if (broadcastExitEvent)
		{
			Messenger<UIMessenger, UIEvent>.Broadcast(UIEvent.ChallengeNPC_ExitChallenge, null, null);
		}
		SaveManager.DisableSaving = false;
		if (!ChallengeManager.NeedsSave)
		{
			return;
		}
		SaveManager.SaveCurrentProfileGameData(SaveDataType.GameMode, SavingType.FileOnly, ignoreTimeRestriction: true);
		ChallengeManager.NeedsSave = false;
		ChallengeTrophyRank challengeTrophyRank = ChallengeManager.GetChallengeTrophyRank(challengeType);
		if (m_prevTrophyRank < challengeTrophyRank)
		{
			int num = Souls_EV.GetChallengeSoulsRewarded(challengeType, challengeTrophyRank) - Souls_EV.GetChallengeSoulsRewarded(challengeType, m_prevTrophyRank);
			if (num > 0)
			{
				StartCoroutine(SpawnSouls(num));
			}
		}
	}

	private IEnumerator SpawnSouls(int soulsGained)
	{
		SoulDrop.FakeSoulCounter_STATIC = soulsGained;
		Messenger<GameMessenger, GameEvent>.Broadcast(GameEvent.SoulChanged, null, null);
		float delay = Time.time + 0.25f;
		while (Time.time < delay)
		{
			yield return null;
		}
		Vector3 position = m_challengeNPCSpawner.PropInstance.Pivot.transform.position;
		position.y += 1f;
		ItemDropManager.DropItem(ItemDropType.Soul, soulsGained, position, largeSpurt: true, forceMagnetize: true);
	}
}
