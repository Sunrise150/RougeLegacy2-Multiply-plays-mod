using System.Collections;
using UnityEngine;

public class WaitUntilAnimChanged_Yield : IEnumerator
{
	private Animator m_animator;

	private string m_animatorBoolName;

	private int m_animationLayer;

	private bool m_needsReset;

	public object Current => null;

	public WaitUntilAnimChanged_Yield(Animator animator, string animatorBoolName, AnimationLayer layer = AnimationLayer.Base)
	{
		CreateNew(animator, animatorBoolName, layer);
	}

	public bool MoveNext()
	{
		if (m_needsReset)
		{
			CreateNew(m_animator, m_animatorBoolName, (AnimationLayer)m_animationLayer);
		}
		if (m_animator.GetCurrentAnimatorStateInfo(m_animationLayer).IsName(m_animatorBoolName))
		{
			m_needsReset = true;
			return false;
		}
		return true;
	}

	public void CreateNew(Animator animator, string animatorBoolName, AnimationLayer layer)
	{
		m_needsReset = false;
		m_animator = animator;
		m_animatorBoolName = animatorBoolName;
		m_animationLayer = (int)layer;
	}

	public void Reset()
	{
		CreateNew(m_animator, m_animatorBoolName, (AnimationLayer)m_animationLayer);
	}
}
