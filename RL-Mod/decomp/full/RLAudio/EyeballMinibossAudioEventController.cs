using FMODUnity;
using UnityEngine;

namespace RLAudio;

public class EyeballMinibossAudioEventController : MonoBehaviour, IAudioEventEmitter
{
	[SerializeField]
	[Tooltip("This is the max move speed per frame. This is used to determine a normalised 'how fast is the Eye moving?' value which is then used as a parameter in its movement audio.")]
	private float m_maxSpeed = 1f;

	[SerializeField]
	private StudioEventEmitter m_movementEventEmitter;

	[SerializeField]
	private StudioEventEmitter m_movementStopEventEmitter;

	private string m_description;

	private EnemyController m_enemyController;

	private Vector2 m_previousHeading;

	private Vector2 m_currentHeading;

	private const string MOVE_SPEED_PARAMETER = "moveSpeed";

	public string Description
	{
		get
		{
			if (string.IsNullOrEmpty(m_description))
			{
				m_description = ToString();
			}
			return m_description;
		}
	}

	private void Start()
	{
		m_enemyController = GetComponent<EnemyController>();
	}

	private void Update()
	{
		if (!m_enemyController)
		{
			return;
		}
		m_currentHeading.x = m_enemyController.HeadingX;
		m_currentHeading.y = m_enemyController.HeadingY;
		if (m_previousHeading != m_currentHeading)
		{
			if (!m_movementEventEmitter.IsPlaying())
			{
				m_movementEventEmitter.Play();
			}
			float value = Mathf.Clamp((m_currentHeading - m_previousHeading).magnitude, 0f, m_maxSpeed) / m_maxSpeed;
			m_movementEventEmitter.SetParameter("moveSpeed", value);
			m_previousHeading = m_currentHeading;
		}
		else if (m_movementEventEmitter.IsPlaying())
		{
			m_movementEventEmitter.Stop();
			m_movementStopEventEmitter.Play();
		}
	}
}
