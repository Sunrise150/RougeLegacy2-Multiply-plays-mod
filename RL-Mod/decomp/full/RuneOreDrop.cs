using UnityEngine;

public class RuneOreDrop : BaseItemDrop
{
	public override ItemDropType ItemDropType => ItemDropType.RuneOre;

	protected override void Collect(GameObject collector)
	{
		int num = Economy_EV.GetItemDropValue(ItemDropType);
		if (base.ValueOverride > -1)
		{
			num = base.ValueOverride;
		}
		num = (int)((float)num * (1f + Economy_EV.GetRuneOreGainMod()));
		SaveManager.PlayerSaveData.RuneOreCollected += num;
		Vector2 absPos = collector.transform.position;
		absPos.x = base.transform.position.x;
		IMidpointObj component = collector.GetComponent<IMidpointObj>();
		if (component != null)
		{
			absPos = component.Midpoint;
			absPos.y += component.Midpoint.y - collector.transform.position.y;
		}
		int num2 = num;
		string text = num2 + " [RuneOre_Icon]";
		TextPopupManager.DisplayTextAtAbsPos(TextPopupType.RuneOreCollected, text, absPos);
		Messenger<GameMessenger, GameEvent>.Broadcast(GameEvent.RuneOreChanged, this, null);
		base.ValueOverride = num;
		base.Collect(collector);
	}
}
