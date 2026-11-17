using System.Collections;
using System.Runtime.CompilerServices;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StatusBarEntry : MonoBehaviour, IGenericPoolObj
{
	[SerializeField]
	private GameObject m_barBG;

	[SerializeField]
	private Image m_barImage;

	[SerializeField]
	private TMP_Text m_counterText;

	[SerializeField]
	private Image m_icon;

	private Vector3 m_storedScale;

	private float m_pauseTime;

	public bool IsTimerPaused { get; private set; }

	public bool IsFreePoolObj { get; set; }

	public bool IsAwakeCalled { get; protected set; } = true;


	public StatusBarEntryType StatusBarType { get; private set; }

	private void Awake()
	{
		m_storedScale = base.transform.localScale;
	}

	public void ResetScale()
	{
		base.transform.localScale = m_storedScale;
	}

	public void ResetPosition()
	{
		Vector3 localPosition = base.transform.localPosition;
		localPosition.z = 1f;
		base.transform.localPosition = localPosition;
	}

	public void StartCounter(StatusBarEntryType statusBarType, int maxCount, int currentCount)
	{
		StopTimer();
		m_barBG.SetActive(value: false);
		m_counterText.gameObject.SetActive(value: true);
		StatusBarType = statusBarType;
		m_icon.sprite = IconLibrary.GetStatusEffectSprite(statusBarType);
		UpdateCounter(maxCount, currentCount);
	}

	public void UpdateCounter(int maxCount, int currentCount)
	{
		m_counterText.text = currentCount.ToString();
		if (currentCount >= maxCount)
		{
			m_counterText.color = Color.green;
		}
		else if (currentCount == 0)
		{
			m_counterText.color = Color.red;
		}
		else
		{
			m_counterText.color = Color.white;
		}
	}

	public void StartNoCounterOrTimer(StatusBarEntryType uiType)
	{
		StopTimer();
		m_barBG.SetActive(value: false);
		m_counterText.gameObject.SetActive(value: false);
		StatusBarType = uiType;
		m_icon.sprite = IconLibrary.GetStatusEffectSprite(uiType);
	}

	public void StartTimer(StatusBarEntryType uiType, float duration)
	{
		StopTimer();
		m_barBG.SetActive(value: true);
		m_counterText.gameObject.SetActive(value: false);
		StatusBarType = uiType;
		m_icon.sprite = IconLibrary.GetStatusEffectSprite(uiType);
		StartCoroutine(StartTimerCoroutine(duration));
	}

	public void StartCounterAndTimer(StatusBarEntryType statusBarType, float duration, int maxCount, int currentCount)
	{
		StopTimer();
		m_counterText.gameObject.SetActive(value: true);
		m_barBG.SetActive(value: true);
		StatusBarType = statusBarType;
		m_icon.sprite = IconLibrary.GetStatusEffectSprite(statusBarType);
		StartCoroutine(StartTimerCoroutine(duration));
		UpdateCounter(maxCount, currentCount);
	}

	private IEnumerator StartTimerCoroutine(float duration)
	{
		m_barImage.fillAmount = 1f;
		float startTime = Time.time;
		float endTime = Time.time + duration;
		while (Time.time < endTime + m_pauseTime)
		{
			if (IsTimerPaused)
			{
				m_pauseTime += Time.deltaTime;
			}
			m_barImage.fillAmount = 1f - (Time.time - (startTime + m_pauseTime)) / duration;
			yield return null;
		}
		StopTimer();
	}

	public void SetTimerPaused(bool paused)
	{
		IsTimerPaused = paused;
		m_pauseTime = 0f;
	}

	public void StopTimer()
	{
		StopAllCoroutines();
	}

	public void ResetValues()
	{
		m_barImage.fillAmount = 1f;
		IsTimerPaused = false;
		m_pauseTime = 0f;
		base.transform.localEulerAngles = Vector3.zero;
	}

	private void OnDisable()
	{
		DisablePooledObjectManager.DisablePooledObject(this);
	}

	[SpecialName]
	GameObject IGenericPoolObj.get_gameObject()
	{
		return base.gameObject;
	}
}
