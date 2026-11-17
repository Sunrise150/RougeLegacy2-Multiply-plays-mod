using System;
using Rewired;

public class JournalOmniUIReadButton : OmniUIButton, IJournalOmniUIButton
{
	private JournalOmniUIDescriptionEventArgs m_descriptionEventArgs;

	private Action<InputActionEventData> m_onCancelButtonPressed;

	public override bool IsButtonActive => true;

	public override EventArgs ButtonEventArgs => m_descriptionEventArgs;

	public int EntryIndex { get; set; }

	public int JournalIndex { get; set; }

	public JournalType JournalType { get; set; }

	public JournalOmniUIWindowController JournalWindowController { get; set; }

	protected override void Awake()
	{
		m_onCancelButtonPressed = OnCancelButtonPressed;
		base.Awake();
	}

	protected override void InitializeButtonEventArgs()
	{
		if (m_descriptionEventArgs == null)
		{
			m_descriptionEventArgs = new JournalOmniUIDescriptionEventArgs(EntryIndex, JournalWindowController.HighlightedCategory, JournalIndex, JournalType);
		}
		else
		{
			m_descriptionEventArgs.Initialize(EntryIndex, JournalWindowController.HighlightedCategory, JournalIndex, JournalType);
		}
	}

	public override void OnConfirmButtonPressed()
	{
		base.OnConfirmButtonPressed();
		JournalWindowController.CommonFields.DescriptionBoxRaycastBlocker.gameObject.SetActive(value: true);
		JournalWindowController.DescriptionBoxScrollInput.AssignButtonToScroll(Rewired_RL.WindowInputActionType.Window_Vertical);
		JournalWindowController.SetKeyboardEnabled(enable: false);
		JournalWindowController.ScrollArrow.SetActive(value: true);
		JournalWindowController.RewiredPlayer.AddInputEventDelegate(m_onCancelButtonPressed, UpdateLoopType.Update, InputActionEventType.ButtonJustPressed, "Window_Cancel");
		RunOnConfirmPressedAnimation();
	}

	private void OnCancelButtonPressed(InputActionEventData eventData)
	{
		JournalWindowController.CommonFields.DescriptionBoxRaycastBlocker.gameObject.SetActive(value: false);
		JournalWindowController.DescriptionBoxScrollInput.RemoveButtonToScroll(Rewired_RL.WindowInputActionType.Window_Vertical);
		JournalWindowController.SetKeyboardEnabled(enable: true);
		JournalWindowController.ScrollArrow.SetActive(value: false);
		JournalWindowController.RewiredPlayer.RemoveInputEventDelegate(m_onCancelButtonPressed, UpdateLoopType.Update, InputActionEventType.ButtonJustPressed, "Window_Cancel");
		if (JournalWindowController.SelectOptionEvent != null)
		{
			JournalWindowController.SelectOptionEvent.Invoke();
		}
		RunOnConfirmPressedAnimation();
	}

	public override void UpdateState()
	{
		if (!base.gameObject.activeSelf)
		{
			base.gameObject.SetActive(value: true);
		}
	}
}
