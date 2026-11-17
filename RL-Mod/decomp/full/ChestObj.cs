using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using FMODUnity;
using RLAudio;
using Sigtrap.Relays;
using UnityEngine;

public class ChestObj : MonoBehaviour, IRoomConsumer, IRootObj, IGenericPoolObj, ITerrainOnEnterHitResponse, IHitResponse
{
	[SerializeField]
	private StudioEventEmitter m_openedAudioEvent;

	[SerializeField]
	private StudioEventEmitter m_ambientAudioEvent;

	[SerializeField]
	private StudioEventEmitter m_proximityAudioEvent;

	[SerializeField]
	private HitboxControllerLite m_hitboxController;

	private Animator m_animator;

	private SkinnedMeshRenderer m_mesh;

	private List<ISpecialItemDrop> m_specialItemDropsList = new List<ISpecialItemDrop>();

	private WaitRL_Yield m_waitYield;

	private WaitUntil m_disableInputYield;

	private ChestOpenedEventArgs m_chestArgs;

	private MaterialPropertyBlock m_matPropertyBlock;

	private ParticleSystem m_particleSystem;

	private float m_currentParticleSystemOpacity = 1f;

	private int m_previousBossNGLevel = -1;

	private Vector3 m_dropPosition;

	private Relay m_onOpenedRelay = new Relay();

	private static List<float> m_dropOddsHelper_STATIC = new List<float>();

	public bool IsFreePoolObj { get; set; }

	public bool IsAwakeCalled { get; protected set; }

	public SpecialItemType SpecialItemOverride { get; set; }

	public BossID BossID { get; private set; }

	public int Gold { get; private set; }

	public bool IsInitialised { get; private set; }

	public ChestLockState LockState { get; private set; }

	public bool IsLocked
	{
		get
		{
			if (LockState != ChestLockState.Locked)
			{
				return LockState == ChestLockState.Failed;
			}
			return true;
		}
	}

	public int Level { get; private set; }

	public ChestType ChestType { get; private set; }

	public bool IsOpen { get; private set; }

	public BaseRoom Room { get; private set; }

	public Interactable Interactable { get; private set; }

	public int ChestIndex { get; private set; }

	public IRelayLink OnOpenedRelay => m_onOpenedRelay.link;

	private void Awake()
	{
		m_animator = GetComponentInChildren<Animator>();
		Interactable = GetComponent<Interactable>();
		m_mesh = GetComponentInChildren<SkinnedMeshRenderer>();
		m_waitYield = new WaitRL_Yield(0f);
		m_disableInputYield = new WaitUntil(() => RewiredMapController.IsMapEnabled(GameInputMode.Game));
		m_chestArgs = new ChestOpenedEventArgs(this, SpecialItemType.None, 0, null);
		m_matPropertyBlock = new MaterialPropertyBlock();
		IsAwakeCalled = true;
	}

	public void SetChestIndex(int index)
	{
		ChestIndex = index;
	}

	private void OnDestroy()
	{
		Interactable.TriggerOnEnterEvent.RemoveListener(OnChestInteractedWith);
	}

	private void OnChestInteractedWith(GameObject otherObj)
	{
		if (!IsOpen)
		{
			OpenChest();
		}
	}

	public void ForceOpenChest()
	{
		m_ambientAudioEvent.Stop();
		m_animator.SetBool("AlreadyOpened", value: true);
		m_animator.SetTrigger("Open");
		m_animator.Update(2f);
		m_animator.Update(2f);
		Interactable.SetIsInteractableActive(isActive: false);
		m_hitboxController.GetCollider(HitboxType.Terrain).enabled = false;
		IsOpen = true;
	}

