using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;

[CreateAssetMenu(menuName = "Custom/Biome Rules/Camera Offset")]
public class CameraOffset_BiomeRule : BiomeRule
{
	[SerializeField]
	private Vector2 m_offsetOverride = new Vector2(0.5f, 0.6f);

	public override BiomeRuleExecutionTime ExecutionTime
	{
		get
		{
			if (GameUtility.IsInLevelEditor)
			{
				return BiomeRuleExecutionTime.WorldCreationComplete;
			}
			return BiomeRuleExecutionTime.BiomeCreationComplete;
		}
	}

	public override IEnumerator RunRule(BiomeType biome)
	{
		if (GameUtility.IsInLevelEditor && (bool)OnPlayManager.BiomeController)
		{
			CinemachineVirtualCamera virtualCamera = OnPlayManager.CurrentRoom.CinemachineCamera.VirtualCamera;
			SetOffset(virtualCamera);
			yield break;
		}
		BiomeController biomeController = WorldBuilder.GetBiomeController(biome);
		if (!(biomeController != null))
		{
			yield break;
		}
		List<CinemachineVirtualCamera> cinemachineVirtualCameras = biomeController.CinemachineVirtualCameras;
		if (cinemachineVirtualCameras != null)
		{
			for (int i = 0; i < cinemachineVirtualCameras.Count; i++)
			{
				SetOffset(cinemachineVirtualCameras[i]);
			}
		}
		else
		{
			Debug.LogFormat("<color=red>| {0} | Can't run rule because {1} Biome's BiomeController's CinemachineVirtualCameras property is null.</color>", this, biome);
		}
	}

	private void SetOffset(CinemachineVirtualCamera virtualCamera)
	{
		CinemachineFramingTransposer cinemachineComponent = virtualCamera.GetCinemachineComponent<CinemachineFramingTransposer>();
		cinemachineComponent.m_ScreenX = m_offsetOverride.x;
		cinemachineComponent.m_ScreenY = m_offsetOverride.y;
	}

	public override void UndoRule(BiomeType biome)
	{
	}
}
