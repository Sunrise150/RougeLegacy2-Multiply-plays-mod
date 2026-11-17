using UnityEngine;

public class ChangeAnimUpdateMode : MonoBehaviour
{
	private enum UpdateModeType
	{
		ScaledTime,
		UnscaledTime
	}

	[SerializeField]
	private UpdateModeType m_updateMode;

	[SerializeField]
	private bool m_changeAllAnimators;

	[SerializeField]
	private bool m_changeAllParticleSystems;

	private Animator[] m_animators;

	private ParticleSystem[] m_particleSystems;

	private AnimatorUpdateMode[] m_storedAnimatorModes;

	private bool[] m_storedPartSysModes;

	private void Awake()
	{
		m_animators = GetComponentsInChildren<Animator>(includeInactive: true);
		m_particleSystems = GetComponentsInChildren<ParticleSystem>(includeInactive: true);
		m_storedAnimatorModes = new AnimatorUpdateMode[m_animators.Length];
		m_storedPartSysModes = new bool[m_particleSystems.Length];
		for (int i = 0; i < m_animators.Length; i++)
		{
			m_storedAnimatorModes[i] = m_animators[i].updateMode;
		}
		for (int j = 0; j < m_particleSystems.Length; j++)
		{
			m_storedPartSysModes[j] = m_particleSystems[j].main.useUnscaledTime;
		}
	}

	private void OnEnable()
	{
		AnimatorUpdateMode updateMode = AnimatorUpdateMode.Normal;
		bool useUnscaledTime = m_updateMode == UpdateModeType.UnscaledTime;
		if (m_updateMode == UpdateModeType.UnscaledTime)
		{
			updateMode = AnimatorUpdateMode.UnscaledTime;
		}
		Animator[] animators = m_animators;
		for (int i = 0; i < animators.Length; i++)
		{
			animators[i].updateMode = updateMode;
		}
		ParticleSystem[] particleSystems = m_particleSystems;
		for (int i = 0; i < particleSystems.Length; i++)
		{
			ParticleSystem.MainModule main = particleSystems[i].main;
			main.useUnscaledTime = useUnscaledTime;
		}
	}

	private void OnDisable()
	{
		for (int i = 0; i < m_animators.Length; i++)
		{
			m_animators[i].updateMode = m_storedAnimatorModes[i];
		}
		for (int j = 0; j < m_particleSystems.Length; j++)
		{
			ParticleSystem.MainModule main = m_particleSystems[j].main;
			main.useUnscaledTime = m_storedPartSysModes[j];
		}
	}
}
