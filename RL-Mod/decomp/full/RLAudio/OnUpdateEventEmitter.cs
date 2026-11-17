using UnityEngine;

namespace RLAudio;

public class OnUpdateEventEmitter : AnimBehaviourEventEmitter
{
	[SerializeField]
	[Range(0f, 1f)]
	private float[] m_playTimes;

	[SerializeField]
	private bool m_skipFirstPlayTimeOnEnter;

	private bool m_skippedFirstPlayTime;

	private int m_nextPlayTimeIndex = -1;

	private float m_previousNormalizedTime = -1f;

	private bool m_hasWarningBeenLogged;

	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		base.OnStateEnter(animator, stateInfo, layerIndex);
		if (GetShouldPlay(animator))
		{
			if ((m_playTimes == null || m_playTimes.Length == 0) && !m_hasWarningBeenLogged)
			{
				m_hasWarningBeenLogged = true;
				Debug.LogFormat("<color=red>| {0} | No play times have been set. Either remove this State Behaviour or set some play times.</color>", base.Description);
			}
			else
			{
				Reset();
			}
		}
	}

	public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		if (!GetShouldPlay(animator) || m_playTimes == null || m_playTimes.Length == 0)
		{
			return;
		}
		float num = stateInfo.normalizedTime % 1f;
		if (num < m_previousNormalizedTime)
		{
			Reset();
		}
		if (m_nextPlayTimeIndex != -1 && num >= m_playTimes[m_nextPlayTimeIndex])
		{
			if (!m_skipFirstPlayTimeOnEnter || (m_skipFirstPlayTimeOnEnter && m_skippedFirstPlayTime))
			{
				Play(animator);
			}
			else
			{
				m_skippedFirstPlayTime = true;
			}
			if (m_nextPlayTimeIndex < m_playTimes.Length - 1)
			{
				m_nextPlayTimeIndex++;
			}
			else
			{
				m_nextPlayTimeIndex = -1;
			}
		}
		m_previousNormalizedTime = num;
	}

	private void Reset()
	{
		m_nextPlayTimeIndex = 0;
		m_previousNormalizedTime = -1f;
	}
}
