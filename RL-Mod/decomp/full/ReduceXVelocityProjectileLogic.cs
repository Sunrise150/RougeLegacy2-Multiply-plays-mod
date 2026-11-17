using UnityEngine;

public class ReduceXVelocityProjectileLogic : BaseProjectileLogic
{
	[SerializeField]
	private float m_xAccelerationReductionSpeed = 1f;

	private bool m_movingRight;

	private void OnEnable()
	{
		m_movingRight = base.SourceProjectile.Velocity.x > 0f;
	}

	private void LateUpdate()
	{
		if (m_movingRight)
		{
			if (base.SourceProjectile.Velocity.x > 0f)
			{
				base.SourceProjectile.HeadingX -= m_xAccelerationReductionSpeed * Time.deltaTime;
				base.SourceProjectile.SetCorgiVelocity(new Vector2(base.SourceProjectile.Velocity.x - m_xAccelerationReductionSpeed * Time.deltaTime, base.SourceProjectile.Velocity.y));
			}
			else
			{
				base.SourceProjectile.HeadingX = 0f;
				base.SourceProjectile.SetCorgiVelocity(new Vector2(0f, base.SourceProjectile.Velocity.y));
			}
		}
		else if (base.SourceProjectile.Velocity.x < 0f)
		{
			base.SourceProjectile.HeadingX += m_xAccelerationReductionSpeed * Time.deltaTime;
			base.SourceProjectile.SetCorgiVelocity(new Vector2(base.SourceProjectile.Velocity.x + m_xAccelerationReductionSpeed * Time.deltaTime, base.SourceProjectile.Velocity.y));
		}
		else
		{
			base.SourceProjectile.HeadingX = 0f;
			base.SourceProjectile.SetCorgiVelocity(new Vector2(0f, base.SourceProjectile.Velocity.y));
		}
	}
}
