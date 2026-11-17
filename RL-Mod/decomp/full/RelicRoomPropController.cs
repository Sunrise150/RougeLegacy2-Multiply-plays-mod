using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using RL_Windows;
using TMPro;
using UnityEngine;

public class RelicRoomPropController : DualChoicePropController, ILocalizable
{
	[SerializeField]
	private SpriteRenderer m_leftBeam;

	[SerializeField]
	private SpriteRenderer m_rightBeam;

	[SerializeField]
	private SpriteRenderer m_leftIconTwin;

	[SerializeField]
	private SpriteRenderer m_rightIconTwin;

	private Vector2Int m_relicTypes;

	private WaitRL_Yield m_waitYield;

	private int m_totalRoomRolls;

	private Vector2Int m_relicModTypes;

	private Vector3 m_leftIconStartingPos;

	private Vector3 m_rightIconStartingPos;

	private Vector3 m_leftIconStartingScale;

	private Vector3 m_rightIconStartingScale;

	private Coroutine m_leftTwinSpinCoroutine;

	private Coroutine m_rightTwinSpinCoroutine;

	private RelicType m_replacedRelic1;

	private RelicType m_replacedRelic2;

	private Action<MonoBehaviour, EventArgs> m_refreshText;

	private static List<RelicType> m_exclusionList = new List<RelicType>();

	public RelicType LeftRelicType => (RelicType)m_relicTypes.x;

	public RelicType RightRelicType => (RelicType)m_relicTypes.y;

	public SpriteRenderer LeftIconTwin => m_leftIconTwin;

	public SpriteRenderer RightIconTwin => m_rightIconTwin;

	protected override void Awake()
	{
		m_waitYield = new WaitRL_Yield(0f);
		base.Awake();
		m_leftIconStartingPos = base.LeftIcon.transform.localPosition;
		m_rightIconStartingPos = base.RightIcon.transform.localPosition;
		m_leftIconStartingScale = base.LeftIcon.transform.localScale;
		m_rightIconStartingScale = base.RightIcon.transform.localScale;
		m_refreshText = RefreshText;
	}

	protected override void InitializePooledPropOnEnter()
	{
		base.InitializePooledPropOnEnter();
		m_totalRoomRolls = 1;
		if (int.TryParse(GetRoomMiscData("TotalRelicRolls"), NumberStyles.Any, SaveManager.CultureInfo, out var result))
		{
			m_totalRoomRolls = result;
		}
		m_exclusionList.Clear();
		if (ChallengeManager.IsInChallenge)
		{
			m_exclusionList.AddRange(Challenge_EV.RELIC_EXCLUSION_ARRAY);
		}
		RollRelics(m_totalRoomRolls, addToTotalRoomRolls: false);
	}

