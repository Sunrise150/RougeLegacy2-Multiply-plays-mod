using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using UnityEngine;

public class LocalizationManager : MonoBehaviour
{
	[Serializable]
	public struct PlatformLocIDReplacement
	{
		public string LocIDToReplace;

		public string XboxLocID;

		public string PlayStationLocID;

		public string NintendoSwitchLocID;

		public string GetReplacementLocID()
		{
			return LocIDToReplace;
		}
	}

	private const string LINE_BREAK = "</LB>";

	private const string FORMATTER_FORCE_M = "[M]";

	private const string FORMATTER_FORCE_F = "[F]";

	private static readonly string[] NAME_FILE_INSTRUCTIONS = new string[5] { "// There must be at least 5 names on this list, otherwise the game will use its default name list.", "// Each name must be separated by a carriage return.", "// Any unsupported characters will appear as tofu (□) in the game.", "// When saving this file, make sure its encoding is UTF-8.", "// Use Notepad++ for best results!" };

	private Dictionary<LanguageType, string> m_languageLocStringDict = new Dictionary<LanguageType, string>
	{
		{
			LanguageType.English,
			"LOC_ID_GAME_SETTING_LANGUAGE_ENGLISH_1"
		},
		{
			LanguageType.French,
			"LOC_ID_GAME_SETTING_LANGUAGE_FRENCH_1"
		},
		{
			LanguageType.German,
			"LOC_ID_GAME_SETTING_LANGUAGE_GERMAN_1"
		},
		{
			LanguageType.Italian,
			"LOC_ID_GAME_SETTING_LANGUAGE_ITALIAN_1"
		},
		{
			LanguageType.Portuguese_BR,
			"LOC_ID_GAME_SETTING_LANGUAGE_BRAZILIAN_PORTUGUESE_1"
		},
		{
			LanguageType.Spanish,
			"LOC_ID_GAME_SETTING_LANGUAGE_SPANISH_1"
		},
		{
			LanguageType.Russian,
			"LOC_ID_GAME_SETTING_LANGUAGE_RUSSIAN_1"
		},
		{
			LanguageType.Korean,
			"LOC_ID_GAME_SETTING_LANGUAGE_KOREAN_1"
		},
		{
			LanguageType.Turkish,
			"LOC_ID_GAME_SETTING_LANGUAGE_TURKISH_1"
		},
		{
			LanguageType.Chinese_Simp,
			"LOC_ID_GAME_SETTING_LANGUAGE_CHINESE_SIMPLIFIED_1"
		},
		{
			LanguageType.Chinese_Trad,
			"LOC_ID_GAME_SETTING_LANGUAGE_CHINESE_TRADITIONAL_1"
		}
	};

	[SerializeField]
	private LanguageTypeTextAssetDictionary m_languageDict;

	[SerializeField]
	private LanguageTypeTextAssetDictionary m_defaultMaleNamesDict;

	[SerializeField]
	private LanguageTypeTextAssetDictionary m_defaultFemaleNamesDict;

	[SerializeField]
	private PlatformLocIDReplacement[] m_platformLocIDReplacementArray;

	private LanguageType m_currentLanguageType;

	private Dictionary<string, string> m_maleLocDict;

	private Dictionary<string, string> m_femaleLocDict;

	private LanguageType[] m_availableLanguages;

	private LanguageChangedEventArgs m_langChangedEventArgs;

	private static bool m_isInitialized;

	private static bool m_usingMaleNameOverrideList;

	private static bool m_usingFemaleNameOverrideList;

	private string[] m_maleNameArray;

	private string[] m_femaleNameArray;

	private Dictionary<LanguageType, CultureInfo> m_cultureInfoDict;

	private static LocalizationManager m_manager = null;

	public static LanguageType[] AvailableLanguages => Instance.m_availableLanguages;

	public static int NumberOfLanguages => Instance.m_availableLanguages.Length;

	public static int MaleDictSize => Instance.m_maleLocDict.Count;

