using System.Runtime.CompilerServices;
using UnityEngine;

public abstract class Hazard : MonoBehaviour, IHazard, IRootObj, IDamageObj, IGenericPoolObj, IPlayHitEffect, IRoomConsumer, ILevelConsumer
{
	[SerializeField]
	private Vector2 m_knockbackMod = new Vector2(1f, 1f);

	[SerializeField]
	private float m_knockbackStrength = 99f;

	[SerializeField]
	protected GameObject m_pivot;

	private float m_baseStunStrength;

	protected StateID m_initialState;

	private Animator m_animator;

	private int level;

	public bool IsFreePoolObj { get; set; }

	public bool IsAwakeCalled { get; protected set; }

	public Animator Animator => m_animator;

	public StatusEffectType[] StatusEffectTypes => null;

	public float[] StatusEffectDurations => null;

	public bool IsDotDamage => false;

	public virtual bool PlayHitEffect => true;

	public virtual bool PlayDirectionalHitEffect => false;

	public string EffectNameOverride => null;

	public float ActualCritChance => 0f;

	public float ActualCritDamage => 0f;

	public virtual float ActualDamage => Hazard_EV.GetDamageAmount(Room);

	public float ActualKnockbackStrength => BaseKnockbackStrength;

	public float ActualStunStrength => BaseStunStrength;

	public virtual float BaseDamage => ActualDamage;

	public float BaseStunStrength
	{
		get
		{
			return m_baseStunStrength;
		}
		set
		{
			m_baseStunStrength = value;
		}
	}

	public float BaseKnockbackStrength
	{
		get
		{
			return m_knockbackStrength;
		}
		set
		{
			m_knockbackStrength = value;
		}
	}

	public Vector2 ExternalKnockbackMod => m_knockbackMod;

	public StateID InitialState
	{
		get
		{
			return m_initialState;
		}
		protected set
		{
			m_initialState = value;
		}
	}

	public float KnockbackStrength => m_knockbackStrength;

	public StrikeType StrikeType => StrikeType.Blunt;

	public BaseRoom Room { get; private set; }

	public int Level => level;

	public void SetLevel(int value)
	{
		level = value;
	}

	protected virtual void Awake()
	{
		m_animator = GetComponentInChildren<Animator>();
		IsAwakeCalled = true;
	}

	public virtual void Initialize(HazardArgs hazardArgs)
	{
		InitialState = hazardArgs.InitialState;
	}

	public abstract void ResetHazard();

	public void ResetValues()
	{
		ResetHazard();
	}

	public virtual void SetRoom(BaseRoom room)
	{
		Room = room;
		SetLevel(room.Level);
	}

	public virtual void SetIsCulled(bool culled)
	{
		if (!this)
		{
			return;
		}
		if (!culled)
		{
			if ((bool)m_animator)
			{
				m_animator.enabled = true;
			}
			if ((bool)m_pivot)
			{
				m_pivot.SetActive(value: true);
			}
		}
		else
		{
			if ((bool)m_animator)
			{
				m_animator.enabled = false;
			}
			if ((bool)m_pivot)
			{
				m_pivot.SetActive(value: false);
			}
		}
	}

	protected virtual void OnDisable()
	{
		DisablePooledObjectManager.DisablePooledObject(this);
	}

	[SpecialName]
	GameObject IDamageObj.get_gameObject()
	{
		return base.gameObject;
	}

	[SpecialName]
	GameObject IRootObj.get_gameObject()
	{
		return base.gameObject;
	}

	[SpecialName]
	GameObject IGenericPoolObj.get_gameObject()
	{
		return base.gameObject;
	}
}
