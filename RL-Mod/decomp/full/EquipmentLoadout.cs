using System;

[Serializable]
public class EquipmentLoadout
{
	public EquipmentType WeaponLoadout;

	public EquipmentType CapeLoadout;

	public EquipmentType ChestLoadout;

	public EquipmentType HeadLoadout;

	public EquipmentType TrinketLoadOut;

	public void LoadLoadout(CharacterData charData)
	{
		charData.EdgeEquipmentType = WeaponLoadout;
		charData.CapeEquipmentType = CapeLoadout;
		charData.ChestEquipmentType = ChestLoadout;
		charData.HeadEquipmentType = HeadLoadout;
		charData.TrinketEquipmentType = TrinketLoadOut;
	}

	public void SaveLoadout(CharacterData charData)
	{
		WeaponLoadout = charData.EdgeEquipmentType;
		CapeLoadout = charData.CapeEquipmentType;
		ChestLoadout = charData.ChestEquipmentType;
		HeadLoadout = charData.HeadEquipmentType;
		TrinketLoadOut = charData.TrinketEquipmentType;
	}

	public static void VerifyLoadoutWeight()
	{
		CharacterData currentCharacter = SaveManager.PlayerSaveData.CurrentCharacter;
		if (!EquipmentManager.CanEquip(EquipmentCategoryType.Weapon, currentCharacter.EdgeEquipmentType))
		{
			currentCharacter.EdgeEquipmentType = EquipmentType.None;
		}
		if (!EquipmentManager.CanEquip(EquipmentCategoryType.Head, currentCharacter.HeadEquipmentType))
		{
			currentCharacter.HeadEquipmentType = EquipmentType.None;
		}
		if (!EquipmentManager.CanEquip(EquipmentCategoryType.Chest, currentCharacter.ChestEquipmentType))
		{
			currentCharacter.ChestEquipmentType = EquipmentType.None;
		}
		if (!EquipmentManager.CanEquip(EquipmentCategoryType.Cape, currentCharacter.CapeEquipmentType))
		{
			currentCharacter.CapeEquipmentType = EquipmentType.None;
		}
		if (!EquipmentManager.CanEquip(EquipmentCategoryType.Trinket, currentCharacter.TrinketEquipmentType))
		{
			currentCharacter.TrinketEquipmentType = EquipmentType.None;
		}
	}

	public EquipmentLoadout Clone()
	{
		return MemberwiseClone() as EquipmentLoadout;
	}
}
