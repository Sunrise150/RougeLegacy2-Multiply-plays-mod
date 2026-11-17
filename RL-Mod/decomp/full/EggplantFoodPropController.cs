using System.Collections;
using RL_Windows;
using UnityEngine;

public class EggplantFoodPropController : MonoBehaviour, IDisplaySpeechBubble
{
	public bool ShouldDisplaySpeechBubble => SaveManager.PlayerSaveData.GetHeirloomLevel(HeirloomType.EggplantFood) <= 0;

	public SpeechBubbleType BubbleType => SpeechBubbleType.PointOfInterest;

	public void TriggerEggplantFoodDialogue()
	{
		DialogueManager.StartNewDialogue();
		DialogueManager.AddDialogue("LOC_ID_EGGPLANT_TEXT_HEIRLOOM_BOX_TITLE_1", "LOC_ID_EGGPLANT_TEXT_HEIRLOOM_BOX_TEXT_1", SaveManager.PlayerSaveData.CurrentCharacter.IsFemale);
		WindowManager.SetWindowIsOpen(WindowID.Dialogue, isOpen: true);
		if (SaveManager.PlayerSaveData.GetHeirloomLevel(HeirloomType.EggplantFood) <= 0)
		{
			DialogueManager.AddDialogueCompleteEndHandler(GiveFood);
		}
	}

	private void GiveFood()
	{
		StartCoroutine(GiveFoodCoroutine());
	}

	private IEnumerator GiveFoodCoroutine()
	{
		RewiredMapController.SetIsInCutscene(inCutscene: true);
		SaveManager.PlayerSaveData.SetHeirloomLevel(HeirloomType.EggplantFood, 1, additive: false, broadcast: true);
		SaveManager.SaveCurrentProfileGameData(SaveDataType.Player, SavingType.FileOnly, ignoreTimeRestriction: true);
		float delay4 = Time.time + 0.25f;
		while (Time.time < delay4)
		{
			yield return null;
		}
		PlayerController playerController = PlayerManager.GetPlayerController();
		playerController.Animator.SetBool("Victory", value: true);
		delay4 = Time.time + 0.75f;
		while (Time.time < delay4)
		{
			yield return null;
		}
		if (!WindowManager.GetIsWindowLoaded(WindowID.SpecialItemDrop))
		{
			WindowManager.LoadWindow(WindowID.SpecialItemDrop);
		}
		HeirloomDrop heirloomDrop = new HeirloomDrop(HeirloomType.EggplantFood);
		(WindowManager.GetWindowController(WindowID.SpecialItemDrop) as SpecialItemDropWindowController).AddSpecialItemDrop(heirloomDrop);
		WindowManager.SetWindowIsOpen(WindowID.SpecialItemDrop, isOpen: true);
		while (WindowManager.GetIsWindowOpen(WindowID.SpecialItemDrop))
		{
			yield return null;
		}
		delay4 = Time.time + 0.25f;
		while (Time.time < delay4)
		{
			yield return null;
		}
		playerController.Animator.SetBool("Victory", value: false);
		delay4 = Time.time + 0.25f;
		while (Time.time < delay4)
		{
			yield return null;
		}
		RewiredMapController.SetIsInCutscene(inCutscene: false);
		RewiredMapController.SetCurrentMapEnabled(enabled: true);
	}
}
