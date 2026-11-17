using System.Collections.Generic;

namespace RLAudio;

public static class SurfaceAudioManager
{
	private static Dictionary<SurfaceType, float> SURFACE_TYPE_PARAMETER_TABLE = new Dictionary<SurfaceType, float>
	{
		{
			SurfaceType.Default,
			0f
		},
		{
			SurfaceType.Stone,
			0f
		},
		{
			SurfaceType.Boardwalk,
			1f
		},
		{
			SurfaceType.Boat,
			2f
		},
		{
			SurfaceType.Lantern,
			3f
		},
		{
			SurfaceType.Barrel,
			4f
		},
		{
			SurfaceType.Box,
			5f
		},
		{
			SurfaceType.Bookshelf,
			6f
		},
		{
			SurfaceType.WeakFloor,
			7f
		},
		{
			SurfaceType.Chair,
			8f
		},
		{
			SurfaceType.Table,
			9f
		},
		{
			SurfaceType.SpikeTrap,
			10f
		},
		{
			SurfaceType.ShootTrap,
			11f
		},
		{
			SurfaceType.RocketBox,
			12f
		},
		{
			SurfaceType.Thorn,
			13f
		},
		{
			SurfaceType.Grass,
			14f
		},
		{
			SurfaceType.Dirt,
			15f
		},
		{
			SurfaceType.Snow,
			16f
		},
		{
			SurfaceType.Magic,
			17f
		}
	};

	private static Dictionary<BiomeType, SurfaceType> BIOME_DEFAULT_SURFACE_TYPE_TABLE = new Dictionary<BiomeType, SurfaceType> { 
	{
		BiomeType.Forest,
		SurfaceType.Snow
	} };

	public static float GetSurfaceParameter(SurfaceType surfaceType)
	{
		if (!SURFACE_TYPE_PARAMETER_TABLE.ContainsKey(surfaceType))
		{
			surfaceType = SurfaceType.Default;
		}
		if (PlayerManager.IsInstantiated && PlayerManager.GetCurrentPlayerRoom() != null)
		{
			BaseRoom currentPlayerRoom = PlayerManager.GetCurrentPlayerRoom();
			if (currentPlayerRoom != null)
			{
				BiomeType appearanceBiomeType = currentPlayerRoom.AppearanceBiomeType;
				if (surfaceType == SurfaceType.Default && BIOME_DEFAULT_SURFACE_TYPE_TABLE.ContainsKey(appearanceBiomeType))
				{
					surfaceType = BIOME_DEFAULT_SURFACE_TYPE_TABLE[appearanceBiomeType];
				}
			}
		}
		return SURFACE_TYPE_PARAMETER_TABLE[surfaceType];
	}
}
