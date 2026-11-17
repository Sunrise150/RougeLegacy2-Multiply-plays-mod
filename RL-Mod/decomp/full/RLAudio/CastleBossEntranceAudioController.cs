using FMODUnity;
using UnityEngine;

namespace RLAudio;

public class CastleBossEntranceAudioController : BossEntranceAudioController
{
	[SerializeField]
	[EventRef]
	private string m_doorUnlockedAudioPath;

	protected override void Awake()
	{
		base.Awake();
		(m_bossEntranceRoomController as CastleBossEntranceRoomController).DoorUnlockedRelay.AddListener(OnDoorUnlocked);
	}

	private void OnDoorUnlocked()
	{
		AudioManager.PlayOneShot(this, m_doorUnlockedAudioPath);
	}
}
