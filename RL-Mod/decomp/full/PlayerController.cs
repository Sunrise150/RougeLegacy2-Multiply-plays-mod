using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using MoreMountains.CorgiEngine;
using RLAudio;
using RL_Windows;
using Rewired;
using SceneManagement_RL;
using Sigtrap.Relays;
using UnityEngine;

public class PlayerController : BaseCharacterController, IDamageObj, IWeaponOnEnterHitResponse, IHitResponse, IHasProjectileNameArray, IAudioEventEmitter
{
	private string[] m_projectileNameArray = new string[6] { "CreatePlatformTalentProjectile", "SuperFartProjectile", "SporeBurstProjectile", "RelicOnHitAreaDamageProjectile", "RelicLandShockwaveProjectile", "RelicDamageAuraOnHitProjectile" };

	[SerializeField]
	private GameObject m_followTargetGO;

	private int m_baseArmor;

	private float m_baseCritDamage;

	private float m_baseDexterity;

	private float m_baseMagicCritDamage;

	private float m_baseMagicDexterity;

	private float m_baseResolve;

	private int m_baseVitality;

	private CharacterDash_RL m_characterDash;

	private CharacterDownStrike_RL m_characterDownStrike;

	private CastAbility_RL m_characterAbilities;

	private CharacterHorizontalMovement_RL m_characterMove;

	private CharacterJump_RL m_characterJump;

	private CharacterClass m_characterClass;

	private CharacterFlight_RL m_characterFlight;

	private PlayerLookController m_lookController;

	private InteractIconController m_interactIconController;

	private BaseRoom m_currentlyInRoom;

	private Player m_rewiredPlayer;

	private BiomeEventArgs m_biomeEventArgs;

	private PlayerDeathEventArgs m_playerDeathEventArgs;

	private bool m_justRolled;

	private Coroutine m_rollCoroutine;

	protected ManaChangeEventArgs m_manaChangeEventArgs;

	private TraitChangedEventArgs m_traitChangeEventArgs;

	private Coroutine m_onHitAreaDamageCoroutine;

	private Coroutine m_noAttackDamageBonusCoroutine;

	private Coroutine m_spinKicksDropCaltropsCoroutine;

	private WaitRL_Yield m_onHitAreaDamage_waitYield;

	private WaitRL_Yield m_noAttackDamageBonus_waitYield;

	private Action<MonoBehaviour, EventArgs> m_onEquippedOrLevelChanged;

	private Action<MonoBehaviour, EventArgs> m_onRelicChanged;

	private Action<MonoBehaviour, EventArgs> m_applyPermanentStatusEffects;

	protected Relay<PlayerDeathEventArgs> m_onPlayerDeathRelay = new Relay<PlayerDeathEventArgs>();

	private Relay<ManaChangeEventArgs> m_manaChangeRelay = new Relay<ManaChangeEventArgs>();

	private int m_currentArmor;

	private int m_currentExhaust;

	private float m_updateAccumulatedXP;

	private float m_updateAccumulatedLifeSteal;

	private TextPopupObj m_xpPopup;

	private RelicChangedEventArgs m_extraLifeEventArgs = new RelicChangedEventArgs(RelicType.ExtraLife_Unity);

	private Coroutine m_disableAbilitiesCoroutine;

	public string[] ProjectileNameArray => m_projectileNameArray;

	public bool IsMushroomBig { get; private set; }

	public string Description => ToString();

	public float TimeEnteredRoom { get; private set; }

	public GameObject FollowTargetGO
	{
		get
		{
			return m_followTargetGO;
		}
		set
		{
			m_followTargetGO = value;
		}
	}

	public bool IsInteractIconVisible
	{
		get
		{
			return m_interactIconController.IsIconVisible;
		}
		set
		{
			m_interactIconController.SetIconVisible(value);
		}
	}

	public bool DisableDoorBlock { get; set; }

	public StatusEffectType[] StatusEffectTypes => null;

	public float[] StatusEffectDurations => null;

	public bool IsSpearSpinning { get; set; }

	public bool IsBlocking { get; set; }

	public bool CloakInterrupted { get; set; }

	public float BlockStartTime { get; set; }

	public bool IsDotDamage => false;

	public IRelayLink<PlayerDeathEventArgs> OnPlayerDeathRelay => m_onPlayerDeathRelay.link;

	public IRelayLink<ManaChangeEventArgs> ManaChangeRelay => m_manaChangeRelay.link;

	public override float BaseScaleToOffsetWith => 1.4f;

	public float BaseResolve
	{
		get
		{
			return m_baseResolve;
		}
		private set
		{
			m_baseResolve = value;
		}
	}

	public float ActualResolve
	{
		get
		{
			float num = 0f;
			SoulShopObj soulShopObj = SaveManager.ModeSaveData.GetSoulShopObj(SoulShopType.MinimumResolveBlock);
			if (!soulShopObj.IsNativeNull())
			{
				num += soulShopObj.CurrentStatGain;
			}
			num += EquipmentManager.Get_EquipmentSet_BonusTypeStatGain(EquipmentSetBonusType.MinimumResolve);
			float num2 = BaseResolve;
			if (ChallengeManager.IsInChallenge)
			{
				num2 = 2.5f;
			}
			return Mathf.Clamp((num2 + ResolveAdd) * (1f + ResolveMod), num, float.MaxValue);
		}
	}

	public float ResolveAdd { get; private set; }

	public float ResolveMod { get; private set; }

	public virtual int SpellOrbs { get; set; }

	public bool DisableArmor { get; set; }

	public virtual float BaseDexterity
	{
		get
		{
			return m_baseDexterity;
		}
		set
		{
			m_baseDexterity = Mathf.Clamp(value, 0f, float.MaxValue);
		}
	}

	public virtual float ActualDexterity
	{
		get
		{
			float num = Mathf.Clamp((BaseDexterity + DexterityAdd + DexterityTemporaryAdd) * (1f + DexterityMod + DexterityTemporaryMod), 0f, float.MaxValue);
			if (ChallengeManager.IsInChallenge)
			{
				num = ChallengeManager.ApplyStatCap(num, isDexterityOrFocus: true);
			}
			return num;
		}
	}

	public override float ActualCritChance => Mathf.Clamp(CritChanceAdd + CritChanceTemporaryAdd + 0f, 0f, 100f);

	public float DexterityAdd { get; set; }

	public float DexterityTemporaryAdd { get; set; }

	public float DexterityMod { get; set; }

	public float DexterityTemporaryMod { get; set; }

	public float CritChanceAdd { get; set; }

	public float CritChanceTemporaryAdd { get; set; }

	public virtual float BaseCritDamage
	{
		get
		{
			return m_baseCritDamage;
		}
		set
		{
			m_baseCritDamage = Mathf.Clamp(value, 0f, float.MaxValue);
		}
	}

	public override float ActualCritDamage => Mathf.Clamp(BaseCritDamage + CritDamageAdd + CritDamageTemporaryAdd, 0f, float.MaxValue);

	public virtual float CritDamageAdd { get; set; }

	public virtual float CritDamageTemporaryAdd { get; set; }

	public virtual float BaseFocus
	{
		get
		{
			return m_baseMagicDexterity;
		}
		set
		{
			m_baseMagicDexterity = Mathf.Clamp(value, 0f, float.MaxValue);
		}
	}

	public virtual float ActualFocus
	{
		get
		{
			float num = Mathf.Clamp((BaseFocus + FocusAdd + FocusTemporaryAdd) * (1f + FocusMod + FocusTemporaryMod), 0f, float.MaxValue);
			if (ChallengeManager.IsInChallenge)
			{
				num = ChallengeManager.ApplyStatCap(num, isDexterityOrFocus: true);
			}
			return num;
		}
	}

	public virtual float ActualMagicCritChance => Mathf.Clamp(MagicCritChanceAdd + MagicCritChanceTemporaryAdd + 0f, 0f, 1f);

	public virtual float FocusAdd { get; set; }

	public virtual float FocusTemporaryAdd { get; set; }

	public float FocusMod { get; set; }

	public float FocusTemporaryMod { get; set; }

	public float MagicCritChanceAdd { get; set; }

	public float MagicCritChanceTemporaryAdd { get; set; }

	public float BaseMagicCritDamage
	{
		get
		{
			return m_baseMagicCritDamage;
		}
		set
		{
			m_baseMagicCritDamage = Mathf.Clamp(value, 0f, float.MaxValue);
		}
	}

	public float ActualMagicCritDamage => Mathf.Clamp(BaseMagicCritDamage + MagicCritDamageAdd + MagicCritDamageTemporaryAdd, 0f, float.MaxValue);

	public float MagicCritDamageAdd { get; set; }

	public float MagicCritDamageTemporaryAdd { get; set; }

	public int BaseArmor
	{
		get
		{
			return m_baseArmor;
		}
		private set
		{
			m_baseArmor = Mathf.Clamp(value, 0, int.MaxValue);
		}
	}

	public int CurrentArmor
	{
		get
		{
			return m_currentArmor;
		}
		set
		{
			m_currentArmor = Mathf.Clamp(value, 0, ActualArmor);
		}
	}

	public int ActualArmor => Mathf.Clamp(BaseArmor + ArmorAdds, 0, int.MaxValue);

	public int ArmorAdds { get; set; }

	public int CurrentExhaust
	{
		get
		{
			return m_currentExhaust;
		}
		set
		{
			m_currentExhaust = Mathf.Clamp(value, 0, 99);
		}
	}

	public override float ActualStrength
	{
		get
		{
			float num = base.ActualStrength;
			if (ChallengeManager.IsInChallenge)
			{
				num = ChallengeManager.ApplyStatCap(num);
				num *= 1f + ChallengeManager.GetActiveHandicapMod();
			}
			return num;
		}
	}

	public override float ActualMagic
	{
		get
		{
			float num = base.ActualMagic;
			if (ChallengeManager.IsInChallenge)
			{
				num = ChallengeManager.ApplyStatCap(num);
				num *= 1f + ChallengeManager.GetActiveHandicapMod();
			}
			return num;
		}
	}

	public float CachedHealthOverride { get; set; }

	public float CachedManaOverride { get; set; }

	public int BaseVitality
	{
		get
		{
			return m_baseVitality;
		}
		set
		{
			m_baseVitality = Mathf.Clamp(value, 0, int.MaxValue);
		}
	}

	public int ClassModdedMaxHealth => Mathf.CeilToInt((float)VitalityModdedMaxHealth * CharacterClass.ClassData.PassiveData.MaxHPMod);

