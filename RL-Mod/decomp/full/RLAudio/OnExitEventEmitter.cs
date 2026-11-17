using UnityEngine;

namespace RLAudio;

public class OnExitEventEmitter : AnimBehaviourEventEmitter
{
	public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		Play(animator);
	}
}
