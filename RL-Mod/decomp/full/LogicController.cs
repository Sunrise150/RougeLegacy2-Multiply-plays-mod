using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using MoreMountains.CorgiEngine;
using UnityEngine;

[Serializable]
[RequireComponent(typeof(EnemyController))]
public class LogicController : MonoBehaviour
{
	[SerializeField]
	private bool m_hasNoLogic;

	[SerializeField]
	[HideInInspector]
	private string m_forceExecuteLogicBlockName;

	[SerializeField]
	[HideInInspector]
	private bool m_printDebug;

	private BaseAIScript m_logicScript;

	private LogicController_SO m_logicControllerSO;

	private EnemyLogicType m_enemyLogicType;

	private bool m_isExecuting;

	private string m_currentLogicBlockName;

	private string m_previousLogicBlockName;

	private bool m_isInitialized;

	private LogicState m_currentLogicState;

	private float m_restStateCounter;

	private bool m_hasExecutedBefore;

	private float m_iterateUpdateTime;

	private bool m_logicIsActivated;

	private float m_reactivationTimeOutTimer;

	private float m_aggroTimer;

	private EnemyController m_enemyController;

	private bool m_isExecutingDeathLogic;

	private Dictionary<string, float> m_cooldownLogicTimersDict;

	private Coroutine m_delayLogicCoroutine;

	private WaitRL_Yield m_logicDelayYield;

	private bool m_isDelayed = true;

	private List<string> m_disabledLogicBlocksList = new List<string>();

	private List<string> m_debugAttackList;

	private int m_debugAttackIndex;

	private Dictionary<LogicState, Dictionary<string, int>> m_logicBlockOddsOverrideDict = new Dictionary<LogicState, Dictionary<string, int>>();

	private static int RANK_STRING_HASH = Animator.StringToHash("Rank");

	private static int ISBOSS_STRING_HASH = Animator.StringToHash("IsBoss");

	private WaitForFixedUpdate m_waitFixedUpdateYield = new WaitForFixedUpdate();

	private bool m_hitboxesDisabled;

	private static Dictionary<Type, Dictionary<LogicState, StringMethodInfo_Dictionary>> m_staticMethodInfoDict = new Dictionary<Type, Dictionary<LogicState, StringMethodInfo_Dictionary>>();

	private static List<string> m_cooldownReductionHelper = new List<string>();

	private Rect m_rangeRect;

	public bool DisableRestLogicInterrupt { get; set; }

	public bool DisableDamageDuringInitialDelay { get; set; } = true;


	public bool ExecuteLogicInAir { get; set; } = true;


	public bool LogicIsActivated
	{
		get
		{
			if (m_logicIsActivated)
			{
				return base.enabled;
			}
			return false;
		}
	}

	public bool DisableLogicActivationByDistance { get; set; }

	public bool IsAggroed => m_aggroTimer > 0f;

	public string ForceExecuteLogicBlockName_OnceOnly { get; set; }

	public string ForceExecuteLogicBlockName
	{
		get
		{
			return m_forceExecuteLogicBlockName;
		}
		set
		{
			m_forceExecuteLogicBlockName = value;
		}
	}

	public string PreviousLogicBlockName => m_previousLogicBlockName;

	public EnemyLogicType EnemyLogicType
	{
		get
		{
			return m_enemyLogicType;
		}
		set
		{
			m_enemyLogicType = value;
		}
	}

	public bool PrintDebug
	{
		get
		{
			return m_printDebug;
		}
		set
		{
			m_printDebug = value;
		}
	}

	public bool IsExecutingLogic => m_isExecuting;

	public float CurrentLogicBlockPercentChance { get; private set; }

	public string CurrentLogicBlockName => m_currentLogicBlockName;

	public LogicState CurrentRangeState
	{
		get
		{
			return m_currentLogicState;
		}
		private set
		{
			m_currentLogicState = value;
		}
	}

	public float CloseRange
	{
		get
		{
			if (m_enemyController != null)
			{
				return m_enemyController.CloseRangeRadius;
			}
			return 0f;
		}
	}

