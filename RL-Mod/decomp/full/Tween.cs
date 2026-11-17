using System;
using System.Collections;
using System.Reflection;
using System.Runtime.CompilerServices;
using Sigtrap.Relays;
using UnityEngine;
using UnityEngine.Events;

public class Tween : MonoBehaviour, IGenericPoolObj
{
	private const int MAX_PROPERTY_COUNT = 10;

	private const string DELAY_STRING = "delay";

	private const string SUBTRACT_STRING = "subtract";

	private PropertyInfo[] m_propertyInfo = new PropertyInfo[10];

	private FieldInfo[] m_fieldInfo = new FieldInfo[10];

	private object[] m_structObjs = new object[10];

	private PropertyInfo[] m_structPropertyInfo = new PropertyInfo[10];

	private FieldInfo[] m_structFieldInfo = new FieldInfo[10];

	private string[] m_structName = new string[10];

	private object m_tweenedObject;

	private float m_tweenStartTime;

	private float m_tweenEndTime;

	private float[] m_initialValues;

	private bool m_tweenTo;

	private EaseDelegate m_ease;

	private bool m_useUnscaledTime;

	private bool m_paused;

	private bool m_updatesForegroundPostProcessing;

	private object[] m_propertiesList;

	private bool m_tweenStarted;

	private bool m_endTween;

	private bool m_endTweenImmediately;

	private bool m_endTweenWithoutEndHandlers;

	private object m_methodObject;

	private object[] m_methodArgs;

	private MethodInfo m_methodInfo;

	private const string m_nullString = "null";

	private Relay m_onTweenCompleteRelay = new Relay();

	public UnityEvent OnTweenComplete_UnityEvent;

	public bool IsFreePoolObj { get; set; }

	public bool IsAwakeCalled { get; protected set; } = true;


	public string TweenedObjectName { get; private set; }

	public bool UseUnscaledTime => m_useUnscaledTime;

	public float CoroutineStartTime { get; private set; }

	public float TweenStartTime => m_tweenStartTime;

	public float TweenEndTime => m_tweenEndTime;

	public float CoroutineElapsedTime
	{
		get
		{
			if (UseUnscaledTime)
			{
				return Time.unscaledTime;
			}
			return Time.time;
		}
	}

	public object[] PropertyArray => m_propertiesList;

	public IRelayLink OnTweenCompleteRelay => m_onTweenCompleteRelay.link;

	public Coroutine TweenCoroutine { get; private set; }

	public bool Paused => m_paused;

	public object TweenedObj => m_tweenedObject;

	public bool IsType { get; private set; }

	public string ID { get; set; }

	public void SetPaused(bool pause)
	{
		m_paused = pause;
	}

	public void SetValues(object tweenObject, float duration, bool useUnscaledTime, EaseDelegate ease, bool tweenTo, object[] properties)
	{
		if (tweenObject is Type)
		{
			IsType = true;
		}
		else
		{
			IsType = false;
		}
		m_tweenedObject = tweenObject;
		UnityEngine.Object @object = tweenObject as UnityEngine.Object;
		if (@object != null)
		{
			TweenedObjectName = @object.name;
		}
		if (CameraController.IsInstantiated && @object == CameraController.ForegroundPostProcessing)
		{
			m_updatesForegroundPostProcessing = true;
		}
		m_ease = ease;
		m_tweenTo = tweenTo;
		m_propertiesList = properties;
		m_useUnscaledTime = useUnscaledTime;
		if (m_useUnscaledTime)
		{
			m_tweenStartTime = Time.unscaledTime;
		}
		else
		{
			m_tweenStartTime = Time.time;
		}
		m_tweenEndTime = m_tweenStartTime + duration;
		m_initialValues = new float[m_propertiesList.Length / 2];
		m_tweenStarted = false;
		ID = null;
		float num = 0f;
		float num2 = 0f;
		for (int i = 0; i < m_propertiesList.Length; i += 2)
		{
			if (m_propertiesList[i + 1] is int)
			{
				m_propertiesList[i + 1] = (float)(int)m_propertiesList[i + 1];
			}
			if ((string)m_propertiesList[i] == "delay")
			{
				num = (float)m_propertiesList[i + 1];
			}
			if ((string)m_propertiesList[i] == "subtract")
			{
				num2 = (float)m_propertiesList[i + 1];
			}
		}
		m_tweenStartTime += num;
		m_tweenEndTime += num;
		m_tweenStartTime -= num2;
		m_tweenEndTime -= num2;
	}

