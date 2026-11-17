using System;

public class GeneticistOmniUIDescriptionEventArgs : EventArgs
{
	public TraitType TraitType { get; private set; }

	public bool IsIncreasingOdds { get; private set; }

	public GeneticistOmniUIDescriptionEventArgs(TraitType traitType, bool isIncreasingOdds)
	{
		Initialize(traitType, isIncreasingOdds);
	}

	public void Initialize(TraitType traitType, bool isIncreasingOdds)
	{
		TraitType = traitType;
		IsIncreasingOdds = isIncreasingOdds;
	}
}
