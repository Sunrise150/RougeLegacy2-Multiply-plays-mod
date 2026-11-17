using UnityEngine;

public class RandomizeParamAnimBehaviour : StateMachineBehaviour
{
	[SerializeField]
	private string m_paramName;

	[SerializeField]
	private float m_randomMin;

	[SerializeField]
	private float m_randomMax = 1f;

	[SerializeField]
	private bool m_randomizeEveryOnEnter = true;

	[SerializeField]
	private bool m_randomizeEveryAnimLoop;

	private bool m_randomNumCached;

	private float m_randomNum;

	private int m_currentLoop;

	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		m_currentLoop = 0;
		if (m_randomizeEveryOnEnter)
		{
			m_randomNum = Random.Range(m_randomMin, m_randomMax);
		}
		else if (!m_randomNumCached)
		{
			m_randomNumCached = true;
			m_randomNum = Random.Range(m_randomMin, m_randomMax);
		}
		animator.SetFloat(m_paramName, m_randomNum);
	}

	public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		if (m_randomizeEveryAnimLoop && m_currentLoop < (int)stateInfo.normalizedTime)
		{
			m_currentLoop = (int)stateInfo.normalizedTime;
			m_randomNum = Random.Range(m_randomMin, m_randomMax);
		}
	}
}
