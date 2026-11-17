using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(menuName = "Custom/Libraries/RelicLibrary")]
public class RelicLibrary : ScriptableObject
{
	public const string RESOURCES_PATH = "Scriptable Objects/Libraries/RelicLibrary";

	[SerializeField]
	private RelicTypeRelicDataDictionary m_relicLibrary;

	private static RelicLibrary m_instance = null;

	private static List<RelicType> m_rarityOneRelicHelper = new List<RelicType>();

	private static List<RelicType> m_rarityTwoRelicHelper = new List<RelicType>();

	private static List<RelicType> m_rarityThreeRelicHelper = new List<RelicType>();

	private static RelicLibrary Instance
	{
		get
		{
			if (m_instance == null)
			{
				m_instance = CDGResources.Load<RelicLibrary>("Scriptable Objects/Libraries/RelicLibrary");
			}
			return m_instance;
		}
	}

	public static RelicData GetRelicData(RelicType relicType)
	{
		if (Instance.m_relicLibrary.TryGetValue(relicType, out var value))
		{
			return value;
		}
		Debug.LogWarningFormat("<color=red>{0}: ({1}) Could not find RelicData ({2}) in Relic Library.</color>", Time.frameCount, Instance, relicType);
		return null;
	}

	public static RelicType GetRandomRelic(RngID rngIDToUse, bool limitMaxStacks, IEnumerable<RelicType> exceptionList = null)
	{
		RelicType relicType = RelicType.None;
		m_rarityOneRelicHelper.Clear();
		m_rarityTwoRelicHelper.Clear();
		m_rarityThreeRelicHelper.Clear();
		RelicType[] typeArray = RelicType_RL.TypeArray;
		foreach (RelicType relicType2 in typeArray)
		{
			if (relicType2 == RelicType.None || (exceptionList != null && exceptionList.Contains(relicType2)))
			{
				continue;
			}
			RelicData relicData = GetRelicData(relicType2);
			if ((bool)relicData && (!limitMaxStacks || SaveManager.PlayerSaveData.GetRelic(relicType2).Level < relicData.MaxStack))
			{
				switch (relicData.Rarity)
				{
				case 1:
					m_rarityOneRelicHelper.Add(relicType2);
					break;
				case 2:
					m_rarityTwoRelicHelper.Add(relicType2);
					break;
				case 3:
					m_rarityThreeRelicHelper.Add(relicType2);
					break;
				}
			}
		}
		float num = 1f;
		float num2 = -1f;
		num2 = ((rngIDToUse != 0) ? RNGManager.GetRandomNumber(rngIDToUse, "RelicLibrary.GetRandomRelic()", 0f, num) : Random.Range(0f, num));
		List<RelicType> list = null;
		if (num2 <= 1f)
		{
			list = m_rarityOneRelicHelper;
		}
		else if (num2 > 1f && num2 <= 1f)
		{
			list = m_rarityTwoRelicHelper;
			if (list.Count == 0)
			{
				list = m_rarityOneRelicHelper;
			}
		}
		else
		{
			list = m_rarityThreeRelicHelper;
			if (list.Count == 0)
			{
				list = m_rarityTwoRelicHelper;
			}
			if (list.Count == 0)
			{
				list = m_rarityOneRelicHelper;
			}
		}
		int count = list.Count;
		int num3 = -1;
		int num4 = 0;
		while (!IsRelicAllowed(relicType))
		{
			num3 = ((rngIDToUse != 0) ? RNGManager.GetRandomNumber(rngIDToUse, "RelicLibrary.GetRandomRelic()", 0, count) : Random.Range(0, count));
			relicType = list[num3];
			num4++;
			if (num4 > 50)
			{
				break;
			}
		}
		if (!IsRelicAllowed(relicType))
		{
			Debug.Log("<color=red>Could not find valid random relic.</color>");
		}
		return relicType;
	}

	public static bool IsRelicAllowed(RelicType relicType)
	{
		if (relicType == RelicType.None)
		{
			return false;
		}
		RelicData relicData = GetRelicData(relicType);
		if (relicData == null)
		{
			return false;
		}
		if (relicData.Rarity > 3)
		{
			return false;
		}
		return true;
	}
}
