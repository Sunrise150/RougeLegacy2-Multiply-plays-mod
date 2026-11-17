using System;
using System.Collections;
using System.Collections.Generic;
using FMODUnity;
using Rewired;
using UnityEngine;
using UnityEngine.UI;

namespace RL_Windows;

public class TeleporterWindowController : MapWindowController
{
	private class TeleporterEntrySorter : IComparer<BiomeType>
	{
		public int Compare(BiomeType x, BiomeType y)
		{
			int num = Map_EV.BIOME_DISPLAY_ORDER.IndexOf(x);
			int num2 = Map_EV.BIOME_DISPLAY_ORDER.IndexOf(y);
			if (num > num2)
			{
				return 1;
			}
			if (num < num2)
			{
				return -1;
			}
			return 0;
		}
	}

	[SerializeField]
	private RectTransform m_teleporterEntries;

	[SerializeField]
	private TeleporterEntry m_teleporterEntryPrefab;

	[SerializeField]
	private RectTransform m_currentlyVisitedIcon;

	[SerializeField]
	private GameObject m_currentSelectedIcon;

	[SerializeField]
	private StudioEventEmitter m_changeSelectionSFX;

	[SerializeField]
	private StudioEventEmitter m_selectedSFX;

	private SortedDictionary<BiomeType, TeleporterEntry> m_teleporterEntriesDict;

	private List<TeleporterSubEntry> m_activeSubEntryList;

	private int m_selectedSubEntryIndex;

	private Action<MonoBehaviour, EventArgs> m_onWorldCreationComplete;

	private Action<MonoBehaviour, EventArgs> m_onPlayerEnterRoom;

	private Action<InputActionEventData> m_onCancelButtonDown;

	private Action<InputActionEventData> m_changeSelectedTeleporter;

	private Action<InputActionEventData> m_onConfirmButtonDown;

	public override WindowID ID => WindowID.Teleporter;

	private void Awake()
	{
		m_onPlayerEnterRoom = OnPlayerEnterRoom;
		m_onWorldCreationComplete = OnWorldCreationComplete;
		m_onCancelButtonDown = OnCancelButtonDown;
		m_changeSelectedTeleporter = ChangeSelectedTeleporter;
		m_onConfirmButtonDown = OnConfirmButtonDown;
	}

	private void OnEnable()
	{
		Messenger<GameMessenger, GameEvent>.AddListener(GameEvent.PlayerEnterRoom, m_onPlayerEnterRoom);
		Messenger<GameMessenger, GameEvent>.AddListener(GameEvent.WorldCreationComplete, m_onWorldCreationComplete);
	}

	private void OnDisable()
	{
		Messenger<GameMessenger, GameEvent>.RemoveListener(GameEvent.PlayerEnterRoom, m_onPlayerEnterRoom);
		Messenger<GameMessenger, GameEvent>.RemoveListener(GameEvent.WorldCreationComplete, m_onWorldCreationComplete);
	}

	private void OnWorldCreationComplete(object sender, EventArgs args)
	{
		StartCoroutine(InitializeTeleporters());
	}

	private void OnPlayerEnterRoom(object sender, EventArgs args)
	{
		if (base.IsInitialized)
		{
			BaseRoom room = (args as RoomViaDoorEventArgs).Room;
			SetEntryVisible(room.AppearanceBiomeType, 0);
		}
	}

	private void SetEntryVisible(BiomeType biomeType, int index)
	{
	}

	public override void Initialize()
	{
		base.Initialize();
		if (GameUtility.IsInLevelEditor)
		{
			StartCoroutine(InitializeTeleporters());
		}
	}

	private IEnumerator InitializeTeleporters()
	{
		while (!MapController.IsInitialized)
		{
			yield return null;
		}
		m_activeSubEntryList = new List<TeleporterSubEntry>();
		m_teleporterEntriesDict = new SortedDictionary<BiomeType, TeleporterEntry>(new TeleporterEntrySorter());
		foreach (GridPointManager gridPointManagersContainingTeleporter in MapController.GridPointManagersContainingTeleporters)
		{
			AddTeleporterEntry(gridPointManagersContainingTeleporter);
		}
		SortTeleporterEntries();
		SortTeleporterSubEntries();
	}

