using System;
using UnityEngine;

public class FireProjectileOnTrigger : MonoBehaviour, IHasProjectileNameArray
{
	[SerializeField]
	private string m_projectileToSpawn;

	[NonSerialized]
	protected string[] m_projectileNameArray;

	public virtual string[] ProjectileNameArray
	{
		get
		{
			if (m_projectileNameArray == null)
			{
				m_projectileNameArray = new string[1] { m_projectileToSpawn };
			}
			return m_projectileNameArray;
		}
	}

	public string ProjectileToSpawn
	{
		get
		{
			return m_projectileToSpawn;
		}
		set
		{
			m_projectileToSpawn = value;
		}
	}

	public void FireProjectile()
	{
		ProjectileManager.FireProjectile(base.gameObject, m_projectileToSpawn, Vector2.zero, matchFacing: false);
	}
}
