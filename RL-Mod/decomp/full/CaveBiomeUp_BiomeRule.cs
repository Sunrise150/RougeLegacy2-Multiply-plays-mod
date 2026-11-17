using System;
using System.Collections;
using FMOD.Studio;
using RLAudio;
using UnityEngine;

[CreateAssetMenu(menuName = "Custom/Biome Rules/Cave Biome Up")]
public class CaveBiomeUp_BiomeRule : BiomeRule
{
	private const string WAVE_ATTACK_WARNING_PROJECTILE = "CaveBossWaveWarningProjectile";

	private const string WAVE_ATTACK_WARNING_LOCKED_PROJECTILE = "CaveBossWaveWarningLockedProjectile";

	private const string WAVE_ATTACK_PROJECTILE = "CaveBossWaveProjectile";

	private const string WAVE_ATTACK_SFX_PREP_LEFT_NAME = "event:/SFX/Enemies/sfx_hazard_bladeStamped_prep_start_loop_left";

	private const string WAVE_ATTACK_SFX_PREP_RIGHT_NAME = "event:/SFX/Enemies/sfx_hazard_bladeStamped_prep_start_loop_right";

	private const string WAVE_ATTACK_SFX_LOCKED_LEFT_NAME = "event:/SFX/Enemies/sfx_hazard_bladeStamped_prep_telegraph_left";

	private const string WAVE_ATTACK_SFX_LOCKED_RIGHT_NAME = "event:/SFX/Enemies/sfx_hazard_bladeStamped_prep_telegraph_right";

	private const string WAVE_ATTACK_SFX_LAUNCH_LEFT_NAME = "event:/SFX/Enemies/sfx_hazard_bladeStamped_launch_left";

	private const string WAVE_ATTACK_SFX_LAUNCH_RIGHT_NAME = "event:/SFX/Enemies/sfx_hazard_bladeStamped_launch_right";

	private BaseRoom m_currentRoom;

	private Action<object, EventArgs> m_onPlayerEnterRoom;

	private Action<object, EventArgs> m_onPlayerExitRoom;

	private bool m_actionsInitialized;

	private EventInstance m_prepLeftEventInstance;

	private EventInstance m_prepRightEventInstance;

	private EventInstance m_lockedLeftEventInstance;

	private EventInstance m_lockedRightEventInstance;

	private float m_waveAttackWarningDuration = 1.5f;

	private float m_waveAttackWarningLockedDuration = 0.35f;

	private Projectile_RL m_waveAttack_WarningProjectile;

	private Projectile_RL m_waveAttack_WarningLockedProjectile;

	private Projectile_RL m_waveAttack_Projectile;

	private Coroutine m_waveAttackCoroutine;

	public override BiomeRuleExecutionTime ExecutionTime => BiomeRuleExecutionTime.PlayerEnterBiome;

	private Vector2 m_waveAttack_Initial_Delay => Hazard_EV.FLYING_WEAPON_WAVE_INITIAL_DELAY;

	private Vector2 m_waveAttackRandomDelay => Hazard_EV.FLYING_WEAPON_WAVE_RANDOM_DELAY;

	private Vector2 m_waveAttackRandomPosOffset => Hazard_EV.FLYING_WEAPON_WAVE_RANDOM_POS_OFFSET;

	private Vector2 m_waveAttackRandomAngle => Hazard_EV.FLYING_WEAPON_WAVE_RANDOM_ANGLE;