	public float MediumRange
	{
		get
		{
			if (m_enemyController != null)
			{
				return m_enemyController.MediumRangeRadius;
			}
			return 0f;
		}
	}

	public float FarRange
	{
		get
		{
			if (m_enemyController != null)
			{
				return m_enemyController.FarRangeRadius;
			}
			return 0f;
		}
	}

	public LogicController_SO LogicControllerSO => m_logicControllerSO;

	public float RestCounter => m_restStateCounter;

	public BaseAIScript LogicScript => m_logicScript;

	public GameObject Player => PlayerManager.GetPlayer();

	public PlayerController PlayerController => PlayerManager.GetPlayerController();

	public bool IsInitialized => m_isInitialized;

	public Func<float, bool> IsInRange { get; set; }

	public void OverrideLogicDelay(float delayOverride)
	{
		m_logicDelayYield.CreateNew(delayOverride);
	}

	public void Awake()
	{
		if (!Application.isPlaying)
		{
			return;
		}
		UpdateLogicReferences();
		if ((bool)LogicScript && (bool)LogicControllerSO)
		{
			m_logicScript = UnityEngine.Object.Instantiate(LogicScript, base.transform);
			m_cooldownLogicTimersDict = new Dictionary<string, float>();
			PopulateStaticMethodInfoDict(EnemyLogicType, LogicScript.GetType());
			if (!m_enemyController)
			{
				m_enemyController = GetComponentInChildren<EnemyController>();
			}
			m_logicDelayYield = new WaitRL_Yield(0.175f);
			if (!GameUtility.IsInGame && !GameUtility.IsInLevelEditor)
			{
				m_isDelayed = false;
			}
			IsInRange = IsInRange_Standard;
		}
	}

	private void CreateDebugAttackList(LogicState logicState)
	{
		foreach (KeyValuePair<string, int> item in GetMethodNameDictionary(EnemyLogicType, logicState))
		{
			if (item.Value > 0 && !m_debugAttackList.Contains(item.Key))
			{
				m_debugAttackList.Add(item.Key);
			}
		}
	}

	private IEnumerator Start()
	{
		while (!m_enemyController.IsInitialized)
		{
			yield return null;
		}
		LogicScript.Initialize(m_enemyController);
		m_enemyController.CharacterHitResponse.OnCharacterHitRelay.AddListener(TriggerAggro);
		m_isInitialized = true;
		AssignAnimParamRank();
	}

	private void OnEnable()
	{
		AssignAnimParamRank();
	}

	public void AssignAnimParamRank()
	{
		if (m_enemyController.IsInitialized)
		{
			if (AnimatorUtility.HasParameter(m_enemyController.Animator, RANK_STRING_HASH))
			{
				m_enemyController.Animator.SetInteger(RANK_STRING_HASH, (int)m_enemyController.EnemyRank);
			}
			if (AnimatorUtility.HasParameter(m_enemyController.Animator, ISBOSS_STRING_HASH))
			{
				m_enemyController.Animator.SetBool(ISBOSS_STRING_HASH, m_enemyController.IsBoss);
			}
		}
	}

	private void OnDestroy()
	{
		m_enemyController.CharacterHitResponse.OnCharacterHitRelay.RemoveListener(TriggerAggro);
	}

	private IEnumerator DelayLogic()
	{
		m_isDelayed = true;
		yield return m_waitFixedUpdateYield;
		yield return null;
		if (m_logicDelayYield.WaitTime > 0f)
		{
			if (DisableDamageDuringInitialDelay && !m_hitboxesDisabled)
			{
				m_enemyController.HitboxController.SetHitboxActiveState(HitboxType.Weapon, active: false);
				m_hitboxesDisabled = true;
			}
			m_logicDelayYield.CreateNew(m_logicDelayYield.WaitTime);
			yield return m_logicDelayYield;
			if (DisableDamageDuringInitialDelay && m_hitboxesDisabled)
			{
				m_enemyController.HitboxController.SetHitboxActiveState(HitboxType.Weapon, active: true);
				m_hitboxesDisabled = false;
			}
		}
		m_isDelayed = false;
		m_isExecuting = false;
	}

