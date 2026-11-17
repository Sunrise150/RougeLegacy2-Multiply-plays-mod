public class MushroomDrop : HealthDrop
{
	private const float MOVEMENT_SPEED = 5f;

	private bool m_moveRight;

	public override ItemDropType ItemDropType => ItemDropType.MushroomDrop;

	protected override void GainHealth(float hpGain = 0f)
	{
		base.GainHealth(hpGain);
		if (!PlayerManager.GetPlayerController().IsMushroomBig)
		{
			PlayerManager.GetPlayerController().SetMushroomBig(setMushroomBig: true, animate: true);
		}
	}

	public override void OnSpawnCollectCollisionCheck()
	{
		base.OnSpawnCollectCollisionCheck();
		m_moveRight = base.CorgiController.Velocity.x > 0f;
	}

	protected override void Update()
	{
		base.Update();
		if ((bool)base.CorgiController && base.CorgiController.State.IsGrounded && !base.Magnetized)
		{
			if ((base.CorgiController.State.IsCollidingLeft && !m_moveRight) || (base.CorgiController.State.IsCollidingRight && m_moveRight))
			{
				m_moveRight = !m_moveRight;
			}
			if (m_moveRight && base.CorgiController.Velocity.x != 5f)
			{
				base.CorgiController.SetHorizontalForce(5f);
			}
			else if (!m_moveRight && base.CorgiController.Velocity.x != -5f)
			{
				base.CorgiController.SetHorizontalForce(-5f);
			}
		}
	}
}
