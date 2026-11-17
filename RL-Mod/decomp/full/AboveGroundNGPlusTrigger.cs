using RLAudio;
using UnityEngine;

public class AboveGroundNGPlusTrigger : MonoBehaviour
{
	public void TriggerNGPlus()
	{
		PropSpawnController propSpawnController = PlayerManager.GetCurrentPlayerRoom().gameObject.FindObjectReference<PropSpawnController>("Elpis");
		if ((bool)propSpawnController && (bool)propSpawnController.PropInstance && !propSpawnController.PropInstance.gameObject.activeSelf && !SaveManager.PlayerSaveData.GetFlag(PlayerSaveFlag.Timeline_Unlocked) && SaveManager.PlayerSaveData.GetFlag(PlayerSaveFlag.SeenParade))
		{
			SaveManager.PlayerSaveData.SetFlag(PlayerSaveFlag.Timeline_Unlocked, value: true);
			Vector3 position = propSpawnController.PropInstance.transform.position;
			position.y += 2f;
			EffectManager.PlayEffect(propSpawnController.PropInstance.gameObject, propSpawnController.PropInstance.Animators[0], "EnemyTeleportOut_Effect", position, 0f, EffectStopType.Gracefully);
			propSpawnController.PropInstance.gameObject.SetActive(value: true);
			AudioManager.PlayOneShotAttached(null, "event:/Cut_Scenes/sfx_cutscene_ngPlusTeleport_playerAppear", base.gameObject);
			AudioManager.PlayOneShotAttached(null, "event:/SFX/Interactables/sfx_redHood_greeting", base.gameObject);
		}
	}
}
