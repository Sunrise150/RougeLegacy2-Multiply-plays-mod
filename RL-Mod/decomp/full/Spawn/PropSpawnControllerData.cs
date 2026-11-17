using UnityEngine;

namespace Spawn;

[CreateAssetMenu(menuName = "Custom/Rogue Legacy 2/Spawn Controller Data/Prop")]
public class PropSpawnControllerData : ScriptableObject
{
	public CameraLayer CameraLayer = CameraLayer.Game;

	public int SubLayer;

	public int SubLayerMod;

	public BiomePropsEntry[] PropsPerBiome;

	public Prop[] DefaultProps;

	public PropTags Tags;
}
