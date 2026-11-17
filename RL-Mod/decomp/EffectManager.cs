using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using SceneManagement_RL;
using UnityEngine;

public class EffectManager : MonoBehaviour, ILoadable
{
	private const string EFFECTSMANAGER_NAME = "EffectManager";

	private const string RESOURCE_PATH = "Prefabs/Managers/EffectManager";

	private const bool LOG_CALLER_STACK = false;

	private Dictionary<string, GenericPool_RL<BaseEffect>> m_effectStringDict;

	private HashSet<Animator> m_globalEffectDisableList = new HashSet<Animator>();

	private static bool m_isInitialized;

	private static EffectManager m_effectManager;

	private Action<MonoBehaviour, EventArgs> m_disableExitRoomEffects;

	private static object[] m_effectArgs;

	private static string m_effectArgsName;

	public static bool IsInitialized => m_isInitialized;

	public static EffectManager Instance => m_effectManager;

	private void Awake()
	{
		if (m_effectManager == null)
		{
			m_effectManager = this;
			m_disableExitRoomEffects = DisableExitRoomEffects;
			m_effectStringDict = new Dictionary<string, GenericPool_RL<BaseEffect>>();
			Messenger<GameMessenger, GameEvent>.AddListener(GameEvent.PlayerExitRoom, m_disableExitRoomEffects);
		}
		else
		{
			UnityEngine.Object.Destroy(base.gameObject);
		}
	}

	private void OnDestroy()
	{
		m_effectManager = null;
		Messenger<GameMessenger, GameEvent>.RemoveListener(GameEvent.PlayerExitRoom, m_disableExitRoomEffects);
		m_isInitialized = false;
	}

	public void LoadSync()
	{
		PopulateDictionary();
		m_isInitialized = true;
	}

	private void PopulateDictionary()
	{
		foreach (EffectCategoryType value in Enum.GetValues(typeof(EffectCategoryType)))
		{
			foreach (EffectEntry effectEntry in EffectLibrary.GetEffectEntryList(value))
			{
				AddDictionaryEntry(effectEntry);
			}
		}
	}

	public IEnumerator LoadAsync()
	{
		yield return EffectLibrary.LoadAsync();
		yield return PopulateDictionaryAsync();
		m_isInitialized = true;
	}

	private IEnumerator PopulateDictionaryAsync()
	{
		Stopwatch timer = new Stopwatch();
		timer.Start();
		foreach (EffectCategoryType value in Enum.GetValues(typeof(EffectCategoryType)))
		{
			foreach (EffectEntry effectEntry in EffectLibrary.GetEffectEntryList(value))
			{
				AddDictionaryEntry(effectEntry);
				if (timer.Elapsed.TotalMilliseconds >= 33.0)
				{
					yield return null;
					timer.Restart();
				}
			}
		}
	}

	private void AddDictionaryEntry(EffectEntry entry)
	{
		BaseEffect effectPrefab = entry.EffectPrefab;
		if (m_effectStringDict.ContainsKey(effectPrefab.name))
		{
			throw new Exception("Effect: " + effectPrefab.name + " already found in Effect Library.  Duplicates not allowed.");
		}
		GenericPool_RL<BaseEffect> genericPool_RL = new GenericPool_RL<BaseEffect>();
		genericPool_RL.Initialize(effectPrefab, entry.EffectPoolSize, disablePooling: false, disableAwakeOnCreation: false);
		m_effectStringDict.Add(effectPrefab.name, genericPool_RL);
	}

	public static bool HasEffect(string effectName)
	{
		return Instance.m_effectStringDict.ContainsKey(effectName);
	}

	public static List<BaseEffect> GetEffectList(string effectName)
	{
		if (HasEffect(effectName))
		{
			return Instance.m_effectStringDict[effectName].ObjectList;
		}
		return null;
	}

	public static void SetEffectParams(string effectName, params object[] args)
	{
		if (args.Length % 2 != 0)
		{
			throw new Exception("Effect params must follow the pattern of <VAR NAME>, <VAR TYPE>");
		}
		m_effectArgsName = effectName;
		m_effectArgs = args;
	}

	private static void ApplyEffectParams(string effectName, BaseEffect effect)
	{
		if (m_effectArgs != null)
		{
			try
			{
				for (int i = 0; i < m_effectArgs.Length - 1; i += 2)
				{
					effect.GetType().GetField(m_effectArgs[i] as string, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).SetValue(effect, m_effectArgs[i + 1]);
				}
			}
			catch (Exception ex)
			{
				UnityEngine.Debug.LogWarning("Applying effect params of type: " + effectName + " failed. Some of the parameters were incorrect.  Full Error: " + ex.Message);
			}
			Array.Clear(m_effectArgs, 0, m_effectArgs.Length);
		}
		m_effectArgs = null;
		m_effectArgsName = null;
	}

