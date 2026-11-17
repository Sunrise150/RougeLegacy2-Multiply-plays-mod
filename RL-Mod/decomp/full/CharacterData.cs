using System;

[Serializable]
public class CharacterData
{
	public string Name = "DEBUG";

	public bool IsFemale;

	public int DuplicateNameCount;

	public bool IsRetired;

	public bool IsVictory;

	public ClassType ClassType = ClassType.SwordClass;

	public AbilityType Weapon = AbilityType.SwordWeapon;

	public AbilityType Spell = AbilityType.FireballSpell;

	public AbilityType Talent;

	public TraitType TraitOne;

	public TraitType TraitTwo;

	public RelicType AntiqueOneOwned;

	public RelicType AntiqueTwoOwned;

	public int EyeType;

	public int MouthType;

	public int FacialHairType;

	public int SkinColorType;

	public int HairType;

	public int HairColorType;

	public int BodyType;

	public EquipmentType EdgeEquipmentType;

	public EquipmentType CapeEquipmentType;

	public EquipmentType ChestEquipmentType;

	public EquipmentType HeadEquipmentType;

	public EquipmentType TrinketEquipmentType;

	public byte Disposition_ID;

	public void CopyEquipment(CharacterData charToCopyFrom)
	{
		EdgeEquipmentType = charToCopyFrom.EdgeEquipmentType;
		CapeEquipmentType = charToCopyFrom.CapeEquipmentType;
		ChestEquipmentType = charToCopyFrom.ChestEquipmentType;
		HeadEquipmentType = charToCopyFrom.HeadEquipmentType;
		TrinketEquipmentType = charToCopyFrom.TrinketEquipmentType;
	}

	public CharacterData Clone()
	{
		return MemberwiseClone() as CharacterData;
	}
}
