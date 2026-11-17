public static class LayerType_RL
{
	public const string Default = "Default";

	public const string TransparentFX = "TransparentFX";

	public const string Ignore_Raycast = "Ignore Raycast";

	public const string Water = "Water";

	public const string UI = "UI";

	public const string Terrain_Hitbox_ItemDrop = "Terrain_Hitbox_HitsPlayerOnly";

	public const string Platform_CollidesWithAll = "Platform_CollidesWithAll";

	public const string Platform_CollidesWithPlayer = "Platform_CollidesWithPlayer";

	public const string Platform_CollidesWithEnemy = "Platform_CollidesWithEnemy";

	public const string Platform_OneWay = "Platform_OneWay";

	public const string Prop_Hitbox = "Prop_Hitbox";

	public const string Terrain_Hitbox = "Terrain_Hitbox";

	public const string LevelBounds = "LevelBounds";

	public const string HELPER_NOTOUCH = "HELPER_NOTOUCH";

	public const string Weapon_Hitbox = "Weapon_Hitbox";

	public const string Weapon_Hitbox_HitsPlayerOnly = "Weapon_Hitbox_HitsPlayerOnly";

	public const string Terrain_Hitbox_HitsPlatform = "Terrain_Hitbox_HitsPlatform";

	public const string Foreground_Persp = "Foreground_Persp";

	public const string Body_Hitbox_ForPlayerOnly = "Body_Hitbox_ForPlayerOnly";

	public const string Body_Hitbox = "Body_Hitbox";

	public const string Background_Persp_Near = "Background_Persp_Near";

	public const string Foreground_Ortho = "Foreground_Ortho";

	public const string Background_Ortho = "Background_Ortho";

	public const string Foreground_Lights = "Foreground_Lights";

	public const string Background_Persp_Far = "Background_Persp_Far";

	public const string TraitMask = "TraitMask";

	public const string Solo = "Solo";

	public const string Character = "Character";

	public const string TerrainHazard_Hitbox = "TerrainHazard_Hitbox";

	public static LayerType ToEnum(string value)
	{
		return value switch
		{
			"Default" => LayerType.Default, 
			"TransparentFX" => LayerType.TransparentFX, 
			"Ignore Raycast" => LayerType.Ignore_Raycast, 
			"Water" => LayerType.Water, 
			"UI" => LayerType.UI, 
			"Platform_CollidesWithAll" => LayerType.Platform_CollidesWithAll, 
			"Platform_CollidesWithPlayer" => LayerType.Platform_CollidesWithPlayer, 
			"Platform_CollidesWithEnemy" => LayerType.Platform_CollidesWithEnemy, 
			"Platform_OneWay" => LayerType.Platform_OneWay, 
			"Prop_Hitbox" => LayerType.Prop_Hitbox, 
			"Terrain_Hitbox" => LayerType.Terrain_Hitbox, 
			"LevelBounds" => LayerType.LevelBounds, 
			"HELPER_NOTOUCH" => LayerType.HELPER_NOTOUCH, 
			"Weapon_Hitbox" => LayerType.Weapon_Hitbox, 
			"Weapon_Hitbox_HitsPlayerOnly" => LayerType.Weapon_Hitbox_HitsPlayerOnly, 
			"Terrain_Hitbox_HitsPlatform" => LayerType.Terrain_Hitbox_HitsPlatform, 
			"Foreground_Persp" => LayerType.Foreground_Persp, 
			"Body_Hitbox_ForPlayerOnly" => LayerType.Body_Hitbox_ForPlayerOnly, 
			"Body_Hitbox" => LayerType.Body_Hitbox, 
			"Background_Persp_Near" => LayerType.Background_Persp_Near, 
			"Foreground_Ortho" => LayerType.Foreground_Ortho, 
			"Background_Ortho" => LayerType.Background_Ortho, 
			"Foreground_Lights" => LayerType.Foreground_Lights, 
			"Background_Persp_Far" => LayerType.Background_Persp_Far, 
			"TraitMask" => LayerType.TraitMask, 
			"Solo" => LayerType.Solo, 
			"Character" => LayerType.Character, 
			"TerrainHazard_Hitbox" => LayerType.TerrainHazard_Hitbox, 
			"Terrain_Hitbox_HitsPlayerOnly" => LayerType.Terrain_Hitbox_ItemDrop, 
			_ => LayerType.None, 
		};
	}

	public static string ToString(LayerType value)
	{
		return value switch
		{
			LayerType.Default => "Default", 
			LayerType.TransparentFX => "TransparentFX", 
			LayerType.Ignore_Raycast => "Ignore Raycast", 
			LayerType.Water => "Water", 
			LayerType.UI => "UI", 
			LayerType.Platform_CollidesWithAll => "Platform_CollidesWithAll", 
			LayerType.Platform_CollidesWithPlayer => "Platform_CollidesWithPlayer", 
			LayerType.Platform_CollidesWithEnemy => "Platform_CollidesWithEnemy", 
			LayerType.Platform_OneWay => "Platform_OneWay", 
			LayerType.Prop_Hitbox => "Prop_Hitbox", 
			LayerType.Terrain_Hitbox => "Terrain_Hitbox", 
			LayerType.LevelBounds => "LevelBounds", 
			LayerType.HELPER_NOTOUCH => "HELPER_NOTOUCH", 
			LayerType.Weapon_Hitbox => "Weapon_Hitbox", 
			LayerType.Weapon_Hitbox_HitsPlayerOnly => "Weapon_Hitbox_HitsPlayerOnly", 
			LayerType.Terrain_Hitbox_HitsPlatform => "Terrain_Hitbox_HitsPlatform", 
			LayerType.Foreground_Persp => "Foreground_Persp", 
			LayerType.Body_Hitbox_ForPlayerOnly => "Body_Hitbox_ForPlayerOnly", 
			LayerType.Body_Hitbox => "Body_Hitbox", 
			LayerType.Background_Persp_Near => "Background_Persp_Near", 
			LayerType.Foreground_Ortho => "Foreground_Ortho", 
			LayerType.Background_Ortho => "Background_Ortho", 
			LayerType.Foreground_Lights => "Foreground_Lights", 
			LayerType.Background_Persp_Far => "Background_Persp_Far", 
			LayerType.TraitMask => "TraitMask", 
			LayerType.Solo => "Solo", 
			LayerType.Character => "Character", 
			LayerType.TerrainHazard_Hitbox => "TerrainHazard_Hitbox", 
			LayerType.Terrain_Hitbox_ItemDrop => "Terrain_Hitbox_HitsPlayerOnly", 
			_ => "", 
		};
	}
}