	private void OnDisable()
	{
		if (m_delayLogicCoroutine != null)
		{
			StopCoroutine(m_delayLogicCoroutine);
			m_delayLogicCoroutine = null;
		}
		m_isDelayed = false;
		m_hitboxesDisabled = false;
		m_logicIsActivated = false;
		m_aggroTimer = 0f;
		m_enemyController.DeactivateEnemy();
	}

	public void SetLogicBlockEnabled(string logicBlockName, bool enabled)
	{
		if (enabled)
		{
			if (m_disabledLogicBlocksList.Contains(logicBlockName))
			{
				m_disabledLogicBlocksList.Remove(logicBlockName);
			}
		}
		else if (!m_disabledLogicBlocksList.Contains(logicBlockName))
		{
			m_disabledLogicBlocksList.Add(logicBlockName);
		}
	}

	public void TriggerAggro(object sender, EventArgs args)
	{
		m_aggroTimer = Enemy_EV.MIN_AGGRO_DURATION;
	}

	public void UpdateLogicReferences()
	{
		if (!Application.isPlaying || !m_logicScript || !m_logicControllerSO)
		{
			if (!m_enemyController)
			{
				m_enemyController = GetComponent<EnemyController>();
			}
			EnemyClassData enemyClassData = EnemyClassLibrary.GetEnemyClassData(m_enemyController.EnemyType);
			if ((bool)enemyClassData)
			{
				m_logicScript = enemyClassData.GetAIScript(m_enemyController.EnemyRank);
				m_logicControllerSO = enemyClassData.GetLogicController();
				EnemyLogicType = ConvertRankToLogicType(m_enemyController.EnemyRank);
			}
		}
	}

	private EnemyLogicType ConvertRankToLogicType(EnemyRank rank)
	{
		return rank switch
		{
			EnemyRank.Advanced => EnemyLogicType.Advanced, 
			EnemyRank.Expert => EnemyLogicType.Expert, 
			EnemyRank.Miniboss => EnemyLogicType.Miniboss, 
			_ => EnemyLogicType.Basic, 
		};
	}

	private Type GetLogicStateAttributeType(LogicState logicState)
	{
		return logicState switch
		{
			LogicState.Close => typeof(CloseLogicAttribute), 
			LogicState.Medium => typeof(MediumLogicAttribute), 
			LogicState.Far => typeof(FarLogicAttribute), 
			LogicState.Wander => typeof(WanderLogicAttribute), 
			_ => typeof(RestLogicAttribute), 
		};
	}

	public StringInt_Dictionary GetMethodNameDictionary(EnemyLogicType enemyLogicType, LogicState logicState)
	{
		if (!LogicControllerSO)
		{
			return null;
		}
		switch (enemyLogicType)
		{
		case EnemyLogicType.Basic:
			switch (logicState)
			{
			case LogicState.Close:
				return LogicControllerSO.BasicCloseLogic;
			case LogicState.Medium:
				return LogicControllerSO.BasicMediumLogic;
			case LogicState.Far:
				return LogicControllerSO.BasicFarLogic;
			case LogicState.Wander:
				return LogicControllerSO.BasicWanderLogic;
			case LogicState.Resting:
				return LogicControllerSO.BasicRestLogic;
			}
			break;
		case EnemyLogicType.Advanced:
			switch (logicState)
			{
			case LogicState.Close:
				return LogicControllerSO.AdvancedCloseLogic;
			case LogicState.Medium:
				return LogicControllerSO.AdvancedMediumLogic;
			case LogicState.Far:
				return LogicControllerSO.AdvancedFarLogic;
			case LogicState.Wander:
				return LogicControllerSO.AdvancerWanderLogic;
			case LogicState.Resting:
				return LogicControllerSO.AdvancedRestLogic;
			}
			break;
		case EnemyLogicType.Expert:
			switch (logicState)
			{
			case LogicState.Close:
				return LogicControllerSO.ExpertCloseLogic;
			case LogicState.Medium:
				return LogicControllerSO.ExpertMediumLogic;
			case LogicState.Far:
				return LogicControllerSO.ExpertFarLogic;
			case LogicState.Wander:
				return LogicControllerSO.ExpertWanderLogic;
			case LogicState.Resting:
				return LogicControllerSO.ExpertRestLogic;
			}
			break;
		case EnemyLogicType.Miniboss:
			switch (logicState)
			{
			case LogicState.Close:
				return LogicControllerSO.MinibossCloseLogic;
			case LogicState.Medium:
				return LogicControllerSO.MinibossMediumLogic;
			case LogicState.Far:
				return LogicControllerSO.MinibossFarLogic;
			case LogicState.Wander:
				return LogicControllerSO.MinibossWanderLogic;
			case LogicState.Resting:
				return LogicControllerSO.MinibossRestLogic;
			}
			break;
		}
		return null;
	}

