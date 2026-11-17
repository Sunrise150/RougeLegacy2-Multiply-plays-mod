using System.Runtime.CompilerServices;
using UnityEngine;

public class DownstrikeKnockbackOverride : MonoBehaviour, IRootObj
{
	[SerializeField]
	private float m_knockbackOverride;

	public float Value => m_knockbackOverride;

	[SpecialName]
	GameObject IRootObj.get_gameObject()
	{
		return base.gameObject;
	}
}
