using UnityEngine;

public class EnableHouseRulesOptionItem : SelectionListOptionItem
{
	protected override void OnEnable()
	{
		base.OnEnable();
		if (base.IsInitialized)
		{
			m_selectedIndex = (SaveManager.PlayerSaveData.EnableHouseRules ? 1 : 0);
			m_incrementValueText.SetText(base.CurrentSelectionString);
		}
	}

	public override void Initialize()
	{
		m_selectionLocIDArray = new string[2] { "LOC_ID_GENERAL_UI_OFF_1", "LOC_ID_GENERAL_UI_ON_1" };
		m_selectedIndex = (SaveManager.PlayerSaveData.EnableHouseRules ? 1 : 0);
		base.Initialize();
	}

	public override void InvokeValueChange()
	{
		Debug.Log("Changed House Rules to: " + base.CurrentSelectionString);
	}

	public override void ConfirmOptionChange()
	{
		bool flag = m_selectedIndex == 1;
		SaveManager.PlayerSaveData.EnableHouseRules = flag;
		Messenger<GameMessenger, GameEvent>.Broadcast(GameEvent.HouseRulesChanged, null, null);
		if (flag)
		{
			StoreAPIManager.GiveAchievement(AchievementType.EnableHouseRules, StoreType.All);
		}
		else if (PlayerManager.IsInstantiated && !PlayerManager.IsDisposed)
		{
			PlayerController playerController = PlayerManager.GetPlayerController();
			if (playerController.CharacterFlight.IsAssistFlying)
			{
				playerController.CharacterFlight.StopFlight();
			}
		}
	}
}