	private static StringMethodInfo_Dictionary GetMethodInfoDictionary(Type logicType, LogicState logicState)
	{
		if (m_staticMethodInfoDict.TryGetValue(logicType, out var value))
		{
			if (value.TryGetValue(logicState, out var value2))
			{
				return value2;
			}
			throw new Exception("Could not find method info dict for AIScript type: " + logicType?.ToString() + " with logic state:" + logicState);
		}
		throw new Exception("Could not find method info dict for AIScript type: " + logicType?.ToString() + " with logic state:" + logicState);
	}

	public int GetLogicBlockOddsOverride(LogicState rangeState, string logicBlockName)
	{
		if (m_logicBlockOddsOverrideDict.TryGetValue(rangeState, out var value) && value.TryGetValue(logicBlockName, out var value2))
		{
			return value2;
		}
		return -1;
	}

	public void ChangeLogicBlockOdds(LogicState rangeState, string logicBlockName, int newOdds)
	{
		if (GetMethodNameDictionary(EnemyLogicType, rangeState).ContainsKey(logicBlockName))
		{
			if (!m_logicBlockOddsOverrideDict.TryGetValue(rangeState, out var value))
			{
				Dictionary<string, int> dictionary = new Dictionary<string, int>();
				m_logicBlockOddsOverrideDict.Add(rangeState, dictionary);
				value = dictionary;
			}
			if (value.ContainsKey(logicBlockName))
			{
				value[logicBlockName] = newOdds;
			}
			else
			{
				value.Add(logicBlockName, newOdds);
			}
		}
	}

	public void ResetLogicBlockOddsOverrides(LogicState rangeState, string logicBlockName)
	{
		if (!m_logicBlockOddsOverrideDict.TryGetValue(rangeState, out var value))
		{
			Dictionary<string, int> dictionary = new Dictionary<string, int>();
			m_logicBlockOddsOverrideDict.Add(rangeState, dictionary);
			value = dictionary;
		}
		if (value.ContainsKey(logicBlockName))
		{
			value.Remove(logicBlockName);
		}
	}

	public void ResetAllLogicBlockOddsOverrides()
	{
		foreach (KeyValuePair<LogicState, Dictionary<string, int>> item in m_logicBlockOddsOverrideDict)
		{
			item.Value.Clear();
		}
		m_logicBlockOddsOverrideDict.Clear();
	}

