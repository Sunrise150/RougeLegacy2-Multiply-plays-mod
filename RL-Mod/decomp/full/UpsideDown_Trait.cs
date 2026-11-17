using UnityEngine;

public class UpsideDown_Trait : BaseTrait
{
	private Vector3 m_storedDeathDefyPos;

	public override TraitType TraitType => TraitType.UpsideDown;

	public override void DisableOnDeath()
	{
		base.DisableOnDeath();
		PlayerController playerController = PlayerManager.GetPlayerController();
		Vector3 midpoint = playerController.Midpoint;
		float num = CameraController.GameCamera.transform.position.y - midpoint.y;
		midpoint.y += num * 2f;
		playerController.transform.position = midpoint;
	}

	public void ApplyDeathDefy(bool reset)
	{
		if (!reset)
		{
			PlayerController playerController = PlayerManager.GetPlayerController();
			m_storedDeathDefyPos = playerController.transform.position;
			Vector3 midpoint = playerController.Midpoint;
			float num = CameraController.GameCamera.transform.position.y - midpoint.y;
			midpoint.y += num * 2f;
			playerController.transform.position = midpoint;
		}
		else
		{
			PlayerManager.GetPlayerController().transform.position = m_storedDeathDefyPos;
		}
	}
}
