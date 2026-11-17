using UnityEngine;

[CreateAssetMenu(menuName = "Custom/Biome Art Data")]
public class BiomeArtData : ScriptableObject
{
	public BiomeType Biome;

	public Ferr2DBiomeArtData Ferr2DBiomeArtData;

	public PostProcessingBiomeArtData PostProcessingData;

	public AmbientLightAndFogBiomeArtData LightAndFogData;

	public BackgroundBiomeArtData BackgroundData;

	public ForegroundBiomeArtData ForegroundData;

	public SkyBiomeArtData SkyData;

	public WeatherBiomeArtData WeatherData;
}