	public int ActualVitality
	{
		get
		{
			int num = BaseVitality + VitalityAdd;
			num = Mathf.CeilToInt((float)num * (1f + VitalityMod));
			if (ChallengeManager.IsInChallenge)
			{
				num = (int)ChallengeManager.ApplyStatCap(num);
				num = Mathf.CeilToInt((float)num * (1f + ChallengeManager.GetActiveHandicapMod()));
			}
			return num;
		}
	}

	public int VitalityAdd { get; set; }

	public float VitalityMod { get; set; }

	public override int BaseMaxHealth => Mathf.CeilToInt((float)BaseVitality * 10f);

	public int VitalityModdedMaxHealth => Mathf.CeilToInt((float)ActualVitality * 10f);

	public override int ActualMaxHealth
	{
		get
		{
			if (TraitManager.IsTraitActive(TraitType.OneHitDeath))
			{
				return 1;
			}
			int classModdedMaxHealth = ClassModdedMaxHealth;
			float traitMaxHealthMod = TraitMaxHealthMod;
			float relicMaxHealthMod = RelicMaxHealthMod;
			float maxHealthMod = base.MaxHealthMod;
			float num = 1f;
			num += traitMaxHealthMod + relicMaxHealthMod + maxHealthMod;
			float num2 = Mathf.Clamp(1f - ActualResolve, 0f, 1f);
			num2 *= 1f;
			return Mathf.Clamp(Mathf.CeilToInt(((float)classModdedMaxHealth * num + 100f * num) * (1f - num2)), 1, int.MaxValue);
		}
	}

	public float TraitMaxHealthMod { get; set; }

	public float RelicMaxHealthMod { get; set; }

	public int BaseMaxMana { get; set; }

	public int ClassModdedMaxMana => Mathf.CeilToInt((float)BaseMaxMana * CharacterClass.ClassData.PassiveData.MaxManaMod * (1f + EquipmentMaxManaMod));

	public int ActualMaxMana => Mathf.Clamp(Mathf.CeilToInt((float)ClassModdedMaxMana * (1f + TraitMaxManaMod)) + PostModMaxManaAdd, 1, int.MaxValue);

	public int PostModMaxManaAdd { get; set; }

	public float TraitMaxManaMod { get; set; }

	public float EquipmentMaxManaMod { get; set; }

	public float ManaRegenMod { get; set; }

	public float CurrentMana { get; private set; }

	public int CurrentManaAsInt => Mathf.CeilToInt(CurrentMana);

	public override Vector2 ExternalKnockbackMod
	{
		get
		{
			return Vector2.one;
		}
		set
		{
		}
	}

	public int BaseRuneWeight { get; set; }

	public int ActualRuneWeight => BaseRuneWeight + RuneWeightAdds;

	public int RuneWeightAdds { get; set; }

	public int BaseAllowedEquipmentWeight { get; set; }

	public int ActualAllowedEquipmentWeight => BaseAllowedEquipmentWeight + AllowedEquipmentWeightAdds;

	public int AllowedEquipmentWeightAdds { get; set; }

	public float AbilityCoolDownMod { get; set; }

	public float BaseMovementSpeed
	{
		get
		{
			return m_characterMove.MovementSpeed;
		}
		set
		{
			m_characterMove.MovementSpeed = value;
		}
	}

	public float ActualMovementSpeed => m_characterMove.MovementSpeed;

	public float MovementSpeedMod
	{
		get
		{
			return m_characterMove.MovementSpeedMultiplier;
		}
		set
		{
			m_characterMove.MovementSpeedMultiplier = Mathf.Max(0f, value);
		}
	}

	public CharacterDash_RL CharacterDash => m_characterDash;

	public CharacterDownStrike_RL CharacterDownStrike => m_characterDownStrike;

	public CharacterClass CharacterClass => m_characterClass;

	public CharacterHorizontalMovement_RL CharacterMove => m_characterMove;

	public CharacterFlight_RL CharacterFlight => m_characterFlight;

	public CharacterJump_RL CharacterJump => m_characterJump;

	public CastAbility_RL CastAbility
	{
		get
		{
			if (!m_characterAbilities)
			{
				m_characterAbilities = GetComponentInChildren<CastAbility_RL>();
			}
			return m_characterAbilities;
		}
	}

	public PlayerLookController LookController => m_lookController;

	public BaseRoom PreviouslyInRoom { get; private set; }

	public BaseRoom CurrentlyInRoom
	{
		get
		{
			return m_currentlyInRoom;
		}
		set
		{
			PreviouslyInRoom = m_currentlyInRoom;
			m_currentlyInRoom = value;
		}
	}

	public bool JustRolled
	{
		get
		{
			return m_justRolled;
		}
		set
		{
			if (m_rollCoroutine != null)
			{
				StopCoroutine(m_rollCoroutine);
			}
			m_justRolled = value;
			if (m_justRolled)
			{
				m_rollCoroutine = StartCoroutine(RollCoroutine(0f));
			}
		}
	}

	public float BaseKnockbackStrength
	{
		get
		{
			return 0f;
		}
		set
		{
		}
	}

	public float ActualKnockbackStrength => 0f;

	public float BaseStunStrength
	{
		get
		{
			return 0f;
		}
		set
		{
		}
	}

	public float ActualStunStrength => 0f;

	public StrikeType StrikeType => StrikeType.Blunt;

	public Projectile_RL DamageAuraProjectile { get; set; }

	public void SetMasteryXP(int value, bool additive)
	{
		int runAccumulatedXP = SaveManager.PlayerSaveData.RunAccumulatedXP;
		int runAccumulatedXP2 = (additive ? (runAccumulatedXP + value) : value);
		SaveManager.PlayerSaveData.RunAccumulatedXP = runAccumulatedXP2;
	}

	public void SetMana(float value, bool additive, bool runEvents, bool canExceedMax = false)
	{
		float currentMana = CurrentMana;
		float num = value;
		if (additive)
		{
			num = CurrentMana + value;
		}
		if (TraitManager.IsTraitActive(TraitType.NoManaCap))
		{
			canExceedMax = true;
		}
		if (!canExceedMax && num > currentMana && num > (float)ActualMaxMana)
		{
			num = ((!(currentMana > (float)ActualMaxMana)) ? Mathf.Min(num, ActualMaxMana) : currentMana);
		}
		CurrentMana = Mathf.Max(num, 0f);
		if (!runEvents)
		{
			return;
		}
		if (m_manaChangeEventArgs == null)
		{
			List<RelicType> list = new List<RelicType>();
			RelicType[] typeArray = RelicType_RL.TypeArray;
			foreach (RelicType relicType in typeArray)
			{
				if (relicType != 0 && SaveManager.PlayerSaveData.GetRelic(relicType).Level > 0)
				{
					list.Add(relicType);
				}
			}
			Debug.LogException(new Exception(string.Format("m_manaChangeEventArgs was null! Class: {0} | Weapon: {1} | Spell: {2} | Talent: {3} | Actual Max Mana: {4} | Current Mana: {5} | Traits: {6} | Relics: {7}", CharacterClass.ClassType, CastAbility.WeaponAbilityType, CastAbility.SpellAbilityType, CastAbility.TalentAbilityType, ActualMaxMana, CurrentMana, string.Join(", ", TraitManager.ActiveTraitTypeList), string.Join(", ", list))));
		}
		else
		{
			m_manaChangeEventArgs.Initialise(this, CurrentMana, currentMana);
		}
		if (m_manaChangeRelay != null)
		{
			m_manaChangeRelay.Dispatch(m_manaChangeEventArgs);
		}
		else
		{
			List<RelicType> list2 = new List<RelicType>();
			RelicType[] typeArray = RelicType_RL.TypeArray;
			foreach (RelicType relicType2 in typeArray)
			{
				if (relicType2 != 0 && SaveManager.PlayerSaveData.GetRelic(relicType2).Level > 0)
				{
					list2.Add(relicType2);
				}
			}
			Debug.LogException(new Exception(string.Format("Mana Change Relay was null! Class: {0} | Weapon: {1} | Spell: {2} | Talent: {3} | Actual Max Mana: {4} | Current Mana: {5} | Traits: {6} | Relics: {7}", CharacterClass.ClassType, CastAbility.WeaponAbilityType, CastAbility.SpellAbilityType, CastAbility.TalentAbilityType, ActualMaxMana, CurrentMana, string.Join(", ", TraitManager.ActiveTraitTypeList), string.Join(", ", list2))));
		}
		Messenger<GameMessenger, GameEvent>.Broadcast(GameEvent.PlayerManaChange, this, m_manaChangeEventArgs);
	}

	public override void SetHealth(float value, bool additive, bool runEvents)
	{
		if ((SaveManager.PlayerSaveData.InHubTown || SaveManager.PlayerSaveData.EndingSpawnRoom == EndingSpawnRoomType.AboveGround) && !ChallengeManager.IsInChallenge && ((CurrentHealth + value <= 0f && additive) || (value <= 0f && !additive)))
		{
			value = 1f;
			additive = false;
		}
		if (m_healthChangeEventArgs == null)
		{
			List<RelicType> list = new List<RelicType>();
			RelicType[] typeArray = RelicType_RL.TypeArray;
			foreach (RelicType relicType in typeArray)
			{
				if (relicType != 0 && SaveManager.PlayerSaveData.GetRelic(relicType).Level > 0)
				{
					list.Add(relicType);
				}
			}
			Debug.LogException(new Exception(string.Format("m_healthChangeEventArgs was null! Class: {0} | Weapon: {1} | Spell: {2} | Talent: {3} | Actual Max Mana: {4} | Current Mana: {5} | Traits: {6} | Relics: {7}", CharacterClass.ClassType, CastAbility.WeaponAbilityType, CastAbility.SpellAbilityType, CastAbility.TalentAbilityType, ActualMaxMana, CurrentMana, string.Join(", ", TraitManager.ActiveTraitTypeList), string.Join(", ", list))));
		}
		if (additive && value > 0f && TraitManager.IsTraitActive(TraitType.MegaHealth))
		{
			value = 0f;
		}
		base.SetHealth(value, additive, runEvents);
		if (runEvents)
		{
			Messenger<GameMessenger, GameEvent>.Broadcast(GameEvent.PlayerHealthChange, this, m_healthChangeEventArgs);
		}
	}

	protected void OnEnable()
	{
		Messenger<UIMessenger, UIEvent>.AddListener(UIEvent.EquippedChanged, m_onEquippedOrLevelChanged);
		Messenger<UIMessenger, UIEvent>.AddListener(UIEvent.EquipmentPurchasedLevelChanged, m_onEquippedOrLevelChanged);
		Messenger<UIMessenger, UIEvent>.AddListener(UIEvent.RuneEquippedLevelChanged, m_onEquippedOrLevelChanged);
		Messenger<UIMessenger, UIEvent>.AddListener(UIEvent.RunePurchaseLevelChanged, m_onEquippedOrLevelChanged);
		Messenger<GameMessenger, GameEvent>.AddListener(GameEvent.RelicLevelChanged, m_onRelicChanged);
		Messenger<GameMessenger, GameEvent>.AddListener(GameEvent.UpdatePools, m_applyPermanentStatusEffects);
	}

