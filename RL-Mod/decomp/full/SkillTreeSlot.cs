using System;
using System.Collections;
using System.Collections.Generic;
using RLAudio;
using RL_Windows;
using Rewired;
using Sigtrap.Relays;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.UI;

public class SkillTreeSlot : MonoBehaviour, IPointerEnterHandler, IEventSystemHandler, ISelectHandler, IDeselectHandler, IPointerClickHandler
{
	public enum NodeDirection
	{
		Up,
		Down,
		Left,
		Right
	}

	public enum SkillTreeAnimType
	{
		None = 0,
		CastleWingHorizontal_Foundation = 100,
		CastleWingHorizontal_Tower = 110,
		CastleWingHorizontal_Wooden = 120,
		CastleWingVertical_Foundation = 200,
		CastleWingVertical_Tower = 210,
		CastleWingVertical_Wooden = 220,
		Cave = 300,
		Forest_Foundation = 400,
		Forest_Grow = 410,
		FlagPole = 450,
		WeatherVane = 460,
		BalconyOrSign = 470,
		TireSwing = 480,
		CameraShakeOnly = 500
	}

	private const string CAVE_APPEAR_SFX = "event:/SFX/UpgradingCastle/sfx_upgrade_castle_digDeep";

	private const string FOUNDATION_APPEAR_SFX = "event:/SFX/UpgradingCastle/sfx_upgrade_castle_buildFoundation";

	private const string TOWER_APPEAR_SFX = "event:/SFX/UpgradingCastle/sfx_upgrade_castle_buildTower";

	private const string WOODEN_APPEAR_SFX = "event:/SFX/UpgradingCastle/sfx_upgrade_castle_wooden_structure";

	private const string FOREST_APPEAR_SFX = "event:/SFX/UpgradingCastle/sfx_upgrade_castle_tree_grow";

	private const string FOREST_GROW_SFX = "event:/SFX/UpgradingCastle/sfx_upgrade_castle_garden_expand";

	private const string FLAGPOLE_SFX = "event:/SFX/UpgradingCastle/sfx_upgrade_castle_flagpole";

	private const string WEATHERVANE_SFX = "event:/SFX/UpgradingCastle/sfx_upgrade_castle_flagpole";

	private const string BALCONY_SFX = "event:/SFX/UpgradingCastle/sfx_upgrade_castle_balcony_small";

	private const string TIRESWING_SFX = "event:/SFX/UpgradingCastle/sfx_upgrade_castle_flagpole";

	private const string CONSTRUCTION_END_SFX = "event:/SFX/UpgradingCastle/sfx_upgrade_castle_constructionEnd";

	private const string VERY_SMALL_CAMERASHAKE_FX = "CameraShakeVerySmall_Effect";

	private const float CAVE_CAMERASHAKE_DURATION = 0.6f;

	private const float TOWER_CAMERASHAKE_DURATION = 1f;

	private const string CAVE_APPEAR_FX = "SkillTreeCastle_CaveAppear_Effect";

	private const string HORIZONTAL_TOWER_APPEAR_FX = "SkillTreeCastle_HorizontalDust_Effect";

	private const string VERTICAL_TOWER_APPEAR_FX = "SkillTreeCastle_VerticalDust_Effect";

	[SerializeField]
	private SkillTreeType m_skillTreeType;

	[Header("Animations")]
	[SerializeField]
	private string m_bgAnimatorParam;

	[SerializeField]
	private SkillTreeAnimType m_animType;

	[SerializeField]
	private Vector2 m_animEffectOffset;

	[Header("Icons")]
	[SerializeField]
	private CanvasGroup m_iconCanvasGroup;

	[SerializeField]
	private Image m_iconImage;

	[SerializeField]
	private Image m_iconFrame;

	[SerializeField]
	private GameObject m_highlightGO;

	[SerializeField]
	private Image m_canBeUpgradedImage;

	[SerializeField]
	private TMP_Text m_levelText;

	[Header("Navigation Nodes")]
	[SerializeField]
	private SkillTreeSlot m_leftNode;

	[SerializeField]
	private SkillTreeSlot m_rightNode;

	[SerializeField]
	private SkillTreeSlot m_topNode;

