using System;
using System.Collections.Generic;
using UnityEngine;

public class Sky : MonoBehaviour
{
	private const float BG_PERSP_SCALE_OFFSET = 1.5f;

	[SerializeField]
	[ReadOnlyOnPlay]
	private Renderer m_skyRenderer;

	private Vector3 m_storedScale = Vector3.one;

	private static SkyLightChangedEventArgs m_skylightEventArgs;

	private static Sky m_currentSky;

	private Action<MonoBehaviour, EventArgs> m_onEnterLineage;

	private Action<MonoBehaviour, EventArgs> m_onPlayerEnterRoom;

	public List<BaseRoom> RoomList { get; private set; } = new List<BaseRoom>();


	public Renderer SkyRenderer => m_skyRenderer;

	public bool IsHeirloomSky { get; set; }

	private void Awake()
	{
		m_onEnterLineage = OnEnterLineage;
		m_onPlayerEnterRoom = OnPlayerEnterRoom;
		Messenger<SceneMessenger, SceneEvent>.AddListener(SceneEvent.EnterLineageScreen, m_onEnterLineage);
		Messenger<GameMessenger, GameEvent>.AddListener(GameEvent.PlayerEnterRoom, m_onPlayerEnterRoom);
		if ((bool)m_skyRenderer)
		{
			m_storedScale = m_skyRenderer.transform.localScale;
		}
		base.gameObject.SetActive(value: false);
	}

	private void OnDestroy()
	{
		Messenger<SceneMessenger, SceneEvent>.RemoveListener(SceneEvent.EnterLineageScreen, m_onEnterLineage);
		Messenger<GameMessenger, GameEvent>.RemoveListener(GameEvent.PlayerEnterRoom, m_onPlayerEnterRoom);
	}

	private void OnEnterLineage(object sender, EventArgs args)
	{
		if (!base.gameObject.activeSelf)
		{
			base.gameObject.SetActive(value: true);
		}
	}

	private void OnPlayerEnterRoom(MonoBehaviour sender, EventArgs eventArgs)
	{
		if (eventArgs is RoomViaDoorEventArgs { Room: var room })
		{
			if (GetShouldSkyBeActive(room))
			{
				if (!base.gameObject.activeInHierarchy)
				{
					base.gameObject.SetActive(value: true);
				}
				m_currentSky = this;
			}
			else if (base.gameObject.activeInHierarchy)
			{
				base.gameObject.SetActive(value: false);
			}
			if (!base.gameObject.activeInHierarchy || room.AppearanceBiomeType == BiomeType.Lineage)
			{
				return;
			}
			float x = room.Bounds.center.x;
			float y = room.Bounds.center.y;
			base.transform.position = new Vector2(x, y);
			if ((bool)m_skyRenderer)
			{
				m_skyRenderer.transform.localScale = m_storedScale;
				float num = room.Bounds.extents.x / m_skyRenderer.bounds.extents.x;
				float num2 = room.Bounds.extents.y / m_skyRenderer.bounds.extents.y;
				if (num > 1f || num2 > 1f)
				{
					float num3 = ((num > num2) ? num : num2);
					num3 += 0.5f * (CameraController.ZoomLevel - 1f);
					m_skyRenderer.transform.localScale = new Vector3(m_storedScale.x * num3, m_storedScale.y * num3, m_storedScale.z);
				}
			}
		}
		else
		{
			Debug.LogFormat("<color=red>[{0}] Failed to cast eventArgs as RoomViaDoorEventArgs</color>", this);
		}
	}

	private bool GetShouldSkyBeActive(BaseRoom room)
	{
		bool result = false;
		if (RoomList.Contains(room))
		{
			result = true;
		}
		return result;
	}
}