	protected void OnDisable()
	{
		Messenger<UIMessenger, UIEvent>.RemoveListener(UIEvent.EquippedChanged, m_onEquippedOrLevelChanged);
		Messenger<UIMessenger, UIEvent>.RemoveListener(UIEvent.EquipmentPurchasedLevelChanged, m_onEquippedOrLevelChanged);
		Messenger<UIMessenger, UIEvent>.RemoveListener(UIEvent.RuneEquippedLevelChanged, m_onEquippedOrLevelChanged);
		Messenger<UIMessenger, UIEvent>.RemoveListener(UIEvent.RunePurchaseLevelChanged, m_onEquippedOrLevelChanged);
		Messenger<GameMessenger, GameEvent>.RemoveListener(GameEvent.RelicLevelChanged, m_onRelicChanged);
		Messenger<GameMessenger, GameEvent>.RemoveListener(GameEvent.UpdatePools, m_applyPermanentStatusEffects);
	}

	private IEnumerator RollCoroutine(float duration)
	{
		float startTime = Time.time;
		while (Time.time < startTime + duration)
		{
			yield return null;
		}
		JustRolled = false;
	}

	private void OnEquippedOrLevelChanged(MonoBehaviour sender, EventArgs args)
	{
		InitializeAllMods(resetHP: true, resetMP: true);
	}

	public float GetActualStatValue(PlayerStat stat)
	{
		return stat switch
		{
			PlayerStat.Vitality => ActualVitality, 
			PlayerStat.Strength => ActualStrength, 
			PlayerStat.Magic => ActualMagic, 
			PlayerStat.Focus => ActualFocus, 
			PlayerStat.Dexterity => ActualDexterity, 
			_ => throw new Exception("Attempting to scale off of an unknown PlayerStat"), 
		};
	}

	public void UpdateFrameAccumulatedXP(float amount)
	{
		m_updateAccumulatedXP += amount;
	}

	public void UpdateFrameAccumulatedLifeSteal(float amount)
	{
		m_updateAccumulatedLifeSteal += amount;
	}

	private void LateUpdate()
	{
		if (m_updateAccumulatedXP > 0f)
		{
			if ((bool)m_xpPopup && m_xpPopup.gameObject.activeSelf)
			{
				m_xpPopup.gameObject.SetActive(value: false);
			}
			string text = string.Format(LocalizationManager.GetString("LOC_ID_XP_UI_XP_POPUP_1", isFemale: false), m_updateAccumulatedXP);
			m_xpPopup = TextPopupManager.DisplayTextDefaultPos(TextPopupType.XPGain, text, this, attachToSource: true, queuePopup: false);
			m_updateAccumulatedXP = 0f;
		}
		if (m_updateAccumulatedLifeSteal > 0f)
		{
			string text2 = string.Format(LocalizationManager.GetString("LOC_ID_STATUS_EFFECT_HEALTH_RESTORE_1", SaveManager.PlayerSaveData.CurrentCharacter.IsFemale), (int)m_updateAccumulatedLifeSteal);
			TextPopupManager.DisplayTextDefaultPos(TextPopupType.HPGained, text2, this, attachToSource: true);
			m_updateAccumulatedLifeSteal = 0f;
		}
	}

	protected override void OnJustGrounded()
	{
		if (!EffectManager.AnimatorEffectsDisabled(m_animator) && !SceneLoader_RL.IsRunningTransitionWithLogic && !RewiredMapController.IsInCutscene)
		{
			RelicObj relic = SaveManager.PlayerSaveData.GetRelic(RelicType.LandShockwave);
			if (relic.Level > 0)
			{
				Projectile_RL projectile_RL = ProjectileManager.FireProjectile(base.gameObject, "RelicLandShockwaveProjectile", new Vector2(0f, 0f));
				projectile_RL.CastAbilityType = CastAbilityType.Talent;
				projectile_RL.MagicScale += 0.75f * (float)(relic.Level - 1);
				projectile_RL.ActualCritDamage = ProjectileManager.CalculateProjectileCritDamage(projectile_RL, getMagicCritDmg: true);
			}
		}
	}

	private void OnRelicChanged(object sender, EventArgs args)
	{
		InitializeAbilities();
		InitializeAllMods(resetHP: false, resetMP: false);
	}

	private void CheckForBiomeChange(BiomeType roomBiome)
	{
		if (CurrentlyInRoom == null || (CurrentlyInRoom != null && CurrentlyInRoom.BiomeType != roomBiome))
		{
			if (m_biomeEventArgs == null)
			{
				m_biomeEventArgs = new BiomeEventArgs();
			}
			BiomeType biome = BiomeType.None;
			if (CurrentlyInRoom != null)
			{
				biome = CurrentlyInRoom.BiomeType;
			}
			m_biomeEventArgs.SetBiome(biome);
			Messenger<GameMessenger, GameEvent>.Broadcast(GameEvent.BiomeExit, this, m_biomeEventArgs);
			m_biomeEventArgs.SetBiome(roomBiome);
			Messenger<GameMessenger, GameEvent>.Broadcast(GameEvent.BiomeEnter, this, m_biomeEventArgs);
		}
	}

	protected override void Awake()
	{
		base.Awake();
		m_characterDash = base.gameObject.GetComponent<CharacterDash_RL>();
		m_characterDownStrike = base.gameObject.GetComponent<CharacterDownStrike_RL>();
		m_characterAbilities = base.gameObject.GetComponent<CastAbility_RL>();
		m_characterMove = base.gameObject.GetComponent<CharacterHorizontalMovement_RL>();
		m_characterJump = base.gameObject.GetComponent<CharacterJump_RL>();
		m_characterFlight = base.gameObject.GetComponent<CharacterFlight_RL>();
		m_lookController = base.gameObject.GetComponentInChildren<PlayerLookController>();
		m_characterClass = base.gameObject.GetComponent<CharacterClass>();
		m_interactIconController = base.gameObject.GetComponentInChildren<InteractIconController>();
		m_playerDeathEventArgs = new PlayerDeathEventArgs(this, null);
		m_manaChangeEventArgs = new ManaChangeEventArgs(this, 0f, 0f);
		m_traitChangeEventArgs = new TraitChangedEventArgs(TraitType.None, TraitType.None);
		m_onHitAreaDamage_waitYield = new WaitRL_Yield(1f);
		m_noAttackDamageBonus_waitYield = new WaitRL_Yield(1f);
		CameraLayerController component = GetComponent<CameraLayerController>();
		component.SetCameraLayer(CameraLayer.Game);
		component.SetSubLayer(CameraLayerUtility.DefaultPlayerSubLayer);
		m_onEquippedOrLevelChanged = OnEquippedOrLevelChanged;
		m_onRelicChanged = OnRelicChanged;
		m_applyPermanentStatusEffects = ApplyPermanentStatusEffects;
	}

	public override void ResetBaseValues()
	{
		base.BaseScale = 1.4f;
		BaseMaxMana = 100;
		BaseAllowedEquipmentWeight = 50;
		BaseRuneWeight = 50;
		BaseArmor = 0;
		BaseCritDamage = 1.1f;
		BaseMagicCritDamage = 1.1f;
		BaseResolve = 1f;
		base.BaseStrength = 15f;
		base.BaseMagic = 15f;
		BaseDexterity = 5f;
		BaseFocus = 5f;
		BaseVitality = 15;
		BaseStunDefense = -1f;
		BaseKnockbackDefense = -1f;
		MovementSpeedMod = 1f;
		base.StunDuration = 99f;
		base.BaseInvincibilityDuration = 1f;
		InitializeAbilities();
		base.ResetBaseValues();
	}

	public void InitializeAbilities()
	{
		CharacterMove.WalkSpeed = 12f;
		float num = 1f + RuneLogicHelper.GetHastePercent();
		CharacterMove.WalkSpeed *= num;
		CharacterMove.MovementSpeed = CharacterMove.WalkSpeed;
		base.ControllerCorgi.DefaultParameters.SpeedAccelerationOnGround = 120f;
		base.ControllerCorgi.DefaultParameters.SpeedAccelerationInAir = 120f;
		CharacterJump.JumpHeight = 8.75f;
		CharacterJump.DoubleJumpHeight = 4.75f;
		CharacterJump.JumpReleaseForce = 4.5f;
		CharacterJump.JumpTimeWindow = 0.1375f;
		CharacterJump.CanJumpWhileDashing = true;
		int extraJumps = RuneLogicHelper.GetExtraJumps();
		extraJumps += (int)EquipmentManager.Get_EquipmentSet_BonusTypeStatGain(EquipmentSetBonusType.ExtraJump);
		extraJumps += (int)EquipmentManager.Get_EquipmentSet_BonusTypeStatGain(EquipmentSetBonusType.ExtraDashJump);
		int level = SaveManager.PlayerSaveData.GetRelic(RelicType.LowMultiJump).Level;
		if (level > 0)
		{
			extraJumps += level * 3;
			CharacterJump.JumpHeight /= 2f;
		}
		CharacterJump.NumberOfJumps = 1;
		if (CharacterJump.NumberOfJumps <= 1 && SaveManager.PlayerSaveData.GetHeirloomLevel(HeirloomType.UnlockDoubleJump) > 0)
		{
			CharacterJump.NumberOfJumps = 2;
		}
		CharacterJump.NumberOfJumps += extraJumps;
		CharacterJump.ResetNumberOfJumps();
		CharacterDash.EnableOmnidash = false;
		if (TraitManager.IsTraitActive(TraitType.OmniDash))
		{
			CharacterDash.EnableOmnidash = true;
		}
		CharacterDash.DashDistance = 8f;
		CharacterDash.DashForce = 26f;
		CharacterDash.DashCooldown = 0f;
		int extraDashes = RuneLogicHelper.GetExtraDashes();
		extraDashes += (int)EquipmentManager.Get_EquipmentSet_BonusTypeStatGain(EquipmentSetBonusType.ExtraDash);
		extraDashes += (int)EquipmentManager.Get_EquipmentSet_BonusTypeStatGain(EquipmentSetBonusType.ExtraDashJump);
		CharacterDash.TotalDashesAllowed = 0;
		if (CharacterDash.TotalDashesAllowed <= 0 && SaveManager.PlayerSaveData.GetHeirloomLevel(HeirloomType.UnlockAirDash) > 0)
		{
			CharacterDash.TotalDashesAllowed = 1;
		}
		CharacterDash.TotalDashesAllowed += extraDashes;
		CharacterDash.ResetNumberOfDashes();
		CharacterDownStrike.AttackSpeed = 32f;
		CharacterDownStrike.ForwardKickAngle = -55f;
		CharacterDownStrike.ForwardKickMinMaxAngle = Player_EV.DOWN_STRIKE_FORWARDKICK_MINMAX_ANGLE;
		CharacterDownStrike.DownKickMinMaxAngle = Player_EV.DOWN_STRIKE_DOWNKICK_MINMAX_ANGLE;
		CharacterDownStrike.AttackBounceHeight = 22f;
		CharacterDownStrike.BounceInputLockDuration = 0.275f;
		CharacterDownStrike.ResetsDoubleJump = false;
		CharacterDownStrike.ResetsDash = false;
	}

