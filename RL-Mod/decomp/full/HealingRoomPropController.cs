using System;
using MoreMountains.CorgiEngine;
using UnityEngine;

public class HealingRoomPropController : DualChoicePropController, ILocalizable
{
	private MaxHealthChangeEventArgs m_healthChangeArgs;

	private Action<MonoBehaviour, EventArgs> m_updateTooltips;

	private Action<MonoBehaviour, EventArgs> m_refreshText;

	protected override void Awake()
	{
		m_healthChangeArgs = new MaxHealthChangeEventArgs(0f, 0f);
		m_updateTooltips = UpdateTooltips;
		m_refreshText = RefreshText;
		base.Awake();
	}

	private void OnEnable()
	{
		Messenger<GameMessenger, GameEvent>.AddListener(GameEvent.PlayerManaChange, m_updateTooltips);
		Messenger<GameMessenger, GameEvent>.AddListener(GameEvent.PlayerMaxHealthChange, m_updateTooltips);
		Messenger<GameMessenger, GameEvent>.AddListener(GameEvent.PlayerHealthChange, m_updateTooltips);
		Messenger<GameMessenger, GameEvent>.AddListener(GameEvent.RelicStatsChanged, m_updateTooltips);
		Messenger<UIMessenger, UIEvent>.AddListener(UIEvent.LanguageChanged, m_refreshText);
	}

	protected override void OnDisable()
	{
		Messenger<UIMessenger, UIEvent>.RemoveListener(UIEvent.LanguageChanged, m_refreshText);
		base.OnDisable();
		RemovePlayerEventListeners();
	}

	private void RemovePlayerEventListeners()
	{
		Messenger<GameMessenger, GameEvent>.RemoveListener(GameEvent.PlayerManaChange, m_updateTooltips);
		Messenger<GameMessenger, GameEvent>.RemoveListener(GameEvent.PlayerMaxHealthChange, m_updateTooltips);
		Messenger<GameMessenger, GameEvent>.RemoveListener(GameEvent.PlayerHealthChange, m_updateTooltips);
		Messenger<GameMessenger, GameEvent>.RemoveListener(GameEvent.RelicStatsChanged, m_updateTooltips);
	}

	private void CalculateHealthAndManaRestored(PlayerController playerController, out float hpGain, out float mpGain)
	{
		float num = playerController.ActualMaxHealth;
		float actualMagic = playerController.ActualMagic;
		hpGain = Mathf.CeilToInt(num * 0f + actualMagic * 5f);
		if (TraitManager.IsTraitActive(TraitType.MegaHealth))
		{
			hpGain = 0f;
		}
		hpGain = Mathf.Clamp(hpGain, 0f, num);
		float num2 = playerController.ActualMaxMana;
		mpGain = Mathf.CeilToInt(num2 * 1f);
		mpGain = Mathf.Clamp(mpGain, 0f, num2);
	}

	private void CalculateMaxHealthGain(PlayerController playerController, out float damageDealt, out float maxHealthGained)
	{
		damageDealt = Mathf.Clamp(Mathf.Abs(Mathf.CeilToInt((float)playerController.ActualMaxHealth * -0.3f)), 1, int.MaxValue);
		playerController.DisableArmor = true;
		CharacterStates.MovementStates previousMovementState = playerController.PreviousMovementState;
		CharacterStates.MovementStates movementState = playerController.MovementState;
		playerController.MovementState = CharacterStates.MovementStates.Idle;
		damageDealt = playerController.CalculateDamageTaken(playerController, out var _, out var _, damageDealt);
		playerController.MovementState = previousMovementState;
		playerController.MovementState = movementState;
		playerController.DisableArmor = false;
		playerController.RelicMaxHealthMod += 0.15f;
		int actualMaxHealth = playerController.ActualMaxHealth;
		playerController.RelicMaxHealthMod -= 0.15f;
		maxHealthGained = actualMaxHealth - playerController.ActualMaxHealth;
	}

	private void UpdateTooltips(MonoBehaviour sender, EventArgs args)
	{
		PlayerController playerController = PlayerManager.GetPlayerController();
		CalculateHealthAndManaRestored(playerController, out var hpGain, out var mpGain);
		base.LeftInfoTextBox.SubHeaderText.text = string.Format(LocalizationManager.GetString("LOC_ID_HEALING_ROOM_SUBHEADER_RESTORE_HEALTH_1", isFemale: false), (int)hpGain, (int)mpGain);
		CalculateMaxHealthGain(playerController, out var damageDealt, out var maxHealthGained);
		base.RightInfoTextBox.SubHeaderText.text = string.Format(LocalizationManager.GetString("LOC_ID_HEALING_ROOM_SUBHEADER_MAX_HEALTH_1", isFemale: false), (int)damageDealt, (int)maxHealthGained);
	}

