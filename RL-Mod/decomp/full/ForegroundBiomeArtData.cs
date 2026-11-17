using System;
using Foreground;
using UnityEngine;

[Serializable]
public class ForegroundBiomeArtData
{
	public ForegroundLibrary Upper;

	public ForegroundLibrary Lower;

	[Range(0f, 100f)]
	public int UpperSpawnOdds = 50;

	[Range(0f, 100f)]
	public int BottomSpawnOdds = 50;

	public Material ForegroundSpriteMaterial;
}