	public void InitializeAllMods(bool resetHP, bool resetMP)
	{
		ResetMods();
		InitializeWeightMods();
		InitializeHealthMods();
		InitializeManaMods();
		InitializeStrengthMods();
		InitializeMagicMods();
		InitializeAbilityMods();
		InitializeInvincibilityMods();
		InitializeArmorMods();
		InitializeCritMods();
		InitializeExhaustMods();
		int value = (int)EquipmentManager.Get_EquipmentSet_BonusTypeStatGain(EquipmentSetBonusType.Revives);
		if (SaveManager.PlayerSaveData.GetRelic(RelicType.ExtraLife_UnityUsed).Level <= 0 && !base.IsDead)
		{
			SaveManager.PlayerSaveData.GetRelic(RelicType.ExtraLife_Unity).SetLevel(value, additive: false, broadcast: false);
		}
		else
		{
			SaveManager.PlayerSaveData.GetRelic(RelicType.ExtraLife_Unity).SetLevel(0, additive: false, broadcast: false);
		}
		Messenger<GameMessenger, GameEvent>.Broadcast(GameEvent.RelicStatsChanged, this, m_extraLifeEventArgs);
		if (resetHP)
		{
			ResetHealth();
		}
		if (resetMP)
		{
			ResetMana();
		}
	}

	public void InitializeHealthMods()
	{
		InitializeResolveHealthMods();
		InitializeVitalityMods();
		InitializeTraitHealthMods();
		InitializeRelicMaxHealthMods();
		InitializeMaxHealthMods();
		m_healthChangeEventArgs.Initialise(this, ActualMaxHealth, ActualMaxHealth);
		Messenger<GameMessenger, GameEvent>.Broadcast(GameEvent.PlayerHealthChange, this, m_healthChangeEventArgs);
		Messenger<GameMessenger, GameEvent>.Broadcast(GameEvent.PlayerResolveChanged, this, null);
	}

	private void InitializeVitalityMods()
	{
		int num = 0;
		num += (int)EquipmentManager.GetTotalEquippedStatValue(EquipmentStatType.Vitality);
		num += SkillTreeLogicHelper.GetVitalityAdds();
		num += (int)EquipmentManager.Get_EquipmentSet_BonusTypeStatGain(EquipmentSetBonusType.VitalityAdd);
		num += (int)Mastery_EV.GetTotalMasteryBonus(MasteryBonusType.Health_Up);
		VitalityAdd = num;
		float num2 = 0f;
		num2 += CharacterClass.ClassData.PassiveData.VitalityMod - 1f;
		num2 += Mastery_EV.GetTotalMasteryBonus(MasteryBonusType.Health_Up_Mod);
		num2 += EquipmentManager.Get_EquipmentSet_BonusTypeStatGain(EquipmentSetBonusType.VitalityMod);
		VitalityMod = num2;
	}

	private void InitializeTraitHealthMods()
	{
		float num = 0f;
		if (TraitManager.IsTraitActive(TraitType.BonusHealth))
		{
			num += 0.1f;
		}
		if (TraitManager.IsTraitActive(TraitType.InvulnDash))
		{
			num -= 0.5f;
		}
		if (TraitManager.IsTraitActive(TraitType.MagicBoost))
		{
			num -= 0.3f;
		}
		if (TraitManager.IsTraitActive(TraitType.DamageBoost))
		{
			num -= 0.3f;
		}
		if (TraitManager.IsTraitActive(TraitType.CantAttack))
		{
			num -= 0.6f;
		}
		if (TraitManager.IsTraitActive(TraitType.CanNowAttack))
		{
			num -= 0.6f;
		}
		if (TraitManager.IsTraitActive(TraitType.SmallHitbox))
		{
			num -= 0.25f;
		}
		if (TraitManager.IsTraitActive(TraitType.BonusMagicStrength))
		{
			num -= 0.65f;
		}
		if (TraitManager.IsTraitActive(TraitType.RevealAllChests))
		{
			num -= 0.19999999f;
		}
		if (TraitManager.IsTraitActive(TraitType.SuperHealer))
		{
			num -= 0f;
		}
		if (TraitManager.IsTraitActive(TraitType.OmniDash))
		{
			num -= 0.19999999f;
		}
		if (TraitManager.IsTraitActive(TraitType.BounceTerrain))
		{
			num -= 0.3f;
		}
		if (TraitManager.IsTraitActive(TraitType.MegaHealth))
		{
			num += 1f;
		}
		TraitMaxHealthMod = num;
	}

	private void InitializeRelicMaxHealthMods()
	{
		float num = 0f;
		num += SaveManager.PlayerSaveData.TemporaryMaxHealthMods;
		num += (float)SaveManager.PlayerSaveData.GetRelic(RelicType.ReplacementRelic).Level * 0.1f;
		num += (float)SaveManager.PlayerSaveData.GetRelic(RelicType.FoodChallengeUsed).Level * 0.3f;
		RelicMaxHealthMod = num;
	}

	private void InitializeResolveHealthMods()
	{
		float num = 0f;
		num -= SaveManager.PlayerSaveData.GetTotalRelicResolveCost();
		if (!ChallengeManager.IsInChallenge)
		{
			int weightLevel = EquipmentManager.GetWeightLevel();
			weightLevel = Mathf.Clamp(weightLevel, 0, Equipment_EV.CRIT_DAMAGE_BONUS_PER_WEIGHT_LEVEL.Length - 1);
			float num2 = Equipment_EV.RESOLVE_BONUS_PER_WEIGHT_LEVEL[weightLevel];
			num += num2;
		}
		num += RuneLogicHelper.GetResolveAdd();
		num += SkillTreeLogicHelper.GetResolveAdds();
		num += EquipmentManager.Get_EquipmentSet_BonusTypeStatGain(EquipmentSetBonusType.Resolve);
		int num3 = BurdenManager.BurdenRequiredForNG(SaveManager.PlayerSaveData.NewGamePlusLevel);
		if (num3 < 0)
		{
			SoulShopObj soulShopObj = SaveManager.ModeSaveData.GetSoulShopObj(SoulShopType.BurdenOverload);
			if (!soulShopObj.IsNativeNull())
			{
				num += soulShopObj.CurrentStatGain * (float)Mathf.Abs(num3);
			}
		}
		ResolveAdd = num;
		float num4 = 0f;
		if (SaveManager.PlayerSaveData.GetRelic(RelicType.GoldIntoResolve).Level > 0)
		{
			num4 += Economy_EV.GetGoldGainMod();
			num4 *= NPC_EV.GetArchitectGoldMod();
		}
		ResolveMod = num4;
	}

	private void InitializeMaxHealthMods()
	{
		float maxHealthMod = 0f;
		base.MaxHealthMod = maxHealthMod;
		if (CurrentHealth > (float)ActualMaxHealth)
		{
			SetHealth(ActualMaxHealth, additive: false, runEvents: false);
		}
	}

	public void InitializeManaMods()
	{
		InitializeMaxManaAdds();
		InitializeEquipmentMaxManaMods();
		InitializeTraitMaxManaMods();
		InitializeManaRegenMods();
		m_manaChangeEventArgs.Initialise(this, ActualMaxMana, ActualMaxMana);
		Messenger<GameMessenger, GameEvent>.Broadcast(GameEvent.PlayerManaChange, this, m_manaChangeEventArgs);
	}

	private void InitializeMaxManaAdds()
	{
		int num = 0;
		num += RuneLogicHelper.GetMaxManaFlat();
		int level = SaveManager.PlayerSaveData.GetRelic(RelicType.BonusMana).Level;
		num += 50 * level;
		RelicObj relic = SaveManager.PlayerSaveData.GetRelic(RelicType.SpellKillMaxMana);
		num += relic.IntValue;
		int level2 = SaveManager.PlayerSaveData.GetRelic(RelicType.FoodChallengeUsed).Level;
		num += 50 * level2;
		num += (int)EquipmentManager.Get_EquipmentSet_BonusTypeStatGain(EquipmentSetBonusType.MaxMana);
		PostModMaxManaAdd = num;
	}

	private void InitializeTraitMaxManaMods()
	{
		float num = 0f;
		if (TraitManager.IsTraitActive(TraitType.DamageBoost))
		{
			num += 0f;
		}
		if (TraitManager.IsTraitActive(TraitType.MagicBoost))
		{
			num += 0.5f;
		}
		if (TraitManager.IsTraitActive(TraitType.BonusMagicStrength))
		{
			num += -0.25f;
		}
		TraitMaxManaMod = num;
	}

	private void InitializeEquipmentMaxManaMods()
	{
		float equipmentMaxManaMod = 0f;
		EquipmentMaxManaMod = equipmentMaxManaMod;
	}

	private void InitializeManaRegenMods()
	{
		float num = 0f;
		num += RuneLogicHelper.GetManaRegenPercent();
		num += EquipmentManager.Get_EquipmentSet_BonusTypeStatGain(EquipmentSetBonusType.ManaRegenMod);
		int weightLevel = EquipmentManager.GetWeightLevel();
		num += Mathf.Clamp((float)weightLevel * -0f, -1f, 0f);
		ManaRegenMod = num;
	}

	public void InitializeStrengthMods()
	{
		int num = 0;
		num += (int)EquipmentManager.GetTotalEquippedStatValue(EquipmentStatType.Strength);
		num += SkillTreeLogicHelper.GetStrengthAdds();
		num += (int)EquipmentManager.Get_EquipmentSet_BonusTypeStatGain(EquipmentSetBonusType.StrengthAdd);
		num += (int)Mastery_EV.GetTotalMasteryBonus(MasteryBonusType.Strength_Up);
		base.StrengthAdd = num;
		int strengthTemporaryAdd = 0;
		base.StrengthTemporaryAdd = strengthTemporaryAdd;
		float num2 = 0f;
		num2 += CharacterClass.ClassData.PassiveData.StrengthMod - 1f;
		num2 += Mastery_EV.GetTotalMasteryBonus(MasteryBonusType.Strength_Up_Mod);
		num2 += EquipmentManager.Get_EquipmentSet_BonusTypeStatGain(EquipmentSetBonusType.StrengthMod);
		base.StrengthMod = num2;
	}

