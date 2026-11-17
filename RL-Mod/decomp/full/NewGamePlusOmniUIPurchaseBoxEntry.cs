using UnityEngine;

public class NewGamePlusOmniUIPurchaseBoxEntry : BaseOmniUIPurchaseBoxEntry<NewGamePlusOmniUIDescriptionEventArgs>
{
	public int CurrentNewGamePlusLevel => NewGamePlusOmniUIWindowController.CurrentNewGamePlusLevel;

	protected override void DisplayNullPurchaseBox()
	{
	}

	protected override void DisplayPurchaseBox(NewGamePlusOmniUIDescriptionEventArgs args)
	{
		OmniUIPurchaseBoxType descriptionType = m_descriptionType;
		if (descriptionType != OmniUIPurchaseBoxType.FlavourText && descriptionType == OmniUIPurchaseBoxType.MoneyCost)
		{
			int num = NewGamePlus_EV.GetBurdensRequiredForNG(CurrentNewGamePlusLevel);
			int num2 = 30;
			if (SaveManager.PlayerSaveData.EnableHouseRules)
			{
				num = Mathf.FloorToInt((float)num * SaveManager.PlayerSaveData.Assist_BurdenRequirementsMod);
				num2 = Mathf.FloorToInt((float)num2 * SaveManager.PlayerSaveData.Assist_BurdenRequirementsMod);
			}
			int max = Mathf.Min(BurdenManager.GetTotalBurdenMaxLevel(), num2);
			num = Mathf.Clamp(num, 0, max);
			num = Mathf.Clamp(num, 0, int.MaxValue);
			int totalBurdenWeight = BurdenManager.GetTotalBurdenWeight();
			string text = $"{totalBurdenWeight}/{num}";
			m_text1.text = text;
		}
	}
}
