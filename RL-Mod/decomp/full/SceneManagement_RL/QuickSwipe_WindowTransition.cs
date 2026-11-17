using System.Collections;
using UnityEngine;

namespace SceneManagement_RL;

public class QuickSwipe_WindowTransition : Transition_V2, ISceneLoadingTransition, ITransition
{
	private Animator m_animator;

	private WaitRL_Yield m_waitRL;

	public override TransitionID ID => TransitionID.QuickSwipe;

	protected override void Awake()
	{
		base.Awake();
		m_animator = GetComponent<Animator>();
		m_waitRL = new WaitRL_Yield(0f, useUnscaledTime: true);
	}

	public IEnumerator TransitionIn()
	{
		m_animator.SetBool("Covered", value: true);
		yield return null;
		AnimatorStateInfo currentAnimatorStateInfo = m_animator.GetCurrentAnimatorStateInfo(0);
		float waitTime = currentAnimatorStateInfo.length * Mathf.Abs(currentAnimatorStateInfo.speed) * currentAnimatorStateInfo.speedMultiplier;
		m_waitRL.CreateNew(waitTime, useUnscaledTime: true);
		yield return m_waitRL;
	}

	public IEnumerator TransitionOut()
	{
		yield return null;
		m_animator.SetBool("Covered", value: false);
		yield return null;
		AnimatorStateInfo currentAnimatorStateInfo = m_animator.GetCurrentAnimatorStateInfo(0);
		float waitTime = currentAnimatorStateInfo.length * Mathf.Abs(currentAnimatorStateInfo.speed) * currentAnimatorStateInfo.speedMultiplier;
		m_waitRL.CreateNew(waitTime, useUnscaledTime: true);
		yield return m_waitRL;
	}

	public override IEnumerator Run()
	{
		yield break;
	}
}
