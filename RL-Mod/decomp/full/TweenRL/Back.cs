namespace TweenRL;

public class Back
{
	public static float EaseIn(float t, float b, float c, float d)
	{
		float num = 1.70158f;
		return c * (t /= d) * t * ((num + 1f) * t - num) + b;
	}

	public static float EaseInSmall(float t, float b, float c, float d)
	{
		float num = 0.85079f;
		return c * (t /= d) * t * ((num + 1f) * t - num) + b;
	}

	public static float EaseInLarge(float t, float b, float c, float d)
	{
		float num = 5.10474f;
		return c * (t /= d) * t * ((num + 1f) * t - num) + b;
	}

	public static float EaseOut(float t, float b, float c, float d)
	{
		float num = 1.70158f;
		return c * ((t = t / d - 1f) * t * ((num + 1f) * t + num) + 1f) + b;
	}

	public static float EaseOutSmall(float t, float b, float c, float d)
	{
		float num = 0.85079f;
		return c * ((t = t / d - 1f) * t * ((num + 1f) * t + num) + 1f) + b;
	}

	public static float EaseOutLarge(float t, float b, float c, float d)
	{
		float num = 5.10474f;
		return c * ((t = t / d - 1f) * t * ((num + 1f) * t + num) + 1f) + b;
	}

	public static float EaseInOut(float t, float b, float c, float d)
	{
		float num = 1.70158f;
		if ((t /= d / 2f) < 1f)
		{
			return c / 2f * (t * t * (((num *= 1.525f) + 1f) * t - num)) + b;
		}
		return c / 2f * ((t -= 2f) * t * (((num *= 1.525f) + 1f) * t + num) + 2f) + b;
	}

	public static float EaseInOutLarge(float t, float b, float c, float d)
	{
		float num = 5.10474f;
		if ((t /= d / 2f) < 1f)
		{
			return c / 2f * (t * t * (((num *= 1.525f) + 1f) * t - num)) + b;
		}
		return c / 2f * ((t -= 2f) * t * (((num *= 1.525f) + 1f) * t + num) + 2f) + b;
	}

	public static float EaseInOutSmall(float t, float b, float c, float d)
	{
		float num = 0.85079f;
		if ((t /= d / 2f) < 1f)
		{
			return c / 2f * (t * t * (((num *= 1.525f) + 1f) * t - num)) + b;
		}
		return c / 2f * ((t -= 2f) * t * (((num *= 1.525f) + 1f) * t + num) + 2f) + b;
	}
}
