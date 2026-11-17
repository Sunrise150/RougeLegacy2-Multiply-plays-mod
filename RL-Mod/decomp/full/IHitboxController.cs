using UnityEngine;

public interface IHitboxController
{
	GameObject gameObject { get; }

	GameObject RootGameObject { get; }

	bool IsInitialized { get; }

	IDamageObj DamageObj { get; }

	CollisionType CollisionType { get; set; }

	CollisionType WeaponCollidesWithType { get; }

	CollisionType TerrainCollidesWithType { get; }

	float RepeatHitDuration { get; set; }

	bool DisableAllCollisions { get; set; }

	Collider2D LastCollidedWith { get; set; }

	bool ResponseMethodsInitialized { get; }

	void HandleCollision(HitResponseType hitResponseType, HitboxInfo hbInfo, Collider2D otherCollider);

	void InvokeAllBodyHitResponseMethods(HitResponseType hitResponseType, IHitboxController otherHBController);

	void InvokeAllWeaponHitResponseMethods(HitResponseType hitResponseType, IHitboxController otherHBController);

	void InvokeAllTerrainHitResponseMethods(HitResponseType hitResponseType, IHitboxController otherHBController);

	void AddHitboxResponseMethod(IHitResponse hitResponse);

	void RemoveHitboxResponseMethod(IHitResponse hitResponse);

	GameObject ContainsHitbox(HitboxType hitboxType, string hitboxName);

	void SetHitboxActiveState(HitboxType hitboxType, bool active);

	bool GetHitboxActiveState(HitboxType hitboxType);

	Collider2D GetCollider(HitboxType hitboxType);

	void ChangeCollisionType(HitboxType hitboxType, CollisionType collisionType);

	void ChangeCanCollideWith(HitboxType hitboxType, CollisionType collisionType);

	void RealignHitboxColliders();

	void Initialize();

	void ResetRepeatHitChecks();

	void RemoveFromRepeatHitChecks(GameObject obj);

	void ResetAllHitboxStates();

	void SetCulledState(bool cull, bool includeRigidbody);
}
