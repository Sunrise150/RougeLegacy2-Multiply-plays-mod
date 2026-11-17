using System;
using System.Collections;
using System.Collections.Generic;
using FMOD.Studio;
using FMODUnity;
using RLAudio;
using UnityEngine;

public class LuteWeaponProjectileLogic : BaseProjectileLogic, IBodyOnEnterHitResponse, IHitResponse, IBodyOnStayHitResponse, IHasProjectileNameArray
{
	[SerializeField]
	[EventRef]
	private string m_bounceAudioPath = string.Empty;

	private static HashSet<LuteWeaponProjectileLogic> m_luteLogicHashset_STATIC = new HashSet<LuteWeaponProjectileLogic>();

	private static List<Projectile_RL> m_maxProjectileList_STATIC = new List<Projectile_RL>();

	[SerializeField]
	private string m_projectileToSpawn;

	[SerializeField]
	private bool m_destroyOnKick;

	[SerializeField]
	private bool m_explodeOnKick;

	[SerializeField]
	private float m_collisionDelayTime;

	[NonSerialized]
	protected string[] m_projectileNameArray;

	private static EventInstance m_bounceEventInstance = default(EventInstance);

	private static int m_bounceCount = 0;

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

	protected override void Awake()
	{
		base.Awake();
		if (!m_bounceEventInstance.isValid())
		{
			m_bounceEventInstance = AudioUtility.GetEventInstance(m_bounceAudioPath, base.transform);
		}
	}

	private void OnEnable()
	{
		StartCoroutine(EnableBodyHitboxCoroutine());
		StartCoroutine(MaxProjectileCheckCoroutine());
	}

	private IEnumerator MaxProjectileCheckCoroutine()
	{
		yield return null;
		if (base.SourceProjectile.CastAbilityType != 0)
		{
			yield break;
		}
		m_maxProjectileList_STATIC.Add(base.SourceProjectile);
		if (m_maxProjectileList_STATIC.Count > 3)
		{
			if ((bool)m_maxProjectileList_STATIC[0])
			{
				m_maxProjectileList_STATIC[0].FlagForDestruction();
			}
			m_maxProjectileList_STATIC.RemoveAt(0);
		}
	}

	private void OnDisable()
	{
		if (base.SourceProjectile.CastAbilityType == CastAbilityType.Weapon)
		{
			int num = m_maxProjectileList_STATIC.IndexOf(base.SourceProjectile);
			if (num != -1)
			{
				m_maxProjectileList_STATIC.RemoveAt(num);
			}
		}
	}

	private void OnDestroy()
	{
		if (m_maxProjectileList_STATIC.Count > 0)
		{
			m_maxProjectileList_STATIC.Clear();
		}
		if (m_bounceEventInstance.isValid())
		{
			m_bounceEventInstance.release();
		}
	}

	private IEnumerator EnableBodyHitboxCoroutine()
	{
		base.SourceProjectile.HitboxController.SetHitboxActiveState(HitboxType.Body, active: false);
		float delayTime = Time.time + m_collisionDelayTime;
		while (Time.time < delayTime)
		{
			yield return null;
		}
		base.SourceProjectile.HitboxController.SetHitboxActiveState(HitboxType.Body, active: true);
	}

	public void BodyOnEnterHitResponse(IHitboxController otherHBController)
	{
		if (base.enabled && otherHBController.CollisionType == CollisionType.PlayerProjectile && (bool)otherHBController.RootGameObject.GetComponent<DownstrikeProjectile_RL>())
		{
			m_luteLogicHashset_STATIC.Add(this);
		}
	}

	public void BodyOnStayHitResponse(IHitboxController otherHBController)
	{
		BodyOnEnterHitResponse(otherHBController);
	}

	private void LateUpdate()
	{
		if (PlayerManager.IsInstantiated && PlayerManager.GetPlayerController().IsGrounded)
		{
			m_bounceCount = 0;
		}
		if (m_luteLogicHashset_STATIC.Count <= 0)
		{
			return;
		}
		LuteWeaponProjectileLogic luteWeaponProjectileLogic = null;
		PlayerController playerController = PlayerManager.GetPlayerController();
		float num = float.MaxValue;
		foreach (LuteWeaponProjectileLogic item in m_luteLogicHashset_STATIC)
		{
			float num2 = CDGHelper.DistanceBetweenPts(playerController.Midpoint, item.SourceProjectile.Midpoint);
			if (num2 < num)
			{
				num = num2;
				luteWeaponProjectileLogic = item;
			}
		}
		if ((bool)luteWeaponProjectileLogic)
		{
			Projectile_RL sourceProjectile = luteWeaponProjectileLogic.SourceProjectile;
			if (luteWeaponProjectileLogic.m_destroyOnKick)
			{
				luteWeaponProjectileLogic.SourceProjectile.FlagForDestruction(playerController.gameObject);
			}
			if (luteWeaponProjectileLogic.m_explodeOnKick)
			{
				Projectile_RL projectile_RL = ProjectileManager.FireProjectile(sourceProjectile.Owner, luteWeaponProjectileLogic.m_projectileToSpawn, sourceProjectile.Midpoint, matchFacing: false, 0f, 1f, useAbsPos: true);
				playerController.CastAbility.InitializeProjectile(projectile_RL);
				projectile_RL.CastAbilityType = sourceProjectile.CastAbilityType;
				projectile_RL.Strength = sourceProjectile.Strength;
				projectile_RL.Magic = sourceProjectile.Magic;
				projectile_RL.DamageMod = sourceProjectile.DamageMod;
				projectile_RL.ActualCritChance = sourceProjectile.ActualCritChance;
				m_bounceCount++;
				if (m_bounceEventInstance.isValid())
				{
					float playerGenderAudioParameterValue = AudioUtility.GetPlayerGenderAudioParameterValue();
					m_bounceEventInstance.setParameterByName("gender", playerGenderAudioParameterValue);
					m_bounceCount = Mathf.Clamp(m_bounceCount, 1, 8);
					m_bounceEventInstance.setParameterByName("bardNoteBounce", m_bounceCount);
					AudioManager.PlayAttached(null, m_bounceEventInstance, PlayerManager.GetPlayer());
				}
				projectile_RL.OnCollisionRelay.AddListener(OnExplosionCollision);
			}
		}
		m_luteLogicHashset_STATIC.Clear();
	}

	private static void OnExplosionCollision(Projectile_RL projectile, GameObject colliderObj)
	{
		if (PlayerManager.GetPlayerController().CharacterClass.ClassType == ClassType.LuteClass && colliderObj.CompareTag("Enemy"))
		{
			PlayerManager.GetPlayerController().StatusEffectController.StartStatusEffect(StatusEffectType.Player_Dance, 0f, projectile);
		}
	}
}