	public void AddTeleporterEntry(GridPointManager gridPointManager)
	{
		TeleporterEntry value = null;
		GlobalTeleporterType globalTeleporterType = GlobalTeleporterType.None;
		BiomeType groupedBiomeType = BiomeType_RL.GetGroupedBiomeType(gridPointManager.Biome);
		globalTeleporterType = gridPointManager.RoomType switch
		{
			RoomType.Transition => GlobalTeleporterType.TransitionRoom, 
			RoomType.BossEntrance => GlobalTeleporterType.BossEntrance, 
			_ => GlobalTeleporterType.SpecialRoom, 
		};
		if (m_teleporterEntriesDict.TryGetValue(groupedBiomeType, out value))
		{
			value.AddSubEntry(globalTeleporterType, gridPointManager);
			return;
		}
		TeleporterEntry teleporterEntry = UnityEngine.Object.Instantiate(m_teleporterEntryPrefab, m_teleporterEntries.transform);
		teleporterEntry.Initialize();
		teleporterEntry.SetBiomeType(groupedBiomeType);
		teleporterEntry.AddSubEntry(globalTeleporterType, gridPointManager);
		teleporterEntry.gameObject.SetActive(value: false);
		m_teleporterEntriesDict.Add(groupedBiomeType, teleporterEntry);
	}

	private void SortTeleporterEntries()
	{
		foreach (KeyValuePair<BiomeType, TeleporterEntry> item in m_teleporterEntriesDict)
		{
			TeleporterEntry value = item.Value;
			int num = Map_EV.BIOME_DISPLAY_ORDER.IndexOf(value.BiomeType);
			if (num == -1)
			{
				value.transform.SetAsLastSibling();
			}
			else
			{
				value.transform.SetSiblingIndex(num);
			}
		}
	}

	private void SortTeleporterSubEntries()
	{
		foreach (KeyValuePair<BiomeType, TeleporterEntry> item in m_teleporterEntriesDict)
		{
			TeleporterEntry value = item.Value;
			TeleporterSubEntry teleporterSubEntry = null;
			TeleporterSubEntry teleporterSubEntry2 = null;
			foreach (TeleporterSubEntry subEntries in value.SubEntriesList)
			{
				if (subEntries.SubEntryType == GlobalTeleporterType.BossEntrance)
				{
					teleporterSubEntry = subEntries;
				}
				else if (subEntries.SubEntryType == GlobalTeleporterType.TransitionRoom)
				{
					teleporterSubEntry2 = subEntries;
				}
				if ((bool)teleporterSubEntry && (bool)teleporterSubEntry2)
				{
					break;
				}
			}
			if ((bool)teleporterSubEntry)
			{
				value.SubEntriesList.Remove(teleporterSubEntry);
				value.SubEntriesList.Add(teleporterSubEntry);
				teleporterSubEntry.transform.SetAsLastSibling();
			}
			if ((bool)teleporterSubEntry2)
			{
				value.SubEntriesList.Remove(teleporterSubEntry2);
				value.SubEntriesList.Insert(0, teleporterSubEntry2);
				teleporterSubEntry2.transform.SetAsFirstSibling();
			}
		}
	}

	private int SubEntrySorter(TeleporterSubEntry entry1, TeleporterSubEntry entry2)
	{
		if (entry1 == entry2)
		{
			return 0;
		}
		Vector3 position = PlayerManager.GetPlayer().transform.position;
		Vector3 position2 = MapController.GetMapRoomEntry(entry1.BiomeType, entry1.BiomeControllerIndex).Terrain.transform.position;
		Vector3 position3 = MapController.GetMapRoomEntry(entry2.BiomeType, entry2.BiomeControllerIndex).Terrain.transform.position;
		float num = CDGHelper.DistanceBetweenPts(position, position2);
		float num2 = CDGHelper.DistanceBetweenPts(position, position3);
		if (num > num2)
		{
			return 1;
		}
		if (num < num2)
		{
			return -1;
		}
		return 0;
	}

