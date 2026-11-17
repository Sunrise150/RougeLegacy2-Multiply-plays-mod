using UnityEngine;

public class DisableTraitsOptionItem : SelectionListOptionItem
{
	protected override void OnEnable()
	{
		base.OnEnable();
		if (base.IsInitialized)
		{
			m_selectedIndex = ((!SaveManager.PlayerSaveData.Assist_DisableTraits) ? 1 : 0);
			m_incrementValueText.SetText(base.CurrentSelectionString);
		}
	}

	public override void Initialize()
	{
		m_selectionLocIDArray = new string[2] { "LOC_ID_GENERAL_UI_ON_1", "LOC_ID_GENERAL_UI_OFF_1" };
		m_selectedIndex = ((!SaveManager.PlayerSaveData.Assist_DisableTraits) ? 1 : 0);
		base.Initialize();
	}

	public override void InvokeValueChange()
	{
		Debug.Log("Changed Disable Traits to: " + base.CurrentSelectionString);
	}

	public override void ConfirmOptionChange()
	{
		bool assist_DisableTraits = m_selectedIndex == 0;
		SaveManager.PlayerSaveData.Assist_DisableTraits = assist_DisableTraits;
		Messenger<GameMessenger, GameEvent>.Broadcast(GameEvent.HouseRulesChanged, null, null);
	}

	private void Update()
	{
		if (!SaveManager.PlayerSaveData.EnableHouseRules)
		{
			m_titleText.alpha = 0.5f;
			m_incrementValueText.alpha = 0.5f;
		}
		else
		{
			m_titleText.alpha = 1f;
			m_incrementValueText.alpha = 1f;
		}
	}

	public override void ActivateOption()
	{
		if (SaveManager.PlayerSaveData.EnableHouseRules)
		{
			base.ActivateOption();
		}
	}
}
