public class SlowTimerRoomController : BaseSpecialRoomController
{
	protected override void OnPlayerEnterRoom(object sender, RoomViaDoorEventArgs eventArgs)
	{
		GlobalTimerHUDController.SlowTime = true;
	}

	protected override void OnPlayerExitRoom(object sender, RoomViaDoorEventArgs eventArgs)
	{
		GlobalTimerHUDController.SlowTime = false;
	}
}