	protected override void OnOpen()
	{
		foreach (KeyValuePair<BiomeType, TeleporterEntry> item in m_teleporterEntriesDict)
		{
			TeleporterEntry value = item.Value;
			foreach (TeleporterSubEntry subEntries in value.SubEntriesList)
			{
				subEntries.gameObject.SetActive(value: false);
			}
			value.gameObject.SetActive(value: false);
		}
		m_activeSubEntryList.Clear();
		foreach (KeyValuePair<BiomeType, TeleporterEntry> item2 in m_teleporterEntriesDict)
		{
			BiomeType key = item2.Key;
			TeleporterEntry value2 = item2.Value;
			bool flag = false;
			foreach (TeleporterSubEntry subEntries2 in value2.SubEntriesList)
			{
				if (MapController.WasRoomVisited(key, subEntries2.BiomeControllerIndex))
				{
					subEntries2.gameObject.SetActive(value: true);
					m_activeSubEntryList.Add(subEntries2);
					flag = true;
				}
			}
			if (flag && !value2.gameObject.activeSelf)
			{
				value2.gameObject.SetActive(value: true);
			}
		}
		if (TraitManager.IsTraitActive(TraitType.MapReveal))
		{
			MapController.SetPlayerIconVisible(visible: true);
			BiomeType biomeType = PlayerManager.GetCurrentPlayerRoom().BiomeType;
			foreach (TeleporterSubEntry activeSubEntry in m_activeSubEntryList)
			{
				if (activeSubEntry.BiomeType == biomeType)
				{
					continue;
				}
				MapRoomEntry mapRoomEntry = MapController.GetMapRoomEntry(activeSubEntry.BiomeType, activeSubEntry.BiomeControllerIndex);
				if ((bool)mapRoomEntry)
				{
					mapRoomEntry.gameObject.SetActive(value: true);
					RoomSaveData roomSaveData = SaveManager.StageSaveData.GetRoomSaveData(activeSubEntry.BiomeType, activeSubEntry.BiomeControllerIndex);
					if (roomSaveData.IsNativeNull() || !roomSaveData.RoomVisited)
					{
						mapRoomEntry.ToggleIconVisibility(MapIconType.Teleporter, -1, visible: true);
						mapRoomEntry.ToggleTerrainVisibility(visible: true);
						mapRoomEntry.ToggleIconVisibility(MapIconType.SpecialIndicator, -1, visible: false);
					}
				}
			}
		}
		else
		{
			_ = PlayerManager.GetCurrentPlayerRoom().BiomeType;
			foreach (TeleporterSubEntry activeSubEntry2 in m_activeSubEntryList)
			{
				MapRoomEntry mapRoomEntry2 = MapController.GetMapRoomEntry(activeSubEntry2.BiomeType, activeSubEntry2.BiomeControllerIndex);
				if ((bool)mapRoomEntry2 && mapRoomEntry2.WasVisited && mapRoomEntry2.RoomType != RoomType.BossEntrance)
				{
					if (mapRoomEntry2.HasTeleporterIcon)
					{
						mapRoomEntry2.ToggleIconVisibility(MapIconType.Teleporter, -1, visible: true);
						mapRoomEntry2.ToggleIconVisibility(MapIconType.SpecialRoom, -1, visible: false);
						mapRoomEntry2.ToggleIconVisibility(MapIconType.SpecialRoomUsed, -1, visible: false);
					}
					mapRoomEntry2.ToggleTerrainVisibility(visible: true);
				}
			}
		}
		if (m_activeSubEntryList.Count > 0)
		{
			BaseRoom currentPlayerRoom = PlayerManager.GetCurrentPlayerRoom();
			int activeTeleporterIndex = GetActiveTeleporterIndex(currentPlayerRoom);
			if (activeTeleporterIndex != -1)
			{
				StartCoroutine(SetTeleporterIconsCoroutine(activeTeleporterIndex));
			}
			else
			{
				Debug.LogWarning("<color=red>You somehow opened the teleporter window in a room without a teleporter in it.</color>");
			}
		}
		MapController.SetTeleporterLineVisible(visible: true);
		base.OnOpen();
	}

