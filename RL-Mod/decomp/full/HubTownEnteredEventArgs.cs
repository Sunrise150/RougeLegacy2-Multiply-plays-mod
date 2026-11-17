using System;

public class HubTownEnteredEventArgs : EventArgs
{
	public HubTownController HubTownController { get; private set; }

	public HubTownEnteredEventArgs(HubTownController hubTownController)
	{
		Initialize(hubTownController);
	}

	public void Initialize(HubTownController hubTownController)
	{
		HubTownController = HubTownController;
	}
}
