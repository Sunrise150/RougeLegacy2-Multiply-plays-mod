using System.Collections;
using RL_Windows;

public class ForestBossTunnel : BossTunnel
{
	private MaxHealthChangeEventArgs m_healthChangeArgs;

	protected override void Awake()
	{
		base.Awake();
		m_healthChangeArgs = new MaxHealthChangeEventArgs(0f, 0f);
	}

	protected override IEnumerator EnterTunnelCoroutine()
	{
		if (0 + SaveManager.PlayerSaveData.GetRelic(RelicType.Lily1).Level + SaveManager.PlayerSaveData.GetRelic(RelicType.Lily2).Level + SaveManager.PlayerSaveData.GetRelic(RelicType.Lily3).Level > 0)
		{
			DialogueManager.StartNewDialogue();
			DialogueManager.AddDialogue("LOC_ID_SPECIAL_TITLE_FOREST_ACCEPT_OFFERING_1", "LOC_ID_SPECIAL_DESCRIPTION_FOREST_ACCEPT_OFFERING_1", SaveManager.PlayerSaveData.CurrentCharacter.IsFemale);
			WindowManager.SetWindowIsOpen(WindowID.Dialogue, isOpen: true);
			while (WindowManager.GetIsWindowOpen(WindowID.Dialogue))
			{
				yield return null;
			}
			RemoveLilyRelics();
		}
		yield return base.EnterTunnelCoroutine();
	}

	private void RemoveLilyRelics()
	{
		_ = 0 + SaveManager.PlayerSaveData.GetRelic(RelicType.Lily1).Level + SaveManager.PlayerSaveData.GetRelic(RelicType.Lily2).Level;
		_ = SaveManager.PlayerSaveData.GetRelic(RelicType.Lily3).Level;
		SaveManager.PlayerSaveData.GetRelic(RelicType.Lily1).SetLevel(0, additive: false);
		SaveManager.PlayerSaveData.GetRelic(RelicType.Lily2).SetLevel(0, additive: false);
		SaveManager.PlayerSaveData.GetRelic(RelicType.Lily3).SetLevel(0, additive: false);
		PlayerManager.GetPlayerController().InitializeHealthMods();
	}
}
