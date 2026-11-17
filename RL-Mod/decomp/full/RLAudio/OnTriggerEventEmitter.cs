using UnityEngine;

namespace RLAudio;

public class OnTriggerEventEmitter : AnimBehaviourEventEmitter
{
	[SerializeField]
	private string m_triggerName;

	private int m_triggerID = -1;

	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		base.OnStateEnter(animator, stateInfo, layerIndex);
		if (m_triggerID == -1)
		{
			m_triggerID = Animator.StringToHash(m_triggerName);
		}
	}

	public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		if (animator.GetBool(m_triggerID))
		{
			Play(animator);
		}
	}
}
