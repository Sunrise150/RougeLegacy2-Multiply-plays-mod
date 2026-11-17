using System.Collections;
using UnityEngine;

public class EnemyAppliesInvulnStatusEffect : BaseStatusEffect
{
	private static string[] m_projectileNameArray = new string[1] { "StatusEffectTeamInvincibilityProjectile" };

	private Projectile_RL m_invulnRadiusProjectile;

	public override string[] ProjectileNameArray => m_projectileNameArray;

	public override StatusEffectType StatusEffectType => StatusEffectType.Enemy_AppliesInvuln;

	public override float StartingDurationOverride => float.MaxValue;

	public override void StartEffect(float duration, IDamageObj caster)
	{
		base.StartEffect(duration, caster);
		m_charController.StatusEffectController.StopStatusEffect(StatusEffectType.Enemy_Invuln, interrupted: false);
	}

	protected override IEnumerator StartEffectCoroutine(IDamageObj caster, bool justCasted)
	{
		m_charController.StatusBarController.ApplyUIEffect(StatusBarEntryType.AppliesInvuln);
		EnemyController enemyController = m_charController as EnemyController;
		BaseRoom currentRoom = PlayerManager.GetCurrentPlayerRoom();
		if (!base.IsHidden)
		{
			CreateInvulnProjectileEffect();
		}
		EnemySpawnController[] enemySpawnControllers;
		while (Time.time < base.EndTime)
		{
			enemySpawnControllers = currentRoom.SpawnControllerManager.EnemySpawnControllers;
			foreach (EnemySpawnController enemySpawnController in enemySpawnControllers)
			{
				if (!enemySpawnController.IsDead && enemySpawnController.ShouldSpawn)
				{
					EnemyController enemyInstance = enemySpawnController.EnemyInstance;
					ApplyInvulnToEnemy(enemyInstance, enemyController);
				}
			}
			foreach (EnemyController summonedEnemy in EnemyManager.SummonedEnemyList)
			{
				ApplyInvulnToEnemy(summonedEnemy, enemyController);
			}
			yield return null;
		}
		enemySpawnControllers = currentRoom.SpawnControllerManager.EnemySpawnControllers;
		for (int i = 0; i < enemySpawnControllers.Length; i++)
		{
			EnemyController enemyInstance2 = enemySpawnControllers[i].EnemyInstance;
			if ((bool)enemyInstance2)
			{
				enemyInstance2.StatusEffectController.StopStatusEffect(StatusEffectType.Enemy_Invuln, interrupted: false);
			}
		}
		foreach (EnemyController summonedEnemy2 in EnemyManager.SummonedEnemyList)
		{
			summonedEnemy2.StatusEffectController.StopStatusEffect(StatusEffectType.Enemy_Invuln, interrupted: false);
		}
		StopEffect();
	}

	private void CreateInvulnProjectileEffect()
	{
		if ((bool)m_invulnRadiusProjectile && !m_invulnRadiusProjectile.IsFreePoolObj && m_invulnRadiusProjectile.OwnerController == m_charController)
		{
			m_invulnRadiusProjectile.FlagForDestruction();
		}
		m_invulnRadiusProjectile = ProjectileManager.FireProjectile(m_charController.gameObject, "StatusEffectTeamInvincibilityProjectile", m_charController.Midpoint - m_charController.transform.localPosition, matchFacing: false);
		float num = m_invulnRadiusProjectile.transform.localScale.x * 25f * 2f / m_invulnRadiusProjectile.transform.lossyScale.x;
		m_invulnRadiusProjectile.transform.localScale = new Vector3(num, num, m_invulnRadiusProjectile.transform.localScale.z);
	}

	private void ApplyInvulnToEnemy(EnemyController enemy, EnemyController sourceEnemyController)
	{
		if ((bool)enemy && !enemy.IsDead && !enemy.StatusEffectController.HasStatusEffect(StatusEffectType.Enemy_AppliesInvuln) && CDGHelper.DistanceBetweenPts(m_charController.Midpoint, enemy.Midpoint) <= 25f)
		{
			enemy.StatusEffectController.StartStatusEffect(StatusEffectType.Enemy_Invuln, 0f, sourceEnemyController);
		}
	}

	public override void StopEffect(bool interrupted = false)
	{
		base.StopEffect(interrupted);
		if ((bool)m_invulnRadiusProjectile && !m_invulnRadiusProjectile.IsFreePoolObj && m_invulnRadiusProjectile.OwnerController == m_charController)
		{
			m_invulnRadiusProjectile.FlagForDestruction();
		}
		m_invulnRadiusProjectile = null;
		if (!PlayerManager.IsInstantiated)
		{
			return;
		}
		BaseRoom currentPlayerRoom = PlayerManager.GetCurrentPlayerRoom();
		if (!currentPlayerRoom)
		{
			return;
		}
		EnemySpawnController[] enemySpawnControllers = currentPlayerRoom.SpawnControllerManager.EnemySpawnControllers;
		for (int i = 0; i < enemySpawnControllers.Length; i++)
		{
			EnemyController enemyInstance = enemySpawnControllers[i].EnemyInstance;
			if ((bool)enemyInstance)
			{
				enemyInstance.StatusEffectController.StopStatusEffect(StatusEffectType.Enemy_Invuln, interrupted: false);
			}
		}
		foreach (EnemyController summonedEnemy in EnemyManager.SummonedEnemyList)
		{
			summonedEnemy.StatusEffectController.StopStatusEffect(StatusEffectType.Enemy_Invuln, interrupted: false);
		}
	}

	public override void SetIsHidden(bool hide)
	{
		base.SetIsHidden(hide);
		if (hide)
		{
			if ((bool)m_invulnRadiusProjectile && !m_invulnRadiusProjectile.IsFreePoolObj && m_invulnRadiusProjectile.OwnerController == m_charController)
			{
				m_invulnRadiusProjectile.gameObject.SetActive(value: false);
			}
		}
		else
		{
			CreateInvulnProjectileEffect();
		}
	}
}
