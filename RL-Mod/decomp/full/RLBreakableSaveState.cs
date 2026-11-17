using System;
using UnityEngine;

[Serializable]
public class RLBreakableSaveState : RLSaveState
{
	[SerializeField]
	private bool m_attackedOnRight;

	public bool AttackerIsOnRight
	{
		get
		{
			return m_attackedOnRight;
		}
		set
		{
			m_attackedOnRight = value;
		}
	}
}
