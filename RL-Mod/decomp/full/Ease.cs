using System;

public class Ease
{
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

	public class Circ
	{
		public static float EaseIn(float t, float b, float c, float d)
		{
			return (0f - c) * (float)(Math.Sqrt(1f - (t /= d) * t) - 1.0) + b;
		}

		public static float EaseOut(float t, float b, float c, float d)
		{
			return c * (float)Math.Sqrt(1f - (t = t / d - 1f) * t) + b;
		}

		public static float EaseInOut(float t, float b, float c, float d)
		{
			if ((t /= d / 2f) < 1f)
			{
				return (0f - c) / 2f * (float)(Math.Sqrt(1f - t * t) - 1.0) + b;
			}
			return c / 2f * (float)(Math.Sqrt(1f - (t -= 2f) * t) + 1.0) + b;
		}
	}

	public class Cubic
	{
		public static float EaseIn(float t, float b, float c, float d)
		{
			return c * (t /= d) * t * t + b;
		}

		public static float EaseOut(float t, float b, float c, float d)
		{
			return c * ((t = t / d - 1f) * t * t + 1f) + b;
		}

		public static float EaseInOut(float t, float b, float c, float d)
		{
			if ((t /= d / 2f) < 1f)
			{
				return c / 2f * t * t * t + b;
			}
			return c / 2f * ((t -= 2f) * t * t + 2f) + b;
		}
	}

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

	public class Linear
	{
		public static float EaseNone(float t, float b, float c, float d)
		{
			return c * t / d + b;
		}

		public static float EaseIn(float t, float b, float c, float d)
		{
			return c * t / d + b;
		}

		public static float EaseOut(float t, float b, float c, float d)
		{
			return c * t / d + b;
		}

		public static float EaseInOut(float t, float b, float c, float d)
		{
			return c * t / d + b;
		}
	}

	public class Quad
	{
		public static float EaseIn(float t, float b, float c, float d)
		{
			return c * (t /= d) * t + b;
		}

		public static float EaseOut(float t, float b, float c, float d)
		{
			return (0f - c) * (t /= d) * (t - 2f) + b;
		}

		public static float EaseInOut(float t, float b, float c, float d)
		{
			if ((t /= d / 2f) < 1f)
			{
				return c / 2f * t * t + b;
			}
			return (0f - c) / 2f * ((t -= 1f) * (t - 2f) - 1f) + b;
		}
	}

	public class Quart
	{
		public static float EaseIn(float t, float b, float c, float d)
		{
			return c * (t /= d) * t * t * t + b;
		}

		public static float EaseOut(float t, float b, float c, float d)
		{
			return (0f - c) * ((t = t / d - 1f) * t * t * t - 1f) + b;
		}

		public static float EaseInOut(float t, float b, float c, float d)
		{
			if ((t /= d / 2f) < 1f)
			{
				return c / 2f * t * t * t * t + b;
			}
			return (0f - c) / 2f * ((t -= 2f) * t * t * t - 2f) + b;
		}
	}

	public class Quint
	{
		public static float EaseIn(float t, float b, float c, float d)
		{
			return c * (t /= d) * t * t * t * t + b;
		}

		public static float EaseOut(float t, float b, float c, float d)
		{
			return c * ((t = t / d - 1f) * t * t * t * t + 1f) + b;
		}

		public static float EaseInOut(float t, float b, float c, float d)
		{
			if ((t /= d / 2f) < 1f)
			{
				return c / 2f * t * t * t * t * t + b;
			}
			return c / 2f * ((t -= 2f) * t * t * t * t + 2f) + b;
		}
	}

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

	public static float None(float t, float b, float c, float d)
	{
		return c * t / d + b;
	}

	public static EaseDelegate GetEase(EaseType easeType, EaseInOutType inOutType)
	{
		return easeType switch
		{
			EaseType.None => None, 
			EaseType.Back => inOutType switch
			{
				EaseInOutType.EaseIn => Back.EaseIn, 
				EaseInOutType.EaseOut => Back.EaseOut, 
				_ => Back.EaseInOut, 
			}, 
			EaseType.BackSmall => inOutType switch
			{
				EaseInOutType.EaseIn => Back.EaseInSmall, 
				EaseInOutType.EaseOut => Back.EaseOutSmall, 
				_ => Back.EaseInOutSmall, 
			}, 
			EaseType.BackLarge => inOutType switch
			{
				EaseInOutType.EaseIn => Back.EaseInLarge, 
				EaseInOutType.EaseOut => Back.EaseOutLarge, 
				_ => Back.EaseInOutLarge, 
			}, 
			EaseType.Bounce => inOutType switch
			{
				EaseInOutType.EaseIn => Bounce.EaseIn, 
				EaseInOutType.EaseOut => Bounce.EaseOut, 
				_ => Bounce.EaseInOut, 
			}, 
			EaseType.Circ => inOutType switch
			{
				EaseInOutType.EaseIn => Circ.EaseIn, 
				EaseInOutType.EaseOut => Circ.EaseOut, 
				_ => Circ.EaseInOut, 
			}, 
			EaseType.Cubic => inOutType switch
			{
				EaseInOutType.EaseIn => Cubic.EaseIn, 
				EaseInOutType.EaseOut => Cubic.EaseOut, 
				_ => Cubic.EaseInOut, 
			}, 
			EaseType.Elastic => inOutType switch
			{
				EaseInOutType.EaseIn => Elastic.EaseIn, 
				EaseInOutType.EaseOut => Elastic.EaseOut, 
				_ => Elastic.EaseInOut, 
			}, 
			EaseType.Expo => inOutType switch
			{
				EaseInOutType.EaseIn => Expo.EaseIn, 
				EaseInOutType.EaseOut => Expo.EaseOut, 
				_ => Expo.EaseInOut, 
			}, 
			EaseType.Quad => inOutType switch
			{
				EaseInOutType.EaseIn => Quad.EaseIn, 
				EaseInOutType.EaseOut => Quad.EaseOut, 
				_ => Quad.EaseInOut, 
			}, 
			EaseType.Quart => inOutType switch
			{
				EaseInOutType.EaseIn => Quart.EaseIn, 
				EaseInOutType.EaseOut => Quart.EaseOut, 
				_ => Quart.EaseInOut, 
			}, 
			EaseType.Quint => inOutType switch
			{
				EaseInOutType.EaseIn => Quint.EaseIn, 
				EaseInOutType.EaseOut => Quint.EaseOut, 
				_ => Quint.EaseInOut, 
			}, 
			EaseType.Sine => inOutType switch
			{
				EaseInOutType.EaseIn => Sine.EaseIn, 
				EaseInOutType.EaseOut => Sine.EaseOut, 
				_ => Sine.EaseInOut, 
			}, 
			_ => inOutType switch
			{
				EaseInOutType.EaseIn => Linear.EaseIn, 
				EaseInOutType.EaseOut => Linear.EaseOut, 
				_ => Linear.EaseInOut, 
			}, 
		};
	}
}