	public void InternalUpdate()
	{
		if (!IsInitialized || !LogicScript.IsInitialized || !m_enemyController.IsInitialized || LogicScript.IsPaused || m_isExecutingDeathLogic)
		{
			return;
		}
		UpdateLogicActivation();
		if (m_hasNoLogic || !m_logicIsActivated)
		{
			return;
		}
		if (m_enemyController != null && m_enemyController.ConditionState != 0 && m_enemyController.ConditionState != CharacterStates.CharacterConditions.DisableHorizontalMovement)
		{
			if (IsExecutingLogic)
			{
				StopAllLogic();
			}
		}
		else
		{
			if (!Application.isPlaying)
			{
				return;
			}
			UpdateLogicState(Time.deltaTime);
			UpdateLBCooldowns(Time.deltaTime);
			UpdateAggroTimer(Time.deltaTime);
			if (!m_isExecuting && !m_isDelayed && (m_enemyController.IsGrounded || m_enemyController.IsFlying || (!m_enemyController.IsGrounded && ExecuteLogicInAir)))
			{
				StopAllLogic();
				string forcedMethodName = (string.IsNullOrEmpty(ForceExecuteLogicBlockName_OnceOnly) ? ForceExecuteLogicBlockName : ForceExecuteLogicBlockName_OnceOnly);
				ForceExecuteLogicBlockName_OnceOnly = null;
				if (m_enemyController.StatusEffectController.HasStatusEffect(StatusEffectType.Enemy_Dizzy))
				{
					forcedMethodName = "Idle";
				}
				StartCoroutine(ExecuteLogic(m_currentLogicState, forcedMethodName));
			}
		}
	}

	private void UpdateLogicActivation()
	{
		if ((bool)m_enemyController && (m_enemyController.ForceActivate || IsAggroed))
		{
			if (!m_logicIsActivated)
			{
				m_reactivationTimeOutTimer = 0f;
				m_logicIsActivated = true;
				m_enemyController.ActivateEnemy();
				m_logicScript.OnEnemyActivated();
			}
		}
		else
		{
			if (IsAggroed || DisableLogicActivationByDistance)
			{
				return;
			}
			Vector3 position = CameraController.GameCamera.transform.position;
			float num = CameraController.GameCamera.orthographicSize * CameraController.GameCamera.aspect;
			float orthographicSize = CameraController.GameCamera.orthographicSize;
			Vector3 midpoint = m_enemyController.Midpoint;
			if (!m_logicIsActivated)
			{
				num += 9f;
				orthographicSize += 5.5f;
				if (midpoint.x > position.x - num && midpoint.x < position.x + num && midpoint.y > position.y - orthographicSize && midpoint.y < position.y + orthographicSize)
				{
					m_reactivationTimeOutTimer = 0f;
					m_logicIsActivated = true;
					m_enemyController.ActivateEnemy();
					m_logicScript.OnEnemyActivated();
				}
			}
			else if (CurrentLogicBlockName == "Idle" && !m_enemyController.IsBoss)
			{
				num += 16f;
				orthographicSize += 12f;
				if (midpoint.x > position.x + num || midpoint.x < position.x - num || midpoint.y > position.y + orthographicSize || midpoint.y < position.y - orthographicSize)
				{
					m_reactivationTimeOutTimer = 5f;
					m_logicIsActivated = false;
					m_enemyController.DeactivateEnemy();
				}
			}
		}
	}

	private void UpdateLBCooldowns(float elapsedTime)
	{
		m_cooldownReductionHelper.Clear();
		foreach (KeyValuePair<string, float> item in m_cooldownLogicTimersDict)
		{
			if (m_cooldownLogicTimersDict[item.Key] > 0f)
			{
				m_cooldownReductionHelper.Add(item.Key);
			}
		}
		foreach (string item2 in m_cooldownReductionHelper)
		{
			m_cooldownLogicTimersDict[item2] -= elapsedTime;
		}
	}

	private void UpdateAggroTimer(float elapsedTime)
	{
		if (m_aggroTimer > 0f)
		{
			m_aggroTimer -= elapsedTime;
		}
	}

