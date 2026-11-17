using UnityEngine;

namespace RLAudio;

public class PlayerAttackAudioEventEmitter : OnEnterEventEmitter
{
	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		base.OnStateEnter(animator, stateInfo, layerIndex);
		float playerGenderAudioParameterValue = AudioUtility.GetPlayerGenderAudioParameterValue();
		float playerSizeAudioParameterValue = AudioUtility.GetPlayerSizeAudioParameterValue();
		if (m_eventInstance.isValid())
		{
			m_eventInstance.setParameterByName("gender", playerGenderAudioParameterValue);
			m_eventInstance.setParameterByName("Player_Size", playerSizeAudioParameterValue);
		}
	}
}
