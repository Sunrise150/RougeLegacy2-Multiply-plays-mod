using System;
using System.Collections;
using FMOD.Studio;
using RLAudio;
using RL_Windows;
using Rewired;
using UnityEngine;

public class CrowsNest_Ability : BaseAbility_RL, ITalent, IAbility
{
	private const string CROWS_NEST_STINGER = "event:/Stingers/sting_pirateShip_loop";

	private const string CROWS_NEST_MOVE_START = "event:/SFX/Weapons/sfx_weapon_pirateShip_move_start";

	private const string CROWS_NEST_MOVE_STOP = "event:/SFX/Weapons/sfx_weapon_pirateShip_move_stop";

	private const string CROWS_NEST_SPAWN_LOOP = "event:/SFX/Weapons/sfx_weapon_pirateShip_spawn_loop";

	[SerializeField]
	private string m_crowsNestDestroyProjectile;

	[SerializeField]
	private string m_fireRepeatProjectileName;

	private bool m_crowsNestActive;

	private bool m_platformActivated;

	private Projectile_RL m_crowsNestProjectile;

	private BoxCollider2D m_crowsNestCollider;

	private float m_crowsNextMoveAmount;

	private GameObject m_crowsNextSideColliderGO;

	private bool m_wasMoving;

	private bool m_movedThisFrame;

	private Action<MonoBehaviour, EventArgs> m_onPlayerExitRoom;

	private Action<MonoBehaviour, EventArgs> m_resumeCooldownIfPlayerExitsRoom;

	private Action<MonoBehaviour, EventArgs> m_onPlayerDash;

	private Action<MonoBehaviour, EventArgs> m_onPlayerDeath;

	private Action<MonoBehaviour, EventArgs> m_onPlayerFakeDeath;

	private Action<Projectile_RL, GameObject> m_resumeCooldown;

	private bool m_decreasesCDOverTime;

	private bool m_decreasesCDOnHit;

	private Action<GameObject> m_onTimeout;

	private EventInstance m_stingerInstance;

	private EventInstance m_spawnLoopInstance;

	private float m_fireRepeatTimer;

	protected override float TellIntroAnimSpeed => 2.25f;

	protected override float TellIntroAnimExitDelay => 0f;

	protected override float TellAnimSpeed => 10f;

	protected override float TellAnimExitDelay => 0f;

	protected override float AttackIntroAnimSpeed => 10f;

	protected override float AttackIntroAnimExitDelay => 0f;

	protected override float AttackAnimSpeed => 1f;

	protected override float AttackAnimExitDelay => 0.15f;

	protected override float ExitAnimSpeed => 1f;

	protected override float ExitAnimExitDelay => 0f;

	public bool CrowsNestActive => m_crowsNestActive;

	protected override void InitializeProjectileNameArray()
	{
		m_projectileNameArray = new string[3] { m_projectileName, m_crowsNestDestroyProjectile, m_fireRepeatProjectileName };
	}

	protected override void Awake()
	{
		base.Awake();
		m_onPlayerExitRoom = OnPlayerExitRoom;
		m_resumeCooldownIfPlayerExitsRoom = ResumeCooldownIfPlayerExitsRoom;
		m_onPlayerDash = OnPlayerDash;
		m_resumeCooldown = ResumeCooldown;
		m_decreasesCDOverTime = base.DecreaseCooldownOverTime;
		m_decreasesCDOnHit = base.DecreaseCooldownWhenHit;
		m_onTimeout = OnTimeout;
		m_onPlayerDeath = OnPlayerDeath;
		m_onPlayerFakeDeath = OnPlayerFakeDeath;
		m_stingerInstance = AudioUtility.GetEventInstance("event:/Stingers/sting_pirateShip_loop", base.transform);
		m_spawnLoopInstance = AudioUtility.GetEventInstance("event:/SFX/Weapons/sfx_weapon_pirateShip_spawn_loop", base.transform);
	}

	private void OnEnable()
	{
		Messenger<GameMessenger, GameEvent>.AddListener(GameEvent.PlayerExitRoom, m_onPlayerExitRoom);
		Messenger<GameMessenger, GameEvent>.AddListener(GameEvent.PlayerDeath, m_onPlayerDeath);
		Messenger<GameMessenger, GameEvent>.AddListener(GameEvent.PlayerFakedDeath, m_onPlayerFakeDeath);
		Messenger<GameMessenger, GameEvent>.AddListener(GameEvent.PlayerEnterRoom, m_resumeCooldownIfPlayerExitsRoom);
	}

