using UnityEngine;

[SharedBetweenAnimators]
public class DisableGOAnimBehaviour : StateMachineBehaviour
{
	public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		animator.gameObject.SetActive(value: false);
	}
}
