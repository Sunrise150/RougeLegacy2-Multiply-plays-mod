using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PlayerHUDController : MonoBehaviour, ILocalizable
{
	private const float MIN_HP_BAR_3_DIGITS_WIDTH = 320f;

	private const float MIN_HP_BAR_2_DIGITS_WIDTH = 270f;

	private const float MIN_MP_BAR_3_DIGITS_WIDTH = 280f;

	private const float MIN_MP_BAR_2_DIGITS_WIDTH = 230f;

	private const float MAX_HP_BAR_WIDTH = 1000f;

	private const float MAX_MP_BAR_WIDTH = 800f;

	private const float APPROXIMATE_MAX_HP = 1000f;

	private const float APPROXIMATE_MAX_MP = 500f;

	[SerializeField]
	private CanvasGroup m_hudCanvasGroup;

	[SerializeField]
	private CanvasGroup m_minimapCanvasGroup;

	[SerializeField]
	private GameObject m_minimapFrameGO;

	[SerializeField]
	private TMP_Text m_levelText;

	[SerializeField]
	private RectTransform m_hpBarPanelRectTransform;

	[SerializeField]
	private Image m_hpBar;

	[SerializeField]
	private RectTransform m_hpBarRectTransform;

	[SerializeField]
	private Image m_hpExhaustBar;

	[SerializeField]
	private RectTransform m_hpExhaustBarRectTransform;

	[SerializeField]
	private TMP_Text m_hpText;

	[SerializeField]
	private RectTransform m_mpBarRectTransform;

	[SerializeField]
	private Image m_mpBar;

	[SerializeField]
	private TMP_Text m_mpText;

	[SerializeField]
	private RectTransform m_shieldIcon;

	[SerializeField]
	private TMP_Text m_shieldText;

	[SerializeField]
	private GameObject m_spellOrbIcon;

	[SerializeField]
	private TMP_Text m_spellOrbText;

	[SerializeField]
	private TMP_Text m_resolveText;

	[SerializeField]
	private RectTransform m_resolveTransform;

	[SerializeField]
	private FadeOutHUDCollider m_fadeOutHUDCollider;

	private Vector3 m_spellOrbStoredPos;

	private float m_storedMaxHP;

	private float m_storedMaxMP;

	private Tween m_playerHUDAlphaTween;

	private Tween m_minimapHUDAlphaTween;

	private TextPopupObj m_maxHealthTextPopup;

	private int m_lastMaxHealthChange;

	private float m_storedFadeOutHUDAlpha = 1f;

	private float m_storedFadeOutMinimapHUDAlpha = 1f;

	private BoxCollider2D m_fadeOutHPCollider;

	private Action<MonoBehaviour, EventArgs> m_tweenInHUD;

	private Action<MonoBehaviour, EventArgs> m_tweenOutHUD;

	private Action<MonoBehaviour, EventArgs> m_onWorldCreated;

	private Action<MonoBehaviour, EventArgs> m_onHealthChange;

	private Action<MonoBehaviour, EventArgs> m_onExhaustChange;

	private Action<MonoBehaviour, EventArgs> m_onMaxHealthChange;

	private Action<MonoBehaviour, EventArgs> m_onManaChange;

	private Action<MonoBehaviour, EventArgs> m_onResolveChange;

	private Action<MonoBehaviour, EventArgs> m_onSkillTreeOpen;

	private Action<MonoBehaviour, EventArgs> m_onSkillTreeClosed;

	private Action<MonoBehaviour, EventArgs> m_onSkillLevelChanged;

	private Action<MonoBehaviour, EventArgs> m_refreshText;

	public static bool IgnoreHealthChangeEvents;

	private Tween m_shieldTween;

	private int m_storedCurrentArmor;

	private void Awake()
	{
		m_fadeOutHUDCollider.SetCanvasGroup(m_hudCanvasGroup);
		m_fadeOutHPCollider = m_fadeOutHUDCollider.GetComponents<BoxCollider2D>()[1];
		SceneManager.sceneLoaded += OnSceneLoaded;
		m_tweenInHUD = TweenInHUD;
		m_tweenOutHUD = TweenOutHUD;
		m_onWorldCreated = OnWorldCreated;
		m_onHealthChange = OnHealthChange;
		m_onExhaustChange = OnExhaustChange;
		m_onMaxHealthChange = OnMaxHealthChange;
		m_onManaChange = OnManaChange;
		m_onResolveChange = OnResolveChange;
		m_onSkillTreeOpen = OnSkillTreeOpen;
		m_onSkillTreeClosed = OnSkillTreeClosed;
		m_onSkillLevelChanged = OnSkillLevelChanged;
		m_refreshText = RefreshText;
	}

	private void OnDestroy()
	{
		SceneManager.sceneLoaded -= OnSceneLoaded;
	}

	private void OnSceneLoaded(Scene scene, LoadSceneMode loadSceneMode)
	{
		if (GameUtility.SceneHasRooms && SceneLoadingUtility.ActiveScene.name != SceneLoadingUtility.GetSceneName(SceneID.Lineage))
		{
			if (!base.gameObject.activeSelf)
			{
				base.gameObject.SetActive(value: true);
			}
		}
		else if (base.gameObject.activeSelf)
		{
			base.gameObject.SetActive(value: false);
		}
	}

	private IEnumerator Start()
	{
		m_spellOrbStoredPos = m_spellOrbIcon.transform.parent.localPosition;
		while (!PlayerManager.IsInstantiated)
		{
			yield return null;
		}
		PlayerController playerController = PlayerManager.GetPlayerController();
		UpdateHealth(playerController);
		UpdateMana(playerController);
		UpdateShield(playerController);
		UpdateSpellOrbs(playerController);
		UpdateResolve(playerController);
		UpdateExhaust(playerController);
		OnSkillLevelChanged(null, null);
	}

	private void OnEnable()
	{
		m_spellOrbIcon.gameObject.SetActive(value: false);
		Messenger<UIMessenger, UIEvent>.AddListener(UIEvent.DisplayPlayerHUD, m_tweenInHUD);
		Messenger<UIMessenger, UIEvent>.AddListener(UIEvent.HidePlayerHUD, m_tweenOutHUD);
		Messenger<GameMessenger, GameEvent>.AddListener(GameEvent.WorldCreationComplete, m_onWorldCreated);
		Messenger<GameMessenger, GameEvent>.AddListener(GameEvent.PlayerHealthChange, m_onHealthChange);
		Messenger<GameMessenger, GameEvent>.AddListener(GameEvent.PlayerExhaustChange, m_onExhaustChange);
		Messenger<GameMessenger, GameEvent>.AddListener(GameEvent.PlayerMaxHealthChange, m_onMaxHealthChange);
		Messenger<GameMessenger, GameEvent>.AddListener(GameEvent.PlayerManaChange, m_onManaChange);
		Messenger<GameMessenger, GameEvent>.AddListener(GameEvent.PlayerResolveChanged, m_onResolveChange);
		Messenger<UIMessenger, UIEvent>.AddListener(UIEvent.SkillTree_Opened, m_onSkillTreeOpen);
		Messenger<UIMessenger, UIEvent>.AddListener(UIEvent.SkillTree_Closed, m_onSkillTreeClosed);
		Messenger<UIMessenger, UIEvent>.AddListener(UIEvent.PauseWindow_Opened, m_onSkillTreeOpen);
		Messenger<UIMessenger, UIEvent>.AddListener(UIEvent.PauseWindow_Closed, m_onSkillTreeClosed);
		Messenger<UIMessenger, UIEvent>.AddListener(UIEvent.SkillLevelChanged, m_onSkillLevelChanged);
		Messenger<UIMessenger, UIEvent>.AddListener(UIEvent.LanguageChanged, m_refreshText);
	}

	private void OnDisable()
	{
		Messenger<UIMessenger, UIEvent>.RemoveListener(UIEvent.DisplayPlayerHUD, m_tweenInHUD);
		Messenger<UIMessenger, UIEvent>.RemoveListener(UIEvent.HidePlayerHUD, m_tweenOutHUD);
		Messenger<GameMessenger, GameEvent>.RemoveListener(GameEvent.WorldCreationComplete, m_onWorldCreated);
		Messenger<GameMessenger, GameEvent>.RemoveListener(GameEvent.PlayerHealthChange, m_onHealthChange);
		Messenger<GameMessenger, GameEvent>.RemoveListener(GameEvent.PlayerExhaustChange, m_onExhaustChange);
		Messenger<GameMessenger, GameEvent>.RemoveListener(GameEvent.PlayerMaxHealthChange, m_onMaxHealthChange);
		Messenger<GameMessenger, GameEvent>.RemoveListener(GameEvent.PlayerManaChange, m_onManaChange);
		Messenger<GameMessenger, GameEvent>.RemoveListener(GameEvent.PlayerResolveChanged, m_onResolveChange);
		Messenger<UIMessenger, UIEvent>.RemoveListener(UIEvent.SkillTree_Opened, m_onSkillTreeOpen);
		Messenger<UIMessenger, UIEvent>.RemoveListener(UIEvent.SkillTree_Closed, m_onSkillTreeClosed);
		Messenger<UIMessenger, UIEvent>.RemoveListener(UIEvent.PauseWindow_Opened, m_onSkillTreeOpen);
		Messenger<UIMessenger, UIEvent>.RemoveListener(UIEvent.PauseWindow_Closed, m_onSkillTreeClosed);
		Messenger<UIMessenger, UIEvent>.RemoveListener(UIEvent.SkillLevelChanged, m_onSkillLevelChanged);
		Messenger<UIMessenger, UIEvent>.RemoveListener(UIEvent.LanguageChanged, m_refreshText);
	}

	private void TweenOutHUD(object sender, EventArgs args)
	{
		m_storedFadeOutHUDAlpha = 1f;
		if (m_hudCanvasGroup.alpha != 0f)
		{
			m_storedFadeOutHUDAlpha = m_hudCanvasGroup.alpha;
		}
		m_storedFadeOutMinimapHUDAlpha = 1f;
		if (m_minimapCanvasGroup.alpha != 0f)
		{
			m_storedFadeOutMinimapHUDAlpha = m_minimapCanvasGroup.alpha;
		}
		float duration = 0f;
		if (args != null && args is PlayerHUDVisibilityEventArgs playerHUDVisibilityEventArgs)
		{
			duration = playerHUDVisibilityEventArgs.TweenDuration;
		}
		if (m_playerHUDAlphaTween != null)
		{
			m_playerHUDAlphaTween.StopTweenWithConditionChecks(runEndHandlers: false, m_hudCanvasGroup);
		}
		if (m_minimapHUDAlphaTween != null)
		{
			m_minimapHUDAlphaTween.StopTweenWithConditionChecks(runEndHandlers: false, m_minimapCanvasGroup);
		}
		m_playerHUDAlphaTween = TweenManager.TweenTo_UnscaledTime(m_hudCanvasGroup, duration, Ease.None, "alpha", 0);
		m_minimapHUDAlphaTween = TweenManager.TweenTo_UnscaledTime(m_minimapCanvasGroup, duration, Ease.None, "alpha", 0);
	}

	private void TweenInHUD(object sender, EventArgs args)
	{
		float duration = 0f;
		if (args != null && args is PlayerHUDVisibilityEventArgs playerHUDVisibilityEventArgs)
		{
			duration = playerHUDVisibilityEventArgs.TweenDuration;
		}
		if (m_playerHUDAlphaTween != null)
		{
			m_playerHUDAlphaTween.StopTweenWithConditionChecks(runEndHandlers: false, m_hudCanvasGroup);
		}
		if (m_minimapHUDAlphaTween != null)
		{
			m_minimapHUDAlphaTween.StopTweenWithConditionChecks(runEndHandlers: false, m_minimapCanvasGroup);
		}
		m_playerHUDAlphaTween = TweenManager.TweenTo_UnscaledTime(m_hudCanvasGroup, duration, Ease.None, "alpha", m_storedFadeOutHUDAlpha);
		m_minimapHUDAlphaTween = TweenManager.TweenTo_UnscaledTime(m_minimapCanvasGroup, duration, Ease.None, "alpha", m_storedFadeOutMinimapHUDAlpha);
	}

	private void OnWorldCreated(object sender, EventArgs args)
	{
		if (m_playerHUDAlphaTween != null)
		{
			m_playerHUDAlphaTween.StopTweenWithConditionChecks(runEndHandlers: false, m_hudCanvasGroup);
		}
		if (m_minimapHUDAlphaTween != null)
		{
			m_minimapHUDAlphaTween.StopTweenWithConditionChecks(runEndHandlers: false, m_minimapCanvasGroup);
		}
		m_hudCanvasGroup.alpha = 1f;
		m_minimapCanvasGroup.alpha = 1f;
		m_minimapCanvasGroup.gameObject.SetActive(value: true);
		if (!TraitManager.IsTraitActive(TraitType.MapReveal))
		{
			m_minimapFrameGO.SetActive(value: true);
		}
		else
		{
			m_minimapFrameGO.SetActive(value: false);
		}
	}

	private void OnSkillTreeOpen(object sender, EventArgs eventArgs)
	{
		m_storedFadeOutHUDAlpha = m_hudCanvasGroup.alpha;
		m_hudCanvasGroup.alpha = 0f;
		m_minimapCanvasGroup.gameObject.SetActive(value: false);
	}

	private void OnSkillTreeClosed(object sender, EventArgs eventArgs)
	{
		m_hudCanvasGroup.alpha = m_storedFadeOutHUDAlpha;
		m_minimapCanvasGroup.gameObject.SetActive(value: true);
		if (!TraitManager.IsTraitActive(TraitType.MapReveal))
		{
			m_minimapFrameGO.SetActive(value: true);
		}
		else
		{
			m_minimapFrameGO.SetActive(value: false);
		}
	}

	private void OnSkillLevelChanged(object sender, EventArgs eventArgs)
	{
		m_levelText.text = string.Format(LocalizationManager.GetString("LOC_ID_HUD_TITLE_CHARACTER_LEVEL_1", isFemale: false), SkillTreeManager.GetTotalSkillObjLevel());
		Vector2 anchoredPosition = m_resolveTransform.anchoredPosition;
		anchoredPosition.x = m_levelText.preferredWidth;
		m_resolveTransform.anchoredPosition = anchoredPosition;
	}

	private void OnHealthChange(object sender, EventArgs eventArgs)
	{
		if (!IgnoreHealthChangeEvents)
		{
			PlayerController playerController = PlayerManager.GetPlayerController();
			UpdateHealth(playerController);
			UpdateShield(playerController);
			UpdateSpellOrbs(playerController);
			UpdateExhaust(playerController);
		}
	}

	private void OnExhaustChange(object sender, EventArgs eventArgs)
	{
		PlayerController playerController = PlayerManager.GetPlayerController();
		UpdateExhaust(playerController);
	}

	private void UpdateExhaust(PlayerController playerController)
	{
		float num = (float)playerController.CurrentExhaust * 0.01f;
		Vector2 sizeDelta = m_hpBarRectTransform.sizeDelta;
		float num2 = m_hpBarPanelRectTransform.sizeDelta.x + sizeDelta.x;
		if (playerController.ActualMaxHealth > 0)
		{
			num2 *= playerController.CurrentHealth / (float)playerController.ActualMaxHealth;
		}
		sizeDelta.x = num2 - m_hpBarPanelRectTransform.sizeDelta.x;
		m_hpExhaustBarRectTransform.sizeDelta = sizeDelta;
		float num3 = (float)playerController.ActualMaxHealth * num;
		float num4 = 1f;
		if (playerController.CurrentHealth > 0f)
		{
			num4 = Mathf.Clamp((playerController.CurrentHealth - num3) / playerController.CurrentHealth, 0f, 1f);
		}
		m_hpExhaustBar.fillAmount = 1f - num4;
	}

	private void OnMaxHealthChange(object sender, EventArgs eventArgs)
	{
		MaxHealthChangeEventArgs maxHealthChangeEventArgs = eventArgs as MaxHealthChangeEventArgs;
		int num = (int)(maxHealthChangeEventArgs.NewMaxHealthValue - maxHealthChangeEventArgs.PrevMaxHealthValue);
		if (num != 0)
		{
			Vector2 absPos = new Vector2(m_hpBarPanelRectTransform.rect.width, 0f - m_hpBarPanelRectTransform.sizeDelta.y);
			if (m_shieldIcon.gameObject.activeSelf)
			{
				absPos.x += m_shieldIcon.sizeDelta.x + 80f;
			}
			else
			{
				absPos.x += 80f;
			}
			if ((bool)m_maxHealthTextPopup && m_maxHealthTextPopup.gameObject.activeInHierarchy)
			{
				num = m_lastMaxHealthChange + num;
				m_maxHealthTextPopup.gameObject.SetActive(value: false);
			}
			string text = string.Format(LocalizationManager.GetString("LOC_ID_HEALING_ROOM_POP_UP_MAX_HEALTH_1", isFemale: false), num);
			if (num < 0)
			{
				text = "<color=\"red\">" + text.Replace('+', ' ') + "</color>";
			}
			m_maxHealthTextPopup = TextPopupManager.DisplayTextAtAbsPos(TextPopupType.MaxHealthChange, text, absPos, m_hpBarPanelRectTransform.gameObject, TextAlignmentOptions.Right);
			m_lastMaxHealthChange = num;
		}
	}

	private void UpdateHealth(PlayerController playerController)
	{
		bool flag = (m_storedCurrentArmor > 0 && playerController.CurrentArmor <= 0) || (m_storedCurrentArmor <= 0 && playerController.CurrentArmor > 0);
		if ((float)playerController.ActualMaxHealth != m_storedMaxHP || flag)
		{
			float num = ((float)playerController.ActualMaxHealth + 220f) / 1000f;
			float value = 1000f * num;
			float min = ((playerController.ActualMaxHealth < 100) ? 270f : 320f);
			Vector2 sizeDelta = m_hpBarPanelRectTransform.sizeDelta;
			sizeDelta.x = Mathf.Clamp(value, min, 1000f);
			m_hpBarPanelRectTransform.sizeDelta = sizeDelta;
			m_storedMaxHP = playerController.ActualMaxHealth;
			Vector2 anchoredPosition = m_shieldIcon.anchoredPosition;
			anchoredPosition.x = m_hpBarPanelRectTransform.anchoredPosition.x + m_hpBarPanelRectTransform.sizeDelta.x + 70f;
			m_shieldIcon.anchoredPosition = anchoredPosition;
			Vector2 offset = m_fadeOutHPCollider.offset;
			offset.x = m_hpBarPanelRectTransform.anchoredPosition.x + sizeDelta.x / 4f;
			if (playerController.CurrentArmor > 0)
			{
				float num2 = m_shieldIcon.sizeDelta.x / 2f;
				sizeDelta.x += num2;
				offset.x += num2;
			}
			m_fadeOutHPCollider.size = sizeDelta;
			m_fadeOutHPCollider.offset = offset;
		}
		float fillAmount = (float)playerController.CurrentHealthAsInt / (float)playerController.ActualMaxHealth;
		if (!TraitManager.IsTraitActive(TraitType.NoHealthBar))
		{
			m_hpBar.fillAmount = fillAmount;
		}
		else
		{
			m_hpBar.fillAmount = 0f;
		}
		if (TraitManager.IsTraitActive(TraitType.NoHealthBar))
		{
			if (m_hpText.gameObject.activeSelf)
			{
				m_hpText.gameObject.SetActive(value: false);
			}
			return;
		}
		if (!m_hpText.gameObject.activeSelf)
		{
			m_hpText.gameObject.SetActive(value: true);
		}
		m_hpText.text = $"{playerController.CurrentHealthAsInt} / {playerController.ActualMaxHealth}";
	}

	private void OnManaChange(object sender, EventArgs eventArgs)
	{
		ManaChangeEventArgs manaChangeEventArgs = eventArgs as ManaChangeEventArgs;
		UpdateMana(manaChangeEventArgs.Player);
		UpdateSpellOrbs(PlayerManager.GetPlayerController());
	}

	private void OnResolveChange(object sender, EventArgs eventArgs)
	{
		UpdateResolve(PlayerManager.GetPlayerController());
	}

	private void UpdateMana(PlayerController player)
	{
		if (m_storedMaxMP != (float)player.ActualMaxMana)
		{
			float num = (float)player.ActualMaxMana / 500f;
			float value = 800f * num;
			float min = ((player.ActualMaxMana < 100) ? 230f : 280f);
			Vector2 sizeDelta = m_mpBarRectTransform.sizeDelta;
			sizeDelta.x = Mathf.Clamp(value, min, 800f);
			m_mpBarRectTransform.sizeDelta = sizeDelta;
			m_storedMaxMP = player.ActualMaxMana;
		}
		float fillAmount = player.CurrentMana / (float)player.ActualMaxMana;
		m_mpBar.fillAmount = fillAmount;
		m_mpText.text = $"{player.CurrentManaAsInt} / {player.ActualMaxMana}";
	}

	private void UpdateShield(PlayerController playerController)
	{
		int currentArmor = playerController.CurrentArmor;
		if (currentArmor <= 0)
		{
			if (m_shieldIcon.gameObject.activeSelf)
			{
				m_shieldIcon.gameObject.SetActive(value: false);
				m_shieldText.gameObject.SetActive(value: false);
			}
		}
		else
		{
			if (!m_shieldIcon.gameObject.activeSelf)
			{
				m_shieldIcon.gameObject.SetActive(value: true);
			}
			if (m_storedCurrentArmor != currentArmor)
			{
				Vector3 one = Vector3.one;
				Vector3 vector = one;
				one *= 1.2f;
				m_shieldIcon.transform.localScale = one;
				if (m_shieldTween != null)
				{
					m_shieldTween.StopTweenWithConditionChecks(runEndHandlers: false, m_shieldIcon.transform);
				}
				m_shieldTween = TweenManager.TweenTo_UnscaledTime(m_shieldIcon.transform, 0.1f, Ease.None, "localScale.x", vector.x, "localScale.y", vector.y, "localScale.z", vector.z);
				if (currentArmor > 0)
				{
					m_shieldText.gameObject.SetActive(value: true);
					m_shieldText.text = currentArmor.ToString();
				}
				else
				{
					m_shieldText.gameObject.SetActive(value: false);
				}
			}
		}
		m_storedCurrentArmor = currentArmor;
	}

	private void UpdateSpellOrbs(PlayerController playerController)
	{
	}

	private void UpdateResolve(PlayerController playerController)
	{
		float actualResolve = playerController.ActualResolve;
		Color color = Color.white;
		if (actualResolve <= 0.5f)
		{
			color = Color.red;
		}
		else if (actualResolve <= 1f)
		{
			color = Color.yellow;
		}
		m_resolveText.color = color;
		m_resolveText.text = string.Format(LocalizationManager.GetString("LOC_ID_GENERAL_UI_PERCENT_1", isFemale: false), Mathf.RoundToInt(actualResolve * 100f));
	}

	public void RefreshText(object sender, EventArgs args)
	{
		OnSkillLevelChanged(null, null);
	}
}
