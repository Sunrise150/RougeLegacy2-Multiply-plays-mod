using System.Collections;
using UnityEngine;

public class GenericEffect : BaseEffect
{
	[SerializeField]
	private bool m_useUnscaledTime;

	private ParticleSystem m_particleSystem;

	private Animator m_animator;

	private WaitRL_Yield m_waitYield;

	private bool m_hasStopParam;

	private AnimatorUpdateMode m_storedUpdateMode;

	private bool m_stoppingParticleSystem;

	private bool m_stoppingAnimator;

	private bool HasParticleSystem => m_particleSystem;

	private bool HasAnimator => m_animator;

	public Animator Animator => m_animator;

	public ParticleSystem ParticleSystem => m_particleSystem;

	protected override void Awake()
	{
		base.Awake();
		m_particleSystem = GetComponentInChildren<ParticleSystem>();
		m_animator = GetComponentInChildren<Animator>();
		if ((bool)m_animator)
		{
			m_hasStopParam = AnimatorUtility.HasParameter(m_animator, "Stop");
			m_storedUpdateMode = m_animator.updateMode;
		}
		m_waitYield = new WaitRL_Yield(0f);
	}

	public override void Play(float duration = 0f, EffectStopType stopType = EffectStopType.Gracefully)
	{
		base.Play(duration, stopType);
		if (HasParticleSystem)
		{
			StartCoroutine(PlayTimedParticleSystem(duration, stopType));
		}
		if (HasAnimator)
		{
			m_animator.enabled = true;
			StartCoroutine(PlayTimedAnimator(duration, stopType));
		}
		if (!HasParticleSystem && !HasAnimator)
		{
			StartCoroutine(PlayTimer(duration, stopType));
		}
	}

	private IEnumerator PlayTimer(float duration, EffectStopType stopType)
	{
		if (duration > 0f)
		{
			m_waitYield.CreateNew(duration, m_useUnscaledTime);
			yield return m_waitYield;
			PlayComplete();
			yield break;
		}
		while (true)
		{
			yield return null;
		}
	}

	private IEnumerator PlayTimedAnimator(float duration, EffectStopType stopType)
	{
		if (!m_hasStopParam && duration == 0f && stopType == EffectStopType.Immediate)
		{
			Debug.LogWarning("<color=yellow>WARNING: Playing effect with 0 duration and stopType Immediate will result in an immediate start and stoppage, and no visible effect will appear.</color>");
		}
		if (duration < 0f)
		{
			yield return null;
			if ((bool)base.SourceAnimator && duration == -1f)
			{
				int nameHash = base.SourceAnimator.GetCurrentAnimatorStateInfo(base.AnimatorLayer).shortNameHash;
				while ((bool)base.SourceAnimator && base.SourceAnimator.gameObject.activeInHierarchy && base.SourceAnimator.GetCurrentAnimatorStateInfo(base.AnimatorLayer).shortNameHash == nameHash)
				{
					yield return null;
				}
			}
			else
			{
				while ((bool)base.Source && base.Source.gameObject.activeInHierarchy)
				{
					yield return null;
				}
			}
		}
		else if (duration > 0f)
		{
			m_waitYield.CreateNew(duration, m_useUnscaledTime);
			yield return m_waitYield;
		}
		if (stopType == EffectStopType.Gracefully)
		{
			if (m_hasStopParam)
			{
				m_animator.SetTrigger("Stop");
				yield return null;
			}
			AnimatorStateInfo currentAnimatorStateInfo = m_animator.GetCurrentAnimatorStateInfo(base.AnimatorLayer);
			float waitTime = currentAnimatorStateInfo.length * (1f - currentAnimatorStateInfo.normalizedTime);
			m_waitYield.CreateNew(waitTime, m_useUnscaledTime);
			yield return m_waitYield;
			PlayComplete();
		}
		else
		{
			PlayComplete();
		}
	}