	private void UpdateLogicState(float elapsedTime)
	{
		if (IsAggroed && m_currentLogicState == LogicState.Wander)
		{
			m_currentLogicState = LogicState.Far;
			return;
		}
		if (m_currentLogicState == LogicState.Resting)
		{
			m_restStateCounter -= elapsedTime;
			if (!(m_restStateCounter <= 0f))
			{
				return;
			}
			if (!DisableRestLogicInterrupt)
			{
				m_currentLogicState = LogicState.None;
				StopAllLogic();
			}
		}
		if (PlayerManager.IsInstantiated)
		{
			if (IsInRange(CloseRange))
			{
				m_currentLogicState = LogicState.Close;
			}
			else if (IsInRange(MediumRange))
			{
				m_currentLogicState = LogicState.Medium;
			}
			else if (IsInRange(FarRange))
			{
				m_currentLogicState = LogicState.Far;
			}
			else
			{
				m_currentLogicState = LogicState.Wander;
			}
		}
	}

	private bool IsInRange_Standard(float rangeRadius)
	{
		float num = rangeRadius * 0.6f;
		m_rangeRect.width = rangeRadius * 2f;
		m_rangeRect.height = num * 2f;
		m_rangeRect.x = m_enemyController.Midpoint.x - rangeRadius;
		m_rangeRect.y = m_enemyController.Midpoint.y - num;
		return m_rangeRect.Contains((Vector2)PlayerManager.GetPlayerController().Midpoint);
	}

	private IEnumerator ExecuteLogic(LogicState logicState, string forcedMethodName)
	{
		StringInt_Dictionary methodNameDictionary = GetMethodNameDictionary(EnemyLogicType, logicState);
		StringMethodInfo_Dictionary methodInfoDictionary = GetMethodInfoDictionary(LogicScript.GetType(), logicState);
		string text = null;
		int num = 0;
		if (!string.IsNullOrEmpty(forcedMethodName))
		{
			if (methodInfoDictionary.ContainsKey(forcedMethodName))
			{
				text = forcedMethodName;
				CurrentLogicBlockPercentChance = 1f;
			}
			else
			{
				Debug.Log("<color=red>Cannot run forced logic block: " + forcedMethodName + ". Logic block does not exist.  Please make sure to disable this before pushing.</color>");
			}
		}
		else
		{
			foreach (KeyValuePair<string, int> item in methodNameDictionary)
			{
				bool num2 = m_disabledLogicBlocksList.Contains(item.Key);
				bool flag = m_cooldownLogicTimersDict.ContainsKey(item.Key);
				if (!num2 && (!flag || (flag && m_cooldownLogicTimersDict[item.Key] <= 0f)))
				{
					int num3 = GetLogicBlockOddsOverride(logicState, item.Key);
					if (num3 == -1)
					{
						num3 = item.Value;
					}
					num += num3;
				}
			}
			int num4 = UnityEngine.Random.Range(1, num + 1);
			if (PrintDebug)
			{
				MonoBehaviour.print("Choosing logic to execute from " + m_currentLogicState.ToString().ToUpper() + " state. Roll is: " + num4 + " out of " + num);
			}
			int num5 = 0;
			foreach (KeyValuePair<string, int> item2 in methodNameDictionary)
			{
				if ((m_cooldownLogicTimersDict.ContainsKey(item2.Key) && m_cooldownLogicTimersDict[item2.Key] > 0f) || m_disabledLogicBlocksList.Contains(item2.Key))
				{
					continue;
				}
				int num6 = GetLogicBlockOddsOverride(logicState, item2.Key);
				if (num6 == -1)
				{
					num6 = item2.Value;
				}
				if (num6 > 0)
				{
					num5 += num6;
					if (num4 <= num5)
					{
						text = item2.Key;
						CurrentLogicBlockPercentChance = (float)num6 / (float)num;
						break;
					}
				}
			}
		}
		if (!string.IsNullOrEmpty(text))
		{
			if (PrintDebug)
			{
				MonoBehaviour.print("Logic choice made. Executing logic: " + text);
			}
			if (!m_hasExecutedBefore)
			{
				text = "Idle";
				m_hasExecutedBefore = true;
			}
			m_currentLogicBlockName = text;
			m_isExecuting = true;
			yield return (IEnumerator)methodInfoDictionary[text].Invoke(LogicScript, null);
			m_isExecuting = false;
			m_previousLogicBlockName = m_currentLogicBlockName;
			m_currentLogicBlockName = null;
			if (PrintDebug)
			{
				MonoBehaviour.print("Logic execution complete...");
			}
		}
		else if (PrintDebug)
		{
			MonoBehaviour.print("Cannot choose logic to execute. Ensure " + m_currentLogicState.ToString().ToUpper() + " state has assigned logic, and the chance of executing is greater than 0%");
		}
	}

