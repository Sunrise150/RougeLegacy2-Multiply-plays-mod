using UnityEngine;

public class Room_EV : MonoBehaviour
{
	public const float MAX_CAMERA_ZOOM_DEFAULT = 1f;

	public const int ROOM_UNIT_WIDTH = 32;

	public const int ROOM_UNIT_HEIGHT = 18;

	public const float WALL_WIDTH = 1f;

	public const int UPPER_LOWER_DOOR_UNIT_SIZE = 6;

	public const int SIDE_DOOR_UNIT_SIZE = 5;

	public static float TRANSITION_LEFT_RIGHT_DISTANCE = 0.3f;

	public static float TRANSITION_TOP_DISTANCE = -0.9f;

	public static float TRANSITION_BOTTOM_DISTANCE = -1.15f;

	public static float TRANSITION_PLAYER_POSITION_LEFT_RIGHT_X_OFFSET = -0.6f;

	public static float TRANSITION_PLAYER_POSITION_BOTTOM_Y_OFFSET = -1f;

	public static float TRANSITION_PLAYER_POSITION_TOP_Y_OFFSET = -0.25f;

	public static float TRANSITION_MINIMUM_SPEED_ON_ENTER_BOTTOM = 19.5f;

	public static float DISABLE_BRAKING_FORCE_TIME = 0.125f;
}
