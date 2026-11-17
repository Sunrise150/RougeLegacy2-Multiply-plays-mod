using UnityEngine;

public class SetAnimatorParamOnEnable : MonoBehaviour
{
	[SerializeField]
	private Animator m_animator;

	[SerializeField]
	private AnimatorControllerParameterType m_paramType;

	[SerializeField]
	private string m_paramName;

	[SerializeField]
	private float m_numberValue;

	private void OnEnable()
	{
		if ((bool)m_animator)
		{
			switch (m_paramType)
			{
			case AnimatorControllerParameterType.Bool:
				m_animator.SetBool(m_paramName, value: true);
				break;
			case AnimatorControllerParameterType.Trigger:
				m_animator.SetTrigger(m_paramName);
				break;
			case AnimatorControllerParameterType.Int:
				m_animator.SetInteger(m_paramName, (int)m_numberValue);
				break;
			case AnimatorControllerParameterType.Float:
				m_animator.SetFloat(m_paramName, m_numberValue);
				break;
			}
		}
	}
}
