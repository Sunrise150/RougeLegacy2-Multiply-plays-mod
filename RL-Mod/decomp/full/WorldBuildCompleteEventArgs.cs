using System;
using System.Collections.Generic;

public class WorldBuildCompleteEventArgs : EventArgs
{
	public int MasterSeed { get; }

	public int BiomeCreationSeed { get; private set; }

	public List<BiomeController> BiomeControllers { get; private set; }

	public bool IsSuccess { get; }

	public WorldBuildCompleteEventArgs(int masterSeed, int seed, List<BiomeController> biomeControllers, bool isSuccess)
	{
		MasterSeed = masterSeed;
		BiomeCreationSeed = seed;
		BiomeControllers = biomeControllers;
		IsSuccess = isSuccess;
	}
}