	public static int FemaleDictSize => Instance.m_femaleLocDict.Count;

	public static bool IsInitialized => m_isInitialized;

	public static string[] MaleNameArray => Instance.m_maleNameArray;

	public static string[] FemaleNameArray => Instance.m_femaleNameArray;

	public static LanguageType CurrentLanguageType => Instance.m_currentLanguageType;

	public static LocalizationManager Instance
	{
		get
		{
			if (!m_manager)
			{
				m_manager = CDGHelper.FindStaticInstance<LocalizationManager>(createNewIfNull: false);
			}
			return m_manager;
		}
	}

	public static string GetLanguageLocID(LanguageType languageType)
	{
		if (Instance.m_languageLocStringDict.TryGetValue(languageType, out var value))
		{
			return value;
		}
		return Instance.m_languageLocStringDict[LanguageType.English];
	}

	public static CultureInfo GetCurrentCultureInfo()
	{
		return GetCultureInfo(CurrentLanguageType);
	}

	public static CultureInfo GetCultureInfo(LanguageType languageType)
	{
		if (Instance.m_cultureInfoDict.TryGetValue(languageType, out var value))
		{
			return value;
		}
		return SaveManager.CultureInfo;
	}

	private void Awake()
	{
		if (!Application.isPlaying || IsInitialized)
		{
			return;
		}
		m_availableLanguages = new LanguageType[m_languageDict.Count];
		Dictionary<LanguageType, TextAsset>.KeyCollection keys = m_languageDict.Keys;
		int num = 0;
		foreach (LanguageType item in keys)
		{
			m_availableLanguages[num] = item;
			num++;
		}
		LoadLanguage(SaveManager.ConfigData.Language, broadcastEvent: false);
		LoadNames();
		InitializeCultureInfo();
		GlobalTextChangedStaticListener.Initialize();
		m_langChangedEventArgs = new LanguageChangedEventArgs(CurrentLanguageType);
		m_isInitialized = true;
	}

	private void InitializeCultureInfo()
	{
		m_cultureInfoDict = new Dictionary<LanguageType, CultureInfo>();
		m_cultureInfoDict.Add(LanguageType.English, CultureInfo.GetCultureInfo("en-US"));
		m_cultureInfoDict.Add(LanguageType.French, CultureInfo.GetCultureInfo("fr-FR"));
		m_cultureInfoDict.Add(LanguageType.German, CultureInfo.GetCultureInfo("de-DE"));
		m_cultureInfoDict.Add(LanguageType.Spanish, CultureInfo.GetCultureInfo("es-ES"));
		m_cultureInfoDict.Add(LanguageType.Italian, CultureInfo.GetCultureInfo("it-IT"));
		m_cultureInfoDict.Add(LanguageType.Portuguese_BR, CultureInfo.GetCultureInfo("pt-BR"));
		m_cultureInfoDict.Add(LanguageType.Turkish, CultureInfo.GetCultureInfo("tr-TR"));
		m_cultureInfoDict.Add(LanguageType.Russian, CultureInfo.GetCultureInfo("ru-RU"));
		m_cultureInfoDict.Add(LanguageType.Korean, CultureInfo.GetCultureInfo("ko-KR"));
		m_cultureInfoDict.Add(LanguageType.Chinese_Simp, CultureInfo.GetCultureInfo("zh-CN"));
		m_cultureInfoDict.Add(LanguageType.Chinese_Trad, CultureInfo.GetCultureInfo("zh-TW"));
	}

	private void ChangeLanguage_Internal(LanguageType languageType, bool broadcastEvent)
	{
		if (CurrentLanguageType != languageType)
		{
			LoadLanguage(languageType, broadcastEvent);
		}
	}

	private bool IsValidName(string name)
	{
		if (string.IsNullOrWhiteSpace(name))
		{
			return false;
		}
		if (name.Contains('\\'))
		{
			return false;
		}
		if (name.Length >= 2 && name[0] == '/' && name[1] == '/')
		{
			return false;
		}
		return true;
	}

