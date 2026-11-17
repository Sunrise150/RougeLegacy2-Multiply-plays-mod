using System.Collections;
using UnityEngine;

public class ExtendSpriteToPlatform : MonoBehaviour
{
	public enum ExtendSpriteDirection
	{
		None,
		Upward,
		Downward
	}

	[SerializeField]
	private SpriteRenderer m_spriteToExtend;

	[SerializeField]
	private bool m_collideWithOneWays;

	[SerializeField]
	private ExtendSpriteDirection m_extendSpriteDirection;

	private EnemyController m_enemyController;

	private void Awake()
	{
		m_enemyController = GetComponent<EnemyController>();
	}

	private void OnEnable()
	{
		StartCoroutine(ExtendSprite());
	}

	private IEnumerator ExtendSprite()
	{
		m_spriteToExtend.size = new Vector2(m_spriteToExtend.size.x, 0f);
		while (!PlayerManager.IsInstantiated)
		{
			yield return null;
		}
		while (!m_enemyController.IsInitialized)
		{
			yield return null;
		}
		yield return null;
		PlayerController playerController = PlayerManager.GetPlayerController();
		LayerMask layerMask = playerController.ControllerCorgi.SavedPlatformMask;
		if (m_collideWithOneWays)
		{
			layerMask = (int)layerMask | (int)playerController.ControllerCorgi.OneWayPlatformMask;
		}
		Vector2 direction = Vector2.zero;
		switch (m_extendSpriteDirection)
		{
		case ExtendSpriteDirection.Upward:
			direction = Vector2.up;
			break;
		case ExtendSpriteDirection.Downward:
			direction = Vector2.down;
			break;
		}
		RaycastHit2D raycastHit2D = Physics2D.Raycast(base.transform.position, direction, 20f, layerMask);
		if ((bool)raycastHit2D)
		{
			Vector2 size = m_spriteToExtend.size;
			size.y = raycastHit2D.distance * direction.y;
			m_spriteToExtend.size = size;
		}
	}
}
