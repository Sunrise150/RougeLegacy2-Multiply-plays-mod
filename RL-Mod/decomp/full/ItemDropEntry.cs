using System;
using UnityEngine;

[Serializable]
public class ItemDropEntry
{
	[SerializeField]
	private int m_itemPoolSize = 5;

	[SerializeField]
	private BaseItemDrop m_itemDropPrefab;

	public int ItemDropPoolSize
	{
		get
		{
			return m_itemPoolSize;
		}
		set
		{
			m_itemPoolSize = value;
		}
	}

	public BaseItemDrop ItemDropPrefab
	{
		get
		{
			return m_itemDropPrefab;
		}
		set
		{
			m_itemDropPrefab = value;
		}
	}
}
