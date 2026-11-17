public abstract class ExecuteImmediateOptionItem : BaseOptionItem
{
	public override OptionsControlType OptionsControlType => OptionsControlType.ExecuteImmediate;

	public override bool PressAndHoldEnabled => false;

	public override void ConfirmOptionChange()
	{
	}

	public override void CancelOptionChange()
	{
	}

	public override void InvokeDecrement()
	{
	}

	public override void InvokeIncrement()
	{
	}

	public override void ActivateOption()
	{
		if (base.OptionItemActivated != null)
		{
			base.OptionItemActivated(this);
		}
	}

	public override void DeactivateOption(bool confirmOptionChange)
	{
	}
}
