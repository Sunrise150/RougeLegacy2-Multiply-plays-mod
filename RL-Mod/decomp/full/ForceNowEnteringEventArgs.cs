using System;

public class ForceNowEnteringEventArgs : EventArgs
{
	public string LocID { get; private set; }

	public int RiskLevel { get; private set; }

	public ForceNowEnteringEventArgs(string locID, int riskLevel)
	{
		Initialize(locID, riskLevel);
	}

	public void Initialize(string locID, int riskLevel)
	{
		LocID = locID;
		RiskLevel = riskLevel;
	}
}
