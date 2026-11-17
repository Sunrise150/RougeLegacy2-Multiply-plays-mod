using UnityEngine;

public class SkillTreeManager : MonoBehaviour
{
	private const string SKILLTREE_MANAGER = "SkillTreeManager";

	private SkillLevelChangedEventArgs m_skillLevelChangedEventArgs;

	private static SkillTreeManager m_skillTreeManager;

	private static int m_cachedTotalSkillLevel;

	private static bool m_useCachedTotalSkillLevel;

	public static SkillTreeManager Instance
	{
		get
		{
			if (!m_skillTreeManager)
			{
				m_skillTreeManager = CDGHelper.FindStaticInstance<SkillTreeManager>(createNewIfNull: false);
			}
			return m_skillTreeManager;
		}
	}

	public static bool IsInitialized { get; private set; }

	private void Awake()
	{
		Initialize();
		Debug.Log("<color=green>Creating SkillTreeManager...</color>");
	}

	private void Initialize()
	{
		if (Application.isPlaying)
		{
			m_skillLevelChangedEventArgs = new SkillLevelChangedEventArgs(SkillTreeType.None, 0, 0);
			if (!SaveManager.EquipmentSaveData.IsInitialized)
			{
				SaveManager.EquipmentSaveData.Initialize();
			}
			IsInitialized = true;
		}
	}

	private void OnDestroy()
	{
		m_skillTreeManager = null;
		IsInitialized = false;
	}

	public static void ResetCachedTotalSkills()
	{
		m_useCachedTotalSkillLevel = false;
	}

	public static int GetTotalSkillObjLevel()
	{
		if (!m_useCachedTotalSkillLevel)
		{
			int num = 0;
			SkillTreeType[] typeArray = SkillTreeType_RL.TypeArray;
			foreach (SkillTreeType skillTreeType in typeArray)
			{
				if (skillTreeType != 0)
				{
					num += GetSkillObjLevel(skillTreeType);
				}
			}
			m_useCachedTotalSkillLevel = true;
			m_cachedTotalSkillLevel = num;
		}
		return m_cachedTotalSkillLevel;
	}

	public static int GetSkillObjLevel(SkillTreeType skillTreeType)
	{
		return GetSkillTreeObj(skillTreeType)?.ClampedLevel ?? 0;
	}

	public static bool SetSkillObjLevel(SkillTreeType skillTreeType, int level, bool additive, bool runEvents = true, bool ignoreMaxLevelClamping = false)
	{
		SkillTreeObj skillTreeObj = GetSkillTreeObj(skillTreeType);
		if (skillTreeObj != null)
		{
			int level2 = skillTreeObj.Level;
			if (additive)
			{
				skillTreeObj.Level += level;
			}
			else
			{
				skillTreeObj.Level = level;
			}
			if (!ignoreMaxLevelClamping)
			{
				skillTreeObj.Level = Mathf.Clamp(skillTreeObj.Level, 0, skillTreeObj.MaxLevel);
			}
			if (level2 != skillTreeObj.Level)
			{
				m_useCachedTotalSkillLevel = false;
				if (runEvents)
				{
					Instance.m_skillLevelChangedEventArgs.Initialize(skillTreeType, level2, skillTreeObj.Level);
					Messenger<UIMessenger, UIEvent>.Broadcast(UIEvent.SkillLevelChanged, Instance, Instance.m_skillLevelChangedEventArgs);
				}
				return true;
			}
		}
		return false;
	}

	public static SkillTreeObj GetSkillTreeObj(SkillTreeType skillTreeType)
	{
		SkillTreeObj value = null;
		SaveManager.EquipmentSaveData.SkillTreeDict.TryGetValue(skillTreeType, out value);
		return value;
	}
}