	public void InitializeMagicMods()
	{
		int num = 0;
		num += (int)EquipmentManager.GetTotalEquippedStatValue(EquipmentStatType.Magic);
		num += SkillTreeLogicHelper.GetMagicAdds();
		num += (int)EquipmentManager.Get_EquipmentSet_BonusTypeStatGain(EquipmentSetBonusType.MagicAdd);
		num += (int)Mastery_EV.GetTotalMasteryBonus(MasteryBonusType.Magic_Up);
		base.MagicAdd = num;
		int magicTemporaryAdd = 0;
		base.MagicTemporaryAdd = magicTemporaryAdd;
		float num2 = 0f;
		num2 += CharacterClass.ClassData.PassiveData.IntelligenceMod - 1f;
		num2 += Mastery_EV.GetTotalMasteryBonus(MasteryBonusType.Magic_Up_Mod);
		num2 += EquipmentManager.Get_EquipmentSet_BonusTypeStatGain(EquipmentSetBonusType.MagicMod);
		base.MagicMod = num2;
	}

	public void InitializeAbilityMods()
	{
		float num = 0f;
		num += SkillTreeLogicHelper.GetAbilityCooldownMods();
		AbilityCoolDownMod = num;
	}

	public void InitializeInvincibilityMods()
	{
		float num = 0f;
		num += SkillTreeLogicHelper.GetInvulnTimeExtension();
		num += (float)SaveManager.PlayerSaveData.GetRelic(RelicType.ExtendInvuln).Level * 1.25f;
		base.InvincibilityDurationAdd = num;
	}

	public void InitializeWeightMods()
	{
		int num = 0;
		num += SkillTreeLogicHelper.GetEquipWeightAdds();
		num += (int)EquipmentManager.Get_EquipmentSet_BonusTypeStatGain(EquipmentSetBonusType.WeightReduction);
		num += (int)Mastery_EV.GetTotalMasteryBonus(MasteryBonusType.EquipmentWeight_Up);
		AllowedEquipmentWeightAdds = num;
		int num2 = 0;
		num2 += SkillTreeLogicHelper.GetRuneWeightAdds();
		num2 += (int)Mastery_EV.GetTotalMasteryBonus(MasteryBonusType.RuneWeight_Up);
		RuneWeightAdds = num2;
	}

	public void InitializeExhaustMods()
	{
		int num = 0;
		num += SaveManager.PlayerSaveData.GetRelic(RelicType.AttackExhaust).Level * 25;
		CurrentExhaust = num;
	}

	public void InitializeArmorMods()
	{
		int num = 0;
		num += (int)EquipmentManager.GetTotalEquippedStatValue(EquipmentStatType.Armor);
		num += SkillTreeLogicHelper.GetArmorAdds();
		num += (int)EquipmentManager.Get_EquipmentSet_BonusTypeStatGain(EquipmentSetBonusType.Armor);
		num += (int)Mastery_EV.GetTotalMasteryBonus(MasteryBonusType.Armor_Add_Up);
		float num2 = 1f;
		num2 += CharacterClass.ClassData.PassiveData.ArmorMod - 1f;
		num2 += EquipmentManager.Get_EquipmentSet_BonusTypeStatGain(EquipmentSetBonusType.ArmorMod);
		num2 += Mastery_EV.GetTotalMasteryBonus(MasteryBonusType.Armor_Add_Up_Mod);
		num2 += RuneLogicHelper.GetArmorRegenMod();
		ArmorAdds = (int)((float)num * num2);
	}

	public void InitializeCritMods()
	{
		float num = 0f;
		num += (float)(int)EquipmentManager.GetTotalEquippedStatValue(EquipmentStatType.Dexterity_Add);
		num += SkillTreeLogicHelper.GetDexterityAdds();
		num += (float)(int)EquipmentManager.Get_EquipmentSet_BonusTypeStatGain(EquipmentSetBonusType.Dexterity_Add);
		num += (float)(int)Mastery_EV.GetTotalMasteryBonus(MasteryBonusType.Dexterity_Add);
		DexterityAdd = num;
		float dexterityTemporaryAdd = 0f;
		DexterityTemporaryAdd = dexterityTemporaryAdd;
		float num2 = 0f;
		num2 += CharacterClass.ClassData.PassiveData.DexterityMod - 1f;
		num2 += EquipmentManager.Get_EquipmentSet_BonusTypeStatGain(EquipmentSetBonusType.DexterityMod);
		num2 += Mastery_EV.GetTotalMasteryBonus(MasteryBonusType.Dexterity_Mod);
		DexterityMod = num2;
		float dexterityTemporaryMod = 0f;
		DexterityTemporaryMod = dexterityTemporaryMod;
		float num3 = 0f;
		num3 += CharacterClass.ClassData.PassiveData.WeaponCritChanceAdd;
		num3 += SkillTreeLogicHelper.GetCritChanceAdds();
		num3 += RuneLogicHelper.GetWeaponCritChanceAdd();
		num3 += EquipmentManager.Get_EquipmentSet_BonusTypeStatGain(EquipmentSetBonusType.FlatWeaponCritChance);
		CritChanceAdd = num3;
		float num4 = 0f;
		num4 += (float)SaveManager.PlayerSaveData.GetRelic(RelicType.AllCritChanceUp).Level * 0.1f;
		CritChanceTemporaryAdd = num4;
		float num5 = 0f;
		num5 += CharacterClass.ClassData.PassiveData.WeaponCritDamageAdd;
		num5 += EquipmentManager.GetTotalEquippedStatValue(EquipmentStatType.CritDamage);
		num5 += SkillTreeLogicHelper.GetCritDamageAdds();
		num5 += EquipmentManager.Get_EquipmentSet_BonusTypeStatGain(EquipmentSetBonusType.CritDamage);
		num5 += (float)(int)Mastery_EV.GetTotalMasteryBonus(MasteryBonusType.WeaponCritDamage_Up);
		num5 += RuneLogicHelper.GetWeaponCritDamageAdd();
		int weightLevel = EquipmentManager.GetWeightLevel();
		weightLevel = Mathf.Clamp(weightLevel, 0, Equipment_EV.CRIT_DAMAGE_BONUS_PER_WEIGHT_LEVEL.Length - 1);
		float num6 = Equipment_EV.CRIT_DAMAGE_BONUS_PER_WEIGHT_LEVEL[weightLevel];
		num5 += num6;
		CritDamageAdd = num5;
		float num7 = 0f;
		num7 += (float)SaveManager.PlayerSaveData.GetRelic(RelicType.AllCritDamageUp).Level * 0.2f;
		CritDamageTemporaryAdd = num7;
		float num8 = 0f;
		num8 += (float)(int)EquipmentManager.GetTotalEquippedStatValue(EquipmentStatType.Focus_Add);
		num8 += SkillTreeLogicHelper.GetFocusAdds();
		num8 += (float)(int)EquipmentManager.Get_EquipmentSet_BonusTypeStatGain(EquipmentSetBonusType.Focus_Add);
		num8 += (float)(int)Mastery_EV.GetTotalMasteryBonus(MasteryBonusType.Focus_Add);
		FocusAdd = num8;
		float focusTemporaryAdd = 0f;
		FocusTemporaryAdd = focusTemporaryAdd;
		float num9 = 0f;
		num9 += CharacterClass.ClassData.PassiveData.FocusMod - 1f;
		num9 += Mastery_EV.GetTotalMasteryBonus(MasteryBonusType.Focus_Mod);
		num9 += EquipmentManager.Get_EquipmentSet_BonusTypeStatGain(EquipmentSetBonusType.FocusMod);
		FocusMod = num9;
		float focusTemporaryMod = 0f;
		FocusTemporaryMod = focusTemporaryMod;
		float num10 = 0f;
		num10 += CharacterClass.ClassData.PassiveData.MagicCritChanceAdd;
		num10 += SkillTreeLogicHelper.GetMagicCritChanceAdds();
		num10 += RuneLogicHelper.GetMagicCritChanceAdd();
		num10 += EquipmentManager.Get_EquipmentSet_BonusTypeStatGain(EquipmentSetBonusType.FlatMagicCritChance);
		MagicCritChanceAdd = num10;
		float num11 = 0f;
		num11 += (float)SaveManager.PlayerSaveData.GetRelic(RelicType.AllCritChanceUp).Level * 0.1f;
		MagicCritChanceTemporaryAdd = num11;
		float num12 = 0f;
		num12 += CharacterClass.ClassData.PassiveData.MagicCritDamageAdd;
		num12 += EquipmentManager.GetTotalEquippedStatValue(EquipmentStatType.MagicCritDamage);
		num12 += SkillTreeLogicHelper.GetMagicCritDamageAdds();
		num12 += EquipmentManager.Get_EquipmentSet_BonusTypeStatGain(EquipmentSetBonusType.MagicCritDamage);
		num12 += (float)(int)Mastery_EV.GetTotalMasteryBonus(MasteryBonusType.MagicCritDamage_Up);
		num12 += RuneLogicHelper.GetMagicCritDamageAdd();
		float num13 = Equipment_EV.MAGIC_CRIT_DAMAGE_BONUS_PER_WEIGHT_LEVEL[weightLevel];
		num12 += num13;
		MagicCritDamageAdd = num12;
		float num14 = 0f;
		num14 += (float)SaveManager.PlayerSaveData.GetRelic(RelicType.AllCritDamageUp).Level * 0.2f;
		MagicCritDamageTemporaryAdd = num14;
	}

	public override void ResetMods()
	{
		JustRolled = false;
		base.ResetMods();
		AbilityCoolDownMod = 0f;
		TraitMaxManaMod = 0f;
		PostModMaxManaAdd = 0;
		VitalityAdd = 0;
		TraitMaxHealthMod = 0f;
		RelicMaxHealthMod = 0f;
		base.InvincibilityDurationAdd = 0f;
		RuneWeightAdds = 0;
		AllowedEquipmentWeightAdds = 0;
		ArmorAdds = 0;
		DexterityAdd = 0f;
		DexterityMod = 0f;
		DexterityTemporaryAdd = 0f;
		DexterityTemporaryMod = 0f;
		CritDamageAdd = 0f;
		CritDamageTemporaryAdd = 0f;
		CritChanceAdd = 0f;
		CritChanceTemporaryAdd = 0f;
		FocusAdd = 0f;
		FocusMod = 0f;
		FocusTemporaryAdd = 0f;
		FocusTemporaryMod = 0f;
		MagicCritDamageAdd = 0f;
		MagicCritDamageTemporaryAdd = 0f;
		MagicCritChanceAdd = 0f;
		MagicCritChanceTemporaryAdd = 0f;
	}

