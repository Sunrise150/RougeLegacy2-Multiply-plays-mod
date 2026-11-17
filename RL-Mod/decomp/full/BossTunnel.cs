using System.Collections;
using RLAudio;
using RLWorldCreation;
using RL_Windows;
using SceneManagement_RL;
using UnityEngine;

public class BossTunnel : Tunnel
{
	public BossTunnelState TunnelState { get; set; }

	protected override IEnumerator EnterTunnelCoroutine()
	{
		if (!base.IsCutsceneTeleport)
		{
			RewiredMapController.SetCurrentMapEnabled(enabled: false);
			MusicManager.StopMusic();
			base.Animator.SetBool("Open", value: true);
			RegenHealthAndMana();
			RegenAbilityCooldowns();
			if ((bool)base.Room.GetComponent<GoldenDoorsEntranceRoomController>())
			{
				PlayerManager.GetPlayerController().CharacterDash.StopDash();
				PlayerManager.GetPlayerController().ControllerCorgi.GravityActive(state: false);
				PlayerManager.GetPlayerController().ControllerCorgi.SetForce(Vector2.zero);
				BiomeTransitionController.TransitionStartRelay.Dispatch(BiomeType.Castle, BiomeType.Garden);
				SceneLoader_RL.RunTransitionWithLogic(TeleportPlayerToGarden(), TransitionID.FadeToBlackWithLoading, cleanup: true);
			}
			else
			{
				float yieldTime = Time.time;
				while (Time.time < yieldTime + 0.5f)
				{
					yield return null;
				}
				SceneLoader_RL.RunTransitionWithLogic(EnterTunnelAnimCoroutine(), base.TransitionType, cleanup: false);
			}
		}
		else
		{
			yield return base.EnterTunnelCoroutine();
		}
	}

	private IEnumerator TeleportPlayerToGarden()
	{
		yield return BiomeTransitionController.BiomeTransitionCoroutine(BiomeType.Castle, BiomeType.Garden);
		PlayerManager.GetPlayerController().ControllerCorgi.GravityActive(state: true);
		PlayerManager.GetPlayerController().StopActiveAbilities(stopPersistentAbilities: true);
		WorldBuilder.BiomeControllers[BiomeType.Garden].TransitionRoom.PlacePlayerInRoom(null);
		BiomeTransitionController.DestroyOldCastleTransitionRoom(BiomeType.Garden);
		BiomeTransitionController.DestroyBiomeRoomInstances(BiomeType.Castle);
	}

	private IEnumerator EnterTunnelAnimCoroutine()
	{
		float yieldTime = Time.time;
		while (Time.time < yieldTime + 2.5f)
		{
			yield return null;
		}
		EnterTunnel();
	}

	private void RegenHealthAndMana()
	{
		PlayerSaveFlag bossFreeHealUsedFlag = GetBossFreeHealUsedFlag();
		if (bossFreeHealUsedFlag != 0 && !SaveManager.PlayerSaveData.GetFlag(bossFreeHealUsedFlag))
		{
			float bossHPMPRegenMod = SkillTreeLogicHelper.GetBossHPMPRegenMod();
			if (bossHPMPRegenMod > 0f)
			{
				SaveManager.PlayerSaveData.SetFlag(bossFreeHealUsedFlag, value: true);
				PlayerController playerController = PlayerManager.GetPlayerController();
				float f = (float)playerController.ActualMaxHealth * bossHPMPRegenMod;
				float f2 = (float)playerController.ActualMaxMana * bossHPMPRegenMod;
				f = Mathf.CeilToInt(f);
				f2 = Mathf.CeilToInt(f2);
				playerController.SetHealth(f, additive: true, runEvents: true);
				playerController.SetMana(f2, additive: true, runEvents: true);
				Vector2 absPos = playerController.Midpoint;
				absPos.y += playerController.CollisionBounds.height / 2f;
				string text = string.Format(LocalizationManager.GetString("LOC_ID_STATUS_EFFECT_HEALTH_RESTORE_1", SaveManager.PlayerSaveData.CurrentCharacter.IsFemale), (int)f);
				string text2 = string.Format(LocalizationManager.GetString("LOC_ID_STATUS_EFFECT_MANA_RESTORE_1", SaveManager.PlayerSaveData.CurrentCharacter.IsFemale), (int)f2);
				TextPopupManager.DisplayTextAtAbsPos(TextPopupType.HPGained, text + "\n<color=#47FFFB>" + text2 + "</color>", absPos);
			}
		}
	}

	private void RegenAbilityCooldowns()
	{
		PlayerController playerController = PlayerManager.GetPlayerController();
		playerController.CastAbility.ResetAbilityAmmo(CastAbilityType.Weapon);
		playerController.CastAbility.ResetAbilityAmmo(CastAbilityType.Talent);
		playerController.CastAbility.ResetAbilityAmmo(CastAbilityType.Spell);
		playerController.CastAbility.ResetAbilityCooldowns(CastAbilityType.Weapon);
		playerController.CastAbility.ResetAbilityCooldowns(CastAbilityType.Talent);
		playerController.CastAbility.ResetAbilityCooldowns(CastAbilityType.Spell);
	}

	protected override void OnPlayerInteractedWithTunnel(GameObject otherObj)
	{
		if (base.IsLocked && !base.IsCutsceneTeleport)
		{
			DialogueManager.StartNewDialogue();
			DialogueManager.AddDialogue(null, "LOC_ID_MISC_EVENT_TEXT_SPELLSWORD_DOOR_1", SaveManager.PlayerSaveData.CurrentCharacter.IsFemale);
			WindowManager.SetWindowIsOpen(WindowID.Dialogue, isOpen: true);
		}
		else
		{
			base.OnPlayerInteractedWithTunnel(otherObj);
		}
	}

	protected virtual PlayerSaveFlag GetBossFreeHealUsedFlag()
	{
		switch (base.Room.BiomeType)
		{
		case BiomeType.Castle:
			return PlayerSaveFlag.CastleBoss_FreeHeal_Used;
		case BiomeType.Stone:
			return PlayerSaveFlag.BridgeBoss_FreeHeal_Used;
		case BiomeType.Forest:
			return PlayerSaveFlag.ForestBoss_FreeHeal_Used;
		case BiomeType.Study:
			return PlayerSaveFlag.StudyBoss_FreeHeal_Used;
		case BiomeType.Tower:
		case BiomeType.TowerExterior:
			return PlayerSaveFlag.TowerBoss_FreeHeal_Used;
		case BiomeType.Cave:
			return PlayerSaveFlag.CaveBoss_FreeHeal_Used;
		case BiomeType.Garden:
			return PlayerSaveFlag.GardenBoss_FreeHeal_Used;
		default:
			return PlayerSaveFlag.None;
		}
	}
}
