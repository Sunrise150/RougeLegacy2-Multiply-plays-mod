using System;

public class ChangeDeadZoneOptionItem : IncrementDecrementOptionItem
{
	public override void Initialize()
	{
		m_minValue = 0f;
		m_maxValue = 1f;
		m_numberOfIncrements = 100;
		base.Initialize();
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		m_currentIncrementValue = SaveManager.ConfigData.DeadZone;
		UpdateIncrementBar();
	}

	protected override void Increment()
	{
	}

	protected override void Decrement()
	{
	}

	public override void ConfirmOptionChange()
	{
		SaveManager.ConfigData.DeadZone = m_currentIncrementValue;
		RewiredOnStartupController.UpdateJoystickCalibrationMap();
	}

	public override void RefreshText(object sender, EventArgs args)
	{
		m_currentIncrementValue = SaveManager.ConfigData.DeadZone;
		UpdateIncrementBar();
	}
}