	private int GetActiveTeleporterIndex(BaseRoom room)
	{
		for (int i = 0; i < m_activeSubEntryList.Count; i++)
		{
			TeleporterSubEntry teleporterSubEntry = m_activeSubEntryList[i];
			if (teleporterSubEntry.BiomeControllerIndex == room.BiomeControllerIndex && teleporterSubEntry.BiomeType == room.BiomeType)
			{
				return i;
			}
		}
		return -1;
	}

	private IEnumerator SetTeleporterIconsCoroutine(int teleporterIndex)
	{
		yield return null;
		LayoutRebuilder.ForceRebuildLayoutImmediate(m_teleporterEntries);
		SetPreviouslySelectedTeleporter(teleporterIndex);
		SelectTeleporter(teleporterIndex);
	}

	protected override void OnClose()
	{
		MapController.SetTeleporterLineVisible(visible: false);
		if (TraitManager.IsTraitActive(TraitType.MapReveal))
		{
			MapController.SetPlayerIconVisible(visible: false);
			BiomeType biomeType = PlayerManager.GetCurrentPlayerRoom().BiomeType;
			foreach (TeleporterSubEntry activeSubEntry in m_activeSubEntryList)
			{
				if (activeSubEntry.BiomeType == biomeType)
				{
					continue;
				}
				MapRoomEntry mapRoomEntry = MapController.GetMapRoomEntry(activeSubEntry.BiomeType, activeSubEntry.BiomeControllerIndex);
				if ((bool)mapRoomEntry)
				{
					mapRoomEntry.gameObject.SetActive(value: false);
					RoomSaveData roomSaveData = SaveManager.StageSaveData.GetRoomSaveData(activeSubEntry.BiomeType, activeSubEntry.BiomeControllerIndex);
					if (roomSaveData.IsNativeNull() || !roomSaveData.RoomVisited)
					{
						mapRoomEntry.ToggleIconVisibility(MapIconType.Teleporter, -1, visible: false);
						mapRoomEntry.ToggleTerrainVisibility(visible: false);
						mapRoomEntry.ToggleIconVisibility(MapIconType.SpecialIndicator, -1, visible: false);
					}
				}
			}
		}
		else
		{
			foreach (TeleporterSubEntry activeSubEntry2 in m_activeSubEntryList)
			{
				MapRoomEntry mapRoomEntry2 = MapController.GetMapRoomEntry(activeSubEntry2.BiomeType, activeSubEntry2.BiomeControllerIndex);
				if ((bool)mapRoomEntry2 && mapRoomEntry2.WasVisited && mapRoomEntry2.RoomType != RoomType.BossEntrance)
				{
					RoomSaveData roomSaveData2 = SaveManager.StageSaveData.GetRoomSaveData(activeSubEntry2.BiomeType, activeSubEntry2.BiomeControllerIndex);
					if (!roomSaveData2.IsNativeNull() && mapRoomEntry2.HasTeleporterIcon)
					{
						mapRoomEntry2.ToggleIconVisibility(MapIconType.Teleporter, -1, !mapRoomEntry2.HasSpecialRoomIcon);
						mapRoomEntry2.ToggleIconVisibility(MapIconType.SpecialRoom, -1, !roomSaveData2.IsRoomComplete);
						mapRoomEntry2.ToggleIconVisibility(MapIconType.SpecialRoomUsed, -1, roomSaveData2.IsRoomComplete);
					}
					mapRoomEntry2.ToggleTerrainVisibility(visible: true);
				}
			}
		}
		base.OnClose();
	}

	private void SetPreviouslySelectedTeleporter(int index)
	{
		float y = m_activeSubEntryList[index].transform.position.y;
		Vector3 position = m_currentlyVisitedIcon.transform.position;
		position.y = y;
		m_currentlyVisitedIcon.transform.position = position;
	}

	private void SelectTeleporter(int index)
	{
		m_selectedSubEntryIndex = index;
		foreach (TeleporterSubEntry activeSubEntry in m_activeSubEntryList)
		{
			activeSubEntry.SetSelected(selected: false);
		}
		TeleporterSubEntry teleporterSubEntry = m_activeSubEntryList[m_selectedSubEntryIndex];
		float y = teleporterSubEntry.transform.position.y;
		Vector3 position = m_currentSelectedIcon.transform.position;
		position.y = y;
		m_currentSelectedIcon.transform.position = position;
		m_activeSubEntryList[index].SetSelected(selected: true);
		MapController.TweenCameraToPosition(MapController.GetMapPositionFromWorld(teleporterSubEntry.GridPointManager.Bounds.center), 0.15f);
	}

