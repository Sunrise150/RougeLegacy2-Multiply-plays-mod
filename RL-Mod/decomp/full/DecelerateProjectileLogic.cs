using UnityEngine;

public class DecelerateProjectileLogic : BaseProjectileLogic
{
	[SerializeField]
	private float m_returnDelay = 0.15f;

	[SerializeField]
	private float m_decelerationSpeed = 15f;

	private float m_firedDelay;

	public string Description => ToString();

	private void OnEnable()
	{
		m_firedDelay = Time.time + m_returnDelay;
		base.SourceProjectile.UpdateMovement();
		base.SourceProjectile.TurnSpeed = 0f;
	}

	private void LateUpdate()
	{
		if (Time.time > m_firedDelay)
		{
			base.SourceProjectile.Speed -= m_decelerationSpeed * Time.deltaTime;
			if (base.SourceProjectile.Speed < 0f)
			{
				base.SourceProjectile.Speed = 0f;
			}
		}
	}
}
