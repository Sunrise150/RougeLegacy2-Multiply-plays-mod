using System;
using UnityEngine;

public class BiomeLightController : MonoBehaviour
{
	[SerializeField]
	private Light m_light;

	[SerializeField]
	[ReadOnly]
	private BiomeType m_currentBiome;

	private Action<MonoBehaviour, EventArgs> m_onBiomeChange;

	private Action<MonoBehaviour, EventArgs> m_onPlayerEnterRoom;

	private void Awake()
	{
		m_onBiomeChange = OnBiomeChange;
		m_onPlayerEnterRoom = OnPlayerEnterRoom;
	}

	private void Start()
	{
		Messenger<GameMessenger, GameEvent>.AddListener(GameEvent.BiomeEnter, m_onBiomeChange);
		Messenger<GameMessenger, GameEvent>.AddListener(GameEvent.PlayerEnterRoom, m_onPlayerEnterRoom);
	}

	private void OnDisable()
	{
		Messenger<GameMessenger, GameEvent>.RemoveListener(GameEvent.BiomeEnter, m_onBiomeChange);
		Messenger<GameMessenger, GameEvent>.RemoveListener(GameEvent.PlayerEnterRoom, m_onPlayerEnterRoom);
	}

	private void OnPlayerEnterRoom(MonoBehaviour sender, EventArgs eventArgs)
	{
		if (eventArgs is RoomViaDoorEventArgs roomViaDoorEventArgs)
		{
			BiomeType appearanceBiomeType = roomViaDoorEventArgs.Room.AppearanceBiomeType;
			if (m_currentBiome != appearanceBiomeType || (bool)roomViaDoorEventArgs.Room.BiomeArtDataOverride)
			{
				UpdateLights(appearanceBiomeType, roomViaDoorEventArgs.Room);
				if ((bool)roomViaDoorEventArgs.Room.BiomeArtDataOverride)
				{
					m_currentBiome = BiomeType.None;
				}
			}
			return;
		}
		throw new InvalidCastException("Failed to cast eventArgs as RoomViaDoorEventArgs");
	}

	private void OnBiomeChange(MonoBehaviour sender, EventArgs eventArgs)
	{
		if (eventArgs is BiomeEventArgs biomeEventArgs)
		{
			UpdateLights(biomeEventArgs.Biome, null);
			return;
		}
		Debug.LogFormat("<color=red>[{0}] Failed to cast eventArgs as BiomeChangeEventArgs</color>", this);
	}

	private void UpdateLights(BiomeType biome, BaseRoom room)
	{
		BiomeArtData biomeArtData = null;
		if (!room.IsNativeNull())
		{
			biomeArtData = room.BiomeArtDataOverride;
		}
		if (!biomeArtData)
		{
			biomeArtData = BiomeArtDataLibrary.GetArtData(biome);
		}
		if (!biomeArtData)
		{
			return;
		}
		if (biomeArtData.LightAndFogData != null)
		{
			Light lightMasterPrefab = biomeArtData.LightAndFogData.LightMasterPrefab;
			if ((bool)lightMasterPrefab)
			{
				m_currentBiome = biome;
				m_light.transform.rotation = lightMasterPrefab.transform.rotation;
				m_light.color = lightMasterPrefab.color;
				m_light.intensity = lightMasterPrefab.intensity;
				m_light.cullingMask = lightMasterPrefab.cullingMask;
				m_light.shadows = lightMasterPrefab.shadows;
				m_light.shadowStrength = lightMasterPrefab.shadowStrength;
				m_light.shadowNormalBias = lightMasterPrefab.shadowNormalBias;
				m_light.shadowBias = lightMasterPrefab.shadowBias;
				m_light.renderMode = lightMasterPrefab.renderMode;
			}
			else
			{
				Debug.LogFormat("<color=red>[{0}] Light Master Prefab field is null in Light And Fog Data on ({1})</color>", this, biomeArtData);
			}
		}
		else
		{
			Debug.LogFormat("<color=red>[{0}] Light And Fog Data is null on ({1})</color>", this, biomeArtData);
		}
	}
}
