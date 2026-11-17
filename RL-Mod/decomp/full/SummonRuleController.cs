using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SummonRuleController : MonoBehaviour, ISerializationCallbackReceiver, IRoomConsumer
{
	[NonSerialized]
	public List<EnemyTypeAndRank> SummonPool = new List<EnemyTypeAndRank>();

	[NonSerialized]
	public List<int> SpawnPoints = new List<int>();

	[NonSerialized]
	public List<int> AvailableSpawnPoints = new List<int>();

	[NonSerialized]
	public EnemyRank SummonDifficultyOverride = EnemyRank.None;

	[NonSerialized]
	public int SummonLevelOverride;

	[NonSerialized]
	public float SavedCurrentEnemyHP;

	[NonSerialized]
	public bool PoolIsBiomeSpecific;

	[NonSerialized]
	public bool PlayerInvincibilityActive;

	[SerializeField]
	private EnemySummonSerializedData[] m_serializedData = new EnemySummonSerializedData[0];

	[SerializeField]
	private UnityEngine.Object[] m_serializedObjArray = new UnityEngine.Object[0];

	[NonSerialized]
	public BaseSummonRule[] m_summonRuleArray = new BaseSummonRule[0];

	private bool m_isComplete;

	private int m_ruleExecutionIndex;

	private bool m_isArenaRunning;

	private bool m_isInitialized;

	public bool StillSummoning
	{
		get
		{
			int num = -1;
			for (int i = 0; i < SummonRuleArray.Length; i++)
			{
				if (SummonRuleArray[i] is SummonEnemy_SummonRule)
				{
					num = i;
				}
			}
			return m_ruleExecutionIndex < num;
		}
	}

	public BaseRoom Room { get; private set; }

	public bool IsArenaComplete => m_isComplete;

	public bool HasArenaStarted => m_isArenaRunning;

	public BaseSummonRule[] SummonRuleArray
	{
		get
		{
			return m_summonRuleArray;
		}
		set
		{
			m_summonRuleArray = value;
		}
	}

	public void ForceControllerComplete()
	{
		StopAllCoroutines();
		m_isComplete = true;
		m_isArenaRunning = false;
	}

	private void OnEnable()
	{
		if (GameUtility.IsInLevelEditor || WorldBuilder.State == BiomeBuildStateID.Complete)
		{
			StopAllCoroutines();
			if (!m_isInitialized)
			{
				Initialize();
			}
			ResetSummonRuleController();
			BaseSummonRule[] summonRuleArray = m_summonRuleArray;
			for (int i = 0; i < summonRuleArray.Length; i++)
			{
				summonRuleArray[i].OnEnable();
			}
			SummonPool.Clear();
			SpawnPoints.Clear();
			SummonDifficultyOverride = EnemyRank.None;
			SummonLevelOverride = 0;
			SavedCurrentEnemyHP = 0f;
			PoolIsBiomeSpecific = false;
		}
	}

	private void OnDisable()
	{
		BaseSummonRule[] summonRuleArray = m_summonRuleArray;
		foreach (BaseSummonRule obj in summonRuleArray)
		{
			obj.OnDisable();
			obj.ResetRule();
		}
		if (EnemyManager.IsInitialized)
		{
			EnemyManager.DisableAllSummonedEnemies();
		}
		StopArena(IsArenaComplete);
	}

	private void Initialize()
	{
		m_isInitialized = true;
		BaseSummonRule[] summonRuleArray = m_summonRuleArray;
		for (int i = 0; i < summonRuleArray.Length; i++)
		{
			summonRuleArray[i].Initialize(this);
		}
	}

	public void SetRoom(BaseRoom room)
	{
		Room = room;
		Room.PlayerEnterRelay.AddListener(OnPlayerEnter);
	}

	private void OnDestroy()
	{
		if ((bool)Room)
		{
			Room.PlayerEnterRelay.RemoveListener(OnPlayerEnter);
		}
	}

	private void OnPlayerEnter(object sender, EventArgs args)
	{
		StartCoroutine(SummonCoroutine());
	}

	public void StartArena()
	{
		m_isArenaRunning = true;
		if (PlayerInvincibilityActive)
		{
			PlayerManager.GetPlayerController().TakesNoDamage = false;
			PlayerInvincibilityActive = false;
		}
	}

	public void StopArena(bool arenaComplete)
	{
		m_isArenaRunning = false;
		m_isComplete = arenaComplete;
		if (PlayerInvincibilityActive)
		{
			PlayerManager.GetPlayerController().TakesNoDamage = false;
			PlayerInvincibilityActive = false;
		}
		StopAllCoroutines();
	}

	public void ResetSummonRuleController()
	{
		BaseSummonRule[] summonRuleArray = m_summonRuleArray;
		for (int i = 0; i < summonRuleArray.Length; i++)
		{
			summonRuleArray[i].ResetRule();
		}
		m_isComplete = false;
	}

	private IEnumerator SummonCoroutine()
	{
		m_ruleExecutionIndex = 0;
		while (!PlayerManager.IsInstantiated)
		{
			yield return null;
		}
		if (m_summonRuleArray != null)
		{
			for (int i = 0; i < m_summonRuleArray.Length; i++)
			{
				m_ruleExecutionIndex = i;
				BaseSummonRule baseSummonRule = m_summonRuleArray[i];
				if (!baseSummonRule.IsRuleComplete)
				{
					yield return baseSummonRule.RunSummonRule();
				}
			}
		}
		m_isComplete = true;
		m_isArenaRunning = false;
	}

	public void AddRule(SummonRuleType ruleType)
	{
		BaseSummonRule summonRule = SummonRuleType_RL.GetSummonRule(ruleType);
		AddNewRule(summonRule);
	}

	private void AddNewRule(BaseSummonRule ruleToAdd)
	{
		if (ruleToAdd != null)
		{
			List<BaseSummonRule> list = m_summonRuleArray.ToList();
			list.Add(ruleToAdd);
			m_summonRuleArray = list.ToArray();
		}
	}

	private void InsertNewRule(BaseSummonRule ruleToAdd, int index)
	{
		if (ruleToAdd != null)
		{
			List<BaseSummonRule> list = m_summonRuleArray.ToList();
			list.Insert(index, ruleToAdd);
			m_summonRuleArray = list.ToArray();
		}
	}

	public void RemoveRule(int index)
	{
		List<BaseSummonRule> list = m_summonRuleArray.ToList();
		list.RemoveAt(index);
		m_summonRuleArray = list.ToArray();
	}

	public BaseSummonRule GetRule(int index)
	{
		return m_summonRuleArray[index];
	}

	public void OnBeforeSerialize()
	{
		if (m_serializedObjArray.Length != SummonRuleArray.Length)
		{
			Array.Clear(m_serializedObjArray, 0, m_serializedObjArray.Length);
			m_serializedObjArray = new UnityEngine.Object[SummonRuleArray.Length];
		}
		if (m_serializedData.Length != SummonRuleArray.Length)
		{
			Array.Clear(m_serializedData, 0, m_serializedData.Length);
			m_serializedData = new EnemySummonSerializedData[SummonRuleArray.Length];
		}
		for (int i = 0; i < SummonRuleArray.Length; i++)
		{
			BaseSummonRule baseSummonRule = SummonRuleArray[i];
			string dataType = baseSummonRule.GetType().ToString();
			string data = JsonUtility.ToJson(baseSummonRule);
			if (m_serializedData[i] == null)
			{
				m_serializedData[i] = new EnemySummonSerializedData();
			}
			EnemySummonSerializedData obj = m_serializedData[i];
			obj.DataType = dataType;
			obj.Data = data;
			m_serializedObjArray[i] = baseSummonRule.SerializedObject;
		}
	}

	public void OnAfterDeserialize()
	{
		List<BaseSummonRule> list = new List<BaseSummonRule>();
		EnemySummonSerializedData[] serializedData = m_serializedData;
		foreach (EnemySummonSerializedData enemySummonSerializedData in serializedData)
		{
			Type type = Type.GetType(enemySummonSerializedData.DataType);
			list.Add((BaseSummonRule)JsonUtility.FromJson(enemySummonSerializedData.Data, type));
		}
		m_summonRuleArray = list.ToArray();
		if (m_summonRuleArray.Length == m_serializedObjArray.Length)
		{
			for (int j = 0; j < m_summonRuleArray.Length; j++)
			{
				SummonRuleArray[j].SetSerializedObject(m_serializedObjArray[j]);
			}
		}
		else
		{
			Debug.Log("<color=red>Could not load serialized objects into the summon rules. For some reason the array sizes do not match.</color>");
		}
	}
}