	private void OnDisable()
	{
		Messenger<GameMessenger, GameEvent>.RemoveListener(GameEvent.PlayerExitRoom, m_onPlayerExitRoom);
		Messenger<GameMessenger, GameEvent>.RemoveListener(GameEvent.PlayerDeath, m_onPlayerDeath);
		Messenger<GameMessenger, GameEvent>.RemoveListener(GameEvent.PlayerFakedDeath, m_onPlayerFakeDeath);
		Messenger<GameMessenger, GameEvent>.RemoveListener(GameEvent.PlayerEnterRoom, m_resumeCooldownIfPlayerExitsRoom);
	}

	public override void PreCastAbility()
	{
		DestroyCrowsNest(fireDestroyProjectile: true);
		base.PreCastAbility();
	}

	protected override void FireProjectile()
	{
		base.FireProjectile();
		m_crowsNestProjectile = m_firedProjectile;
		m_crowsNestCollider = m_crowsNestProjectile.HitboxController.GetCollider(HitboxType.Platform) as BoxCollider2D;
		m_crowsNestCollider.gameObject.layer = 11;
		m_abilityController.PlayerController.CharacterJump.ResetBrakeForce();
		m_abilityController.PlayerController.SetVelocity(0f, 10f, additive: false);
		m_crowsNestProjectile.HitboxController.DisableAllCollisions = false;
		m_crowsNestProjectile.OnTimeoutEffectTriggerRelay.AddOnce(m_onTimeout);
		m_fireRepeatTimer = Time.time + 0.15f;
		bool flag = true;
		if (ChallengeManager.IsInChallenge && ChallengeManager.ActiveChallenge.ChallengeData.ScoringType == ChallengeScoringType.Battle)
		{
			flag = false;
		}
		else
		{
			EnemySpawnController[] enemySpawnControllers = m_abilityController.PlayerController.CurrentlyInRoom.SpawnControllerManager.EnemySpawnControllers;
			foreach (EnemySpawnController enemySpawnController in enemySpawnControllers)
			{
				if ((bool)enemySpawnController.EnemyInstance && enemySpawnController.EnemyInstance.IsBoss && enemySpawnController.EnemyInstance.EnemyType != EnemyType.Eggplant)
				{
					flag = false;
					break;
				}
			}
		}
		if (m_stingerInstance.isValid() && flag && MusicManager.CurrentSong != SongID.JUKEBOX_Misc_PirateSong_1_ASITP_PirateLoop_1)
		{
			AudioManager.Play(this, m_stingerInstance);
			m_stingerInstance.setParameterByName("pirateShip_on", 1f);
		}
		if (m_spawnLoopInstance.isValid())
		{
			AudioManager.Play(this, m_spawnLoopInstance);
			m_spawnLoopInstance.setParameterByName("pirateShip_move", 0f);
		}
		for (int j = 0; j < m_crowsNestProjectile.HitboxController.gameObject.transform.childCount; j++)
		{
			GameObject gameObject = m_crowsNestProjectile.HitboxController.gameObject.transform.GetChild(j).gameObject;
			if (gameObject.CompareTag("Barricade"))
			{
				m_crowsNextSideColliderGO = gameObject.gameObject;
				m_crowsNextSideColliderGO.SetActive(value: false);
				break;
			}
		}
		if (m_decreasesCDOverTime)
		{
			base.DecreaseCooldownOverTime = false;
		}
		if (m_decreasesCDOnHit)
		{
			base.DecreaseCooldownWhenHit = false;
		}
		base.DisplayPausedAbilityCooldown = true;
		m_crowsNestProjectile.OnDeathRelay.AddOnce(m_resumeCooldown);
		m_crowsNextMoveAmount = 0f;
		m_crowsNestActive = true;
		m_wasMoving = false;
		m_movedThisFrame = false;
	}

