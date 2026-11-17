using FMOD.Studio;
using FMODUnity;
using UnityEngine;

namespace RLAudio;

public class PlayerFallAudioEventController : MonoBehaviour, IAudioEventEmitter
{
	[SerializeField]
	[EventRef]
	private string m_fallingEventPath;

	private CorgiController_RL m_corgiController;

	private EventInstance m_fallingEventInstance;

	private bool m_isFalling;

	private string m_description = string.Empty;

	private const string FALL_SPEED_PARAMETER = "fallSpeed";

	public string Description
	{
		get
		{
			if (m_description == string.Empty)
			{
				m_description = ToString();
			}
			return m_description;
		}
	}

	private void Awake()
	{
		m_corgiController = GetComponent<CorgiController_RL>();
		m_corgiController.CorgiStartFallingRelay.AddListener(OnPlayerStartFalling);
		m_corgiController.OnCorgiLandedEnterRelay.AddListener(OnPlayerLanded);
		m_fallingEventInstance = AudioUtility.GetEventInstance(m_fallingEventPath, base.transform);
	}

	private void OnDestroy()
	{
		if (m_fallingEventInstance.isValid())
		{
			m_fallingEventInstance.release();
		}
	}

	private void OnPlayerStartFalling(CorgiController_RL corgiController)
	{
		m_isFalling = true;
		AudioManager.PlayAttached(this, m_fallingEventInstance, base.gameObject);
	}

	private void OnPlayerLanded(CorgiController_RL corgiController)
	{
		m_isFalling = false;
		AudioManager.Stop(m_fallingEventInstance);
	}

	private void Update()
	{
		if (!(m_corgiController == null) && m_isFalling && m_fallingEventInstance.isValid())
		{
			float num = Mathf.Abs(m_corgiController.Velocity.y);
			float num2 = Mathf.Abs(Global_EV.TerminalVelocity);
			float value = num / num2;
			value = Mathf.Clamp(value, 0f, 1f);
			m_fallingEventInstance.setParameterByName("fallSpeed", value);
		}
	}
}
