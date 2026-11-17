using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

[CreateAssetMenu(menuName = "Custom/Libraries/FontAsset Library")]
public class FontAssetLibrary : ScriptableObject
{
	[Serializable]
	private class FontAssetEntry
	{
		[SerializeField]
		private TMP_FontAsset m_defaultFontAsset;

		[SerializeField]
		private LanguageTypeFontAssetDictionary m_fontAssetTable;

		public TMP_FontAsset DefaultFontAsset => m_defaultFontAsset;

		public LanguageTypeFontAssetDictionary FontAssetTable => m_fontAssetTable;
	}

	[Serializable]
	private class FontMaterialEntry
	{
		[SerializeField]
		private Material m_defaultFontMaterial;

		[SerializeField]
		private LanguageTypeFontMaterialDictionary m_fontMaterialTable;

		public Material DefaultFontMaterial => m_defaultFontMaterial;

		public LanguageTypeFontMaterialDictionary FontMaterialTable => m_fontMaterialTable;
	}

	public const string RESOURCES_PATH = "Scriptable Objects/Libraries/FontAssetLibrary";

	private const string ASSET_PATH = "Assets/Content/Scriptable Objects/Libraries/FontAssetLibrary.asset";

	[Header("Font Asset Replacement")]
	[SerializeField]
	private FontAssetEntry[] m_fontAssetArray;

	[Header("Font Material Preset Replacement")]
	[Space(10f)]
	[SerializeField]
	private FontMaterialEntry[] m_fontMaterialArray;

	private Dictionary<TMP_FontAsset, FontAssetEntry> m_fontAssetDict;

	private Dictionary<Material, FontMaterialEntry> m_fontMaterialDict;

	private static FontAssetLibrary m_instance;

	private static FontAssetLibrary Instance
	{
		get
		{
			if (!m_instance)
			{
				if (Application.isPlaying)
				{
					m_instance = CDGResources.Load<FontAssetLibrary>("Scriptable Objects/Libraries/FontAssetLibrary");
				}
				if ((bool)m_instance)
				{
					m_instance.Initialize();
				}
			}
			return m_instance;
		}
	}

	private void Initialize()
	{
		m_fontAssetDict = new Dictionary<TMP_FontAsset, FontAssetEntry>();
		FontAssetEntry[] fontAssetArray = m_fontAssetArray;
		foreach (FontAssetEntry fontAssetEntry in fontAssetArray)
		{
			if (!m_fontAssetDict.ContainsKey(fontAssetEntry.DefaultFontAsset))
			{
				m_fontAssetDict.Add(fontAssetEntry.DefaultFontAsset, fontAssetEntry);
			}
			else
			{
				Debug.Log("<color=yellow>WARNING: Duplicate font ASSET entry found. Ignoring entry: " + fontAssetEntry.DefaultFontAsset.name);
			}
		}
		m_fontMaterialDict = new Dictionary<Material, FontMaterialEntry>();
		FontMaterialEntry[] fontMaterialArray = m_fontMaterialArray;
		foreach (FontMaterialEntry fontMaterialEntry in fontMaterialArray)
		{
			if (!m_fontMaterialDict.ContainsKey(fontMaterialEntry.DefaultFontMaterial))
			{
				m_fontMaterialDict.Add(fontMaterialEntry.DefaultFontMaterial, fontMaterialEntry);
			}
			else
			{
				Debug.Log("<color=yellow>WARNING: Duplicate font MATERIAL entry found. Ignoring entry: " + fontMaterialEntry.DefaultFontMaterial.name);
			}
		}
	}

	public static TMP_FontAsset GetFontAsset(TMP_FontAsset fontAsset, LanguageType language)
	{
		if (m_instance.m_fontAssetDict.TryGetValue(fontAsset, out var value) && value.FontAssetTable.TryGetValue(language, out var value2))
		{
			return value2;
		}
		return fontAsset;
	}

	public static Material GetFontMaterial(Material material, LanguageType language)
	{
		if (m_instance.m_fontMaterialDict.TryGetValue(material, out var value) && value.FontMaterialTable.TryGetValue(language, out var value2))
		{
			return value2;
		}
		return material;
	}
}
