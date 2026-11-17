using System.Collections;
using FMODUnity;
using RLAudio;
using UnityEngine;

public class GardenRoomController : BaseSpecialRoomController
{
	protected override void OnPlayerEnterRoom(object sender, RoomViaDoorEventArgs eventArgs)
	{
		base.OnPlayerEnterRoom(sender, eventArgs);
		SaveManager.PlayerSaveData.EndingSpawnRoom = EndingSpawnRoomType.None;
		StartCoroutine(FlipPlayer());
		if (MusicManager.CurrentSong != SongID.TraitorBoss_Tettix)
		{
			StartCoroutine(PlaySong());
		}
	}

	private IEnumerator FlipPlayer()
	{
		yield return null;
		PlayerController playerController = PlayerManager.GetPlayerController();
		if (playerController.IsFacingRight)
		{
			playerController.CharacterCorgi.Flip(IgnoreFlipOnDirectionChange: true, ignoreTurnParam: true);
		}
	}

	private IEnumerator PlaySong()
	{
		float delay = Time.time + 2f;
		while (Time.time < delay)
		{
			yield return null;
		}
		MusicManager.PlayMusic(SongID.TraitorBoss_Tettix, isOverride: false);
		if (MusicManager.CurrentMusicInstance.isValid())
		{
			RuntimeManager.StudioSystem.setParameterByName("bossEncounterProgress_cain", 0f);
		}
		GetComponent<RoomMusicOverrideController>().SetOverride(SongID.TraitorBoss_Tettix);
	}
}
