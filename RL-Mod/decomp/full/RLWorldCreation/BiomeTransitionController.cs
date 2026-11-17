using System;
using System.Collections;
using SceneManagement_RL;
using Sigtrap.Relays;
using UnityEngine;

namespace RLWorldCreation;

public class BiomeTransitionController : MonoBehaviour
{
	public static Relay<BiomeType, BiomeType> TransitionStartRelay = new Relay<BiomeType, BiomeType>();

	private static BiomeTransitionController Instance { get; set; }

	private void Awake()
	{
		if (Instance == null)
		{
			Instance = this;
		}
	}

	private IEnumerator TransitionCoroutine(Door door)
	{
		if (!door)
		{
			Debug.Log("Biome Transition: door is null!");
		}
		if (!door.Room)
		{
			Debug.Log("Biome Transition: door.Room is null!");
		}
		Debug.Log("Biome Transition: From " + door.Room.BiomeType.ToString() + " to " + door.TransitionsToBiome.ToString() + " (seed: " + RNGSeedManager.GetCurrentSeed(SceneLoadingUtility.ActiveScene.name).ToString("X") + "-" + BurdenManager.GetBurdenLevel(BurdenType.RoomCount));
		door.Room.BroadcastPlayerExitRoomEvents(door);
		yield return BiomeTransitionCoroutine(door.Room.BiomeType, door.TransitionsToBiome);
		PlayerManager.GetPlayerController().ControllerCorgi.GravityActive(state: true);
		PlayerManager.GetPlayerController().StopActiveAbilities(stopPersistentAbilities: true);
		if (!door.ConnectedRoom)
		{
			Debug.Log("Biome Transition: door.ConnectedRoom is null!");
		}
		Door connectedDoor = door.ConnectedDoor;
		door.ConnectedRoom.PlacePlayerInRoom(connectedDoor);
		DestroyOldCastleTransitionRoom(door.TransitionsToBiome);
		DestroyBiomeRoomInstances(door.Room.BiomeType);
		GC.Collect();
	}

	public static void DestroyBiomeRoomInstances(BiomeType prevBiome)
	{
		WorldBuilder.GetBiomeController(prevBiome).Reset();
	}

	public static void DestroyOldCastleTransitionRoom(BiomeType nextBiome)
	{
		if (nextBiome == BiomeType.Castle && WorldBuilder.OldCastleTransitionRoom != null && !WorldBuilder.OldCastleTransitionRoom.Equals(null))
		{
			UnityEngine.Object.Destroy(WorldBuilder.OldCastleTransitionRoom.gameObject);
		}
	}

	public static IEnumerator BiomeTransitionCoroutine(BiomeType originBiome, BiomeType destinationBiome)
	{
		originBiome = BiomeType_RL.GetGroupedBiomeType(originBiome);
		destinationBiome = BiomeType_RL.GetGroupedBiomeType(destinationBiome);
		if (!WorldBuilder.GetBiomeController(destinationBiome).IsInstantiated)
		{
			yield return WorldBuilder.InstantiateBiome(destinationBiome);
		}
		Messenger<GameMessenger, GameEvent>.Broadcast(GameEvent.UpdatePools, null, null);
		UpdateObjectPools(originBiome, destinationBiome);
		if ((GameUtility.IsInGame && WorldBuilder.DeactivateRoomGameObjects) || GameUtility.IsInLevelEditor)
		{
			BaseRoom currentPlayerRoom = PlayerManager.GetCurrentPlayerRoom();
			if (currentPlayerRoom != null)
			{
				currentPlayerRoom.gameObject.SetActive(value: false);
			}
		}
	}

	public static void UpdateObjectPools(BiomeType previousBiome, BiomeType newBiome)
	{
		SharedWorldObjectPoolManager.CreateBiomePools(newBiome);
	}

	public static void Run(Door door)
	{
		if (Instance != null)
		{
			PlayerManager.GetPlayerController().CharacterDash.StopDash();
			PlayerManager.GetPlayerController().ControllerCorgi.GravityActive(state: false);
			PlayerManager.GetPlayerController().ControllerCorgi.SetForce(Vector2.zero);
			TransitionStartRelay.Dispatch(door.Room.BiomeType, door.TransitionsToBiome);
			SceneLoader_RL.RunTransitionWithLogic(Instance.TransitionCoroutine(door), TransitionID.FadeToBlackWithLoading, cleanup: true);
		}
		else
		{
			Debug.LogFormat("<color=red>| BiomeTransitionController | No instance of <b>BiomeTransitionController</b> exists in Scene</color>");
		}
	}
}
