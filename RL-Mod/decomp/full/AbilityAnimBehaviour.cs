using UnityEngine;

[SharedBetweenAnimators]
public class AbilityAnimBehaviour : StateMachineBehaviour
{
	[Space(10f)]
	[Tooltip("The type of state the animator is in.")]
	[SerializeField]
	private AbilityAnimState m_animatorStateType;

	private int m_isTraitorBossID = Animator.StringToHash("IsTraitorBoss");

	public AbilityAnimState AnimatorStateType => m_animatorStateType;

	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		if (!animator.GetBool(m_isTraitorBossID))
		{
			AbilityAnimBehaviourManager.CurrentAnimatorState = this;
			AbilityAnimBehaviourManager.InvokeAnimOnEnter(AnimatorStateType);
		}
	}

	public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		if (!animator.GetBool(m_isTraitorBossID) && AbilityAnimBehaviourManager.CurrentAnimatorState == this)
		{
			if (m_animatorStateType == AbilityAnimState.Exit)
			{
				AbilityAnimBehaviourManager.AnimationComplete = true;
			}
			AbilityAnimBehaviourManager.InvokeAnimOnExit(AnimatorStateType);
		}
	}
}
