using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Serializable]
public class LineageSaveData : IVersionUpdateable
{
	public const int MAX_PORTRAITS = 100;

	public int REVISION_NUMBER = 1;

	public int FILE_NUMBER;

	public List<CharacterData> LineageHeirList = new List<CharacterData>(100);

	public Dictionary<string, int> DuplicateNameCountDict = new Dictionary<string, int>(20);

	public int NumberOfHeirs;

	[NonSerialized]
	private bool m_portraitTestComplete;

	public LineageSaveData()
	{
		AddCharacterToLineage(new CharacterData
		{
			BodyType = 0,
			EyeType = 0,
			FacialHairType = 0,
			HairColorType = 0,
			MouthType = 0,
			SkinColorType = 0,
			IsFemale = false,
			ClassType = ClassType.SwordClass,
			Weapon = AbilityType.SwordWeapon,
			EdgeEquipmentType = EquipmentType.None,
			CapeEquipmentType = EquipmentType.None,
			HeadEquipmentType = EquipmentType.None,
			ChestEquipmentType = EquipmentType.None,
			TrinketEquipmentType = EquipmentType.None,
			Name = "Lee"
		});
	}

	public void CreatePortraitTest()
	{
		if (m_portraitTestComplete)
		{
			return;
		}
		for (int i = 0; i < 52; i++)
		{
			CharacterData characterData = new CharacterData();
			characterData.BodyType = UnityEngine.Random.Range(0, LookLibrary.GetBodyLookData().Count);
			characterData.EyeType = UnityEngine.Random.Range(0, LookLibrary.GetEyeLookData().Count);
			characterData.FacialHairType = UnityEngine.Random.Range(0, LookLibrary.GetFacialHairLookData().Count);
			characterData.HairColorType = UnityEngine.Random.Range(0, LookLibrary.GetHairColorLookData().Count);
			characterData.MouthType = UnityEngine.Random.Range(0, LookLibrary.GetMouthLookData().Count);
			characterData.SkinColorType = UnityEngine.Random.Range(0, LookLibrary.GetSkinColorLookData().Count);
			characterData.IsFemale = UnityEngine.Random.Range(0, 2) == 1;
			List<ClassType> list = ClassType_RL.TypeArray.ToList();
			list.Remove(ClassType.None);
			characterData.ClassType = list[UnityEngine.Random.Range(0, list.Count)];
			List<AbilityType> list2 = new List<AbilityType>();
			foreach (AbilityType value in Enum.GetValues(typeof(AbilityType)))
			{
				int num = (int)value;
				if (num > 10 && num < 130 && AbilityLibrary.GetAbility(value) != null)
				{
					list2.Add(value);
				}
			}
			characterData.Weapon = list2[UnityEngine.Random.Range(0, list2.Count)];
			characterData.EdgeEquipmentType = (EquipmentType)UnityEngine.Random.Range(0, 3);
			characterData.CapeEquipmentType = (EquipmentType)UnityEngine.Random.Range(0, 3);
			characterData.HeadEquipmentType = (EquipmentType)UnityEngine.Random.Range(0, 3);
			characterData.ChestEquipmentType = (EquipmentType)UnityEngine.Random.Range(0, 3);
			characterData.TrinketEquipmentType = (EquipmentType)UnityEngine.Random.Range(0, 3);
			string[] availableNames = CharacterCreator.GetAvailableNames(characterData.IsFemale);
			string name = availableNames[UnityEngine.Random.Range(0, availableNames.Length)];
			characterData.Name = name;
			AddCharacterToLineage(characterData);
		}
		m_portraitTestComplete = true;
	}

	public void AddCharacterToLineage(CharacterData charData)
	{
		int count = LineageHeirList.Count;
		if (count >= 100)
		{
			for (int i = 0; i < count - 1; i++)
			{
				LineageHeirList[i] = LineageHeirList[i + 1];
			}
			LineageHeirList[count - 1] = charData;
		}
		else
		{
			LineageHeirList.Add(charData);
		}
		NumberOfHeirs++;
	}

	public void UpdateVersion()
	{
		if (REVISION_NUMBER != 1 && REVISION_NUMBER <= 0)
		{
			NumberOfHeirs = LineageHeirList.Count;
			if (DuplicateNameCountDict == null)
			{
				DuplicateNameCountDict = new Dictionary<string, int>(20);
			}
			if (LineageHeirList == null)
			{
				LineageHeirList = new List<CharacterData>(100);
			}
			foreach (CharacterData lineageHeir in LineageHeirList)
			{
				if (!DuplicateNameCountDict.ContainsKey(lineageHeir.Name))
				{
					DuplicateNameCountDict.Add(lineageHeir.Name, 0);
				}
				else
				{
					DuplicateNameCountDict[lineageHeir.Name] = DuplicateNameCountDict[lineageHeir.Name] + 1;
				}
			}
			if (LineageHeirList.Count >= 100)
			{
				List<CharacterData> range = LineageHeirList.GetRange(LineageHeirList.Count - 100, 100);
				LineageHeirList.Clear();
				LineageHeirList.AddRange(range);
			}
		}
		REVISION_NUMBER = 1;
	}
}
