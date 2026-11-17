using System;
using Spawn;
using UnityEngine;

public class Ferr2DSpawnController : SimpleSpawnController
{
	[SerializeField]
	private PropSpawnControllerData m_spawnControllerData;

	private Ferr2DT_PathTerrain m_ferr2D;

	private void Awake()
	{
		m_ferr2D = GetComponent<Ferr2DT_PathTerrain>();
	}

	protected override void DoIsSpawned()
	{
		if (base.Room == null)
		{
			throw new ArgumentNullException("Room");
		}
		if (m_spawnControllerData == null)
		{
			throw new ArgumentNullException("m_spawnControllerData");
		}
		Prop[] array = m_spawnControllerData.DefaultProps;
		for (int i = 0; i < m_spawnControllerData.PropsPerBiome.Length; i++)
		{
			if (BiomeType_RL.GetGroupedBiomeType(base.Room.AppearanceBiomeType) == m_spawnControllerData.PropsPerBiome[i].Biome)
			{
				array = m_spawnControllerData.PropsPerBiome[i].Props;
				break;
			}
		}
		if (array != null && array.Length != 0)
		{
			int num = 0;
			if (array.Length > 1)
			{
				num = RNGManager.GetRandomNumber(RngID.Prop_RoomSeed, $"Ferr2D Spawn Controller. Get Material.", 0, array.Length);
			}
			Ferr2DT_PathTerrain component = array[num].GetComponent<Ferr2DT_PathTerrain>();
			if ((bool)component)
			{
				CameraLayerController component2 = GetComponent<CameraLayerController>();
				component2.SetCameraLayer(m_spawnControllerData.CameraLayer);
				component2.SetSubLayer(m_spawnControllerData.SubLayer);
				m_ferr2D.SetMaterial(component.TerrainMaterial);
				m_ferr2D.BuildMeshOnly();
				return;
			}
			throw new ArgumentNullException("ferr2DComponent");
		}
		throw new ArgumentNullException("potentialProps");
	}
}
