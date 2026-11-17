using System;

public class TraitChangedEventArgs : EventArgs
{
	public TraitType TraitOne { get; private set; }

	public TraitType TraitTwo { get; private set; }

	public TraitChangedEventArgs(TraitType traitOne, TraitType traitTwo)
	{
		Initialize(traitOne, traitTwo);
	}

	public void Initialize(TraitType traitOne, TraitType traitTwo)
	{
		TraitOne = traitOne;
		TraitTwo = traitTwo;
	}
}
