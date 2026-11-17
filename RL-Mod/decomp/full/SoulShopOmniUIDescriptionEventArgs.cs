using System;

public class SoulShopOmniUIDescriptionEventArgs : EventArgs
{
	public SoulShopType SoulShopType { get; private set; }

	public OmniUIButtonType ButtonType { get; private set; }

	public SoulShopOmniUIDescriptionEventArgs(SoulShopType soulShopType, OmniUIButtonType buttonType)
	{
		Initialize(soulShopType, buttonType);
	}

	public void Initialize(SoulShopType soulShopType, OmniUIButtonType buttonType)
	{
		SoulShopType = soulShopType;
		ButtonType = buttonType;
	}
}
