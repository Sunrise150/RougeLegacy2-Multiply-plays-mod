using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterSortController : MonoBehaviour
{
	private const float DEPTH_MULTIPLIER = 1f;

	private const float PLAYER_STARTING_DEPTH_OFFSET = -0.5f;

	private const float ENEMY_STARTING_DEPTH_OFFSET = 0f;

	[SerializeField]
	private float m_depthOverride = -1f;

	private Collider2D m_collider;

	private CameraLayerController m_cameraLayerController;

	private Action<MonoBehaviour, EventArgs> m_onPlayerEnterRoom;

	public int SortIndex { get; private set; }

	public BaseCharacterController CharacterController { get; private set; }

	public int SubLayer => m_cameraLayerController.SubLayer;

	public List<CharacterSortController> SortList { get; private set; } = new List<CharacterSortController>(5);


	public bool IsInitialized { get; private set; }

	public bool IsPlayer { get; private set; }

	private void Awake()
	{
		CharacterController = GetComponentInParent<BaseCharacterController>();
		m_cameraLayerController = CharacterController.GetComponent<CameraLayerController>();
		IsPlayer = CharacterController is PlayerController;
		m_collider = CharacterController.VisualBoundsObj.Collider;
		m_onPlayerEnterRoom = OnPlayerEnterRoom;
	}

	private IEnumerator Start()
	{
		m_collider.gameObject.layer = 29;
		m_collider.isTrigger = true;
		while (!CameraController.IsInstantiated)
		{
			yield return null;
		}
		while (!CharacterController.IsInitialized)
		{
			yield return null;
		}
		ResetCharacterLayers();
		UpdateSortDepth();
		IsInitialized = true;
	}

	private void OnEnable()
	{
		m_collider.enabled = false;
		m_collider.enabled = true;
		if (!SortList.Contains(this))
		{
			SortList.Add(this);
		}
		if (IsPlayer)
		{
			Messenger<GameMessenger, GameEvent>.AddListener(GameEvent.PlayerEnterRoom, m_onPlayerEnterRoom);
		}
	}

	private void OnDisable()
	{
		SortList.Clear();
		SortIndex = 0;
		CharacterController.Visuals.transform.SetLocalPositionZ(0f);
		if (IsPlayer)
		{
			Messenger<GameMessenger, GameEvent>.RemoveListener(GameEvent.PlayerEnterRoom, m_onPlayerEnterRoom);
		}
	}

	private void OnPlayerEnterRoom(object sender, EventArgs args)
	{
		SortList.Clear();
		if (!SortList.Contains(this))
		{
			SortList.Add(this);
		}
	}

	private void OnTriggerEnter2D(Collider2D collider)
	{
		if (!IsPlayer)
		{
			CharacterSortController component = collider.GetComponent<CharacterSortController>();
			if ((bool)component && !SortList.Contains(component))
			{
				SortList.Add(component);
				UpdateSortList();
			}
		}
	}

	private void OnTriggerExit2D(Collider2D collider)
	{
		if (IsPlayer)
		{
			return;
		}
		CharacterSortController component = collider.GetComponent<CharacterSortController>();
		if ((bool)component && SortList.Contains(component))
		{
			SortList.Remove(component);
			if (component.SortIndex < SortIndex)
			{
				SortIndex--;
			}
			if (SortList.Count <= 1)
			{
				SortIndex = 0;
			}
			UpdateSortList();
		}
	}

	public void UpdateSortList()
	{
		SortList.Sort(SortBySubLayer);
		for (int i = 0; i < SortList.Count; i++)
		{
			CharacterSortController characterSortController = SortList[i];
			if (characterSortController.IsPlayer)
			{
				continue;
			}
			if (i > characterSortController.SortIndex)
			{
				characterSortController.SortIndex = i;
			}
			else
			{
				bool flag = false;
				for (int j = 0; j < i; j++)
				{
					if (i == SortList[j].SortIndex)
					{
						flag = true;
						break;
					}
				}
				if (flag)
				{
					characterSortController.SortIndex = i + 1;
				}
			}
			characterSortController.UpdateSortDepth();
		}
	}

	public void UpdateSortDepth()
	{
		float num = ((m_depthOverride != -1f) ? ((float)SortIndex * m_depthOverride) : ((float)SortIndex * 1f));
		num = ((!IsPlayer) ? (num + 0f) : (num + -0.5f));
		if ((bool)CharacterController.Visuals)
		{
			CharacterController.Visuals.transform.SetLocalPositionZ(num);
		}
	}

	private int SortBySubLayer(CharacterSortController a, CharacterSortController b)
	{
		if (a.SubLayer > b.SubLayer || a.IsPlayer)
		{
			return -1;
		}
		if (a.SubLayer < b.SubLayer || b.IsPlayer)
		{
			return 1;
		}
		if (a.SortIndex < b.SortIndex)
		{
			return -1;
		}
		if (a.SortIndex > b.SortIndex)
		{
			return 1;
		}
		return 0;
	}

	public void ResetCharacterLayers()
	{
		int layer = 29;
		for (int i = 0; i < CharacterController.RendererArray.Count; i++)
		{
			CharacterController.RendererArray[i].gameObject.layer = layer;
		}
	}
}
