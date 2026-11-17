using System;
using System.Collections.Generic;
using UnityEngine;

public class RelicHUDController : MonoBehaviour
{
	private const int STARTING_DICTIONARY_SIZE = 5;

	[SerializeField]
	private CanvasGroup m_relicIconGroup;

	[SerializeField]
	private RelicHUDIconController m_relicIconTemplate;

	private List<RelicHUDIconController> m_relicIconPool;

	private Dictionary<RelicType, RelicHUDIconController> m_activeRelicIconDict;

	private Comparison<RelicHUDIconController> m_iconSortMethod;

	private Action<MonoBehaviour, EventArgs> m_updateAllRelics;

	private Action<MonoBehaviour, EventArgs> m_showCanvasGroup;

	private Action<MonoBehaviour, EventArgs> m_hideCanvasGroup;

	private Action<MonoBehaviour, EventArgs> m_onRelicChanged;

	private Action<MonoBehaviour, EventArgs> m_onRelicPurified;

	private void Awake()
	{
		m_relicIconGroup.gameObject.SetActive(value: false);
		m_iconSortMethod = IconSort;
		m_relicIconPool = new List<RelicHUDIconController>(5);
		m_activeRelicIconDict = new Dictionary<RelicType, RelicHUDIconController>(5);
		for (int i = 0; i < 5; i++)
		{
			RelicHUDIconController relicHUDIconController = UnityEngine.Object.Instantiate(m_relicIconTemplate, m_relicIconGroup.transform);
			relicHUDIconController.gameObject.SetActive(value: false);
			m_relicIconPool.Add(relicHUDIconController);
		}
		m_updateAllRelics = UpdateAllRelics;
		m_showCanvasGroup = ShowCanvasGroup;
		m_hideCanvasGroup = HideCanvasGroup;
		m_onRelicChanged = OnRelicChanged;
		m_onRelicPurified = OnRelicPurified;
		m_relicIconTemplate.gameObject.SetActive(value: false);
		if (GameUtility.IsInLevelEditor)
		{
			Messenger<GameMessenger, GameEvent>.AddListener(GameEvent.LevelEditorWorldCreationComplete, m_updateAllRelics);
			Messenger<GameMessenger, GameEvent>.AddListener(GameEvent.LevelEditorWorldCreationComplete, m_showCanvasGroup);
		}
		else
		{
			Messenger<GameMessenger, GameEvent>.AddListener(GameEvent.WorldCreationComplete, m_updateAllRelics);
			Messenger<GameMessenger, GameEvent>.AddListener(GameEvent.WorldCreationComplete, m_showCanvasGroup);
		}
		Messenger<GameMessenger, GameEvent>.AddListener(GameEvent.RelicsReset, m_updateAllRelics);
		Messenger<GameMessenger, GameEvent>.AddListener(GameEvent.PlayerDeath, m_hideCanvasGroup);
	}

	private void OnDestroy()
	{
		if (GameUtility.IsInLevelEditor)
		{
			Messenger<GameMessenger, GameEvent>.RemoveListener(GameEvent.LevelEditorWorldCreationComplete, m_updateAllRelics);
			Messenger<GameMessenger, GameEvent>.RemoveListener(GameEvent.LevelEditorWorldCreationComplete, m_showCanvasGroup);
		}
		else
		{
			Messenger<GameMessenger, GameEvent>.RemoveListener(GameEvent.WorldCreationComplete, m_updateAllRelics);
			Messenger<GameMessenger, GameEvent>.RemoveListener(GameEvent.WorldCreationComplete, m_showCanvasGroup);
		}
		Messenger<GameMessenger, GameEvent>.RemoveListener(GameEvent.RelicsReset, m_updateAllRelics);
		Messenger<GameMessenger, GameEvent>.RemoveListener(GameEvent.PlayerDeath, m_hideCanvasGroup);
	}

	private void OnEnable()
	{
		Messenger<GameMessenger, GameEvent>.AddListener(GameEvent.RelicLevelChanged, m_onRelicChanged);
		Messenger<GameMessenger, GameEvent>.AddListener(GameEvent.RelicStatsChanged, m_onRelicChanged);
		Messenger<GameMessenger, GameEvent>.AddListener(GameEvent.RelicPurified, m_onRelicPurified);
		Messenger<UIMessenger, UIEvent>.AddListener(UIEvent.SkillTree_Opened, m_hideCanvasGroup);
		Messenger<UIMessenger, UIEvent>.AddListener(UIEvent.SkillTree_Closed, m_showCanvasGroup);
	}

	private void OnDisable()
	{
		Messenger<GameMessenger, GameEvent>.RemoveListener(GameEvent.RelicLevelChanged, m_onRelicChanged);
		Messenger<GameMessenger, GameEvent>.RemoveListener(GameEvent.RelicStatsChanged, m_onRelicChanged);
		Messenger<GameMessenger, GameEvent>.RemoveListener(GameEvent.RelicPurified, m_onRelicPurified);
		Messenger<UIMessenger, UIEvent>.RemoveListener(UIEvent.SkillTree_Opened, m_hideCanvasGroup);
		Messenger<UIMessenger, UIEvent>.RemoveListener(UIEvent.SkillTree_Closed, m_showCanvasGroup);
	}

