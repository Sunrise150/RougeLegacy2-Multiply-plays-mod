using UnityEngine;

public class NewGamePlus_EV
{
	public const int BURDEN_COST_PER_NG = 2;

	public const int BURDEN_COST_PER_NG_SLOW_SCALING = 1;

	public const int BURDEN_SCALING_NG_TRIGGER = 10;

	public const int MAX_NG_PLUS = 100;

	public const int NEWGAMEPLUS_BURDEN_CAP = 30;

	public const int ROOM_LEVEL_GAIN_PER_NG = 44;

	public const bool NEWGAMEPLUS_TELEPORT_TO_HUBTOWN = false;

	public const float ENEMY_PROJECTILE_SPEED_TURN_MOD = 1.07f;

	public const float ENEMY_HEALTH_MOD_PER_NG = 0.01f;

	public const float ENEMY_DAMAGE_MOD_PER_NG = 0.007f;

	public static int GetBurdensRequiredForNG(int ngLevel)
	{
		int result = 0;
		if (ngLevel > 0)
		{
			result = 2 * Mathf.Min(10, ngLevel) + Mathf.Max(0, ngLevel - 10);
		}
		return result;
	}
}
