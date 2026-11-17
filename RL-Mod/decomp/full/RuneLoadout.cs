using System;
using System.Collections.Generic;

[Serializable]
public class RuneLoadout
{
	public List<SerializableVector2Int> RuneLevels = new List<SerializableVector2Int>(20);

	public void LoadLoadout()
	{
		RuneType[] typeArray = RuneType_RL.TypeArray;
		foreach (RuneType runeType in typeArray)
		{
			if (runeType != 0)
			{
				RuneManager.SetRuneEquippedLevel(runeType, 0, additive: false, runEvents: false);
			}
		}
		foreach (SerializableVector2Int runeLevel in RuneLevels)
		{
			RuneManager.SetRuneEquippedLevel((RuneType)runeLevel.x, runeLevel.y, additive: false, runEvents: false);
		}
	}

	public void SaveLoadout()
	{
		RuneLevels.Clear();
		foreach (KeyValuePair<RuneType, RuneObj> item in SaveManager.EquipmentSaveData.RuneDict)
		{
			RuneType key = item.Key;
			RuneObj value = item.Value;
			if (!value.IsNativeNull())
			{
				int equippedLevel = value.EquippedLevel;
				if (equippedLevel > 0)
				{
					RuneLevels.Add(new SerializableVector2Int((int)key, equippedLevel));
				}
			}
		}
	}

	public RuneLoadout Clone()
	{
		return MemberwiseClone() as RuneLoadout;
	}
}
