using System;

public interface IPropsWindow
{
	BiomeLayer BiomeLayerMask { get; }

	bool ShowPropTableIndicators { get; }

	event EventHandler<EventArgs> BiomeLayerChangeEvent;

	event EventHandler<EventArgs> CloseEvent;
}
