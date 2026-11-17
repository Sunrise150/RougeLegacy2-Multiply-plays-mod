using UnityEngine;

public class ClownNPCController : MonoBehaviour, IRoomConsumer
{
	private Interactable m_interact;

	private BaseRoom m_room;

	public BaseRoom Room => m_room;

	public void SetRoom(BaseRoom room)
	{
		m_room = room;
		ClownEntranceRoomController component = m_room.gameObject.GetComponent<ClownEntranceRoomController>();
		if (component != null)
		{
			m_interact.TriggerOnEnterEvent.AddListener(component.TriggerDialogue);
		}
	}

	private void Awake()
	{
		m_interact = GetComponent<Interactable>();
	}

	private void OnDestroy()
	{
		if (m_room != null)
		{
			ClownEntranceRoomController component = m_room.gameObject.GetComponent<ClownEntranceRoomController>();
			if (component != null && m_interact != null)
			{
				m_interact.TriggerOnEnterEvent.RemoveListener(component.TriggerDialogue);
			}
		}
	}
}
