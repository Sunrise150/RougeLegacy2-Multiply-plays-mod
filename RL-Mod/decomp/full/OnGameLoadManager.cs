using System.Globalization;
using System.IO;
using UnityEngine;

public class OnGameLoadManager
{
	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSplashScreen)]
	private static void Run()
	{
		LanguageType languageType = GetLanguageType(CultureInfo.CurrentCulture);
		Debug.Log("DEFAULT LANGUAGE: " + languageType);
		CultureInfo obj = new CultureInfo("en-US", useUserOverride: false)
		{
			NumberFormat = 
			{
				CurrencyDecimalSeparator = "."
			}
		};
		CultureInfo.CurrentCulture = obj;
		SaveManager.SetCultureInfo(obj);
		SaveManager.ConfigData.Language = languageType;
		SaveFileSystem.Initialize();
		SaveFileSystem.MountSaveDirectory();
		Time.fixedDeltaTime = 1f / 60f;
		Time.maximumDeltaTime = 1f / 30f;
		Debug.Log("<color=yellow>Setting FixedDeltaTime to " + 1f / 60f + ".</color>");
		Application.targetFrameRate = 300;
		Debug.Log("<color=yellow>Setting target FPS to " + 300 + ".</color>");
		if (File.Exists(SaveManager.GetConfigPath()))
		{
			SaveManager.LoadConfigFile();
		}
		if (Application.isEditor)
		{
			GameResolutionManager.SetResolution(Screen.width, Screen.height, FullScreenMode.Windowed, saveConfig: false);
			return;
		}
		SetScreenModeAndResolution();
		GameResolutionManager.SetVsyncEnable(SaveManager.ConfigData.EnableVsync);
		Application.focusChanged -= OnFocusChanged;
		Application.focusChanged += OnFocusChanged;
		ConfineMouseToGameWindow(!SaveManager.ConfigData.DisableCursorConfine);
	}

	private static LanguageType GetLanguageType(CultureInfo cultureInfo)
	{
		if (cultureInfo.TwoLetterISOLanguageName == "en")
		{
			return LanguageType.English;
		}
		if (cultureInfo.TwoLetterISOLanguageName == "fr")
		{
			return LanguageType.French;
		}
		if (cultureInfo.TwoLetterISOLanguageName == "de")
		{
			return LanguageType.German;
		}
		if (cultureInfo.TwoLetterISOLanguageName == "ru")
		{
			return LanguageType.Russian;
		}
		if (cultureInfo.TwoLetterISOLanguageName == "es")
		{
			return LanguageType.Spanish;
		}
		if (cultureInfo.Name == "pt-BR")
		{
			return LanguageType.Portuguese_BR;
		}
		if (cultureInfo.Name == "zh-Hans")
		{
			return LanguageType.Chinese_Simp;
		}
		if (cultureInfo.TwoLetterISOLanguageName == "ko")
		{
			return LanguageType.Korean;
		}
		if (cultureInfo.TwoLetterISOLanguageName == "tr")
		{
			return LanguageType.Turkish;
		}
		if (cultureInfo.Name == "zh-Hant")
		{
			return LanguageType.Chinese_Trad;
		}
		if (cultureInfo.TwoLetterISOLanguageName == "it")
		{
			return LanguageType.Italian;
		}
		return LanguageType.English;
	}

	private static void OnFocusChanged(bool isFocused)
	{
		if (isFocused)
		{
			ConfineMouseToGameWindow(!SaveManager.ConfigData.DisableCursorConfine);
		}
		else
		{
			ConfineMouseToGameWindow(isConfined: false);
		}
	}

	public static void ConfineMouseToGameWindow(bool isConfined)
	{
		if (GameManager.IsGameManagerInstantiated && GameManager.IsGamePaused)
		{
			isConfined = false;
		}
		if (isConfined)
		{
			Cursor.lockState = CursorLockMode.Confined;
		}
		else
		{
			Cursor.lockState = CursorLockMode.None;
		}
	}

	private static void SetScreenModeAndResolution()
	{
		int screenWidth = SaveManager.ConfigData.ScreenWidth;
		int screenHeight = SaveManager.ConfigData.ScreenHeight;
		bool flag = false;
		Resolution[] resolutions = Screen.resolutions;
		for (int i = 0; i < resolutions.Length; i++)
		{
			if (resolutions[i].width == screenWidth && resolutions[i].height == screenHeight)
			{
				flag = true;
				break;
			}
		}
		if (!flag)
		{
			Resolution currentResolution = Screen.currentResolution;
			Debug.LogFormat("Resolution Config is requesting an unsupported resolution {0}x{1}, forcing full-res: {2}x{3}", SaveManager.ConfigData.ScreenWidth, SaveManager.ConfigData.ScreenHeight, currentResolution.width, currentResolution.height);
			SaveManager.ConfigData.ScreenWidth = currentResolution.width;
			SaveManager.ConfigData.ScreenHeight = currentResolution.height;
			screenWidth = SaveManager.ConfigData.ScreenWidth;
			screenHeight = SaveManager.ConfigData.ScreenHeight;
		}
		FullScreenMode screenMode = (FullScreenMode)SaveManager.ConfigData.ScreenMode;
		GameResolutionManager.SetResolution(screenWidth, screenHeight, screenMode, saveConfig: false);
	}
}
