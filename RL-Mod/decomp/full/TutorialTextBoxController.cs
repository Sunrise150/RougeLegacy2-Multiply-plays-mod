using System;
using System.Collections;
using FMODUnity;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TutorialTextBoxController : MonoBehaviour
{
	private const float FADE_DURATION = 0.25f;

	private const float BACKGROUND_OPACITY = 0.425f;

	private Vector3 TUTORIAL_TEXT_POSITION = new Vector3(0f, 6f, 1f);

	[SerializeField]
	private CanvasGroup m_canvasGroup;

	[SerializeField]
	private TMP_Text m_text;

	[SerializeField]
	private bool m_stickToCamera;

	[SerializeField]
	private ConditionFlag m_mustHaveConditions;

	[SerializeField]
	private ConditionFlag m_mustNotHaveConditions;

	[SerializeField]
	private StudioEventEmitter m_showEventEmitter;

	[SerializeField]
	private StudioEventEmitter m_hideEventEmitter;

	private Tween m_fadeTween;

	private Action<MonoBehaviour, EventArgs> m_onPlayerExitRoom;

	private Action m_disableCanvasAction;

	private bool m_displayText;

	public bool StickToCamera
	{
		get
		{
			return m_stickToCamera;
		}
		set
		{
			m_stickToCamera = value;
		}
	}

	public ConditionFlag MustHaveConditions
	{
		get
		{
			return m_mustHaveConditions;
		}
		set
		{
			m_mustHaveConditions = value;
		}
	}

	public ConditionFlag MustNotHaveConditions
	{
		get
		{
			return m_mustNotHaveConditions;
		}
		set
		{
			m_mustNotHaveConditions = value;
		}
	}

	private void Awake()
	{
		m_disableCanvasAction = DisableCanvas;
		m_text.gameObject.SetActive(value: false);
		if (!m_displayText)
		{
			m_canvasGroup.gameObject.SetActive(value: false);
		}
		m_canvasGroup.alpha = 0f;
		m_onPlayerExitRoom = OnPlayerExitRoom;
	}

	private IEnumerator Start()
	{
		while (!SceneManager.GetActiveScene().isLoaded)
		{
			yield return null;
		}
		m_text.gameObject.SetActive(value: true);
		TextGlyphConverter glyphConverter = m_text.GetComponent<TextGlyphConverter>();
		while (!glyphConverter.IsInitialized)
		{
			yield return null;
		}
		if (m_stickToCamera)
		{
			base.gameObject.transform.SetParent(CameraController.GameCamera.transform, worldPositionStays: false);
			base.gameObject.transform.localPosition = TUTORIAL_TEXT_POSITION;
			SceneManager.activeSceneChanged += RemoveFromCamera;
		}
	}

	private void OnEnable()
	{
		if (m_stickToCamera)
		{
			Messenger<GameMessenger, GameEvent>.AddListener(GameEvent.PlayerExitRoom, m_onPlayerExitRoom);
		}
		if (TraitManager.IsTraitActive(TraitType.UpsideDown))
		{
			if (base.transform.localScale.y > 0f)
			{
				base.transform.SetLocalScaleY(base.transform.localScale.y * -1f);
			}
		}
		else if (base.transform.localScale.y < 0f)
		{
			base.transform.SetLocalScaleY(base.transform.localScale.y * -1f);
		}
	}

	private void OnDisable()
	{
		if (m_stickToCamera)
		{
			Messenger<GameMessenger, GameEvent>.RemoveListener(GameEvent.PlayerExitRoom, m_onPlayerExitRoom);
		}
		m_displayText = false;
	}

	private void OnPlayerExitRoom(object sender, EventArgs args)
	{
		HideText();
	}

	private void RemoveFromCamera(Scene arg0, Scene arg1)
	{
		SceneManager.activeSceneChanged -= RemoveFromCamera;
		UnityEngine.Object.Destroy(base.gameObject);
	}

	public void HideText()
	{
		if (m_canvasGroup.gameObject.activeSelf)
		{
			m_hideEventEmitter.Play();
			if (m_fadeTween != null)
			{
				m_fadeTween.StopTweenWithConditionChecks(runEndHandlers: false, m_canvasGroup, "TutorialTextTween");
			}
			m_fadeTween = TweenManager.TweenTo(m_canvasGroup, 0.25f, Ease.None, "alpha", 0);
			m_fadeTween.OnTweenCompleteRelay.AddListener(m_disableCanvasAction);
			m_fadeTween.ID = "TutorialTextTween";
		}
	}

	private void DisableCanvas()
	{
		m_canvasGroup.gameObject.SetActive(value: false);
	}

	public void DisplayText()
	{
		if (AllMustHaveConditionsMet() && AllMustNotHaveConditionsMet())
		{
			m_showEventEmitter.Play();
			if (m_fadeTween != null)
			{
				m_fadeTween.StopTweenWithConditionChecks(runEndHandlers: false, m_canvasGroup, "TutorialTextTween");
			}
			m_displayText = true;
			if (!m_canvasGroup.gameObject.activeSelf)
			{
				m_canvasGroup.gameObject.SetActive(value: true);
			}
			m_fadeTween = TweenManager.TweenTo(m_canvasGroup, 0.25f, Ease.None, "alpha", 1);
			m_fadeTween.ID = "TutorialTextTween";
		}
	}

	private bool AllMustHaveConditionsMet()
	{
		if (m_mustHaveConditions == ConditionFlag.None)
		{
			return true;
		}
		ConditionFlag[] typeArray = ConditionFlag_RL.TypeArray;
		foreach (ConditionFlag conditionFlag in typeArray)
		{
			if ((m_mustHaveConditions & conditionFlag) != 0 && !ConditionFlag_RL.IsConditionFulfilled(conditionFlag))
			{
				return false;
			}
		}
		return true;
	}

	private bool AllMustNotHaveConditionsMet()
	{
		if (m_mustNotHaveConditions == ConditionFlag.None)
		{
			return true;
		}
		ConditionFlag[] typeArray = ConditionFlag_RL.TypeArray;
		foreach (ConditionFlag conditionFlag in typeArray)
		{
			if ((m_mustNotHaveConditions & conditionFlag) != 0 && ConditionFlag_RL.IsConditionFulfilled(conditionFlag))
			{
				return false;
			}
		}
		return true;
	}
}