	public void TriggerDeath()
	{
		m_isExecutingDeathLogic = true;
		m_enemyController.HitboxController.gameObject.SetActive(value: false);
		StopAllLogic();
	}

	public void StopAllLogic(bool resetCurrentState = false)
	{
		if (IsInitialized)
		{
			m_isExecuting = false;
			LogicScript.StopAllCoroutines();
			LogicScript.OnLBCompleteOrCancelled();
			StopAllCoroutines();
			if (resetCurrentState)
			{
				m_currentLogicState = LogicState.None;
			}
			if (m_isDelayed && base.isActiveAndEnabled && !m_isExecutingDeathLogic)
			{
				m_delayLogicCoroutine = StartCoroutine(DelayLogic());
			}
			m_enemyController.AttackingWithContactDamage = false;
		}
	}

	public void SetLBCooldown(string logicBlockName, float cooldown, bool ignoreMods)
	{
		if (!ignoreMods)
		{
			float num = BurdenManager.GetBurdenStatGain(BurdenType.EnemyAggression);
			if (m_enemyController.StatusEffectController.HasStatusEffect(StatusEffectType.Enemy_Speed) || m_enemyController.StatusEffectController.HasStatusEffect(StatusEffectType.Enemy_ArmorShred))
			{
				num += 0.25f;
			}
			num = Mathf.Max(1f - num, 0.25f);
			cooldown *= num;
		}
		if (!m_cooldownLogicTimersDict.ContainsKey(logicBlockName))
		{
			m_cooldownLogicTimersDict.Add(logicBlockName, cooldown);
		}
		m_cooldownLogicTimersDict[logicBlockName] = cooldown;
	}

	public void EnterRestState(float duration, bool ignoreMods)
	{
		m_currentLogicState = LogicState.Resting;
		float num = BurdenManager.GetBurdenStatGain(BurdenType.EnemyAggression);
		if (!ignoreMods)
		{
			if (m_enemyController.StatusEffectController.HasStatusEffect(StatusEffectType.Enemy_Speed) || m_enemyController.StatusEffectController.HasStatusEffect(StatusEffectType.Enemy_ArmorShred))
			{
				num += 0.25f;
			}
			num = Mathf.Max(1f - num, 0.25f);
			duration *= num;
		}
		m_restStateCounter = duration;
	}

	private Type GetEnemyLogicTypeAttribute(EnemyLogicType logicType)
	{
		return logicType switch
		{
			EnemyLogicType.Advanced => typeof(AdvancedEnemyAttribute), 
			EnemyLogicType.Expert => typeof(ExpertEnemyAttribute), 
			EnemyLogicType.Miniboss => typeof(MinibossEnemyAttribute), 
			_ => typeof(BasicEnemyAttribute), 
		};
	}