	public void ResetGravityAndMovement()
	{
		if (!base.ControllerCorgi)
		{
			return;
		}
		base.FallMultiplierOverride = 1f;
		base.AscentMultiplierOverride = 1f;
		CharacterMove.MovementSpeedMultiplier = 1f;
		if (TraitManager.IsTraitActive(TraitType.LowerGravity))
		{
			LowerGravity_Trait lowerGravity_Trait = TraitManager.GetActiveTrait(TraitType.LowerGravity) as LowerGravity_Trait;
			if ((bool)lowerGravity_Trait)
			{
				base.AscentMultiplierOverride *= 1f;
				base.FallMultiplierOverride *= 0.35f;
				lowerGravity_Trait.LowerGravityApplied = true;
			}
		}
	}

	public override void ResetCharacter()
	{
		base.ResetCharacter();
		base.Pivot.SetActive(value: true);
		PlayerManager.GetPlayerController().ControllerCorgi.enabled = true;
		base.TakesNoDamage = false;
		Messenger<GameMessenger, GameEvent>.Broadcast(GameEvent.GoldSavedChanged, this, null);
		Messenger<GameMessenger, GameEvent>.Broadcast(GameEvent.GoldChanged, this, null);
		SkillTreeManager.ResetCachedTotalSkills();
		Messenger<UIMessenger, UIEvent>.Broadcast(UIEvent.SkillLevelChanged, this, null);
		SaveManager.PlayerSaveData.TemporaryHeirloomList.Clear();
		if ((bool)base.ControllerCorgi)
		{
			base.ControllerCorgi.GravityActive(state: true);
		}
		m_animator.updateMode = AnimatorUpdateMode.Normal;
		m_animator.SetBool("Victory", value: false);
		for (int i = 0; i < 12; i++)
		{
			m_animator.ResetTrigger("Death" + i);
		}
		m_animator.Play("Idle", 0);
		m_animator.Update(1f);
		InitializeAllMods(resetHP: true, resetMP: true);
		if (!CharacterClass.OverrideSaveFileValues)
		{
			LookController.InitializeLook(SaveManager.PlayerSaveData.CurrentCharacter);
			base.ControllerCorgi.SetRaysParameters();
			if (SaveManager.PlayerSaveData.CurrentCharacter.ClassType != CharacterClass.ClassType)
			{
				CharacterClass.ClassType = SaveManager.PlayerSaveData.CurrentCharacter.ClassType;
			}
			if (SaveManager.PlayerSaveData.CurrentCharacter.Weapon != CharacterClass.WeaponAbilityType)
			{
				CharacterClass.SetAbility(CastAbilityType.Weapon, SaveManager.PlayerSaveData.CurrentCharacter.Weapon);
			}
			else
			{
				CastAbility.ReinitializeAbility(CastAbilityType.Weapon);
			}
			if (SaveManager.PlayerSaveData.CurrentCharacter.Spell != CharacterClass.SpellAbilityType)
			{
				CharacterClass.SetAbility(CastAbilityType.Spell, SaveManager.PlayerSaveData.CurrentCharacter.Spell);
			}
			else
			{
				CastAbility.ReinitializeAbility(CastAbilityType.Spell);
			}
			if (SaveManager.PlayerSaveData.CurrentCharacter.Talent != CharacterClass.TalentAbilityType)
			{
				CharacterClass.SetAbility(CastAbilityType.Talent, SaveManager.PlayerSaveData.CurrentCharacter.Talent);
			}
			else
			{
				CastAbility.ReinitializeAbility(CastAbilityType.Talent);
			}
			Messenger<GameMessenger, GameEvent>.Broadcast(GameEvent.UpdateAbilityHUD, this, null);
		}
		else
		{
			SaveManager.PlayerSaveData.CurrentCharacter.ClassType = CharacterClass.ClassType;
			SaveManager.PlayerSaveData.CurrentCharacter.Weapon = CharacterClass.WeaponAbilityType;
			SaveManager.PlayerSaveData.CurrentCharacter.Talent = CharacterClass.TalentAbilityType;
			SaveManager.PlayerSaveData.CurrentCharacter.Spell = CharacterClass.SpellAbilityType;
		}
		if (TraitManager.IsInitialized)
		{
			TraitType traitOne = SaveManager.PlayerSaveData.CurrentCharacter.TraitOne;
			TraitType traitTwo = SaveManager.PlayerSaveData.CurrentCharacter.TraitTwo;
			if (!TraitManager.IsTraitActive(traitOne) || !TraitManager.IsTraitActive(traitTwo))
			{
				m_traitChangeEventArgs.Initialize(traitOne, traitTwo);
				Messenger<GameMessenger, GameEvent>.Broadcast(GameEvent.TraitsChanged, this, m_traitChangeEventArgs);
			}
		}
		RecreateRendererArray();
		ResetRendererArrayColor();
		base.BlinkPulseEffect.ResetAllBlackFills();
		Vector3 localPosition = base.Visuals.transform.localPosition;
		localPosition.x = 0f;
		base.Visuals.transform.localPosition = localPosition;
		base.Visuals.gameObject.SetLayerRecursively(0, includeInactive: true);
		GetComponentInChildren<CharacterSortController>().ResetCharacterLayers();
		if (m_disableAbilitiesCoroutine != null)
		{
			StopCoroutine(m_disableAbilitiesCoroutine);
		}
		SetAllAbilitiesPermitted(permitted: true);
		ResetAbilityCooldowns();
		ResetAllAbilityAmmo();
		SetMushroomBig(setMushroomBig: false, animate: false);
		if (CurrentExhaust > 0 && !base.StatusBarController.HasActiveStatusBarEntry(StatusBarEntryType.Exhaust))
		{
			base.StatusBarController.ApplyUIEffect(StatusBarEntryType.Exhaust, 99, CurrentExhaust);
		}
		ApplyPermanentStatusEffects(null, null);
	}

	public void ApplyPermanentStatusEffects(object sender, EventArgs args)
	{
		if (base.isActiveAndEnabled)
		{
			StartCoroutine(ApplyPermanentStatusEffectsCoroutine());
		}
	}

	private IEnumerator ApplyPermanentStatusEffectsCoroutine()
	{
		yield return null;
		if (CharacterClass.ClassType == ClassType.BoxingGloveClass)
		{
			base.StatusEffectController.StartStatusEffect(StatusEffectType.Player_NoContactDamage, 0f, this);
		}
		if (CharacterClass.ClassType == ClassType.GunClass)
		{
			base.StatusEffectController.StartStatusEffect(StatusEffectType.Player_Suave, 0f, this);
		}
	}

	public override void ResetHealth()
	{
		CurrentArmor = ActualArmor;
		CachedHealthOverride = 0f;
		base.ResetHealth();
	}

	public void ResetMana()
	{
		SpellOrbs = 0;
		CachedManaOverride = 0f;
		SetMana(ActualMaxMana, additive: false, runEvents: true);
	}

	public override void ResetStates()
	{
		StopActiveAbilities(stopPersistentAbilities: true);
		DisableDoorBlock = false;
		DisableArmor = false;
		ResetGravityAndMovement();
		IsBlocking = false;
		IsSpearSpinning = false;
		CloakInterrupted = false;
		BlockStartTime = 0f;
		base.ResetStates();
	}

	public void ResetAbilityCooldowns()
	{
		m_characterAbilities.ResetAbilityCooldowns(CastAbilityType.Weapon);
		m_characterAbilities.ResetAbilityCooldowns(CastAbilityType.Talent);
		m_characterAbilities.ResetAbilityCooldowns(CastAbilityType.Spell);
	}

	public void ResetAllAbilityAmmo()
	{
		m_characterAbilities.ResetAbilityAmmo(CastAbilityType.Weapon);
		m_characterAbilities.ResetAbilityAmmo(CastAbilityType.Talent);
		m_characterAbilities.ResetAbilityAmmo(CastAbilityType.Spell);
	}

	public void StopActiveAbilities(bool stopPersistentAbilities)
	{
		m_characterAbilities.StopAllAbilities(stopPersistentAbilities);
		base.Animator.SetBool("Bounce", value: false);
		base.Animator.SetBool("DanceBounce", value: false);
		switch (base.MovementState)
		{
		case CharacterStates.MovementStates.Dashing:
			m_characterDash.StopDash();
			break;
		case CharacterStates.MovementStates.DownStriking:
			m_characterDownStrike.StopDownStrike();
			break;
		case CharacterStates.MovementStates.Jumping:
		case CharacterStates.MovementStates.DoubleJumping:
			m_characterJump.JumpStop();
			m_characterJump.ResetBrakeForce();
			break;
		}
		m_characterFlight.StopFlight();
		DisableAbilitiesForXSeconds(0.1f);
	}

	public void DisableAbilitiesForXSeconds(float duration)
	{
		if (m_disableAbilitiesCoroutine != null)
		{
			StopCoroutine(m_disableAbilitiesCoroutine);
		}
		if (duration > 0f)
		{
			m_disableAbilitiesCoroutine = StartCoroutine(DisableAbilitiesCoroutine(duration));
		}
		else
		{
			SetAllAbilitiesPermitted(permitted: true);
		}
	}

	private IEnumerator DisableAbilitiesCoroutine(float duration)
	{
		SetAllAbilitiesPermitted(permitted: false);
		float startTime = Time.time;
		while (Time.time < startTime + duration)
		{
			yield return null;
		}
		SetAllAbilitiesPermitted(permitted: true);
	}

	public void SetAllAbilitiesPermitted(bool permitted)
	{
		m_characterDash.AbilityPermitted = permitted;
		m_characterAbilities.AbilityPermitted = permitted;
		m_characterDownStrike.AbilityPermitted = permitted;
		m_characterMove.AbilityPermitted = permitted;
		m_characterJump.AbilityPermitted = permitted;
	}

	private float ApplyAssistDamageMods(IDamageObj damageObj, float damageTaken)
	{
		float num = damageTaken;
		if (SaveManager.PlayerSaveData.EnableHouseRules)
		{
			num *= SaveManager.PlayerSaveData.Assist_EnemyDamageMod;
		}
		return num;
	}

