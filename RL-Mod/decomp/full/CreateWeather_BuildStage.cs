using System.Collections;
using System.Collections.Generic;

public class CreateWeather_BuildStage : IBiomeBuildStage
{
	public IEnumerator Run(BiomeController biomeController)
	{
		Dictionary<WeatherBiomeArtData, Weather[]> dictionary = new Dictionary<WeatherBiomeArtData, Weather[]>();
		foreach (BaseRoom room in biomeController.Rooms)
		{
			BiomeArtData biomeArtData = room.BiomeArtDataOverride;
			if (!biomeArtData)
			{
				biomeArtData = BiomeArtDataLibrary.GetArtData(room.AppearanceBiomeType);
			}
			Weather[] array;
			if (dictionary.ContainsKey(biomeArtData.WeatherData))
			{
				array = dictionary[biomeArtData.WeatherData];
				for (int i = 0; i < array.Length; i++)
				{
					array[i].RoomList.Add(room);
				}
				continue;
			}
			Weather[] array2 = BiomeCreatorTools.CreateWeatherInstances(biomeArtData);
			dictionary.Add(biomeArtData.WeatherData, array2);
			array = array2;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].RoomList.Add(room);
			}
			biomeController.WeatherCreatedByWorldBuilder(array2);
		}
		yield break;
	}
}
