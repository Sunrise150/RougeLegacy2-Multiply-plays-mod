public class RebelRoomController : BaseSpecialRoomController
{
	protected override void OnPlayerEnterRoom(object sender, RoomViaDoorEventArgs eventArgs)
	{
		base.OnPlayerEnterRoom(sender, eventArgs);
		if (SaveManager.PlayerSaveData.GetInsightState(InsightType.Ending_RebelsHidout) < InsightState.ResolvedButNotViewed)
		{
			SaveManager.PlayerSaveData.SetInsightState(InsightType.Ending_RebelsHidout, InsightState.ResolvedButNotViewed, forceOverride: false);
			InsightObjectiveCompleteHUDEventArgs eventArgs2 = new InsightObjectiveCompleteHUDEventArgs(InsightType.Ending_RebelsHidout, discovered: false);
			Messenger<UIMessenger, UIEvent>.Broadcast(UIEvent.DisplayObjectiveCompleteHUD, null, eventArgs2);
		}
	}
}
