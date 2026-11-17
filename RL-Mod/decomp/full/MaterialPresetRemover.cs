using System;
using System.Linq;
using TMPro;
using UnityEngine;

public class MaterialPresetRemover : MonoBehaviour
{
	[SerializeField]
	private LanguageType[] m_languagesToRemove;

	private TMP_Text m_tmpText;

	private Material m_cachedMaterialPreset;

	private bool m_materialPresetRemoved;

	private void Awake()
	{
		m_tmpText = GetComponent<TMP_Text>();
		if ((bool)m_tmpText)
		{
			m_cachedMaterialPreset = m_tmpText.fontSharedMaterial;
			if ((bool)m_cachedMaterialPreset)
			{
				Messenger<UIMessenger, UIEvent>.AddListener(UIEvent.LanguageChanged, OnLanguageChanged);
			}
		}
	}

	private void OnEnable()
	{
		OnLanguageChanged(null, null);
	}

	private void OnLanguageChanged(object sender, EventArgs args)
	{
		if (!LocalizationManager.IsInitialized || !m_tmpText)
		{
			return;
		}
		if (m_languagesToRemove.Contains(LocalizationManager.CurrentLanguageType))
		{
			if (!m_materialPresetRemoved)
			{
				m_tmpText.fontSharedMaterial = m_tmpText.font.material;
				m_materialPresetRemoved = true;
			}
		}
		else if (m_materialPresetRemoved)
		{
			m_tmpText.fontSharedMaterial = m_cachedMaterialPreset;
			m_materialPresetRemoved = false;
		}
	}

	private void OnDestroy()
	{
		Messenger<UIMessenger, UIEvent>.RemoveListener(UIEvent.LanguageChanged, OnLanguageChanged);
	}
}
