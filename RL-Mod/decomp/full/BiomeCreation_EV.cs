using System.Collections.Generic;
using UnityEngine;

public static class BiomeCreation_EV
{
	public static int DEFAULT_ROOM_RNG_WEIGHT = 100;

	public const int DEFAULT_HEALING_ROOM_SPAWN_WEIGHT = 25;

	public const int DEFAULT_RELIC_ROOM_SPAWN_WEIGHT = 30;

	public const int DEFAULT_ARENA_ROOM_SPAWN_WEIGHT = 20;

	public const int DEFAULT_CURIO_ROOM_SPAWN_WEIGHT = 25;

	public const int DEFAULT_TELEPORTER_ROOM_SPAWN_WEIGHT = 0;

	public const int DEFAULT_PORTRAIT_ROOM_SPAWN_WEIGHT = 0;

	public const int DEFAULT_CLOWN_ROOM_SPAWN_WEIGHT = 0;

	public const int DEFAULT_JOURNAL_ROOM_SPAWN_WEIGHT = 0;

	public const bool USE_RANDOM_STANDARD_ROOM_POOL_CULLING = false;

	public const int WORLD_BUILD_TIMESLICE_THRESHOLD = 30;

	public const bool FORCE_ROOM_WEIGHTS_TO_DEFAULT = false;

	public const bool VALIDATE_BIOME_AFTER_BUILD = false;

	public const bool TEST_MEGA_BIOMES = false;

	public const float MEGA_BIOME_SIZE_MULTIPLIER = 3f;

	public static Dictionary<BiomeType, List<BiomeTag>> BIOME_TAGS = new Dictionary<BiomeType, List<BiomeTag>>
	{
		{
			BiomeType.Castle,
			new List<BiomeTag>
			{
				BiomeTag.Interior,
				BiomeTag.Player_Dash_FALSE,
				BiomeTag.Player_DoubleJump_FALSE
			}
		},
		{
			BiomeType.Stone,
			new List<BiomeTag>
			{
				BiomeTag.Exterior,
				BiomeTag.Player_Dash_TRUE,
				BiomeTag.Player_DoubleJump_FALSE
			}
		},
		{
			BiomeType.Forest,
			new List<BiomeTag>
			{
				BiomeTag.Exterior,
				BiomeTag.Player_Dash_TRUE,
				BiomeTag.Player_DoubleJump_FALSE
			}
		},
		{
			BiomeType.ForestBottom,
			new List<BiomeTag>
			{
				BiomeTag.Exterior,
				BiomeTag.Player_Dash_TRUE,
				BiomeTag.Player_DoubleJump_FALSE
			}
		},
		{
			BiomeType.ForestTop,
			new List<BiomeTag>
			{
				BiomeTag.Exterior,
				BiomeTag.Player_Dash_TRUE,
				BiomeTag.Player_DoubleJump_FALSE
			}
		},
		{
			BiomeType.Study,
			new List<BiomeTag>
			{
				BiomeTag.Interior,
				BiomeTag.Player_Dash_TRUE,
				BiomeTag.Player_DoubleJump_TRUE
			}
		},
		{
			BiomeType.Tower,
			new List<BiomeTag>
			{
				BiomeTag.Interior,
				BiomeTag.Player_Dash_TRUE,
				BiomeTag.Player_DoubleJump_TRUE
			}
		},
		{
			BiomeType.TowerExterior,
			new List<BiomeTag>
			{
				BiomeTag.Exterior,
				BiomeTag.Player_Dash_TRUE,
				BiomeTag.Player_DoubleJump_TRUE
			}
		},
		{
			BiomeType.Cave,
			new List<BiomeTag>
			{
				BiomeTag.Exterior,
				BiomeTag.Player_Dash_TRUE,
				BiomeTag.Player_DoubleJump_TRUE
			}
		},
		{
			BiomeType.CaveMiddle,
			new List<BiomeTag>
			{
				BiomeTag.Exterior,
				BiomeTag.Player_Dash_TRUE,
				BiomeTag.Player_DoubleJump_TRUE
			}
		},
		{
			BiomeType.CaveBottom,
			new List<BiomeTag>
			{
				BiomeTag.Exterior,
				BiomeTag.Player_Dash_TRUE,
				BiomeTag.Player_DoubleJump_TRUE
			}
		},
		{
			BiomeType.Dragon,
			new List<BiomeTag>
			{
				BiomeTag.Interior,
				BiomeTag.Player_Dash_TRUE,
				BiomeTag.Player_DoubleJump_TRUE
			}
		}
	};

	public static Dictionary<BiomeType, BiomeType> TUNNEL_DESTINATION_BIOME_TYPE_TABLE = new Dictionary<BiomeType, BiomeType> { 
	{
		BiomeType.Stone,
		BiomeType.Sunken
	} };

