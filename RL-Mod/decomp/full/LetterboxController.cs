using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LetterboxController : MonoBehaviour
{
	[SerializeField]
	private RectTransform m_leftPillarBox;

	[SerializeField]
	private RectTransform m_rightPillarBox;

	[SerializeField]
	private RectTransform m_topLetterBox;

	[SerializeField]
	private RectTransform m_bottomLetterBox;

	private Action<MonoBehaviour, EventArgs> m_onAspectRatioChanged;

	private void Awake()
	{
		SetPillarBoxesEnabled(enable: false);
		SetLetterBoxesEnabled(enable: false);
		SceneManager.sceneLoaded += OnSceneLoaded;
		m_onAspectRatioChanged = OnAspectRatioChanged;
	}

	private void OnDestroy()
	{
		SceneManager.sceneLoaded -= OnSceneLoaded;
	}

	private void OnSceneLoaded(Scene arg0, LoadSceneMode arg1)
	{
		if (AspectRatioManager.Disable_16_9_Aspect)
		{
			SetLetterBoxesEnabled(enable: false);
			SetPillarBoxesEnabled(enable: false);
		}
	}

	private void OnEnable()
	{
		Messenger<SceneMessenger, SceneEvent>.AddListener(SceneEvent.AspectRatioChanged, m_onAspectRatioChanged);
	}

	private void OnDisable()
	{
		Messenger<SceneMessenger, SceneEvent>.RemoveListener(SceneEvent.AspectRatioChanged, m_onAspectRatioChanged);
	}

	private void OnAspectRatioChanged(object sender, EventArgs args)
	{
		SetLetterBoxesEnabled(enable: false);
		SetPillarBoxesEnabled(enable: false);
		if (!AspectRatioManager.Disable_16_9_Aspect && !AspectRatioManager.IsScreen_16_9_AspectRatio)
		{
			if (AspectRatioManager.CurrentScreenAspectRatio > 1.7777778f)
			{
				SetPillarBoxesEnabled(enable: true, 1.7777778f);
			}
			else
			{
				SetLetterBoxesEnabled(enable: true, 1.7777778f);
			}
		}
	}

	private void SetPillarBoxesEnabled(bool enable, float aspectRatio = -1f)
	{
		if (aspectRatio != -1f)
		{
			float num = GameResolutionManager.Resolution.x;
			float num2 = GameResolutionManager.Resolution.y;
			float num3 = aspectRatio * num2;
			float num4 = 1080f / num2;
			float num5 = (num - num3) * num4;
			num5 *= 0.5f;
			m_leftPillarBox.sizeDelta = new Vector2(num5, m_leftPillarBox.sizeDelta.y);
			m_rightPillarBox.sizeDelta = new Vector2(num5, m_rightPillarBox.sizeDelta.y);
		}
		if (m_leftPillarBox.gameObject.activeSelf != enable)
		{
			m_leftPillarBox.gameObject.SetActive(enable);
		}
		if (m_rightPillarBox.gameObject.activeSelf != enable)
		{
			m_rightPillarBox.gameObject.SetActive(enable);
		}
	}

	private void SetLetterBoxesEnabled(bool enable, float aspectRatio = -1f)
	{
		if (aspectRatio != -1f)
		{
			float num = GameResolutionManager.Resolution.x;
			float num2 = GameResolutionManager.Resolution.y;
			float num3 = 1f / aspectRatio * num;
			float num4 = 1920f / num;
			float num5 = (num2 - num3) * num4;
			num5 *= 0.5f;
			m_topLetterBox.sizeDelta = new Vector2(m_topLetterBox.sizeDelta.x, num5);
			m_bottomLetterBox.sizeDelta = new Vector2(m_bottomLetterBox.sizeDelta.x, num5);
		}
		if (m_topLetterBox.gameObject.activeSelf != enable)
		{
			m_topLetterBox.gameObject.SetActive(enable);
		}
		if (m_bottomLetterBox.gameObject.activeSelf != enable)
		{
			m_bottomLetterBox.gameObject.SetActive(enable);
		}
	}
}
