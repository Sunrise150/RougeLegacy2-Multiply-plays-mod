using System.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace SceneManagement_RL;

public class LoadingGate_SceneTransition : Transition_V2, ISceneLoadingTransition, ITransition
{
	[SerializeField]
	private RectTransform m_gate;

	[SerializeField]
	private RectTransform m_archway;

	[SerializeField]
	private CanvasGroup m_blackness;

	[SerializeField]
	private RectTransform m_loadingGear;

	[SerializeField]
	private GameObject m_loadingGatePanel;

	[SerializeField]
	private UnityEvent m_closeStartEvent;

	[SerializeField]
	private UnityEvent m_closeEndEvent;

	[SerializeField]
	private UnityEvent m_openStartEvent;

	[SerializeField]
	private UnityEvent m_openEndEvent;

	private float m_gateStartingY;

	private float m_archwayStartingScale;

	private Animator m_animator;

	public override TransitionID ID => TransitionID.CastleGate;

	public bool DropPlayer { get; private set; }

	protected override void Awake()
	{
		base.Awake();
		m_gateStartingY = m_gate.anchoredPosition.y;
		m_archwayStartingScale = m_archway.localScale.x;
		m_animator = GetComponent<Animator>();
	}

	public IEnumerator TransitionIn()
	{
		DropPlayer = false;
		if (m_closeStartEvent != null)
		{
			m_closeStartEvent.Invoke();
		}
		m_animator.SetBool("Closed", value: true);
		float delayTime = Time.unscaledTime + 0.5f;
		while (Time.unscaledTime < delayTime)
		{
			yield return null;
		}
		delayTime = Time.unscaledTime + 0.325f;
		while (Time.unscaledTime < delayTime)
		{
			yield return null;
		}
		RumbleManager.StartRumble(useLeftMotor: true, useRightMotor: true, 0.8f, 0.2f);
		while (m_animator.GetCurrentAnimatorStateInfo(0).normalizedTime < 0.9f)
		{
			yield return null;
		}
		if (m_closeEndEvent != null)
		{
			m_closeEndEvent.Invoke();
		}
	}

	public IEnumerator TransitionOut()
	{
		if (m_openStartEvent != null)
		{
			m_openStartEvent.Invoke();
		}
		float delayTime = Time.unscaledTime + 0.5f;
		while (Time.unscaledTime < delayTime)
		{
			yield return null;
		}
		RumbleManager.StartRumble(useLeftMotor: true, useRightMotor: true, 0.8f, 0.2f);
		m_animator.SetBool("Closed", value: false);
		delayTime = Time.unscaledTime + 0.25f;
		while (Time.unscaledTime < delayTime)
		{
			yield return null;
		}
		DropPlayer = true;
		delayTime = Time.unscaledTime + 0.25f;
		while (Time.unscaledTime < delayTime)
		{
			yield return null;
		}
		while (m_animator.GetCurrentAnimatorStateInfo(0).normalizedTime < 1f)
		{
			yield return null;
		}
		if (m_openEndEvent != null)
		{
			m_openEndEvent.Invoke();
		}
	}

	public override IEnumerator Run()
	{
		yield break;
	}

	private void Update()
	{
		Vector3 localEulerAngles = m_loadingGear.localEulerAngles;
		localEulerAngles.z -= 30f * Time.unscaledDeltaTime;
		m_loadingGear.localEulerAngles = localEulerAngles;
	}
}