	private void LoadOrCreateCustomNames(bool male)
	{
		string text = (male ? "male" : "female");
		Debug.Log("Checking for custom " + text + " names");
		string text2 = Path.Combine(Application.persistentDataPath, "CustomData");
		if (!Directory.Exists(text2))
		{
			try
			{
				Directory.CreateDirectory(text2);
				Debug.Log("<color=green>Created CustomData directory</color>");
			}
			catch (Exception ex)
			{
				Debug.Log("<color=red>CustomData directory does not exist and could not be created.</color> " + ex);
				return;
			}
		}
		text2 = Path.Combine(text2, StoreAPIManager.GetPlatformDirectoryName());
		if (!Directory.Exists(text2))
		{
			try
			{
				Directory.CreateDirectory(text2);
				Debug.Log("<color=green>Created CustomData/Platform subdirectory</color>");
			}
			catch (Exception ex2)
			{
				Debug.Log("<color=red>CustomData/Platform subdirectory does not exist and could not be created.</color> " + ex2);
				return;
			}
		}
		string path = Path.Combine(text2, male ? "MaleNameList.txt" : "FemaleNameList.txt");
		if (File.Exists(path))
		{
			try
			{
				string[] array = File.ReadAllLines(path);
				List<string> list = new List<string>();
				string[] array2 = array;
				for (int i = 0; i < array2.Length; i++)
				{
					string item = array2[i].Trim();
					if (IsValidName(item) && !list.Contains(item))
					{
						list.Add(item);
					}
				}
				if (list.Count >= 5)
				{
					if (male)
					{
						m_maleNameArray = list.ToArray();
						m_usingMaleNameOverrideList = true;
					}
					else
					{
						m_femaleNameArray = list.ToArray();
						m_usingFemaleNameOverrideList = true;
					}
					Debug.Log("Loaded custom " + text + " heir names");
				}
				return;
			}
			catch (Exception ex3)
			{
				Debug.Log("<color=red>Failed to load " + text + " heir names from file.</color> " + ex3);
				return;
			}
		}
		try
		{
			using (StreamWriter streamWriter = File.CreateText(path))
			{
				streamWriter.WriteLine("// Add your own " + text + " heir names in Rogue Legacy 2 here.");
				for (int j = 0; j < NAME_FILE_INSTRUCTIONS.Length; j++)
				{
					streamWriter.WriteLine(NAME_FILE_INSTRUCTIONS[j]);
				}
			}
			Debug.Log("<color=green>Successfully created custom " + text + " heir name file</color>");
		}
		catch (Exception ex4)
		{
			Debug.Log("<color=red>Failed to create custom " + text + " heir name file.</color> " + ex4);
		}
	}

	private void LoadNames()
	{
		LoadOrCreateCustomNames(male: true);
		LoadOrCreateCustomNames(male: false);
	}

