using System.Collections;
using MoreMountains.CorgiEngine;
using UnityEngine;

public class NotMovingSlowGame_Trait : BaseTrait
{
	[SerializeField]
	private float m_timeScaleUpSpeed = 5f;

	[SerializeField]
	private float m_timeScaleDownSpeed = 5f;

	private PlayerController m_playerController;

	private bool m_isMoving;

	private float m_slowDownDelayCounter;

	public override TraitType TraitType => TraitType.NotMovingSlowGame;

	private IEnumerator Start()
	{
		yield return new WaitUntil(() => PlayerManager.IsInstantiated);
		m_playerController = PlayerManager.GetPlayerController();
	}

	private void OnDisable()
	{
		RLTimeScale.SetTimeScale(TimeScaleType.Game, 1f);
	}

	private void Update()
	{
		if (m_playerController != null && !m_playerController.IsDead && !GameManager.IsGamePaused && m_playerController.ConditionState != CharacterStates.CharacterConditions.Stunned)
		{
			if (m_playerController.Velocity != Vector2.zero || !m_playerController.IsGrounded || m_playerController.CastAbility.AnyAbilityInProgress || !m_playerController.Animator.GetCurrentAnimatorStateInfo(0).IsName("Idle"))
			{
				float max = 1f;
				float value = RLTimeScale.TimeScale + m_timeScaleUpSpeed * Time.unscaledDeltaTime;
				RLTimeScale.SetTimeScale(TimeScaleType.Game, Mathf.Clamp(value, 0.01f, max));
			}
			else
			{
				float value2 = RLTimeScale.TimeScale - m_timeScaleDownSpeed * Time.unscaledDeltaTime;
				RLTimeScale.SetTimeScale(TimeScaleType.Game, Mathf.Clamp(value2, 0.01f, 1f));
			}
		}
	}
}
