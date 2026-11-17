namespace TweenRL;

public class Bounce
{
	public static float EaseOut(float t, float b, float c, float d)
	{
		if ((t /= d) < 0.36363637f)
		{
			return c * (7.5625f * t * t) + b;
		}
		if (t < 0.72727275f)
		{
			return c * (7.5625f * (t -= 0.54545456f) * t + 0.75f) + b;
		}
		if (t < 0.90909094f)
		{
			return c * (7.5625f * (t -= 0.8181818f) * t + 0.9375f) + b;
		}
		return c * (7.5625f * (t -= 21f / 22f) * t + 63f / 64f) + b;
	}

	public static float EaseIn(float t, float b, float c, float d)
	{
		return c - EaseOut(d - t, 0f, c, d) + b;
	}

	public static float EaseInOut(float t, float b, float c, float d)
	{
		if (t < d / 2f)
		{
			return EaseIn(t * 2f, 0f, c, d) * 0.5f + b;
		}
		return EaseOut(t * 2f - d, 0f, c, d) * 0.5f + c * 0.5f + b;
	}
}
