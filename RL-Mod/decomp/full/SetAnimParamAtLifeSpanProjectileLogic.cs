using System.Collections;
using UnityEngine;

public class SetAnimParamAtLifeSpanProjectileLogic : BaseProjectileLogic
{
	[SerializeField]
	private AnimatorControllerParameterType m_animParamType;

	[SerializeField]
	private float m_triggerAtNormalizedLifeSpan = 0.8f;

	[SerializeField]
	private string m_animParamName;

	[SerializeField]
	private float m_numValue;

	[SerializeField]
	private bool m_boolValue;

	private void OnEnable()
	{
		if ((bool)base.SourceProjectile)
		{
			StartCoroutine(FlashCoroutine());
		}
	}

	private IEnumerator FlashCoroutine()
	{
		float num = base.SourceProjectile.Lifespan * m_triggerAtNormalizedLifeSpan;
		float endTime = Time.time + num;
		while (Time.time < endTime)
		{
			yield return null;
		}
		if (!base.SourceProjectile)
		{
			yield break;
		}
		switch (m_animParamType)
		{
		case AnimatorControllerParameterType.Bool:
			base.SourceProjectile.Animator.SetBool(m_animParamName, m_boolValue);
			break;
		case AnimatorControllerParameterType.Trigger:
			if (m_boolValue)
			{
				base.SourceProjectile.Animator.SetTrigger(m_animParamName);
			}
			break;
		case AnimatorControllerParameterType.Int:
			base.SourceProjectile.Animator.SetInteger(m_animParamName, (int)m_numValue);
			break;
		case AnimatorControllerParameterType.Float:
			base.SourceProjectile.Animator.SetFloat(m_animParamName, m_numValue);
			break;
		}
	}
}
