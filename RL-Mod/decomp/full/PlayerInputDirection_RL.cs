using System;
using UnityEngine;

public static class PlayerInputDirection_RL
{
	private static Vector2Int CDG_8_DIRECTION_ANGLE_DOWN = new Vector2Int(254, 285);

	private static Vector2Int CDG_8_DIRECTION_ANGLE_DOWN_LEFT = new Vector2Int(210, 253);

	private static Vector2Int CDG_8_DIRECTION_ANGLE_LEFT = new Vector2Int(150, 209);

	private static Vector2Int CDG_8_DIRECTION_ANGLE_UP_LEFT = new Vector2Int(106, 149);

	private static Vector2Int CDG_8_DIRECTION_ANGLE_UP = new Vector2Int(74, 105);

	private static Vector2Int CDG_8_DIRECTION_ANGLE_UP_RIGHT = new Vector2Int(30, 73);

	private static Vector2Int CDG_8_DIRECTION_ANGLE_RIGHT = new Vector2Int(330, 29);

	private static Vector2Int CDG_8_DIRECTION_ANGLE_DOWN_RIGHT = new Vector2Int(286, 329);

	private static PlayerInputDirection[] m_typeArray;

	public static PlayerInputDirection[] TypeArray
	{
		get
		{
			if (m_typeArray == null)
			{
				m_typeArray = Enum.GetValues(typeof(PlayerInputDirection)) as PlayerInputDirection[];
			}
			return m_typeArray;
		}
	}

	public static Vector2Int GetInputDirectionVector(PlayerInputDirection direction)
	{
		return direction switch
		{
			PlayerInputDirection.DownLeft => CDG_8_DIRECTION_ANGLE_DOWN_LEFT, 
			PlayerInputDirection.Left => CDG_8_DIRECTION_ANGLE_LEFT, 
			PlayerInputDirection.UpLeft => CDG_8_DIRECTION_ANGLE_UP_LEFT, 
			PlayerInputDirection.Up => CDG_8_DIRECTION_ANGLE_UP, 
			PlayerInputDirection.UpRight => CDG_8_DIRECTION_ANGLE_UP_RIGHT, 
			PlayerInputDirection.Right => CDG_8_DIRECTION_ANGLE_RIGHT, 
			PlayerInputDirection.DownRight => CDG_8_DIRECTION_ANGLE_DOWN_RIGHT, 
			_ => CDG_8_DIRECTION_ANGLE_DOWN, 
		};
	}

	public static bool InputDirectionPressed(float degrees, PlayerInputDirection direction)
	{
		degrees = CDGHelper.WrapAngleDegrees(degrees, convertToHalfRadius: false);
		Vector2Int inputDirectionVector = GetInputDirectionVector(direction);
		if (inputDirectionVector.x > inputDirectionVector.y)
		{
			if (!(degrees >= (float)inputDirectionVector.x))
			{
				return degrees <= (float)inputDirectionVector.y;
			}
			return true;
		}
		if (degrees >= (float)inputDirectionVector.x)
		{
			return degrees <= (float)inputDirectionVector.y;
		}
		return false;
	}

	public static PlayerInputDirection GetInputDirection(float degrees)
	{
		PlayerInputDirection[] typeArray = TypeArray;
		foreach (PlayerInputDirection playerInputDirection in typeArray)
		{
			if (InputDirectionPressed(degrees, playerInputDirection))
			{
				return playerInputDirection;
			}
		}
		return PlayerInputDirection.Down;
	}
}
