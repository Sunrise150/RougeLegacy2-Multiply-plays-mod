using System;
using System.Runtime.CompilerServices;
using UnityEngine;

public class SentryWithIce_Hazard : Hazard, IPointHazard, IHazard, IRootObj, IHasProjectileNameArray
{
	private IHazard[] m_hazards = new IHazard[2];

	private Action<Sentry_Hazard, bool> m_onSentryStateChange;

	[NonSerialized]
	private string[] m_projectileNameArray;

	public string[] ProjectileNameArray
	{
		get
		{
			if (m_projectileNameArray == null)
			{
				m_projectileNameArray = new string[1] { "SentryBounceBoltProjectile" };
			}
			return m_projectileNameArray;
		}
	}

	protected override void Awake()
	{
		base.Awake();
		m_onSentryStateChange = OnSentryStateChange;
	}

	public override void Initialize(HazardArgs hazardArgs)
	{
		for (int i = 0; i < m_hazards.Length; i++)
		{
			switch (i)
			{
			case 0:
				m_hazards[i] = HazardManager.GetHazard(HazardType.Sentry);
				(m_hazards[i] as Sentry_Hazard).SentryRestingRelay.AddListener(m_onSentryStateChange);
				break;
			case 1:
				m_hazards[i] = HazardManager.GetHazard(HazardType.IceCrystal);
				break;
			}
			m_hazards[i].gameObject.SetActive(value: true);
			m_hazards[i].gameObject.transform.SetPositionAndRotation(base.transform.position, base.transform.rotation);
			m_hazards[i].Initialize(hazardArgs);
			m_hazards[i].ResetHazard();
			(m_hazards[i] as Hazard).SetRoom(base.Room);
		}
	}

	private void OnSentryStateChange(Sentry_Hazard hazard, bool resting)
	{
		(m_hazards[1] as IceCrystal_Hazard).SetIceCrystalPaused(resting);
	}

	public override void ResetHazard()
	{
		for (int i = 0; i < m_hazards.Length; i++)
		{
			m_hazards[i].ResetHazard();
		}
	}

	public override void SetIsCulled(bool culled)
	{
		base.SetIsCulled(culled);
		for (int i = 0; i < m_hazards.Length; i++)
		{
			m_hazards[i].SetIsCulled(culled);
		}
	}

	protected override void OnDisable()
	{
		for (int i = 0; i < m_hazards.Length; i++)
		{
			if (!m_hazards[i].IsNativeNull())
			{
				if (i == 0)
				{
					(m_hazards[i] as Sentry_Hazard).SentryRestingRelay.RemoveListener(m_onSentryStateChange);
				}
				if ((bool)m_hazards[i].gameObject)
				{
					m_hazards[i].gameObject.SetActive(value: false);
				}
			}
		}
		base.OnDisable();
	}

	[SpecialName]
	GameObject IRootObj.get_gameObject()
	{
		return base.gameObject;
	}
}
