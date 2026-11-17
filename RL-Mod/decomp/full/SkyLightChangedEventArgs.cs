using System;
using UnityEngine;

public class SkyLightChangedEventArgs : EventArgs
{
	public Color NewSkyLightColor { get; private set; }

	public float Lerp { get; private set; }

	public SkyLightChangedEventArgs(Color newSkyLightColor, float lerp)
	{
		Initialize(newSkyLightColor, lerp);
	}

	public void Initialize(Color newSkyLightColor, float lerp)
	{
		NewSkyLightColor = newSkyLightColor;
		Lerp = lerp;
	}
}