	private void OpenChest()
	{
		if (IsLocked)
		{
			RelicObj relic = SaveManager.PlayerSaveData.GetRelic(RelicType.FreeFairyChest);
			if (relic.Level <= 0)
			{
				m_animator.SetTrigger("LockRattle");
				AudioManager.PlayOneShot(null, "event:/UI/InGame/ui_ig_locked", base.transform.position);
				return;
			}
			m_animator.SetBool("FairyFailed", value: false);
			relic.SetLevel(-1, additive: true);
			SaveManager.PlayerSaveData.GetRelic(RelicType.FreeFairyChestUsed).SetLevel(1, additive: true);
		}
		m_onOpenedRelay.Dispatch();
		m_openedAudioEvent.Play();
		m_ambientAudioEvent.Stop();
		Interactable.SetIsInteractableActive(isActive: false);
		m_hitboxController.GetCollider(HitboxType.Terrain).enabled = false;
		IsOpen = true;
		if (BossID != 0)
		{
			m_previousBossNGLevel = SaveManager.ModeSaveData.GetHighestNGBossBeaten(SaveManager.PlayerSaveData.GameModeType, BossID);
		}
		else
		{
			m_previousBossNGLevel = -1;
		}
		StopAllCoroutines();
		StartCoroutine(OpenChestAnimCoroutine());
	}

	public void SetSparkleParticleOpacity(float opacity)
	{
		opacity = Mathf.Clamp(1f - opacity, 0f, 1f);
		if (m_currentParticleSystemOpacity == opacity)
		{
			return;
		}
		if (!m_particleSystem)
		{
			m_particleSystem = base.gameObject.GetComponentInChildren<ParticleSystem>();
		}
		if ((bool)m_particleSystem)
		{
			Renderer component = m_particleSystem.GetComponent<Renderer>();
			if ((bool)component)
			{
				if (component.material.HasProperty(ShaderID_RL._MultiplyColor))
				{
					Color color = component.material.GetColor(ShaderID_RL._MultiplyColor);
					color.a = opacity;
					component.material.SetColor(ShaderID_RL._MultiplyColor, color);
				}
				else
				{
					Debug.LogFormat("| {0} | Sparkle Renderer's Material does not have a property called <b>{1}</b>.</color>", this, ShaderID_RL._MultiplyColor);
				}
			}
		}
		else
		{
			Debug.Log("No particle system");
		}
		m_currentParticleSystemOpacity = opacity;
	}

