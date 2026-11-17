using System;

public class NewGamePlusOmniUIDescriptionEventArgs : EventArgs
{
	public BurdenType BurdenType { get; private set; }

	public OmniUIButtonType ButtonType { get; private set; }

	public NewGamePlusOmniUIDescriptionEventArgs(BurdenType burdenType, OmniUIButtonType buttonType)
	{
		Initialize(burdenType, buttonType);
	}

	public void Initialize(BurdenType burdenType, OmniUIButtonType buttonType)
	{
		BurdenType = burdenType;
		ButtonType = buttonType;
	}
}