	private void FireProjectileOnRepeat()
	{
		float num = m_crowsNestProjectile.Lifespan * 0.19999999f;
		if (!(m_crowsNestProjectile.LifespanTimer <= num))
		{
			float num2 = UnityEngine.Random.Range(Ability_EV.CROWS_NEST_RANDOM_BULLET_ANGLES.x, Ability_EV.CROWS_NEST_RANDOM_BULLET_ANGLES.y);
			Vector2 offset = (Vector2)m_crowsNestProjectile.transform.position + new Vector2(1.15f, 0f);
			if (m_crowsNestProjectile.transform.localScale.x < 0f)
			{
				num2 = 180f - num2;
				offset = (Vector2)m_crowsNestProjectile.transform.position - new Vector2(1.15f, 0f);
			}
			Projectile_RL projectile_RL = ProjectileManager.FireProjectile(m_abilityController.PlayerController.gameObject, m_fireRepeatProjectileName, offset, matchFacing: false, num2, 1f, useAbsPos: true);
			m_abilityController.InitializeProjectile(projectile_RL);
			projectile_RL.CastAbilityType = CastAbilityType.Talent;
		}
	}

	private void ResumeCooldownIfPlayerExitsRoom(object sender, EventArgs args)
	{
		if (m_decreasesCDOverTime)
		{
			base.DecreaseCooldownOverTime = true;
		}
		if (m_decreasesCDOnHit)
		{
			base.DecreaseCooldownWhenHit = true;
		}
		base.DisplayPausedAbilityCooldown = false;
	}

	private void ResumeCooldown(Projectile_RL proj, GameObject obj)
	{
		if (m_decreasesCDOverTime)
		{
			base.DecreaseCooldownOverTime = true;
		}
		if (m_decreasesCDOnHit)
		{
			base.DecreaseCooldownWhenHit = true;
		}
		base.DisplayPausedAbilityCooldown = false;
	}

	private void LateUpdate()
	{
		if (!m_crowsNestActive)
		{
			return;
		}
		bool flag = (bool)m_crowsNestProjectile && m_crowsNestProjectile.gameObject.activeSelf && !m_abilityController.PlayerController.CharacterDash.IsDashing && ((!m_platformActivated && (bool)m_abilityController.PlayerController.ControllerCorgi.StandingOnCollider && m_abilityController.PlayerController.ControllerCorgi.StandingOnCollider == m_crowsNestCollider) || (m_platformActivated && Mathf.Abs(m_abilityController.PlayerController.transform.localPosition.y - m_crowsNestProjectile.transform.localPosition.y) <= 0.1f));
		if (!m_platformActivated && flag)
		{
			ActivatePlatform();
		}
		if (m_platformActivated)
		{
			if (flag)
			{
				m_movedThisFrame = false;
				HandlePlatformControls();
				if (Time.time > m_fireRepeatTimer)
				{
					m_fireRepeatTimer = Time.time + 0.15f;
					FireProjectileOnRepeat();
				}
				if (m_crowsNestProjectile.IsFlipped == m_abilityController.PlayerController.IsFacingRight)
				{
					m_crowsNestProjectile.Flip();
				}
				if (m_movedThisFrame)
				{
					if (!m_wasMoving)
					{
						AudioManager.PlayOneShotAttached(this, "event:/SFX/Weapons/sfx_weapon_pirateShip_move_start", base.gameObject);
					}
					m_spawnLoopInstance.setParameterByName("pirateShip_move", 1f);
				}
				else
				{
					if (m_wasMoving)
					{
						AudioManager.PlayOneShotAttached(this, "event:/SFX/Weapons/sfx_weapon_pirateShip_move_stop", base.gameObject);
					}
					m_spawnLoopInstance.setParameterByName("pirateShip_move", 0f);
				}
				m_wasMoving = m_movedThisFrame;
			}
			else
			{
				DeactivatePlatform();
			}
		}
		else if (Time.time > m_fireRepeatTimer)
		{
			m_fireRepeatTimer = Time.time + 0.15f;
			FireProjectileOnRepeat();
		}
		if (Rewired_RL.Player.GetButtonDown(m_abilityController.GetAbilityInputString(base.CastAbilityType)))
		{
			DestroyCrowsNest(fireDestroyProjectile: true);
		}
	}