	private IEnumerator OpenChestAnimCoroutine()
	{
		SpecialItemType rewardType = GetSpecialItemTypeToDrop();
		ISpecialItemDrop specialItemDropObj = CalculateSpecialItemDropObj(rewardType);
		if (ChestType != ChestType.Boss && ChestType != ChestType.Black && rewardType != SpecialItemType.Gold && rewardType != SpecialItemType.Ore && rewardType != SpecialItemType.Soul && specialItemDropObj == null)
		{
			rewardType = ((ChestType != ChestType.Fairy) ? SpecialItemType.Gold : SpecialItemType.Ore);
		}
		m_dropPosition = base.transform.position;
		m_dropPosition.y += 1f;
		m_animator.SetTrigger("Open");
		float architectGoldMod = NPC_EV.GetArchitectGoldMod();
		int level = SaveManager.PlayerSaveData.GetRelic(RelicType.NoGoldXPBonus).Level;
		if (rewardType == SpecialItemType.Gold && ChestType != ChestType.Boss && ChestType != ChestType.Black && (architectGoldMod <= 0f || level > 0))
		{
			m_animator.SetBool("Empty", value: true);
			EffectManager.PlayEffect(base.gameObject, m_animator, "ChestOpenMoths_Effect", m_dropPosition, 5f, EffectStopType.Immediate);
		}
		else if (specialItemDropObj != null)
		{
			EffectManager.PlayEffect(base.gameObject, m_animator, "ChestOpenUnscaledTime_Effect", base.transform.position, 0f, EffectStopType.Gracefully);
		}
		else
		{
			EffectManager.PlayEffect(base.gameObject, m_animator, "ChestOpen_Effect", base.transform.position, 0f, EffectStopType.Gracefully);
		}
		PlayerController playerController = PlayerManager.GetPlayerController();
		if (specialItemDropObj != null)
		{
			playerController.SetVelocity(0f, 0f, additive: false);
			playerController.StopActiveAbilities(stopPersistentAbilities: true);
			playerController.CharacterHitResponse.SetInvincibleTime(999f, additive: false, playInvincibility: false);
			RewiredMapController.SetMapEnabled(GameInputMode.Game, enabled: false);
		}
		string treasureChestOpenState = "TreasureChest_Open_Animation";
		while (!m_animator.GetCurrentAnimatorStateInfo(0).IsName(treasureChestOpenState))
		{
			yield return null;
		}
		if (ChestType != ChestType.Boss && TraitManager.IsTraitActive(TraitType.ExplosiveChests))
		{
			float angleInDeg = UnityEngine.Random.Range(Trait_EV.EXPLOSIVE_CHEST_PROJECTILE_ANGLE.x, Trait_EV.EXPLOSIVE_CHEST_PROJECTILE_ANGLE.y);
			ProjectileManager.FireProjectile(base.gameObject, "ExplosiveChestsPotionProjectile", base.transform.position, matchFacing: false, angleInDeg, 1f, useAbsPos: true);
		}
		int level2 = SaveManager.PlayerSaveData.GetRelic(RelicType.ChestHealthRestore).Level;
		float num = Mathf.CeilToInt(playerController.ActualMagic * 1f) * level2;
		if (num > 0f)
		{
			playerController.SetHealth(num, additive: true, runEvents: true);
			Vector2 absPos = playerController.Midpoint;
			absPos.y += playerController.CollisionBounds.height / 2f;
			string text = string.Format(LocalizationManager.GetString("LOC_ID_STATUS_EFFECT_HEALTH_RESTORE_1", SaveManager.PlayerSaveData.CurrentCharacter.IsFemale), (int)num);
			TextPopupManager.DisplayTextAtAbsPos(TextPopupType.HPGained, text, absPos);
		}
		if (specialItemDropObj == null)
		{
			if (ChestType == ChestType.Boss)
			{
				DropRewardFromBossChest();
				yield break;
			}
			if (ChestType == ChestType.Black)
			{
				DropRegularRewardFromBlackChest(rewardType, null, Level);
				yield break;
			}
			if (ChestType == ChestType.Gold)
			{
				int amount = Mathf.CeilToInt((float)GetOreDropAmount(ItemDropType.EquipmentOre, Level) * 1.5f);
				int amount2 = Mathf.CeilToInt((float)GetOreDropAmount(ItemDropType.RuneOre, Level) * 0.5f);
				ItemDropManager.DropItem(ItemDropType.EquipmentOre, amount, m_dropPosition, largeSpurt: true, forceMagnetize: true, fromChest: true);
				ItemDropManager.DropItem(ItemDropType.RuneOre, amount2, m_dropPosition, largeSpurt: true, forceMagnetize: true, fromChest: true);
			}
			if (ChestType == ChestType.Silver)
			{
				int amount3 = Mathf.CeilToInt((float)GetOreDropAmount(ItemDropType.EquipmentOre, Level) * 0.5f);
				ItemDropManager.DropItem(ItemDropType.EquipmentOre, amount3, m_dropPosition, largeSpurt: false, forceMagnetize: false, fromChest: true);
			}
			DropRewardFromRegularChest(rewardType, null, Level);
			yield break;
		}
		yield return null;
		GameManager.SetIsPaused(isPaused: true);
		AnimatorUpdateMode storedPlayerUpdateMode = playerController.Animator.updateMode;
		AnimatorUpdateMode storedChestUpdateMode = m_animator.updateMode;
		playerController.Animator.updateMode = AnimatorUpdateMode.UnscaledTime;
		m_animator.updateMode = AnimatorUpdateMode.UnscaledTime;
		playerController.CharacterMove.SetHorizontalMove(0f);
		playerController.Animator.SetBool("Victory", value: true);
		m_waitYield.CreateNew(0.5f, useUnscaledTime: true);
		yield return m_waitYield;
		if (ChestType == ChestType.Boss)
		{
			DropRewardFromBossChest();
		}
		else if (ChestType == ChestType.Black)
		{
			DropRegularRewardFromBlackChest(rewardType, specialItemDropObj, Level);
		}
		else
		{
			DropRewardFromRegularChest(rewardType, specialItemDropObj, Level);
		}
		yield return m_disableInputYield;
		RewiredMapController.SetMapEnabled(GameInputMode.Game, enabled: false);
		GameManager.SetIsPaused(isPaused: true);
		if (ChestType == ChestType.Gold)
		{
			int amount4 = Mathf.CeilToInt((float)GetOreDropAmount(ItemDropType.EquipmentOre, Level) * 1.5f);
			int amount5 = Mathf.CeilToInt((float)GetOreDropAmount(ItemDropType.RuneOre, Level) * 0.5f);
			ItemDropManager.DropItem(ItemDropType.EquipmentOre, amount4, m_dropPosition, largeSpurt: true, forceMagnetize: true, fromChest: true);
			ItemDropManager.DropItem(ItemDropType.RuneOre, amount5, m_dropPosition, largeSpurt: true, forceMagnetize: true, fromChest: true);
		}
		else if (ChestType == ChestType.Silver)
		{
			ItemDropManager.DropItem(ItemDropType.EquipmentOre, Mathf.CeilToInt((float)GetOreDropAmount(ItemDropType.EquipmentOre, Level) * 0.5f), m_dropPosition, largeSpurt: false, forceMagnetize: false, fromChest: true);
		}
		else if (ChestType == ChestType.Fairy)
		{
			ItemDropManager.DropItem(ItemDropType.RuneOre, GetOreDropAmount(ItemDropType.RuneOre, Level), m_dropPosition, largeSpurt: true, forceMagnetize: true, fromChest: true);
		}
		playerController.Animator.SetBool("Victory", value: false);
		m_waitYield.CreateNew(0.5f, useUnscaledTime: true);
		yield return m_waitYield;
		playerController.CharacterHitResponse.SetInvincibleTime(1f, additive: false, playInvincibility: false);
		RewiredMapController.SetMapEnabled(GameInputMode.Game, enabled: true);
		GameManager.SetIsPaused(isPaused: false);
		playerController.Animator.updateMode = storedPlayerUpdateMode;
		m_animator.updateMode = storedChestUpdateMode;
	}