	public void AddCustomEndHandler(object methodObject, string functionName, params object[] args)
	{
		if (methodObject == null)
		{
			throw new Exception("methodObject cannot be null");
		}
		Type[] array = new Type[args.Length];
		for (int i = 0; i < args.Length; i++)
		{
			Type type = args[i] as Type;
			if (type != null)
			{
				array[i] = type;
			}
			else
			{
				array[i] = args[i].GetType();
			}
		}
		m_methodInfo = methodObject.GetType().GetMethod(functionName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, array, null);
		if (m_methodInfo == null)
		{
			throw new Exception("Function " + functionName + " not found in class " + methodObject.GetType().ToString());
		}
		m_methodObject = methodObject;
		m_methodArgs = args;
	}

	public void AddCustomEndHandler_Type(Type objectType, string functionName, params object[] args)
	{
		Type[] array = new Type[args.Length];
		for (int i = 0; i < args.Length; i++)
		{
			Type type = args[i] as Type;
			if (type != null)
			{
				array[i] = type;
			}
			else
			{
				array[i] = args[i].GetType();
			}
		}
		m_methodInfo = objectType.GetMethod(functionName, array);
		if (m_methodInfo == null)
		{
			throw new Exception("Function " + functionName + " not found in class " + objectType.ToString());
		}
		m_methodObject = null;
		m_methodArgs = args;
	}

	public void StartTween()
	{
		CoroutineStartTime = Time.time;
		if (UseUnscaledTime)
		{
			CoroutineStartTime = Time.unscaledTime;
		}
		TweenCoroutine = StartCoroutine(UpdateCoroutine());
	}

	public void StopTween(bool runEndHandlers)
	{
		m_endTween = true;
		m_endTweenImmediately = true;
		m_endTweenWithoutEndHandlers = !runEndHandlers;
	}

	public void StopTweenWithConditionChecks(bool runEndHandlers, object tweenedObj, string id = null)
	{
		if (base.isActiveAndEnabled && tweenedObj == TweenedObj && (string.IsNullOrEmpty(id) || !(id != ID)))
		{
			StopTween(runEndHandlers);
		}
	}

	public void EndTween(bool runEndHandlers)
	{
		m_endTween = true;
		m_endTweenImmediately = false;
		m_endTweenWithoutEndHandlers = !runEndHandlers;
	}

	private void InvokeEndHandlers()
	{
		m_onTweenCompleteRelay.Dispatch();
		if (OnTweenComplete_UnityEvent != null)
		{
			OnTweenComplete_UnityEvent.Invoke();
		}
		if (m_methodInfo != null)
		{
			m_methodInfo.Invoke(m_methodObject, m_methodArgs);
		}
	}

	private void InitializeTween()
	{
		int num = 0;
		for (int i = 0; i < m_propertiesList.Length; i += 2)
		{
			string text = (string)m_propertiesList[i];
			if (!(text != "delay") || !(text != "subtract"))
			{
				continue;
			}
			string text2 = m_propertiesList[i] as string;
			string text3 = null;
			int num2 = text2.IndexOf('.');
			Type type = m_tweenedObject.GetType();
			m_structObjs[num] = null;
			m_structPropertyInfo[num] = null;
			m_structFieldInfo[num] = null;
			m_structName[num] = null;
			bool num3 = num2 > 0;
			if (num3)
			{
				text3 = text2.Substring(num2 + 1);
				text2 = text2.Substring(0, num2);
			}
			if (IsType)
			{
				m_propertyInfo[num] = (m_tweenedObject as Type).GetProperty(text2);
				if (m_propertyInfo[num] == null)
				{
					m_fieldInfo[num] = (m_tweenedObject as Type).GetField(text2);
				}
			}
			else
			{
				m_propertyInfo[num] = type.GetProperty(text2);
				if (m_propertyInfo[num] == null)
				{
					m_fieldInfo[num] = type.GetField(text2);
				}
			}
			if (num3)
			{
				m_structName[num] = text2;
				if (m_propertyInfo[num] != null)
				{
					m_structObjs[num] = m_propertyInfo[num].GetValue(m_tweenedObject, null);
				}
				else
				{
					m_structObjs[num] = type.GetField(m_structName[num]).GetValue(m_tweenedObject);
				}
				Type type2 = m_structObjs[num].GetType();
				m_structPropertyInfo[num] = type2.GetProperty(text3);
				if (m_structPropertyInfo[num] == null)
				{
					m_structFieldInfo[num] = type2.GetField(text3);
				}
			}
			if (m_propertyInfo[num] == null && m_fieldInfo[num] == null)
			{
				throw new Exception("Property/Field " + text2 + " not found on object " + m_tweenedObject.GetType().ToString());
			}
			if (m_propertyInfo[num] != null)
			{
				if (m_structObjs[num] == null)
				{
					m_initialValues[num] = Convert.ToSingle(m_propertyInfo[num].GetValue(m_tweenedObject, null));
				}
				else if (m_structPropertyInfo[num] != null)
				{
					m_initialValues[num] = Convert.ToSingle(m_structPropertyInfo[num].GetValue(m_structObjs[num], null));
				}
				else
				{
					m_initialValues[num] = Convert.ToSingle(m_structFieldInfo[num].GetValue(m_structObjs[num]));
				}
			}
			else if (m_structObjs[num] == null)
			{
				m_initialValues[num] = Convert.ToSingle(m_fieldInfo[num].GetValue(m_tweenedObject));
			}
			else if (m_structPropertyInfo[num] != null)
			{
				m_initialValues[num] = Convert.ToSingle(m_structPropertyInfo[num].GetValue(m_structObjs[num], null));
			}
			else
			{
				m_initialValues[num] = Convert.ToSingle(m_structFieldInfo[num].GetValue(m_structObjs[num]));
			}
			num++;
		}
	}

