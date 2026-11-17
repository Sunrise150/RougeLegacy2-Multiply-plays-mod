using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TextPopupManager : MonoBehaviour
{
	private const float TEXT_POPUP_DELAY_INTERVAL = 0.15f;

	private const float TEXT_POPUP_Y_OFFSET = 0.9f;

	private const float TEXT_POPUP_Y_OFFSET_DURATION = 0.1f;

	private const string TEXTPOPUPMANAGER_NAME = "TextPopupManager";

	private const string RESOURCE_PATH = "Prefabs/Managers/TextPopupManager";

	private Dictionary<TextPopupType, GenericPool_RL<TextPopupObj>> m_textPopupDict;

	private Dictionary<TextPopupType, float> m_popupLockoutDurationTable;

	private Dictionary<GameObject, Vector2> m_popupYOffsetDict = new Dictionary<GameObject, Vector2>();

	private List<GameObject> m_offsetClearHelper = new List<GameObject>();

	private static bool m_isInitialized;

	private static TextPopupManager m_textPopupManager;

	private static TextPopupManager Instance
	{
		get
		{
			if (m_textPopupManager == null)
			{
				m_textPopupManager = CDGHelper.FindStaticInstance<TextPopupManager>(createNewIfNull: false);
			}
			return m_textPopupManager;
		}
	}

	public static bool IsInitialized => m_isInitialized;

	private void Awake()
	{
		Initialize();
		SceneManager.sceneLoaded += OnSceneLoaded;
	}

	private void OnDestroy()
	{
		m_isInitialized = false;
		SceneManager.sceneLoaded -= OnSceneLoaded;
	}

	private void OnSceneLoaded(Scene arg0, LoadSceneMode arg1)
	{
		if (IsInitialized)
		{
			DisableAllTextPopups();
		}
	}

	private void Initialize()
	{
		m_popupLockoutDurationTable = new Dictionary<TextPopupType, float>();
		m_textPopupDict = new Dictionary<TextPopupType, GenericPool_RL<TextPopupObj>>();
		foreach (KeyValuePair<TextPopupType, TextPopupLibrary.TextPopupEntry> item in TextPopupLibrary.TextPopupDict)
		{
			TextPopupObj textPopupPrefab = item.Value.TextPopupPrefab;
			if (m_textPopupDict.ContainsKey(item.Key))
			{
				throw new Exception("Text Popup Type: " + item.Key.ToString() + " already found in TextPopup Library.  Duplicates not allowed.");
			}
			GenericPool_RL<TextPopupObj> genericPool_RL = new GenericPool_RL<TextPopupObj>();
			genericPool_RL.Initialize(textPopupPrefab, item.Value.PoolSize, disablePooling: false);
			m_textPopupDict.Add(item.Key, genericPool_RL);
			m_popupLockoutDurationTable.Add(item.Key, 0f);
		}
		m_isInitialized = true;
	}

	private TextPopupObj GetTextPopup(TextPopupType popupType)
	{
		GenericPool_RL<TextPopupObj> value = null;
		if (!m_textPopupDict.TryGetValue(popupType, out value))
		{
			throw new Exception("Text Popup Type: " + popupType.ToString() + " cannot be found in TextPopupManager.  Please make sure the prefab is added to the TextPopup Library prefab.");
		}
		return value.GetFreeObj();
	}

	public static TextPopupObj DisplayTextDefaultPos(TextPopupType popupType, string text, BaseCharacterController charController, bool attachToSource, bool queuePopup = true)
	{
		Vector2 spawnPos = charController.Midpoint;
		spawnPos.y += charController.CollisionBounds.height / 2f;
		spawnPos.y += 0.5f;
		return DisplayText(popupType, text, charController.gameObject, spawnPos, attachToSource, queuePopup);
	}

	public static TextPopupObj DisplayText(TextPopupType popupType, string text, GameObject source, Vector2 spawnPos, bool attachToSource, bool queuePopup = true)
	{
		spawnPos.y += 0.5f;
		TextPopupObj textPopup = Instance.GetTextPopup(popupType);
		textPopup.gameObject.SetActive(value: true);
		textPopup.Source = source;
		if (!textPopup.Text.Equals(text))
		{
			textPopup.SetText(text, TextAlignmentOptions.Center);
		}
		if (queuePopup)
		{
			if (!Instance.m_popupYOffsetDict.ContainsKey(source))
			{
				Instance.m_popupYOffsetDict.Add(source, new Vector2(Time.time, 0.9f));
			}
			else
			{
				Vector2 value = Instance.m_popupYOffsetDict[source];
				spawnPos.y += value.y;
				value.y += 0.9f;
				Instance.m_popupYOffsetDict[source] = value;
			}
		}
		textPopup.transform.localPosition = new Vector3(spawnPos.x, spawnPos.y, textPopup.transform.localPosition.z);
		if (attachToSource)
		{
			textPopup.transform.SetParent(source.transform, worldPositionStays: true);
		}
		textPopup.Spawn();
		return textPopup;
	}

	public static TextPopupObj DisplayTextAtAbsPos(TextPopupType popupType, string text, Vector2 absPos, GameObject source = null, TextAlignmentOptions alignmentOptions = TextAlignmentOptions.Center)
	{
		absPos.y += 0.5f;
		TextPopupObj textPopup = Instance.GetTextPopup(popupType);
		textPopup.gameObject.SetActive(value: true);
		if (!textPopup.Text.Equals(text))
		{
			textPopup.SetText(text, alignmentOptions);
		}
		textPopup.transform.localPosition = new Vector3(0f, 0f, textPopup.transform.localPosition.z);
		if ((bool)source)
		{
			textPopup.transform.SetParent(source.transform, worldPositionStays: false);
			textPopup.gameObject.transform.localPosition = new Vector3(absPos.x, absPos.y, textPopup.transform.localPosition.z);
			float num = 1f / source.transform.lossyScale.x;
			textPopup.gameObject.transform.localScale = new Vector3(num, num, num);
		}
		else
		{
			textPopup.gameObject.transform.position = new Vector3(absPos.x, absPos.y, textPopup.transform.position.z);
		}
		textPopup.Spawn();
		return textPopup;
	}

	public static TextPopupObj DisplayLocIDText(TextPopupType popupType, string locID, StringGenderType genderType, Vector2 position, float lockoutDuration = 0f)
	{
		position.y += 0.5f;
		if (Instance.m_popupLockoutDurationTable[popupType] <= Time.time)
		{
			TextPopupObj textPopup = Instance.GetTextPopup(popupType);
			textPopup.gameObject.SetActive(value: true);
			textPopup.SetLocIDText(locID, genderType);
			textPopup.gameObject.transform.position = new Vector3(position.x, position.y, textPopup.transform.position.z);
			textPopup.Spawn();
			Instance.m_popupLockoutDurationTable[popupType] = Time.time + lockoutDuration;
			return textPopup;
		}
		return null;
	}

	private void LateUpdate()
	{
		if (m_popupYOffsetDict.Count <= 0)
		{
			return;
		}
		m_offsetClearHelper.Clear();
		foreach (KeyValuePair<GameObject, Vector2> item in m_popupYOffsetDict)
		{
			Vector2 value = item.Value;
			if (Time.time > value.x + 0.1f)
			{
				m_offsetClearHelper.Add(item.Key);
			}
		}
		foreach (GameObject item2 in m_offsetClearHelper)
		{
			m_popupYOffsetDict.Remove(item2);
		}
	}

	public static void DisableAllTextPopups()
	{
		foreach (KeyValuePair<TextPopupType, GenericPool_RL<TextPopupObj>> item in Instance.m_textPopupDict)
		{
			item.Value.DisableAll();
		}
	}

	public static void DestroyPools()
	{
		foreach (KeyValuePair<TextPopupType, GenericPool_RL<TextPopupObj>> item in Instance.m_textPopupDict)
		{
			item.Value.DestroyPool();
		}
		Instance.m_textPopupDict.Clear();
	}
}