	[SerializeField]
	private SkillTreeSlot m_bottomNode;

	[SerializeField]
	private Image m_newIndicator;

	[SerializeField]
	private GameObject m_levelLockGO;

	[SerializeField]
	private TMP_Text m_levelLockText;

	[SerializeField]
	private bool m_disableTopNavNode;

	[SerializeField]
	private bool m_disableBottomNavNode;

	[SerializeField]
	private bool m_disableLeftNavNode;

	[SerializeField]
	private bool m_disableRightNavNode;

	[Header("Unlockable Nodes")]
	[SerializeField]
	[ReadOnly]
	private List<SkillTreeSlot> m_unlockSlotList;

	[SerializeField]
	[ReadOnly]
	private List<bool> m_unlockSlotDisableList;

	[Header("Debug")]
	[SerializeField]
	private Sprite m_hasNoLinkedDataSprite;

	[SerializeField]
	private Sprite m_goldFrameSprite;

	private Button m_button;

	private static HighlightedSkillChangedEventArgs m_highlightedSkillChangedArgs = new HighlightedSkillChangedEventArgs(SkillTreeType.None);

	private static GoldChangedEventArgs m_goldChangedArgs = new GoldChangedEventArgs(0, 0);

	private SkillTreeData m_skillTreeData;

	private Animator m_castleAnimator;

	private Vector3 m_storedScale;

	private bool m_alreadyUnlockedEventPlayed;

	private bool m_selected;

	public Relay UpgradedRelay = new Relay();

	public Relay UpgradeFailedRelay = new Relay();

	public Relay FullyUpgradedRelay = new Relay();

	public Relay SelectedRelay = new Relay();

	public UnityEvent OnAlreadyUnlockedEvent;

	public bool HasAnimated { get; private set; }

	public Vector3 StoredScale => m_storedScale;

	public bool HasAnimParam => !string.IsNullOrEmpty(m_bgAnimatorParam);

	public int SlotIndex { get; private set; }

	public SkillTreeType SkillTreeType
	{
		get
		{
			return m_skillTreeType;
		}
		set
		{
			bool num = m_skillTreeType != value;
			m_skillTreeType = value;
			if (num)
			{
				UpdateSkillTreeType();
			}
		}
	}

	public string BGAnimatorParam
	{
		get
		{
			return m_bgAnimatorParam;
		}
		set
		{
			m_bgAnimatorParam = value;
		}
	}

	public SkillTreeAnimType AnimType
	{
		get
		{
			return m_animType;
		}
		set
		{
			m_animType = value;
		}
	}

	public Vector2 AnimEffectOffset
	{
		get
		{
			return m_animEffectOffset;
		}
		set
		{
			m_animEffectOffset = value;
		}
	}

	public bool HasData => m_skillTreeType != SkillTreeType.None;

	public bool DisableTopNavigationNode
	{
		get
		{
			return m_disableTopNavNode;
		}
		set
		{
			m_disableTopNavNode = value;
		}
	}

	public bool DisableBottomNavigationNode
	{
		get
		{
			return m_disableBottomNavNode;
		}
		set
		{
			m_disableBottomNavNode = value;
		}
	}

	public bool DisableLeftNavigationNode
	{
		get
		{
			return m_disableLeftNavNode;
		}
		set
		{
			m_disableLeftNavNode = value;
		}
	}

	public bool DisableRightNavigationNode
	{
		get
		{
			return m_disableRightNavNode;
		}
		set
		{
			m_disableRightNavNode = value;
		}
	}

	public SkillTreeSlot TopNode
	{
		get
		{
			return m_topNode;
		}
		set
		{
			if (value == this)
			{
				throw new Exception("Cannot link navigation node to self.");
			}
			m_topNode = value;
		}
	}

	public SkillTreeSlot BottomNode
	{
		get
		{
			return m_bottomNode;
		}
		set
		{
			if (value == this)
			{
				throw new Exception("Cannot link navigation node to self.");
			}
			m_bottomNode = value;
		}
	}

	public SkillTreeSlot LeftNode
	{
		get
		{
			return m_leftNode;
		}
		set
		{
			if (value == this)
			{
				throw new Exception("Cannot link navigation node to self.");
			}
			m_leftNode = value;
		}
	}