	public void RollRelics(int numRolls, bool addToTotalRoomRolls, bool rollMods = true)
	{
		if (m_leftTwinSpinCoroutine != null)
		{
			StopCoroutine(m_leftTwinSpinCoroutine);
		}
		if (m_rightTwinSpinCoroutine != null)
		{
			StopCoroutine(m_rightTwinSpinCoroutine);
		}
		if (addToTotalRoomRolls)
		{
			m_totalRoomRolls += numRolls;
			SetRoomMiscData("TotalRelicRolls", m_totalRoomRolls.ToString());
		}
		Prop component = GetComponent<Prop>();
		RelicPropTypeOverride relicPropTypeOverride = null;
		if ((bool)component && (bool)component.PropSpawnController)
		{
			relicPropTypeOverride = component.PropSpawnController.gameObject.GetComponent<RelicPropTypeOverride>();
		}
		for (int i = 0; i < numRolls; i++)
		{
			bool flag = false;
			bool flag2 = false;
			m_relicTypes.x = 0;
			m_relicTypes.y = 0;
			m_relicModTypes.x = 0;
			m_relicModTypes.y = 0;
			m_replacedRelic1 = RelicType.None;
			m_replacedRelic2 = RelicType.None;
			if ((bool)relicPropTypeOverride)
			{
				if (relicPropTypeOverride.Relic1Override != 0)
				{
					m_relicTypes.x = (int)relicPropTypeOverride.Relic1Override;
					flag = true;
				}
				if (relicPropTypeOverride.Relic2Override != 0)
				{
					m_relicTypes.y = (int)relicPropTypeOverride.Relic2Override;
					flag2 = true;
				}
				if ((relicPropTypeOverride.Relic1Override == RelicType.Lily1 || relicPropTypeOverride.Relic1Override == RelicType.Lily2 || relicPropTypeOverride.Relic1Override == RelicType.Lily3) && SaveManager.PlayerSaveData.GetInsightState(InsightType.ForestBoss_DoorOpened) >= InsightState.ResolvedButNotViewed)
				{
					flag = false;
				}
				if ((relicPropTypeOverride.Relic2Override == RelicType.Lily1 || relicPropTypeOverride.Relic2Override == RelicType.Lily2 || relicPropTypeOverride.Relic2Override == RelicType.Lily3) && SaveManager.PlayerSaveData.GetInsightState(InsightType.ForestBoss_DoorOpened) >= InsightState.ResolvedButNotViewed)
				{
					flag2 = false;
				}
				if ((relicPropTypeOverride.Relic1Override == RelicType.DragonKeyWhite && SaveManager.PlayerSaveData.GetFlag(PlayerSaveFlag.CaveMiniboss_WhiteDoor_Opened)) || (relicPropTypeOverride.Relic1Override == RelicType.DragonKeyBlack && SaveManager.PlayerSaveData.GetFlag(PlayerSaveFlag.CaveMiniboss_BlackDoor_Opened)))
				{
					flag = false;
				}
				if ((relicPropTypeOverride.Relic2Override == RelicType.DragonKeyWhite && SaveManager.PlayerSaveData.GetFlag(PlayerSaveFlag.CaveMiniboss_WhiteDoor_Opened)) || (relicPropTypeOverride.Relic2Override == RelicType.DragonKeyBlack && SaveManager.PlayerSaveData.GetFlag(PlayerSaveFlag.CaveMiniboss_BlackDoor_Opened)))
				{
					flag2 = false;
				}
				if (Global_EV.RELIC_ROOM_TEST_RELICS.x != 0)
				{
					m_relicTypes.x = Global_EV.RELIC_ROOM_TEST_RELICS.x;
					flag = true;
				}
				if (Global_EV.RELIC_ROOM_TEST_RELICS.y != 0)
				{
					m_relicTypes.y = Global_EV.RELIC_ROOM_TEST_RELICS.y;
					flag2 = true;
				}
			}
			RngID rngIDToUse = RngID.SpecialProps_RoomSeed;
			if (GameUtility.IsInLevelEditor)
			{
				rngIDToUse = RngID.None;
			}
			if (!flag)
			{
				m_relicTypes.x = (int)RelicLibrary.GetRandomRelic(rngIDToUse, limitMaxStacks: false, m_exclusionList);
			}
			if (!flag2)
			{
				while (m_relicTypes.y == m_relicTypes.x || m_relicTypes.y == 0)
				{
					Debug.Log("Duplicate relic or not relic found.  Attempting to roll again...");
					m_relicTypes.y = (int)RelicLibrary.GetRandomRelic(rngIDToUse, limitMaxStacks: false, m_exclusionList);
				}
			}
			if (!m_exclusionList.Contains((RelicType)m_relicTypes.x))
			{
				m_exclusionList.Add((RelicType)m_relicTypes.x);
			}
			if (!m_exclusionList.Contains((RelicType)m_relicTypes.y) && base.RightIcon.gameObject.activeSelf)
			{
				m_exclusionList.Add((RelicType)m_relicTypes.y);
			}
			if (rollMods)
			{
				if (!flag)
				{
					RollRelicMod(leftSide: true, rngIDToUse);
				}
				if (!flag2)
				{
					RollRelicMod(leftSide: false, rngIDToUse);
				}
			}
			RelicObj relic = SaveManager.PlayerSaveData.GetRelic((RelicType)m_relicTypes.x);
			RelicData relicData = RelicLibrary.GetRelicData((RelicType)m_relicTypes.x);
			if (relic != null && relic.Level >= relicData.MaxStack)
			{
				m_replacedRelic1 = (RelicType)m_relicTypes.x;
				m_relicTypes.x = 600;
				m_relicModTypes.x = 0;
			}
			RelicObj relic2 = SaveManager.PlayerSaveData.GetRelic((RelicType)m_relicTypes.y);
			RelicData relicData2 = RelicLibrary.GetRelicData((RelicType)m_relicTypes.y);
			if (relic2 != null && relic2.Level >= relicData2.MaxStack)
			{
				m_replacedRelic2 = (RelicType)m_relicTypes.y;
				m_relicTypes.y = 600;
				m_relicModTypes.y = 0;
			}
		}
		InitializeTextBox(leftSide: true);
		InitializeTextBox(leftSide: false);
	}

