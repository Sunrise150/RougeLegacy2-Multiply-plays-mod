using RLAudio;
using UnityEngine;

public class ChakramProjectileLogic : BaseProjectileLogic, ITerrainOnEnterHitResponse, IHitResponse, IAudioEventEmitter
{
	[SerializeField]
	private float m_returnDelay = 0.15f;

	[SerializeField]
	private float m_returnSpeed = 15f;

	[SerializeField]
	private float m_pauseAtApexDelay;

	[SerializeField]
	private bool m_returnToPlayer;

	[SerializeField]
	private bool m_grabbleByPlayer;

	[SerializeField]
	private string m_changeDirectionAudioEvent;

	[SerializeField]
	private bool m_canCrit = true;

	[SerializeField]
	private bool m_flipOnReturn;

	[SerializeField]
	private GameObject m_critEffectGO;

	private float m_fireStartTime;

	private float m_maxSpeed;

	private float m_startingVelocity;

	private float m_pauseAtApexCounter;

	private float m_direction;

	private bool m_isReturning;

	private bool m_movingAlongYAxis;

	public string Description => ToString();

	private void OnEnable()
	{
		m_maxSpeed = base.SourceProjectile.ProjectileData.Speed;
		m_fireStartTime = Time.time;
		base.SourceProjectile.UpdateMovement();
		if (base.SourceProjectile.Velocity.x != 0f)
		{
			m_startingVelocity = base.SourceProjectile.Velocity.x;
			m_movingAlongYAxis = false;
		}
		else
		{
			m_startingVelocity = base.SourceProjectile.Velocity.y;
			m_movingAlongYAxis = true;
		}
		m_direction = Mathf.Sign(m_startingVelocity);
		m_pauseAtApexCounter = m_pauseAtApexDelay;
		m_isReturning = false;
		base.SourceProjectile.TurnSpeed = 0f;
		if ((bool)m_critEffectGO)
		{
			m_critEffectGO.SetActive(value: false);
		}
	}

	private void LateUpdate()
	{
		if (!(Time.time > m_fireStartTime + m_returnDelay))
		{
			return;
		}
		if (!m_returnToPlayer)
		{
			base.SourceProjectile.Speed -= m_returnSpeed * Time.deltaTime;
			if (base.SourceProjectile.Speed < 0f - m_maxSpeed)
			{
				base.SourceProjectile.Speed = 0f - m_maxSpeed;
			}
		}
		else if (!m_isReturning)
		{
			base.SourceProjectile.Speed -= m_returnSpeed * Time.deltaTime;
			if (base.SourceProjectile.Speed < 0f - m_maxSpeed)
			{
				base.SourceProjectile.Speed = 0f - m_maxSpeed;
			}
		}
		else
		{
			base.SourceProjectile.Speed += m_returnSpeed * Time.deltaTime;
			if (base.SourceProjectile.Speed > m_maxSpeed)
			{
				base.SourceProjectile.Speed = m_maxSpeed;
			}
		}
		float num = (m_movingAlongYAxis ? Mathf.Sign(base.SourceProjectile.Velocity.y) : Mathf.Sign(base.SourceProjectile.Velocity.x));
		if (m_direction != num && !m_isReturning)
		{
			m_isReturning = true;
			m_direction = num;
			if (m_flipOnReturn)
			{
				base.SourceProjectile.Flip();
			}
			if (m_returnToPlayer)
			{
				PlayerController playerController = PlayerManager.GetPlayerController();
				base.SourceProjectile.Heading = playerController.Midpoint - base.SourceProjectile.Midpoint;
				base.SourceProjectile.TurnSpeed = 200f;
			}
			if (m_grabbleByPlayer)
			{
				ChangeHitboxOnTerrainChange();
			}
			if (m_canCrit)
			{
				if (base.SourceProjectile.ActualCritChance < 100f)
				{
					base.SourceProjectile.ActualCritChance += 100f;
				}
				if ((bool)m_critEffectGO)
				{
					m_critEffectGO.SetActive(value: true);
				}
			}
			AudioManager.PlayOneShotAttached(this, m_changeDirectionAudioEvent, base.gameObject);
		}
		if (m_pauseAtApexCounter > 0f && m_isReturning)
		{
			m_pauseAtApexCounter -= Time.deltaTime;
			base.SourceProjectile.Speed = 0f;
		}
	}

	private void ChangeHitboxOnTerrainChange()
	{
		IHitboxController hitboxController = base.SourceProjectile.HitboxController;
		CollisionType terrainCollidesWithType = hitboxController.TerrainCollidesWithType;
		terrainCollidesWithType |= CollisionType.Player;
		terrainCollidesWithType |= CollisionType.Player_Dodging;
		hitboxController.ChangeCanCollideWith(HitboxType.Terrain, terrainCollidesWithType);
	}

	private void OnDisable()
	{
		if (m_grabbleByPlayer)
		{
			IHitboxController hitboxController = base.SourceProjectile.HitboxController;
			CollisionType terrainCollidesWithType = hitboxController.TerrainCollidesWithType;
			terrainCollidesWithType = terrainCollidesWithType & ~CollisionType.Player & ~CollisionType.Player_Dodging;
			hitboxController.ChangeCanCollideWith(HitboxType.Terrain, terrainCollidesWithType);
		}
		if ((bool)m_critEffectGO)
		{
			m_critEffectGO.SetActive(value: false);
		}
	}

	public void TerrainOnEnterHitResponse(IHitboxController otherHBController)
	{
		base.SourceProjectile.FlagForDestruction();
	}
}
