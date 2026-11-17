using System;
using UnityEngine;

public class OnProjectileDeathProjectileLogic : BaseProjectileLogic, IHasProjectileNameArray
{
	private enum SpawnProjectileCollisionType
	{
		Any,
		PlatformsOnly,
		CharactersOnly
	}

	[SerializeField]
	private string m_projectileToSpawn;

	[SerializeField]
	private Vector2 m_offset;

	[SerializeField]
	private bool m_matchFacing;

	[SerializeField]
	private float m_angle;

	[SerializeField]
	private bool m_spawnAtCollisionPoint;

	[SerializeField]
	private bool m_ignoreTimeoutDeath;

	[SerializeField]
	private SpawnProjectileCollisionType m_spawnCollisionType;

	[SerializeField]
	[Tooltip("Added this specifically for the AstroWand. Allows the spawned projectile to use its own crit chance calculation in the OnEnable method of its ProjectileLogic, rather than being overwritten with its source's crit chance.")]
	private bool m_useSourceProjectileActualCritChance = true;

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

	public bool MatchFacing
	{
		get
		{
			return m_matchFacing;
		}
		set
		{
			m_matchFacing = value;
		}
	}

	public float Angle
	{
		get
		{
			return m_angle;
		}
		set
		{
			m_angle = value;
		}
	}

	private void Start()
	{
		base.SourceProjectile.OnDeathRelay.AddListener(SpawnProjectile);
	}

	protected virtual void SpawnProjectile(Projectile_RL projectile, GameObject colliderObj)
	{
		if (!base.enabled || (m_ignoreTimeoutDeath && base.SourceProjectile.LifespanTimer <= 0f) || (!colliderObj && m_spawnCollisionType == SpawnProjectileCollisionType.CharactersOnly))
		{
			return;
		}
		bool flag = true;
		if ((bool)colliderObj)
		{
			switch (m_spawnCollisionType)
			{
			case SpawnProjectileCollisionType.PlatformsOnly:
				if (!colliderObj.CompareTag("Platform") && !colliderObj.CompareTag("OneWay"))
				{
					flag = false;
				}
				break;
			case SpawnProjectileCollisionType.CharactersOnly:
				if (!colliderObj.CompareTag("Enemy") && !colliderObj.CompareTag("Player") && !colliderObj.CompareTag("Player_Dodging"))
				{
					flag = false;
				}
				break;
			}
		}
		if (!flag)
		{
			return;
		}
		Vector2 offset = base.SourceProjectile.Midpoint;
		Projectile_RL projectile_RL = ProjectileManager.FireProjectile(base.SourceProjectile.Owner, m_projectileToSpawn, offset, MatchFacing, Angle, 1f, useAbsPos: true);
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
		if (m_useSourceProjectileActualCritChance)
		{
			projectile_RL.ActualCritChance = base.SourceProjectile.ActualCritChance;
		}
		if (m_spawnAtCollisionPoint)
		{
			Vector3 localPosition = base.SourceProjectile.Midpoint;
			if (base.SourceProjectile.HitboxController.LastCollidedWith != null)
			{
				localPosition = base.SourceProjectile.HitboxController.LastCollidedWith.ClosestPoint(base.SourceProjectile.Midpoint);
			}
			projectile_RL.transform.localPosition = localPosition;
		}
		else if (base.SourceProjectile.SnapToOwner)
		{
			projectile_RL.transform.localPosition = base.SourceProjectile.transform.position;
		}
		else
		{
			projectile_RL.transform.localPosition = base.SourceProjectile.transform.localPosition;
		}
		Vector2 offset2 = Offset;
		if (MatchFacing && base.SourceProjectile.IsFlipped)
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
