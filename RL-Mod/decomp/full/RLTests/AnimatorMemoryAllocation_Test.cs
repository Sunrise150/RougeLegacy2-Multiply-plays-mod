using UnityEngine;

namespace RLTests;

public class AnimatorMemoryAllocation_Test : MonoBehaviour
{
	[SerializeField]
	private string m_parameterName;

	[SerializeField]
	private AnimatorParameterType m_parameterType;

	private Animator m_animator;

	private void Awake()
	{
		m_animator = GetComponent<Animator>();
	}

	private void Update()
	{
		if (Input.GetKeyDown(KeyCode.DownArrow))
		{
			GetValue();
		}
	}

	private void GetValue()
	{
		_ = m_animator.name;
		_ = m_animator.tag;
		if (m_animator.parameters.Length != 0)
		{
			_ = m_animator.parameters[0].name;
		}
		switch (m_parameterType)
		{
		case AnimatorParameterType.Float:
			m_animator.GetFloat(m_parameterName);
			break;
		case AnimatorParameterType.Bool:
			m_animator.GetBool(m_parameterName);
			break;
		case AnimatorParameterType.Int:
			m_animator.GetInteger(m_parameterName);
			break;
		}
	}
}