	public SkillTreeSlot RightNode
	{
		get
		{
			return m_rightNode;
		}
		set
		{
			if (value == this)
			{
				throw new Exception("Cannot link navigation node to self.");
			}
			m_rightNode = value;
		}
	}

	public Button Button
	{
		get
		{
			if (m_button == null)
			{
				m_button = GetComponent<Button>();
			}
			return m_button;
		}
	}

	public List<SkillTreeSlot> UnlockSlotList
	{
		get
		{
			return m_unlockSlotList;
		}
		set
		{
			m_unlockSlotList = value;
		}
	}

	public List<bool> UnlockSlotDisableList
	{
		get
		{
			return m_unlockSlotDisableList;
		}
		set
		{
			m_unlockSlotDisableList = value;
		}
	}

	public bool IsInitialized { get; private set; }

	public void Initialize(int slotIndex, Animator castleAnimator)
	{
		m_storedScale = base.transform.localScale;
		SlotIndex = slotIndex;
		m_button = GetComponent<Button>();
		m_highlightGO.SetActive(value: false);
		m_canBeUpgradedImage.gameObject.SetActive(value: false);
		if (m_skillTreeType != 0)
		{
			m_skillTreeData = SkillTreeLibrary.GetSkillTreeData(m_skillTreeType);
			m_castleAnimator = castleAnimator;
		}
		Navigation navigation = m_button.navigation;
		navigation.mode = Navigation.Mode.Explicit;
		m_button.navigation = navigation;
		RefreshSlotState(updateAnimParams: false);
		IsInitialized = true;
	}

	public void UpdateSkillTreeType()
	{
		SkillTreeObj skillTreeObj = SkillTreeManager.GetSkillTreeObj(SkillTreeType);
		if (Application.isPlaying)
		{
			if (skillTreeObj.IsLocked)
			{
				m_iconImage.sprite = IconLibrary.GetSkillTreeLockedIcon();
			}
			else if (skillTreeObj.IsSoulLocked)
			{
				m_iconImage.sprite = IconLibrary.GetSkillTreeSoulLockedIcon();
			}
			else
			{
				m_iconImage.sprite = IconLibrary.GetSkillTreeIcon(SkillTreeType);
			}
		}
		else
		{
			Sprite skillTreeIcon = IconLibrary.GetSkillTreeIcon(SkillTreeType);
			if (skillTreeIcon != IconLibrary.GetDefaultSprite())
			{
				m_iconImage.sprite = skillTreeIcon;
			}
		}
		base.name = "SkillTreeSlot_" + SkillTreeType;
	}

