using System;

public class GamepadType_RL
{
	private static GamepadType[] m_gamepadTypeArray;

	public static GamepadType[] GamepadTypeArray
	{
		get
		{
			if (m_gamepadTypeArray == null)
			{
				m_gamepadTypeArray = Enum.GetValues(typeof(GamepadType)) as GamepadType[];
			}
			return m_gamepadTypeArray;
		}
	}
}
