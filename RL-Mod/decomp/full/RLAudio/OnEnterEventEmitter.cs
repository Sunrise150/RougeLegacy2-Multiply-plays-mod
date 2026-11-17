using UnityEngine;

namespace RLAudio;

public class OnEnterEventEmitter : AnimBehaviourEventEmitter
{
	[SerializeField]
	private bool m_stopOnExit;

	[SerializeField]
	[Range(0f, 3f)]
	private float m_timeAsPercent;

	private bool m_hasPlayed;

	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		base.OnStateEnter(animator, stateInfo, layerIndex);
		if (GetShouldPlay(animator))
		{
			m_hasPlayed = false;
			if (m_timeAsPercent == 0f)
			{
				Play(animator);
				m_hasPlayed = true;
			}
		}
	}

	public override void OnStateUpdate(Animator animator, AnimatorStateInfo animatorStateInfo, int layerIndex)
	{
		if (m_timeAsPercent != 0f && GetShouldPlay(animator))
		{
			float normalizedTime = animatorStateInfo.normalizedTime;
			if (!m_hasPlayed && normalizedTime >= m_timeAsPercent)
			{
				Play(animator);
				m_hasPlayed = true;
			}
		}
	}

	public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		if (GetShouldPlay(animator) && m_stopOnExit)
		{
			Stop();
		}
	}
}
