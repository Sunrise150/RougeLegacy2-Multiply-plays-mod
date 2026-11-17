using FMODUnity;
using RLAudio;
using UnityEngine;

public class CaveMinibossTunnel : Tunnel
{
	[SerializeField]
	private StudioEventEmitter m_lockedEventEmitter;

	public bool IsWhiteTunnel { get; set; }

	public override void SetIsLocked(bool isLocked)
	{
		if ((IsWhiteTunnel && SaveManager.PlayerSaveData.GetFlag(PlayerSaveFlag.CaveMiniboss_White_Defeated)) || (!IsWhiteTunnel && SaveManager.PlayerSaveData.GetFlag(PlayerSaveFlag.CaveMiniboss_Black_Defeated)))
		{
			base.SetIsLocked(isLocked);
			return;
		}
		base.IsLocked = isLocked;
		if ((bool)m_interactable)
		{
			m_interactable.SetIsInteractableActive(isActive: true);
		}
	}

	protected override void OnPlayerInteractedWithTunnel(GameObject otherObj)
	{
		Vector2 position = base.transform.position;
		position.y += 2f;
		if (base.IsLocked)
		{
			base.Animator.SetTrigger("GateShake");
			if (IsWhiteTunnel)
			{
				TextPopupManager.DisplayLocIDText(TextPopupType.OutOfAmmo, "LOC_ID_STATUS_EFFECT_WHITE_KEY_REQUIRED_1", StringGenderType.UsePlayerData, position);
			}
			else
			{
				TextPopupManager.DisplayLocIDText(TextPopupType.OutOfAmmo, "LOC_ID_STATUS_EFFECT_BLACK_KEY_REQUIRED_1", StringGenderType.UsePlayerData, position);
			}
			AudioManager.Play(null, m_lockedEventEmitter);
		}
		else if (IsWhiteTunnel && !SaveManager.PlayerSaveData.GetFlag(PlayerSaveFlag.CaveMiniboss_WhiteDoor_Opened))
		{
			SaveManager.PlayerSaveData.GetRelic(RelicType.DragonKeyWhite).SetLevel(0, additive: false);
			SaveManager.PlayerSaveData.SetFlag(PlayerSaveFlag.CaveMiniboss_WhiteDoor_Opened, value: true);
			base.Animator.SetTrigger("GateOpen");
			TextPopupManager.DisplayLocIDText(TextPopupType.OutOfAmmo, "LOC_ID_STATUS_EFFECT_DRAGON_LOCK_UNLOCKED_1", StringGenderType.UsePlayerData, position);
			AudioManager.PlayOneShot(null, "event:/Cut_Scenes/sfx_Cutscene_doorUnlock_doorOpen", base.gameObject.transform.position);
		}
		else if (!IsWhiteTunnel && !SaveManager.PlayerSaveData.GetFlag(PlayerSaveFlag.CaveMiniboss_BlackDoor_Opened))
		{
			SaveManager.PlayerSaveData.GetRelic(RelicType.DragonKeyBlack).SetLevel(0, additive: false);
			SaveManager.PlayerSaveData.SetFlag(PlayerSaveFlag.CaveMiniboss_BlackDoor_Opened, value: true);
			base.Animator.SetTrigger("GateOpen");
			TextPopupManager.DisplayLocIDText(TextPopupType.OutOfAmmo, "LOC_ID_STATUS_EFFECT_DRAGON_LOCK_UNLOCKED_1", StringGenderType.UsePlayerData, position);
			AudioManager.PlayOneShot(null, "event:/Cut_Scenes/sfx_Cutscene_doorUnlock_doorOpen", base.gameObject.transform.position);
		}
		else
		{
			base.OnPlayerInteractedWithTunnel(otherObj);
		}
	}
}
