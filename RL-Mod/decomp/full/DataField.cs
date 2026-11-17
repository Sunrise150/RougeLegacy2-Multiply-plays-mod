using System.Collections.Generic;
using UnityEngine;

public class DataField<T>
{
	[SerializeField]
	private T m_currentValue;

	[SerializeField]
	private T m_defaultValue;

	public T Value
	{
		get
		{
			return m_currentValue;
		}
		set
		{
			m_currentValue = value;
		}
	}

	public T DefaultValue => m_defaultValue;

	public bool IsDefault => EqualityComparer<T>.Default.Equals(Value, m_defaultValue);

	public void SetDefaultValue(T defaultValue)
	{
		m_currentValue = defaultValue;
		m_defaultValue = defaultValue;
	}

	public void Reset()
	{
		Value = m_defaultValue;
	}
}