	public static void LoadLanguage(LanguageType languageType, bool broadcastEvent)
	{
		if (Instance.m_maleLocDict == null)
		{
			Instance.m_maleLocDict = new Dictionary<string, string>();
		}
		else
		{
			Instance.m_maleLocDict.Clear();
		}
		if (Instance.m_femaleLocDict == null)
		{
			Instance.m_femaleLocDict = new Dictionary<string, string>();
		}
		else
		{
			Instance.m_femaleLocDict.Clear();
		}
		MonoBehaviour.print("Loading Rogue Legacy 2: " + System_EV.GetVersionString());
		MonoBehaviour.print("Loading language: " + languageType);
		TextAsset value = null;
		if (!Instance.m_languageDict.TryGetValue(languageType, out value))
		{
			Debug.Log("<color=red>Could not load language type: " + languageType.ToString() + ". Language file does not exist.  Loading English language file.</color>");
			value = Instance.m_languageDict[LanguageType.English];
			languageType = LanguageType.English;
		}
		if (!m_usingMaleNameOverrideList)
		{
			if (Instance.m_defaultMaleNamesDict.ContainsKey(languageType))
			{
				Instance.m_maleNameArray = Instance.m_defaultMaleNamesDict[languageType].text.Split(new string[1] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
			}
			else
			{
				Instance.m_maleNameArray = Instance.m_defaultMaleNamesDict[LanguageType.English].text.Split(new string[1] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
			}
		}
		if (!m_usingFemaleNameOverrideList)
		{
			if (Instance.m_defaultFemaleNamesDict.ContainsKey(languageType))
			{
				Instance.m_femaleNameArray = Instance.m_defaultFemaleNamesDict[languageType].text.Split(new string[1] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
			}
			else
			{
				Instance.m_femaleNameArray = Instance.m_defaultFemaleNamesDict[LanguageType.English].text.Split(new string[1] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
			}
		}
		string[] array = value.text.Split(new string[1] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
		string[] array2 = null;
		string[] array3 = array;
		foreach (string text in array3)
		{
			if (string.IsNullOrWhiteSpace(text) || text[0] == '\\' || text[0] == '/' || text[1] == '/')
			{
				continue;
			}
			array2 = text.Split('\t');
			int num = array2.Length;
			string text2 = array2[0];
			if (string.IsNullOrWhiteSpace(text2))
			{
				continue;
			}
			string text3 = text2;
			text2 = text2.Trim();
			if (text2 != text3)
			{
				Debug.Log("<color=yellow>WARNING: White spacing found before or after locstring: " + text2 + ".  The white spacing has been removed.</color>");
			}
			string text4 = array2[1];
			string text5 = null;
			if (num > 2)
			{
				text5 = array2[2];
			}
			bool flag = false;
			if (string.IsNullOrWhiteSpace(text4))
			{
				flag = true;
				text4 = text2;
			}
			if (string.IsNullOrWhiteSpace(text5))
			{
				text5 = text4;
				if (flag)
				{
					text5 += "-F";
				}
			}
			if (Instance.m_maleLocDict.ContainsKey(text2))
			{
				Instance.m_maleLocDict[text2] = text4;
				Instance.m_femaleLocDict[text2] = text5;
			}
			else
			{
				Instance.m_maleLocDict.Add(text2, text4);
				Instance.m_femaleLocDict.Add(text2, text5);
			}
		}
		PlatformLocIDReplacement[] platformLocIDReplacementArray = Instance.m_platformLocIDReplacementArray;
		for (int i = 0; i < platformLocIDReplacementArray.Length; i++)
		{
			PlatformLocIDReplacement platformLocIDReplacement = platformLocIDReplacementArray[i];
			if (Instance.m_maleLocDict.ContainsKey(platformLocIDReplacement.LocIDToReplace))
			{
				string replacementLocID = platformLocIDReplacement.GetReplacementLocID();
				if (Instance.m_maleLocDict.ContainsKey(replacementLocID))
				{
					Instance.m_maleLocDict[platformLocIDReplacement.LocIDToReplace] = Instance.m_maleLocDict[replacementLocID];
				}
				else
				{
					Debug.Log("Failed to replace male LocID: " + platformLocIDReplacement.LocIDToReplace + ". Replacement LocID: " + replacementLocID + " not found.");
				}
				if (Instance.m_femaleLocDict.ContainsKey(replacementLocID))
				{
					Instance.m_femaleLocDict[platformLocIDReplacement.LocIDToReplace] = Instance.m_femaleLocDict[replacementLocID];
					continue;
				}
				Debug.Log("Failed to replace female LocID: " + platformLocIDReplacement.LocIDToReplace + ". Replacement LocID: " + replacementLocID + " not found.");
			}
			else
			{
				Debug.Log("Failed to replace LocID: " + platformLocIDReplacement.LocIDToReplace + ". LocID not found.");
			}
		}
		Instance.m_currentLanguageType = languageType;
		if (broadcastEvent)
		{
			Instance.m_langChangedEventArgs.Initialize(CurrentLanguageType);
			Messenger<UIMessenger, UIEvent>.Broadcast(UIEvent.LanguageChanged, Instance, Instance.m_langChangedEventArgs);
		}
	}

	public static bool ContainsLocID(string locID, bool isFemale)
	{
		if (!isFemale)
		{
			return Instance.m_maleLocDict.ContainsKey(locID);
		}
		return Instance.m_femaleLocDict.ContainsKey(locID);
	}

	public static string GetString(string locID, bool isFemale, out bool isFormatString, bool disableWordScramble = false)
	{
		isFormatString = false;
		string @string = GetString(locID, isFemale, disableWordScramble);
		if (@string.Contains('{') && @string.Contains('}'))
		{
			isFormatString = true;
		}
		return @string;
	}

	public static string GetString(string locID, bool isFemale, bool disableWordScramble = false)
	{
		if (string.IsNullOrWhiteSpace(locID))
		{
			return "";
		}
		string value = "";
		Dictionary<string, string> dictionary = Instance.m_maleLocDict;
		if (isFemale)
		{
			dictionary = Instance.m_femaleLocDict;
		}
		if (!dictionary.TryGetValue(locID, out value))
		{
			return locID;
		}
		value = value.Replace("</LB>", "\n");
		int num = value.IndexOf('\n');
		while (num != -1 && num < value.Length - 1)
		{
			int i;
			for (i = 0; num + i + 1 < value.Length && value[num + i + 1] == ' '; i++)
			{
			}
			if (i > 0)
			{
				value = value.Remove(num + 1, i);
			}
			num = value.IndexOf('\n', num + 1);
		}
		int num2 = value.IndexOf('\r');
		while (num2 != -1 && num2 < value.Length - 1)
		{
			int j;
			for (j = 0; num2 + j + 1 < value.Length && value[num2 + j + 1] == ' '; j++)
			{
			}
			if (j > 0)
			{
				value = value.Remove(num2, j + 1);
			}
			num2 = value.IndexOf('\r');
		}
		return value;
	}

	public static string GetLocalizedPlayerName(CharacterData charData)
	{
		bool isFemale = charData.IsFemale;
		string arg = charData.Name;
		string arg2 = GetString("LOC_ID_ROYAL_TITLE_GENDER_1", isFemale);
		if ((charData.TraitOne == TraitType.Disposition || charData.TraitTwo == TraitType.Disposition) && charData.Disposition_ID == 2)
		{
			arg2 = "";
		}
		string text = string.Format(GetString("LOC_ID_FORMATTER_TITLE_1", isFemale), arg2, arg);
		if (charData.DuplicateNameCount > 0)
		{
			text = text + " " + CDGHelper.ToRoman(charData.DuplicateNameCount + 1);
		}
		return text;
	}

	public static void ChangeLanguage(LanguageType languageType, bool broadcastEvent)
	{
		Instance.ChangeLanguage_Internal(languageType, broadcastEvent);
	}

	public static void ForceRefreshAllTextGlyphs()
	{
		TextGlyphConverter[] array = UnityEngine.Object.FindObjectsOfType<TextGlyphConverter>();
		foreach (TextGlyphConverter textGlyphConverter in array)
		{
			if ((bool)textGlyphConverter && textGlyphConverter.IsInitialized)
			{
				textGlyphConverter.UpdateText(forceUpdate: true);
			}
		}
	}

	public static string GetFormatterGenderForcedString(string formatter, out bool isFemale)
	{
		isFemale = false;
		if (formatter.Contains("[M]"))
		{
			isFemale = false;
			formatter = formatter.Replace("[M]", "");
			formatter = formatter.Trim();
		}
		else if (formatter.Contains("[F]"))
		{
			isFemale = true;
			formatter = formatter.Replace("[F]", "");
			formatter = formatter.Trim();
		}
		return formatter;
	}
}