	private void ShowCanvasGroup(object sender, EventArgs args)
	{
		if (m_activeRelicIconDict.Count > 0)
		{
			m_relicIconGroup.gameObject.SetActive(value: true);
		}
	}

	private void HideCanvasGroup(object sender, EventArgs args)
	{
		m_relicIconGroup.gameObject.SetActive(value: false);
	}

	private void UpdateAllRelics(MonoBehaviour sender, EventArgs args)
	{
		bool flag = false;
		if (PlayerManager.IsInstantiated)
		{
			RelicType[] typeArray = RelicType_RL.TypeArray;
			foreach (RelicType relicType in typeArray)
			{
				if (relicType != 0)
				{
					RelicObj relic = SaveManager.PlayerSaveData.GetRelic(relicType);
					if (relic.Level > 0)
					{
						relic.ApplyRelic(0);
					}
					if (UpdateRelicState(relicType))
					{
						flag = true;
					}
				}
			}
		}
		if (flag)
		{
			UpdateRelicOrder();
		}
		if (m_activeRelicIconDict.Count > 0)
		{
			if (!m_relicIconGroup.gameObject.activeSelf)
			{
				m_relicIconGroup.gameObject.SetActive(value: true);
			}
		}
		else if (m_relicIconGroup.gameObject.activeSelf)
		{
			m_relicIconGroup.gameObject.SetActive(value: false);
		}
	}

	private void UpdateRelicOrder()
	{
		m_relicIconPool.Sort(m_iconSortMethod);
		int num = 0;
		foreach (RelicHUDIconController item in m_relicIconPool)
		{
			if (item.RelicType != 0)
			{
				item.transform.SetSiblingIndex(num);
				num++;
			}
		}
	}

	private int IconSort(RelicHUDIconController a, RelicHUDIconController b)
	{
		int relicType = (int)a.RelicType;
		int relicType2 = (int)b.RelicType;
		if (relicType > relicType2)
		{
			return 1;
		}
		if (relicType < relicType2)
		{
			return -1;
		}
		return 0;
	}

	private bool UpdateRelicState(RelicType relicType)
	{
		bool result = false;
		RelicHUDIconController value2;
		if (SaveManager.PlayerSaveData.GetRelic(relicType).Level > 0)
		{
			if (!m_activeRelicIconDict.TryGetValue(relicType, out var value))
			{
				value = GetFreeRelicIcon();
				value.SetRelicType(relicType);
				value.UpdateRelic();
				if (!value.gameObject.activeSelf)
				{
					value.gameObject.SetActive(value: true);
				}
				m_activeRelicIconDict.Add(relicType, value);
				result = true;
			}
			else
			{
				value.UpdateRelic();
			}
		}
		else if (m_activeRelicIconDict.TryGetValue(relicType, out value2))
		{
			value2.SetRelicType(RelicType.None);
			value2.gameObject.SetActive(value: false);
			m_activeRelicIconDict.Remove(relicType);
		}
		return result;
	}

	private RelicHUDIconController GetFreeRelicIcon()
	{
		foreach (RelicHUDIconController item in m_relicIconPool)
		{
			if (item.RelicType == RelicType.None)
			{
				return item;
			}
		}
		int count = m_relicIconPool.Count;
		for (int i = 0; i < 5; i++)
		{
			RelicHUDIconController relicHUDIconController = UnityEngine.Object.Instantiate(m_relicIconTemplate, m_relicIconGroup.transform);
			relicHUDIconController.gameObject.SetActive(value: false);
			m_relicIconPool.Add(relicHUDIconController);
		}
		return m_relicIconPool[count];
	}

	private void OnRelicChanged(MonoBehaviour sender, EventArgs args)
	{
		RelicChangedEventArgs relicChangedEventArgs = args as RelicChangedEventArgs;
		if (UpdateRelicState(relicChangedEventArgs.RelicType))
		{
			UpdateRelicOrder();
		}
		if (m_activeRelicIconDict.Count > 0)
		{
			if (!m_relicIconGroup.gameObject.activeSelf)
			{
				m_relicIconGroup.gameObject.SetActive(value: true);
			}
		}
		else if (m_relicIconGroup.gameObject.activeSelf)
		{
			m_relicIconGroup.gameObject.SetActive(value: false);
		}
	}

	private void OnRelicPurified(MonoBehaviour sender, EventArgs args)
	{
		RelicChangedEventArgs relicChangedEventArgs = args as RelicChangedEventArgs;
		if (m_activeRelicIconDict.ContainsKey(relicChangedEventArgs.RelicType))
		{
			m_activeRelicIconDict[relicChangedEventArgs.RelicType].PlayPurifyEffect();
		}
	}
}
