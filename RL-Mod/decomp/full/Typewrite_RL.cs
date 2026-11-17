using System.Collections;
using Sigtrap.Relays;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(TMP_Text))]
public class Typewrite_RL : MonoBehaviour
{
	[SerializeField]
	private float m_typewriteCharDelay = 0.015f;

	[SerializeField]
	private float m_typewriteCharLongDelay = 0.1f;

	[SerializeField]
	private bool m_typewriteOnStart = true;

	[SerializeField]
	private bool m_useUnscaledTime;

	private TMP_Text m_tmpText;

	private WaitRL_Yield m_typewriteWaitYield;

	private WaitRL_Yield m_typewriteLongWaitYield;

	private Coroutine m_typewriteCoroutine;

	private Relay m_onTypewriteCompleteRelay = new Relay();

	private Relay m_onTypewriteLongDelayRelay = new Relay();

	private Relay m_onTypewriteShortDelayRelay = new Relay();

	public IRelayLink OnTypewriteCompleteRelay => m_onTypewriteCompleteRelay.link;

	public IRelayLink OnTypewriteLongDelayRelay => m_onTypewriteLongDelayRelay.link;

	public IRelayLink OnTypewriteShortDelayRelay => m_onTypewriteShortDelayRelay.link;

	public bool UseUnscaledTime
	{
		get
		{
			return m_useUnscaledTime;
		}
		set
		{
			m_useUnscaledTime = value;
		}
	}

	public bool TypewriteOnStart
	{
		get
		{
			return m_typewriteOnStart;
		}
		set
		{
			m_typewriteOnStart = value;
		}
	}

	public bool IsPaused { get; private set; }

	public bool IsTypewriting { get; private set; }

	public float TypewriteCharDelay
	{
		get
		{
			return m_typewriteCharDelay;
		}
		set
		{
			m_typewriteCharDelay = value;
		}
	}

	public float TypewriteCharLongDelay
	{
		get
		{
			return m_typewriteCharLongDelay;
		}
		set
		{
			m_typewriteCharLongDelay = value;
		}
	}

	private void Awake()
	{
		m_tmpText = GetComponent<TMP_Text>();
		m_typewriteWaitYield = new WaitRL_Yield(TypewriteCharDelay, UseUnscaledTime);
		m_typewriteLongWaitYield = new WaitRL_Yield(TypewriteCharLongDelay, UseUnscaledTime);
	}

	private void Start()
	{
		if (TypewriteOnStart)
		{
			StartTypewriting();
		}
	}

	public void StartTypewriting()
	{
		m_typewriteCoroutine = StartCoroutine(TypewriterCoroutine());
	}

	private IEnumerator TypewriterCoroutine()
	{
		IsTypewriting = true;
		m_tmpText.ForceMeshUpdate();
		int charCount = m_tmpText.textInfo.characterCount;
		int counter = 0;
		m_tmpText.maxVisibleCharacters = 0;
		yield return m_typewriteWaitYield;
		while (counter < charCount)
		{
			while (IsPaused)
			{
				yield return null;
			}
			counter++;
			if (counter < charCount)
			{
				char character = m_tmpText.textInfo.characterInfo[counter - 1].character;
				m_tmpText.maxVisibleCharacters = counter;
				if (character == ',' || character == '.' || character == '!')
				{
					m_onTypewriteLongDelayRelay.Dispatch();
					m_typewriteLongWaitYield.CreateNew(TypewriteCharLongDelay, UseUnscaledTime);
					yield return m_typewriteLongWaitYield;
				}
				else
				{
					m_onTypewriteShortDelayRelay.Dispatch();
					m_typewriteWaitYield.CreateNew(TypewriteCharDelay, UseUnscaledTime);
					yield return m_typewriteWaitYield;
				}
			}
		}
		yield return null;
		StopTypewriting();
	}

	public void StopTypewriting()
	{
		if (m_typewriteCoroutine != null)
		{
			StopCoroutine(m_typewriteCoroutine);
		}
		IsTypewriting = false;
		m_tmpText.maxVisibleCharacters = m_tmpText.text.Length;
		m_onTypewriteCompleteRelay.Dispatch();
	}

	public void PauseTypewriting()
	{
		IsPaused = true;
	}

	public void ResumeTypewriting()
	{
		IsPaused = false;
	}
}
