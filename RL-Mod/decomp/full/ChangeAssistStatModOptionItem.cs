using UnityEngine;

public class ChangeAssistStatModOptionItem : SelectionListOptionItem
{
	private enum StatType
	{
		EnemyHealth,
		EnemyDamage,
		AimTimeSlow,
		BurdenRequirement
	}

	[SerializeField]
	private StatType m_statType;

	private int m_minValue;

	private int m_maxValue;

	private int m_incrementValue;

	protected override bool UsesLocID => false;

	public override bool PressAndHoldEnabled => true;

	private int GetInitialIndex()
	{
		float num = 1f;
		switch (m_statType)
		{
		case StatType.EnemyHealth:
			num = SaveManager.PlayerSaveData.Assist_EnemyHealthMod;
			break;
		case StatType.EnemyDamage:
			num = SaveManager.PlayerSaveData.Assist_EnemyDamageMod;
			break;
		case StatType.AimTimeSlow:
			num = SaveManager.PlayerSaveData.Assist_AimTimeSlow;
			break;
		case StatType.BurdenRequirement:
			num = SaveManager.PlayerSaveData.Assist_BurdenRequirementsMod;
			break;
		}
		return (Mathf.RoundToInt(num * 100f) - m_minValue) / m_incrementValue;
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		if (base.IsInitialized)
		{
			m_selectedIndex = Mathf.Clamp(GetInitialIndex(), 0, m_selectionLocIDArray.Length - 1);
			m_incrementValueText.SetText(base.CurrentSelectionString);
		}
	}

	public override void Initialize()
	{
		switch (m_statType)
		{
		case StatType.EnemyHealth:
			m_minValue = 50;
			m_maxValue = 200;
			m_incrementValue = 5;
			break;
		case StatType.EnemyDamage:
			m_minValue = 50;
			m_maxValue = 200;
			m_incrementValue = 5;
			break;
		case StatType.AimTimeSlow:
			m_minValue = 25;
			m_maxValue = 100;
			m_incrementValue = 5;
			break;
		case StatType.BurdenRequirement:
			m_minValue = 50;
			m_maxValue = 200;
			m_incrementValue = 50;
			break;
		}
		int num = (m_maxValue - m_minValue) / m_incrementValue + 1;
		m_selectionLocIDArray = new string[num];
		for (int i = 0; i < num; i++)
		{
			int num2 = m_minValue + m_incrementValue * i;
			m_selectionLocIDArray[i] = string.Format(LocalizationManager.GetString("LOC_ID_GENERAL_UI_PERCENT_1", isFemale: false), num2);
		}
		base.Initialize();
	}

	public override void InvokeValueChange()
	{
		Debug.Log("Changed " + m_statType.ToString() + " to: " + m_selectionLocIDArray[m_selectedIndex]);
	}

	public override void ConfirmOptionChange()
	{
		float num = (float)(m_minValue + m_incrementValue * m_selectedIndex) / 100f;
		switch (m_statType)
		{
		case StatType.EnemyHealth:
			SaveManager.PlayerSaveData.Assist_EnemyHealthMod = num;
			break;
		case StatType.EnemyDamage:
			SaveManager.PlayerSaveData.Assist_EnemyDamageMod = num;
			break;
		case StatType.AimTimeSlow:
			SaveManager.PlayerSaveData.Assist_AimTimeSlow = num;
			break;
		case StatType.BurdenRequirement:
			SaveManager.PlayerSaveData.Assist_BurdenRequirementsMod = num;
			break;
		}
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
