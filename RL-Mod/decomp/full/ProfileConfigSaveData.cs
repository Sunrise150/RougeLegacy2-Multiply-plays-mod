using System;

[Serializable]
public class ProfileConfigSaveData : IVersionUpdateable
{
	public int REVISION_NUMBER;

	public bool UseNonScientificNames;

	public bool EnableDualButtonDash;

	public bool EnableQuickDrop;

	public bool ToggleMouseAiming = true;

	public bool DisableSlowdownOnHit;

	public bool DisablePressDownSpinKick;

	public void UpdateVersion()
	{
		if (REVISION_NUMBER == 0)
		{
			SaveManager.ConfigData.UseNonScientificNames = UseNonScientificNames;
			SaveManager.ConfigData.EnableDualButtonDash = EnableDualButtonDash;
			SaveManager.ConfigData.EnableQuickDrop = EnableQuickDrop;
			SaveManager.ConfigData.ToggleMouseAiming = ToggleMouseAiming;
			SaveManager.ConfigData.DisableSlowdownOnHit = DisableSlowdownOnHit;
			SaveManager.ConfigData.DisablePressDownSpinKick = DisablePressDownSpinKick;
			REVISION_NUMBER = 1;
			SaveManager.SaveConfigFile();
			SaveManager.SaveProfileConfigFile();
		}
		REVISION_NUMBER = 1;
	}
}
