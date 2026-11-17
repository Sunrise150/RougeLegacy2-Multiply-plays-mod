using SceneManagement_RL;

public class ReturnToMainMenuOptionItem : ExecuteImmediateOptionItem
{
	public override void ActivateOption()
	{
		base.ActivateOption();
		if (PlayerManager.IsInstantiated)
		{
			BaseRoom currentPlayerRoom = PlayerManager.GetCurrentPlayerRoom();
			if (!currentPlayerRoom.IsNativeNull() && currentPlayerRoom.SaveController.CanSaveRoom)
			{
				RoomSaveData roomSaveData = SaveManager.StageSaveData.GetRoomSaveData(currentPlayerRoom.BiomeType, currentPlayerRoom.BiomeControllerIndex);
				currentPlayerRoom.SaveController.SaveRoomState(roomSaveData, exitingToMainMenu: true);
			}
		}
		if (ChallengeManager.IsInChallenge)
		{
			ChallengeManager.SetActiveChallenge(ChallengeType.None);
			ChallengeManager.RestoreCharacter(updateVisuals: false);
			SaveManager.DisableSaving = false;
		}
		if (SceneLoader_RL.CurrentScene != SceneLoadingUtility.GetSceneName(SceneID.Tutorial))
		{
			SaveManager.PlayerSaveData.SecondsPlayed += (uint)GameTimer.TotalSessionAccumulatedTime;
			GameTimer.ClearSessionAccumulatedTime();
			SaveManager.SaveAllCurrentProfileGameData(SavingType.FileOnly, saveStageData: true, ignoreTimeRestriction: true);
		}
		SceneLoader_RL.LoadScene(SceneID.MainMenu, TransitionID.FadeToBlackNoLoading);
	}
}
