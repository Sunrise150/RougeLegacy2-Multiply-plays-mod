using System;

[Serializable]
public class ConfigSaveData : IVersionUpdateable
{
	public int REVISION_NUMBER = 1;

	public byte CurrentProfile;

	public float MusicVolume = 1f;

	public float SFXVolume = 1f;

	public float MasterVolume = 1f;

	public float DeadZone = 0.25f;

	public int AimFidelity = 2;

	public int QualitySetting = 2;

	public bool EnableVsync = true;

	public int PrimaryDisplay;

	public int ScreenWidth = -1;

	public int ScreenHeight = -1;

	public int ScreenMode = 1;

	public bool Disable_16_9;

	public bool DisableCursorConfine;

	public LanguageType Language;

	public string UserReportName;

	public string UserReportEmail;

	public bool UseNonScientificNames;

	public bool EnableDualButtonDash;

	public bool EnableQuickDrop;

	public bool ToggleMouseAiming = true;

	public bool DisableSlowdownOnHit;

	public bool DisablePressDownSpinKick;

	public bool DisableReloadInteractButton;

	public bool DisableHUDFadeOut;

	public bool ToggleMouseAttackFlip;

	public bool DisableRumble;

	public InputIconSetting InputIconSetting;

	public void UpdateVersion()
	{
		if (REVISION_NUMBER <= 0 && DeadZone < 0.25f)
		{
			DeadZone = 0.25f;
		}
		REVISION_NUMBER = 1;
	}
}
