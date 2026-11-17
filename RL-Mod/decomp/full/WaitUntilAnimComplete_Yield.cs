using System.Collections;
using UnityEngine;

internal class WaitUntilAnimComplete_Yield : IEnumerator
{
	private Animator m_animator;

	private int m_storedStateNameHash;

	private bool m_needsReset;

	private int m_animLayer;

	private bool m_clipIsEmpty;

	public object Current => null;

	public WaitUntilAnimComplete_Yield(Animator animator, int animLayer)
	{
		CreateNew(animator, animLayer);
	}

	public bool MoveNext()
	{
		if (m_needsReset)
		{
			CreateNew(m_animator, m_animLayer);
		}
		int num;
		if (!m_clipIsEmpty)
		{
			AnimatorStateInfo currentAnimatorStateInfo = m_animator.GetCurrentAnimatorStateInfo(m_animLayer);
			if (currentAnimatorStateInfo.normalizedTime < 1f)
			{
				num = ((currentAnimatorStateInfo.shortNameHash == m_storedStateNameHash) ? 1 : 0);
				if (num != 0)
				{
					goto IL_005e;
				}
			}
			else
			{
				num = 0;
			}
			m_needsReset = true;
			goto IL_005e;
		}
		m_needsReset = true;
		return false;
		IL_005e:
		return (byte)num != 0;
	}

	public void CreateNew(Animator animator, int animLayer)
	{
		m_needsReset = false;
		m_animator = animator;
		m_animLayer = animLayer;
		m_storedStateNameHash = m_animator.GetCurrentAnimatorStateInfo(m_animLayer).shortNameHash;
		m_clipIsEmpty = m_animator.GetCurrentAnimatorClipInfoCount(m_animLayer) <= 0;
	}

	public void Reset()
	{
		CreateNew(m_animator, m_animLayer);
	}
}
