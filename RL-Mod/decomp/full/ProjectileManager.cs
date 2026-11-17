using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ProjectileManager : MonoBehaviour
{
	private const string PROJECTILEMANAGER_NAME = "ProjectileManager";

	private const string RESOURCE_PATH = "Prefabs/Managers/ProjectileManager";

	private const byte OFFSCREEN_ICON_POOLSIZE = 20;

	[SerializeField]
	private OffscreenIconObj m_offscreenIconPrefab;

	private Dictionary<string, GenericPool_RL<Projectile_RL>> m_projectileDict;

	private GenericPool_RL<OffscreenIconObj> m_offscreenIconPool;

	private Action<MonoBehaviour, EventArgs> m_createAbilityPool;

	private Action<MonoBehaviour, EventArgs> m_disableExitRoomProjectiles;

	private static bool m_isInitialized;

	private static ProjectileManager m_projectileManager = null;

	public static int ActiveProjectileCount = 0;

	private static HashSet<string> m_queueProjectileSet = new HashSet<string>();

	public static bool IsInitialized => m_isInitialized;

	public static ProjectileManager Instance
	{
		get
		{
			if (!m_projectileManager)
			{
				m_projectileManager = CDGHelper.FindStaticInstance<ProjectileManager>(createNewIfNull: false);
			}
			return m_projectileManager;
		}
	}

	private void Awake()
	{
		m_createAbilityPool = CreateAbilityPool;
		m_disableExitRoomProjectiles = DisableExitRoomProjectiles;
		Messenger<GameMessenger, GameEvent>.AddListener(GameEvent.ChangeAbility, m_createAbilityPool);
		Initialize();
		SceneManager.sceneLoaded += OnSceneLoaded;
	}

	private void OnSceneLoaded(Scene arg0, LoadSceneMode arg1)
	{
		if (IsInitialized)
		{
			DisableAllProjectiles(flagForDestruction: false);
		}
	}

	private void OnDestroy()
	{
		DestroyBiomePools();
		Messenger<GameMessenger, GameEvent>.RemoveListener(GameEvent.ChangeAbility, m_createAbilityPool);
		Messenger<GameMessenger, GameEvent>.RemoveListener(GameEvent.PlayerExitRoom, m_disableExitRoomProjectiles);
		m_isInitialized = false;
		m_projectileManager = null;
		SceneManager.sceneLoaded -= OnSceneLoaded;
	}

	private void Initialize()
	{
		m_projectileDict = new Dictionary<string, GenericPool_RL<Projectile_RL>>(StringComparer.OrdinalIgnoreCase);
		m_offscreenIconPool = new GenericPool_RL<OffscreenIconObj>();
		m_offscreenIconPool.Initialize(m_offscreenIconPrefab, 20, disablePooling: false);
		Messenger<GameMessenger, GameEvent>.AddListener(GameEvent.PlayerExitRoom, m_disableExitRoomProjectiles);
		m_isInitialized = true;
	}

	public void CreateEnemyProjectilePools(List<EnemyController> enemyPrefabList)
	{
		foreach (EnemyController enemyPrefab in enemyPrefabList)
		{
			BaseAIScript aIScript = EnemyClassLibrary.GetEnemyClassData(enemyPrefab.EnemyType).GetAIScript(enemyPrefab.EnemyRank);
			if ((bool)aIScript && aIScript.ProjectileNameArray != null)
			{
				string[] projectileNameArray = aIScript.ProjectileNameArray;
				foreach (string projectileName in projectileNameArray)
				{
					QueueProjectileToPool(projectileName);
				}
			}
		}
		BaseStatusEffect[] components = EnemyLibrary.GetEnemyPrefab(EnemyType.Skeleton, EnemyRank.Basic).GetComponentInChildren<StatusEffectController>().GetComponents<BaseStatusEffect>();
		foreach (BaseStatusEffect baseStatusEffect in components)
		{
			if (baseStatusEffect.ProjectileNameArray != null)
			{
				string[] projectileNameArray = baseStatusEffect.ProjectileNameArray;
				foreach (string projectileName2 in projectileNameArray)
				{
					QueueProjectileToPool(projectileName2);
				}
			}
		}
	}

	private void CreateAbilityPool(MonoBehaviour sender, EventArgs args)
	{
		ChangeAbilityEventArgs changeAbilityEventArgs = args as ChangeAbilityEventArgs;
		if (changeAbilityEventArgs.Ability == null)
		{
			return;
		}
		BaseAbility_RL ability = AbilityLibrary.GetAbility(changeAbilityEventArgs.Ability.AbilityType);
		if ((bool)ability && ability.ProjectileNameArray != null)
		{
			string[] projectileNameArray = ability.ProjectileNameArray;
			foreach (string projectileName in projectileNameArray)
			{
				AddProjectileToPool(projectileName);
			}
		}
	}

	private void Internal_CreateBiomePools(BiomeType biomeType)
	{
		BiomeController biomeController = null;
		biomeController = ((!GameUtility.IsInLevelEditor || !OnPlayManager.BiomeController) ? WorldBuilder.GetBiomeController(biomeType) : OnPlayManager.BiomeController);
		if (!biomeController)
		{
			return;
		}
		IHasProjectileNameArray[] componentsInChildren = PlayerManager.GetPlayerController().GetComponentsInChildren<IHasProjectileNameArray>(includeInactive: true);
		foreach (IHasProjectileNameArray hasProjectileNameArray in componentsInChildren)
		{
			if (hasProjectileNameArray != null && hasProjectileNameArray.ProjectileNameArray != null)
			{
				string[] projectileNameArray = hasProjectileNameArray.ProjectileNameArray;
				foreach (string projectileName in projectileNameArray)
				{
					QueueProjectileToPool(projectileName);
				}
			}
		}
		foreach (BaseRoom room in biomeController.Rooms)
		{
			ISpawnController[] spawnControllers = room.SpawnControllerManager.SpawnControllers;
			for (int i = 0; i < spawnControllers.Length; i++)
			{
				if (spawnControllers[i] is IHasProjectileNameArray { ProjectileNameArray: not null, ProjectileNameArray: var projectileNameArray2 })
				{
					foreach (string projectileName2 in projectileNameArray2)
					{
						QueueProjectileToPool(projectileName2);
					}
				}
			}
		}
		if (TraitManager.IsTraitActive(TraitType.ExplosiveChests))
		{
			QueueProjectileToPool("ExplosiveChestsPotionProjectile");
			QueueProjectileToPool("ExplosiveChestsPotionExplosionProjectile");
		}
		if (TraitManager.IsTraitActive(TraitType.ExplosiveEnemies))
		{
			QueueProjectileToPool("ExplosiveEnemiesPotionProjectile");
			QueueProjectileToPool("ExplosiveEnemiesPotionExplosionProjectile");
		}
	}

	public void AddProjectileToPool(string projectileName)
	{
		if (string.IsNullOrEmpty(projectileName) || m_projectileDict.ContainsKey(projectileName))
		{
			return;
		}
		ProjectileEntry projectileEntry = ProjectileLibrary.GetProjectileEntry(projectileName);
		if (projectileEntry == null)
		{
			return;
		}
		m_projectileDict.Add(projectileName, CreatePool(projectileEntry.ProjectilePrefab, projectileEntry.PoolSize));
		IHasProjectileNameArray[] componentsInChildren = projectileEntry.ProjectilePrefab.GetComponentsInChildren<IHasProjectileNameArray>();
		foreach (IHasProjectileNameArray hasProjectileNameArray in componentsInChildren)
		{
			if (hasProjectileNameArray.ProjectileNameArray != null)
			{
				string[] projectileNameArray = hasProjectileNameArray.ProjectileNameArray;
				foreach (string projectileName2 in projectileNameArray)
				{
					AddProjectileToPool(projectileName2);
				}
			}
		}
	}

	public static void ClearProjectileQueue()
	{
		m_queueProjectileSet.Clear();
	}

	private void QueueProjectileToPool(string projectileName)
	{
		if (string.IsNullOrEmpty(projectileName) || m_queueProjectileSet.Contains(projectileName))
		{
			return;
		}
		ProjectileEntry projectileEntry = ProjectileLibrary.GetProjectileEntry(projectileName);
		if (projectileEntry == null)
		{
			return;
		}
		m_queueProjectileSet.Add(projectileName);
		IHasProjectileNameArray[] componentsInChildren = projectileEntry.ProjectilePrefab.GetComponentsInChildren<IHasProjectileNameArray>();
		foreach (IHasProjectileNameArray hasProjectileNameArray in componentsInChildren)
		{
			if (hasProjectileNameArray.ProjectileNameArray != null)
			{
				string[] projectileNameArray = hasProjectileNameArray.ProjectileNameArray;
				foreach (string projectileName2 in projectileNameArray)
				{
					QueueProjectileToPool(projectileName2);
				}
			}
		}
	}

	public static void CreatePoolsFromQueuedProjectiles()
	{
		Instance.CreatePoolsFromQueuedProjectiles_Internal();
	}

	private void CreatePoolsFromQueuedProjectiles_Internal()
	{
		List<string> list = m_projectileDict.Keys.ToList();
		int num = 0;
		int num2 = 0;
		foreach (string item in m_queueProjectileSet)
		{
			ProjectileEntry projectileEntry = ProjectileLibrary.GetProjectileEntry(item);
			if (projectileEntry != null)
			{
				list.Remove(item);
				if (!m_projectileDict.ContainsKey(item))
				{
					num++;
					m_projectileDict.Add(item, CreatePool(projectileEntry.ProjectilePrefab, projectileEntry.PoolSize));
				}
				else
				{
					num2++;
				}
			}
		}
		foreach (string item2 in list)
		{
			if (m_projectileDict.ContainsKey(item2))
			{
				m_projectileDict[item2].DestroyPool();
				m_projectileDict.Remove(item2);
			}
		}
	}

	private GenericPool_RL<Projectile_RL> CreatePool(Projectile_RL prefab, int poolSize)
	{
		if (!prefab)
		{
			return null;
		}
		GenericPool_RL<Projectile_RL> genericPool_RL = new GenericPool_RL<Projectile_RL>();
		genericPool_RL.Initialize(prefab, poolSize, disablePooling: false);
		return genericPool_RL;
	}

	private Projectile_RL FireProjectile_Internal(GameObject source, string projectileName, Vector2 positionOffset, bool matchFacing = true, float angleInDeg = 0f, float speedMod = 1f, bool useAbsPos = false, bool playSpawnAudio = true, bool playLifetimeAudio = true, bool playDeathAudio = true)
	{
		GenericPool_RL<Projectile_RL> value = null;
		if (!m_projectileDict.TryGetValue(projectileName, out value))
		{
			throw new Exception("Projectile: " + projectileName + " cannot be found in ProjectileManager.  Please make sure the prefab is added to the Projectile Library prefab. If pooling is on, ensure the projectile name is properly assigned to its respective ProjectileNameArray.");
		}
		BaseCharacterController component = source.GetRoot().GetComponent<BaseCharacterController>();
		Projectile_RL freeObj = value.GetFreeObj();
		if ((bool)freeObj.Animator)
		{
			freeObj.Animator.enabled = false;
		}
		freeObj.gameObject.SetActive(value: true);
		freeObj.MatchFacing = matchFacing;
		freeObj.Owner = source;
		float num = 1f;
		if (freeObj.CompareTag("EnemyProjectile") && !freeObj.IsWeighted)
		{
			speedMod += BurdenManager.GetBurdenStatGain(BurdenType.EnemyProjectiles);
			num += (float)BurdenManager.GetBurdenLevel(BurdenType.EnemyProjectiles) * 0.07000005f;
		}
		freeObj.Speed *= speedMod;
		freeObj.TurnSpeed *= num;
		if ((bool)freeObj.AudioEventEmitter)
		{
			freeObj.AudioEventEmitter.SetPlaySpawnAudio(playSpawnAudio);
			freeObj.AudioEventEmitter.SetPlayLifetimeAudio(playLifetimeAudio);
			freeObj.AudioEventEmitter.SetPlayDeathAudio(playDeathAudio);
		}
		if ((bool)component)
		{
			float num2 = 1f;
			num2 = ((!freeObj.SnapToOwner) ? (component.transform.localScale.x / component.BaseScaleToOffsetWith) : (1f / component.BaseScaleToOffsetWith));
			if (!useAbsPos)
			{
				positionOffset *= num2;
			}
		}
		ApplyProjectileDamage(component, freeObj);
		bool flag = true;
		if (matchFacing && (bool)component)
		{
			flag = component.IsFacingRight;
		}
		if (freeObj.SnapToOwner && !useAbsPos)
		{
			if ((bool)component)
			{
				freeObj.transform.SetParent(component.Pivot.transform, worldPositionStays: false);
			}
			else
			{
				freeObj.transform.SetParent(source.transform, worldPositionStays: false);
			}
			freeObj.transform.localPosition = positionOffset;
			freeObj.transform.localEulerAngles = new Vector3(0f, 0f, angleInDeg);
			freeObj.RotationSpeed = 0f - freeObj.RotationSpeed;
		}
		else
		{
			if (useAbsPos)
			{
				freeObj.transform.localPosition = positionOffset;
			}
			else
			{
				freeObj.transform.localPosition = DetermineSpawnPosition(source, positionOffset, flag);
			}
			if (!flag)
			{
				angleInDeg = 180f - angleInDeg;
				freeObj.Flip();
				freeObj.RotationSpeed = 0f - freeObj.RotationSpeed;
			}
		}
		float num3 = 1f;
		if (freeObj.ScaleWithOwner)
		{
			if ((bool)component)
			{
				num3 = ((!freeObj.SnapToOwner) ? (source.transform.localScale.x / component.BaseScaleToOffsetWith) : (1f / component.BaseScaleToOffsetWith));
			}
			else if (!freeObj.SnapToOwner)
			{
				num3 = source.transform.localScale.x / 1f;
			}
		}
		else if (freeObj.SnapToOwner)
		{
			num3 = 1f / source.transform.localScale.x;
		}
		freeObj.transform.localScale = new Vector3(freeObj.transform.localScale.x * num3, freeObj.transform.localScale.y * num3, freeObj.transform.localScale.z);
		Vector2 heading = CDGHelper.AngleToVector(Mathf.RoundToInt(angleInDeg));
		freeObj.Heading = heading;
		if (freeObj.PivotFollowsOrientation)
		{
			float num4 = angleInDeg;
			if (freeObj.IsFlipped && !freeObj.SnapToOwner)
			{
				num4 = 180f + num4;
			}
			if (!freeObj.Pivot)
			{
				Vector3 localEulerAngles = freeObj.transform.localEulerAngles;
				localEulerAngles.z = num4;
				freeObj.transform.localEulerAngles = localEulerAngles;
			}
			else
			{
				Vector3 localEulerAngles2 = freeObj.Pivot.transform.localEulerAngles;
				localEulerAngles2.z = num4;
				freeObj.Pivot.transform.localEulerAngles = localEulerAngles2;
			}
		}
		freeObj.SetCorgiVelocity(freeObj.Heading * freeObj.Speed);
		freeObj.UpdateMovement();
		if ((bool)freeObj.Animator)
		{
			freeObj.Animator.enabled = true;
		}
		freeObj.Spawn();
		AddTraitEffects(freeObj);
		return freeObj;
	}

	public static void ApplyProjectileDamage(BaseCharacterController characterController, Projectile_RL projectile)
	{
		projectile.MagicScale = projectile.ProjectileData.MagicScale;
		projectile.StrengthScale = projectile.ProjectileData.StrengthScale;
		if (!characterController)
		{
			return;
		}
		projectile.Magic = characterController.ActualMagic;
		projectile.Strength = characterController.ActualStrength;
		PlayerController playerController = characterController as PlayerController;
		if (!playerController)
		{
			projectile.ActualCritChance = characterController.ActualCritChance;
			projectile.ActualCritDamage = characterController.ActualCritDamage;
			return;
		}
		CastAbilityType lastCastAbilityTypeCasted = playerController.CastAbility.LastCastAbilityTypeCasted;
		bool flag = !playerController.CastAbility.GetAbility(lastCastAbilityTypeCasted).DealsNoDamage;
		if (projectile.DamageType == DamageType.Strength)
		{
			projectile.ActualCritChance = playerController.ActualCritChance;
			projectile.ActualCritDamage = CalculateProjectileCritDamage(projectile, getMagicCritDmg: false);
		}
		else
		{
			projectile.ActualCritChance = playerController.ActualMagicCritChance;
			projectile.ActualCritDamage = CalculateProjectileCritDamage(projectile, getMagicCritDmg: true);
		}
		if (projectile.DamageType == DamageType.Strength)
		{
			if (TraitManager.IsTraitActive(TraitType.BonusStrength))
			{
				projectile.DamageMod += 0.100000024f;
			}
			if (TraitManager.IsTraitActive(TraitType.BonusMagicStrength))
			{
				projectile.DamageMod += -0.5f;
			}
			if (TraitManager.IsTraitActive(TraitType.DamageBoost))
			{
				projectile.DamageMod += 0.5f;
			}
			AngryOnHit_Trait angryOnHit_Trait = TraitManager.GetActiveTrait(TraitType.AngryOnHit) as AngryOnHit_Trait;
			if ((bool)angryOnHit_Trait)
			{
				projectile.DamageMod += angryOnHit_Trait.StrengthMultiplier;
			}
			projectile.DamageMod += (float)SaveManager.PlayerSaveData.GetRelic(RelicType.WeaponSwap).Level * 0.07000005f;
			projectile.DamageMod += (float)SaveManager.PlayerSaveData.GetRelic(RelicType.AttackCooldown).Level * 1f;
			projectile.DamageMod += (float)SaveManager.PlayerSaveData.GetRelic(RelicType.LowResolveWeaponDamage).Level * 0.00999999f * (float)Mathf.Max(Mathf.RoundToInt((1f - playerController.ActualResolve) * 100f), 0);
			if (playerController.StatusEffectController.HasStatusEffect(StatusEffectType.Player_Suave))
			{
				int num = Mathf.RoundToInt(projectile.Magic * 0.15f);
				if (projectile.ActualDamage > 0f)
				{
					projectile.DamageMod += (float)num / projectile.ActualDamage;
				}
			}
			bool flag2 = projectile is DownstrikeProjectile_RL;
			if (flag2)
			{
				projectile.DamageMod += 0.4f * (float)SaveManager.PlayerSaveData.GetRelic(RelicType.SpinKickDamageBonus).Level;
			}
			if (lastCastAbilityTypeCasted == CastAbilityType.Weapon && !flag2 && projectile.BaseDamage > 0f)
			{
				int level = SaveManager.PlayerSaveData.GetRelic(RelicType.WeaponsBurnAdd).Level;
				if (level > 0)
				{
					projectile.AttachStatusEffect(StatusEffectType.Enemy_Burn, 2f * (float)level);
					projectile.DamageMod -= 0f;
				}
				if (SaveManager.PlayerSaveData.GetRelic(RelicType.WeaponsPoisonAdd).Level > 0)
				{
					projectile.AttachStatusEffect(StatusEffectType.Enemy_Poison);
				}
				if (SaveManager.PlayerSaveData.GetRelic(RelicType.WeaponsComboAdd).Level > 0)
				{
					float duration = 0f;
					if (!projectile.StatusEffectTypes.IsNativeNull() && projectile.StatusEffectTypes.IndexOf(StatusEffectType.Player_Combo) != -1)
					{
						duration = float.MaxValue;
					}
					projectile.AttachStatusEffect(StatusEffectType.Player_Combo, duration);
				}
			}
		}
		else
		{
			if (TraitManager.IsTraitActive(TraitType.BonusMagicStrength))
			{
				projectile.DamageMod += 0f;
			}
			if (TraitManager.IsTraitActive(TraitType.MagicBoost))
			{
				projectile.DamageMod += 0.5f;
			}
			RelicObj relic = SaveManager.PlayerSaveData.GetRelic(RelicType.MagicDamageEnemyCount);
			if (relic.Level > 0)
			{
				projectile.DamageMod += (float)relic.IntValue * 0.1f;
			}
			projectile.DamageMod += (float)SaveManager.PlayerSaveData.GetRelic(RelicType.BonusMana).Level * 0.08000004f;
			projectile.DamageMod += (float)SaveManager.PlayerSaveData.GetRelic(RelicType.SpellSwap).Level * 0.07000005f;
			projectile.DamageMod += (float)SaveManager.PlayerSaveData.GetRelic(RelicType.LowResolveMagicDamage).Level * 0.00999999f * (float)Mathf.Max(Mathf.RoundToInt((1f - playerController.ActualResolve) * 100f), 0);
			if (TraitManager.IsTraitActive(TraitType.ManaCostAndDamageUp) && projectile.CompareTag("PlayerProjectile"))
			{
				BaseAbility_RL ability = playerController.CastAbility.GetAbility(lastCastAbilityTypeCasted);
				if ((bool)ability && ability.BaseCost > 0)
				{
					projectile.DamageMod += 1f;
				}
			}
			if (lastCastAbilityTypeCasted == CastAbilityType.Spell)
			{
				int level2 = SaveManager.PlayerSaveData.GetRelic(RelicType.SpellsBurnAdd).Level;
				if (level2 > 0 && flag)
				{
					projectile.AttachStatusEffect(StatusEffectType.Enemy_Burn, 3.05f * (float)level2);
				}
				if (playerController.CurrentManaAsInt >= playerController.ActualMaxMana)
				{
					int level3 = SaveManager.PlayerSaveData.GetRelic(RelicType.MaxManaDamage).Level;
					projectile.DamageMod += 0.2f * (float)level3;
				}
			}
		}
		if (playerController.StatusEffectController.HasStatusEffect(StatusEffectType.Player_Combo))
		{
			projectile.DamageMod += 0.02f * (float)playerController.StatusEffectController.GetStatusEffect(StatusEffectType.Player_Combo).TimesStacked;
		}
		if (playerController.IsInvincible)
		{
			float num2 = 1f * (float)SaveManager.PlayerSaveData.GetRelic(RelicType.InvulnDamageBuff).Level;
			projectile.DamageMod += num2;
		}
		int level4 = SaveManager.PlayerSaveData.GetRelic(RelicType.BonusDamageCurse).Level;
		projectile.DamageMod += 1f * (float)level4;
		int level5 = SaveManager.PlayerSaveData.GetRelic(RelicType.RelicAmountDamageUp).Level;
		int totalUniqueRelics = SaveManager.PlayerSaveData.GetTotalUniqueRelics();
		projectile.DamageMod += 0.06f * (float)totalUniqueRelics * (float)level5;
		projectile.DamageMod += (float)SaveManager.PlayerSaveData.GetRelic(RelicType.AttackExhaust).Level * 0.5f;
		projectile.DamageMod += (float)SaveManager.PlayerSaveData.GetRelic(RelicType.TalentSwap).Level * 0f;
		projectile.DamageMod += (float)SaveManager.PlayerSaveData.GetRelic(RelicType.ReplacementRelic).Level * 0.1f;
		projectile.DamageMod += (float)SaveManager.PlayerSaveData.GetRelic(RelicType.DamageNoHitChallenge).Level * 1.25f;
		if ((float)playerController.CurrentHealthAsInt <= 0.5f * (float)playerController.ActualMaxHealth)
		{
			projectile.DamageMod += (float)SaveManager.PlayerSaveData.GetRelic(RelicType.LowHealthStatBonus).Level * 0.2f;
		}
		if ((float)playerController.CurrentHealthAsInt >= 0.5f * (float)playerController.ActualMaxHealth)
		{
			projectile.DamageMod += (float)SaveManager.PlayerSaveData.GetRelic(RelicType.MaxHealthStatBonus).Level * 0.1f;
		}
		if (playerController.IsGrounded)
		{
			projectile.DamageMod += (float)SaveManager.PlayerSaveData.GetRelic(RelicType.GroundDamageBonus).Level * 0.125f;
		}
		if (playerController.StatusEffectController.HasStatusEffect(StatusEffectType.Player_Dance))
		{
			float num3 = (float)playerController.StatusEffectController.GetStatusEffect(StatusEffectType.Player_Dance).TimesStacked * 0.15f;
			projectile.DamageMod += num3;
		}
	}

	public static float CalculateProjectileCritDamage(Projectile_RL projectile, bool getMagicCritDmg)
	{
		PlayerController playerController = PlayerManager.GetPlayerController();
		float num = 0f;
		if (!getMagicCritDmg)
		{
			float actualDexterity = playerController.ActualDexterity;
			float actualCritDamage = playerController.ActualCritDamage;
			float strengthScale = projectile.StrengthScale;
			return Mathf.RoundToInt(actualDexterity * strengthScale * actualCritDamage);
		}
		float actualFocus = playerController.ActualFocus;
		float actualMagicCritDamage = playerController.ActualMagicCritDamage;
		float magicScale = projectile.MagicScale;
		return Mathf.RoundToInt(actualFocus * magicScale * actualMagicCritDamage);
	}

	private void AddTraitEffects(Projectile_RL projectile)
	{
		if (TraitManager.IsTraitActive(TraitType.ColorTrails) && projectile.Speed > 0f)
		{
			bool useOwnerCollisionPoint = projectile.UseOwnerCollisionPoint;
			projectile.UseOwnerCollisionPoint = false;
			BaseEffect baseEffect = EffectManager.PlayEffect(projectile.gameObject, projectile.Animator, "ColorTrails_Trait_Effect", projectile.Midpoint, 0f, EffectStopType.Gracefully);
			TrailRenderer componentInChildren = baseEffect.GetComponentInChildren<TrailRenderer>();
			componentInChildren.startColor = UnityEngine.Random.ColorHSV(0f, 1f, 1f, 1f, 0.5f, 1f);
			componentInChildren.endColor = UnityEngine.Random.ColorHSV(0f, 1f, 1f, 1f, 0.5f, 1f);
			componentInChildren.time = 18f;
			componentInChildren.widthMultiplier = 1.5f;
			baseEffect.transform.SetParent(projectile.gameObject.transform, worldPositionStays: true);
			projectile.UseOwnerCollisionPoint = useOwnerCollisionPoint;
		}
	}

	public static Vector3 DetermineSpawnPosition(GameObject source, Vector2 spawnOffset, bool isFacingRight)
	{
		Vector3 vector = Vector2.zero;
		if (isFacingRight)
		{
			return source.transform.position + source.transform.rotation * spawnOffset;
		}
		return source.transform.position - source.transform.rotation * new Vector2(spawnOffset.x, 0f - spawnOffset.y);
	}

	public static Projectile_RL FireProjectile(GameObject source, string projectileName, Vector2 offset, bool matchFacing = true, float angleInDeg = 0f, float speedMod = 1f, bool useAbsPos = false, bool playSpawnAudio = true, bool playLifetimeAudio = true, bool playDeathAudio = true)
	{
		return Instance.FireProjectile_Internal(source, projectileName, offset, matchFacing, angleInDeg, speedMod, useAbsPos, playSpawnAudio, playLifetimeAudio, playDeathAudio);
	}

	public static void DisableAllProjectiles(bool flagForDestruction)
	{
		foreach (KeyValuePair<string, GenericPool_RL<Projectile_RL>> item in Instance.m_projectileDict)
		{
			if (flagForDestruction)
			{
				foreach (Projectile_RL @object in item.Value.ObjectList)
				{
					@object.FlagForDestruction();
				}
			}
			else
			{
				item.Value.DisableAll();
			}
		}
	}

	public static void DisableAllProjectiles(bool flagForDestruction, params string[] tags)
	{
		foreach (KeyValuePair<string, GenericPool_RL<Projectile_RL>> item in Instance.m_projectileDict)
		{
			foreach (string text in tags)
			{
				if (!item.Value.GetFreeObj().CompareTag(text))
				{
					continue;
				}
				if (flagForDestruction)
				{
					foreach (Projectile_RL @object in item.Value.ObjectList)
					{
						@object.FlagForDestruction();
					}
				}
				else
				{
					item.Value.DisableAll();
				}
			}
		}
	}

	public static void DisableAllProjectiles(bool flagForDestruction, GameObject source, string projectileName)
	{
		if (!Instance.m_projectileDict.TryGetValue(projectileName, out var value))
		{
			return;
		}
		foreach (Projectile_RL @object in value.ObjectList)
		{
			if (@object.isActiveAndEnabled && !@object.IsFreePoolObj && @object.Owner == source)
			{
				if (flagForDestruction)
				{
					@object.FlagForDestruction();
				}
				else
				{
					@object.gameObject.SetActive(value: false);
				}
			}
		}
	}

	private static void DisableExitRoomProjectiles(object sender, EventArgs args)
	{
		foreach (KeyValuePair<string, GenericPool_RL<Projectile_RL>> item in Instance.m_projectileDict)
		{
			foreach (Projectile_RL @object in item.Value.ObjectList)
			{
				if ((bool)@object && @object.isActiveAndEnabled && @object.DestroyOnRoomChange)
				{
					@object.gameObject.SetActive(value: false);
				}
			}
		}
	}

	public static void DestroyBiomePools()
	{
		foreach (KeyValuePair<string, GenericPool_RL<Projectile_RL>> item in Instance.m_projectileDict)
		{
			item.Value.DestroyPool();
		}
		Instance.m_projectileDict.Clear();
	}

	public static void DestroyOffscreenIconPool()
	{
		Instance.m_offscreenIconPool.DestroyPool();
	}

	public static void CreateBiomePools(BiomeType biome)
	{
		Instance.Internal_CreateBiomePools(biome);
	}

	public static void AttachOffscreenIconToProjectile(Projectile_RL projectile)
	{
		OffscreenIconObj freeObj = Instance.m_offscreenIconPool.GetFreeObj();
		freeObj.gameObject.SetActive(value: true);
		freeObj.AttachProjectile(projectile);
	}

	public static void Reset()
	{
		DisableAllProjectiles(flagForDestruction: false);
	}
}
