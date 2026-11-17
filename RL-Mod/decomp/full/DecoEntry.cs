using System;
using UnityEngine;

[Serializable]
public class DecoEntry
{
	[SerializeField]
	private Deco m_deco;

	[SerializeField]
	private bool m_canFlip = true;

	public bool CanFlip
	{
		get
		{
			return m_canFlip;
		}
		set
		{
			m_canFlip = value;
		}
	}

	public Deco Deco
	{
		get
		{
			return m_deco;
		}
		set
		{
			m_deco = value;
		}
	}
}
