using System;

public class SoulShopOmniUIWindowController : BaseOmniUIWindowController<BaseOmniUICategoryEntry, SoulShopOmniUIEntry>
{
	public SoulShopType SelectedSoulShopType => base.ActiveEntryArray[base.SelectedEntryIndex].SoulShopType;

	public override WindowID ID => WindowID.SoulShop;

	protected override void CreateCategoryEntries()
	{
	}

	protected override void CreateEntries()
	{
		if (base.EntryArray != null)
		{
			Array.Clear(base.EntryArray, 0, base.EntryArray.Length);
			base.EntryArray = null;
		}
		base.EntryArray = base.EntryLayoutGroup.transform.GetComponentsInChildren<SoulShopOmniUIEntry>();
		int num = 0;
		SoulShopOmniUIEntry[] entryArray = base.EntryArray;
		foreach (SoulShopOmniUIEntry obj in entryArray)
		{
			obj.Initialize(this);
			obj.SetEntryIndex(num);
			num++;
		}
	}

	protected override void OnOpen()
	{
		if (SoulShopOmniUIIncrementResourceButton.OreTransferAmount > SaveManager.PlayerSaveData.EquipmentOreCollected)
		{
			SoulShopOmniUIIncrementResourceButton.OreTransferAmount = 0;
		}
		if (SoulShopOmniUIIncrementResourceButton.AetherTransferAmount > SaveManager.PlayerSaveData.RuneOreCollected)
		{
			SoulShopOmniUIIncrementResourceButton.AetherTransferAmount = 0;
		}
		SoulDrop.FakeSoulCounter_STATIC = 0;
		ItemDropManager.DisableAllItemDrops();
		Messenger<GameMessenger, GameEvent>.Broadcast(GameEvent.SoulChanged, null, null);
		if (SaveManager.ModeSaveData.GetSoulShopObj(SoulShopType.UnlockOverload).CurrentOwnedLevel > 0)
		{
			StoreAPIManager.GiveAchievement(AchievementType.SoulShopOverload, StoreType.All);
		}
		base.OnOpen();
	}

	protected override void OnClose()
	{
		base.OnClose();
		SaveManager.SaveCurrentProfileGameData(SaveDataType.GameMode, SavingType.FileOnly, ignoreTimeRestriction: true);
	}
}