	public static Dictionary<BiomeType, bool> IS_IT_RAINING_IN_BIOME_TABLE = new Dictionary<BiomeType, bool> { 
	{
		BiomeType.Stone,
		true
	} };

	private static Dictionary<BiomeType, bool> DUPLICATE_ROOMS_ALLOWED = new Dictionary<BiomeType, bool>();

	public static Dictionary<BiomeType, int> FORCE_EASY_ROOMS_AT_BIOME_START = new Dictionary<BiomeType, int> { 
	{
		BiomeType.Castle,
		2
	} };

	public static Dictionary<BiomeType, int> REQUEST_EASY_FAIRY_ROOMS_IN_BIOME = new Dictionary<BiomeType, int>
	{
		{
			BiomeType.Castle,
			1
		},
		{
			BiomeType.Forest,
			1
		}
	};

	public const bool DONT_CREATE_MIRRORED_VERSION_OF_ROOMS_WITH_MAX_NUMBER_DOORS = false;

	public const bool DONT_CREATE_MIRRORED_VERSION_OF_1X1_ROOMS_WITH_DOORS_ON_LEFT_AND_RIGHT = false;

	public const bool DONT_CREATE_MIRRORED_VERSION_OF_1XN_ROOMS_WITH_THE_SAME_DOORS_ON_LEFT_AND_RIGHT = false;

	public static bool USE_DEFAULT_MERGE_LOGIC_FOR_TOWER_INTERIOR = true;

	public const bool MERGE_TOWER_INTERIOR = true;

	public const bool MERGE_TOWER_EXTERIOR = true;

	public const bool MERGE_TOWER_TOP = true;

	public const bool MERGE_DRAGON = true;

	public const float NIBIRU_DEEP_WATER_RAISE_PER_BOSS_KILLED = 2.5f;

	public static List<BiomeType> DISABLE_HORIZONTAL_FILL_IN_BIOMES = new List<BiomeType> { BiomeType.Stone };

	public static List<BiomeType> DISABLE_VERTICAL_FILL_IN_BIOMES = new List<BiomeType> { BiomeType.TowerExterior };

	public static List<BiomeType> DO_NOT_ADD_ONE_WAYS_TO_BOTTOM_DOORS = new List<BiomeType>
	{
		BiomeType.TowerExterior,
		BiomeType.Dragon
	};

	private static Dictionary<RoomSide, int> CASTLE_BOUNDS = new Dictionary<RoomSide, int>
	{
		{
			RoomSide.Left,
			2
		},
		{
			RoomSide.Right,
			30
		},
		{
			RoomSide.Top,
			30
		},
		{
			RoomSide.Bottom,
			-30
		}
	};

	private static Dictionary<RoomSide, int> TOWN_BOUNDS = new Dictionary<RoomSide, int>
	{
		{
			RoomSide.Left,
			-10
		},
		{
			RoomSide.Right,
			100
		},
		{
			RoomSide.Top,
			7
		},
		{
			RoomSide.Bottom,
			-7
		}
	};

	private static Dictionary<RoomSide, int> DRAGON_BOUNDS = new Dictionary<RoomSide, int>
	{
		{
			RoomSide.Left,
			-10
		},
		{
			RoomSide.Right,
			100
		},
		{
			RoomSide.Top,
			0
		},
		{
			RoomSide.Bottom,
			-100
		}
	};

	private static Dictionary<RoomSide, int> FOREST_BOUNDS = new Dictionary<RoomSide, int>
	{
		{
			RoomSide.Left,
			0
		},
		{
			RoomSide.Right,
			50
		},
		{
			RoomSide.Top,
			10
		},
		{
			RoomSide.Bottom,
			-10
		}
	};

	private static Dictionary<RoomSide, int> STONE_BOUNDS = new Dictionary<RoomSide, int>
	{
		{
			RoomSide.Left,
			0
		},
		{
			RoomSide.Right,
			200
		},
		{
			RoomSide.Top,
			1
		},
		{
			RoomSide.Bottom,
			0
		}
	};

	private static Dictionary<RoomSide, int> LAKE_BOUNDS = new Dictionary<RoomSide, int>
	{
		{
			RoomSide.Left,
			-100
		},
		{
			RoomSide.Right,
			100
		},
		{
			RoomSide.Top,
			100
		},
		{
			RoomSide.Bottom,
			-100
		}
	};

	private static Dictionary<RoomSide, int> STUDY_BOUNDS = new Dictionary<RoomSide, int>
	{
		{
			RoomSide.Left,
			-7
		},
		{
			RoomSide.Right,
			7
		},
		{
			RoomSide.Top,
			50
		},
		{
			RoomSide.Bottom,
			2
		}
	};