	private void RollRelicMod(bool leftSide, RngID rngIDToUse)
	{
		if (leftSide)
		{
			m_relicModTypes.x = 0;
		}
		else
		{
			m_relicModTypes.y = 0;
		}
		RelicType relicType = (leftSide ? LeftRelicType : RightRelicType);
		RelicData relicData = RelicLibrary.GetRelicData(relicType);
		RelicObj relic = SaveManager.PlayerSaveData.GetRelic(relicType);
		if (relicType == RelicType.None)
		{
			return;
		}
		RelicModType relicModType = RelicModType.None;
		if (TraitManager.IsTraitActive(TraitType.TwinRelics))
		{
			relicModType = RelicModType.DoubleRelic;
		}
		else
		{
			float num = ((rngIDToUse != 0) ? RNGManager.GetRandomNumber(rngIDToUse, "RelicRoomPropController.RollRelicMods()", 0f, 1f) : UnityEngine.Random.Range(0f, 1f));
			if (num <= 0.2f)
			{
				int num2 = ((rngIDToUse != 0) ? RNGManager.GetRandomNumber(rngIDToUse, "RelicRoomPropController.RollRelicMods()", 0, RelicModType_RL.TypeArray.Length) : UnityEngine.Random.Range(1, RelicModType_RL.TypeArray.Length));
				relicModType = RelicModType_RL.TypeArray[num2];
			}
		}
		if (relicModType == RelicModType.DoubleRelic && (bool)relicData && relicData.MaxStack - relic.Level <= 1)
		{
			relicModType = RelicModType.None;
		}
		if (leftSide)
		{
			m_relicModTypes.x = (int)relicModType;
		}
		else
		{
			m_relicModTypes.y = (int)relicModType;
		}
	}

