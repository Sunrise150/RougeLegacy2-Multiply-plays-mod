using System;
using System.Collections;
using RLAudio;
using UnityEngine;

[CreateAssetMenu(menuName = "Custom/Biome Rules/Bridge Biome Up")]
public class BridgeBiomeUp_BiomeRule : BiomeRule, IAudioEventEmitter
{
	private const string WARNING_PROJECTILE = "BridgeBiomeUpCannonBallWarningProjectile";

	private const string CANNONBALL_PROJECTILE = "BridgeBiomeUpCannonBallProjectile";

	private BaseRoom m_currentRoom;

	private Action<object, EventArgs> m_onPlayerEnterRoom;

	private Action<object, EventArgs> m_onPlayerExitRoom;

	private bool m_actionsInitialized;

	private Coroutine m_attackCoroutine;

	private WaitRL_Yield m_waitBetweenCannonballRows;

	public override BiomeRuleExecutionTime ExecutionTime => BiomeRuleExecutionTime.PlayerEnterBiome;

	public string Description => "BridgeBiomeUp_BiomeRule";

	public override IEnumerator RunRule(BiomeType biome)
	{
		if (!m_actionsInitialized)
		{
			m_onPlayerEnterRoom = OnPlayerEnterRoom;
			m_onPlayerExitRoom = OnPlayerExitRoom;
			m_waitBetweenCannonballRows = new WaitRL_Yield(0.1f);
			m_actionsInitialized = true;
		}
		if (BurdenManager.IsBurdenActive(BurdenType.BridgeBiomeUp))
		{
			ProjectileManager.Instance.AddProjectileToPool("BridgeBiomeUpCannonBallWarningProjectile");
			ProjectileManager.Instance.AddProjectileToPool("BridgeBiomeUpCannonBallProjectile");
			Messenger<GameMessenger, GameEvent>.AddListener(GameEvent.PlayerEnterRoom, m_onPlayerEnterRoom);
			Messenger<GameMessenger, GameEvent>.AddListener(GameEvent.PlayerExitRoom, m_onPlayerExitRoom);
		}
		yield break;
	}

	public override void UndoRule(BiomeType biome)
	{
		if (m_actionsInitialized)
		{
			Messenger<GameMessenger, GameEvent>.RemoveListener(GameEvent.PlayerEnterRoom, m_onPlayerEnterRoom);
			Messenger<GameMessenger, GameEvent>.RemoveListener(GameEvent.PlayerExitRoom, m_onPlayerExitRoom);
		}
	}

	private void OnPlayerEnterRoom(object sender, EventArgs args)
	{
		if (args is RoomViaDoorEventArgs roomViaDoorEventArgs)
		{
			MergeRoom mergeRoom = roomViaDoorEventArgs.Room as MergeRoom;
			if ((bool)mergeRoom)
			{
				m_currentRoom = mergeRoom;
				m_attackCoroutine = m_currentRoom.StartCoroutine(Attack_Coroutine());
			}
		}
	}

	private void OnPlayerExitRoom(object sender, EventArgs args)
	{
		if (m_attackCoroutine != null && (bool)m_currentRoom)
		{
			m_currentRoom.StopCoroutine(m_attackCoroutine);
		}
		m_attackCoroutine = null;
	}

	public IEnumerator Attack_Coroutine()
	{
		float num = 5f;
		while (true)
		{
			float waitTime = Time.time + num;
			while (Time.time < waitTime)
			{
				yield return null;
			}
			float y = PlayerManager.GetCurrentPlayerRoom().Bounds.max.y + 10f;
			float num2 = 5f;
			Vector2 vector;
			Vector2 offset5 = (vector = new Vector2(PlayerManager.GetPlayerController().Midpoint.x, y));
			Vector2 offset4 = (vector = vector);
			Vector2 offset3 = (vector = vector);
			Vector2 offset2 = (vector = vector);
			Vector2 offset6 = vector;
			offset6.y = y;
			offset2.x += num2;
			offset3.x += 0f - num2;
			offset4.x += num2 * 2f;
			offset5.x += (0f - num2) * 2f;
			AudioManager.PlayOneShot(this, "event:/SFX/Enemies/sfx_enemy_skeletonBoss_portal_multiple", PlayerManager.GetPlayerController().Midpoint);
			ProjectileManager.FireProjectile(m_currentRoom.gameObject, "BridgeBiomeUpCannonBallWarningProjectile", offset6, matchFacing: false, 270f, 1f, useAbsPos: true);
			ProjectileManager.FireProjectile(m_currentRoom.gameObject, "BridgeBiomeUpCannonBallWarningProjectile", offset2, matchFacing: false, 270f, 1f, useAbsPos: true);
			ProjectileManager.FireProjectile(m_currentRoom.gameObject, "BridgeBiomeUpCannonBallWarningProjectile", offset3, matchFacing: false, 270f, 1f, useAbsPos: true);
			ProjectileManager.FireProjectile(m_currentRoom.gameObject, "BridgeBiomeUpCannonBallWarningProjectile", offset4, matchFacing: false, 270f, 1f, useAbsPos: true);
			ProjectileManager.FireProjectile(m_currentRoom.gameObject, "BridgeBiomeUpCannonBallWarningProjectile", offset5, matchFacing: false, 270f, 1f, useAbsPos: true);
			ProjectileManager.FireProjectile(m_currentRoom.gameObject, "BridgeBiomeUpCannonBallProjectile", offset6, matchFacing: true, -90f, 1f, useAbsPos: true);
			yield return m_waitBetweenCannonballRows;
			ProjectileManager.FireProjectile(m_currentRoom.gameObject, "BridgeBiomeUpCannonBallProjectile", offset2, matchFacing: true, -90f, 1f, useAbsPos: true);
			Projectile_RL projectile_RL = ProjectileManager.FireProjectile(m_currentRoom.gameObject, "BridgeBiomeUpCannonBallProjectile", offset3, matchFacing: true, -90f, 1f, useAbsPos: true);
			AudioManager.PlayOneShotAttached(this, "event:/SFX/Enemies/sfx_enemy_skeletonBoss_cannon_projectiles_loop", projectile_RL.gameObject);
			yield return m_waitBetweenCannonballRows;
			ProjectileManager.FireProjectile(m_currentRoom.gameObject, "BridgeBiomeUpCannonBallProjectile", offset4, matchFacing: true, -90f, 1f, useAbsPos: true);
			ProjectileManager.FireProjectile(m_currentRoom.gameObject, "BridgeBiomeUpCannonBallProjectile", offset5, matchFacing: true, -90f, 1f, useAbsPos: true);
			num = UnityEngine.Random.Range(8f, 13f);
		}
	}
}