	private IEnumerator PlayTimedParticleSystem(float duration, EffectStopType stopType)
	{
		m_particleSystem.Clear();
		m_particleSystem.Play(withChildren: true);
		if (duration < 0f)
		{
			if ((bool)base.Source)
			{
				yield return null;
				if ((bool)base.SourceAnimator && duration == -1f)
				{
					int nameHash = base.SourceAnimator.GetCurrentAnimatorStateInfo(base.AnimatorLayer).shortNameHash;
					while ((bool)base.SourceAnimator && base.SourceAnimator.gameObject.activeInHierarchy && base.SourceAnimator.GetCurrentAnimatorStateInfo(base.AnimatorLayer).shortNameHash == nameHash)
					{
						yield return null;
					}
				}
				else
				{
					while ((bool)base.Source && base.Source.gameObject.activeInHierarchy)
					{
						yield return null;
					}
				}
			}
		}
		else if (duration >= 0f)
		{
			if (duration == 0f)
			{
				duration = m_particleSystem.main.duration;
				if (duration == 0f)
				{
					duration = 0.05f;
				}
			}
			m_waitYield.CreateNew(duration, m_useUnscaledTime);
			yield return m_waitYield;
		}
		if (stopType == EffectStopType.Gracefully)
		{
			m_particleSystem.Stop(withChildren: true, ParticleSystemStopBehavior.StopEmitting);
		}
		else
		{
			m_particleSystem.Stop(withChildren: true, ParticleSystemStopBehavior.StopEmittingAndClear);
		}
		bool playedStopAgain = false;
		while (m_particleSystem.IsAlive(withChildren: true))
		{
			yield return null;
			if (!playedStopAgain)
			{
				if (stopType == EffectStopType.Gracefully)
				{
					m_particleSystem.Stop(withChildren: true, ParticleSystemStopBehavior.StopEmitting);
				}
				else
				{
					m_particleSystem.Stop(withChildren: true, ParticleSystemStopBehavior.StopEmittingAndClear);
				}
				playedStopAgain = true;
			}
		}
		if (!HasAnimator)
		{
			PlayComplete();
		}
	}

	protected override void PlayComplete()
	{
		base.PlayComplete();
		if ((bool)m_animator)
		{
			m_animator.updateMode = m_storedUpdateMode;
		}
	}

	public override void Stop(EffectStopType stopType)
	{
		StopAllCoroutines();
		if (!base.gameObject.activeSelf)
		{
			PlayComplete();
			return;
		}
		switch (stopType)
		{
		case EffectStopType.Gracefully:
			StartCoroutine(StopGracefullyCoroutine());
			break;
		case EffectStopType.Immediate:
			if (HasParticleSystem && m_particleSystem.isPlaying)
			{
				m_particleSystem.Stop(withChildren: true, ParticleSystemStopBehavior.StopEmittingAndClear);
			}
			PlayComplete();
			break;
		}
	}

	private IEnumerator StopGracefullyCoroutine()
	{
		if (HasParticleSystem && m_particleSystem.isPlaying)
		{
			m_stoppingParticleSystem = true;
			StartCoroutine(StopParticleSystemGracefullyCoroutine());
		}
		if (HasAnimator)
		{
			m_stoppingAnimator = true;
			StartCoroutine(StopAnimatorGracefullyCoroutine());
		}
		while (m_stoppingParticleSystem || m_stoppingAnimator)
		{
			yield return null;
		}
		PlayComplete();
	}

	private IEnumerator StopParticleSystemGracefullyCoroutine()
	{
		m_particleSystem.Stop(withChildren: true, ParticleSystemStopBehavior.StopEmitting);
		bool playedStopAgain = false;
		while (m_particleSystem.IsAlive(withChildren: true))
		{
			yield return null;
			if (!playedStopAgain)
			{
				m_particleSystem.Stop(withChildren: true, ParticleSystemStopBehavior.StopEmitting);
				playedStopAgain = true;
			}
		}
		m_stoppingParticleSystem = false;
	}

	private IEnumerator StopAnimatorGracefullyCoroutine()
	{
		if (m_hasStopParam)
		{
			m_animator.SetTrigger("Stop");
			yield return null;
		}
		AnimatorStateInfo currentAnimatorStateInfo = m_animator.GetCurrentAnimatorStateInfo(base.AnimatorLayer);
		float waitTime = currentAnimatorStateInfo.length * (1f - currentAnimatorStateInfo.normalizedTime);
		m_waitYield.CreateNew(waitTime, m_useUnscaledTime);
		yield return m_waitYield;
		m_stoppingAnimator = false;
		m_animator.enabled = false;
	}

	public override void ResetValues()
	{
		base.ResetValues();
		if ((bool)m_animator)
		{
			m_animator.updateMode = m_storedUpdateMode;
		}
	}
}