	protected override void InitializePooledPropOnEnter()
	{
		base.InitializePooledPropOnEnter();
		base.LeftIcon.sprite = IconLibrary.GetMiscIcon(MiscIconType.HealingRoom_RestoreHPMP);
		base.LeftInfoTextBox.HeaderText.text = LocalizationManager.GetString("LOC_ID_HEALING_ROOM_TITLE_RESTORE_HEALTH_1", isFemale: false);
		base.LeftInfoTextBox.DescriptionText.text = LocalizationManager.GetString("LOC_ID_HEALING_ROOM_DESCRIPTION_RESTORE_HEALTH_1", isFemale: false);
		base.RightIcon.sprite = IconLibrary.GetMiscIcon(MiscIconType.HealingRoom_IncreaseMaxHP);
		base.RightInfoTextBox.HeaderText.text = LocalizationManager.GetString("LOC_ID_HEALING_ROOM_TITLE_MAX_HEALTH_1", isFemale: false);
		base.RightInfoTextBox.DescriptionText.text = LocalizationManager.GetString("LOC_ID_HEALING_ROOM_DESCRIPTION_MAX_HEALTH_1", isFemale: false);
		UpdateTooltips(null, null);
	}

	public void RestoreHPMP()
	{
		RemovePlayerEventListeners();
		PlayerController playerController = PlayerManager.GetPlayerController();
		CalculateHealthAndManaRestored(playerController, out var hpGain, out var mpGain);
		playerController.SetHealth(hpGain, additive: true, runEvents: true);
		playerController.SetMana(mpGain, additive: true, runEvents: true);
		Vector2 absPos = playerController.Midpoint;
		absPos.y += playerController.CollisionBounds.height / 2f;
		string text = string.Format(LocalizationManager.GetString("LOC_ID_STATUS_EFFECT_HEALTH_RESTORE_1", SaveManager.PlayerSaveData.CurrentCharacter.IsFemale), (int)hpGain);
		string text2 = string.Format(LocalizationManager.GetString("LOC_ID_STATUS_EFFECT_MANA_RESTORE_1", SaveManager.PlayerSaveData.CurrentCharacter.IsFemale), (int)mpGain);
		TextPopupManager.DisplayTextAtAbsPos(TextPopupType.HPGained, text + "\n<color=#47FFFB>" + text2 + "</color>", absPos);
		PropComplete();
		DisableProp(firstTimeDisabled: true);
	}

	public void IncreaseMaxHealth()
	{
		RemovePlayerEventListeners();
		PlayerController playerController = PlayerManager.GetPlayerController();
		playerController.CharacterHitResponse.StopInvincibleTime();
		float damageOverride = Mathf.Clamp(Mathf.Abs(Mathf.CeilToInt((float)playerController.ActualMaxHealth * -0.3f)), 1, int.MaxValue);
		playerController.DisableArmor = true;
		playerController.CharacterHitResponse.StartHitResponse(playerController.gameObject, playerController, damageOverride);
		playerController.DisableArmor = false;
		if (!playerController.IsDead)
		{
			int actualMaxHealth = playerController.ActualMaxHealth;
			SaveManager.PlayerSaveData.TemporaryMaxHealthMods += 0.15f;
			playerController.InitializeHealthMods();
			int actualMaxHealth2 = playerController.ActualMaxHealth;
			m_healthChangeArgs.Initialise(actualMaxHealth2, actualMaxHealth);
			Messenger<GameMessenger, GameEvent>.Broadcast(GameEvent.PlayerMaxHealthChange, this, m_healthChangeArgs);
			PropComplete();
			DisableProp(firstTimeDisabled: true);
		}
		else
		{
			PropComplete();
			DisableProp(firstTimeDisabled: false);
		}
	}

	protected override void DisableProp(bool firstTimeDisabled)
	{
		if (firstTimeDisabled)
		{
			base.Animator.SetBool("Used", value: true);
		}
		else
		{
			base.Animator.SetBool("InstantlyUsed", value: true);
		}
		base.DisableProp(firstTimeDisabled);
	}

	public void RefreshText(object sender, EventArgs args)
	{
		base.LeftInfoTextBox.HeaderText.text = LocalizationManager.GetString("LOC_ID_HEALING_ROOM_TITLE_RESTORE_HEALTH_1", isFemale: false);
		base.LeftInfoTextBox.DescriptionText.text = LocalizationManager.GetString("LOC_ID_HEALING_ROOM_DESCRIPTION_RESTORE_HEALTH_1", isFemale: false);
		base.RightInfoTextBox.HeaderText.text = LocalizationManager.GetString("LOC_ID_HEALING_ROOM_TITLE_MAX_HEALTH_1", isFemale: false);
		base.RightInfoTextBox.DescriptionText.text = LocalizationManager.GetString("LOC_ID_HEALING_ROOM_DESCRIPTION_MAX_HEALTH_1", isFemale: false);
		UpdateTooltips(null, null);
	}
}
