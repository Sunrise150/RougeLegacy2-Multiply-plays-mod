using System;
using System.Collections;
using RLAudio;
using UnityEngine;

[CreateAssetMenu(menuName = "Custom/Biome Rules/Tower Biome Up")]
public class TowerBiomeUp_BiomeRule : BiomeRule
{
	private const string TOWER_WARNING_PROJECTILE = "TowerBiomeUpWarningProjectile";

	private const string TOWER_EXPLOSION_PROJECTILE = "TowerBiomeUpExplosionProjectile";

	private const string TOWER_LANCER_PROJECTILE = "TowerBiomeUpLancerProjectile";

	private BaseRoom m_currentRoom;

	private Projectile_RL[] m_lancerWarningProjectiles;

	private Projectile_RL[] m_lancerProjectiles;

	private Coroutine m_lancerAttackCoroutine;

	private Coroutine[] m_fireProjectileCoroutines;

	private Action<object, EventArgs> m_onPlayerEnterRoom;

	private Action<object, EventArgs> m_onPlayerExitRoom;

	private bool m_isInitialized;

	private float m_lancerAttackWarningDuration = 1.75f;

	public override BiomeRuleExecutionTime ExecutionTime => BiomeRuleExecutionTime.PlayerEnterBiome;

	private Vector2 m_lancerAttack_InitialDelay => Hazard_EV.NIGHTMARE_LANCER_INITIAL_DELAY;

	private Vector2 m_lancerAttackRandomDelay => Hazard_EV.NIGHTMARE_LANCER_RANDOM_DELAY;

	private Vector2Int m_lancerAttackRandomAmount => Hazard_EV.NIGHTMARE_LANCER_RANDOM_AMOUNT;

	private Vector2 m_lancerAttack_RandomSpawnInterval => Hazard_EV.NIGHTMARE_LANCER_RANDOM_SPAWN_INTERVAL;

	private Vector2 m_lancerAttack_RandomPosXOffset => Hazard_EV.NIGHTMARE_LANCER_RANDOM_X_OFFSET;

	private Vector2 m_lancerAttack_RandomPosYOffset => Hazard_EV.NIGHTMARE_LANCER_RANDOM_Y_OFFSET;

	private void Initialize()
	{
		m_lancerWarningProjectiles = new Projectile_RL[m_lancerAttackRandomAmount.y];
		m_lancerProjectiles = new Projectile_RL[m_lancerAttackRandomAmount.y];
		m_fireProjectileCoroutines = new Coroutine[m_lancerAttackRandomAmount.y];
		m_onPlayerEnterRoom = OnPlayerEnterRoom;
		m_onPlayerExitRoom = OnPlayerExitRoom;
		m_isInitialized = true;
	}

	public override IEnumerator RunRule(BiomeType biome)
	{
		if (BurdenManager.IsBurdenActive(BurdenType.TowerBiomeUp))
		{
			if (!m_isInitialized)
			{
				Initialize();
			}
			ProjectileManager.Instance.AddProjectileToPool("TowerBiomeUpWarningProjectile");
			ProjectileManager.Instance.AddProjectileToPool("TowerBiomeUpExplosionProjectile");
			ProjectileManager.Instance.AddProjectileToPool("TowerBiomeUpLancerProjectile");
			Messenger<GameMessenger, GameEvent>.AddListener(GameEvent.PlayerEnterRoom, m_onPlayerEnterRoom);
			Messenger<GameMessenger, GameEvent>.AddListener(GameEvent.PlayerExitRoom, m_onPlayerExitRoom);
		}
		yield break;
	}

	public override void UndoRule(BiomeType biome)
	{
		if (m_isInitialized)
		{
			Messenger<GameMessenger, GameEvent>.RemoveListener(GameEvent.PlayerEnterRoom, m_onPlayerEnterRoom);
			Messenger<GameMessenger, GameEvent>.RemoveListener(GameEvent.PlayerExitRoom, m_onPlayerExitRoom);
		}
	}

	private void OnPlayerEnterRoom(object sender, EventArgs args)
	{
		if (!(args is RoomViaDoorEventArgs { Room: var room }))
		{
			return;
		}
		Room room2 = room as Room;
		MergeRoom mergeRoom = room as MergeRoom;
		bool flag = false;
		if ((bool)mergeRoom)
		{
			GridPointManager[] standaloneGridPointManagers = mergeRoom.StandaloneGridPointManagers;
			for (int i = 0; i < standaloneGridPointManagers.Length; i++)
			{
				if (standaloneGridPointManagers[i].RoomMetaData.SpecialRoomType == SpecialRoomType.BossEntrance)
				{
					flag = true;
					break;
				}
			}
		}
		if ((room.RoomType == RoomType.Standard || room.RoomType == RoomType.Trap) && room.AppearanceBiomeType == BiomeType.TowerExterior && !flag && (((bool)room2 && !room2.GridPointManager.IsTunnelDestination) || ((bool)mergeRoom && !mergeRoom.StandaloneGridPointManagers[0].IsTunnelDestination)))
		{
			m_currentRoom = room;
			m_lancerAttackCoroutine = m_currentRoom.StartCoroutine(Lancer_Attack_Coroutine());
		}
	}

