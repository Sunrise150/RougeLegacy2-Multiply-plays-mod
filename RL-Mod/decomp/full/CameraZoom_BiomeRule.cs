using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(menuName = "Custom/Biome Rules/Camera Zoom")]
public class CameraZoom_BiomeRule : BiomeRule
{
	[SerializeField]
	private float m_zoomLevel = 1f;

	private float m_previousZoomLevel;

	public override BiomeRuleExecutionTime ExecutionTime => BiomeRuleExecutionTime.PlayerEnterBiome;

	public override IEnumerator RunRule(BiomeType biome)
	{
		if (GameUtility.IsInLevelEditor && (bool)OnPlayManager.BiomeController)
		{
			CameraZoomController component = OnPlayManager.CurrentRoom.GetComponent<CameraZoomController>();
			if ((bool)component)
			{
				m_previousZoomLevel = component.ZoomLevel;
				component.SetZoomLevel(m_zoomLevel);
			}
			yield break;
		}
		BiomeController biomeController = WorldBuilder.GetBiomeController(BiomeType_RL.GetGroupedBiomeType(biome));
		if (!biomeController)
		{
			yield break;
		}
		List<BaseRoom> list = ((biome == BiomeType.TowerExterior) ? biomeController.Rooms.Where((BaseRoom room) => room.AppearanceBiomeType == BiomeType.TowerExterior).ToList() : biomeController.Rooms.Where((BaseRoom room) => room.AppearanceBiomeType == biome).ToList());
		foreach (BaseRoom item in list)
		{
			CameraZoomController component2 = item.gameObject.GetComponent<CameraZoomController>();
			if ((bool)component2)
			{
				component2.SetZoomLevel(m_zoomLevel);
			}
		}
	}

	public override void UndoRule(BiomeType biome)
	{
		if (GameUtility.IsInLevelEditor && (bool)OnPlayManager.CurrentRoom)
		{
			CameraZoomController component = OnPlayManager.CurrentRoom.GetComponent<CameraZoomController>();
			if ((bool)component && component.OverrideZoomLevel)
			{
				component.SetZoomLevel(m_previousZoomLevel);
			}
		}
	}
}
