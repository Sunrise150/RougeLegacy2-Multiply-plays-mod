using UnityEngine;

public static class AssistMode_EV
{
	public const int ENEMY_DAMAGE_SCALE_AMOUNT = 5;

	public const int ENEMY_DAMAGE_MIN_AMOUNT = 50;

	public const int ENEMY_DAMAGE_MAX_AMOUNT = 200;

	public const int ENEMY_HEALTH_SCALE_AMOUNT = 5;

	public const int ENEMY_HEALTH_MIN_AMOUNT = 50;

	public const int ENEMY_HEALTH_MAX_AMOUNT = 200;

	public const int AIM_TIME_SLOW_SCALE_AMOUNT = 5;

	public const int AIM_TIME_SLOW_MIN_AMOUNT = 25;

	public const int AIM_TIME_SLOW_MAX_AMOUNT = 100;

	public const int BURDEN_REQUIREMENT_SCALE_AMOUNT = 50;

	public const int BURDEN_REQUIREMENT_MIN_AMOUNT = 50;

	public const int BURDEN_REQUIREMENT_MAX_AMOUNT = 200;

	public const bool BURDEN_REQUIREMENT_MODFIES_MAX_BURDENS = true;

	public const int DIFFICULTY_INITIAL_VALUE = 5000;

	public const int DIFFICULTY_ENEMY_DAMAGE_MOD = 10;

	public const int DIFFICULTY_ENEMY_HEALTH_MOD = 10;

	public const int DIFFICULTY_BURDEN_REQUIREMENT_MOD = 5;

	public const int DIFFICULTY_AIM_TIME_SLOW_MOD = 50;

	public const int DIFFICULTY_TOGGLE_FLIGHT_ADD = 400;

	public const int DIFFICULTY_TOGGLE_NO_COLLISION_ADD = 1000;

	public static int GetDifficulty()
	{
		int num = 5000;
		int num2 = 100 - Mathf.RoundToInt(SaveManager.PlayerSaveData.Assist_EnemyDamageMod * 100f);
		num -= num2 * 10;
		int num3 = 100 - Mathf.RoundToInt(SaveManager.PlayerSaveData.Assist_EnemyHealthMod * 100f);
		num -= num3 * 10;
		int num4 = 100 - Mathf.RoundToInt(SaveManager.PlayerSaveData.Assist_AimTimeSlow * 100f);
		num -= num4 * 50;
		int num5 = 100 - Mathf.RoundToInt(SaveManager.PlayerSaveData.Assist_BurdenRequirementsMod * 100f);
		num -= num5 * 5;
		if (SaveManager.PlayerSaveData.Assist_EnableFlightToggle)
		{
			num -= 400;
		}
		if (SaveManager.PlayerSaveData.Assist_DisableEnemyContactDamage)
		{
			num -= 1000;
		}
		return num;
	}
}
