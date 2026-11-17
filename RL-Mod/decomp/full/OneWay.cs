using UnityEngine;

public class OneWay : MonoBehaviour, IPlayHitEffect
{
	public bool PlayDirectionalHitEffect => false;

	public bool PlayHitEffect => true;

	public string EffectNameOverride => null;

	public void SetVisuals(BiomeType biomeType, BaseRoom room)
	{
		BiomeArtData biomeArtData = room.BiomeArtDataOverride;
		if (!biomeArtData)
		{
			biomeArtData = BiomeArtDataLibrary.GetArtData(biomeType);
		}
		if ((bool)biomeArtData)
		{
			if (biomeArtData.Ferr2DBiomeArtData != null && biomeArtData.Ferr2DBiomeArtData.OneWayMaster != null && biomeArtData.Ferr2DBiomeArtData.OneWayMaster.Ferr2DSettings != null)
			{
				Ferr2DSettings ferr2DSettings = biomeArtData.Ferr2DBiomeArtData.OneWayMaster.Ferr2DSettings;
				if (ferr2DSettings == null)
				{
					Debug.LogFormat("Unable to find Ferr2DSettings for One Way in Biome ({0})", biomeType);
				}
				else
				{
					UpdateFerr2DSettings(ferr2DSettings);
				}
			}
			else
			{
				Debug.LogFormat("<color=red>[{0}] Biome Art Data ({1})'s Ferr2DBiomeArtData, Ferr2DBiomeArtData.OneWayMaster or Ferr2DBiomeArtData.OneWayMaster.Ferr2DSettings is null</color>", this, biomeArtData);
			}
		}
		else
		{
			Debug.LogFormat("<color=red>[{0}] Failed to find Biome Art Data for Biome ({1})</color>", this, biomeType);
		}
	}

	private void UpdateFerr2DSettings(Ferr2DSettings settings)
	{
		Ferr2DT_PathTerrain component = GetComponent<Ferr2DT_PathTerrain>();
		component.SetMaterial(settings.Material);
		component.fillSplit = false;
		component.fillSplitDistance = settings.GridSpacing;
		component.vertexColorType = settings.ColorType;
		component.vertexGradientDistance = settings.VertexGradientDistance;
		component.vertexGradient = settings.VertexGradient;
		component.pixelsPerUnit = settings.PixelsPerUnit;
		component.createTangents = settings.CreateTangents;
		Renderer component2 = base.gameObject.GetComponent<Renderer>();
		component2.sortingLayerID = settings.SortingLayer;
		component2.sortingOrder = settings.OrderInLayer;
	}
}
