using System;

namespace TweenRL;

public class Expo
{
	public static float EaseIn(float t, float b, float c, float d)
	{
		if (t != 0f)
		{
			return (float)((double)c * Math.Pow(2.0, 10f * (t / d - 1f)) + (double)b);
		}
		return b;
	}

	public static float EaseOut(float t, float b, float c, float d)
	{
		if (t != d)
		{
			return (float)((double)c * (0.0 - Math.Pow(2.0, -10f * t / d) + 1.0) + (double)b);
		}
		return b + c;
	}

	public static float EaseInOut(float t, float b, float c, float d)
	{
		if (t == 0f)
		{
			return b;
		}
		if (t == d)
		{
			return b + c;
		}
		if ((t /= d / 2f) < 1f)
		{
			return (float)((double)(c / 2f) * Math.Pow(2.0, 10f * (t - 1f)) + (double)b);
		}
		return (float)((double)(c / 2f) * (0.0 - Math.Pow(2.0, -10f * (t -= 1f)) + 2.0) + (double)b);
	}
}
