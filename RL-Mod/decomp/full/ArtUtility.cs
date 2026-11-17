public static class ArtUtility
{
	public static BiomeType GetTerrainBiomeType(BaseRoom room)
	{
		BiomeType biomeType = room.AppearanceBiomeType;
		if (room is Room && (room.AppearanceBiomeType != room.BiomeType || (bool)room.BiomeArtDataOverride))
		{
			BiomeArtData biomeArtData = room.BiomeArtDataOverride;
			if (!biomeArtData)
			{
				biomeArtData = BiomeArtDataLibrary.GetArtData(biomeType);
			}
			Ferr2DT_PathTerrain ferr2DT_PathTerrain = null;
			if ((bool)biomeArtData && biomeArtData.Ferr2DBiomeArtData != null)
			{
				ferr2DT_PathTerrain = biomeArtData.Ferr2DBiomeArtData.TerrainMaster.Master;
			}
			if (!ferr2DT_PathTerrain)
			{
				biomeType = room.BiomeType;
			}
		}
		return biomeType;
	}
}
