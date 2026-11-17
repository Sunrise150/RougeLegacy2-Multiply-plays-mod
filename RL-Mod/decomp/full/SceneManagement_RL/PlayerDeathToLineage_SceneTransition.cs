using System.Collections;
using RL_Windows;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SceneManagement_RL;

public class PlayerDeathToLineage_SceneTransition : Transition_V2, ISceneLoadingTransition, ITransition
{
	[SerializeField]
	private float m_timeToFade = 1f;

	[SerializeField]
	private CanvasScaler m_canvasScaler;

	[SerializeField]
	private CanvasGroup m_bgFadeCanvasGroup;

	[SerializeField]
	private CanvasGroup m_portraitCanvasGroup;

	[SerializeField]
	private RawImage m_portraitRawImage;

	[SerializeField]
	private RectTransform m_portraitGORectTransform;

	[SerializeField]
	private CanvasGroup m_playerModelCanvasGroup;

	[SerializeField]
	private Camera m_playerModelCamera;

	[SerializeField]
	private CanvasGroup m_deathPanelCanvasGroup;

	[SerializeField]
	private TMP_Text m_slainText;

	[SerializeField]
	private TMP_Text m_tipText;

	[SerializeField]
	private TMP_Text m_rankText;

	[SerializeField]
	private TMP_Text m_rankCounterText;

	private RawImage m_playerModelRawImage;

	private RenderTexture m_portraitToRender;

	private Vector3[] m_worldCorners = new Vector3[4];

	public override TransitionID ID => TransitionID.PlayerDeathToLineage;

	private void OnEnable()
	{
		m_bgFadeCanvasGroup.alpha = 0f;
		m_deathPanelCanvasGroup.alpha = 1f;
		m_portraitCanvasGroup.alpha = 0f;
		m_playerModelCanvasGroup.alpha = 0f;
	}

	public void SetData(RenderTexture portraitTexture, Vector3 playerAnchoredPos, string slainText, string tipText, string rankText, string rankCounterText)
	{
		if (m_portraitRawImage.texture == null)
		{
			RenderTexture renderTexture = new RenderTexture(portraitTexture);
			renderTexture.Create();
			m_portraitRawImage.texture = renderTexture;
		}
		if (m_playerModelRawImage == null)
		{
			m_playerModelRawImage = m_playerModelCanvasGroup.GetComponent<RawImage>();
			RectTransform component = m_playerModelCanvasGroup.GetComponent<RectTransform>();
			component.anchoredPosition = playerAnchoredPos;
			RenderTexture renderTexture2 = new RenderTexture((int)component.sizeDelta.x, (int)component.sizeDelta.y, 24, portraitTexture.format);
			renderTexture2.antiAliasing = Mathf.Clamp(QualitySettings.antiAliasing, 1, 10);
			renderTexture2.Create();
			m_playerModelRawImage.texture = renderTexture2;
			m_playerModelCamera.CopyFrom(CameraController.SoloCam.Camera);
			m_playerModelCamera.targetTexture = m_playerModelRawImage.texture as RenderTexture;
			m_playerModelCamera.clearFlags = CameraClearFlags.Color;
			m_playerModelCamera.backgroundColor = new Color(0f, 0f, 0f, 0f);
			float num = (float)m_playerModelRawImage.texture.width / 60f;
			float num2 = (float)m_playerModelRawImage.texture.height / 60f;
			float num3 = 1f;
			float num4 = 1f;
			if (!Mathf.Approximately(1.7777778f, AspectRatioManager.CurrentScreenAspectRatio))
			{
				if (AspectRatioManager.CurrentScreenAspectRatio < 1.7777778f)
				{
					num4 = AspectRatioManager.CurrentScreenAspectRatio / 1.7777778f;
					if (AspectRatioManager.Disable_16_9_Aspect)
					{
						num3 = num4;
					}
				}
				else
				{
					num3 = 1.7777778f / AspectRatioManager.CurrentScreenAspectRatio;
				}
				if (!AspectRatioManager.Disable_16_9_Aspect || AspectRatioManager.CurrentScreenAspectRatio < 1.7777778f)
				{
					num *= num3;
					num2 *= num4;
				}
			}
			m_playerModelCamera.projectionMatrix = Matrix4x4.Ortho((0f - num) * 0.5f, num * 0.5f, (0f - num2) * 0.5f, num2 * 0.5f, 0.1f * num3, 5000f * num4);
		}
		m_portraitToRender = portraitTexture;
		m_portraitGORectTransform.anchoredPosition = Vector2.zero;
		m_slainText.text = slainText;
		m_tipText.text = tipText;
		m_rankText.text = rankText;
		m_rankCounterText.text = rankCounterText;
		if (SkillTreeLogicHelper.IsTotemUnlocked())
		{
			m_rankText.gameObject.SetActive(value: true);
			m_rankCounterText.gameObject.SetActive(value: true);
		}
		else
		{
			m_rankText.gameObject.SetActive(value: false);
			m_rankCounterText.gameObject.SetActive(value: false);
		}
	}

	public IEnumerator TransitionIn()
	{
		PlayerController playerController = PlayerManager.GetPlayerController();
		Graphics.CopyTexture(m_portraitToRender, m_portraitRawImage.texture);
		_ = m_playerModelCamera.transform.position;
		Vector3 position = playerController.transform.position;
		position.z = m_playerModelCamera.transform.position.z;
		m_playerModelCamera.transform.position = position;
		m_playerModelCamera.Render();
		m_portraitCanvasGroup.alpha = 1f;
		m_playerModelCanvasGroup.alpha = 1f;
		(WindowManager.GetWindowController(WindowID.PlayerDeath) as PlayerDeathWindowController).DeactivatePortrait();
		yield return TweenManager.TweenTo_UnscaledTime(m_bgFadeCanvasGroup, m_timeToFade, Ease.None, "alpha", 1).TweenCoroutine;
		MapController.DeathMapCamera.gameObject.SetActive(value: false);
	}

	public IEnumerator TransitionOut()
	{
		m_bgFadeCanvasGroup.alpha = 1f;
		m_deathPanelCanvasGroup.alpha = 1f;
		m_portraitCanvasGroup.alpha = 1f;
		m_playerModelCanvasGroup.alpha = 1f;
		TweenManager.TweenTo_UnscaledTime(m_deathPanelCanvasGroup, 0.2f, Ease.None, "alpha", 0);
		TweenManager.TweenTo_UnscaledTime(m_playerModelCanvasGroup, 0.2f, Ease.None, "alpha", 0);
		float delay = Time.unscaledTime + 0.1f;
		while (Time.unscaledTime < delay)
		{
			yield return null;
		}
		float num = -222f;
		(WindowManager.GetWindowController(WindowID.Lineage) as LineageWindowController).RunOpenTransition(m_timeToFade);
		TweenManager.TweenBy_UnscaledTime(m_portraitGORectTransform, m_timeToFade, Ease.Quad.EaseInOut, "anchoredPosition.x", num);
		TweenManager.TweenTo_UnscaledTime(m_bgFadeCanvasGroup, m_timeToFade, Ease.None, "alpha", 0);
		yield return TweenManager.TweenTo_UnscaledTime(m_portraitCanvasGroup, m_timeToFade, Ease.None, "alpha", 0).TweenCoroutine;
	}

	public override IEnumerator Run()
	{
		yield break;
	}
}