	public override IEnumerator RunRule(BiomeType biome)
	{
		if (!m_actionsInitialized)
		{
			if (!m_prepLeftEventInstance.isValid())
			{
				m_prepLeftEventInstance = AudioUtility.GetEventInstance("event:/SFX/Enemies/sfx_hazard_bladeStamped_prep_start_loop_left", CameraController.UICamera.transform);
			}
			if (!m_prepRightEventInstance.isValid())
			{
				m_prepRightEventInstance = AudioUtility.GetEventInstance("event:/SFX/Enemies/sfx_hazard_bladeStamped_prep_start_loop_right", CameraController.UICamera.transform);
			}
			if (!m_lockedLeftEventInstance.isValid())
			{
				m_lockedLeftEventInstance = AudioUtility.GetEventInstance("event:/SFX/Enemies/sfx_hazard_bladeStamped_prep_telegraph_left", CameraController.UICamera.transform);
			}
			if (!m_lockedRightEventInstance.isValid())
			{
				m_lockedRightEventInstance = AudioUtility.GetEventInstance("event:/SFX/Enemies/sfx_hazard_bladeStamped_prep_telegraph_right", CameraController.UICamera.transform);
			}
			m_onPlayerEnterRoom = OnPlayerEnterRoom;
			m_onPlayerExitRoom = OnPlayerExitRoom;
			m_actionsInitialized = true;
		}
		if (BurdenManager.IsBurdenActive(BurdenType.CaveBiomeUp))
		{
			ProjectileManager.Instance.AddProjectileToPool("CaveBossWaveWarningProjectile");
			ProjectileManager.Instance.AddProjectileToPool("CaveBossWaveWarningLockedProjectile");
			ProjectileManager.Instance.AddProjectileToPool("CaveBossWaveProjectile");
			Messenger<GameMessenger, GameEvent>.AddListener(GameEvent.PlayerEnterRoom, m_onPlayerEnterRoom);
			Messenger<GameMessenger, GameEvent>.AddListener(GameEvent.PlayerExitRoom, m_onPlayerExitRoom);
		}
		yield break;
	}

