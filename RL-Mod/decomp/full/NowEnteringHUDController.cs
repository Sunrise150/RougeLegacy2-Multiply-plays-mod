using System;
using System.Collections;
using System.Linq;
using TMPro;
using UnityEngine;

public class NowEnteringHUDController : MonoBehaviour, ILocalizable
{
	[SerializeField]
	private float m_displayDuration = 3f;

	[SerializeField]
	private CanvasGroup m_bgCanvasGroup;

	[SerializeField]
	private TMP_Text m_biomeTitleText;

	[SerializeField]
	private RectTransform m_textMaskRectTransform;

	[SerializeField]
	private GameObject m_dangerGO;

	[SerializeField]
	private GameObject m_grayStarsGO;

	[SerializeField]
	private GameObject m_yellowStarsGO;

	[SerializeField]
	private GameObject m_unknownStarsGO;

	[SerializeField]
	private CanvasGroup m_biomeTextCanvasGroup;

	[SerializeField]
	private BiomeType[] m_allowedBiomes;

	private Tween m_bgTween;

	private Tween m_biomeTitleTween;

	private WaitRL_Yield m_waitYield;

	private float m_textMaskRectWidth;

	private Action<MonoBehaviour, EventArgs> m_onPlayerEnterBiome;

	private Action<MonoBehaviour, EventArgs> m_onPlayerDeath;

	private Action<MonoBehaviour, EventArgs> m_onForceNowEntering;

	private Action<MonoBehaviour, EventArgs> m_refreshText;

	private static int m_currentRiskLevel;

	private static string m_currentLocID;

	public static int CurrentRiskLevel => m_currentRiskLevel;

	public static string CurrentLocID => m_currentLocID;

