public static class Map_EV
{
	public const float PAN_SPEED = 14f;

	public const float ZOOM_SPEED = 14f;

	public const float MIN_ZOOM = 6f;

	public const float MAX_ZOOM = 24f;

	public const float ROOM_SCALE = 1f / 15f;

	public const bool RECAP_ZOOM_OUT_BEFORE_PANNING = false;

	public const float RECAP_PAN_TWEEN_TIME = 0.5f;

	public const float RECAP_ZOOM_TWEEN_TIME = 0.5f;

	public const float RECAP_INITIAL_ZOOM = 3f;

	public const float RECAP_MAX_ZOOM = 6f;

	public const float RECAP_MAX_FREE_ZOOM = 12f;

	public static BiomeType[] BIOME_DISPLAY_ORDER = new BiomeType[6]
	{
		BiomeType.Castle,
		BiomeType.Stone,
		BiomeType.Forest,
		BiomeType.Study,
		BiomeType.Tower,
		BiomeType.Cave
	};
}