	public void InitializeTextBox(bool leftSide)
	{
		PlayerManager.GetPlayerController();
		RelicType relicType = RelicType.None;
		RelicModType relicModType = RelicModType.None;
		GenericInfoTextBox genericInfoTextBox = null;
		SpriteRenderer spriteRenderer = null;
		SpriteRenderer spriteRenderer2 = null;
		SpriteRenderer spriteRenderer3 = null;
		Vector3 localPosition;
		Vector3 localScale;
		if (leftSide)
		{
			relicType = LeftRelicType;
			genericInfoTextBox = base.LeftInfoTextBox;
			spriteRenderer = base.LeftIcon;
			spriteRenderer2 = m_leftIconTwin;
			relicModType = (RelicModType)m_relicModTypes.x;
			spriteRenderer3 = m_leftBeam;
			localPosition = m_leftIconStartingPos;
			localScale = m_leftIconStartingScale;
		}
		else
		{
			relicType = RightRelicType;
			genericInfoTextBox = base.RightInfoTextBox;
			spriteRenderer = base.RightIcon;
			spriteRenderer2 = m_rightIconTwin;
			relicModType = (RelicModType)m_relicModTypes.y;
			spriteRenderer3 = m_rightBeam;
			localPosition = m_rightIconStartingPos;
			localScale = m_rightIconStartingScale;
		}
		spriteRenderer.transform.localPosition = localPosition;
		spriteRenderer.transform.localScale = localScale;
		spriteRenderer2.gameObject.SetActive(value: false);
		if ((bool)spriteRenderer3)
		{
			if (relicModType != 0)
			{
				spriteRenderer3.color = Color.yellow;
			}
			else
			{
				spriteRenderer3.color = Color.white;
			}
		}
		RelicData relicData = RelicLibrary.GetRelicData(relicType);
		if (!relicData)
		{
			return;
		}
		genericInfoTextBox.HeaderText.text = LocalizationManager.GetString(relicData.Title, SaveManager.PlayerSaveData.CurrentCharacter.IsFemale);
		if (relicModType == RelicModType.DoubleRelic)
		{
			spriteRenderer2.gameObject.SetActive(value: true);
			spriteRenderer2.sprite = IconLibrary.GetRelicSprite(relicType, getLargeIcon: false);
			if (leftSide)
			{
				if (m_leftTwinSpinCoroutine != null)
				{
					StopCoroutine(m_leftTwinSpinCoroutine);
				}
				m_leftTwinSpinCoroutine = StartCoroutine(TwinRelicAnimCoroutine(leftSide));
			}
			else
			{
				if (m_rightTwinSpinCoroutine != null)
				{
					StopCoroutine(m_rightTwinSpinCoroutine);
				}
				m_rightTwinSpinCoroutine = StartCoroutine(TwinRelicAnimCoroutine(leftSide));
			}
			genericInfoTextBox.HeaderText.text = string.Format(LocalizationManager.GetString("LOC_ID_RELIC_TITLE_APPEND_DoubleRelic_1", SaveManager.PlayerSaveData.CurrentCharacter.IsFemale), genericInfoTextBox.HeaderText.text);
		}
		if (relicType == RelicType.GoldDeathCurse)
		{
			int num = (int)((float)SaveManager.PlayerSaveData.GoldCollected * 1.35f);
			genericInfoTextBox.SubHeaderText.text = string.Format(LocalizationManager.GetString("LOC_ID_RELIC_COST_DEATH_1", SaveManager.PlayerSaveData.CurrentCharacter.IsFemale), num);
		}
		else
		{
			float costAmount = relicData.CostAmount;
			float relicCostMod = SkillTreeLogicHelper.GetRelicCostMod();
			int num2 = Mathf.RoundToInt(Mathf.Clamp(costAmount - relicCostMod, 0f, float.MaxValue) * 100f);
			if (relicModType == RelicModType.DoubleRelic)
			{
				num2 *= 2;
			}
			genericInfoTextBox.SubHeaderText.text = string.Format(LocalizationManager.GetString("LOC_ID_RELIC_UI_RESOLVE_COST_1", isFemale: false), num2);
			bool isFormatString;
			string text = LocalizationManager.GetString(relicData.Description, SaveManager.PlayerSaveData.CurrentCharacter.IsFemale, out isFormatString);
			if (isFormatString)
			{
				int num3 = SaveManager.PlayerSaveData.GetRelic(relicType).Level;
				if (relicModType == RelicModType.DoubleRelic)
				{
					num3++;
				}
				float altValue;
				float relicFormatString = Relic_EV.GetRelicFormatString(relicType, num3 + 1, out altValue);
				text = string.Format(text, relicFormatString.ToCIString(), altValue.ToCIString());
			}
			if (!SaveManager.PlayerSaveData.GetRelic(relicType).WasSeen && !ChallengeManager.IsInChallenge)
			{
				text = LocalizationManager.GetString("LOC_ID_LINEAGE_HIDDEN_RELIC_1", isFemale: false);
			}
			TMP_Text subHeaderText = genericInfoTextBox.SubHeaderText;
			subHeaderText.text = subHeaderText.text + "\n" + text;
		}
		genericInfoTextBox.DescriptionText.text = LocalizationManager.GetString(relicData.Description02, SaveManager.PlayerSaveData.CurrentCharacter.IsFemale);
		spriteRenderer.sprite = IconLibrary.GetRelicSprite(relicType, getLargeIcon: false);
		spriteRenderer.color = Color.white;
	}

