using UnityEngine;

public class SetLegWeightAnimBehaviour : StateMachineBehaviour
{
	[SerializeField]
	private float m_movingOnGroundLegWeight;

	[SerializeField]
	private float m_movingInAirLegWeight;

	[SerializeField]
	private float m_stationaryOnGroundLegWeight = 1f;

	[SerializeField]
	private float m_stationaryInAirLegWeight;

	private static bool m_isMoving;

	private static bool m_isGrounded;

	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		if (PlayerManager.IsInstantiated)
		{
			PlayerController playerController = PlayerManager.GetPlayerController();
			if (playerController != null && playerController.IsInitialized)
			{
				UpdateLegLayerWeight(playerController, animator);
			}
		}
	}

	private void UpdateLegLayerWeight(PlayerController playerController, Animator animator)
	{
		m_isGrounded = playerController.IsGrounded;
		m_isMoving = playerController.Velocity.x != 0f;
		if (m_isGrounded)
		{
			if (m_isMoving)
			{
				animator.SetLayerWeight(2, m_movingOnGroundLegWeight);
			}
			else
			{
				animator.SetLayerWeight(2, m_stationaryOnGroundLegWeight);
			}
		}
		else if (m_isMoving)
		{
			animator.SetLayerWeight(2, m_movingInAirLegWeight);
		}
		else
		{
			animator.SetLayerWeight(2, m_stationaryInAirLegWeight);
		}
	}

	public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		if (!PlayerManager.IsInstantiated)
		{
			return;
		}
		PlayerController playerController = PlayerManager.GetPlayerController();
		if (playerController != null && playerController.IsInitialized)
		{
			bool isGrounded = playerController.IsGrounded;
			bool flag = playerController.Velocity.x != 0f;
			if (isGrounded != m_isGrounded || flag != m_isMoving)
			{
				UpdateLegLayerWeight(playerController, animator);
			}
		}
	}
}
