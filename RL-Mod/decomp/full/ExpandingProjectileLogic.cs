using UnityEngine;

public class ExpandingProjectileLogic : BaseProjectileLogic
{
	[SerializeField]
	private float m_shoutScale = 10f;

	[SerializeField]
	private bool m_scaleRelative;

	private Tween m_scaleTween;

	private void OnEnable()
	{
		Vector3 vector = ((base.SourceProjectile.transform.parent != null) ? base.SourceProjectile.transform.parent.lossyScale : Vector3.one);
		float num = Mathf.Abs(m_shoutScale / vector.x);
		float num2 = Mathf.Abs(m_shoutScale / vector.y);
		if (!m_scaleRelative)
		{
			m_scaleTween = TweenManager.TweenTo(base.SourceProjectile.transform, base.SourceProjectile.Lifespan, Ease.None, "localScale.x", num, "localScale.y", num2);
		}
		else
		{
			m_scaleTween = TweenManager.TweenBy(base.SourceProjectile.transform, base.SourceProjectile.Lifespan, Ease.None, "localScale.x", num, "localScale.y", num2);
		}
	}

	private void OnDisable()
	{
		if ((bool)m_scaleTween)
		{
			m_scaleTween.StopTweenWithConditionChecks(runEndHandlers: false, base.SourceProjectile, "ExpandProjectile");
		}
	}
}
