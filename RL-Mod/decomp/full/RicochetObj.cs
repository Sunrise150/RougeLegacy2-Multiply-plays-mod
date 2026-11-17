using System.Runtime.CompilerServices;
using UnityEngine;

public class RicochetObj : MonoBehaviour, IRicochetObj, IRootObj
{
	[SerializeField]
	private Vector2 m_externalRicochetKnockbackAmount = Player_EV.PLAYER_BASE_KNOCKBACK_DISTANCE;

	public Vector2 ExternalRicochetKnockbackAmount
	{
		get
		{
			return m_externalRicochetKnockbackAmount;
		}
		set
		{
			m_externalRicochetKnockbackAmount = value;
		}
	}

	[SpecialName]
	GameObject IRootObj.get_gameObject()
	{
		return base.gameObject;
	}
}