	private void DropRewardFromRegularChest(SpecialItemType itemDropType, ISpecialItemDrop specialItemDrop, int chestLevel)
	{
		int num = 0;
		m_specialItemDropsList.Clear();
		bool forceMagnetize = ChestType == ChestType.Fairy || ChestType == ChestType.Gold;
		switch (itemDropType)
		{
		case SpecialItemType.Gold:
			ItemDropManager.DropGold(Gold, m_dropPosition, TraitManager.IsTraitActive(TraitType.ItemsGoFlying) || ChestType == ChestType.Gold, forceMagnetize, fromChest: true);
			num = Gold;
			break;
		case SpecialItemType.Ore:
		{
			ItemDropType itemDropType2 = ((ChestType == ChestType.Fairy) ? ItemDropType.RuneOre : ItemDropType.EquipmentOre);
			num = GetOreDropAmount(itemDropType2, chestLevel);
			ItemDropManager.DropItem(itemDropType2, num, m_dropPosition, largeSpurt: true, forceMagnetize, fromChest: true);
			break;
		}
		case SpecialItemType.Soul:
			ItemDropManager.DropItem(ItemDropType.Soul, 200, m_dropPosition, largeSpurt: true, forceMagnetize: true, fromChest: true);
			break;
		default:
			if (specialItemDrop != null)
			{
				m_specialItemDropsList.Add(specialItemDrop);
			}
			foreach (ISpecialItemDrop specialItemDrops in m_specialItemDropsList)
			{
				ItemDropManager.DropSpecialItem(specialItemDrops);
			}
			break;
		case SpecialItemType.None:
			break;
		}
		m_chestArgs.Initialize(this, itemDropType, num, m_specialItemDropsList);
		Messenger<GameMessenger, GameEvent>.Broadcast(GameEvent.ChestOpened, this, m_chestArgs);
	}

