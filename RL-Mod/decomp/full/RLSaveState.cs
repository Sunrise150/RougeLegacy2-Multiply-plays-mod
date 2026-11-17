using System;
using UnityEngine;

[Serializable]
public class RLSaveState
{
	[SerializeField]
	private bool m_isStateActive = true;

	[SerializeField]
	private bool m_isSpawned;

	public bool IsSpawned
	{
		get
		{
			return m_isSpawned;
		}
		set
		{
			m_isSpawned = value;
		}
	}

	public bool IsStateActive
	{
		get
		{
			return m_isStateActive;
		}
		set
		{
			m_isStateActive = value;
		}
	}
}
