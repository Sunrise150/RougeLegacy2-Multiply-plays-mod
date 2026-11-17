using System.Collections;
using RL_Windows;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

public class IsSkillTreeCastleSectionBuiltEventController : MonoBehaviour
{
	[SerializeField]
	private string m_parameter;

	[SerializeField]
	[FormerlySerializedAs("m_windowOpenedEvent")]
	private UnityEvent m_sectionBuiltEvent;

	[SerializeField]
	private UnityEvent m_windowClosedEvent;

	private WaitUntil m_waitForSkillTreeToOpen;

	private WaitUntil m_waitUntilParameterIsSet;

	private bool m_isBuilt;

	private Coroutine m_checkCoroutine;

	private SkillTreeWindowController m_skillTreeWindow;

	private UnityAction m_onWindowOpened;

	private UnityAction m_onWindowClosed;

	private void Awake()
	{
		m_onWindowOpened = OnWindowOpened;
		m_onWindowClosed = OnWindowClosed;
	}

	private void Start()
	{
		m_skillTreeWindow = WindowManager.GetWindowController(WindowID.SkillTree) as SkillTreeWindowController;
		m_skillTreeWindow.WindowOpenedEvent.RemoveListener(m_onWindowOpened);
		m_skillTreeWindow.WindowOpenedEvent.AddListener(m_onWindowOpened);
		m_skillTreeWindow.WindowClosedEvent.RemoveListener(m_onWindowClosed);
		m_skillTreeWindow.WindowClosedEvent.AddListener(m_onWindowClosed);
		m_checkCoroutine = StartCoroutine(Check());
	}

	private IEnumerator Check()
	{
		if (m_waitForSkillTreeToOpen == null)
		{
			m_waitForSkillTreeToOpen = new WaitUntil(() => WindowManager.GetIsWindowOpen(WindowID.SkillTree));
			m_waitUntilParameterIsSet = new WaitUntil(() => m_skillTreeWindow.CastleAnimator.GetBool(m_parameter));
		}
		yield return m_waitForSkillTreeToOpen;
		if (!m_isBuilt)
		{
			yield return m_waitUntilParameterIsSet;
			m_isBuilt = true;
		}
		OnSectionBuilt();
		m_checkCoroutine = null;
	}

	private void OnWindowOpened()
	{
		if (m_checkCoroutine == null)
		{
			m_checkCoroutine = StartCoroutine(Check());
		}
	}

	private void OnSectionBuilt()
	{
		if (m_sectionBuiltEvent != null)
		{
			m_sectionBuiltEvent.Invoke();
		}
	}

	private void OnWindowClosed()
	{
		StopAllCoroutines();
		m_checkCoroutine = null;
		if (m_isBuilt && m_windowClosedEvent != null)
		{
			m_windowClosedEvent.Invoke();
		}
	}
}