	private IEnumerator TwinRelicAnimCoroutine(bool leftSide)
	{
		GameObject icon;
		GameObject iconTwin;
		Vector3 startingPos;
		float startingScale;
		if (leftSide)
		{
			icon = base.LeftIcon.gameObject;
			iconTwin = m_leftIconTwin.gameObject;
			startingPos = m_leftIconStartingPos;
			startingScale = m_leftIconStartingScale.x;
		}
		else
		{
			icon = base.RightIcon.gameObject;
			iconTwin = m_rightIconTwin.gameObject;
			startingPos = m_rightIconStartingPos;
			startingScale = m_rightIconStartingScale.x;
		}
		while (true)
		{
			float num = Mathf.Sin(Time.timeSinceLevelLoad * 1.5f) / 1.5f;
			float xPos = startingPos.x + num;
			icon.transform.SetLocalPositionX(xPos);
			float xPos2 = startingPos.x - num;
			iconTwin.transform.SetLocalPositionX(xPos2);
			float zPos;
			float num2 = (zPos = Mathf.Cos(Time.timeSinceLevelLoad * 1.5f) / 1.5f);
			icon.transform.SetLocalPositionZ(zPos);
			float zPos2 = 0f - num2;
			iconTwin.transform.SetLocalPositionZ(zPos2);
			float num3 = num2 * 0.5f;
			float num4 = startingScale - num3;
			icon.transform.localScale = new Vector3(num4, num4, num4);
			float num5 = startingScale + num3;
			iconTwin.transform.localScale = new Vector3(num5, num5, num5);
			yield return null;
		}
	}

	public static int CalculateHealthCost(float cost, PlayerController playerController, bool applySkillReduc)
	{
		if (applySkillReduc)
		{
			cost = Mathf.Clamp(cost - SkillTreeLogicHelper.GetRelicCostMod(), 0f, 1f);
		}
		return Mathf.RoundToInt((float)playerController.ClassModdedMaxHealth * (1f + playerController.TraitMaxHealthMod) * cost);
	}

	private string GetHealthCostString(RelicData relicData)
	{
		PlayerManager.GetPlayerController();
		RelicCostType costType = relicData.CostType;
		if (costType == RelicCostType.MaxHealthDamage || costType != RelicCostType.MaxHealthPermanent)
		{
			return LocalizationManager.GetString("LOC_ID_RELIC_COST_DAMAGE_HEALTH_1", SaveManager.PlayerSaveData.CurrentCharacter.IsFemale);
		}
		return LocalizationManager.GetString("LOC_ID_RELIC_COST_MAX_HEALTH_1", SaveManager.PlayerSaveData.CurrentCharacter.IsFemale);
	}

	public void ChooseLeftRelic()
	{
		StartCoroutine(ChooseRelic(LeftRelicType, (RelicModType)m_relicModTypes.x));
	}

	public void ChooseRightRelic()
	{
		StartCoroutine(ChooseRelic(RightRelicType, (RelicModType)m_relicModTypes.y));
	}

