using System;

public class ItemCollectedEventArgs : EventArgs
{
	public BaseItemDrop Item { get; private set; }

	public ItemCollectedEventArgs(BaseItemDrop item)
	{
		Item = item;
	}

	public void Initialize(BaseItemDrop item)
	{
		Item = item;
	}
}