	public static BaseEffect PlayEffect(GameObject source, Animator sourceAnimator, string effectName, Vector3 effectPosition, float duration, EffectStopType stopType, EffectTriggerDirection direction = EffectTriggerDirection.None)
	{
		GenericPool_RL<BaseEffect> value = null;
		if (!Instance.m_effectStringDict.TryGetValue(effectName, out value))
		{
			throw new Exception("Effect: " + effectName + " cannot be found in EffectManager.  Please make sure the prefab is added to the Effect Library prefab.");
		}
		BaseEffect freeObj = value.GetFreeObj();
		ApplyEffectParams(effectName, freeObj);
		freeObj.EffectDirection = direction;
		freeObj.Source = source;
		freeObj.SourceAnimator = sourceAnimator;
		freeObj.gameObject.transform.position = new Vector3(effectPosition.x, effectPosition.y, freeObj.gameObject.transform.position.z);
		freeObj.gameObject.SetActive(value: true);
		freeObj.Play(duration, stopType);
		return freeObj;
	}

	public static BaseEffect PlayHitEffect(IDamageObj damageObj, Vector3 collisionPoint, string effectNameOverride, StrikeType strikeType, bool isUnscaled = false)
	{
		if (!string.IsNullOrEmpty(effectNameOverride))
		{
			return PlayEffect(damageObj.gameObject, null, effectNameOverride, collisionPoint, 0f, EffectStopType.Gracefully);
		}
		string text = "";
		if (isUnscaled)
		{
			text = "Unscaled";
		}
		return strikeType switch
		{
			StrikeType.Sharp => PlayEffect(damageObj.gameObject, null, "HitSparksSlash_Effect" + text, collisionPoint, 0f, EffectStopType.Gracefully), 
			StrikeType.NoDamage => PlayEffect(damageObj.gameObject, null, "HitSparksInvincible_Effect" + text, collisionPoint, 0f, EffectStopType.Gracefully), 
			StrikeType.Invincible_StatusEffect => PlayEffect(damageObj.gameObject, null, "ShieldHit_Effect" + text, collisionPoint, 0f, EffectStopType.Gracefully), 
			StrikeType.Critical => PlayEffect(damageObj.gameObject, null, "CriticalHit_Effect" + text, collisionPoint, 0f, EffectStopType.Gracefully), 
			StrikeType.OnHitAreaRelic => PlayEffect(damageObj.gameObject, null, "RelicOnHitAreaDamage_Chains_Effect", collisionPoint, 0f, EffectStopType.Gracefully), 
			_ => PlayEffect(damageObj.gameObject, null, "HitSparksGeneric_Effect" + text, collisionPoint, 0f, EffectStopType.Gracefully), 
		};
	}

	public static BaseEffect PlayDirectionalHitEffect(IDamageObj damageObj, GameObject damageRootObj, Vector3 collisionPoint)
	{
		Vector3 vector = damageRootObj.transform.position;
		IMidpointObj component = damageRootObj.GetComponent<IMidpointObj>();
		if (component != null)
		{
			vector = component.Midpoint;
		}
		BaseEffect baseEffect = PlayEffect(damageObj.gameObject, null, "DamageFlashDirectional_Effect", collisionPoint, 0f, EffectStopType.Gracefully);
		CDGTransformExtensions.SetLocalEulerZ(zRot: CDGHelper.AngleBetweenPts(vector, collisionPoint) - 90f, transform: baseEffect.transform);
		return baseEffect;
	}

	public static BaseEffect PlayHitEffectCamShake(float duration)
	{
		return PlayEffect(PlayerManager.GetCurrentPlayerRoom().gameObject, null, "CameraShakeVerySmall_Effect", Vector3.zero, duration, EffectStopType.Gracefully);
	}

	public static void StopEffect(BaseEffect effect, EffectStopType stopMode)
	{
		effect.Stop(stopMode);
	}

	public static void DisableAllEffectWithName(string effectName)
	{
		if ((bool)Instance && Instance.m_effectStringDict.TryGetValue(effectName, out var value))
		{
			value.DisableAll();
		}
	}

	public static void DisableAllEffects()
	{
		foreach (KeyValuePair<string, GenericPool_RL<BaseEffect>> item in Instance.m_effectStringDict)
		{
			if (!item.Value.DisableAll())
			{
				throw new Exception("Failed to disable effect: " + item.Key);
			}
		}
	}

	private void DisableExitRoomEffects(object sender, EventArgs args)
	{
		if (SceneLoader_RL.IsRunningTransitionWithLogic)
		{
			DisableAllEffects();
			return;
		}
		foreach (KeyValuePair<string, GenericPool_RL<BaseEffect>> item in Instance.m_effectStringDict)
		{
			foreach (BaseEffect @object in item.Value.ObjectList)
			{
				if (!@object.DisableDestroyOnRoomChange && @object.gameObject.activeSelf)
				{
					@object.gameObject.SetActive(value: false);
				}
			}
		}
	}

	public static void AddAnimatorToDisableList(Animator animator)
	{
		if ((bool)animator && !Instance.m_globalEffectDisableList.Contains(animator))
		{
			Instance.m_globalEffectDisableList.Add(animator);
		}
	}

	public static void RemoveAnimatorFromDisableList(Animator animator)
	{
		if ((bool)animator && Instance.m_globalEffectDisableList.Contains(animator))
		{
			Instance.m_globalEffectDisableList.Remove(animator);
		}
	}

	public static bool AnimatorEffectsDisabled(Animator animator)
	{
		if (!animator)
		{
			return false;
		}
		return Instance.m_globalEffectDisableList.Contains(animator);
	}

	public static void Reset()
	{
		DisableAllEffects();
	}
}
You are not using the latest version of the tool, please update.
Latest version is '11.2.0.10050' (yours is '8.2.0.7535-95108c96')