	public IEnumerator UpdateCoroutine()
	{
		float coroutineElapsedTime = Time.time;
		if (m_useUnscaledTime)
		{
			coroutineElapsedTime = Time.unscaledTime;
		}
		while (coroutineElapsedTime < m_tweenEndTime && !m_endTween)
		{
			while (m_paused && !m_endTween)
			{
				yield return null;
				if (!m_useUnscaledTime)
				{
					m_tweenEndTime += Time.deltaTime;
					coroutineElapsedTime = Time.time;
				}
				else
				{
					m_tweenEndTime += Time.unscaledDeltaTime;
					coroutineElapsedTime = Time.unscaledTime;
				}
			}
			while (coroutineElapsedTime < m_tweenStartTime && !m_endTween)
			{
				yield return null;
				coroutineElapsedTime = Time.time;
				if (m_useUnscaledTime)
				{
					coroutineElapsedTime = Time.unscaledTime;
				}
			}
			if (!m_tweenStarted)
			{
				m_tweenStarted = true;
				InitializeTween();
			}
			if (m_tweenedObject == null || m_tweenedObject.ToString().Equals("null"))
			{
				base.gameObject.SetActive(value: false);
				yield break;
			}
			int num = 0;
			for (int i = 0; i < m_propertiesList.Length - 1; i += 2)
			{
				string text = (string)m_propertiesList[i];
				if (!(text != "delay") || !(text != "subtract"))
				{
					continue;
				}
				float num2 = ((!m_tweenTo) ? m_ease(coroutineElapsedTime - m_tweenStartTime, m_initialValues[num], (float)m_propertiesList[i + 1], m_tweenEndTime - m_tweenStartTime) : m_ease(coroutineElapsedTime - m_tweenStartTime, m_initialValues[num], (float)m_propertiesList[i + 1] - m_initialValues[num], m_tweenEndTime - m_tweenStartTime));
				bool flag = m_propertyInfo[num] == null;
				bool num3 = m_structObjs[num] != null;
				bool flag2 = m_structPropertyInfo[num] == null;
				if (!num3)
				{
					if (!flag)
					{
						if (m_propertyInfo[num].PropertyType == typeof(int))
						{
							m_propertyInfo[num].SetValue(m_tweenedObject, (int)num2, null);
						}
						else
						{
							m_propertyInfo[num].SetValue(m_tweenedObject, num2, null);
						}
					}
					else if (m_fieldInfo[num].FieldType == typeof(int))
					{
						m_fieldInfo[num].SetValue(m_tweenedObject, (int)num2);
					}
					else
					{
						m_fieldInfo[num].SetValue(m_tweenedObject, num2);
					}
				}
				else
				{
					UpdateStructValues(num);
					if (!flag2)
					{
						if (m_structPropertyInfo[num].PropertyType == typeof(int))
						{
							m_structPropertyInfo[num].SetValue(m_structObjs[num], (int)num2, null);
						}
						else
						{
							m_structPropertyInfo[num].SetValue(m_structObjs[num], num2, null);
						}
					}
					else if (m_structFieldInfo[num].FieldType == typeof(int))
					{
						m_structFieldInfo[num].SetValue(m_structObjs[num], (int)num2);
					}
					else
					{
						m_structFieldInfo[num].SetValue(m_structObjs[num], num2);
					}
					if (!flag)
					{
						m_propertyInfo[num].SetValue(m_tweenedObject, m_structObjs[num], null);
					}
					else
					{
						m_fieldInfo[num].SetValue(m_tweenedObject, m_structObjs[num]);
					}
				}
				num++;
			}
			coroutineElapsedTime = Time.time;
			if (m_useUnscaledTime)
			{
				coroutineElapsedTime = Time.unscaledTime;
			}
			if (m_updatesForegroundPostProcessing)
			{
				CameraController.ForegroundPostProcessing.ApplyShader();
			}
			yield return null;
		}
		if (!m_tweenStarted)
		{
			m_tweenStarted = true;
			InitializeTween();
		}
		Complete();
	}

