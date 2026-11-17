using System;
using RLAudio;
using UnityEngine;

public class CaveMinibossRoomController : MinibossRoomController
{
	protected override void InitializeObjectiveCompleteArgs(float bossDefeatedDisplayDuration)
	{
		string @string = LocalizationManager.GetString("LOC_ID_BIG_TEXT_UI_STYGIAN_BOSS_DEFEATED_TITLE_1", isFemale: false);
		m_bossDefeatedArgs.Initialize(base.Boss.EnemyType, EnemyRank.Miniboss, bossDefeatedDisplayDuration, @string);
	}

	protected override void TeleportOut()
	{
		BiomeController obj = (GameUtility.IsInLevelEditor ? OnPlayManager.BiomeController : WorldBuilder.GetBiomeController(BiomeType.Cave));
		BaseRoom baseRoom = null;
		foreach (BaseRoom room in obj.Rooms)
		{
			if (room.RoomType == RoomType.BossEntrance)
			{
				baseRoom = room;
				break;
			}
		}
		if ((bool)baseRoom && base.Room.BiomeType != BiomeType.Stone)
		{
			if ((bool)base.TunnelSpawnController && (bool)base.TunnelSpawnController.Tunnel)
			{
				RewiredMapController.SetIsInCutscene(inCutscene: true);
				CutsceneManager.InitializeCutscene(m_bossSaveFlag, base.TunnelSpawnController.Tunnel.Destination);
				DragonCutsceneController component = baseRoom.gameObject.GetComponent<DragonCutsceneController>();
				base.TunnelSpawnController.Tunnel.ForceEnterTunnel(animate: true, component.TunnelSpawner.Tunnel);
			}
			else
			{
				Debug.Log("Could not exit boss room, tunnel spawn controller is null.");
			}
		}
		else if ((bool)base.TunnelSpawnController && (bool)base.TunnelSpawnController.Tunnel)
		{
			base.TunnelSpawnController.Tunnel.ForceEnterTunnel(animate: true);
		}
		else
		{
			Debug.Log("Could not exit boss room, tunnel spawn controller is null.");
		}
	}

	protected override void OnBossDeath(object sender, EventArgs args)
	{
		base.OnBossDeath(sender, args);
		if (base.NumBossesDeadOrDying >= base.NumBosses)
		{
			MusicManager.StopMusic();
		}
	}
}
