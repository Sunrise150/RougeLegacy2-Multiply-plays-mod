using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class BurstEffect : BaseEffect
{
	[SerializeField]
	private Vector2 m_burstMinMaxMagnitude;

	[SerializeField]
	private Vector2 m_randDelayBetweenBursts;

	[SerializeField]
	private bool m_moveTowardsImmediately;

	[SerializeField]
	private UnityEvent m_effectCompleteUnityEvent;

	private BurstLogic[] m_particleArray;

	private WaitRL_Yield m_burstDelayYield;

	private Transform m_destinationOverride;

	private IMidpointObj m_destinationMidpointOverride;

	public Transform DestinationOverride
	{
		get
		{
			return m_destinationOverride;
		}
		set
		{
			m_destinationOverride = value;
			if (m_particleArray != null)
			{
				BurstLogic[] particleArray = m_particleArray;
				for (int i = 0; i < particleArray.Length; i++)
				{
					particleArray[i].DestinationOverride = m_destinationOverride;
				}
			}
		}
	}

	public IMidpointObj DestinationMidpointOverride
	{
		get
		{
			return m_destinationMidpointOverride;
		}
		set
		{
			m_destinationMidpointOverride = value;
			if (m_particleArray != null)
			{
				BurstLogic[] particleArray = m_particleArray;
				for (int i = 0; i < particleArray.Length; i++)
				{
					particleArray[i].DestinationMidpointOverride = m_destinationMidpointOverride;
				}
			}
		}
	}

	protected override void Awake()
	{
		base.Awake();
		m_particleArray = GetComponentsInChildren<BurstLogic>(includeInactive: true);
		BurstLogic[] particleArray = m_particleArray;
		for (int i = 0; i < particleArray.Length; i++)
		{
			particleArray[i].gameObject.SetActive(value: false);
		}
		m_burstDelayYield = new WaitRL_Yield(0f);
	}

	public override void Play(float duration = 0f, EffectStopType stopType = EffectStopType.Gracefully)
	{
		base.Play(duration, stopType);
		StartCoroutine(PlayBurstCoroutine());
	}

	private IEnumerator PlayBurstCoroutine()
	{
		Vector3 spawnPos = base.Source.transform.position;
		IMidpointObj component = base.Source.GetComponent<IMidpointObj>();
		if (component != null)
		{
			spawnPos = component.Midpoint;
		}
		BurstLogic[] particleArray = m_particleArray;
		foreach (BurstLogic burstLogic in particleArray)
		{
			burstLogic.transform.position = spawnPos;
			burstLogic.ClearTrail();
			burstLogic.gameObject.SetActive(value: true);
			if (!m_moveTowardsImmediately)
			{
				burstLogic.Burst(Random.Range(10, 20));
			}
			else
			{
				burstLogic.BurstTowards(Random.Range(10, 20));
			}
			if (m_randDelayBetweenBursts.y > 0f)
			{
				m_burstDelayYield.CreateNew(Random.Range(m_randDelayBetweenBursts.x, m_randDelayBetweenBursts.y));
				yield return m_burstDelayYield;
			}
		}
		while (!AllParticlesComplete())
		{
			yield return null;
		}
		Stop(EffectStopType.Gracefully);
	}

	private bool AllParticlesComplete()
	{
		BurstLogic[] particleArray = m_particleArray;
		for (int i = 0; i < particleArray.Length; i++)
		{
			if (particleArray[i].gameObject.activeSelf)
			{
				return false;
			}
		}
		return true;
	}

	public override void Stop(EffectStopType stopType)
	{
		BurstLogic[] particleArray = m_particleArray;
		foreach (BurstLogic burstLogic in particleArray)
		{
			if (burstLogic.gameObject.activeSelf)
			{
				burstLogic.gameObject.SetActive(value: false);
			}
		}
		DestinationOverride = null;
		DestinationMidpointOverride = null;
		if (m_effectCompleteUnityEvent != null)
		{
			m_effectCompleteUnityEvent.Invoke();
		}
		PlayComplete();
	}
}
