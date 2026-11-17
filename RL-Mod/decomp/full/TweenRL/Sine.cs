using System;

namespace TweenRL;

public class Sine
{
	public static float EaseIn(float t, float b, float c, float d)
	{
		return (float)((double)(0f - c) * Math.Cos((double)(t / d) * (Math.PI / 2.0)) + (double)c + (double)b);
	}

	public static float EaseOut(float t, float b, float c, float d)
	{
		return (float)((double)c * Math.Sin((double)(t / d) * (Math.PI / 2.0)) + (double)b);
	}

	public static float EaseInOut(float t, float b, float c, float d)
	{
		return (float)((double)((0f - c) / 2f) * (Math.Cos(Math.PI * (double)t / (double)d) - 1.0) + (double)b);
	}
}
