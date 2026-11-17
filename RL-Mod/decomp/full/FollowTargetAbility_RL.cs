using System.Collections;
using MoreMountains.CorgiEngine;
using Rewired;
using UnityEngine;

public class FollowTargetAbility_RL : BaseAbility_RL
{
	[SerializeField]
	protected float m_gravityReductionModWhenAiming = 1f;

	[SerializeField]
	protected GameObject m_targetGO;

	[SerializeField]
	protected bool m_targetAlwaysVisible;

	protected float m_targetMoveSpeed = 20f;

	protected float m_targetTurnSpeed = 9999f;

	protected bool m_isFollowing;

	protected bool m_isHovering;

	protected float m_storedFallMultiplier;

	protected float m_orientation;

	protected virtual AbilityAnimState StateToHoldAttackOn => AbilityAnimState.Attack_Intro;

	public override void Initialize(CastAbility_RL abilityController, CastAbilityType castAbilityType)
	{
		base.Initialize(abilityController, castAbilityType);
		if (m_targetAlwaysVisible)
		{
			m_targetGO.SetActive(value: true);
		}
		else
		{
			m_targetGO.SetActive(value: false);
		}
	}

	public override void PreCastAbility()
	{
		base.PreCastAbility();
		if (!m_targetGO.activeSelf)
		{
			m_targetGO.SetActive(value: true);
		}
		m_isHovering = false;
		m_abilityController.PlayerController.ConditionState = CharacterStates.CharacterConditions.DisableHorizontalMovement;
		if (m_abilityController.PlayerController.IsFacingRight)
		{
			m_orientation = 1f;
		}
		else
		{
			m_orientation = -1f;
		}
		m_storedFallMultiplier = m_abilityController.PlayerController.FallMultiplierOverride;
		Vector2 vector = (Vector2)m_abilityController.PlayerController.Midpoint + m_projectileOffset;
		m_targetGO.transform.position = vector;
		if (m_gravityReductionModWhenAiming < 1f)
		{
			m_abilityController.PlayerController.CharacterJump.ResetBrakeForce();
			m_abilityController.PlayerController.SetVelocity(0f, 0f, additive: false);
			m_abilityController.PlayerController.FallMultiplierOverride = m_gravityReductionModWhenAiming;
			if (!m_abilityController.PlayerController.IsGrounded)
			{
				m_animator.SetBool("Hover", value: true);
				m_isHovering = true;
			}
		}
	}

	protected override IEnumerator ChangeAnim(float duration)
	{
		if (duration <= 0f)
		{
			yield return null;
		}
		while (duration > 0f)
		{
			duration -= Time.deltaTime;
			yield return null;
		}
		while (base.CurrentAbilityAnimState == StateToHoldAttackOn && Rewired_RL.Player.GetButton(m_abilityController.GetAbilityInputString(base.CastAbilityType)))
		{
			yield return null;
		}
		m_animator.SetTrigger("Change_Ability_Anim");
		PerformTurnAnimCheck();
	}

	protected override void FireProjectile()
	{
		m_abilityController.PlayerController.FallMultiplierOverride = m_storedFallMultiplier;
		m_abilityController.PlayerController.ConditionState = CharacterStates.CharacterConditions.Normal;
		base.FireProjectile();
	}

	protected override void Update()
	{
		base.Update();
		if (!base.AbilityActive)
		{
			if (!m_abilityController.OnPauseResetInput && m_abilityController.PlayerController.CurrentMana >= (float)base.ActualCost && (base.CurrentAmmo > 0 || base.MaxAmmo == 0) && base.CooldownTimer <= 0f && m_abilityController.PlayerController.CharacterCorgi.REPlayer.GetButton(m_abilityController.GetAbilityInputString(base.CastAbilityType)))
			{
				m_abilityController.StartAbility(base.CastAbilityType);
			}
			return;
		}
		m_isFollowing = false;
		if (base.CurrentAbilityAnimState == StateToHoldAttackOn && Rewired_RL.Player.GetButton(m_abilityController.GetAbilityInputString(base.CastAbilityType)))
		{
			UpdateFollow();
			ConstrainTargetToRoom();
			m_isFollowing = true;
		}
		if (m_isHovering && m_abilityController.PlayerController.IsGrounded)
		{
			m_isHovering = false;
			m_animator.SetBool("Hover", value: false);
		}
		else if (!m_isHovering && !m_abilityController.PlayerController.IsGrounded)
		{
			m_isHovering = true;
			m_animator.SetBool("Hover", value: true);
		}
	}

	protected virtual void UpdateFollow()
	{
		ControllerType lastActiveControllerType = ReInput.controllers.GetLastActiveControllerType();
		float num = 0f;
		float num2 = 0f;
		if (SaveManager.ConfigData.ToggleMouseAiming && (lastActiveControllerType == ControllerType.Mouse || lastActiveControllerType == ControllerType.Keyboard))
		{
			Vector3 position = m_targetGO.transform.position;
			Vector2 vector = CameraController.GameCamera.ScreenToWorldPoint(ReInput.controllers.Mouse.screenPosition);
			if (CDGHelper.DistanceBetweenPts(vector, position) > 0.15f)
			{
				m_orientation = CDGHelper.TurnToFaceRadians(position, vector, CDGHelper.ToRadians(m_targetTurnSpeed), m_orientation, Time.deltaTime, convertToHalfRadius: false);
				num = Mathf.Sin(m_orientation);
				num2 = Mathf.Cos(m_orientation);
			}
			else
			{
				m_targetGO.transform.position = vector;
			}
		}
		else if (lastActiveControllerType == ControllerType.Joystick || !SaveManager.ConfigData.ToggleMouseAiming)
		{
			num = Rewired_RL.Player.GetAxis("MoveVertical");
			num2 = Rewired_RL.Player.GetAxis("MoveHorizontal");
		}
		if (num != 0f || num2 != 0f)
		{
			Vector3 position2 = m_targetGO.transform.position;
			position2.x += num2 * m_targetMoveSpeed * Time.deltaTime;
			position2.y += num * m_targetMoveSpeed * Time.deltaTime;
			m_targetGO.transform.position = position2;
		}
	}

	public void ConstrainTargetToRoom()
	{
		BaseRoom currentPlayerRoom = PlayerManager.GetCurrentPlayerRoom();
		if (!currentPlayerRoom.IsNativeNull())
		{
			Rect boundsRect = currentPlayerRoom.BoundsRect;
			Vector2 vector = m_targetGO.transform.position;
			if (vector.x < boundsRect.xMin)
			{
				vector.x = boundsRect.xMin;
			}
			else if (vector.x > boundsRect.xMax)
			{
				vector.x = boundsRect.xMax;
			}
			if (vector.y < boundsRect.yMin)
			{
				vector.y = boundsRect.yMin;
			}
			else if (vector.y > boundsRect.yMax)
			{
				vector.y = boundsRect.yMax;
			}
			m_targetGO.transform.position = vector;
		}
	}

	public override void StopAbility(bool abilityInterrupted)
	{
		base.StopAbility(abilityInterrupted);
		if (!m_targetAlwaysVisible && m_targetGO.activeSelf)
		{
			m_targetGO.SetActive(value: false);
		}
		if (m_gravityReductionModWhenAiming < 1f)
		{
			m_abilityController.PlayerController.FallMultiplierOverride = m_storedFallMultiplier;
		}
		if (m_abilityController.PlayerController.ConditionState != CharacterStates.CharacterConditions.Stunned)
		{
			m_abilityController.PlayerController.ConditionState = CharacterStates.CharacterConditions.Normal;
		}
		m_animator.SetBool("Hover", value: false);
	}
}
