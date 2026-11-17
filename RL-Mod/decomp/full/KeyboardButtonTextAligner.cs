using TMPro;
using UnityEngine;

public class KeyboardButtonTextAligner : MonoBehaviour
{
	[SerializeField]
	private int m_spriteIndex;

	[SerializeField]
	private TMP_Text m_parentTMPObject;

	[SerializeField]
	private Vector2 m_positionOffset;

	private RectTransform m_transformRect;

	private int m_cachedCharIndex = -1;

	private string m_cachedString;

	private bool m_textAlignSuccessful;

	public TMP_Text BaseTMPObject { get; set; }

	public int SpriteIndex
	{
		get
		{
			return m_spriteIndex;
		}
		set
		{
			m_spriteIndex = value;
		}
	}

	public TMP_Text ParentTMPObject
	{
		get
		{
			return m_parentTMPObject;
		}
		set
		{
			m_parentTMPObject = value;
		}
	}

	public Vector2 PositionOffset
	{
		get
		{
			return m_positionOffset;
		}
		set
		{
			m_positionOffset = value;
		}
	}

	private void Awake()
	{
		m_transformRect = GetComponent<RectTransform>();
	}

	private void LateUpdate()
	{
		if (ParentTMPObject != null)
		{
			UpdateTypewriting();
			UpdateAlpha();
		}
	}

	private void UpdateTypewriting()
	{
		if (ParentTMPObject.maxVisibleCharacters <= m_cachedCharIndex && BaseTMPObject.isActiveAndEnabled)
		{
			BaseTMPObject.enabled = false;
		}
		else if (ParentTMPObject.maxVisibleCharacters > m_cachedCharIndex && !BaseTMPObject.isActiveAndEnabled)
		{
			BaseTMPObject.enabled = true;
		}
	}

	private void UpdateAlpha()
	{
		if (BaseTMPObject.isActiveAndEnabled && BaseTMPObject.alpha != ParentTMPObject.alpha)
		{
			BaseTMPObject.alpha = ParentTMPObject.alpha;
		}
	}

	public void AlignText()
	{
		m_textAlignSuccessful = false;
		if (!ParentTMPObject || !BaseTMPObject || string.IsNullOrEmpty(BaseTMPObject.text))
		{
			return;
		}
		m_cachedCharIndex = -1;
		int num = 0;
		TMP_CharacterInfo[] characterInfo = ParentTMPObject.textInfo.characterInfo;
		for (int i = 0; i < characterInfo.Length; i++)
		{
			if (characterInfo[i].elementType == TMP_TextElementType.Sprite)
			{
				num++;
			}
			if (num == m_spriteIndex)
			{
				m_cachedCharIndex = i;
				break;
			}
		}
		m_cachedString = ParentTMPObject.text;
		if (m_cachedCharIndex != -1)
		{
			TMP_CharacterInfo tMP_CharacterInfo = ParentTMPObject.textInfo.characterInfo[m_cachedCharIndex];
			Vector3 bottomLeft = tMP_CharacterInfo.bottomLeft;
			Vector3 bottomRight = tMP_CharacterInfo.bottomRight;
			Vector3 topLeft = tMP_CharacterInfo.topLeft;
			float num2 = bottomRight.x - bottomLeft.x;
			float num3 = topLeft.y - bottomLeft.y;
			float num4 = 0.75f;
			num3 *= num4;
			bottomLeft.y = topLeft.y - num3;
			bottomRight.y = bottomLeft.y;
			Vector3 localPosition = new Rect(bottomLeft.x, bottomLeft.y, num2, num3).center + m_positionOffset;
			localPosition.z = m_transformRect.localPosition.z;
			m_transformRect.localPosition = localPosition;
			m_transformRect.sizeDelta = new Vector2(num2, num3);
			Vector3 localScale = m_transformRect.localScale;
			localScale.y = 0.75f;
			m_transformRect.localScale = localScale;
			Invoke("ForceUpdateBaseTMPObject", 0f);
			m_textAlignSuccessful = true;
		}
	}

	private void ForceUpdateBaseTMPObject()
	{
		if ((bool)BaseTMPObject)
		{
			BaseTMPObject.gameObject.SetActive(value: false);
			BaseTMPObject.gameObject.SetActive(value: true);
		}
	}
}
