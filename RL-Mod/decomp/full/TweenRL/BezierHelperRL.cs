using System;
using System.Collections.Generic;
using UnityEngine;

namespace TweenRL;

public class BezierHelperRL
{
	private static double[] FactorialLookup;

	private static double Factorial(int n)
	{
		if (FactorialLookup == null)
		{
			CreateFactorialTable();
		}
		if (n < 0)
		{
			throw new Exception("n is less than 0");
		}
		if (n > 32)
		{
			throw new Exception("n is greater than 32");
		}
		return FactorialLookup[n];
	}

	private static void CreateFactorialTable()
	{
		FactorialLookup = new double[33]
		{
			1.0, 1.0, 2.0, 6.0, 24.0, 120.0, 720.0, 5040.0, 40320.0, 362880.0,
			3628800.0, 39916800.0, 479001600.0, 6227020800.0, 87178291200.0, 1307674368000.0, 20922789888000.0, 355687428096000.0, 6402373705728000.0, 1.21645100408832E+17,
			2.43290200817664E+18, 5.109094217170944E+19, 1.1240007277776077E+21, 2.585201673888498E+22, 6.204484017332394E+23, 1.5511210043330986E+25, 4.0329146112660565E+26, 1.0888869450418352E+28, 3.0488834461171387E+29, 8.841761993739702E+30,
			2.6525285981219107E+32, 8.222838654177922E+33, 2.631308369336935E+35
		};
	}

	private static double Ni(int n, int i)
	{
		double num = Factorial(n);
		double num2 = Factorial(i);
		double num3 = Factorial(n - i);
		return num / (num2 * num3);
	}

	private static float Bernstein(int n, int i, float t)
	{
		double num = (((double)t != 0.0 || i != 0) ? Math.Pow(t, i) : 1.0);
		double num2 = ((n != i || (double)t != 1.0) ? Math.Pow(1f - t, n - i) : 1.0);
		return (float)(Ni(n, i) * num * num2);
	}

	public static Vector2 GetBezierPt(Vector2[] b, float elapsedTime)
	{
		int num = b.Length;
		float num2 = 0f;
		float num3 = 0f;
		for (int i = 0; i != num; i++)
		{
			float num4 = Bernstein(num - 1, i, elapsedTime);
			num2 += num4 * b[i].x;
			num3 += num4 * b[i].y;
		}
		return new Vector2(num2, num3);
	}

	public static Vector2 GetBezierPt(List<Vector2> b, float elapsedTime)
	{
		int count = b.Count;
		float num = 0f;
		float num2 = 0f;
		for (int i = 0; i != count; i++)
		{
			float num3 = Bernstein(count - 1, i, elapsedTime);
			num += num3 * b[i].x;
			num2 += num3 * b[i].y;
		}
		return new Vector2(num, num2);
	}

	public static Vector2 GetBezierPt(float[] b, float elapsedTime)
	{
		int num = b.Length / 2;
		int num2 = 0;
		float num3 = 0f;
		float num4 = 0f;
		for (int i = 0; i != num; i++)
		{
			float num5 = Bernstein(num - 1, i, elapsedTime);
			num3 += num5 * b[num2];
			num4 += num5 * b[num2 + 1];
			num2 += 2;
		}
		return new Vector2(num3, num4);
	}
}