	private IEnumerator ChooseRelic(RelicType relicType, RelicModType relicModType)
	{
		RelicData relicData = RelicLibrary.GetRelicData(relicType);
		PlayerController playerController = PlayerManager.GetPlayerController();
		playerController.CharacterHitResponse.StopInvincibleTime();
		int previousMaxHP = playerController.ActualMaxHealth;
		RewiredMapController.SetCurrentMapEnabled(enabled: false);
		playerController.Animator.SetBool("Victory", value: true);
		m_waitYield.CreateNew(0.5f);
		yield return m_waitYield;
		RelicDrop relicDrop = new RelicDrop(relicType, relicModType);
		if (!WindowManager.GetIsWindowLoaded(WindowID.SpecialItemDrop))
		{
			WindowManager.LoadWindow(WindowID.SpecialItemDrop);
		}
		(WindowManager.GetWindowController(WindowID.SpecialItemDrop) as SpecialItemDropWindowController).AddSpecialItemDrop(relicDrop);
		WindowManager.SetWindowIsOpen(WindowID.SpecialItemDrop, isOpen: true);
		while (WindowManager.GetIsWindowOpen(WindowID.SpecialItemDrop))
		{
			yield return null;
		}
		RewiredMapController.SetCurrentMapEnabled(enabled: false);
		playerController.Animator.SetBool("Victory", value: false);
		m_waitYield.CreateNew(0.25f);
		yield return m_waitYield;
		if (relicType == RelicType.GoldDeathCurse)
		{
			SaveManager.PlayerSaveData.GoldCollected = (int)((float)SaveManager.PlayerSaveData.GoldCollected * 1.35f);
			Messenger<GameMessenger, GameEvent>.Broadcast(GameEvent.GoldChanged, null, null);
			RewiredMapController.SetCurrentMapEnabled(enabled: true);
			playerController.DisableArmor = true;
			playerController.CharacterHitResponse.StartHitResponse(playerController.gameObject, playerController, 2.1474836E+09f, trueDamage: true);
			playerController.DisableArmor = false;
		}
		else
		{
			playerController.InitializeHealthMods();
			playerController.InitializeManaMods();
		}
		if (relicType != RelicType.GoldDeathCurse)
		{
			ItemDropManager.DropSpecialItem(relicDrop, displayItemDropWindow: false);
			RewiredMapController.SetCurrentMapEnabled(enabled: true);
			float costAmount = relicData.CostAmount;
			float relicCostMod = SkillTreeLogicHelper.GetRelicCostMod();
			int num = Mathf.RoundToInt(Mathf.Clamp(costAmount - relicCostMod, 0f, float.MaxValue) * 100f);
			if (relicModType == RelicModType.DoubleRelic)
			{
				num *= 2;
			}
			int num2 = previousMaxHP - playerController.ActualMaxHealth;
			if (num > 0)
			{
				string text = string.Format(LocalizationManager.GetString("LOC_ID_RELIC_UI_RESOLVE_LOST_1", isFemale: false), -num);
				if (num2 > 0)
				{
					text = text + "\n" + string.Format(LocalizationManager.GetString("LOC_ID_RELIC_UI_HP_LOST_1", isFemale: false), -num2);
				}
				TextPopupManager.DisplayTextDefaultPos(TextPopupType.OutOfAmmo, text, playerController, attachToSource: true);
			}
			DisableProp(firstTimeDisabled: true);
		}
		else
		{
			DisableProp(firstTimeDisabled: false);
		}
		PropComplete();
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
		m_leftIconTwin.gameObject.SetActive(value: false);
		m_rightIconTwin.gameObject.SetActive(value: false);
	}

	private void OnEnable()
	{
		Messenger<UIMessenger, UIEvent>.AddListener(UIEvent.LanguageChanged, m_refreshText);
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		Messenger<UIMessenger, UIEvent>.RemoveListener(UIEvent.LanguageChanged, m_refreshText);
	}

	public void RefreshText(object sender, EventArgs args)
	{
		InitializeTextBox(leftSide: true);
		InitializeTextBox(leftSide: false);
	}
}
