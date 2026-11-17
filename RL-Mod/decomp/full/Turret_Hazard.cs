using System;
using UnityEngine;

public class Turret_Hazard : Hazard, IHasProjectileNameArray
{
	[Header("Projectile Setup")]
	[SerializeField]
	protected string m_projectileName;

	[SerializeField]
	protected Vector2 m_projectileOffset;

	[SerializeField]
	protected float m_startingProjectileAngle;

	[Header("Fire Rate Setup")]
	[SerializeField]
	protected bool m_useHalfLoopDelay = true;

	[SerializeField]
	protected float m_initialFireDelay;

	[SerializeField]
	protected TurretLogicType m_logicType;

	[SerializeField]
	[ConditionalHide("UseCustomLogic", true)]
	protected float m_loopFireDelay = 1f;

	[SerializeField]
	[ConditionalHide("UseCustomLogic", true)]
	protected float m_projectileSpeedMod = 1f;

	[SerializeField]
	protected bool m_forceActivate;

	protected WaitRL_Yield m_waitYield;

	protected float m_timePlayerEnteredRoom;

	protected bool m_tellPlayed;

	protected int m_timesFired;

	[NonSerialized]
	protected string[] m_projectileNameArray;

	public virtual string[] ProjectileNameArray
	{
		get
		{
			if (m_projectileNameArray == null)
			{
				m_projectileNameArray = new string[1] { m_projectileName };
			}
			return m_projectileNameArray;
		}
	}

	protected bool UseCustomLogic => m_logicType == TurretLogicType.Custom;

	public virtual float ProjectileSpeedMod => m_logicType switch
	{
		TurretLogicType.Normal => 10f, 
		TurretLogicType.Slow => 10f, 
		TurretLogicType.Fast => 10f, 
		_ => m_projectileSpeedMod, 
	};

	public virtual float LoopFireDelay => m_logicType switch
	{
		TurretLogicType.Normal => 2.5f, 
		TurretLogicType.Slow => 3.5f, 
		TurretLogicType.Fast => 1.5f, 
		_ => m_loopFireDelay, 
	};

	public float InitialFireDelay
	{
		get
		{
			if (m_useHalfLoopDelay)
			{
				return LoopFireDelay * 0.5f + m_initialFireDelay + 0.875f;
			}
			return m_initialFireDelay + 0.875f;
		}
	}

	public override float BaseDamage => 25f;

	public bool InAttackRange
	{
		get
		{
			if (m_forceActivate)
			{
				return true;
			}
			if (PlayerManager.IsInstantiated)
			{
				return Vector2.Distance(PlayerManager.GetPlayerController().Midpoint, base.transform.position) <= 55f * CameraController.ZoomLevel;
			}
			return false;
		}
	}

	protected override void Awake()
	{
		base.Awake();
		m_waitYield = new WaitRL_Yield(0f);
	}

	private void OnPlayerEnterRoom(object sender, RoomViaDoorEventArgs e)
	{
		if (base.isActiveAndEnabled)
		{
			m_timePlayerEnteredRoom = Time.time;
			ResetHazard();
			PrepFiringLogic();
		}
	}

	private void OnPlayerExitRoom(object sender, RoomViaDoorEventArgs e)
	{
		if (base.isActiveAndEnabled)
		{
			ResetHazard();
		}
	}

	protected virtual void PrepFiringLogic()
	{
		m_tellPlayed = false;
	}

	public override void Initialize(HazardArgs hazardArgs)
	{
		if (!(hazardArgs is TurretHazardArgs))
		{
			return;
		}
		TurretHazardArgs turretHazardArgs = hazardArgs as TurretHazardArgs;
		base.InitialState = turretHazardArgs.InitialState;
		switch (base.InitialState)
		{
		case StateID.One:
			m_forceActivate = false;
			break;
		case StateID.Two:
			m_forceActivate = true;
			break;
		case StateID.Random:
			if (CDGHelper.RandomPlusMinus() > 0)
			{
				m_forceActivate = true;
			}
			else
			{
				m_forceActivate = false;
			}
			break;
		}
		m_logicType = turretHazardArgs.LogicType;
		m_loopFireDelay = turretHazardArgs.LoopFireDelay;
		m_projectileSpeedMod = turretHazardArgs.ProjectileSpeedMod;
		m_useHalfLoopDelay = turretHazardArgs.UseHalfLoopDelay;
		m_initialFireDelay = turretHazardArgs.InitialFireDelay;
	}

	protected virtual void Update()
	{
		float num = m_timePlayerEnteredRoom + InitialFireDelay + (float)m_timesFired * LoopFireDelay;
		if (!(Time.time < num - 0.75f))
		{
			if (InAttackRange && !m_tellPlayed)
			{
				PlayTell();
			}
			if (!(Time.time < num))
			{
				FireProjectile();
			}
		}
	}

	protected virtual void PlayTell()
	{
		m_tellPlayed = true;
		base.Animator.SetTrigger("Tell");
	}

	protected virtual void FireProjectile()
	{
		if (InAttackRange)
		{
			float num = m_startingProjectileAngle + base.transform.localEulerAngles.z;
			Vector2 projectileOffset = m_projectileOffset;
			if (base.transform.localScale.x < 0f)
			{
				num += 180f;
				projectileOffset.x = 0f - projectileOffset.x;
			}
			if (base.transform.localScale.y < 0f)
			{
				num = 0f - num;
				projectileOffset.y = 0f - projectileOffset.y;
			}
			ProjectileManager.FireProjectile(base.gameObject, m_projectileName, projectileOffset, matchFacing: true, num, ProjectileSpeedMod);
			base.Animator.SetTrigger("Fire");
			base.Animator.ResetTrigger("Tell");
		}
		else
		{
			base.Animator.ResetTrigger("Fire");
			base.Animator.ResetTrigger("Tell");
			base.Animator.SetTrigger("Reset");
		}
		m_timesFired++;
		PrepFiringLogic();
	}

	private void OnEnable()
	{
		m_timePlayerEnteredRoom = Time.time;
		ResetHazard();
		PrepFiringLogic();
	}

	public override void ResetHazard()
	{
		m_tellPlayed = false;
		m_timesFired = 0;
		base.Animator.ResetTrigger("Fire");
		base.Animator.ResetTrigger("Tell");
		base.Animator.SetTrigger("Reset");
	}
}
