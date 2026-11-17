using System;
using UnityEngine;

[Serializable]
public struct SerializableVector2Int
{
	public int x;

	public int y;

	public SerializableVector2Int(int rX, int rY)
	{
		x = rX;
		y = rY;
	}

	public override string ToString()
	{
		return $"[{x}, {y}]";
	}

	public static implicit operator Vector2Int(SerializableVector2Int rValue)
	{
		return new Vector2Int(rValue.x, rValue.y);
	}

	public static implicit operator SerializableVector2Int(Vector2Int rValue)
	{
		return new SerializableVector2Int(rValue.x, rValue.y);
	}
}