	public override void UndoRule(BiomeType biome)
	{
		if (m_prepLeftEventInstance.isValid())
		{
			m_prepLeftEventInstance.release();
		}
		if (m_prepRightEventInstance.isValid())
		{
			m_prepRightEventInstance.release();
		}
		if (m_lockedLeftEventInstance.isValid())
		{
			m_lockedLeftEventInstance.release();
		}
		if (m_lockedRightEventInstance.isValid())
		{
			m_lockedRightEventInstance.release();
		}
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
				m_waveAttackCoroutine = m_currentRoom.StartCoroutine(Wave_Attack_Coroutine());
			}
		}
	}

	private void OnPlayerExitRoom(object sender, EventArgs args)
	{
		StopWaveAttackCoroutine();
		m_waveAttackCoroutine = null;
		if (m_lockedLeftEventInstance.isValid())
		{
			AudioManager.Stop(m_lockedLeftEventInstance);
		}
		if (m_lockedRightEventInstance.isValid())
		{
			AudioManager.Stop(m_lockedRightEventInstance);
		}
		if (m_prepLeftEventInstance.isValid())
		{
			AudioManager.Stop(m_prepLeftEventInstance);
		}
		if (m_prepRightEventInstance.isValid())
		{
			AudioManager.Stop(m_prepRightEventInstance);
		}
	}

	public IEnumerator Wave_Attack_Coroutine()
	{
		float delay = Time.time + UnityEngine.Random.Range(m_waveAttack_Initial_Delay.x, m_waveAttack_Initial_Delay.y);
		while (true)
		{
			if (Time.time < delay)
			{
				yield return null;
				continue;
			}
			yield return FireWaveProjectile();
			delay = Time.time + UnityEngine.Random.Range(m_waveAttackRandomDelay.x, m_waveAttackRandomDelay.y);
		}
	}

	private IEnumerator FireWaveProjectile()
	{
		PlayerController playerController = PlayerManager.GetPlayerController();
		Vector2 offset = playerController.Midpoint;
		int num = CDGHelper.RandomPlusMinus();
		int num2 = CDGHelper.RandomPlusMinus();
		float num3 = UnityEngine.Random.Range(m_waveAttackRandomPosOffset.x, m_waveAttackRandomPosOffset.y);
		float startingAngle3;
		if (num > 0)
		{
			if (num2 > 0)
			{
				startingAngle3 = 0f;
				offset.x -= 32f;
			}
			else
			{
				startingAngle3 = 180f;
				offset.x += 32f;
			}
			offset.y += num3;
		}
		else
		{
			if (num2 > 0)
			{
				startingAngle3 = -90f;
				offset.y += 18f;
			}
			else
			{
				startingAngle3 = 90f;
				offset.y -= 18f;
			}
			offset.x += num3;
		}
		float num4 = UnityEngine.Random.Range(m_waveAttackRandomAngle.x, m_waveAttackRandomAngle.y);
		startingAngle3 += num4;
		startingAngle3 = CDGHelper.WrapAngleDegrees(startingAngle3, convertToHalfRadius: true);
		bool playSFXLeft = offset.x < playerController.Midpoint.x;
		if (playSFXLeft)
		{
			if (m_prepLeftEventInstance.isValid())
			{
				AudioManager.Play(null, m_prepLeftEventInstance);
			}
		}
		else if (m_prepRightEventInstance.isValid())
		{
			AudioManager.Play(null, m_prepRightEventInstance);
		}
		m_waveAttack_WarningProjectile = ProjectileManager.FireProjectile(m_currentRoom.gameObject, "CaveBossWaveWarningProjectile", offset, matchFacing: false, startingAngle3, 1f, useAbsPos: true);
		_ = m_waveAttack_WarningProjectile.transform.localPosition;
		float delay = Time.time + m_waveAttackWarningDuration;
		while (Time.time < delay)
		{
			yield return null;
		}
		Vector3 warningProjPos = m_waveAttack_WarningProjectile.transform.position;
		StopProjectile(ref m_waveAttack_WarningProjectile);
		if (playSFXLeft)
		{
			if (m_prepLeftEventInstance.isValid())
			{
				AudioManager.Stop(m_prepLeftEventInstance);
			}
			if (m_lockedLeftEventInstance.isValid())
			{
				AudioManager.Play(null, m_lockedLeftEventInstance);
			}
		}
		else
		{
			if (m_prepRightEventInstance.isValid())
			{
				AudioManager.Stop(m_prepRightEventInstance);
			}
			if (m_lockedRightEventInstance.isValid())
			{
				AudioManager.Play(null, m_lockedRightEventInstance);
			}
		}
		m_waveAttack_WarningLockedProjectile = ProjectileManager.FireProjectile(m_currentRoom.gameObject, "CaveBossWaveWarningLockedProjectile", warningProjPos, matchFacing: false, startingAngle3, 1f, useAbsPos: true);
		float delayLocked = Time.time + m_waveAttackWarningLockedDuration;
		while (Time.time < delayLocked)
		{
			yield return null;
		}
		StopProjectile(ref m_waveAttack_WarningLockedProjectile);
		if (playSFXLeft)
		{
			if (m_lockedLeftEventInstance.isValid())
			{
				AudioManager.Stop(m_lockedLeftEventInstance);
			}
		}
		else if (m_lockedRightEventInstance.isValid())
		{
			AudioManager.Stop(m_lockedRightEventInstance);
		}
		if (playSFXLeft)
		{
			AudioManager.PlayOneShot(null, "event:/SFX/Enemies/sfx_hazard_bladeStamped_launch_left", CameraController.UICamera.transform.position);
		}
		else
		{
			AudioManager.PlayOneShot(null, "event:/SFX/Enemies/sfx_hazard_bladeStamped_launch_right", CameraController.UICamera.transform.position);
		}
		m_waveAttack_Projectile = ProjectileManager.FireProjectile(m_currentRoom.gameObject, "CaveBossWaveProjectile", warningProjPos, matchFacing: false, startingAngle3, 1f, useAbsPos: true);
	}

	private void StopWaveAttackCoroutine()
	{
		StopProjectile(ref m_waveAttack_Projectile);
		StopProjectile(ref m_waveAttack_WarningProjectile);
		StopProjectile(ref m_waveAttack_WarningLockedProjectile);
		if (m_waveAttackCoroutine != null && (bool)m_currentRoom)
		{
			m_currentRoom.StopCoroutine(m_waveAttackCoroutine);
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
