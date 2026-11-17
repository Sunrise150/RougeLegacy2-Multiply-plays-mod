using UnityEngine;

public class PlayerSpawn : MonoBehaviour, IPlayerSpawn
{
	[SerializeField]
	private SpriteRenderer m_sprite;

	private void Start()
	{
		m_sprite.enabled = false;
	}

	public void SpawnPlayer()
	{
		GameObject player = PlayerManager.GetPlayer();
		player.SetActive(value: true);
		player.transform.SetParent(null);
		player.transform.position = base.transform.position;
	}
}
