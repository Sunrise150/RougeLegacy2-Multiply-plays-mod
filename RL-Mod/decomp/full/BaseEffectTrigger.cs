using UnityEngine;

public abstract class BaseEffectTrigger : MonoBehaviour
{
	[SerializeField]
	private CollisionType m_canCollideWith = CollisionType.Player | CollisionType.PlayerProjectile | CollisionType.Player_Dodging;

	[SerializeField]
	private EffectTriggerEntry[] m_effectList = new EffectTriggerEntry[0];

	protected GameObject m_rootObj;

	public EffectTriggerEntry[] TriggerArray
	{
		get
		{
			return m_effectList;
		}
		set
		{
			m_effectList = value;
		}
	}

	public abstract bool RequiresCollider { get; }

	public CollisionType CanCollideWith
	{
		get
		{
			return m_canCollideWith;
		}
		set
		{
			m_canCollideWith = value;
		}
	}

	public abstract Vector3 Midpoint { get; }

	protected virtual void Awake()
	{
		m_rootObj = this.GetRoot();
	}
}
