using System.Collections;
using System.Runtime.CompilerServices;
using UnityEngine;

public class ExhaustPoint_Hazard : Hazard, IPointHazard, IHazard, IRootObj
{
	[SerializeField]
	private SpriteRenderer m_exhaustPointWarningSprite;

	[SerializeField]
	private Animator m_exhaustPointWarningAnimator;

	private float m_radius;

	private float m_storedWarningScaleAmount;

	private bool m_inRange;

	private Coroutine m_increaseExhaustCoroutine;

	private Coroutine m_decreaseExhaustCoroutine;

	public override void Initialize(HazardArgs hazardArgs)
	{
		base.Initialize(hazardArgs);
		if (hazardArgs is PointHazardArgs pointHazardArgs)
		{
			m_radius = pointHazardArgs.Radius;
		}
		else
		{
			Debug.LogFormat("<color=red>| {0} | Failed to cast hazardArgs as PointHazardArgs. If you see this message please bug it on Pivotal.</color>");
		}
		m_exhaustPointWarningSprite.gameObject.transform.localScale = Vector3.one;
		float num = 4f;
		float num2 = m_radius * 2f * 1.75f;
		m_storedWarningScaleAmount = num2 / num;
		Vector3 localScale = new Vector3(m_storedWarningScaleAmount, m_storedWarningScaleAmount, m_storedWarningScaleAmount);
		m_exhaustPointWarningSprite.gameObject.transform.localScale = localScale;
		ResetHazard();
	}

	private void FixedUpdate()
	{
		if (!PlayerManager.IsInstantiated)
		{
			return;
		}
		if (CDGHelper.DistanceBetweenPts(PlayerManager.GetPlayerController().Midpoint, base.transform.localPosition) <= m_radius * 1.75f)
		{
			if (!m_inRange)
			{
				m_exhaustPointWarningAnimator.SetBool("Aware", value: true);
				if (m_increaseExhaustCoroutine != null)
				{
					StopCoroutine(m_increaseExhaustCoroutine);
				}
				if (m_decreaseExhaustCoroutine != null)
				{
					StopCoroutine(m_decreaseExhaustCoroutine);
				}
				m_increaseExhaustCoroutine = StartCoroutine(IncreaseExhaustCoroutine());
			}
			m_inRange = true;
		}
		else if (m_inRange)
		{
			m_exhaustPointWarningAnimator.SetBool("Aware", value: false);
			if (m_increaseExhaustCoroutine != null)
			{
				StopCoroutine(m_increaseExhaustCoroutine);
			}
			if (m_decreaseExhaustCoroutine != null)
			{
				StopCoroutine(m_decreaseExhaustCoroutine);
			}
			m_decreaseExhaustCoroutine = StartCoroutine(DecreaseExhaustCoroutine());
			m_inRange = false;
		}
	}

	private IEnumerator IncreaseExhaustCoroutine()
	{
		float intervalDuration = 0.01f;
		PlayerController playerController = PlayerManager.GetPlayerController();
		while (playerController.CurrentExhaust < 20)
		{
			playerController.CurrentExhaust++;
			Messenger<GameMessenger, GameEvent>.Broadcast(GameEvent.PlayerExhaustChange, null, null);
			float delay = Time.time + intervalDuration;
			while (Time.time < delay)
			{
				yield return null;
			}
		}
	}

	private IEnumerator DecreaseExhaustCoroutine()
	{
		float intervalDuration = 0.01f;
		PlayerController playerController = PlayerManager.GetPlayerController();
		while (playerController.CurrentExhaust > 0)
		{
			playerController.CurrentExhaust--;
			Messenger<GameMessenger, GameEvent>.Broadcast(GameEvent.PlayerExhaustChange, null, null);
			float delay = Time.time + intervalDuration;
			while (Time.time < delay)
			{
				yield return null;
			}
		}
	}

	public override void ResetHazard()
	{
		m_inRange = false;
		Vector3 localScale = new Vector3(m_storedWarningScaleAmount, m_storedWarningScaleAmount, m_storedWarningScaleAmount);
		m_exhaustPointWarningSprite.gameObject.transform.localScale = localScale;
	}

	[SpecialName]
	GameObject IRootObj.get_gameObject()
	{
		return base.gameObject;
	}
}
