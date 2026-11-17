using System.Collections;
using UnityEngine;

public class EasyBreakables_Trait : BaseTrait
{
	private GameObject m_breakablesHitbox;

	public override TraitType TraitType => TraitType.EasyBreakables;

	public IEnumerator Start()
	{
		if (!PlayerManager.IsInstantiated)
		{
			yield return new WaitUntil(() => PlayerManager.IsInstantiated);
		}
		IHitboxController hbController = PlayerManager.GetPlayerController().HitboxController;
		if (!hbController.IsInitialized)
		{
			yield return new WaitUntil(() => hbController.IsInitialized);
		}
		Collider2D collider = hbController.GetCollider(HitboxType.Terrain);
		AddBreakablesHitbox(collider, hbController as HitboxController);
	}

	private void AddBreakablesHitbox(Collider2D hbCollider, HitboxController hbController)
	{
		CollisionType collisionType = CollisionType.None;
		collisionType |= CollisionType.FlimsyBreakable;
		collisionType |= CollisionType.Breakable;
		LayerType layerType = LayerType.Weapon_Hitbox;
		GameObject gameObject = new GameObject();
		Collider2D collider = hbController.GetCollider(HitboxType.Body);
		gameObject.transform.SetParent(collider.transform.parent, worldPositionStays: false);
		BoxCollider2D boxCollider2D = (BoxCollider2D)CDGHelper.CopyComponent(hbCollider, gameObject, forceAdd: true);
		Vector2 size = boxCollider2D.size;
		size.x += 1f;
		boxCollider2D.size = size;
		HitboxInfo hitboxInfo = gameObject.AddComponent<HitboxInfo>();
		hitboxInfo.HitboxController = hbController;
		hitboxInfo.CollidesWithType = collisionType;
		TagType equivalentTag = CollisionType_RL.GetEquivalentTag(hbController.CollisionType);
		hitboxInfo.tag = TagType_RL.ToString(equivalentTag);
		hitboxInfo.SetCollider(boxCollider2D);
		gameObject.layer = LayerMask.NameToLayer(LayerType_RL.ToString(layerType));
		gameObject.name = hbCollider.name + " - (EasyBreakable Trait)";
		Rigidbody2D rigidbody2D = gameObject.AddComponent<Rigidbody2D>();
		rigidbody2D.bodyType = RigidbodyType2D.Kinematic;
		rigidbody2D.sleepMode = RigidbodySleepMode2D.NeverSleep;
		m_breakablesHitbox = gameObject;
	}

	private void OnDestroy()
	{
		Object.Destroy(m_breakablesHitbox);
	}
}
