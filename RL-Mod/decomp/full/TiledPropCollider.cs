using System;
using UnityEngine;

public class TiledPropCollider : MonoBehaviour, IRoomConsumer
{
	[SerializeField]
	private BoxCollider2D m_collider;

	private SpriteRenderer m_spriteRenderer;

	public BaseRoom Room { get; private set; }

	private void Awake()
	{
		m_spriteRenderer = GetComponentInChildren<SpriteRenderer>();
	}

	public void SetRoom(BaseRoom room)
	{
		Room = room;
		if (Room != null)
		{
			Room.PlayerEnterRelay.AddListener(OnPlayerEnterRoom);
		}
	}

	private void OnDestroy()
	{
		if (Room != null)
		{
			Room.PlayerEnterRelay.RemoveListener(OnPlayerEnterRoom);
		}
	}

	private void OnPlayerEnterRoom(object sender, EventArgs args)
	{
		m_collider.size = m_spriteRenderer.size;
		m_collider.offset = new Vector2(0f, m_spriteRenderer.size.y / 2f);
	}
}
