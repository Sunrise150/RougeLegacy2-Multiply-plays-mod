using System;
using System.Collections.Generic;
using UnityEngine;

public class WanderingMusicianSpawnController : MonoBehaviour
{
	private const float WANDERING_MUSICIAN_SPAWN_ODDS = 0.5f;

	private bool m_isInLevelEditor;

	private Action<MonoBehaviour, EventArgs> m_onWorldCreationComplete;

	private Action<MonoBehaviour, EventArgs> m_onLevelEditorCreationComplete;

	public static BiomeType BiomeControllerType { get; private set; }

	public static int BiomeControllerIndex { get; private set; }

	public static bool IsInitialized { get; private set; }

	private void Awake()
	{
		m_onWorldCreationComplete = OnWorldCreationComplete;
		m_onLevelEditorCreationComplete = OnLevelEditorCreationComplete;
	}

	private void OnEnable()
	{
		BiomeControllerType = BiomeType.None;
		BiomeControllerIndex = -1;
		m_isInLevelEditor = GameUtility.IsInLevelEditor;
		if (!m_isInLevelEditor)
		{
			Messenger<GameMessenger, GameEvent>.AddListener(GameEvent.WorldCreationComplete, m_onWorldCreationComplete);
		}
		else
		{
			Messenger<GameMessenger, GameEvent>.AddListener(GameEvent.LevelEditorWorldCreationComplete, m_onLevelEditorCreationComplete);
		}
	}

	private void OnDisable()
	{
		if (!GameUtility.IsApplicationQuitting)
		{
			if (!m_isInLevelEditor)
			{
				Messenger<GameMessenger, GameEvent>.RemoveListener(GameEvent.WorldCreationComplete, m_onWorldCreationComplete);
			}
			else
			{
				Messenger<GameMessenger, GameEvent>.RemoveListener(GameEvent.LevelEditorWorldCreationComplete, m_onLevelEditorCreationComplete);
			}
		}
	}

	private void OnWorldCreationComplete(MonoBehaviour sender, EventArgs eventArgs)
	{
		if (RNGManager.GetRandomNumber(RngID.SpecialRoomProps, "WandererSpawnChance - WanderingMusicianSpawnController.OnWorldCreationComplete()", 0f, 1f) <= 0.5f)
		{
			List<BaseRoom> list = new List<BaseRoom>();
			foreach (KeyValuePair<BiomeType, BiomeController> biomeController in WorldBuilder.BiomeControllers)
			{
				list.Add(biomeController.Value.TransitionRoom);
			}
			if (list.Count > 0)
			{
				int randomNumber = RNGManager.GetRandomNumber(RngID.SpecialRoomProps, "WandererRoomSpawnIndex - WanderingMusicianSpawnController.OnWorldCreationComplete()", 0, list.Count);
				if (list[randomNumber] != null)
				{
					BiomeControllerType = list[randomNumber].BiomeType;
					BiomeControllerIndex = list[randomNumber].BiomeControllerIndex;
				}
			}
		}
		IsInitialized = true;
	}

	private void OnLevelEditorCreationComplete(MonoBehaviour sender, EventArgs eventArgs)
	{
		if (RNGManager.GetRandomNumber(RngID.SpecialRoomProps, "WandererSpawnChance - WanderingMusicianSpawnController.OnWorldCreationComplete()", 0f, 1f) <= 0.5f)
		{
			LevelEditorWorldCreationCompleteEventArgs obj = eventArgs as LevelEditorWorldCreationCompleteEventArgs;
			BiomeControllerType = obj.BuiltRoom.BiomeType;
			BiomeControllerIndex = obj.BuiltRoom.BiomeControllerIndex;
		}
		IsInitialized = true;
	}
}