	private int GetOreDropAmount(ItemDropType oreDropType, int chestLevel)
	{
		int num = ((oreDropType == ItemDropType.RuneOre) ? 150 : 175);
		float num2 = ((oreDropType == ItemDropType.RuneOre) ? 2f : 4.5f);
		return (int)((float)num + (float)chestLevel * num2);
	}

	private void DropRewardFromBossChest()
	{
		if (BossID != 0)
		{
			if (Economy_EV.BOSS_DROP_TABLE.ContainsKey(BossID))
			{
				int newGamePlusLevel = SaveManager.PlayerSaveData.NewGamePlusLevel;
				int bossSoulsDropAmount = Souls_EV.GetBossSoulsDropAmount(BossID, m_previousBossNGLevel);
				ItemDropManager.DropItem(ItemDropType.Soul, SoulDrop.FakeSoulCounter_STATIC = Mathf.Clamp(Souls_EV.GetBossSoulsDropAmount(BossID, newGamePlusLevel) - bossSoulsDropAmount, 0, int.MaxValue), m_dropPosition, largeSpurt: true, forceMagnetize: true, fromChest: true);
				float num = (float)newGamePlusLevel * 0.75f;
				if (IsVariantBossChest(BossID))
				{
					num += 1.25f;
				}
				int goldAmount = Economy_EV.BOSS_DROP_TABLE[BossID].GoldAmount;
				ItemDropManager.DropGold(goldAmount + Mathf.RoundToInt((float)goldAmount * num), m_dropPosition, largeSpurt: true, forceMagnetize: true, fromChest: true);
				int oreCount = Economy_EV.BOSS_DROP_TABLE[BossID].OreCount;
				oreCount += Mathf.RoundToInt((float)oreCount * num);
				ItemDropManager.DropItem(ItemDropType.EquipmentOre, oreCount, m_dropPosition, largeSpurt: true, forceMagnetize: true, fromChest: true);
				int runeOreCount = Economy_EV.BOSS_DROP_TABLE[BossID].RuneOreCount;
				runeOreCount += Mathf.RoundToInt((float)runeOreCount * num);
				ItemDropManager.DropItem(ItemDropType.RuneOre, runeOreCount, m_dropPosition, largeSpurt: true, forceMagnetize: true, fromChest: true);
			}
			m_chestArgs.Initialize(this, SpecialItemType.Gold, 0, m_specialItemDropsList);
			Messenger<GameMessenger, GameEvent>.Broadcast(GameEvent.ChestOpened, this, m_chestArgs);
		}
		else
		{
			Debug.LogFormat("<color=red>[{0}] Chest is of Type, Boss, but its BossID is set to None. You must specify a Boss ID</color>", this);
		}
	}

	private bool IsVariantBossChest(BossID bossID)
	{
		return bossID switch
		{
			BossID.Castle_Boss => BurdenManager.GetBurdenLevel(BurdenType.CastleBossUp) > 0, 
			BossID.Bridge_Boss => BurdenManager.GetBurdenLevel(BurdenType.BridgeBossUp) > 0, 
			BossID.Forest_Boss => BurdenManager.GetBurdenLevel(BurdenType.ForestBossUp) > 0, 
			BossID.Tower_Boss => BurdenManager.GetBurdenLevel(BurdenType.TowerBossUp) > 0, 
			BossID.Cave_Boss => BurdenManager.GetBurdenLevel(BurdenType.CaveBossUp) > 0, 
			BossID.Final_Boss => BurdenManager.GetBurdenLevel(BurdenType.FinalBossUp) > 0, 
			_ => false, 
		};
	}

