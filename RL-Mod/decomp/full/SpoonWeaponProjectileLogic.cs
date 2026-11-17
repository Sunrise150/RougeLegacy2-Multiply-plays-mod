using System;
using UnityEngine;

public class SpoonWeaponProjectileLogic : BaseProjectileLogic, IHasProjectileNameArray
{
	[SerializeField]
	private string m_projectileToSpawn;

	[SerializeField]
	private Vector2 m_offset;

	[NonSerialized]
	protected string[] m_projectileNameArray;

	public virtual string[] ProjectileNameArray
	{
		get
		{
			if (m_projectileNameArray == null)
			{
				m_projectileNameArray = new string[1] { m_projectileToSpawn };
			}
			return m_projectileNameArray;
		}
	}

	public string ProjectileToSpawn
	{
		get
		{
			return m_projectileToSpawn;
		}
		set
		{
			m_projectileToSpawn = value;
		}
	}

	public Vector2 Offset
	{
		get
		{
			return m_offset;
		}
		set
		{
			m_offset = value;
		}
	}

	private void Start()
	{
		base.SourceProjectile.OnDeathRelay.AddListener(SpawnProjectile);
	}

	private void SpawnProjectile(Projectile_RL projectile, GameObject colliderObj)
	{
		if (!base.enabled || base.SourceProjectile.LifespanTimer <= 0f)
		{
			return;
		}
		bool flag = true;
		if ((bool)colliderObj && !colliderObj.CompareTag("Platform") && !colliderObj.CompareTag("OneWay"))
		{
			flag = false;
		}
		Vector3 vector = base.SourceProjectile.Midpoint;
		if (base.SourceProjectile.HitboxController.LastCollidedWith != null)
		{
			vector = base.SourceProjectile.HitboxController.LastCollidedWith.ClosestPoint(base.SourceProjectile.Midpoint);
		}
		if (!flag)
		{
			return;
		}
		Vector2 velocity = base.SourceProjectile.Velocity;
		velocity.Normalize();
		Vector2 vector2 = base.SourceProjectile.Midpoint - vector;
		vector2.Normalize();
		Vector2 pt;
		if (vector2 == Vector2.zero)
		{
			pt = velocity;
			pt.x *= -1f;
		}
		else
		{
			Vector2 vector3 = new Vector2(vector2.y, vector2.x * -1f);
			pt = 2f * (Vector2.Dot(velocity, vector3) / Vector2.Dot(vector3, vector3)) * vector3 - velocity;
			if ((pt.y > 0f && velocity.y > 0f && vector2.x > 0f && velocity.x > 0f) || (pt.y > 0f && velocity.y > 0f && vector2.x < 0f && velocity.x < 0f))
			{
				pt.y *= -1f;
			}
		}
		pt.Normalize();
		float angleInDeg = CDGHelper.VectorToAngle(pt);
		Vector2 offset = base.SourceProjectile.Midpoint;
		Projectile_RL projectile_RL = ProjectileManager.FireProjectile(base.SourceProjectile.Owner, m_projectileToSpawn, offset, matchFacing: false, angleInDeg, 0.8f, useAbsPos: true);
		PlayerController playerController = base.SourceProjectile.OwnerController as PlayerController;
		if ((bool)playerController)
		{
			CastAbilityType lastCastAbilityTypeCasted = playerController.CastAbility.LastCastAbilityTypeCasted;
			playerController.CastAbility.SetLastCastAbilityTypeOverride(base.SourceProjectile.CastAbilityType);
			playerController.CastAbility.InitializeProjectile(projectile_RL);
			playerController.CastAbility.SetLastCastAbilityTypeOverride(lastCastAbilityTypeCasted);
			if (!projectile_RL.HitboxController.ResponseMethodsInitialized && (bool)colliderObj)
			{
				projectile_RL.ForceDispatchOnCollisionRelay(colliderObj);
			}
		}
		projectile_RL.CastAbilityType = base.SourceProjectile.CastAbilityType;
		projectile_RL.Strength = base.SourceProjectile.Strength;
		projectile_RL.Magic = base.SourceProjectile.Magic;
		projectile_RL.DamageMod = base.SourceProjectile.DamageMod;
		projectile_RL.ActualCritChance = base.SourceProjectile.ActualCritChance;
		projectile_RL.transform.localPosition += (Vector3)(vector2 * 0.5f);
		Vector2 offset2 = Offset;
		if (base.SourceProjectile.IsFlipped)
		{
			offset2.x = 0f - offset2.x;
		}
		projectile_RL.transform.localPosition += (Vector3)offset2;
		if (base.SourceProjectile.StatusEffectTypes != null)
		{
			StatusEffectType[] statusEffectTypes = base.SourceProjectile.StatusEffectTypes;
			foreach (StatusEffectType type in statusEffectTypes)
			{
				projectile_RL.AttachStatusEffect(type);
			}
		}
		projectile_RL.StopChangeCollisionPointCoroutine();
	}

	private void OnDestroy()
	{
		base.SourceProjectile.OnDeathRelay.RemoveListener(SpawnProjectile);
	}
}
