using System;

public class EnchantressOmniUIDescriptionEventArgs : EventArgs
{
	public RuneType RuneType { get; private set; }

	public OmniUIButtonType ButtonType { get; private set; }

	public EnchantressOmniUIDescriptionEventArgs(RuneType runeType, OmniUIButtonType buttonType)
	{
		Initialize(runeType, buttonType);
	}

	public void Initialize(RuneType runeType, OmniUIButtonType buttonType)
	{
		RuneType = runeType;
		ButtonType = buttonType;
	}
}
