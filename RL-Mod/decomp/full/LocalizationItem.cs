using System;
using TMPro;
using UnityEngine;

public class LocalizationItem : MonoBehaviour, ILocalizable
{
	[SerializeField]
	[ReadOnlyOnPlay]
	private string m_locID;

	[SerializeField]
	private StringGenderType m_stringGenderType;

	[SerializeField]
	private bool m_alwaysRefreshText;

	protected TMP_Text m_textObj;

	private bool m_isInitialized;

	private bool m_textLoaded;

	private LanguageType m_currentLanguageType;

	private Action<MonoBehaviour, EventArgs> m_refreshText;

	public string LocID => m_locID;

	public TMP_Text TextObj => m_textObj;

	public StringGenderType StringGenderType
	{
		get
		{
			return m_stringGenderType;
		}
		set
		{
			m_stringGenderType = value;
		}
	}

	protected virtual string GetLocString(string locID, bool isFemale)
	{
		return LocalizationManager.GetString(locID, isFemale);
	}

	private void OnEnable()
	{
		if (!m_isInitialized)
		{
			Initialize();
		}
		if (!m_textLoaded || m_currentLanguageType != LocalizationManager.CurrentLanguageType)
		{
			RefreshText(null, null);
		}
		if (!m_alwaysRefreshText)
		{
			Messenger<UIMessenger, UIEvent>.AddListener(UIEvent.LanguageChanged, m_refreshText);
		}
	}

	private void OnDisable()
	{
		if (!m_alwaysRefreshText)
		{
			Messenger<UIMessenger, UIEvent>.RemoveListener(UIEvent.LanguageChanged, m_refreshText);
		}
	}

	private void Awake()
	{
		m_refreshText = RefreshText;
		Initialize();
		if (m_alwaysRefreshText)
		{
			Messenger<UIMessenger, UIEvent>.AddListener(UIEvent.LanguageChanged, m_refreshText);
		}
	}

	private void OnDestroy()
	{
		if (m_alwaysRefreshText)
		{
			Messenger<UIMessenger, UIEvent>.RemoveListener(UIEvent.LanguageChanged, m_refreshText);
		}
	}

	private void Initialize()
	{
		m_textObj = GetComponent<TMP_Text>();
		if ((bool)m_textObj)
		{
			m_isInitialized = true;
			RefreshText(null, null);
		}
		else
		{
			Debug.Log("Could not find TMP_Text object on LocalizationItem GameObject.");
		}
	}

	public void SetString(string locID)
	{
		m_locID = locID;
		RefreshText(null, null);
	}

	public void RefreshText(object sender, EventArgs args)
	{
		m_textLoaded = false;
		if (m_isInitialized && LocalizationManager.IsInitialized)
		{
			bool isFemale = SaveManager.PlayerSaveData.CurrentCharacter.IsFemale;
			if (m_stringGenderType == StringGenderType.Male)
			{
				isFemale = false;
			}
			else if (m_stringGenderType == StringGenderType.Female)
			{
				isFemale = true;
			}
			if (!string.IsNullOrEmpty(m_locID))
			{
				string locString = GetLocString(m_locID, isFemale);
				m_textObj.text = locString;
			}
			m_currentLanguageType = LocalizationManager.CurrentLanguageType;
			m_textLoaded = true;
		}
	}
}