	public void PurchaseSkillUpgrade(int numLevels)
	{
		SkillTreeObj skillTreeObj = SkillTreeManager.GetSkillTreeObj(SkillTreeType);
		if (skillTreeObj != null && !skillTreeObj.IsLocked && !skillTreeObj.IsLevelLocked && !skillTreeObj.IsSoulLocked)
		{
			int levelsToAdd = Mathf.Clamp(numLevels, 0, skillTreeObj.MaxLevel - skillTreeObj.Level);
			int numLevelsPurchaseableWithGold = skillTreeObj.GetNumLevelsPurchaseableWithGold(levelsToAdd, SaveManager.PlayerSaveData.GoldCollectedIncludingBank);
			if (numLevelsPurchaseableWithGold > 0)
			{
				numLevelsPurchaseableWithGold = Mathf.Max(1, numLevelsPurchaseableWithGold);
				for (int i = 0; i < numLevelsPurchaseableWithGold; i++)
				{
					if (skillTreeObj.Level < skillTreeObj.MaxLevel)
					{
						int goldCostWithLevelAppreciation = skillTreeObj.GoldCostWithLevelAppreciation;
						int goldCollectedIncludingBank = SaveManager.PlayerSaveData.GoldCollectedIncludingBank;
						SaveManager.PlayerSaveData.SubtractFromGoldIncludingBank(goldCostWithLevelAppreciation);
						SaveManager.PlayerSaveData.GoldSpent += goldCostWithLevelAppreciation;
						SaveManager.PlayerSaveData.GoldSpentOnSkills += goldCostWithLevelAppreciation;
						m_goldChangedArgs.Initialize(goldCollectedIncludingBank, goldCollectedIncludingBank + goldCostWithLevelAppreciation);
						Messenger<GameMessenger, GameEvent>.Broadcast(GameEvent.GoldChanged, this, m_goldChangedArgs);
						bool flag = i >= numLevelsPurchaseableWithGold - 1;
						if (!SkillTreeManager.SetSkillObjLevel(SkillTreeType, 1, additive: true, flag))
						{
							throw new Exception("Failed to upgrade skill: " + SkillTreeType.ToString() + " in skill tree. This should never happen.");
						}
						Debug.Log("Successfully leveled up " + skillTreeObj.SkillTreeType);
						if (flag)
						{
							if (skillTreeObj.Level < skillTreeObj.MaxLevel)
							{
								UpgradedRelay.Dispatch();
							}
							else
							{
								FullyUpgradedRelay.Dispatch();
							}
						}
						if ((WindowManager.GetWindowController(WindowID.SkillTree) as SkillTreeWindowController).HasAllSkills())
						{
							StoreAPIManager.GiveAchievement(AchievementType.AllSkills, StoreType.All);
						}
					}
					else
					{
						Debug.Log("Cannot level up " + skillTreeObj.SkillTreeType.ToString() + ".  Reason: Skill already max level.");
						StartCoroutine(ShakeAnimCoroutine());
						UpgradeFailedRelay.Dispatch();
					}
				}
			}
			else
			{
				StartCoroutine(ShakeAnimCoroutine());
				UpgradeFailedRelay.Dispatch();
			}
		}
		else
		{
			StartCoroutine(ShakeAnimCoroutine());
			UpgradeFailedRelay.Dispatch();
		}
	}

	private IEnumerator ShakeAnimCoroutine()
	{
		base.transform.localScale = m_storedScale + new Vector3(0.1f, 0.1f, 0.1f);
		TweenManager.TweenTo_UnscaledTime(base.transform, 0.25f, Ease.Back.EaseOut, "localScale.x", m_storedScale.x, "localScale.y", m_storedScale.y, "localScale.z", m_storedScale.z);
		int shakeCount = 0;
		float z = 5f;
		float shakeDelay = 0.05f;
		float shakeTime = Time.time + shakeDelay;
		Vector3 shakeEuler = base.transform.localEulerAngles;
		shakeEuler.z = z;
		base.transform.localEulerAngles = shakeEuler;
		while (shakeCount < 3)
		{
			if (Time.time >= shakeTime)
			{
				shakeEuler.z *= -1f;
				base.transform.localEulerAngles = shakeEuler;
				shakeTime = Time.time + shakeDelay;
				shakeCount++;
			}
			yield return null;
		}
		shakeEuler.z = 0f;
		base.transform.localEulerAngles = shakeEuler;
	}

	public void OnPointerClick(PointerEventData eventData)
	{
		if (eventData.button == PointerEventData.InputButton.Left && !SkillTreeWindowController.CastleViewEnabled)
		{
			PurchaseSkillUpgrade(1);
		}
	}

	public void OnPointerEnter(PointerEventData eventData)
	{
		if (RewiredOnStartupController.CurrentActiveControllerType == ControllerType.Mouse && !SkillTreeWindowController.CastleViewEnabled)
		{
			OnSelect(eventData);
		}
	}

	public void OnSelect(BaseEventData eventData)
	{
		if (!SkillTreeWindowController.CastleViewEnabled && (bool)m_button && m_button.interactable)
		{
			Select();
		}
	}

	public void Select(bool dispatchSelectedEvent = true)
	{
		if (m_selected)
		{
			return;
		}
		if (m_button.interactable)
		{
			m_selected = true;
			m_button.Select();
			m_highlightGO.SetActive(value: true);
			base.transform.localScale = m_storedScale + new Vector3(0.1f, 0.1f, 0.1f);
			TweenManager.TweenTo_UnscaledTime(base.transform, 0.25f, Ease.Back.EaseOut, "localScale.x", m_storedScale.x, "localScale.y", m_storedScale.y, "localScale.z", m_storedScale.z);
			m_highlightedSkillChangedArgs.Initialize(SkillTreeType);
			Messenger<UIMessenger, UIEvent>.Broadcast(UIEvent.SkillTree_HighlightedSkillChanged, this, m_highlightedSkillChangedArgs);
			if (dispatchSelectedEvent)
			{
				SelectedRelay.Dispatch();
			}
		}
		else
		{
			Debug.LogFormat("<color=red>{0}: You attempted to select a non interactable Skill Tree Slot ({1})</color>", Time.frameCount, m_skillTreeType);
		}
	}

