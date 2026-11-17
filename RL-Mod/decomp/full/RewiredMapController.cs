using System.Collections.Generic;
using Rewired;

public static class RewiredMapController
{
	private static bool m_shouldGameInputBeEnabled = false;

	private static GameInputMode m_currentGameInputMode = GameInputMode.Game;

	private static Dictionary<GameInputMode, Rewired_RL.MapCategoryType[]> m_gameModeCategoryDict = new Dictionary<GameInputMode, Rewired_RL.MapCategoryType[]>
	{
		{
			GameInputMode.Game,
			new Rewired_RL.MapCategoryType[2]
			{
				Rewired_RL.MapCategoryType.Action,
				Rewired_RL.MapCategoryType.ActionRemappable
			}
		},
		{
			GameInputMode.Window,
			new Rewired_RL.MapCategoryType[2]
			{
				Rewired_RL.MapCategoryType.Window,
				Rewired_RL.MapCategoryType.WindowRemappable
			}
		}
	};

	public static bool IsInCutscene { get; private set; }

	public static GameInputMode CurrentGameInputMode => m_currentGameInputMode;

	public static bool IsCurrentMapEnabled => IsMapEnabled(m_currentGameInputMode);

	private static Rewired_RL.MapCategoryType[] GetMapCategoryTypes(GameInputMode inputMode)
	{
		return m_gameModeCategoryDict[inputMode];
	}

	public static void SetMap(GameInputMode inputMode)
	{
		if (ReInput.isReady)
		{
			m_currentGameInputMode = inputMode;
			ReInput.players.GetPlayer(0).controllers.maps.SetAllMapsEnabled(state: false);
			SetMapEnabled(m_currentGameInputMode, enabled: true);
		}
	}

	public static void DisableAllMap()
	{
		if (ReInput.isReady)
		{
			ReInput.players.GetPlayer(0).controllers.maps.SetAllMapsEnabled(state: false);
		}
	}

	public static void SetMapEnabled(GameInputMode inputMode, bool enabled)
	{
		if (!ReInput.isReady)
		{
			return;
		}
		if (inputMode == GameInputMode.Game)
		{
			m_shouldGameInputBeEnabled = enabled;
		}
		if (inputMode != GameInputMode.Game || (inputMode == GameInputMode.Game && !IsInCutscene))
		{
			Rewired_RL.MapCategoryType[] mapCategoryTypes = GetMapCategoryTypes(inputMode);
			foreach (Rewired_RL.MapCategoryType categoryType in mapCategoryTypes)
			{
				ReInput.players.GetPlayer(0).controllers.maps.SetMapsEnabled(enabled, Rewired_RL.GetMapCategoryID(categoryType));
			}
		}
	}

	public static void SetIsInCutscene(bool inCutscene)
	{
		if (inCutscene)
		{
			Rewired_RL.MapCategoryType[] mapCategoryTypes = GetMapCategoryTypes(GameInputMode.Game);
			foreach (Rewired_RL.MapCategoryType categoryType in mapCategoryTypes)
			{
				ReInput.players.GetPlayer(0).controllers.maps.SetMapsEnabled(state: false, Rewired_RL.GetMapCategoryID(categoryType));
			}
		}
		else if (m_shouldGameInputBeEnabled)
		{
			Rewired_RL.MapCategoryType[] mapCategoryTypes = GetMapCategoryTypes(GameInputMode.Game);
			foreach (Rewired_RL.MapCategoryType categoryType2 in mapCategoryTypes)
			{
				ReInput.players.GetPlayer(0).controllers.maps.SetMapsEnabled(state: true, Rewired_RL.GetMapCategoryID(categoryType2));
			}
		}
		IsInCutscene = inCutscene;
	}

	public static void SetCurrentMapEnabled(bool enabled)
	{
		SetMapEnabled(m_currentGameInputMode, enabled);
	}

	public static void SetMouseEnabled(bool enabled)
	{
		ReInput.controllers.Mouse.enabled = enabled;
	}

	public static bool IsMapEnabled(GameInputMode inputMode)
	{
		if (!ReInput.isReady)
		{
			return false;
		}
		Rewired_RL.MapCategoryType categoryType = GetMapCategoryTypes(inputMode)[0];
		ControllerType controllerType = ((ReInput.players.GetPlayer(0).controllers.Keyboard == null) ? ControllerType.Joystick : ControllerType.Keyboard);
		return ReInput.players.GetPlayer(0).controllers.maps.GetMap(controllerType, 0, Rewired_RL.GetMapCategoryID(categoryType), 0).enabled;
	}
}
