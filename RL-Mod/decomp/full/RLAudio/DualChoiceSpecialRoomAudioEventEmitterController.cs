using System;
using FMODUnity;
using UnityEngine;
using UnityEngine.Events;

namespace RLAudio;

public class DualChoiceSpecialRoomAudioEventEmitterController : MonoBehaviour, IAudioEventEmitter
{
	[SerializeField]
	[EventRef]
	private string m_leftHoverAudioEventPath;

	[SerializeField]
	[EventRef]
	private string m_rightHoverAudioEventPath;

	[SerializeField]
	[EventRef]
	private string m_leftChosenAudioEventPath;

	[SerializeField]
	[EventRef]
	private string m_rightChosenAudioEventPath;

	[SerializeField]
	[EventRef]
	private string m_roomDiscoveredAudioEventPath;

	[SerializeField]
	[EventRef]
	private string m_roomCompleteAudioEventPath;

	[SerializeField]
	private UnityEvent m_choiceMadeUnityEvent;

	private string m_description = string.Empty;

	private RoomSide m_currentChoice = RoomSide.None;

	private GameObject m_currentChoiceGameObject;

	protected bool m_isRoomComplete;

	private DualChoicePropController m_dualChoiceController;

	private BaseSpecialRoomController m_specialRoomController;

	private Room m_room;

	private Action<GameObject> m_onLeftChoiceHover;

	private Action<GameObject> m_onRightChoiceHover;

	public string Description
	{
		get
		{
			if (m_description == string.Empty)
			{
				m_description = ToString();
			}
			return m_description;
		}
	}

	private void Awake()
	{
		m_room = GetComponent<Room>();
		m_room.PlayerEnterRelay.AddListener(OnPlayerEnterRoom);
		m_specialRoomController = GetComponent<BaseSpecialRoomController>();
		m_specialRoomController.RoomCompletedRelay.AddListener(OnRoomComplete);
		m_onLeftChoiceHover = OnLeftChoiceHover;
		m_onRightChoiceHover = OnRightChoiceHover;
	}

	protected virtual void OnPlayerEnterRoom(object sender, RoomViaDoorEventArgs eventArgs)
	{
		if (!m_dualChoiceController)
		{
			PropSpawnController[] propSpawnControllers = m_room.SpawnControllerManager.PropSpawnControllers;
			for (int i = 0; i < propSpawnControllers.Length; i++)
			{
				if (!propSpawnControllers[i].ShouldSpawn || propSpawnControllers[i].PropInstance == null)
				{
					continue;
				}
				m_dualChoiceController = propSpawnControllers[i].PropInstance.gameObject.GetComponent<DualChoicePropController>();
				if (!m_dualChoiceController)
				{
					continue;
				}
				Interactable[] componentsInChildren = propSpawnControllers[i].PropInstance.gameObject.GetComponentsInChildren<Interactable>();
				for (int j = 0; j < componentsInChildren.Length; j++)
				{
					if (!componentsInChildren[j].TriggerOnTouch)
					{
						componentsInChildren[j].m_triggerOnEnterEvent.AddListener(OnChoiceMade);
					}
				}
				break;
			}
		}
		if ((bool)m_dualChoiceController)
		{
			m_dualChoiceController.LeftHoverRelay.AddListener(m_onLeftChoiceHover);
			m_dualChoiceController.RightHoverRelay.AddListener(m_onRightChoiceHover);
		}
		if (!m_isRoomComplete)
		{
			AudioManager.PlayOneShot(this, m_roomDiscoveredAudioEventPath);
		}
	}

	private void OnDisable()
	{
		if ((bool)m_dualChoiceController)
		{
			m_dualChoiceController.LeftHoverRelay.RemoveListener(m_onLeftChoiceHover);
			m_dualChoiceController.RightHoverRelay.RemoveListener(m_onRightChoiceHover);
		}
	}

	private void OnChoiceMade(GameObject gameObj)
	{
		if (m_choiceMadeUnityEvent != null)
		{
			m_choiceMadeUnityEvent.Invoke();
		}
		string path = m_leftChosenAudioEventPath;
		if (m_currentChoice == RoomSide.Right)
		{
			path = m_rightChosenAudioEventPath;
		}
		PlayChoiceAudio(path, gameObj.transform.position);
	}

	private void OnLeftChoiceHover(GameObject gameObj)
	{
		m_currentChoice = RoomSide.Left;
		m_currentChoiceGameObject = gameObj;
		PlayChoiceAudio(m_rightHoverAudioEventPath, gameObj.transform.position);
	}

	private void OnRightChoiceHover(GameObject gameObj)
	{
		m_currentChoice = RoomSide.Right;
		m_currentChoiceGameObject = gameObj;
		PlayChoiceAudio(m_rightHoverAudioEventPath, gameObj.transform.position);
	}

	private void PlayChoiceAudio(string path, Vector3 position)
	{
		AudioManager.PlayOneShot(this, path, position);
	}

	protected virtual void OnRoomComplete()
	{
		AudioManager.PlayOneShot(this, m_roomCompleteAudioEventPath);
		m_isRoomComplete = true;
	}
}