	private static Dictionary<RoomSide, int> TOWER_BOUNDS = new Dictionary<RoomSide, int>
	{
		{
			RoomSide.Left,
			-10
		},
		{
			RoomSide.Right,
			10
		},
		{
			RoomSide.Top,
			250
		},
		{
			RoomSide.Bottom,
			0
		}
	};

	private static Dictionary<RoomSide, int> CAVE_BOUNDS = new Dictionary<RoomSide, int>
	{
		{
			RoomSide.Left,
			-50
		},
		{
			RoomSide.Right,
			100
		},
		{
			RoomSide.Top,
			-8
		},
		{
			RoomSide.Bottom,
			-30
		}
	};

	private static Dictionary<RoomSide, int> SUNKEN_BOUNDS = new Dictionary<RoomSide, int>
	{
		{
			RoomSide.Left,
			2
		},
		{
			RoomSide.Right,
			30
		},
		{
			RoomSide.Top,
			0
		},
		{
			RoomSide.Bottom,
			-35
		}
	};

	private static Dictionary<RoomSide, int> DEFAULT_BOUNDS = new Dictionary<RoomSide, int>
	{
		{
			RoomSide.Left,
			-25
		},
		{
			RoomSide.Right,
			25
		},
		{
			RoomSide.Top,
			25
		},
		{
			RoomSide.Bottom,
			-25
		}
	};

	private static Dictionary<BiomeType, float> TRACE_DEPTH_TABLE = new Dictionary<BiomeType, float>
	{
		{
			BiomeType.Study,
			0.5f
		},
		{
			BiomeType.Tower,
			0.5f
		}
	};

	private static Dictionary<BiomeType, List<RoomSide>> REPLACE_SIDES_TABLE = new Dictionary<BiomeType, List<RoomSide>> { 
	{
		BiomeType.Tower,
		new List<RoomSide> { RoomSide.Top }
	} };

	public static int GetDefaultSpecialRoomWeight(SpecialRoomType specialRoomType)
	{
		return specialRoomType switch
		{
			SpecialRoomType.Healing => 25, 
			SpecialRoomType.Relic => 30, 
			SpecialRoomType.Arena => 20, 
			SpecialRoomType.Curio => 25, 
			SpecialRoomType.Journal => 0, 
			SpecialRoomType.Portrait => 0, 
			SpecialRoomType.Clown => 0, 
			SpecialRoomType.Teleporter => 0, 
			_ => 0, 
		};
	}

	public static bool GetDuplicateRoomsAllowed(BiomeType biome)
	{
		bool result = false;
		if (DUPLICATE_ROOMS_ALLOWED.ContainsKey(biome))
		{
			result = DUPLICATE_ROOMS_ALLOWED[biome];
		}
		return result;
	}

	public static int GetBiomeLevel(BiomeType biomeType)
	{
		return biomeType switch
		{
			BiomeType.Castle => 1, 
			BiomeType.Stone => 2, 
			BiomeType.Forest => 3, 
			BiomeType.Study => 4, 
			BiomeType.Tower => 5, 
			BiomeType.Cave => 6, 
			BiomeType.Garden => -1, 
			_ => 0, 
		};
	}

	public static Dictionary<RoomSide, int> GetBiomeBorderOffsets(BiomeType biome)
	{
		return biome switch
		{
			BiomeType.Castle => CASTLE_BOUNDS, 
			BiomeType.Forest => FOREST_BOUNDS, 
			BiomeType.Stone => STONE_BOUNDS, 
			BiomeType.Lake => LAKE_BOUNDS, 
			BiomeType.Town => TOWN_BOUNDS, 
			BiomeType.Dragon => DRAGON_BOUNDS, 
			BiomeType.Study => STUDY_BOUNDS, 
			BiomeType.Tower => TOWER_BOUNDS, 
			BiomeType.Cave => CAVE_BOUNDS, 
			BiomeType.Sunken => SUNKEN_BOUNDS, 
			_ => DEFAULT_BOUNDS, 
		};
	}

	public static List<RoomSide> GetSidesToReplace(BiomeType biome)
	{
		if (REPLACE_SIDES_TABLE.ContainsKey(biome))
		{
			return REPLACE_SIDES_TABLE[biome];
		}
		return new List<RoomSide>
		{
			RoomSide.Left,
			RoomSide.Right,
			RoomSide.Top,
			RoomSide.Bottom
		};
	}

	public static int GetNumberOfRoomsToLookBack(BiomeType biome, int currentRoomCount, int totalTargetRoomCount)
	{
		if (TRACE_DEPTH_TABLE.ContainsKey(biome))
		{
			return Mathf.Min(Mathf.RoundToInt(TRACE_DEPTH_TABLE[biome] * (float)totalTargetRoomCount), currentRoomCount);
		}
		return currentRoomCount;
	}
}
