using System;
using RL_Windows;
using UnityEngine.Events;

public class JukeboxShop : BaseShop, IRoomConsumer, IDisplaySpeechBubble
{
	private UnityAction m_closeShop;

	private static bool m_hasSubmittedAchievementsThisGameSession;

	public BaseRoom Room { get; private set; }

	public override bool ShouldDisplaySpeechBubble
	{
		get
		{
			SongID[] typeArray = SongType_RL.TypeArray;
			foreach (SongID songID in typeArray)
			{
				if (songID != 0 && Jukebox_EV.JukeboxDataDict.ContainsKey(songID) && SaveManager.PlayerSaveData.GetSongFoundState(songID) == FoundState.FoundButNotViewed)
				{
					return true;
				}
			}
			return false;
		}
	}

	public override SpeechBubbleType BubbleType => SpeechBubbleType.GearAvailable;

	public string Description => ToString();

	protected override void Awake()
	{
		base.Awake();
		m_closeShop = CloseShop;
	}

	public void SetRoom(BaseRoom room)
	{
		Room = room;
		Room.PlayerEnterRelay.AddListener(OnPlayerEnterRoom);
	}

	protected override bool HasEventDialogue()
	{
		return false;
	}

	private void OnDestroy()
	{
		if ((bool)Room)
		{
			Room.PlayerEnterRelay.RemoveListener(OnPlayerEnterRoom);
		}
	}

	private void OnPlayerEnterRoom(object sender, EventArgs args)
	{
		SoulShopObj soulShopObj = SaveManager.ModeSaveData.GetSoulShopObj(SoulShopType.UnlockJukebox);
		if (soulShopObj.IsNativeNull())
		{
			return;
		}
		if (soulShopObj.CurrentOwnedLevel <= 0)
		{
			base.gameObject.SetActive(value: false);
			return;
		}
		PropSpawnController propSpawnController = Room.gameObject.FindObjectReference<PropSpawnController>("PropBehindJukebox");
		if ((bool)propSpawnController && (bool)propSpawnController.PropInstance)
		{
			propSpawnController.PropInstance.gameObject.SetActive(value: false);
		}
	}

	protected override void OpenShop_Internal()
	{
		if (!WindowManager.GetIsWindowLoaded(WindowID.Jukebox))
		{
			WindowManager.LoadWindow(WindowID.Jukebox);
		}
		WindowManager.GetWindowController(WindowID.Jukebox).WindowClosedEvent.AddListener(m_closeShop);
		if (!m_hasSubmittedAchievementsThisGameSession)
		{
			Achievement_EV.RunAchievementSafetyChecks();
			StoreAPIManager.GiveAllUnlockedAchievements();
			m_hasSubmittedAchievementsThisGameSession = true;
		}
		WindowManager.SetWindowIsOpen(WindowID.Jukebox, isOpen: true);
	}

	public override void CloseShop()
	{
		base.CloseShop();
		WindowManager.GetWindowController(WindowID.Jukebox).WindowClosedEvent.RemoveListener(m_closeShop);
	}
}