	private void DropRegularRewardFromBlackChest(SpecialItemType itemDropType, ISpecialItemDrop specialItemDrop, int level)
	{
		int oreDropAmount = GetOreDropAmount(ItemDropType.EquipmentOre, level);
		ItemDropManager.DropItem(ItemDropType.EquipmentOre, oreDropAmount, m_dropPosition, largeSpurt: true, forceMagnetize: true, fromChest: true);
		oreDropAmount = GetOreDropAmount(ItemDropType.RuneOre, level);
		ItemDropManager.DropItem(ItemDropType.RuneOre, oreDropAmount, m_dropPosition, largeSpurt: true, forceMagnetize: true, fromChest: true);
		int newGamePlusLevel = SaveManager.PlayerSaveData.NewGamePlusLevel;
		int totalBlackChestSoulsCollected = Souls_EV.GetTotalBlackChestSoulsCollected();
		EquipmentCategoryType finalGearCategoryType = EquipmentType_RL.GetFinalGearCategoryType(itemDropType);
		SaveManager.ModeSaveData.SetHighestNGBlackChestOpened(finalGearCategoryType, newGamePlusLevel, additive: false, overrideEvenIfLower: false);
		int num = Mathf.Clamp(Souls_EV.GetTotalBlackChestSoulsCollected() - totalBlackChestSoulsCollected, 0, int.MaxValue);
		if (num > 0)
		{
			SoulDrop.FakeSoulCounter_STATIC = num;
			ItemDropManager.DropItem(ItemDropType.Soul, num, m_dropPosition, largeSpurt: true, forceMagnetize: true, fromChest: true);
		}
		m_specialItemDropsList.Clear();
		if (specialItemDrop != null)
		{
			m_specialItemDropsList.Add(specialItemDrop);
			ItemDropManager.DropSpecialItem(specialItemDrop);
		}
		m_chestArgs.Initialize(this, itemDropType, oreDropAmount, m_specialItemDropsList);
		Messenger<GameMessenger, GameEvent>.Broadcast(GameEvent.ChestOpened, this, m_chestArgs);
	}

	private ISpecialItemDrop CalculateSpecialItemDropObj(SpecialItemType specialItemType)
	{
		bool flag = true;
		ISpecialItemDrop specialItemDrop = null;
		switch (specialItemType)
		{
		case SpecialItemType.Blueprint:
			specialItemDrop = SpecialItemDropUtility.GetBlueprintDrop(Level, ChestType_RL.GetChestRarity(ChestType));
			break;
		case SpecialItemType.Rune:
			specialItemDrop = SpecialItemDropUtility.GetRuneDrop(Level);
			break;
		case SpecialItemType.Relic:
			specialItemDrop = SpecialItemDropUtility.GetRandomRelicDrop();
			break;
		case SpecialItemType.Challenge:
			specialItemDrop = SpecialItemDropUtility.GetChallengeDrop();
			break;
		case SpecialItemType.FinalGear_Weapon:
		case SpecialItemType.FinalGear_Helm:
		case SpecialItemType.FinalGear_Chest:
		case SpecialItemType.FinalGear_Cape:
		case SpecialItemType.FinalGear_Trinket:
			specialItemDrop = SpecialItemDropUtility.GetFinalBlueprintDrop(EquipmentType_RL.GetFinalGearCategoryType(specialItemType));
			break;
		default:
			flag = false;
			break;
		}
		if (flag && specialItemDrop == null)
		{
			Debug.Log("Tried to drop special item of type: " + specialItemType.ToString() + " but failed. No available drops found.");
		}
		return specialItemDrop;
	}

	private SpecialItemType GetSpecialItemTypeToDrop()
	{
		if (SpecialItemOverride != 0)
		{
			return SpecialItemOverride;
		}
		if (ChestType == ChestType.Black && SpecialItemOverride == SpecialItemType.None)
		{
			throw new Exception("Black Chests MUST have a SpecialItemOverride assigned to it.");
		}
		if (ChestType == ChestType.Gold)
		{
			return SpecialItemType.Challenge;
		}
		Vector2[] chestItemTypeOdds = Economy_EV.GetChestItemTypeOdds(ChestType);
		int randomOdds = CDGHelper.GetRandomOdds(GetDropOdds(chestItemTypeOdds));
		return (SpecialItemType)chestItemTypeOdds[randomOdds].x;
	}

