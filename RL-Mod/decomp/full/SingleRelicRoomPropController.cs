public class SingleRelicRoomPropController : RelicRoomPropController
{
	protected override void InitializePooledPropOnEnter()
	{
		base.InitializePooledPropOnEnter();
		base.RightInfoTextBox.gameObject.SetActive(value: false);
		base.RightIcon.gameObject.SetActive(value: false);
		base.RightIconTwin.gameObject.SetActive(value: false);
	}
}