	public override float CalculateDamageTaken(IDamageObj damageObj, out CriticalStrikeType critType, out float damageBlocked, float damageOverride = -1f, bool trueDamage = false, bool pureCalculation = true)
	{
		critType = CriticalStrikeType.None;
		damageBlocked = 0f;
		float num = 1f;
		if (base.TakesNoDamage)
		{
			return 0f;
		}
		if (base.StatusEffectController.HasInvulnStack)
		{
			return 0f;
		}
		Projectile_RL projectile_RL = damageObj as Projectile_RL;
		if (!DisableArmor)
		{
			RelicObj relic = SaveManager.PlayerSaveData.GetRelic(RelicType.TakeNoDamage);
			if (relic.Level > 0)
			{
				if (!pureCalculation)
				{
					relic.SetLevel(-1, additive: true);
				}
				return 0f;
			}
		}
		float actualDamage = damageObj.ActualDamage;
		if (damageOverride != -1f)
		{
			actualDamage = damageOverride;
			if (trueDamage)
			{
				return actualDamage;
			}
			actualDamage = ApplyAssistDamageMods(damageObj, actualDamage);
		}
		else
		{
			actualDamage = ApplyAssistDamageMods(damageObj, actualDamage);
			float actualCritChance = damageObj.ActualCritChance;
			float num2 = UnityEngine.Random.Range(0f, 1f);
			if (actualCritChance > 0f && actualCritChance >= num2)
			{
				actualDamage += damageObj.ActualCritDamage;
				if (actualCritChance >= 100f)
				{
					critType = CriticalStrikeType.Guaranteed;
				}
				else
				{
					critType = CriticalStrikeType.Regular;
				}
			}
			else if ((damageObj.gameObject.CompareTag("Enemy") || damageObj.gameObject.CompareTag("EnemyProjectile")) && BurdenManager.GetBurdenLevel(BurdenType.EnemyArmorShred) > 0 && CurrentArmor <= 0)
			{
				num += 1.15f;
				critType = CriticalStrikeType.Guaranteed;
			}
			if ((bool)projectile_RL)
			{
				num += projectile_RL.DamageMod;
			}
		}
		if (TraitManager.IsTraitActive(TraitType.Vampire))
		{
			num += 1.25f;
		}
		num += 1f * (float)SaveManager.PlayerSaveData.GetRelic(RelicType.BonusDamageCurse).Level;
		num += 1f * (float)SaveManager.PlayerSaveData.GetRelic(RelicType.FlightBonusCurse).Level;
		if (base.StatusEffectController.HasStatusEffect(StatusEffectType.Player_GodMode))
		{
			num += 0f;
		}
		if (base.MovementState == CharacterStates.MovementStates.Dashing)
		{
			num -= SkillTreeLogicHelper.GetDashDamageMod();
		}
		num = Mathf.Clamp(num, 0f, 999999f);
		actualDamage *= num;
		int currentArmor = CurrentArmor;
		if (!DisableArmor)
		{
			if (IsBlocking)
			{
				float num3 = 0f;
				num3 = ((!(Time.time < BlockStartTime + 0.135f)) ? 0.5f : 0f);
				actualDamage *= num3;
			}
			float num4 = Mathf.Min(CurrentArmor, (0.35f + RuneLogicHelper.GetArmorMinBlockAdd()) * actualDamage);
			float num5 = actualDamage - num4;
			int num6 = (int)(actualDamage - num5);
			if (num6 > 0 && (!IsBlocking || (IsBlocking && num5 > 0f)))
			{
				int num7 = 0;
				float num8 = 0f;
				if (damageObj.gameObject.CompareTag("Enemy") || damageObj.gameObject.CompareTag("EnemyProjectile"))
				{
					float burdenStatGain = BurdenManager.GetBurdenStatGain(BurdenType.EnemyArmorShred);
					if (burdenStatGain > 0f)
					{
						num7 += Mathf.Clamp(Mathf.RoundToInt((float)ActualArmor * burdenStatGain), 1, int.MaxValue);
					}
					EnemyController enemyController = damageObj as EnemyController;
					if ((bool)enemyController || (bool)projectile_RL)
					{
						if (!enemyController)
						{
							enemyController = projectile_RL.OwnerController as EnemyController;
						}
						if ((bool)enemyController && enemyController.StatusEffectController.HasStatusEffect(StatusEffectType.Enemy_ArmorShred))
						{
							num8 += 0.1f;
						}
					}
				}
				num7 += Mathf.RoundToInt(num8 * (float)ActualArmor);
				CurrentArmor -= num7;
			}
			actualDamage -= (float)num6;
			damageBlocked += num6;
			damageBlocked = Mathf.RoundToInt(damageBlocked);
		}
		actualDamage = Mathf.Max(0f, Mathf.Floor(actualDamage));
		if (actualDamage > 0f)
		{
			if (SaveManager.PlayerSaveData.GetRelic(RelicType.ResolveCombatChallenge).Level > 0)
			{
				actualDamage = 99999f;
			}
			if (SaveManager.PlayerSaveData.GetRelic(RelicType.GoldCombatChallenge).Level > 0)
			{
				actualDamage = 99999f;
			}
			if (SaveManager.PlayerSaveData.GetRelic(RelicType.FoodChallenge).Level > 0)
			{
				actualDamage = 99999f;
			}
			RelicObj relic2 = SaveManager.PlayerSaveData.GetRelic(RelicType.FreeHitRegenerate);
			if (relic2.Level > 0)
			{
				int num9 = 6;
				num9 -= relic2.Level - 1;
				if (relic2.IntValue >= num9)
				{
					actualDamage = 0f;
					damageBlocked = 0f;
					CurrentArmor = currentArmor;
					if (!pureCalculation)
					{
						relic2.SetIntValue(0, additive: false);
						BaseCharacterHitResponse.GLYPHS_TO_ADD_TO_PLAYER_DAMAGE_HELPER.Add(RelicType.FreeHitRegenerate.ToString());
					}
				}
			}
			RelicObj relic3 = SaveManager.PlayerSaveData.GetRelic(RelicType.ManaDamageReduction);
			if (relic3.Level > 0 && actualDamage > 0f && (float)CurrentManaAsInt >= 150f && relic3.IntValue < 2 * relic3.Level)
			{
				actualDamage = Mathf.Floor(actualDamage * 0f);
				if (!pureCalculation)
				{
					SetMana(-150f, additive: true, runEvents: true);
					relic3.SetIntValue(1, additive: true);
					BaseCharacterHitResponse.GLYPHS_TO_ADD_TO_PLAYER_DAMAGE_HELPER.Add(RelicType.ManaDamageReduction.ToString());
				}
			}
			if (CurrentExhaust > 0)
			{
				float num10 = (float)CurrentExhaust * 0.01f;
				float num11 = (float)ActualMaxHealth * num10;
				actualDamage += num11;
			}
			if (!pureCalculation && CurrentHealth - actualDamage <= 0f)
			{
				int level = SaveManager.PlayerSaveData.GetRelic(RelicType.FatalBlowDodge).Level;
				if (level > 0 && UnityEngine.Random.Range(0f, 1f) <= 0.25f * (float)level)
				{
					actualDamage = 0f;
					BaseCharacterHitResponse.GLYPHS_TO_ADD_TO_PLAYER_DAMAGE_HELPER.Add(RelicType.FatalBlowDodge.ToString());
					AudioManager.PlayOneShot(this, "event:/UI/InGame/ui_ig_gravebells_lose");
				}
			}
		}
		return actualDamage;
	}

	public override void KillCharacter(GameObject killer, bool broadcastEvent)
	{
		if (base.IsDead)
		{
			return;
		}
		base.KillCharacter(killer, broadcastEvent);
		if (broadcastEvent)
		{
			m_playerDeathEventArgs.Initialize(this, killer);
			Messenger<GameMessenger, GameEvent>.Broadcast(GameEvent.PlayerDeath, this, m_playerDeathEventArgs);
			m_onPlayerDeathRelay.Dispatch(m_playerDeathEventArgs);
			m_onPreDisableRelay.Dispatch(this);
			m_onDeathEffectTriggerRelay.Dispatch((killer != null) ? killer : null);
			if ((bool)killer)
			{
				EnemyController component = killer.GetComponent<EnemyController>();
				if ((bool)component)
				{
					SaveManager.ModeSaveData.SetTimesDefeatedByEnemy(SaveManager.PlayerSaveData.GameModeType, component.EnemyType, component.EnemyRank, 1, additive: true);
				}
			}
		}
		AnimatorControllerParameter[] parameters = base.Animator.parameters;
		foreach (AnimatorControllerParameter animatorControllerParameter in parameters)
		{
			switch (animatorControllerParameter.type)
			{
			case AnimatorControllerParameterType.Bool:
				base.Animator.SetBool(animatorControllerParameter.name, value: false);
				break;
			case AnimatorControllerParameterType.Trigger:
				base.Animator.ResetTrigger(animatorControllerParameter.name);
				break;
			}
		}
		StopActiveAbilities(stopPersistentAbilities: true);
		WindowManager.SetWindowIsOpen(WindowID.PlayerDeath, isOpen: true);
	}

	public void EnterRoom(BaseRoom room, Door door, Vector3 localPosition)
	{
		CheckForBiomeChange(room.BiomeType);
		CurrentlyInRoom = room;
		base.transform.position = room.gameObject.transform.TransformPoint(localPosition);
		if (!door || room.BiomeType == BiomeType.Stone)
		{
			base.ControllerCorgi.SetLastStandingPosition(base.transform.position);
		}
		else
		{
			base.ControllerCorgi.SetLastStandingPosition(door.transform.position);
		}
		if (!room.gameObject.activeInHierarchy)
		{
			room.gameObject.SetActive(value: true);
		}
		TimeEnteredRoom = Time.time;
		if ((bool)base.ControllerCorgi)
		{
			StartCoroutine(ResetCorgiControllerRayParameters());
		}
	}

	public void EnterRoom(BaseRoom room)
	{
		if (room is Room)
		{
			Room room2 = room as Room;
			EnterRoom(room2, null, room2.PlayerSpawn.transform.localPosition);
		}
		else
		{
			EnterRoom(room, null, Vector3.zero);
		}
	}