	protected override void SubscribeToRewiredInputEvents()
	{
		if (ReInput.isReady)
		{
			base.RewiredPlayer.AddInputEventDelegate(m_onCancelButtonDown, UpdateLoopType.Update, InputActionEventType.ButtonJustPressed, "Window_Select");
			base.RewiredPlayer.AddInputEventDelegate(m_onCancelButtonDown, UpdateLoopType.Update, InputActionEventType.ButtonJustPressed, "Window_Cancel");
			base.RewiredPlayer.AddInputEventDelegate(m_changeSelectedTeleporter, UpdateLoopType.Update, InputActionEventType.ButtonJustPressed, "Window_Vertical");
			base.RewiredPlayer.AddInputEventDelegate(m_changeSelectedTeleporter, UpdateLoopType.Update, InputActionEventType.NegativeButtonJustPressed, "Window_Vertical");
			base.RewiredPlayer.AddInputEventDelegate(m_onConfirmButtonDown, UpdateLoopType.Update, InputActionEventType.ButtonJustPressed, "Window_Confirm");
		}
	}

	protected override void UnsubscribeFromRewiredInputEvents()
	{
		if (ReInput.isReady)
		{
			base.RewiredPlayer.RemoveInputEventDelegate(m_onCancelButtonDown, UpdateLoopType.Update, InputActionEventType.ButtonJustPressed, "Window_Select");
			base.RewiredPlayer.RemoveInputEventDelegate(m_onCancelButtonDown, UpdateLoopType.Update, InputActionEventType.ButtonJustPressed, "Window_Cancel");
			base.RewiredPlayer.RemoveInputEventDelegate(m_changeSelectedTeleporter, UpdateLoopType.Update, InputActionEventType.ButtonJustPressed, "Window_Vertical");
			base.RewiredPlayer.RemoveInputEventDelegate(m_changeSelectedTeleporter, UpdateLoopType.Update, InputActionEventType.NegativeButtonJustPressed, "Window_Vertical");
			base.RewiredPlayer.RemoveInputEventDelegate(m_onConfirmButtonDown, UpdateLoopType.Update, InputActionEventType.ButtonJustPressed, "Window_Confirm");
		}
	}

	protected void ChangeSelectedTeleporter(InputActionEventData inputActionEventData)
	{
		if (m_activeSubEntryList.Count > 0)
		{
			int selectedSubEntryIndex = m_selectedSubEntryIndex;
			int selectedSubEntryIndex2 = m_selectedSubEntryIndex;
			float num = inputActionEventData.GetAxis();
			if (num == 0f)
			{
				num = 0f - inputActionEventData.GetAxis();
			}
			selectedSubEntryIndex2 = ((!(num < 0f)) ? (selectedSubEntryIndex2 - 1) : (selectedSubEntryIndex2 + 1));
			if (selectedSubEntryIndex2 < 0)
			{
				selectedSubEntryIndex2 = m_activeSubEntryList.Count - 1;
			}
			else if (selectedSubEntryIndex2 > m_activeSubEntryList.Count - 1)
			{
				selectedSubEntryIndex2 = 0;
			}
			if (selectedSubEntryIndex2 != selectedSubEntryIndex)
			{
				m_changeSelectionSFX.Play();
				SelectTeleporter(selectedSubEntryIndex2);
			}
		}
	}

	protected override void OnConfirmButtonDown(InputActionEventData obj)
	{
		if (m_activeSubEntryList != null && m_selectedSubEntryIndex >= 0 && m_selectedSubEntryIndex < m_activeSubEntryList.Count)
		{
			WindowManager.CloseAllOpenWindows();
			m_selectedSFX.Play();
			TeleporterSubEntry teleporterSubEntry = m_activeSubEntryList[m_selectedSubEntryIndex];
			GlobalTeleporterController.ActiveTeleporter.OnEnterTeleportPlayer_V2(teleporterSubEntry.BiomeType, teleporterSubEntry.GridPointManager);
		}
	}
}