	private void OnPlayerExitRoom(object sender, EventArgs args)
	{
		if (m_isInitialized)
		{
			StopLancerAttackCoroutine();
		}
	}

	public IEnumerator Lancer_Attack_Coroutine()
	{
		float delay2 = Time.time + UnityEngine.Random.Range(m_lancerAttack_InitialDelay.x, m_lancerAttack_InitialDelay.y);
		while (true)
		{
			if (Time.time < delay2)
			{
				yield return null;
				continue;
			}
			int randAmount = UnityEngine.Random.Range(m_lancerAttackRandomAmount.x, m_lancerAttackRandomAmount.y + 1);
			for (int i = 0; i < randAmount; i++)
			{
				m_fireProjectileCoroutines[i] = m_currentRoom.StartCoroutine(FireLancerProjectile(i));
				float num = UnityEngine.Random.Range(m_lancerAttack_RandomSpawnInterval.x, m_lancerAttack_RandomSpawnInterval.y);
				delay2 = num + Time.time;
				while (Time.time < delay2)
				{
					yield return null;
				}
			}
			delay2 = Time.time + UnityEngine.Random.Range(m_lancerAttackRandomDelay.x, m_lancerAttackRandomDelay.y);
		}
	}

	private IEnumerator FireLancerProjectile(int index)
	{
		PlayerController playerController = PlayerManager.GetPlayerController();
		float num = UnityEngine.Random.Range(m_lancerAttack_RandomPosXOffset.x, m_lancerAttack_RandomPosXOffset.y);
		float num2 = UnityEngine.Random.Range(m_lancerAttack_RandomPosYOffset.x, m_lancerAttack_RandomPosYOffset.y);
		if (CDGHelper.RandomPlusMinus() > 0)
		{
			num *= -1f;
		}
		if (CDGHelper.RandomPlusMinus() > 0)
		{
			num2 *= -1f;
		}
		Vector2 vector = (Vector2)CameraController.GameCamera.transform.position + new Vector2(num, num2);
		AudioManager.PlayOneShot(null, "event:/SFX/Enemies/sfx_hazard_shadowKnight_prep", vector);
		m_lancerWarningProjectiles[index] = ProjectileManager.FireProjectile(m_currentRoom.gameObject, "TowerBiomeUpWarningProjectile", vector, matchFacing: false, 0f, 1f, useAbsPos: true);
		float delay = Time.time + m_lancerAttackWarningDuration;
		while (Time.time < delay)
		{
			yield return null;
		}
		Vector3 position = m_lancerWarningProjectiles[index].transform.position;
		StopProjectile(ref m_lancerWarningProjectiles[index]);
		ProjectileManager.FireProjectile(m_currentRoom.gameObject, "TowerBiomeUpExplosionProjectile", position, matchFacing: false, 0f, 1f, useAbsPos: true);
		EffectManager.PlayEffect(m_currentRoom.gameObject, null, "CameraShakeVerySmall_Effect", position, 0.1f, EffectStopType.Gracefully);
		float num3 = CDGHelper.VectorToAngle(playerController.Midpoint - position);
		m_lancerProjectiles[index] = ProjectileManager.FireProjectile(m_currentRoom.gameObject, "TowerBiomeUpLancerProjectile", position, matchFacing: true, num3, 1f, useAbsPos: true);
		m_lancerProjectiles[index].Orientation = CDGHelper.ToRadians(num3);
	}

	private void StopLancerAttackCoroutine()
	{
		for (int i = 0; i < m_lancerProjectiles.Length; i++)
		{
			StopProjectile(ref m_lancerProjectiles[i]);
			StopProjectile(ref m_lancerWarningProjectiles[i]);
			if (m_fireProjectileCoroutines[i] != null && (bool)m_currentRoom)
			{
				m_currentRoom.StopCoroutine(m_fireProjectileCoroutines[i]);
			}
			m_fireProjectileCoroutines[i] = null;
		}
		if (m_lancerAttackCoroutine != null && (bool)m_currentRoom)
		{
			m_currentRoom.StopCoroutine(m_lancerAttackCoroutine);
		}
		m_lancerAttackCoroutine = null;
	}

	protected void StopProjectile(ref Projectile_RL projectile)
	{
		if ((bool)projectile && projectile.isActiveAndEnabled)
		{
			projectile.FlagForDestruction();
		}
		projectile = null;
	}
}
