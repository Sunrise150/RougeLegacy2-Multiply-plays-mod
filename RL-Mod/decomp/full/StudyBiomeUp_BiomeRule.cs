using System;
using System.Collections;
using UnityEngine;

[CreateAssetMenu(menuName = "Custom/Biome Rules/Study Biome Up")]
public class StudyBiomeUp_BiomeRule : BiomeRule
{
	private const string VOID_ATTACK_WARNING_PROJECTILE = "StudyBiomeUpVoidWarningProjectile";

	private const string VOID_ATTACK_PROJECTILE = "StudyBiomeUpVoidProjectile";

	private BaseRoom m_currentRoom;

	private Action<object, EventArgs> m_onPlayerEnterRoom;

	private Action<object, EventArgs> m_onPlayerExitRoom;

	private bool m_actionsInitialized;

	public static Vector2 VOID_WAVE_INITIAL_DELAY = new Vector2(1.5f, 3.5f);

	public static Vector2 VOID_WAVE_RANDOM_DELAY = new Vector2(10f, 12f);

	public const float VOID_WAVE_WARNING_DURATION = 1.5f;

	private float m_voidAttackWarningDuration = 2.5f;

	private Projectile_RL m_voidAttack_WarningProjectile;

	private Projectile_RL m_voidAttack_Projectile;

	private Coroutine m_voidAttackCoroutine;

	public override BiomeRuleExecutionTime ExecutionTime => BiomeRuleExecutionTime.PlayerEnterBiome;

	private Vector2 m_voidAttack_Initial_Delay => Hazard_EV.VOID_WAVE_INITIAL_DELAY;

	private Vector2 m_voidAttackRandomDelay => Hazard_EV.VOID_WAVE_RANDOM_DELAY;

	public override IEnumerator RunRule(BiomeType biome)
	{
		if (!m_actionsInitialized)
		{
			m_onPlayerEnterRoom = OnPlayerEnterRoom;
			m_onPlayerExitRoom = OnPlayerExitRoom;
			m_actionsInitialized = true;
		}
		if (BurdenManager.IsBurdenActive(BurdenType.StudyBiomeUp))
		{
			ProjectileManager.Instance.AddProjectileToPool("StudyBiomeUpVoidWarningProjectile");
			ProjectileManager.Instance.AddProjectileToPool("StudyBiomeUpVoidProjectile");
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
		if (args is RoomViaDoorEventArgs { Room: var room })
		{
			Room room2 = room as Room;
			MergeRoom mergeRoom = room as MergeRoom;
			if ((room.RoomType == RoomType.Standard || room.RoomType == RoomType.Trap) && (((bool)room2 && !room2.GridPointManager.IsTunnelDestination) || ((bool)mergeRoom && !mergeRoom.StandaloneGridPointManagers[0].IsTunnelDestination)))
			{
				m_currentRoom = room;
				m_voidAttackCoroutine = m_currentRoom.StartCoroutine(Void_Attack_Coroutine());
			}
		}
	}

	private void OnPlayerExitRoom(object sender, EventArgs args)
	{
		StopVoidAttack();
		m_voidAttackCoroutine = null;
	}

	public IEnumerator Void_Attack_Coroutine()
	{
		float delay = Time.time + UnityEngine.Random.Range(m_voidAttack_Initial_Delay.x, m_voidAttack_Initial_Delay.y);
		while (true)
		{
			if (Time.time < delay)
			{
				yield return null;
				continue;
			}
			yield return FireVoidProjectile();
			delay = Time.time + UnityEngine.Random.Range(m_voidAttackRandomDelay.x, m_voidAttackRandomDelay.y);
		}
	}

	private IEnumerator FireVoidProjectile()
	{
		Vector2 offset = CameraController.GameCamera.transform.position;
		bool startOnLeft = CDGHelper.RandomPlusMinus() > 0;
		float startingAngle;
		if (startOnLeft)
		{
			startingAngle = 0f;
			offset.x = CameraController.GameCamera.transform.position.x - 16f - 1f;
		}
		else
		{
			startingAngle = 180f;
			offset.x = CameraController.GameCamera.transform.position.x + 16f + 1f;
		}
		m_voidAttack_WarningProjectile = ProjectileManager.FireProjectile(m_currentRoom.gameObject, "StudyBiomeUpVoidWarningProjectile", offset, matchFacing: false, 90f, 1f, useAbsPos: true);
		m_voidAttack_WarningProjectile.transform.SetParent(CameraController.ForegroundOrthoCam.transform, worldPositionStays: true);
		float delay = Time.time + m_voidAttackWarningDuration;
		while (Time.time < delay)
		{
			yield return null;
		}
		Vector3 position = m_voidAttack_WarningProjectile.transform.position;
		StopProjectile(ref m_voidAttack_WarningProjectile);
		m_voidAttack_Projectile = ProjectileManager.FireProjectile(m_currentRoom.gameObject, "StudyBiomeUpVoidProjectile", position, matchFacing: false, startingAngle, 1f, useAbsPos: true);
		Vector3 localScale = m_voidAttack_Projectile.transform.localScale;
		if (startOnLeft)
		{
			localScale.x = Mathf.Abs(localScale.x);
		}
		else
		{
			localScale.x = Mathf.Abs(localScale.x) * -1f;
		}
		m_voidAttack_Projectile.transform.localScale = localScale;
	}

	private void StopVoidAttack()
	{
		StopProjectile(ref m_voidAttack_Projectile);
		StopProjectile(ref m_voidAttack_WarningProjectile);
		if (m_voidAttackCoroutine != null && (bool)m_currentRoom)
		{
			m_currentRoom.StopCoroutine(m_voidAttackCoroutine);
		}
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