	public void OnDeselect(BaseEventData eventData)
	{
		m_selected = false;
		m_highlightGO.SetActive(value: false);
		Vector3 localEulerAngles = base.transform.localEulerAngles;
		localEulerAngles.z = 0f;
		base.transform.localEulerAngles = localEulerAngles;
	}

	private void OnDisable()
	{
		m_selected = false;
		m_highlightGO.SetActive(value: false);
		m_alreadyUnlockedEventPlayed = false;
	}

	public IEnumerator AnimateBGImage()
	{
		HasAnimated = true;
		Vector3 effectPosition = base.transform.position + (Vector3)AnimEffectOffset;
		effectPosition.z = -10f;
		if (!string.IsNullOrEmpty(BGAnimatorParam))
		{
			switch (m_animType)
			{
			case SkillTreeAnimType.CastleWingHorizontal_Foundation:
			case SkillTreeAnimType.CastleWingVertical_Foundation:
				AudioManager.PlayOneShot(null, "event:/SFX/UpgradingCastle/sfx_upgrade_castle_buildFoundation");
				break;
			case SkillTreeAnimType.CastleWingHorizontal_Tower:
			case SkillTreeAnimType.CastleWingVertical_Tower:
				AudioManager.PlayOneShot(null, "event:/SFX/UpgradingCastle/sfx_upgrade_castle_buildTower");
				break;
			case SkillTreeAnimType.CastleWingHorizontal_Wooden:
			case SkillTreeAnimType.CastleWingVertical_Wooden:
				AudioManager.PlayOneShot(null, "event:/SFX/UpgradingCastle/sfx_upgrade_castle_wooden_structure");
				break;
			case SkillTreeAnimType.Forest_Foundation:
				AudioManager.PlayOneShot(null, "event:/SFX/UpgradingCastle/sfx_upgrade_castle_tree_grow");
				break;
			case SkillTreeAnimType.Forest_Grow:
				AudioManager.PlayOneShot(null, "event:/SFX/UpgradingCastle/sfx_upgrade_castle_garden_expand");
				break;
			case SkillTreeAnimType.FlagPole:
				AudioManager.PlayOneShot(null, "event:/SFX/UpgradingCastle/sfx_upgrade_castle_flagpole");
				break;
			case SkillTreeAnimType.WeatherVane:
				AudioManager.PlayOneShot(null, "event:/SFX/UpgradingCastle/sfx_upgrade_castle_flagpole");
				break;
			case SkillTreeAnimType.BalconyOrSign:
				AudioManager.PlayOneShot(null, "event:/SFX/UpgradingCastle/sfx_upgrade_castle_balcony_small");
				break;
			case SkillTreeAnimType.TireSwing:
				AudioManager.PlayOneShot(null, "event:/SFX/UpgradingCastle/sfx_upgrade_castle_flagpole");
				break;
			case SkillTreeAnimType.Cave:
				AudioManager.PlayOneShot(null, "event:/SFX/UpgradingCastle/sfx_upgrade_castle_digDeep");
				break;
			}
			switch (m_animType)
			{
			case SkillTreeAnimType.CastleWingHorizontal_Foundation:
			case SkillTreeAnimType.CastleWingHorizontal_Tower:
			case SkillTreeAnimType.CastleWingHorizontal_Wooden:
			case SkillTreeAnimType.Forest_Foundation:
				EffectManager.PlayEffect(base.gameObject, m_castleAnimator, "CameraShakeVerySmall_Effect", Vector3.zero, 1f, EffectStopType.Gracefully);
				EffectManager.PlayEffect(base.gameObject, m_castleAnimator, "SkillTreeCastle_HorizontalDust_Effect", effectPosition, 0f, EffectStopType.Gracefully);
				break;
			case SkillTreeAnimType.CastleWingVertical_Foundation:
			case SkillTreeAnimType.CastleWingVertical_Tower:
			case SkillTreeAnimType.CastleWingVertical_Wooden:
				EffectManager.PlayEffect(base.gameObject, m_castleAnimator, "CameraShakeVerySmall_Effect", Vector3.zero, 1f, EffectStopType.Gracefully);
				EffectManager.PlayEffect(base.gameObject, m_castleAnimator, "SkillTreeCastle_VerticalDust_Effect", effectPosition, 0f, EffectStopType.Gracefully);
				break;
			case SkillTreeAnimType.Cave:
				EffectManager.PlayEffect(base.gameObject, m_castleAnimator, "CameraShakeVerySmall_Effect", Vector3.zero, 0.6f, EffectStopType.Gracefully);
				EffectManager.PlayEffect(base.gameObject, m_castleAnimator, "SkillTreeCastle_CaveAppear_Effect", effectPosition, 0f, EffectStopType.Gracefully);
				break;
			case SkillTreeAnimType.Forest_Grow:
			case SkillTreeAnimType.FlagPole:
			case SkillTreeAnimType.WeatherVane:
			case SkillTreeAnimType.BalconyOrSign:
			case SkillTreeAnimType.TireSwing:
			case SkillTreeAnimType.CameraShakeOnly:
				EffectManager.PlayEffect(base.gameObject, m_castleAnimator, "CameraShakeVerySmall_Effect", Vector3.zero, 1f, EffectStopType.Gracefully);
				break;
			}
			m_castleAnimator.SetBool(BGAnimatorParam, value: false);
			m_castleAnimator.Update(1f);
			m_castleAnimator.SetBool(BGAnimatorParam, value: true);
			float delay = Time.time + 1.5f;
			while (Time.time < delay)
			{
				yield return null;
			}
			AudioManager.PlayOneShot(null, "event:/SFX/UpgradingCastle/sfx_upgrade_castle_constructionEnd");
		}
		m_alreadyUnlockedEventPlayed = true;
	}

