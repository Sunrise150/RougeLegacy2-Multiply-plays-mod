using FMOD.Studio;
using UnityEngine;

namespace RLAudio;

public class LandEventEmitter : AnimBehaviourEventEmitter
{
	private const string IMPACT_PARAMETER_NAME = "Impact";

	private bool m_isImpactParameterInitialized;

	private PARAMETER_ID m_impactParameterID;

	private bool m_hasSearchedForCorgiController;

	private CorgiController_RL m_corgiController;

	private void InitializeImpactParameter()
	{
		if (m_useSurfaceParameter)
		{
			m_impactParameterID = AnimBehaviourEventEmitterUtility.GetParameterID(m_eventDescription, "Impact");
		}
		m_isImpactParameterInitialized = true;
	}

	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		base.OnStateEnter(animator, stateInfo, layerIndex);
		if (!m_isImpactParameterInitialized)
		{
			InitializeImpactParameter();
		}
		if (!m_hasSearchedForCorgiController && m_corgiController == null)
		{
			m_hasSearchedForCorgiController = true;
			m_corgiController = animator.GetComponent<CorgiController_RL>();
			if (m_corgiController == null)
			{
				Debug.LogFormat("<color=red>| {0} | Missing CorgiController_RL component on ({1}). If you see this message, please add a bug report to Pivotal.</color>", this, animator.name);
			}
		}
		if (m_corgiController != null)
		{
			m_eventInstance.setParameterByID(m_impactParameterID, GetFallSpeedAsAPercentageOfTerminalVelocity());
			Play(animator);
		}
	}

	private float GetFallSpeedAsAPercentageOfTerminalVelocity()
	{
		float num = Mathf.Abs(m_corgiController.Velocity.y);
		float result = 1f;
		if (num < Mathf.Abs(Global_EV.TerminalVelocity))
		{
			result = num / Mathf.Abs(Global_EV.TerminalVelocity);
		}
		return result;
	}
}