	private void ActivatePlatform()
	{
		m_crowsNestCollider.gameObject.layer = 9;
		Vector3 localPosition = m_abilityController.PlayerController.transform.localPosition;
		localPosition.x = m_crowsNestProjectile.transform.localPosition.x;
		m_abilityController.PlayerController.transform.localPosition = localPosition;
		m_abilityController.PlayerController.ControllerCorgi.SetRaysParameters();
		if ((bool)m_crowsNextSideColliderGO)
		{
			m_crowsNextSideColliderGO.SetActive(value: true);
		}
		Messenger<GameMessenger, GameEvent>.AddListener(GameEvent.PlayerDash, m_onPlayerDash);
		m_platformActivated = true;
		m_abilityController.PlayerController.ControllerCorgi.DisableOneWayCollision = true;
	}

	private void DeactivatePlatform()
	{
		m_crowsNestCollider.gameObject.layer = 11;
		if ((bool)m_crowsNextSideColliderGO)
		{
			m_crowsNextSideColliderGO.SetActive(value: false);
		}
		Messenger<GameMessenger, GameEvent>.RemoveListener(GameEvent.PlayerDash, m_onPlayerDash);
		m_platformActivated = false;
		m_abilityController.PlayerController.ControllerCorgi.DisableOneWayCollision = false;
	}

	private void OnPlayerDash(object sender, EventArgs args)
	{
	}

	private void HandlePlatformControls()
	{
		PlayerController playerController = m_abilityController.PlayerController;
		if (playerController.CastAbility.IsAiming || (ReInput.isReady && Rewired_RL.Player.GetButton("FreeLook")))
		{
			return;
		}
		if (Rewired_RL.Player.GetButtonDown("Jump"))
		{
			DeactivatePlatform();
			return;
		}
		if (playerController.CharacterDash.IsDashing)
		{
			DeactivatePlatform();
			return;
		}
		int num = 0;
		num |= 0x100;
		float y = playerController.CurrentlyInRoom.Bounds.max.y;
		Bounds bounds = default(Bounds);
		bounds.center = playerController.transform.position;
		bounds.Encapsulate(playerController.HitboxController.GetCollider(HitboxType.Platform).bounds);
		bounds.Encapsulate(m_crowsNestCollider.bounds);
		Vector2 origin = bounds.center;
		Vector2 size = bounds.size;
		float num2 = 0.5f;
		RaycastHit2D raycastHit2D = Physics2D.BoxCast(origin, size, 0f, Vector2.up, num2, num);
		bool flag = (!raycastHit2D || ((bool)raycastHit2D && bounds.center.y > raycastHit2D.point.y)) && bounds.max.y < y - num2;
		float y2 = playerController.CurrentlyInRoom.Bounds.min.y;
		RaycastHit2D raycastHit2D2 = Physics2D.BoxCast(origin, size, 0f, Vector2.down, num2, num);
		bool flag2 = (!raycastHit2D2 || ((bool)raycastHit2D2 && bounds.center.y < raycastHit2D2.point.y)) && bounds.min.y > y2 + num2;
		float num3 = 14f * Time.deltaTime;
		float x = playerController.CurrentlyInRoom.Bounds.min.x;
		RaycastHit2D raycastHit2D3 = Physics2D.BoxCast(origin, size, 0f, Vector2.left, num2, num);
		bool flag3 = (!raycastHit2D3 || ((bool)raycastHit2D3 && bounds.center.x < raycastHit2D3.point.x)) && bounds.min.x > x + num2;
		float x2 = playerController.CurrentlyInRoom.Bounds.max.x;
		RaycastHit2D raycastHit2D4 = Physics2D.BoxCast(origin, size, 0f, Vector2.right, num2, num);
		bool num4 = (!raycastHit2D4 || ((bool)raycastHit2D4 && bounds.center.x > raycastHit2D4.point.x)) && bounds.max.x < x2 - num2;
		float num5 = Rewired_RL.Player.GetAxis("MoveVertical");
		float num6 = Rewired_RL.Player.GetAxis("MoveHorizontal");
		if (!flag && num5 > 0f)
		{
			num5 = 0f;
		}
		if (!flag2 && num5 < 0f)
		{
			num5 = 0f;
		}
		if (!flag3 && num6 < 0f)
		{
			num6 = 0f;
		}
		if (!num4 && num6 > 0f)
		{
			num6 = 0f;
		}
		playerController.transform.SetPositionX(playerController.transform.localPosition.x + num6 * num3);
		playerController.transform.SetPositionY(playerController.transform.localPosition.y + num5 * num3);
		playerController.ControllerCorgi.SetRaysParameters();
		playerController.ControllerCorgi.ResetState();
		m_crowsNestProjectile.transform.SetPositionX(m_crowsNestProjectile.transform.position.x + num6 * num3);
		m_crowsNestProjectile.transform.SetPositionY(m_crowsNestProjectile.transform.position.y + num5 * num3);
		playerController.ControllerCorgi.ForceStandingOn(m_crowsNestCollider);
		if (num6 != 0f || num5 != 0f)
		{
			m_movedThisFrame = true;
		}
	}

