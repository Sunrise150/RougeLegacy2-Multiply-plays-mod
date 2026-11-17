using System;
using UnityEngine;

public class TweenManager : MonoBehaviour
{
	[SerializeField]
	[ReadOnlyOnPlay]
	private int m_poolSize = 20;

	private GenericPool_RL<Tween> m_tweenPool;

	private Tween m_lastTweenedObj;

	private static bool m_isInitialized;

	private static TweenManager m_tweenManager;

	public static bool IsInitialized => m_isInitialized;

	public static TweenManager Instance
	{
		get
		{
			if (m_tweenManager == null)
			{
				m_tweenManager = CDGHelper.FindStaticInstance<TweenManager>(createNewIfNull: false);
			}
			return m_tweenManager;
		}
	}

	private void Awake()
	{
		Initialize();
	}

	private void Initialize()
	{
		Tween tween = new GameObject("Tween").AddComponent<Tween>();
		m_tweenPool = new GenericPool_RL<Tween>();
		m_tweenPool.Initialize(tween, m_poolSize, disablePooling: false);
		tween.transform.SetParent(base.transform);
		tween.gameObject.SetActive(value: false);
		m_isInitialized = true;
	}

	public static Tween RunFunction(float delay, object methodObject, string functionName, params object[] args)
	{
		Tween tween = TweenTo(methodObject, delay, Ease.None);
		tween.AddCustomEndHandler(methodObject, functionName, args);
		return tween;
	}

	public static Tween RunFunction_UnscaledTime(float delay, object methodObject, string functionName, params object[] args)
	{
		Tween tween = TweenTo_UnscaledTime(methodObject, delay, Ease.None);
		tween.AddCustomEndHandler(methodObject, functionName, args);
		return tween;
	}

	public static Tween TweenBy(object TweenRLObj, float duration, EaseDelegate ease, params object[] properties)
	{
		if (properties.Length % 2 != 0)
		{
			throw new Exception("TweenRL.By parameters must be submitted as follows - <property name>, <property value>");
		}
		Tween freeObj = Instance.m_tweenPool.GetFreeObj();
		freeObj.gameObject.SetActive(value: true);
		if (ease == null)
		{
			ease = Ease.None;
		}
		freeObj.SetValues(TweenRLObj, duration, useUnscaledTime: false, ease, tweenTo: false, properties);
		Instance.m_lastTweenedObj = freeObj;
		freeObj.StartTween();
		return freeObj;
	}

	public static Tween TweenBy_UnscaledTime(object TweenRLObj, float duration, EaseDelegate ease, params object[] properties)
	{
		if (properties.Length % 2 != 0)
		{
			throw new Exception("TweenRL.By parameters must be submitted as follows - <property name>, <property value>");
		}
		Tween freeObj = Instance.m_tweenPool.GetFreeObj();
		freeObj.gameObject.SetActive(value: true);
		if (ease == null)
		{
			ease = Ease.None;
		}
		freeObj.SetValues(TweenRLObj, duration, useUnscaledTime: true, ease, tweenTo: false, properties);
		Instance.m_lastTweenedObj = freeObj;
		freeObj.StartTween();
		return freeObj;
	}

	public static Tween TweenTo(object TweenRLObj, float duration, EaseDelegate ease, params object[] properties)
	{
		if (properties.Length % 2 != 0)
		{
			throw new Exception("TweenRL.To parameters must be submitted as follows - <property name>, <property value>");
		}
		Tween freeObj = Instance.m_tweenPool.GetFreeObj();
		freeObj.gameObject.SetActive(value: true);
		if (ease == null)
		{
			ease = Ease.None;
		}
		freeObj.SetValues(TweenRLObj, duration, useUnscaledTime: false, ease, tweenTo: true, properties);
		Instance.m_lastTweenedObj = freeObj;
		freeObj.StartTween();
		return freeObj;
	}

	public static Tween TweenTo_UnscaledTime(object TweenRLObj, float duration, EaseDelegate ease, params object[] properties)
	{
		if (properties.Length % 2 != 0)
		{
			throw new Exception("TweenRL.To parameters must be submitted as follows - <property name>, <property value>");
		}
		Tween freeObj = Instance.m_tweenPool.GetFreeObj();
		freeObj.gameObject.SetActive(value: true);
		if (ease == null)
		{
			ease = Ease.None;
		}
		freeObj.SetValues(TweenRLObj, duration, useUnscaledTime: true, ease, tweenTo: true, properties);
		Instance.m_lastTweenedObj = freeObj;
		freeObj.StartTween();
		return freeObj;
	}

	public static void StopAllTweens(bool runEndHandlers)
	{
		foreach (Tween @object in Instance.m_tweenPool.ObjectList)
		{
			if (@object.isActiveAndEnabled)
			{
				@object.StopTween(runEndHandlers);
			}
		}
	}

	public static void StopAllTweensContaining(object tweenedObj, bool runEndHandlers)
	{
		foreach (Tween @object in Instance.m_tweenPool.ObjectList)
		{
			if (@object.isActiveAndEnabled && @object.TweenedObj == tweenedObj)
			{
				@object.StopTween(runEndHandlers);
			}
		}
	}

	public static void StopAllTweensContaining(string ID, bool runEndHandlers)
	{
		foreach (Tween @object in Instance.m_tweenPool.ObjectList)
		{
			if (@object.isActiveAndEnabled && @object.ID == ID)
			{
				@object.StopTween(runEndHandlers);
			}
		}
	}

	public static void StopAllTweensContaining(object tweenedObj, string ID, bool runEndHandlers)
	{
		foreach (Tween @object in Instance.m_tweenPool.ObjectList)
		{
			if (@object.isActiveAndEnabled && @object.TweenedObj == tweenedObj && @object.ID == ID)
			{
				@object.StopTween(runEndHandlers);
			}
		}
	}

	public static void SetPauseAllTweensContaining(object tweenedObj, bool isPaused)
	{
		foreach (Tween @object in Instance.m_tweenPool.ObjectList)
		{
			if (@object.isActiveAndEnabled && @object.TweenedObj == tweenedObj)
			{
				@object.SetPaused(isPaused);
			}
		}
	}

	public static void SetPauseAllTweensContaining(object tweenedObj, string ID, bool isPaused)
	{
		foreach (Tween @object in Instance.m_tweenPool.ObjectList)
		{
			if (@object.isActiveAndEnabled && @object.TweenedObj == tweenedObj && @object.ID == ID)
			{
				@object.SetPaused(isPaused);
			}
		}
	}

	private void OnDestroy()
	{
		m_isInitialized = false;
	}
}
