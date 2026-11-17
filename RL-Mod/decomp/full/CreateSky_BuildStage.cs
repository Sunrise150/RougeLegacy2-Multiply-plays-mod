using System.Collections;
using System.Collections.Generic;

public class CreateSky_BuildStage : IBiomeBuildStage
{
	public IEnumerator Run(BiomeController biomeController)
	{
		Dictionary<SkyBiomeArtData, Sky> dictionary = new Dictionary<SkyBiomeArtData, Sky>();
		foreach (BaseRoom room in biomeController.Rooms)
		{
			BiomeArtData biomeArtData = room.BiomeArtDataOverride;
			if (!biomeArtData)
			{
				biomeArtData = BiomeArtDataLibrary.GetArtData(room.AppearanceBiomeType);
			}
			if (dictionary.ContainsKey(biomeArtData.SkyData))
			{
				dictionary[biomeArtData.SkyData].RoomList.Add(room);
				continue;
			}
			Sky sky = BiomeCreatorTools.CreateSkyInstance(biomeArtData);
			dictionary.Add(biomeArtData.SkyData, sky);
			sky.RoomList.Add(room);
			biomeController.SkyCreatedByWorldBuilder(sky);
		}
		yield break;
	}
}