	private void OnPlayerExitRoom(object sender, EventArgs args)
	{
		DestroyCrowsNest(fireDestroyProjectile: false);
	}

	private void OnPlayerDeath(object sender, EventArgs args)
	{
		DestroyCrowsNest(fireDestroyProjectile: false);
	}

	private void OnPlayerFakeDeath(object sender, EventArgs args)
	{
		StartCoroutine(OnPlayerFakeDeathCoroutine());
	}

	private IEnumerator OnPlayerFakeDeathCoroutine()
	{
		if (m_stingerInstance.isValid())
		{
			m_stingerInstance.getPlaybackState(out var state);
			if (state != PLAYBACK_STATE.STOPPED && state != PLAYBACK_STATE.STOPPING)
			{
				m_stingerInstance.setParameterByName("pirateShip_on", 0f);
			}
		}
		yield return null;
		while (WindowManager.GetIsWindowOpen(WindowID.DeathDefy))
		{
			yield return null;
		}
		if (m_crowsNestActive && m_stingerInstance.isValid())
		{
			AudioManager.Play(null, m_stingerInstance);
			m_stingerInstance.setParameterByName("pirateShip_on", 1f);
		}
	}

	private void OnTimeout(GameObject obj)
	{
		DestroyCrowsNest(fireDestroyProjectile: true);
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		if (!GameManager.IsApplicationClosing)
		{
			DestroyCrowsNest(fireDestroyProjectile: false);
		}
		if (m_stingerInstance.isValid())
		{
			m_stingerInstance.release();
		}
		if (m_spawnLoopInstance.isValid())
		{
			m_spawnLoopInstance.release();
		}
	}

	private void DestroyCrowsNest(bool fireDestroyProjectile)
	{
		if ((bool)m_crowsNestProjectile && m_crowsNestProjectile.gameObject.activeSelf && !m_crowsNestProjectile.IsFreePoolObj)
		{
			if (fireDestroyProjectile)
			{
				float angleInDeg = 0f;
				Projectile_RL projectile_RL = ProjectileManager.FireProjectile(m_crowsNestProjectile.gameObject, m_crowsNestDestroyProjectile, Vector2.zero, matchFacing: true, angleInDeg);
				m_abilityController.InitializeProjectile(projectile_RL);
				projectile_RL.CastAbilityType = CastAbilityType.Talent;
				projectile_RL.Owner = m_abilityController.gameObject;
				ProjectileManager.ApplyProjectileDamage(m_abilityController.PlayerController, projectile_RL);
				projectile_RL.RotationSpeed = projectile_RL.InitialRotationSpeed;
				if (m_crowsNestProjectile.IsFlipped)
				{
					projectile_RL.Heading = new Vector2(0f - projectile_RL.Heading.x, projectile_RL.Heading.y);
				}
			}
			if ((bool)m_crowsNextSideColliderGO)
			{
				m_crowsNextSideColliderGO.SetActive(value: false);
			}
			m_crowsNestProjectile.HitboxController.DisableAllCollisions = true;
			m_crowsNestProjectile.FlagForDestruction();
		}
		if (m_stingerInstance.isValid())
		{
			m_stingerInstance.getPlaybackState(out var state);
			if (state != PLAYBACK_STATE.STOPPED && state != PLAYBACK_STATE.STOPPING)
			{
				m_stingerInstance.setParameterByName("pirateShip_on", 0f);
			}
		}
		if (m_spawnLoopInstance.isValid())
		{
			AudioManager.Stop(m_spawnLoopInstance);
		}
		m_abilityController.PlayerController.ControllerCorgi.DisableOneWayCollision = false;
		Messenger<GameMessenger, GameEvent>.RemoveListener(GameEvent.PlayerDash, m_onPlayerDash);
		m_crowsNextSideColliderGO = null;
		m_crowsNestCollider = null;
		m_platformActivated = false;
		m_crowsNestActive = false;
	}
}
