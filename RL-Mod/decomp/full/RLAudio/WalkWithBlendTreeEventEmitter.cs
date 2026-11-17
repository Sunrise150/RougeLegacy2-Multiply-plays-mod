using UnityEngine;

namespace RLAudio;

public class WalkWithBlendTreeEventEmitter : OnUpdateEventEmitter
{
	[SerializeField]
	private string m_walkParameter = "FacingSpeed";

	[SerializeField]
	private Vector2 m_idleRange;

	public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		float @float = animator.GetFloat(m_walkParameter);
		if (@float < m_idleRange.x || @float > m_idleRange.y)
		{
			base.OnStateUpdate(animator, stateInfo, layerIndex);
		}
	}
}