	private void PopulateStaticMethodInfoDict(EnemyLogicType enemyLogicType, Type logicScriptType)
	{
		if (m_staticMethodInfoDict.ContainsKey(logicScriptType))
		{
			return;
		}
		m_staticMethodInfoDict.Add(logicScriptType, new Dictionary<LogicState, StringMethodInfo_Dictionary>());
		MethodInfo[] methods = logicScriptType.GetMethods(BindingFlags.Instance | BindingFlags.Public);
		StringInt_Dictionary methodNameDictionary = GetMethodNameDictionary(enemyLogicType, LogicState.Close);
		StringInt_Dictionary methodNameDictionary2 = GetMethodNameDictionary(enemyLogicType, LogicState.Medium);
		StringInt_Dictionary methodNameDictionary3 = GetMethodNameDictionary(enemyLogicType, LogicState.Far);
		StringInt_Dictionary methodNameDictionary4 = GetMethodNameDictionary(enemyLogicType, LogicState.Wander);
		StringInt_Dictionary methodNameDictionary5 = GetMethodNameDictionary(enemyLogicType, LogicState.Resting);
		Dictionary<LogicState, StringMethodInfo_Dictionary> dictionary = m_staticMethodInfoDict[logicScriptType];
		StringMethodInfo_Dictionary stringMethodInfo_Dictionary2 = (dictionary[LogicState.Close] = new StringMethodInfo_Dictionary());
		StringMethodInfo_Dictionary stringMethodInfo_Dictionary3 = stringMethodInfo_Dictionary2;
		stringMethodInfo_Dictionary2 = (dictionary[LogicState.Medium] = new StringMethodInfo_Dictionary());
		StringMethodInfo_Dictionary stringMethodInfo_Dictionary5 = stringMethodInfo_Dictionary2;
		stringMethodInfo_Dictionary2 = (dictionary[LogicState.Far] = new StringMethodInfo_Dictionary());
		StringMethodInfo_Dictionary stringMethodInfo_Dictionary7 = stringMethodInfo_Dictionary2;
		stringMethodInfo_Dictionary2 = (dictionary[LogicState.Wander] = new StringMethodInfo_Dictionary());
		StringMethodInfo_Dictionary stringMethodInfo_Dictionary9 = stringMethodInfo_Dictionary2;
		stringMethodInfo_Dictionary2 = (dictionary[LogicState.Resting] = new StringMethodInfo_Dictionary());
		StringMethodInfo_Dictionary stringMethodInfo_Dictionary11 = stringMethodInfo_Dictionary2;
		MethodInfo[] array = methods;
		foreach (MethodInfo methodInfo in array)
		{
			string key = methodInfo.Name;
			if (methodNameDictionary.ContainsKey(key) && !stringMethodInfo_Dictionary3.ContainsKey(key))
			{
				stringMethodInfo_Dictionary3.Add(key, methodInfo);
			}
			if (methodNameDictionary2.ContainsKey(key) && !stringMethodInfo_Dictionary5.ContainsKey(key))
			{
				stringMethodInfo_Dictionary5.Add(key, methodInfo);
			}
			if (methodNameDictionary3.ContainsKey(key) && !stringMethodInfo_Dictionary7.ContainsKey(key))
			{
				stringMethodInfo_Dictionary7.Add(key, methodInfo);
			}
			if (methodNameDictionary4.ContainsKey(key) && !stringMethodInfo_Dictionary9.ContainsKey(key))
			{
				stringMethodInfo_Dictionary9.Add(key, methodInfo);
			}
			if (methodNameDictionary5.ContainsKey(key) && !stringMethodInfo_Dictionary11.ContainsKey(key))
			{
				stringMethodInfo_Dictionary11.Add(key, methodInfo);
			}
		}
	}

	public void Pause()
	{
		LogicScript.Pause();
	}

	public void Unpause()
	{
		LogicScript.Unpause();
	}

	public void ResetLogic()
	{
		if (IsInitialized)
		{
			m_restStateCounter = 0f;
			m_currentLogicState = LogicState.Wander;
			StopAllLogic();
			LogicScript.ResetScript();
			LogicScript.Unpause();
			if (base.gameObject.activeInHierarchy && base.enabled)
			{
				StartCoroutine(ExecuteLogic(LogicState.Close, "Idle"));
				m_delayLogicCoroutine = StartCoroutine(DelayLogic());
			}
			m_isExecutingDeathLogic = false;
			ResetAllLogicBlockOddsOverrides();
			if ((bool)m_enemyController && m_enemyController.HitboxController != null && !m_enemyController.HitboxController.gameObject.activeInHierarchy)
			{
				m_enemyController.HitboxController.gameObject.SetActive(value: true);
			}
		}
	}
}
