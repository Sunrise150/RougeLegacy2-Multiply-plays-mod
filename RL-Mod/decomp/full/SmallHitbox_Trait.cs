using System.Collections;
using UnityEngine;

public class SmallHitbox_Trait : BaseTrait
{
	[SerializeField]
	private GameObject m_traitSprite;

	private Vector2 m_storedBodyColliderSize;

	private bool m_traitInitialized;

	public override TraitType TraitType => TraitType.SmallHitbox;

	private IEnumerator Start()
	{
		while (!PlayerManager.IsInstantiated)
		{
			yield return null;
		}
		PlayerController playerController = PlayerManager.GetPlayerController();
		playerController.ControllerCorgi.SetRaysParameters();
		Vector3 position = new Vector3(playerController.Midpoint.x, playerController.Midpoint.y, m_traitSprite.transform.localPosition.z);
		m_traitSprite.transform.position = position;
		m_traitSprite.transform.SetParent(playerController.Visuals.transform, worldPositionStays: true);
		BoxCollider2D boxCollider2D = (BoxCollider2D)playerController.HitboxController.GetCollider(HitboxType.Body);
		m_storedBodyColliderSize = boxCollider2D.size;
		boxCollider2D.size = m_storedBodyColliderSize * 0.1f;
		playerController.InitializeHealthMods();
		playerController.ResetHealth();
		playerController.BlinkPulseEffect.ActivateBlackFill(BlackFillType.SmallHitbox_Trait, 0f);
		m_traitInitialized = true;
	}

	private void OnDisable()
	{
		if (!PlayerManager.IsDisposed && (bool)PlayerManager.GetPlayerController())
		{
			PlayerManager.GetPlayerController().BlinkPulseEffect.DisableBlackFill(BlackFillType.SmallHitbox_Trait, 0f);
		}
	}

	private void OnDestroy()
	{
		Object.DestroyImmediate(m_traitSprite);
		if (!PlayerManager.IsDisposed && m_traitInitialized)
		{
			PlayerController playerController = PlayerManager.GetPlayerController();
			playerController.RecreateRendererArray();
			((BoxCollider2D)playerController.HitboxController.GetCollider(HitboxType.Body)).size = m_storedBodyColliderSize;
		}
	}
}