	public void RefreshSlotState(bool updateAnimParams)
	{
		m_iconCanvasGroup.alpha = 1f;
		if (m_newIndicator.gameObject.activeSelf)
		{
			m_newIndicator.gameObject.SetActive(value: false);
		}
		if (m_levelLockGO.activeSelf)
		{
			m_levelLockGO.SetActive(value: false);
		}
		if (SkillTreeType != 0)
		{
			SkillTreeObj skillTreeObj = SkillTreeManager.GetSkillTreeObj(SkillTreeType);
			if (skillTreeObj.Level == 0 && !skillTreeObj.IsLocked && !skillTreeObj.IsSoulLocked)
			{
				m_iconCanvasGroup.alpha = 0.5f;
			}
			if (HasAnimParam && updateAnimParams)
			{
				UpdateAnimatorParams();
			}
			int maxLevel = skillTreeObj.MaxLevel;
			int clampedLevel = skillTreeObj.ClampedLevel;
			m_levelText.text = $"{clampedLevel} / {maxLevel}";
			if (!skillTreeObj.IsLocked && !skillTreeObj.IsSoulLocked)
			{
				m_levelText.gameObject.SetActive(value: true);
			}
			else
			{
				m_levelText.gameObject.SetActive(value: false);
			}
			if (clampedLevel >= maxLevel)
			{
				m_iconFrame.overrideSprite = m_goldFrameSprite;
			}
			else
			{
				m_iconFrame.overrideSprite = null;
			}
			if (skillTreeObj != null && !skillTreeObj.IsLocked && !skillTreeObj.IsLevelLocked && !skillTreeObj.IsSoulLocked && clampedLevel < maxLevel && SaveManager.PlayerSaveData.GoldCollectedIncludingBank >= skillTreeObj.GoldCostWithLevelAppreciation && !m_newIndicator.gameObject.activeSelf)
			{
				m_newIndicator.gameObject.SetActive(value: true);
			}
			if (skillTreeObj != null && skillTreeObj.IsLevelLocked)
			{
				m_levelLockText.text = skillTreeObj.UnlockLevel.ToString();
				m_levelLockGO.SetActive(value: true);
			}
		}
	}

