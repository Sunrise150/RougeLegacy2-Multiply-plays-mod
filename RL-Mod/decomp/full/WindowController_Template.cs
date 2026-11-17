using RL_Windows;

public class WindowController_Template : WindowController
{
	public override WindowID ID => WindowID.Map;

	protected override void OnOpen()
	{
		m_windowCanvas.gameObject.SetActive(value: true);
	}

	protected override void OnClose()
	{
		m_windowCanvas.gameObject.SetActive(value: false);
	}

	protected override void OnFocus()
	{
	}

	protected override void OnLostFocus()
	{
	}
}
