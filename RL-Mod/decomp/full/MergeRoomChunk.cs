using Sigtrap.Relays;
using UnityEngine;

public class MergeRoomChunk : MonoBehaviour
{
	private Relay<Vector2> m_playerEnteredChunkRelay = new Relay<Vector2>();

	public IRelayLink<Vector2> PlayerEnteredChunkRelay => m_playerEnteredChunkRelay;

	private void OnTriggerEnter2D(Collider2D collision)
	{
		if (collision.gameObject.CompareTag("Player"))
		{
			m_playerEnteredChunkRelay.Dispatch(base.transform.position);
		}
	}
}
