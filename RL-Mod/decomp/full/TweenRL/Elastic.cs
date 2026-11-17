using System;

namespace TweenRL;

public class Elastic
{
	public static float EaseIn(float t, float b, float c, float d)
	{
		float num = 0f;
		float num2 = 0f;
		if (t == 0f)
		{
			return b;
		}
		if ((t /= d) == 1f)
		{
			return b + c;
		}
		if (num2 == 0f)
		{
			num2 = d * 0.3f;
		}
		float num3;
		if (num == 0f || num < Math.Abs(c))
		{
			num = c;
			num3 = num2 / 4f;
		}
		else
		{
			num3 = (float)((double)num2 / (Math.PI * 2.0) * Math.Asin(c / num));
		}
		return (float)(0.0 - (double)num * Math.Pow(2.0, 10f * (t -= 1f)) * Math.Sin((double)(t * d - num3) * (Math.PI * 2.0) / (double)num2) + (double)b);
	}

	public static float EaseOut(float t, float b, float c, float d)
	{
		float num = 0f;
		float num2 = 0f;
		if (t == 0f)
		{
			return b;
		}
		if ((t /= d) == 1f)
		{
			return b + c;
		}
		if (num2 == 0f)
		{
			num2 = d * 0.3f;
		}
		float num3;
		if (num == 0f || num < Math.Abs(c))
		{
			num = c;
			num3 = num2 / 4f;
		}
		else
		{
			num3 = (float)((double)num2 / (Math.PI * 2.0) * Math.Asin(c / num));
		}
		return (float)((double)num * Math.Pow(2.0, -10f * t) * Math.Sin((double)(t * d - num3) * (Math.PI * 2.0) / (double)num2) + (double)c + (double)b);
	}

	public static float EaseInOut(float t, float b, float c, float d)
	{
		float num = 0f;
		float num2 = 0f;
		if (t == 0f)
		{
			return b;
		}
		if ((t /= d / 2f) == 2f)
		{
			return b + c;
		}
		if (num2 == 0f)
		{
			num2 = d * 0.45000002f;
		}
		float num3;
		if (num == 0f || num < Math.Abs(c))
		{
			num = c;
			num3 = num2 / 4f;
		}
		else
		{
			num3 = (float)((double)num2 / (Math.PI * 2.0) * Math.Asin(c / num));
		}
		if (t < 1f)
		{
			return (float)(-0.5 * ((double)num * Math.Pow(2.0, 10f * (t -= 1f)) * Math.Sin((double)(t * d - num3) * (Math.PI * 2.0) / (double)num2)) + (double)b);
		}
		return (float)((double)num * Math.Pow(2.0, -10f * (t -= 1f)) * Math.Sin((double)(t * d - num3) * (Math.PI * 2.0) / (double)num2) * 0.5 + (double)c + (double)b);
	}
}
