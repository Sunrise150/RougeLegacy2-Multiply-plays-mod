using UnityEngine;

public class PlayerRoomSpawn : MonoBehaviour
{
	private void Start()
	{
		GetComponent<SpriteRenderer>().enabled = false;
	}
}
