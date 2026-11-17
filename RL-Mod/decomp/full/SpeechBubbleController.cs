using System;
using RL_Windows;
using TMPro;
using UnityEngine;

public class SpeechBubbleController : MonoBehaviour
{
	[SerializeField]
	private TMP_Text m_speechBubbleText;

	[SerializeField]
	private SpriteRenderer m_bg;

	[SerializeField]
	private GameObject m_arrowGO;

	[SerializeField]
	private GameObject m_offscreenArrowGO;

	[SerializeField]
	private bool m_displayOffscreen;

	private Tween m_appearTween;

	private Tween m_bounceTween;

	private Tween m_scaleTween;

	private Vector3 m_startingPos;

	private Vector3 m_absStartingPos;

	private Vector3 m_offscreenArrowStartingPos;

	private int m_storedLayer;

	private bool m_isInSkillTree;

	private IDisplaySpeechBubble[] m_displaySpeechBubbleConditions;

	private string m_overrideText;

	private bool m_isFlipped;

	private Action<MonoBehaviour, EventArgs> m_onSkillTreeOpened;

	private Action<MonoBehaviour, EventArgs> m_onSkillTreeClosed;

	public bool DisplayOffscreen
	{
		get
		{
			return m_displayOffscreen;
		}
		set
		{
			m_displayOffscreen = value;
		}
	}

	public SpeechBubbleType SpeechBubbleType { get; private set; }

	public void SetSpeechBubbleType(SpeechBubbleType bubbleType)
	{
		SpeechBubbleType = bubbleType;
		switch (bubbleType)
		{
		case SpeechBubbleType.OverrideText:
			m_speechBubbleText.text = m_overrideText;
			break;
		case SpeechBubbleType.Dialogue:
			m_speechBubbleText.text = "...";
			break;
		case SpeechBubbleType.Sleeping:
			m_speechBubbleText.text = "Zzz";
			break;
		case SpeechBubbleType.GearAvailable:
			m_speechBubbleText.text = LocalizationManager.GetString("LOC_ID_GENERAL_UI_NEW_1", isFemale: false).ToUpper();
			break;
		case SpeechBubbleType.TeleportToManor_PizzaGirlOnly:
			m_speechBubbleText.text = "!!";
			break;
		default:
			m_speechBubbleText.text = "!";
			break;
		}
	}

	private void Awake()
	{
		m_startingPos = base.transform.localPosition;
		m_startingPos.z = -1f;
		m_displaySpeechBubbleConditions = base.gameObject.GetRoot().GetComponentsInChildren<IDisplaySpeechBubble>();
		SetSpeechBubbleEnabled(enable: false);
		m_offscreenArrowGO.SetActive(value: false);
		m_onSkillTreeOpened = OnSkillTreeOpened;
		m_onSkillTreeClosed = OnSkillTreeClosed;
	}

	private void Start()
	{
		m_offscreenArrowStartingPos = m_offscreenArrowGO.transform.localPosition;
		m_absStartingPos = base.transform.position;
		m_absStartingPos.z = -1f;
		m_storedLayer = base.gameObject.layer;
		if (base.transform.lossyScale.x < 0f)
		{
			base.transform.SetLocalScaleX(base.transform.localScale.x * -1f);
			m_isFlipped = true;
		}
	}

	private void OnEnable()
	{
		if (m_displayOffscreen)
		{
			if (WindowManager.GetIsWindowOpen(WindowID.SkillTree))
			{
				m_isInSkillTree = true;
			}
			Messenger<UIMessenger, UIEvent>.AddListener(UIEvent.SkillTree_Opened, m_onSkillTreeOpened);
			Messenger<UIMessenger, UIEvent>.AddListener(UIEvent.SkillTree_Closed, m_onSkillTreeClosed);
		}
	}

	private void OnDisable()
	{
		if (m_displayOffscreen)
		{
			Messenger<UIMessenger, UIEvent>.RemoveListener(UIEvent.SkillTree_Opened, m_onSkillTreeOpened);
			Messenger<UIMessenger, UIEvent>.RemoveListener(UIEvent.SkillTree_Closed, m_onSkillTreeClosed);
		}
	}

	private void OnSkillTreeOpened(object sender, EventArgs args)
	{
		m_isInSkillTree = true;
	}

	private void OnSkillTreeClosed(object sender, EventArgs args)
	{
		m_isInSkillTree = false;
	}

	public void SetBGColor(Color color)
	{
		if ((bool)m_bg)
		{
			m_bg.color = color;
		}
	}

	public void SetOverrideText(string text)
	{
		m_overrideText = text;
	}

