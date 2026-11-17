using System;
using UnityEngine;

public class CoinDrop : BaseItemDrop
{
	private GoldChangedEventArgs m_goldChangedEventArgs;

	public override ItemDropType ItemDropType => ItemDropType.Coin;

	protected override void Collect(GameObject collector)
	{
		int num = Economy_EV.GetItemDropValue(ItemDropType);
		if (base.ValueOverride > -1)
		{
			num = base.ValueOverride;
		}
		if (num > 0)
		{
			float goldGainMod = Economy_EV.GetGoldGainMod();
			float architectGoldMod = NPC_EV.GetArchitectGoldMod();
			num = Mathf.RoundToInt((float)num * (1f + goldGainMod) * architectGoldMod);
			num = Mathf.Clamp(num, 0, int.MaxValue);
			int goldCollected = SaveManager.PlayerSaveData.GoldCollected;
			SaveManager.PlayerSaveData.GoldCollected += num;
			if (m_goldChangedEventArgs == null)
			{
				m_goldChangedEventArgs = new GoldChangedEventArgs(goldCollected, SaveManager.PlayerSaveData.GoldCollected);
			}
			else
			{
				m_goldChangedEventArgs.Initialize(goldCollected, SaveManager.PlayerSaveData.GoldCollected);
			}
			Messenger<GameMessenger, GameEvent>.Broadcast(GameEvent.GoldChanged, this, m_goldChangedEventArgs);
			Vector2 absPos = collector.transform.position;
			absPos.x = base.transform.position.x;
			IMidpointObj component = collector.GetComponent<IMidpointObj>();
			if (component != null)
			{
				absPos = component.Midpoint;
				absPos.y += component.Midpoint.y - collector.transform.position.y;
			}
			string text = num + " [Coin_Icon]";
			TextPopupManager.DisplayTextAtAbsPos(TextPopupType.GoldCollected, text, absPos);
			base.ValueOverride = num;
			base.Collect(collector);
			return;
		}
		throw new Exception("Gold Drop of type " + ItemDropType.ToString() + " is worth 0 gold.");
	}
}
