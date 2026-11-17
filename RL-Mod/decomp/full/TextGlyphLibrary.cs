using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

[CreateAssetMenu(menuName = "Custom/Libraries/Text Glyph Library")]
public class TextGlyphLibrary : ScriptableObject
{
	public const string RESOURCES_PATH = "Scriptable Objects/Libraries/TextGlyphLibrary";

	private const string TMP_SPRITE_RICHTEXT_STRING_FORMAT = "<sprite=\"{0}\" name=\"{1}\">";

	[SerializeField]
	[HideInInspector]
	private TMP_SpriteAsset[] m_spriteAssetArray;

	private Dictionary<string, string> m_textGlyphDict;

	private static bool m_isInitialized;

	private static TextGlyphLibrary m_instance;

	public static Dictionary<string, string> TextGlyphDict => Instance.m_textGlyphDict;

	public static TMP_SpriteAsset[] SpriteAssetArray
	{
		get
		{
			return Instance.m_spriteAssetArray;
		}
		set
		{
			Instance.m_spriteAssetArray = value;
		}
	}

	public static TextGlyphLibrary Instance
	{
		get
		{
			if (m_instance == null)
			{
				m_instance = CDGResources.Load<TextGlyphLibrary>("Scriptable Objects/Libraries/TextGlyphLibrary");
				if (Application.isPlaying && !m_isInitialized)
				{
					m_instance.Initialize();
				}
			}
			return m_instance;
		}
	}

	private void Initialize()
	{
		m_textGlyphDict = new Dictionary<string, string>();
		TMP_SpriteAsset[] spriteAssetArray = m_spriteAssetArray;
		foreach (TMP_SpriteAsset tMP_SpriteAsset in spriteAssetArray)
		{
			if (tMP_SpriteAsset == null)
			{
				continue;
			}
			foreach (TMP_SpriteCharacter item in tMP_SpriteAsset.spriteCharacterTable)
			{
				if (item != null)
				{
					if (m_textGlyphDict.ContainsKey(item.name))
					{
						throw new Exception("Text Glyph Library has two sprites with the same |" + item.name + "| on different spritesheets.  All sprites must have distinct names.");
					}
					m_textGlyphDict.Add(item.name, tMP_SpriteAsset.spriteSheet.name);
				}
			}
		}
		m_isInitialized = true;
	}

	public static string GetTextGlyphSpritesheet(string entryName)
	{
		if (Instance.m_textGlyphDict.ContainsKey(entryName))
		{
			return Instance.m_textGlyphDict[entryName];
		}
		return null;
	}

	public static string GetTextGlyphRichTextString(string entryName)
	{
		if (Instance.m_textGlyphDict.ContainsKey(entryName))
		{
			return $"<sprite=\"{Instance.m_textGlyphDict[entryName]}\" name=\"{entryName}\">";
		}
		return null;
	}

	public static bool ContainsTextGlyph(string entryName)
	{
		return Instance.m_textGlyphDict.ContainsKey(entryName);
	}
}
