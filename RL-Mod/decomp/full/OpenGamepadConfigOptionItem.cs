using Rewired;

public class OpenGamepadConfigOptionItem : OpenSuboptionOptionItem
{
	public bool IsAvailable => Rewired_RL.IsGamepadConnected;

	protected override void OnEnable()
	{
		base.OnEnable();
		UpdateTextAlpha();
		ReInput.ControllerConnectedEvent += OnControllerConnnectionChanged;
		ReInput.ControllerDisconnectedEvent += OnControllerConnnectionChanged;
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		ReInput.ControllerConnectedEvent -= OnControllerConnnectionChanged;
		ReInput.ControllerDisconnectedEvent -= OnControllerConnnectionChanged;
	}

	private void OnControllerConnnectionChanged(ControllerStatusChangedEventArgs args)
	{
		UpdateTextAlpha();
	}

	private void UpdateTextAlpha()
	{
		if (!IsAvailable)
		{
			m_incrementValueText.alpha = 0.5f;
		}
		else
		{
			m_incrementValueText.alpha = 1f;
		}
	}

	public override void ActivateOption()
	{
		if (IsAvailable)
		{
			base.ActivateOption();
		}
	}
}
