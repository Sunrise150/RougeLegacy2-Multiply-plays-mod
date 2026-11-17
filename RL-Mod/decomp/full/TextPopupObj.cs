using System.Collections;
using System.Runtime.CompilerServices;
using TMPro;
using UnityEngine;

public abstract class TextPopupObj : MonoBehaviour, IGenericPoolObj
{
	[SerializeField]
	private bool m_disableTweenAnimations;

	[SerializeField]
	[ConditionalHide("m_disableTweenAnimations", true)]
	protected float m_animationClipDuration = 1f;

	[SerializeField]
	private bool m_useUnscaledTime;

	protected TMP_Text m_tmpText;

	protected LocalizationItem m_locItem;

	protected TextGlyphConverter m_textGlyphConverter;

	protected int m_storedLayer;

	public float Alpha => m_tmpText.alpha;

	public bool IsFreePoolObj { get; set; }

	public bool IsAwakeCalled { get; protected set; }

	public float InitialDelay { get; set; }

	public GameObject Source { get; set; }

	public TMP_Text TMPText => m_tmpText;

	public string Text => m_tmpText.text;

	public void SetText(string text, TextAlignmentOptions alignmentOptions)
	{
		m_locItem.enabled = false;
		m_tmpText.text = text;
		m_tmpText.alignment = alignmentOptions;
	}

	public void SetLocIDText(string locID, StringGenderType genderType)
	{
		m_locItem.enabled = true;
		m_locItem.StringGenderType = genderType;
		m_locItem.SetString(locID);
	}

	private void Awake()
	{
		m_tmpText = GetComponentInChildren<TMP_Text>();
		m_locItem = m_tmpText.GetComponent<LocalizationItem>();
		m_textGlyphConverter = m_tmpText.GetComponent<TextGlyphConverter>();
		m_storedLayer = base.gameObject.layer;
		ResetValues();
		IsAwakeCalled = true;
	}

	public void Spawn()
	{
		StartCoroutine(SpawnCoroutine());
	}

	private IEnumerator SpawnCoroutine()
	{
		if (InitialDelay > 0f)
		{
			Vector2 posOffset = Vector2.zero;
			if ((bool)Source)
			{
				posOffset = base.transform.localPosition - Source.transform.position;
			}
			m_tmpText.alpha = 0f;
			if (!m_useUnscaledTime)
			{
				float startTime3 = Time.time;
				while (Time.time < startTime3 + InitialDelay)
				{
					yield return null;
				}
			}
			else
			{
				float startTime3 = Time.unscaledTime;
				while (Time.unscaledTime < startTime3 + InitialDelay)
				{
					yield return null;
				}
			}
			m_tmpText.alpha = 1f;
			if ((bool)Source && base.transform.parent != Source.transform)
			{
				posOffset += (Vector2)Source.transform.position;
				base.transform.localPosition = new Vector3(posOffset.x, posOffset.y, base.transform.localPosition.z);
			}
		}
		if (!m_disableTweenAnimations)
		{
			yield return SpawnEffectCoroutine();
		}
		else
		{
			if (!(m_animationClipDuration >= 0f))
			{
				yield break;
			}
			if (!m_useUnscaledTime)
			{
				float startTime3 = m_animationClipDuration + Time.time;
				while (Time.time < startTime3)
				{
					yield return null;
				}
			}
			else
			{
				float startTime3 = m_animationClipDuration + Time.unscaledTime;
				while (Time.unscaledTime < startTime3)
				{
					yield return null;
				}
			}
			base.gameObject.SetActive(value: false);
		}
	}

	protected abstract IEnumerator SpawnEffectCoroutine();

	protected virtual void OnDisable()
	{
		DisablePooledObjectManager.DisablePooledObject(this);
	}

	public virtual void ResetValues()
	{
		StopAllCoroutines();
		base.gameObject.transform.localPosition = Vector3.zero;
		base.gameObject.transform.localScale = new Vector3(1f, 1f, 1f);
		base.gameObject.transform.localEulerAngles = Vector3.zero;
		m_tmpText.alpha = 1f;
		m_locItem.enabled = true;
		if (base.gameObject.layer != m_storedLayer)
		{
			base.gameObject.SetLayerRecursively(m_storedLayer);
		}
		InitialDelay = 0f;
		Source = null;
	}

	[SpecialName]
	GameObject IGenericPoolObj.get_gameObject()
	{
		return base.gameObject;
	}
}
