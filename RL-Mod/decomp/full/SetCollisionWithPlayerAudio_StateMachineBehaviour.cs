using FMODUnity;
using RLAudio;
using UnityEngine;

public class SetCollisionWithPlayerAudio_StateMachineBehaviour : StateMachineBehaviour
{
	[SerializeField]
	[EventRef]
	private string m_audioPath = string.Empty;

	[SerializeField]
	private bool m_clearOnExit = true;

	private CollisionWithPlayerAudioController m_collisionWithPlayerAudioController;

	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		base.OnStateEnter(animator, stateInfo, layerIndex);
		if (m_collisionWithPlayerAudioController == null)
		{
			m_collisionWithPlayerAudioController = animator.gameObject.GetComponent<CollisionWithPlayerAudioController>();
			if (m_collisionWithPlayerAudioController == null)
			{
				throw new MissingComponentException($"| {this} | {animator.gameObject.name} is missing a ICharacterCollisionAudioOverrideController Component.");
			}
		}
		m_collisionWithPlayerAudioController.SetCollisionWithPlayerAudioOverride(m_audioPath);
	}

	public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		base.OnStateExit(animator, stateInfo, layerIndex);
		if (m_clearOnExit)
		{
			m_collisionWithPlayerAudioController.SetCollisionWithPlayerAudioOverride(string.Empty);
		}
	}
}
