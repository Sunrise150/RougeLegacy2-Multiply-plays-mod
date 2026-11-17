using UnityEngine;

namespace SceneManagement_RL;

public class TreeDeath_SceneTransition : HestiaDeath_SceneTransition
{
	protected override string AmbienceSFXName => "event:/Cut_Scenes/sfx_openingCutscene_blackScreen";

	public override TransitionID ID => TransitionID.TreeDeath;

	protected override void InitializeText()
	{
		int num = Mathf.Min(SaveManager.PlayerSaveData.TreeCutsceneDisplayCount, Ending_EV.TREE_CUTSCENE_DIALOGUE_1_LOCID.Length - 1);
		m_text1.text = LocalizationManager.GetString(Ending_EV.TREE_CUTSCENE_DIALOGUE_1_LOCID[num], SaveManager.PlayerSaveData.CurrentCharacter.IsFemale);
		m_text2.text = LocalizationManager.GetString(Ending_EV.TREE_CUTSCENE_DIALOGUE_2_LOCID[num], SaveManager.PlayerSaveData.CurrentCharacter.IsFemale);
		SaveManager.PlayerSaveData.SetFlag(PlayerSaveFlag.Play_Tree_DeathCutscene, value: false);
		SaveManager.PlayerSaveData.TreeCutsceneDisplayCount++;
	}
}
