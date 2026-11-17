using System;
using UnityEngine;

[Serializable]
public class ProjectileEntry
{
	[SerializeField]
	private Projectile_RL m_projectilePrefab;

	[SerializeField]
	private int m_poolSize = 10;

	public Projectile_RL ProjectilePrefab
	{
		get
		{
			return m_projectilePrefab;
		}
		set
		{
			m_projectilePrefab = value;
		}
	}

	public int PoolSize
	{
		get
		{
			return m_poolSize;
		}
		set
		{
			m_poolSize = value;
		}
	}
}
