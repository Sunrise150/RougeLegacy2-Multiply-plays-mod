using UnityEngine;

public class SoulDrop : BaseItemDrop
{
	public static int FakeSoulCounter_STATIC;

	private GoldChangedEventArgs m_goldChangedEventArgs;

	public override ItemDropType ItemDropType => ItemDropType.Soul;

	protected override void Collect(GameObject collector)
	{
		int num = Economy_EV.GetItemDropValue(ItemDropType);
		if (base.ValueOverride > -1)
		{
			num = base.ValueOverride;
		}
		FakeSoulCounter_STATIC -= num;
		Vector2 absPos = collector.transform.position;
		absPos.x = base.transform.position.x;
		IMidpointObj component = collector.GetComponent<IMidpointObj>();
		if (component != null)
		{
			absPos = component.Midpoint;
			absPos.y += component.Midpoint.y - collector.transform.position.y;
		}
		int num2 = num;
		string text = num2 + " [Soul_Icon]";
		TextPopupManager.DisplayTextAtAbsPos(TextPopupType.SoulCollected, text, absPos);
		Messenger<GameMessenger, GameEvent>.Broadcast(GameEvent.SoulChanged, this, null);
		base.ValueOverride = num;
		base.Collect(collector);
		if (!ItemDropManager.HasActiveItemDropsOfType(ItemDropType.Soul))
		{
			FakeSoulCounter_STATIC = 0;
		}
	}
}
