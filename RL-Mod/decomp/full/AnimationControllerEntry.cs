using System;
using UnityEngine;

[Serializable]
public class AnimationControllerEntry
{
	[SerializeField]
	[ReadOnly]
	private int m_id;

	[SerializeField]
	private string m_animationParameter;

	[SerializeField]
	private AnimatorControllerParameterType m_parameterType = AnimatorControllerParameterType.Bool;

	[SerializeField]
	private bool m_boolValue;

	[SerializeField]
	private int m_intValue;

	[SerializeField]
	private float m_floatValue;

	public int Id
	{
		get
		{
			return m_id;
		}
		set
		{
			m_id = value;
		}
	}

	public string AnimationParameter
	{
		get
		{
			return m_animationParameter;
		}
		set
		{
			m_animationParameter = value;
		}
	}

	public bool BoolValue
	{
		get
		{
			return m_boolValue;
		}
		set
		{
			m_boolValue = value;
		}
	}

	public int IntValue
	{
		get
		{
			return m_intValue;
		}
		set
		{
			m_intValue = value;
		}
	}

	public AnimatorControllerParameterType ParameterType
	{
		get
		{
			return m_parameterType;
		}
		set
		{
			m_parameterType = value;
		}
	}

	public float FloatValue
	{
		get
		{
			return m_floatValue;
		}
		set
		{
			m_floatValue = value;
		}
	}
}
