using System;
using UnityEngine;

public class ChangeAimFidelityOptionItem : IncrementDecrementOptionItem
{
	public override void Initialize()
	{
		m_minValue = 1f;
		m_maxValue = 90f;
		m_numberOfIncrements = 90;
		base.Initialize();
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		m_currentIncrementValue = SaveManager.ConfigData.AimFidelity;
		UpdateIncrementBar();
	}

	protected override void Increment()
	{
	}

	protected override void Decrement()
	{
	}

	protected override void UpdateIncrementBar()
	{
		m_currentIncrementValue = Mathf.RoundToInt(m_currentIncrementValue);
		base.UpdateIncrementBar();
	}

	public override void ConfirmOptionChange()
	{
		SaveManager.ConfigData.AimFidelity = Mathf.RoundToInt(m_currentIncrementValue);
	}

	public override void RefreshText(object sender, EventArgs args)
	{
		m_currentIncrementValue = SaveManager.ConfigData.AimFidelity;
		UpdateIncrementBar();
	}
}
