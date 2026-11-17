using UnityEngine;

namespace RLAudio;

public class OnFacingChangeEventEmitter : AnimBehaviourEventEmitter
{
	private bool m_hasSearchedForCorgiController;

	private BaseCharacterController m_characterController;

	private bool m_previousIsFacingRightValue;

	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		base.OnStateEnter(animator, stateInfo, layerIndex);
		if (!m_hasSearchedForCorgiController)
		{
			m_hasSearchedForCorgiController = true;
			m_characterController = animator.GetComponent<BaseCharacterController>();
			if (m_characterController == null)
			{
				Debug.LogFormat("<color=red>| {0} | This behaviour requires that the Animator's GameObject has a BaseCharacterController component attached. If you see this message, please add a bug to Pivotal and attach the stack trace.</color>", this);
			}
		}
		if (m_characterController != null)
		{
			m_previousIsFacingRightValue = m_characterController.IsFacingRight;
		}
	}

	public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		if (m_characterController != null && m_characterController.IsFacingRight != m_previousIsFacingRightValue)
		{
			m_previousIsFacingRightValue = m_characterController.IsFacingRight;
			Play(animator);
		}
	}
}