	public void EnterRoom(Door door)
	{
		Vector2 vector = new Vector2(0f, 0f);
		if (door.Side == RoomSide.Left || door.Side == RoomSide.Right)
		{
			int num = 1;
			if (door.Side == RoomSide.Right)
			{
				num = -1;
			}
			float y = base.transform.position.y - door.CenterPoint.y;
			vector = new Vector2((float)num * Room_EV.TRANSITION_PLAYER_POSITION_LEFT_RIGHT_X_OFFSET, y);
		}
		else if (door.Side == RoomSide.Bottom)
		{
			float x = base.transform.position.x - door.CenterPoint.x;
			vector = new Vector2(x, Room_EV.TRANSITION_PLAYER_POSITION_BOTTOM_Y_OFFSET);
			if (base.Velocity.y < Room_EV.TRANSITION_MINIMUM_SPEED_ON_ENTER_BOTTOM)
			{
				SetVelocity(base.Velocity.x, Room_EV.TRANSITION_MINIMUM_SPEED_ON_ENTER_BOTTOM, additive: false);
			}
		}
		else if (door.Side == RoomSide.Top)
		{
			float x2 = base.transform.position.x - door.CenterPoint.x;
			vector = new Vector2(x2, Room_EV.TRANSITION_PLAYER_POSITION_TOP_Y_OFFSET);
		}
		Vector2 vector2 = door.CenterPoint + vector;
		Vector3 localPosition = door.Room.gameObject.transform.InverseTransformPoint(vector2);
		EnterRoom(door.Room, door, localPosition);
	}

	public void DisableEffectsOnEnterTunnel()
	{
		StartCoroutine(DisableEffectsOnEnterTunnelCoroutine(0.1f));
	}

	private IEnumerator DisableEffectsOnEnterTunnelCoroutine(float duration)
	{
		if ((bool)m_animator)
		{
			EffectManager.AddAnimatorToDisableList(m_animator);
			m_animator.Play("LandIdle", 0, 1f);
			float delay = Time.time + duration;
			while (Time.time < delay)
			{
				yield return null;
			}
			EffectManager.RemoveAnimatorFromDisableList(m_animator);
		}
	}

	private IEnumerator ResetCorgiControllerRayParameters()
	{
		while (!base.ControllerCorgi.IsInitialized)
		{
			yield return null;
		}
		base.ControllerCorgi.SetRaysParameters();
	}

	public void WeaponOnEnterHitResponse(IHitboxController otherHBController)
	{
		Vector3 collisionPoint = base.Midpoint;
		if (base.HitboxController.LastCollidedWith != null)
		{
			collisionPoint = base.HitboxController.LastCollidedWith.ClosestPoint(base.Midpoint);
		}
		IPlayHitEffect playHitEffect = otherHBController?.RootGameObject.GetComponent<IPlayHitEffect>();
		bool flag = playHitEffect.IsNativeNull();
		if (otherHBController.IsNativeNull() || (!flag && playHitEffect.PlayHitEffect))
		{
			if (!flag)
			{
				EffectManager.PlayHitEffect(this, collisionPoint, playHitEffect.EffectNameOverride, StrikeType);
			}
			else
			{
				EffectManager.PlayHitEffect(this, collisionPoint, null, StrikeType);
			}
		}
		if (!flag && playHitEffect.PlayDirectionalHitEffect)
		{
			EffectManager.PlayDirectionalHitEffect(this, base.HitboxController.RootGameObject, collisionPoint);
		}
	}

	public void PauseGravity(bool stopVelocity, bool lockPlayerControls)
	{
		base.ControllerCorgi.GravityActive(state: false);
		if (stopVelocity)
		{
			SetVelocity(0f, 0f, additive: false);
			CharacterJump.ResetBrakeForce();
		}
		if (lockPlayerControls)
		{
			base.ConditionState = CharacterStates.CharacterConditions.DisableHorizontalMovement;
		}
	}

	public void ResumeGravity()
	{
		base.ControllerCorgi.GravityActive(state: true);
		if (base.ConditionState != CharacterStates.CharacterConditions.Stunned)
		{
			base.ConditionState = CharacterStates.CharacterConditions.Normal;
		}
	}

	public void SetMushroomBig(bool setMushroomBig, bool animate)
	{
		if (setMushroomBig == IsMushroomBig)
		{
			return;
		}
		IsMushroomBig = setMushroomBig;
		if (animate)
		{
			StartCoroutine(MushroomAnimCoroutine(setMushroomBig));
			return;
		}
		Vector3 localScale = new Vector3(1.4f, 1.4f, 1f);
		if (TraitManager.IsTraitActive(TraitType.YouAreSmall))
		{
			localScale.x *= 0.55f;
			localScale.y *= 0.55f;
		}
		base.transform.localScale = localScale;
		Messenger<GameMessenger, GameEvent>.Broadcast(GameEvent.PlayerScaleChanged, null, null);
	}

	private IEnumerator MushroomAnimCoroutine(bool growBig)
	{
		Vector3 smallScale = new Vector3(1.4f, 1.4f, 1f);
		Vector3 largeScale = smallScale;
		if (!TraitManager.IsTraitActive(TraitType.YouAreSmall))
		{
			largeScale.x *= 1.5f;
			largeScale.y *= 1.5f;
		}
		else
		{
			smallScale.x *= 0.55f;
			smallScale.y *= 0.55f;
		}
		float num = smallScale.x + (largeScale.x - smallScale.x) / 2f;
		Vector3 mediumScale = new Vector3(num, num, 1f);
		float interval = 0.1f;
		RewiredMapController.SetCurrentMapEnabled(enabled: false);
		RLTimeScale.SetTimeScale(TimeScaleType.Game, 0f);
		if (growBig)
		{
			yield return MushroomIntervalCoroutine(interval, mediumScale);
			yield return MushroomIntervalCoroutine(interval, smallScale);
			yield return MushroomIntervalCoroutine(interval, mediumScale);
			yield return MushroomIntervalCoroutine(interval, smallScale);
			yield return MushroomIntervalCoroutine(interval, mediumScale);
			yield return MushroomIntervalCoroutine(interval, largeScale);
		}
		else
		{
			yield return MushroomIntervalCoroutine(interval, mediumScale);
			yield return MushroomIntervalCoroutine(interval, largeScale);
			yield return MushroomIntervalCoroutine(interval, mediumScale);
			yield return MushroomIntervalCoroutine(interval, largeScale);
			yield return MushroomIntervalCoroutine(interval, mediumScale);
			yield return MushroomIntervalCoroutine(interval, smallScale);
		}
		Messenger<GameMessenger, GameEvent>.Broadcast(GameEvent.PlayerScaleChanged, null, null);
		RewiredMapController.SetCurrentMapEnabled(enabled: true);
		RLTimeScale.SetTimeScale(TimeScaleType.Game, 1f);
	}

	private IEnumerator MushroomIntervalCoroutine(float interval, Vector3 scale)
	{
		base.transform.localScale = scale;
		float delay = Time.unscaledTime + interval;
		while (Time.unscaledTime < delay)
		{
			yield return null;
		}
	}

	public void StartOnHitAreaDamageTimer()
	{
		if (m_onHitAreaDamageCoroutine != null)
		{
			StopCoroutine(m_onHitAreaDamageCoroutine);
			m_onHitAreaDamageCoroutine = null;
		}
		m_onHitAreaDamageCoroutine = StartCoroutine(OnHitAreaDamageCoroutine());
	}

	public void StopOnHitAreaDamageTimer()
	{
		SaveManager.PlayerSaveData.GetRelic(RelicType.OnHitAreaDamage).SetIntValue(0, additive: false);
		if (m_onHitAreaDamageCoroutine != null)
		{
			StopCoroutine(m_onHitAreaDamageCoroutine);
			m_onHitAreaDamageCoroutine = null;
		}
	}

	private IEnumerator OnHitAreaDamageCoroutine()
	{
		m_onHitAreaDamage_waitYield.CreateNew(1f);
		RelicObj relicObj = SaveManager.PlayerSaveData.GetRelic(RelicType.OnHitAreaDamage);
		int timeRequirement = Relic_EV.GetRelicMaxStack(relicObj.RelicType, relicObj.Level);
		while (relicObj.IntValue < timeRequirement)
		{
			yield return m_onHitAreaDamage_waitYield;
			relicObj.SetIntValue(1, additive: true);
		}
	}

	public void StartNoAttackDamageBonusTimer()
	{
		if (m_noAttackDamageBonusCoroutine != null)
		{
			StopCoroutine(m_noAttackDamageBonusCoroutine);
			m_noAttackDamageBonusCoroutine = null;
		}
		m_noAttackDamageBonusCoroutine = StartCoroutine(NoAttackDamageBonusCoroutine());
	}

	public void StopNoAttackDamageBonusTimer()
	{
		SaveManager.PlayerSaveData.GetRelic(RelicType.NoAttackDamageBonus).SetIntValue(0, additive: false);
		if (m_noAttackDamageBonusCoroutine != null)
		{
			StopCoroutine(m_noAttackDamageBonusCoroutine);
			m_noAttackDamageBonusCoroutine = null;
		}
	}

	private IEnumerator NoAttackDamageBonusCoroutine()
	{
		m_noAttackDamageBonus_waitYield.CreateNew(1f);
		RelicObj relicObj = SaveManager.PlayerSaveData.GetRelic(RelicType.NoAttackDamageBonus);
		int timeRequirement = Relic_EV.GetRelicMaxStack(relicObj.RelicType, relicObj.Level);
		while (relicObj.IntValue < timeRequirement)
		{
			yield return m_noAttackDamageBonus_waitYield;
			relicObj.SetIntValue(1, additive: true);
		}
	}

	public void StartSpinKicksDropCaltropsTimer()
	{
		if (m_spinKicksDropCaltropsCoroutine != null)
		{
			StopCoroutine(m_spinKicksDropCaltropsCoroutine);
			m_spinKicksDropCaltropsCoroutine = null;
		}
		m_spinKicksDropCaltropsCoroutine = StartCoroutine(SpinKicksDropCaltropsCoroutine());
	}

	public void StopSpinKicksDropCaltropsTimer()
	{
		SaveManager.PlayerSaveData.GetRelic(RelicType.SpinKickLeavesCaltrops).SetIntValue(0, additive: false);
		if (m_spinKicksDropCaltropsCoroutine != null)
		{
			StopCoroutine(m_spinKicksDropCaltropsCoroutine);
			m_spinKicksDropCaltropsCoroutine = null;
		}
	}

	private IEnumerator SpinKicksDropCaltropsCoroutine()
	{
		RelicObj relicObj = SaveManager.PlayerSaveData.GetRelic(RelicType.SpinKickLeavesCaltrops);
		int timeRequirement = Relic_EV.GetRelicMaxStack(relicObj.RelicType, relicObj.Level);
		while (relicObj.IntValue < timeRequirement)
		{
			float delay = Time.time + 1f;
			while (Time.time < delay)
			{
				yield return null;
			}
			relicObj.SetIntValue(1, additive: true);
		}
	}

	[SpecialName]
	GameObject IDamageObj.get_gameObject()
	{
		return base.gameObject;
	}
}
