using System;
using UnityEngine;

[Serializable]
public struct AnimBehaviourCondition
{
	public string ParamName;

	public AnimatorControllerParameterType ParamType;

	public ParamBoolType BoolType;

	public int IntValue;

	public float FloatValue;

	public ParamEqualityType EqualityType;

	private bool m_isInitialized;

	private bool m_hasParameter;

	private int m_id;

	public bool IsTrue(Animator animator)
	{
		if (!m_isInitialized)
		{
			m_isInitialized = true;
			m_hasParameter = AnimatorUtility.HasParameter(animator, ParamName);
			if (m_hasParameter)
			{
				m_id = Animator.StringToHash(ParamName);
			}
		}
		if (m_hasParameter)
		{
			switch (ParamType)
			{
			case AnimatorControllerParameterType.Bool:
				if (BoolType == ParamBoolType.True)
				{
					return animator.GetBool(m_id);
				}
				return !animator.GetBool(m_id);
			case AnimatorControllerParameterType.Trigger:
				return animator.GetBool(m_id);
			case AnimatorControllerParameterType.Float:
				switch (EqualityType)
				{
				case ParamEqualityType.Greater:
					return animator.GetFloat(m_id) > FloatValue;
				case ParamEqualityType.GreaterOrEqual:
					return animator.GetFloat(m_id) >= FloatValue;
				case ParamEqualityType.Less:
					return animator.GetFloat(m_id) < FloatValue;
				case ParamEqualityType.LessOrEqual:
					return animator.GetFloat(m_id) <= FloatValue;
				case ParamEqualityType.Equals:
					return animator.GetFloat(m_id) == FloatValue;
				case ParamEqualityType.NotEqual:
					return animator.GetFloat(m_id) != FloatValue;
				}
				break;
			case AnimatorControllerParameterType.Int:
				switch (EqualityType)
				{
				case ParamEqualityType.Greater:
					return animator.GetInteger(m_id) > IntValue;
				case ParamEqualityType.GreaterOrEqual:
					return animator.GetInteger(m_id) >= IntValue;
				case ParamEqualityType.Less:
					return animator.GetInteger(m_id) < IntValue;
				case ParamEqualityType.LessOrEqual:
					return animator.GetInteger(m_id) <= IntValue;
				case ParamEqualityType.Equals:
					return animator.GetInteger(m_id) == IntValue;
				case ParamEqualityType.NotEqual:
					return animator.GetInteger(m_id) != IntValue;
				}
				break;
			}
		}
		return false;
	}
}
