using System;

public static class TagType_RL
{
	public const string Untagged = "Untagged";

	public const string Respawn = "Respawn";

	public const string Finish = "Finish";

	public const string EditorOnly = "EditorOnly";

	public const string MainCamera = "MainCamera";

	public const string Player = "Player";

	public const string GameController = "GameController";

	public const string LevelBounds = "LevelBounds";

	public const string Music = "Music";

	public const string Background = "Background";

	public const string Platform = "Platform";

	public const string Door = "Door";

	public const string Room = "Room";

	public const string OneWay = "OneWay";

	public const string Enemy = "Enemy";

	public const string Breakable = "Breakable";

	public const string Projectile = "Projectile";

	public const string ItemDrop = "ItemDrop";

	public const string Hazard = "Hazard";

	public const string PlayerProjectile = "PlayerProjectile";

	public const string EnemyProjectile = "EnemyProjectile";

	public const string Chest = "Chest";

	public const string EquipmentButton = "EquipmentButton";

	public const string Prop = "Prop";

	public const string EditorProp = "EditorProp";

	public const string LevelEditor = "LevelEditor";

	public const string Cutout = "Cutout";

	public const string Tunnel = "Tunnel";

	public const string NPC = "NPC";

	public const string Barricade = "Barricade";

	public const string TriggerHazard = "TriggerHazard";

	public const string Effect = "Effect";

	public const string Player_Dodging = "Player_Dodging";

	public const string FlimsyBreakable = "FlimsyBreakable";

	public const string SpawnController = "SpawnController";

	public const string TeleporterTube = "TeleporterTube";

	public const string Generic_Bounceable = "Generic_Bounceable";

	public const string MagicPlatform = "MagicPlatform";

	public const string PlayerStatusEffect = "PlayerStatusEffect";

	public const string EnemyStatusEffect = "EnemyStatusEffect";

	public const string NonResonant_Bounceable = "NonResonant_Bounceable";

	public static TagType ToEnum(string value)
	{
		return value switch
		{
			"Untagged" => TagType.Untagged, 
			"Respawn" => TagType.Respawn, 
			"Finish" => TagType.Finish, 
			"EditorOnly" => TagType.EditorOnly, 
			"MainCamera" => TagType.MainCamera, 
			"Player" => TagType.Player, 
			"GameController" => TagType.GameController, 
			"LevelBounds" => TagType.LevelBounds, 
			"Music" => TagType.Music, 
			"Background" => TagType.Background, 
			"Platform" => TagType.Platform, 
			"Door" => TagType.Door, 
			"Room" => TagType.Room, 
			"OneWay" => TagType.OneWay, 
			"Enemy" => TagType.Enemy, 
			"Breakable" => TagType.Breakable, 
			"Projectile" => TagType.Projectile, 
			"ItemDrop" => TagType.ItemDrop, 
			"Hazard" => TagType.Hazard, 
			"PlayerProjectile" => TagType.PlayerProjectile, 
			"EnemyProjectile" => TagType.EnemyProjectile, 
			"Chest" => TagType.Chest, 
			"EquipmentButton" => TagType.EquipmentButton, 
			"Prop" => TagType.Prop, 
			"EditorProp" => TagType.EditorProp, 
			"LevelEditor" => TagType.LevelEditor, 
			"Cutout" => TagType.Cutout, 
			"Tunnel" => TagType.Tunnel, 
			"NPC" => TagType.NPC, 
			"Barricade" => TagType.Barricade, 
			"TriggerHazard" => TagType.TriggerHazard, 
			"Effect" => TagType.Effect, 
			"Player_Dodging" => TagType.Player_Dodging, 
			"FlimsyBreakable" => TagType.FlimsyBreakable, 
			"SpawnController" => TagType.SpawnController, 
			"TeleporterTube" => TagType.TeleporterTube, 
			"Generic_Bounceable" => TagType.Generic_Bounceable, 
			"MagicPlatform" => TagType.MagicPlatform, 
			"PlayerStatusEffect" => TagType.PlayerStatusEffect, 
			"EnemyStatusEffect" => TagType.EnemyStatusEffect, 
			"NonResonant_Bounceable" => TagType.NonResonant_Bounceable, 
			_ => throw new Exception("Tag: " + value + " does not exist in Tag manager."), 
		};
	}

	public static string ToString(TagType value)
	{
		return value switch
		{
			TagType.Untagged => "Untagged", 
			TagType.Respawn => "Respawn", 
			TagType.Finish => "Finish", 
			TagType.EditorOnly => "EditorOnly", 
			TagType.MainCamera => "MainCamera", 
			TagType.Player => "Player", 
			TagType.GameController => "GameController", 
			TagType.LevelBounds => "LevelBounds", 
			TagType.Music => "Music", 
			TagType.Background => "Background", 
			TagType.Platform => "Platform", 
			TagType.Door => "Door", 
			TagType.Room => "Room", 
			TagType.OneWay => "OneWay", 
			TagType.Enemy => "Enemy", 
			TagType.Breakable => "Breakable", 
			TagType.Projectile => "Projectile", 
			TagType.ItemDrop => "ItemDrop", 
			TagType.Hazard => "Hazard", 
			TagType.PlayerProjectile => "PlayerProjectile", 
			TagType.EnemyProjectile => "EnemyProjectile", 
			TagType.Chest => "Chest", 
			TagType.EquipmentButton => "EquipmentButton", 
			TagType.Prop => "Prop", 
			TagType.EditorProp => "EditorProp", 
			TagType.LevelEditor => "LevelEditor", 
			TagType.Cutout => "Cutout", 
			TagType.Tunnel => "Tunnel", 
			TagType.NPC => "NPC", 
			TagType.Barricade => "Barricade", 
			TagType.TriggerHazard => "TriggerHazard", 
			TagType.Effect => "Effect", 
			TagType.Player_Dodging => "Player_Dodging", 
			TagType.FlimsyBreakable => "FlimsyBreakable", 
			TagType.SpawnController => "SpawnController", 
			TagType.TeleporterTube => "TeleporterTube", 
			TagType.Generic_Bounceable => "Generic_Bounceable", 
			TagType.MagicPlatform => "MagicPlatform", 
			TagType.PlayerStatusEffect => "PlayerStatusEffect", 
			TagType.EnemyStatusEffect => "EnemyStatusEffect", 
			TagType.NonResonant_Bounceable => "NonResonant_Bounceable", 
			_ => throw new Exception("Tag: " + value.ToString() + " does not exist in Tag manager."), 
		};
	}
}