	private List<float> GetDropOdds(Vector2[] dropTypeArray)
	{
		m_dropOddsHelper_STATIC.Clear();
		int num = dropTypeArray.Length;
		for (int i = 0; i < num; i++)
		{
			m_dropOddsHelper_STATIC.Add(dropTypeArray[i].y);
		}
		return m_dropOddsHelper_STATIC;
	}

	public void Initialise(ChestType chestType, int level, int gold, BossID bossID = BossID.None)
	{
		ChestType = chestType;
		Level = level;
		Gold = gold;
		BossID = bossID;
		if ((bool)Interactable)
		{
			Interactable.SetIsInteractableActive(!IsLocked);
			Interactable.TriggerOnEnterEvent.AddListener(OnChestInteractedWith);
		}
		else
		{
			Debug.LogFormat("<color=red>[{0}] m_interactable field is null</color>", this);
		}
		IsInitialised = true;
	}

	public void SetChestLockState(ChestLockState lockState)
	{
		LockState = lockState;
		switch (lockState)
		{
		case ChestLockState.Unlocked:
			m_animator.SetBool("Locked", value: false);
			m_animator.SetBool("FairyFailed", value: false);
			break;
		case ChestLockState.Locked:
			m_animator.SetBool("Locked", value: true);
			m_animator.SetBool("FairyFailed", value: false);
			break;
		case ChestLockState.Failed:
			m_animator.SetBool("Locked", value: false);
			m_animator.SetBool("FairyFailed", value: true);
			m_ambientAudioEvent.Stop();
			if (SaveManager.PlayerSaveData.GetRelic(RelicType.FreeFairyChest).Level > 0)
			{
				Interactable.SetIsInteractableActive(isActive: true);
			}
			else
			{
				Interactable.SetIsInteractableActive(isActive: false);
			}
			break;
		}
	}

	public void SetOpacity(float opacity)
	{
		if (opacity < 0f || opacity > 1f)
		{
			throw new ArgumentOutOfRangeException("opacity", $"<color=red>opacity must have a value between 0 and 1.</color>");
		}
		m_mesh.GetPropertyBlock(m_matPropertyBlock);
		m_matPropertyBlock.SetFloat("_Opacity", opacity);
		m_mesh.SetPropertyBlock(m_matPropertyBlock);
	}

	public void SetRoom(BaseRoom room)
	{
		Room = room;
	}

	private void OnDisable()
	{
		if ((bool)m_particleSystem && m_currentParticleSystemOpacity != 1f)
		{
			SetSparkleParticleOpacity(1f);
			m_particleSystem = null;
		}
		DisablePooledObjectManager.DisablePooledObject(this);
	}

	public void TerrainOnEnterHitResponse(IHitboxController otherHBController)
	{
		if (otherHBController.RootGameObject.CompareTag("Player") && Interactable.IsInteractableActive)
		{
			m_proximityAudioEvent.Play();
		}
	}

	public void ResetValues()
	{
		SetChestLockState(ChestLockState.Unlocked);
		m_animator.SetBool("AlreadyOpened", value: false);
		m_animator.SetBool("Open", value: false);
		m_animator.SetBool("Locked", value: false);
		m_animator.SetBool("FairyFailed", value: false);
		m_animator.SetBool("Empty", value: false);
		m_animator.Update(0f);
		m_animator.Update(0f);
		Interactable.SetIsInteractableActive(isActive: true);
		m_hitboxController.GetCollider(HitboxType.Terrain).enabled = true;
		IsOpen = false;
	}

	[SpecialName]
	GameObject IRootObj.get_gameObject()
	{
		return base.gameObject;
	}

	[SpecialName]
	GameObject IGenericPoolObj.get_gameObject()
	{
		return base.gameObject;
	}
}
