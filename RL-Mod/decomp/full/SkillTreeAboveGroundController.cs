using System.Collections;
using RL_Windows;
using UnityEngine;

public class SkillTreeAboveGroundController : MonoBehaviour
{
	[SerializeField]
	private GameObject m_skillTreeCastle_SurfaceGrass;

	[SerializeField]
	private GameObject m_skillTreeCastle_SurfaceRock;

	private Weather m_weather;

	private void OnEnable()
	{
		bool flag = false;
		if (PlayerManager.IsInstantiated && (bool)PlayerManager.GetCurrentPlayerRoom())
		{
			EndingSpawnRoomTypeController component = PlayerManager.GetCurrentPlayerRoom().GetComponent<EndingSpawnRoomTypeController>();
			if ((bool)component && component.EndingSpawnRoomType == EndingSpawnRoomType.AboveGround)
			{
				flag = true;
			}
		}
		if (flag)
		{
			if (!m_skillTreeCastle_SurfaceGrass.activeSelf)
			{
				m_skillTreeCastle_SurfaceGrass.SetActive(value: true);
			}
			if (!m_skillTreeCastle_SurfaceRock.activeSelf)
			{
				m_skillTreeCastle_SurfaceRock.SetActive(value: true);
			}
			if (WindowManager.GetIsWindowOpen(WindowID.ConfirmMenuBig))
			{
				WindowManager.GetWindowController(WindowID.ConfirmMenuBig).WindowCanvas.sortingOrder = 30;
			}
			StartCoroutine(SetWeatherCoroutine());
		}
		else
		{
			if (m_skillTreeCastle_SurfaceGrass.activeSelf)
			{
				m_skillTreeCastle_SurfaceGrass.SetActive(value: false);
			}
			if (m_skillTreeCastle_SurfaceRock.activeSelf)
			{
				m_skillTreeCastle_SurfaceRock.SetActive(value: false);
			}
		}
	}

	private IEnumerator SetWeatherCoroutine()
	{
		float delay = 0.1f + Time.time;
		while (Time.time < delay)
		{
			yield return null;
		}
		if (!m_weather)
		{
			m_weather = Object.FindObjectOfType<Weather>();
		}
		if ((bool)m_weather)
		{
			m_weather.gameObject.SetLayerRecursively(5);
		}
	}

	private void OnDisable()
	{
		bool flag = false;
		if (PlayerManager.IsInstantiated && (bool)PlayerManager.GetCurrentPlayerRoom())
		{
			EndingSpawnRoomTypeController component = PlayerManager.GetCurrentPlayerRoom().GetComponent<EndingSpawnRoomTypeController>();
			if ((bool)component && component.EndingSpawnRoomType == EndingSpawnRoomType.AboveGround)
			{
				flag = true;
			}
		}
		if (flag && (bool)m_weather)
		{
			m_weather.gameObject.SetLayerRecursively(0);
		}
	}
}