	public float TextMaskRectWidth
	{
		get
		{
			return m_textMaskRectWidth;
		}
		set
		{
			m_textMaskRectWidth = value;
			if (m_textMaskRectTransform != null)
			{
				m_textMaskRectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, m_textMaskRectWidth);
				m_textMaskRectTransform.ForceUpdateRectTransforms();
			}
		}
	}

	private void Awake()
	{
		m_waitYield = new WaitRL_Yield(0f);
		m_bgCanvasGroup.gameObject.SetActive(value: false);
		m_biomeTextCanvasGroup.gameObject.SetActive(value: false);
		m_onPlayerEnterBiome = OnPlayerEnterBiome;
		m_onPlayerDeath = OnPlayerDeath;
		m_onForceNowEntering = OnForceNowEntering;
		m_refreshText = RefreshText;
	}

	private void OnEnable()
	{
		Messenger<GameMessenger, GameEvent>.AddListener(GameEvent.BiomeEnter, m_onPlayerEnterBiome);
		Messenger<GameMessenger, GameEvent>.AddListener(GameEvent.PlayerDeath, m_onPlayerDeath);
		Messenger<GameMessenger, GameEvent>.AddListener(GameEvent.PlayerFakedDeath, m_onPlayerDeath);
		Messenger<UIMessenger, UIEvent>.AddListener(UIEvent.ForceNowEntering, m_onForceNowEntering);
		Messenger<UIMessenger, UIEvent>.AddListener(UIEvent.LanguageChanged, m_refreshText);
	}

	private void OnDisable()
	{
		Messenger<GameMessenger, GameEvent>.RemoveListener(GameEvent.BiomeEnter, m_onPlayerEnterBiome);
		Messenger<GameMessenger, GameEvent>.RemoveListener(GameEvent.PlayerDeath, m_onPlayerDeath);
		Messenger<GameMessenger, GameEvent>.RemoveListener(GameEvent.PlayerFakedDeath, m_onPlayerDeath);
		Messenger<UIMessenger, UIEvent>.RemoveListener(UIEvent.ForceNowEntering, m_onForceNowEntering);
		Messenger<UIMessenger, UIEvent>.RemoveListener(UIEvent.LanguageChanged, m_refreshText);
	}

	private void OnPlayerDeath(object sender, EventArgs args)
	{
		ClearNowEnteringText();
	}

	private void OnForceNowEntering(object sender, EventArgs args)
	{
		if (args is ForceNowEnteringEventArgs forceNowEnteringEventArgs)
		{
			TriggerNowEnteringText(forceNowEnteringEventArgs.LocID, forceNowEnteringEventArgs.RiskLevel);
		}
	}

	private void OnPlayerEnterBiome(object sender, EventArgs args)
	{
		ClearNowEnteringText();
		BiomeEventArgs biomeEventArgs = args as BiomeEventArgs;
		if (m_allowedBiomes.Contains(biomeEventArgs.Biome) && (biomeEventArgs.Biome != BiomeType.Garden || SaveManager.PlayerSaveData.EndingSpawnRoom <= EndingSpawnRoomType.None))
		{
			BiomeData data = BiomeDataLibrary.GetData(biomeEventArgs.Biome);
			if ((bool)data)
			{
				string biomeNameLocID = data.BiomeNameLocID;
				int biomeLevel = BiomeCreation_EV.GetBiomeLevel(biomeEventArgs.Biome);
				TriggerNowEnteringText(biomeNameLocID, biomeLevel);
			}
		}
	}

	private void TriggerNowEnteringText(string locID, int riskLevel)
	{
		if (CutsceneManager.IsCutsceneActive || locID == m_currentLocID)
		{
			return;
		}
		m_currentLocID = locID;
		m_currentRiskLevel = riskLevel;
		m_biomeTitleText.text = LocalizationManager.GetString(m_currentLocID, isFemale: false);
		if (m_currentRiskLevel == -2)
		{
			m_grayStarsGO.SetActive(value: false);
			m_yellowStarsGO.SetActive(value: false);
			m_unknownStarsGO.SetActive(value: false);
			m_dangerGO.SetActive(value: false);
		}
		else if (m_currentRiskLevel == -1)
		{
			m_grayStarsGO.SetActive(value: false);
			m_yellowStarsGO.SetActive(value: false);
			m_unknownStarsGO.SetActive(value: true);
			m_dangerGO.SetActive(value: true);
		}
		else
		{
			m_grayStarsGO.SetActive(value: true);
			m_yellowStarsGO.SetActive(value: true);
			m_unknownStarsGO.SetActive(value: false);
			m_dangerGO.SetActive(value: true);
			GameObject gameObject = m_grayStarsGO.transform.GetChild(6).gameObject;
			GameObject gameObject2 = m_yellowStarsGO.transform.GetChild(6).gameObject;
			if (m_currentRiskLevel > 6)
			{
				gameObject.gameObject.SetActive(value: true);
				gameObject2.gameObject.SetActive(value: true);
			}
			else
			{
				gameObject.gameObject.SetActive(value: false);
				gameObject2.gameObject.SetActive(value: false);
			}
			for (int i = 0; i < m_yellowStarsGO.transform.childCount; i++)
			{
				GameObject gameObject3 = m_yellowStarsGO.transform.GetChild(i).gameObject;
				if (i < m_currentRiskLevel)
				{
					gameObject3.SetActive(value: true);
				}
				else
				{
					gameObject3.SetActive(value: false);
				}
			}
		}
		ClearNowEnteringText();
		m_bgCanvasGroup.gameObject.SetActive(value: true);
		m_biomeTextCanvasGroup.gameObject.SetActive(value: true);
		TextMaskRectWidth = 1f;
		m_bgCanvasGroup.alpha = 0f;
		m_biomeTextCanvasGroup.alpha = 1f;
		RefreshText(null, null);
		StartCoroutine(DisplayBiomeTitleCoroutine());
	}

	private void ClearNowEnteringText()
	{
		StopAllCoroutines();
		m_bgCanvasGroup.alpha = 0f;
		m_biomeTextCanvasGroup.alpha = 0f;
		if (m_bgCanvasGroup.gameObject.activeSelf)
		{
			m_bgCanvasGroup.gameObject.SetActive(value: false);
		}
		if (m_biomeTextCanvasGroup.gameObject.activeSelf)
		{
			m_biomeTextCanvasGroup.gameObject.SetActive(value: false);
		}
		if (m_bgTween != null)
		{
			m_bgTween.StopTweenWithConditionChecks(runEndHandlers: false, m_bgCanvasGroup, "NowEnteringTween");
		}
		if (m_biomeTitleTween != null)
		{
			m_biomeTitleTween.StopTweenWithConditionChecks(runEndHandlers: false, this, "NowEnteringTween");
		}
	}

	private IEnumerator DisplayBiomeTitleCoroutine()
	{
		float fadeSpeed = 0.5f;
		m_bgTween = TweenManager.TweenTo(m_bgCanvasGroup, fadeSpeed, Ease.Quad.EaseIn, "alpha", 1);
		m_bgTween.ID = "NowEnteringTween";
		float num = Mathf.Max(m_biomeTitleText.preferredWidth + 200f, 1000f);
		m_biomeTitleTween = TweenManager.TweenTo(this, 1f, Ease.Quad.EaseOut, "delay", 0.25f, "TextMaskRectWidth", num);
		m_biomeTitleTween.ID = "NowEnteringTween";
		yield return m_biomeTitleTween.TweenCoroutine;
		m_waitYield.CreateNew(m_displayDuration);
		yield return m_waitYield;
		m_bgTween = TweenManager.TweenTo(m_bgCanvasGroup, fadeSpeed, Ease.Quad.EaseIn, "alpha", 0);
		m_bgTween.ID = "NowEnteringTween";
		m_biomeTitleTween = TweenManager.TweenTo(m_biomeTextCanvasGroup, fadeSpeed, Ease.Quad.EaseIn, "delay", 0.25f, "alpha", 0);
		m_biomeTitleTween.ID = "NowEnteringTween";
		yield return m_biomeTitleTween.TweenCoroutine;
		m_bgCanvasGroup.gameObject.SetActive(value: false);
		m_biomeTextCanvasGroup.gameObject.SetActive(value: false);
	}

	public void RefreshText(object sender, EventArgs args)
	{
		m_biomeTitleText.text = LocalizationManager.GetString(m_currentLocID, isFemale: false);
	}
}