	private void UpdateStructValues(int index)
	{
		if (m_propertyInfo[index] != null)
		{
			m_structObjs[index] = m_propertyInfo[index].GetValue(m_tweenedObject, null);
		}
		else
		{
			m_structObjs[index] = m_fieldInfo[index].GetValue(m_tweenedObject);
		}
	}

	public void Complete()
	{
		if (!m_endTweenImmediately && m_tweenedObject != null && !m_tweenedObject.Equals(null))
		{
			int num = 0;
			for (int i = 0; i < m_propertiesList.Length - 1; i += 2)
			{
				string text = (string)m_propertiesList[i];
				if (!(text != "delay") || !(text != "subtract"))
				{
					continue;
				}
				float num2 = ((!m_tweenTo) ? (m_initialValues[num] + (float)m_propertiesList[i + 1]) : ((float)m_propertiesList[i + 1]));
				m_tweenStartTime = m_tweenEndTime;
				bool flag = m_propertyInfo[num] == null;
				bool num3 = m_structObjs[num] != null;
				bool flag2 = m_structPropertyInfo[num] == null;
				if (!num3)
				{
					if (!flag)
					{
						if (m_propertyInfo[num].PropertyType == typeof(int))
						{
							m_propertyInfo[num].SetValue(m_tweenedObject, (int)num2, null);
						}
						else
						{
							m_propertyInfo[num].SetValue(m_tweenedObject, num2, null);
						}
					}
					else if (m_fieldInfo[num].FieldType == typeof(int))
					{
						m_fieldInfo[num].SetValue(m_tweenedObject, (int)num2);
					}
					else
					{
						m_fieldInfo[num].SetValue(m_tweenedObject, num2);
					}
				}
				else
				{
					UpdateStructValues(num);
					if (!flag2)
					{
						if (m_structPropertyInfo[num].PropertyType == typeof(int))
						{
							m_structPropertyInfo[num].SetValue(m_structObjs[num], (int)num2, null);
						}
						else
						{
							m_structPropertyInfo[num].SetValue(m_structObjs[num], num2, null);
						}
					}
					else if (m_structFieldInfo[num].FieldType == typeof(int))
					{
						m_structFieldInfo[num].SetValue(m_structObjs[num], (int)num2);
					}
					else
					{
						m_structFieldInfo[num].SetValue(m_structObjs[num], num2);
					}
					if (!flag)
					{
						m_propertyInfo[num].SetValue(m_tweenedObject, m_structObjs[num], null);
					}
					else
					{
						m_fieldInfo[num].SetValue(m_tweenedObject, m_structObjs[num]);
					}
				}
				num++;
			}
		}
		if (!m_endTweenWithoutEndHandlers)
		{
			InvokeEndHandlers();
		}
		base.gameObject.SetActive(value: false);
	}

	private void OnDisable()
	{
		DisablePooledObjectManager.DisablePooledObject(this);
	}

	public void ResetValues()
	{
		m_tweenedObject = null;
		Array.Clear(m_propertyInfo, 0, m_propertyInfo.Length);
		Array.Clear(m_fieldInfo, 0, m_fieldInfo.Length);
		if (m_propertiesList != null)
		{
			Array.Clear(m_propertiesList, 0, m_propertiesList.Length);
		}
		m_propertiesList = null;
		if (m_initialValues != null)
		{
			Array.Clear(m_initialValues, 0, m_initialValues.Length);
		}
		m_initialValues = null;
		m_tweenStartTime = 0f;
		m_tweenEndTime = 0f;
		m_tweenTo = false;
		m_ease = Ease.None;
		m_paused = false;
		ID = null;
		m_tweenStarted = false;
		TweenCoroutine = null;
		CoroutineStartTime = 0f;
		m_endTween = false;
		m_endTweenImmediately = false;
		m_endTweenWithoutEndHandlers = false;
		m_onTweenCompleteRelay.RemoveAll();
		if (OnTweenComplete_UnityEvent != null)
		{
			OnTweenComplete_UnityEvent.RemoveAllListeners();
		}
		m_methodObject = null;
		m_methodInfo = null;
		if (m_methodArgs != null)
		{
			Array.Clear(m_methodArgs, 0, m_methodArgs.Length);
		}
	}

	[SpecialName]
	GameObject IGenericPoolObj.get_gameObject()
	{
		return base.gameObject;
	}
}
