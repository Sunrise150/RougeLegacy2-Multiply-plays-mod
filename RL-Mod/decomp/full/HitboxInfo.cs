using System;
using UnityEngine;

[Serializable]
public class HitboxInfo : MonoBehaviour
{
	public Collider2D Collider { get; private set; }

	public CollisionType CollidesWithType { get; set; }

	public IHitboxController HitboxController { get; set; }

	public GameObject RootGameObj => HitboxController.RootGameObject;

	public void SetCollider(Collider2D collider)
	{
		Collider = collider;
	}

	private void OnTriggerEnter2D(Collider2D otherCollider)
	{
		OnTriggerHandler(HitResponseType.OnEnter, otherCollider);
	}

	private void OnTriggerStay2D(Collider2D otherCollider)
	{
		OnTriggerHandler(HitResponseType.OnStay, otherCollider);
	}

	private void OnTriggerExit2D(Collider2D otherCollider)
	{
		OnTriggerHandler(HitResponseType.OnExit, otherCollider);
	}

	private void OnTriggerHandler(HitResponseType hitResponseType, Collider2D otherCollider)
	{
		IHitboxController hitboxController = HitboxController;
		if (hitboxController != null && !hitboxController.DisableAllCollisions && CDGHelper.DoCollisionTypesCollide_V2(CollidesWithType, otherCollider.gameObject) && (!otherCollider.CompareTag("Generic_Bounceable") || SaveManager.PlayerSaveData.GetHeirloomLevel(HeirloomType.UnlockBouncableDownstrike) > 0))
		{
			hitboxController.HandleCollision(hitResponseType, this, otherCollider);
		}
	}
}