	public void SetSpeechBubbleEnabled(bool enable)
	{
		if (GameManager.IsApplicationClosing)
		{
			return;
		}
		if (enable)
		{
			if (!m_displaySpeechBubbleConditions.IsNativeNull() && m_displaySpeechBubbleConditions.Length != 0)
			{
				bool flag = false;
				IDisplaySpeechBubble[] displaySpeechBubbleConditions = m_displaySpeechBubbleConditions;
				foreach (IDisplaySpeechBubble displaySpeechBubble in displaySpeechBubbleConditions)
				{
					if (displaySpeechBubble.ShouldDisplaySpeechBubble)
					{
						flag = true;
						SpeechBubbleType bubbleType = displaySpeechBubble.BubbleType;
						if (bubbleType > SpeechBubbleType)
						{
							SetSpeechBubbleType(bubbleType);
						}
					}
				}
				if (flag)
				{
					if (!base.gameObject.activeSelf)
					{
						base.gameObject.SetActive(value: true);
					}
					return;
				}
				if (base.gameObject.activeSelf)
				{
					base.gameObject.SetActive(value: false);
				}
				SetSpeechBubbleType(SpeechBubbleType.None);
			}
			else if (!base.gameObject.activeSelf)
			{
				base.gameObject.SetActive(value: true);
			}
		}
		else
		{
			if (base.gameObject.activeSelf)
			{
				base.gameObject.SetActive(value: false);
			}
			SetSpeechBubbleType(SpeechBubbleType.None);
		}
	}

	private void LateUpdate()
	{
		if (!m_displayOffscreen)
		{
			return;
		}
		float num = 1f;
		float num2 = 0.5f;
		float num3 = 1.2f;
		float orthographicSize = CameraController.GameCamera.orthographicSize;
		float num4 = orthographicSize * (float)Screen.width / (float)Screen.height;
		float num5 = CameraController.GameCamera.transform.localPosition.x - num4;
		float num6 = CameraController.GameCamera.transform.localPosition.y - orthographicSize;
		Rect rect = new Rect(num5 + num, num6 + num2, (num4 - num) * 2f, (orthographicSize - num3) * 2f);
		if (!rect.Contains(m_absStartingPos) && !m_isInSkillTree)
		{
			Vector3 absStartingPos = m_absStartingPos;
			Vector3 vector = absStartingPos;
			float xMin = rect.xMin;
			float xMax = rect.xMax;
			float yMin = rect.yMin;
			float yMax = rect.yMax;
			if (absStartingPos.x < xMin)
			{
				vector.x = xMin;
			}
			else if (absStartingPos.x > xMax)
			{
				vector.x = xMax;
			}
			if (absStartingPos.y < yMin)
			{
				vector.y = yMin;
			}
			else if (absStartingPos.y > yMax)
			{
				vector.y = yMax;
			}
			base.transform.position = vector;
			if (m_arrowGO.activeSelf)
			{
				m_arrowGO.SetActive(value: false);
			}
			if (!m_offscreenArrowGO.activeSelf)
			{
				m_offscreenArrowGO.SetActive(value: true);
			}
			float zRot = CDGHelper.AngleBetweenPts(vector, m_absStartingPos);
			m_offscreenArrowGO.transform.SetLocalEulerZ(zRot);
			float num7 = 0.4f;
			float num8 = vector.x - absStartingPos.x;
			float num9 = Mathf.Clamp(num8, 0f - num7, num7);
			m_offscreenArrowGO.transform.SetLocalPositionX(m_offscreenArrowStartingPos.x - num9);
			float num10 = 0.45f;
			float num11 = vector.y - absStartingPos.y;
			float num12 = Mathf.Clamp(num11, 0f - num10, num10);
			m_offscreenArrowGO.transform.SetLocalPositionY(m_offscreenArrowStartingPos.y - num12);
			if (((num8 > 0f - num7 && num8 < num7 && num11 == 0f) || (num11 > 0f - num10 && num11 < num10 && num8 == 0f)) && m_offscreenArrowGO.activeSelf)
			{
				m_offscreenArrowGO.SetActive(value: false);
			}
			if (base.gameObject.layer != 23)
			{
				base.gameObject.SetLayerRecursively(23);
			}
		}
		else
		{
			base.transform.position = m_absStartingPos;
			if (!m_arrowGO.activeSelf)
			{
				m_arrowGO.SetActive(value: true);
			}
			if (m_offscreenArrowGO.activeSelf)
			{
				m_offscreenArrowGO.SetActive(value: false);
			}
			if (base.gameObject.layer != m_storedLayer)
			{
				base.gameObject.SetLayerRecursively(m_storedLayer);
			}
		}
	}
}
