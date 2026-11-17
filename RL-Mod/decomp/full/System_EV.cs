using System;

public static class System_EV
{
	public const float FIXED_DELTA_TIME = 1f / 60f;

	public const float MAXIMUM_DELTA_TIME = 1f / 30f;

	public const int TARGET_FRAME_RATE = 300;

	public const byte NUM_SAVE_BACKUPS = 5;

	public const float SAVE_RESTRICTION_DURATION = 10f;

	public const int NUM_PROFILES = 5;

	public const int MIN_NUMBER_VERTICAL_RAYS = 2;

	public const int MAX_NUMBER_VERTICAL_RAYS = 6;

	public const int MIN_NUMBER_HORIZONTAL_RAYS = 2;

	public const int MAX_NUMBER_HORIZONTAL_RAYS = 6;

	public const float DISTANCE_BETWEEN_VERTICAL_RAYS = 0.5f;

	public const float DISTANCE_BETWEEN_HORIZONTAL_RAYS = 0.5f;

	public const bool STICK_TO_SLOPES = true;

	public const float BELOW_RAYCAST_LENGTH = 2f;

	public const int MAX_SLOPE_ANGLE = 46;

	public const float TYPEWRITER_SPEED = 0.015f;

	public const float TYPEWRITER_LONG_SPEED = 0.1f;

	public const int MAJOR_VERSION = 1;

	public const int MINOR_VERSION = 0;

	public const int PATCH_VERSION = 0;

	public const string HOTPATCH_VERSION = "";

	public const string PLATFORM_VERSION = "steam";

	public const int PLAYER_SAVE_REVISION_NUMBER = 13;

	public const int EQUIPMENT_SAVE_REVISION_NUMBER = 8;

	public const int LINEAGE_SAVE_REVISION_NUMBER = 1;

	public const int STAGE_SAVE_REVISION_NUMBER = 14;

	public const int MODE_SAVE_REVISION_NUMBER = 2;

	public const int CONFIG_SAVE_REVISION_NUMBER = 1;

	public const int PROFILE_CONFIG_SAVE_REVISION_NUMBER = 1;

	public static DateTime NEXT_PATCH_DATE = new DateTime(2020, 10, 27, 17, 0, 0, DateTimeKind.Utc);

	public const string PATCH_TIMER_TEXT = "TBD";

	public const int UPDATES_SINCE_EA_LAUNCH = 32;

	public static string GetVersionString()
	{
		return string.Format("v{0}.{1}.{2}{3}-{4}", 1, 0, 0, "", "steam");
	}
}
