using System;
using UnityEngine;
using UnityEngine.Events;

public class OnCollisionSpawnProjectileLogic : BaseProjectileLogic, IHasProjectileNameArray
{
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
	private bool m_onlyCollideWithProjectiles;

	[SerializeField]
	private UnityEvent m_projectileSpawnedUnityEvent;

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
		base.SourceProjectile.OnCollisionRelay.AddListener(SpawnProjectile);
	}

	protected virtual void SpawnProjectile(Projectile_RL projectile, GameObject colliderObj)
	{
		if (m_onlyCollideWithProjectiles && !CollisionType_RL.IsProjectile(colliderObj))
		{
			return;
		}
		if (m_projectileSpawnedUnityEvent != null)
		{
			m_projectileSpawnedUnityEvent.Invoke();
		}
		Vector2 offset = base.SourceProjectile.Midpoint;
		Projectile_RL projectile_RL = ProjectileManager.FireProjectile(base.SourceProjectile.Owner, m_projectileToSpawn, offset, MatchFacing, Angle, 1f, useAbsPos: true);
		PlayerController playerController = base.SourceProjectile.OwnerController as PlayerController;
		if ((bool)playerController)
		{
			bool num = playerController.StatusEffectController.HasStatusEffect(StatusEffectType.Player_FreeCrit);
			float duration = 0f;
			if (num)
			{
				duration = playerController.StatusEffectController.GetStatusEffect(StatusEffectType.Player_FreeCrit).EndTime - Time.time;
				playerController.StatusEffectController.StopStatusEffect(StatusEffectType.Player_FreeCrit, interrupted: true);
			}
			CastAbilityType lastCastAbilityTypeCasted = playerController.CastAbility.LastCastAbilityTypeCasted;
			playerController.CastAbility.SetLastCastAbilityTypeOverride(base.SourceProjectile.CastAbilityType);
			playerController.CastAbility.InitializeProjectile(projectile_RL);
			playerController.CastAbility.SetLastCastAbilityTypeOverride(lastCastAbilityTypeCasted);
			if (num)
			{
				playerController.StatusEffectController.StartStatusEffect(StatusEffectType.Player_FreeCrit, duration, null);
			}
			if (!projectile_RL.HitboxController.ResponseMethodsInitialized && (bool)colliderObj)
			{
				projectile_RL.ForceDispatchOnCollisionRelay(colliderObj);
			}
		}
		projectile_RL.CastAbilityType = base.SourceProjectile.CastAbilityType;
		if (m_spawnAtCollisionPoint)
		{
			Vector3 localPosition = base.SourceProjectile.Midpoint;
			if ((bool)base.SourceProjectile.HitboxController.LastCollidedWith)
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
		base.SourceProjectile.OnCollisionRelay.RemoveListener(SpawnProjectile);
	}
}
