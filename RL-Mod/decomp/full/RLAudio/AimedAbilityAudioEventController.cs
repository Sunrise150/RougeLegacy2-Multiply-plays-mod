using FMOD.Studio;
using FMODUnity;
using UnityEngine;
using UnityEngine.Serialization;

namespace RLAudio;

public class AimedAbilityAudioEventController : MonoBehaviour, IAudioEventEmitter
{
	[SerializeField]
	[EventRef]
	[FormerlySerializedAs("m_bowDrawEventPath")]
	protected string m_abilityBeginEventPath;

	[SerializeField]
	[EventRef]
	[FormerlySerializedAs("m_bowAimEventPath")]
	protected string m_aimEventPath;

	[SerializeField]
	[EventRef]
	protected string m_switchSidesEventPath;

	[SerializeField]
	[EventRef]
	[FormerlySerializedAs("m_bowFiredEventPath")]
	protected string m_abilityFiredEventPath;

	[SerializeField]
	[EventRef]
	protected string m_kickbackEventPath;

	protected const string AIM_SPEED_PARAMETER_NAME = "aim_speed";

	protected const float MIN_AIM_SPEED_DELTA = 0.1f;

	protected AimedAbility_RL m_aimedAbility;

	protected bool m_isAbilityStarted;

	protected PARAMETER_ID m_aimSpeedParameterID;

	protected float m_prevAimSpeedChange;

	protected EventInstance m_aimEventInstance;

	protected EventInstance m_beginAbilityEventInstance;

	protected EventInstance m_switchSideEventInstance;

	protected EventInstance m_abilityFiredEventInstance;

	protected EventInstance m_kickbackEventInstance;

	public string Description => "Aimed Ability Audio Event Emitter Controller";

	protected virtual void Start()
	{
		m_aimedAbility = GetComponent<AimedAbility_RL>();
		if ((bool)m_aimedAbility)
		{
			m_aimedAbility.AimSpeedChangeRelay.AddListener(OnAimSpeedChange);
			m_aimedAbility.BeginCastingRelay.AddListener(OnBeginAbility);
			m_aimedAbility.FireProjectileRelay.AddListener(OnAbilityFired);
			m_aimedAbility.SwitchSidesRelay.AddListener(OnSwitchSides);
			m_aimedAbility.KickbackRelay.AddListener(OnKickback);
			m_beginAbilityEventInstance = AudioUtility.GetEventInstance(m_abilityBeginEventPath, base.transform);
			m_aimEventInstance = AudioUtility.GetEventInstance(m_aimEventPath, base.transform);
			m_switchSideEventInstance = AudioUtility.GetEventInstance(m_switchSidesEventPath, base.transform);
			m_abilityFiredEventInstance = AudioUtility.GetEventInstance(m_abilityFiredEventPath, base.transform);
			m_kickbackEventInstance = AudioUtility.GetEventInstance(m_kickbackEventPath, base.transform);
			m_aimSpeedParameterID = GetAimSpeedParameterID();
		}
		else
		{
			Debug.LogFormat("<color=red>| {0} | Missing <b>AimedAbility_RL</b> component. Please add a bug report to Pivotal and assign it to Paul</color>", this);
		}
	}

	protected void OnSwitchSides()
	{
		if (m_switchSideEventInstance.isValid())
		{
			AudioManager.PlayAttached(this, m_switchSideEventInstance, base.gameObject);
		}
	}

	protected PARAMETER_ID GetAimSpeedParameterID()
	{
		PARAMETER_ID result = default(PARAMETER_ID);
		if (!string.IsNullOrEmpty(m_aimEventPath))
		{
			return AnimBehaviourEventEmitterUtility.GetParameterID(RuntimeManager.GetEventDescription(m_aimEventPath), "aim_speed");
		}
		Debug.LogFormat("<color=red>| {0} | No Bow Drawn Event Path has been set. Please add a bug report to Pivotal and assign it to Paul</color>", this);
		return result;
	}

	protected virtual void OnAbilityFired(Projectile_RL projectile)
	{
		m_isAbilityStarted = false;
		if (m_abilityFiredEventInstance.isValid())
		{
			AudioManager.PlayAttached(this, m_abilityFiredEventInstance, base.gameObject);
		}
		AudioManager.Stop(m_aimEventInstance);
	}

	private void OnBeginAbility()
	{
		m_isAbilityStarted = true;
		if (m_beginAbilityEventInstance.isValid())
		{
			AudioManager.PlayAttached(this, m_beginAbilityEventInstance, base.gameObject);
		}
		if (m_aimEventInstance.isValid())
		{
			m_aimEventInstance.setParameterByID(m_aimSpeedParameterID, 0f);
			AudioManager.PlayAttached(this, m_aimEventInstance, base.gameObject);
		}
	}

	protected void OnAimSpeedChange(float changeAsPercentOfMax)
	{
		m_aimEventInstance.getPlaybackState(out var state);
		if (m_isAbilityStarted && m_aimEventPath != null && !m_aimSpeedParameterID.Equals(default(PARAMETER_ID)))
		{
			if (state == PLAYBACK_STATE.STOPPED || state == PLAYBACK_STATE.STOPPING)
			{
				AudioManager.PlayAttached(this, m_aimEventInstance, base.gameObject);
			}
			m_aimEventInstance.setParameterByID(m_aimSpeedParameterID, changeAsPercentOfMax);
		}
	}

	protected void OnKickback()
	{
		if (m_kickbackEventInstance.isValid())
		{
			AudioManager.PlayAttached(this, m_kickbackEventInstance, base.gameObject);
		}
	}

	private void OnDestroy()
	{
		if (m_beginAbilityEventInstance.isValid())
		{
			m_beginAbilityEventInstance.release();
		}
		if (m_aimEventInstance.isValid())
		{
			m_aimEventInstance.release();
		}
		if (m_switchSideEventInstance.isValid())
		{
			m_switchSideEventInstance.release();
		}
		if (m_abilityFiredEventInstance.isValid())
		{
			m_abilityFiredEventInstance.release();
		}
		if (m_kickbackEventInstance.isValid())
		{
			m_kickbackEventInstance.release();
		}
	}
}
