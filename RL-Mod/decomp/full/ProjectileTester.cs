using System.Collections;
using UnityEngine;

public class ProjectileTester : MonoBehaviour
{
	[SerializeField]
	private string m_projectilePrefabName;

	[SerializeField]
	private float m_initialDelay = 1f;

	[SerializeField]
	private float m_fireInterval = 0.25f;

	private void OnEnable()
	{
		StartCoroutine(FireCoroutine());
	}

	private IEnumerator FireCoroutine()
	{
		float delay3 = m_initialDelay + Time.time;
		while (Time.time < delay3)
		{
			yield return null;
		}
		Vector2 spawnOffset = new Vector2(1f, 1f);
		if (string.IsNullOrWhiteSpace(m_projectilePrefabName))
		{
			ProjectileEntry[] projectileEntryArray = ProjectileLibrary.ProjectileEntryArray;
			for (int i = 0; i < projectileEntryArray.Length; i++)
			{
				string text = projectileEntryArray[i].ProjectilePrefab.name;
				Debug.Log("Firing projectile: " + text);
				ProjectileManager.Instance.AddProjectileToPool(text);
				ProjectileManager.FireProjectile(base.gameObject, text, spawnOffset, matchFacing: false);
				delay3 = m_fireInterval + Time.time;
				while (Time.time < delay3)
				{
					yield return null;
				}
			}
			yield break;
		}
		bool flag = false;
		ProjectileEntry[] projectileEntryArray2 = ProjectileLibrary.ProjectileEntryArray;
		for (int j = 0; j < projectileEntryArray2.Length; j++)
		{
			if (projectileEntryArray2[j].ProjectilePrefab.name == m_projectilePrefabName)
			{
				flag = true;
				break;
			}
		}
		if (flag)
		{
			while (true)
			{
				string projectilePrefabName = m_projectilePrefabName;
				ProjectileManager.Instance.AddProjectileToPool(projectilePrefabName);
				ProjectileManager.FireProjectile(base.gameObject, projectilePrefabName, spawnOffset, matchFacing: false);
				delay3 = m_fireInterval + Time.time;
				while (Time.time < delay3)
				{
					yield return null;
				}
			}
		}
		Debug.Log("<color=red>Could not fire projectile: " + m_projectilePrefabName + ". Projectile not found in ProjectileLibrary.</color>");
	}
}