	public void UpdateAnimatorParams()
	{
		if (SkillTreeManager.GetSkillTreeObj(SkillTreeType).Level > 0)
		{
			m_castleAnimator.SetBool(BGAnimatorParam, value: true);
			m_castleAnimator.Update(2f);
			HasAnimated = true;
			if (!m_alreadyUnlockedEventPlayed)
			{
				m_alreadyUnlockedEventPlayed = true;
				OnAlreadyUnlockedEvent.Invoke();
			}
		}
		else
		{
			m_castleAnimator.SetBool(BGAnimatorParam, value: false);
		}
	}

	public void UnlockConnectedSkillSlots()
	{
		for (int i = 0; i < m_unlockSlotList.Count; i++)
		{
			SkillTreeSlot skillTreeSlot = m_unlockSlotList[i];
			bool flag = m_unlockSlotDisableList[i];
			if ((bool)skillTreeSlot && !flag && skillTreeSlot.HasData)
			{
				SkillUnlockState skillUnlockState = skillTreeSlot.m_skillTreeData.SkillUnlockState;
				if ((uint)skillUnlockState <= 1u)
				{
					skillTreeSlot.gameObject.SetActive(value: true);
				}
			}
		}
		UpdateNavigationNodes();
	}

	public void UpdateNavigationNodes()
	{
		Navigation navigation = m_button.navigation;
		int recursionCount = 0;
		SkillTreeSlot skillTreeSlot = RecursiveNodeSearch(NodeDirection.Up, recursionCount);
		if ((bool)skillTreeSlot && !DisableTopNavigationNode)
		{
			navigation.selectOnUp = skillTreeSlot.Button;
		}
		recursionCount = 0;
		SkillTreeSlot skillTreeSlot2 = RecursiveNodeSearch(NodeDirection.Down, recursionCount);
		if ((bool)skillTreeSlot2 && !DisableBottomNavigationNode)
		{
			navigation.selectOnDown = skillTreeSlot2.Button;
		}
		recursionCount = 0;
		SkillTreeSlot skillTreeSlot3 = RecursiveNodeSearch(NodeDirection.Left, recursionCount);
		if ((bool)skillTreeSlot3 && !DisableLeftNavigationNode)
		{
			navigation.selectOnLeft = skillTreeSlot3.Button;
		}
		recursionCount = 0;
		SkillTreeSlot skillTreeSlot4 = RecursiveNodeSearch(NodeDirection.Right, recursionCount);
		if ((bool)skillTreeSlot4 && !DisableRightNavigationNode)
		{
			navigation.selectOnRight = skillTreeSlot4.Button;
		}
		m_button.navigation = navigation;
	}

	public SkillTreeSlot RecursiveNodeSearch(NodeDirection direction, int recursionCount)
	{
		recursionCount++;
		if (recursionCount > 20)
		{
			throw new Exception("No: " + direction.ToString() + " " + m_skillTreeType);
		}
		SkillTreeSlot result = null;
		switch (direction)
		{
		case NodeDirection.Up:
			if ((bool)m_topNode)
			{
				result = ((m_topNode.isActiveAndEnabled && m_topNode.HasData) ? m_topNode : m_topNode.RecursiveNodeSearch(direction, recursionCount));
			}
			break;
		case NodeDirection.Down:
			if ((bool)m_bottomNode)
			{
				result = ((m_bottomNode.isActiveAndEnabled && m_bottomNode.HasData) ? m_bottomNode : m_bottomNode.RecursiveNodeSearch(direction, recursionCount));
			}
			break;
		case NodeDirection.Left:
			if ((bool)m_leftNode)
			{
				result = ((m_leftNode.isActiveAndEnabled && m_leftNode.HasData) ? m_leftNode : m_leftNode.RecursiveNodeSearch(direction, recursionCount));
			}
			break;
		case NodeDirection.Right:
			if ((bool)m_rightNode)
			{
				result = ((m_rightNode.isActiveAndEnabled && m_rightNode.HasData) ? m_rightNode : m_rightNode.RecursiveNodeSearch(direction, recursionCount));
			}
			break;
		}
		return result;
	}
}
