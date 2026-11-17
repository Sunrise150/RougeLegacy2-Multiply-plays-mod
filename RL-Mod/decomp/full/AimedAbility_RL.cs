using System;
using System.Collections;
using MoreMountains.CorgiEngine;
using Rewired;
using Sigtrap.Relays;
using UnityEngine;

public class AimedAbility_RL : BaseAbility_RL
{
	[SerializeField]
	protected float m_gravityReductionModWhenAiming = 1f;

	[SerializeField]
	protected bool m_stopVelocityWhenAiming;

	[SerializeField]
	protected bool m_disableMovementWhileAiming;

	[SerializeField]
	private Vector2 m_pushbackAmount = Vector2.zero;

	[SerializeField]
	protected bool m_pushbackIgnoresDirection;

	[SerializeField]
	protected bool m_continueFireOnHold;

	[SerializeField]
	[ConditionalHide("m_continueFireOnHold", true)]
	protected bool m_keepFiringWhenEmpty;

	[SerializeField]
	protected string m_critProjectileName;

	[Header("Aim Line Settings")]
	[SerializeField]
	protected LineRenderer m_aimLine;

	[SerializeField]
	protected float m_aimLineLength = 10f;

	[SerializeField]
	[Tooltip("Calculate how fast the Player is adjusting their aim. This is mainly used for audio purposes")]
	protected float m_maxAimSpeedSFXDelta = 90f;

	[SerializeField]
	private bool m_useUnmoddedAngle;

	protected float m_aimAngle;

	protected float m_unmoddedAngle;

	protected float m_projSpeed;

	protected float m_projFallMultiplier;

	protected float m_projGravityKickInDelay;

	protected bool m_projIsWeighted;

	private float m_prevAngle;

	private bool m_prevIsFacingRight;

	protected bool m_isAiming;

	protected float m_storedFallMultiplier;

	protected bool m_isHovering;

	private bool m_isTimeSlow;

	protected Relay m_kickbackRelay = new Relay();

	protected Relay<float> m_aimSpeedChangeRelay = new Relay<float>();

	protected Relay m_switchSidesRelay = new Relay();

	protected Relay<Projectile_RL> m_fireProjectileRelay = new Relay<Projectile_RL>();

	public float AimAngle => m_aimAngle;

	public float UnmoddedAimAngle => m_unmoddedAngle;

	public override string ProjectileName
	{
		get
		{
			if (!IsInCritWindow || string.IsNullOrEmpty(m_critProjectileName))
			{
				return m_projectileName;
			}
			return m_critProjectileName;
		}
	}

	protected virtual AbilityAnimState StateToHoldAttackOn => AbilityAnimState.Attack_Intro;

	public virtual Vector2 PushbackAmount => m_pushbackAmount;

	public override bool IsAiming => m_isAiming;

	protected virtual bool CancelTimeSlowOnFire => true;

	public IRelayLink KickbackRelay => m_kickbackRelay.link;

	public IRelayLink<float> AimSpeedChangeRelay => m_aimSpeedChangeRelay.link;

	public IRelayLink SwitchSidesRelay => m_switchSidesRelay.link;

	public IRelayLink<Projectile_RL> FireProjectileRelay => m_fireProjectileRelay.link;

	protected override void InitializeProjectileNameArray()
	{
		if (!string.IsNullOrEmpty(m_critProjectileName))
		{
			m_projectileNameArray = new string[2] { m_critProjectileName, m_projectileName };
		}
		else
		{
			base.InitializeProjectileNameArray();
		}
	}

	public override void Initialize(CastAbility_RL abilityController, CastAbilityType castAbilityType)
	{
		base.Initialize(abilityController, castAbilityType);
		InitializeAimLine();
	}

	protected virtual void InitializeAimLine()
	{
		Projectile_RL projectile = ProjectileLibrary.GetProjectile(m_projectileName);
		if ((bool)projectile)
		{
			m_projFallMultiplier = projectile.FallMultiplierOverride;
			m_projGravityKickInDelay = projectile.GravityKickInDelay;
			m_projSpeed = projectile.ProjectileData.Speed;
			m_projIsWeighted = projectile.IsWeighted;
		}
		if ((bool)m_aimLine)
		{
			Color startColor = new Color(1f, 1f, 1f, 0.25f);
			m_aimLine.startColor = startColor;
			m_aimLine.gameObject.SetActive(value: false);
		}
	}

	public override void PreCastAbility()
	{
		base.PreCastAbility();
		m_aimAngle = 0f;
		m_unmoddedAngle = ((!m_abilityController.PlayerController.IsFacingRight) ? 180 : 0);
		m_animator.SetFloat("Attack_Direction", 2f);
		if ((bool)m_aimLine)
		{
			m_aimLine.transform.localScale = new Vector3(1f / m_abilityController.PlayerController.transform.lossyScale.x, 1f / m_abilityController.PlayerController.transform.lossyScale.y, 1f);
			float num = m_abilityController.PlayerController.transform.lossyScale.x / m_abilityController.PlayerController.BaseScaleToOffsetWith;
			Vector2 vector = ProjectileOffset * num;
			m_aimLine.transform.position = m_abilityController.transform.localPosition + (Vector3)vector;
			m_aimLine.gameObject.SetActive(value: true);
		}
		UpdateArrowAim();
		UpdateAimLine();
	}

	public override IEnumerator CastAbility()
	{
		if (!ReInput.isReady)
		{
			yield break;
		}
		if (m_stopVelocityWhenAiming)
		{
			m_abilityController.PlayerController.CharacterJump.ResetBrakeForce();
			m_abilityController.PlayerController.SetVelocity(0f, 0f, additive: false);
		}
		if (m_disableMovementWhileAiming)
		{
			m_abilityController.PlayerController.ConditionState = CharacterStates.CharacterConditions.DisableHorizontalMovement;
		}
		m_storedFallMultiplier = m_abilityController.PlayerController.FallMultiplierOverride;
		if (m_gravityReductionModWhenAiming < 1f)
		{
			m_abilityController.PlayerController.FallMultiplierOverride = m_gravityReductionModWhenAiming;
			m_isHovering = false;
			if (!m_abilityController.PlayerController.IsGrounded && m_abilityController.PlayerController.MovementState != CharacterStates.MovementStates.Dashing)
			{
				m_animator.SetBool("Hover", value: true);
				m_isHovering = true;
			}
		}
		if (SaveManager.PlayerSaveData.EnableHouseRules && SaveManager.PlayerSaveData.Assist_AimTimeSlow < 1f)
		{
			RLTimeScale.SetTimeScale(TimeScaleType.AimTimeSlowAssist, SaveManager.PlayerSaveData.Assist_AimTimeSlow);
			m_isTimeSlow = true;
		}
		yield return base.CastAbility();
	}

	protected override void EnterAnimationState(AbilityAnimState animState)
	{
		if (animState == AbilityAnimState.Attack_Intro)
		{
			base.CurrentAbilityAnimState = animState;
			OnEnterAttackIntroLogic();
			m_animator.SetFloat("Ability_Anim_Speed", AttackIntroAnimSpeed);
			float duration = CalculateTotalAnimationDelay();
			m_changeAnimCoroutine = StartCoroutine(ChangeAnim(duration));
		}
		else
		{
			base.EnterAnimationState(animState);
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
		while (base.CurrentAbilityAnimState == StateToHoldAttackOn && Rewired_RL.Player.GetButton(m_abilityController.GetAbilityInputString(base.CastAbilityType)) && !m_continueFireOnHold)
		{
			yield return null;
		}
		if (m_continueFireOnHold && (base.CurrentAmmo > 0 || m_keepFiringWhenEmpty) && base.CurrentAbilityAnimState == AbilityAnimState.Exit && Rewired_RL.Player.GetButton(m_abilityController.GetAbilityInputString(base.CastAbilityType)))
		{
			if (!m_abilityController.PlayerController.IsGrounded && m_abilityController.PlayerController.MovementState != CharacterStates.MovementStates.Dashing)
			{
				m_animator.SetBool("Hover", value: true);
				m_isHovering = true;
			}
			CancelChangeAnimCoroutine();
			m_animator.Play(AbilityTellIntroName.Replace("Tell", "Attack"));
		}
		else
		{
			m_animator.SetTrigger("Change_Ability_Anim");
			PerformTurnAnimCheck();
		}
	}

	protected override void OnExitExitLogic()
	{
		if (!m_continueFireOnHold || (base.CurrentAmmo <= 0 && !m_keepFiringWhenEmpty) || !Rewired_RL.Player.GetButton(m_abilityController.GetAbilityInputString(base.CastAbilityType)))
		{
			base.OnExitExitLogic();
		}
	}

	public override void StopAbility(bool abilityInterrupted)
	{
		m_animator.SetFloat("Attack_Direction", 2f);
		if (m_abilityController.PlayerController.ConditionState != CharacterStates.CharacterConditions.Stunned)
		{
			m_abilityController.PlayerController.ConditionState = CharacterStates.CharacterConditions.Normal;
		}
		if ((bool)m_aimLine)
		{
			m_aimLine.gameObject.SetActive(value: false);
		}
		m_animator.SetBool("Hover", value: false);
		if (m_gravityReductionModWhenAiming < 1f)
		{
			m_abilityController.PlayerController.FallMultiplierOverride = m_storedFallMultiplier;
		}
		CancelTimeSlow();
		m_isAiming = false;
		base.StopAbility(abilityInterrupted);
	}

	protected override void Update()
	{
		base.Update();
		if (!base.AbilityActive)
		{
			if (!m_continueFireOnHold && !m_abilityController.OnPauseResetInput && m_abilityController.PlayerController.CurrentMana >= (float)base.ActualCost && (base.CurrentAmmo > 0 || base.MaxAmmo == 0) && base.CooldownTimer <= 0f && m_abilityController.PlayerController.CharacterCorgi.REPlayer.GetButton(m_abilityController.GetAbilityInputString(base.CastAbilityType)))
			{
				m_abilityController.StartAbility(base.CastAbilityType);
			}
			return;
		}
		m_isAiming = false;
		if (base.CurrentAbilityAnimState < AbilityAnimState.Attack && (Rewired_RL.Player.GetButtonDown(m_abilityController.GetAbilityInputString(base.CastAbilityType)) || Rewired_RL.Player.GetButton(m_abilityController.GetAbilityInputString(base.CastAbilityType))))
		{
			UpdateArrowAim();
			UpdateAimLine();
			m_isAiming = true;
			if (m_prevIsFacingRight != m_abilityController.PlayerController.IsFacingRight)
			{
				m_prevIsFacingRight = m_abilityController.PlayerController.IsFacingRight;
				m_switchSidesRelay.Dispatch();
			}
		}
		if (!m_isAiming && Rewired_RL.Player.GetButton(m_abilityController.GetAbilityInputString(base.CastAbilityType)) && m_continueFireOnHold)
		{
			m_isAiming = true;
		}
		if (m_isHovering && (m_abilityController.PlayerController.IsGrounded || m_abilityController.PlayerController.MovementState == CharacterStates.MovementStates.Dashing))
		{
			m_isHovering = false;
			m_animator.SetBool("Hover", value: false);
		}
		else if (!m_isHovering && !m_abilityController.PlayerController.IsGrounded && m_abilityController.PlayerController.MovementState != CharacterStates.MovementStates.Dashing)
		{
			m_isHovering = true;
			m_animator.SetBool("Hover", value: true);
		}
	}

	protected override void FireProjectile()
	{
		m_abilityController.PlayerController.FallMultiplierOverride = m_storedFallMultiplier;
		m_abilityController.PlayerController.ConditionState = CharacterStates.CharacterConditions.Normal;
		if (!string.IsNullOrEmpty(ProjectileName))
		{
			float num = m_aimAngle;
			if (m_useUnmoddedAngle)
			{
				num = m_unmoddedAngle;
			}
			Vector2 offset = CDGHelper.RotatedPoint(new Vector2(ProjectileOffset.x, 0f), Vector2.zero, num);
			offset.y += ProjectileOffset.y;
			m_firedProjectile = ProjectileManager.FireProjectile(m_abilityController.gameObject, ProjectileName, offset, matchFacing: true, num);
			m_abilityController.InitializeProjectile(m_firedProjectile);
			ApplyAbilityCosts();
			if (!m_abilityController.PlayerController.IsGrounded && PushbackAmount != Vector2.zero)
			{
				m_abilityController.PlayerController.CharacterJump.ResetBrakeForce();
				Vector2 vector = CDGHelper.AngleToVector(0f - m_aimAngle);
				if (m_abilityController.PlayerController.IsFacingRight)
				{
					vector.x = 0f - vector.x;
				}
				m_abilityController.PlayerController.ConditionState = CharacterStates.CharacterConditions.DisableHorizontalMovement;
				if (!m_pushbackIgnoresDirection)
				{
					Vector2 vector2 = new Vector2(vector.x * PushbackAmount.x, vector.y * PushbackAmount.y);
					if (vector2.y < m_abilityController.PlayerController.Velocity.y)
					{
						vector2.y = m_abilityController.PlayerController.Velocity.y;
					}
					m_abilityController.PlayerController.SetVelocity(vector2.x, vector2.y, additive: false);
				}
				else
				{
					Vector2 pushbackAmount = PushbackAmount;
					if (m_abilityController.PlayerController.IsFacingRight)
					{
						pushbackAmount.x = 0f - pushbackAmount.x;
					}
					m_abilityController.PlayerController.SetVelocity(pushbackAmount.x, pushbackAmount.y, additive: false);
				}
				m_kickbackRelay.Dispatch();
			}
		}
		if ((bool)m_aimLine)
		{
			m_aimLine.gameObject.SetActive(value: false);
		}
		m_fireProjectileRelay.Dispatch(m_firedProjectile);
		if (CancelTimeSlowOnFire)
		{
			CancelTimeSlow();
		}
	}

	protected virtual void UpdateAimLine()
	{
		if (!m_aimLine)
		{
			return;
		}
		float aimAngle = m_aimAngle;
		float num = m_projSpeed;
		if (num == 0f)
		{
			num = 1f;
		}
		Vector2 vector = CDGHelper.AngleToVector(aimAngle) * num;
		float projGravityKickInDelay = m_projGravityKickInDelay;
		float num2 = (m_projIsWeighted ? (-50f) : 0f);
		Vector2 vector2 = vector * projGravityKickInDelay;
		float aimLineLength = m_aimLineLength;
		aimLineLength -= vector2.magnitude;
		float num3 = Mathf.Cos(aimAngle * ((float)Math.PI / 180f)) * aimLineLength;
		float num4 = 0f;
		num4 = ((vector.x > 0f) ? (num3 / vector.x) : ((!(aimAngle >= 0f)) ? ((0f - aimLineLength) / vector.y) : (aimLineLength / vector.y)));
		m_aimLine.SetPosition(0, Vector3.zero);
		int positionCount = m_aimLine.positionCount;
		if (vector2.magnitude > m_aimLineLength)
		{
			Vector2 vector3 = CDGHelper.AngleToVector(aimAngle) * m_aimLineLength;
			for (int i = 1; i < positionCount; i++)
			{
				float num5 = (float)i / (float)(positionCount - 1);
				if (!m_abilityController.PlayerController.IsFacingRight && base.AbilityActive)
				{
					m_aimLine.SetPosition(i, new Vector3((0f - vector3.x) * num5, vector3.y * num5, 0f));
				}
				else
				{
					m_aimLine.SetPosition(i, new Vector3(vector3.x * num5, vector3.y * num5, 0f));
				}
			}
			return;
		}
		if (!m_abilityController.PlayerController.IsFacingRight)
		{
			m_aimLine.SetPosition(1, new Vector3(0f - vector2.x, vector2.y, 0f));
		}
		else
		{
			m_aimLine.SetPosition(1, new Vector3(vector2.x, vector2.y, 0f));
		}
		float num6 = ((m_projIsWeighted && num2 != 0f) ? (vector.y / (0f - num2)) : vector.y);
		float num7 = vector.x * num6;
		float maxHeight = GetMaxHeight(vector.y, 0f - num2);
		float num8 = ((m_projIsWeighted && num2 != 0f && m_projFallMultiplier != 0f) ? (vector.y / (0f - num2 * m_projFallMultiplier)) : vector.y);
		float num9 = vector.x * num8;
		float maxHeight2 = GetMaxHeight(vector.y, 0f - num2 * m_projFallMultiplier);
		Vector2 vector4 = new Vector2(num9 - num7, maxHeight2 - maxHeight);
		int num10 = 0;
		for (int j = 2; j < positionCount; j++)
		{
			float num11 = 0f;
			if (positionCount - 1 > 0)
			{
				num11 = (float)j / (float)(positionCount - 1);
			}
			float num12 = num4 * num11;
			float num13 = num2;
			float y = vector.y;
			Vector2 vector5 = Vector2.zero;
			if (num12 >= num6 || aimAngle > 180f || aimAngle < 0f)
			{
				num13 *= m_projFallMultiplier;
				if (aimAngle <= 180f && aimAngle >= 0f)
				{
					vector5 = vector4;
					if (num10 == 0)
					{
						num10 = j;
					}
				}
			}
			float arcHeight = GetArcHeight(y, num12, 0f - num13);
			float num14 = num3 * num11 + vector2.x - vector5.x;
			if (!m_abilityController.PlayerController.IsFacingRight)
			{
				num14 = 0f - num14;
			}
			float y2 = arcHeight + vector2.y - vector5.y;
			if (j == num10)
			{
				m_aimLine.SetPosition(j, m_aimLine.GetPosition(j - 1));
			}
			else
			{
				m_aimLine.SetPosition(j, new Vector3(num14, y2, 0f));
			}
		}
	}

	protected virtual void UpdateArrowAim(bool doNotUpdatePlayerAnims = false)
	{
		ControllerType lastActiveControllerType = ReInput.controllers.GetLastActiveControllerType();
		float num = 0f;
		float num2 = 0f;
		bool isFacingRight = m_abilityController.PlayerController.IsFacingRight;
		if (SaveManager.ConfigData.ToggleMouseAiming && (lastActiveControllerType == ControllerType.Mouse || lastActiveControllerType == ControllerType.Keyboard))
		{
			Vector2 pt = (Vector2)m_abilityController.PlayerController.transform.localPosition + ProjectileOffset;
			pt.x -= ProjectileOffset.x;
			Vector2 vector = ReInput.controllers.Mouse.screenPosition;
			if (TraitManager.IsTraitActive(TraitType.UpsideDown))
			{
				float num3 = CameraController.GameCamera.transform.position.y - pt.y;
				pt.y += num3 * 2f;
				vector = new Vector2(vector.x, vector.y + 68f);
			}
			Vector2 pt2 = CameraController.GameCamera.ScreenToWorldPoint(vector);
			num = CDGHelper.AngleBetweenPts(pt, pt2);
		}
		else if (lastActiveControllerType == ControllerType.Joystick || !SaveManager.ConfigData.ToggleMouseAiming)
		{
			float axis = Rewired_RL.Player.GetAxis("MoveVertical");
			float axis2 = Rewired_RL.Player.GetAxis("MoveHorizontal");
			float axis3 = Rewired_RL.Player.GetAxis("MoveVerticalR");
			float axis4 = Rewired_RL.Player.GetAxis("MoveHorizontalR");
			Vector2 pt3 = new Vector2(axis2, axis);
			pt3.Normalize();
			Vector2 vector2 = new Vector2(axis4, axis3);
			vector2.Normalize();
			if (Mathf.Abs(vector2.magnitude) > 0.1f)
			{
				pt3 = vector2;
			}
			if (Mathf.Abs(pt3.magnitude) < 0.6f)
			{
				UpdateAimSpeed();
				return;
			}
			num = CDGHelper.VectorToAngle(pt3);
			if (!isFacingRight && num == 0f && axis2 == 0f)
			{
				num = 180f;
			}
		}
		num2 = num;
		int numberOfDirections = Mathf.RoundToInt(360f / (float)SaveManager.ConfigData.AimFidelity);
		num = CDGHelper.GetLockedAngle(num, numberOfDirections);
		bool flag = false;
		if (num > 90f && num < 270f)
		{
			num = (num - 180f) * -1f;
			flag = true;
		}
		num = CDGHelper.WrapAngleDegrees(num, convertToHalfRadius: true);
		if (!doNotUpdatePlayerAnims)
		{
			if ((flag && isFacingRight) || (!flag && !isFacingRight))
			{
				m_abilityController.PlayerController.CharacterCorgi.Flip();
				isFacingRight = m_abilityController.PlayerController.IsFacingRight;
			}
			if (!isFacingRight)
			{
				num *= -1f;
			}
		}
		if (TraitManager.IsTraitActive(TraitType.UpsideDown))
		{
			num *= -1f;
			num2 *= -1f;
		}
		if (!doNotUpdatePlayerAnims)
		{
			float t = (num - -90f) / 180f;
			float value = Mathf.Lerp(4f, 0f, t);
			if (!m_abilityController.PlayerController.IsFacingRight)
			{
				value = Mathf.Lerp(0f, 4f, t);
			}
			m_animator.SetFloat("Attack_Direction", value);
		}
		m_aimAngle = num;
		m_unmoddedAngle = num2;
		if (!isFacingRight)
		{
			m_aimAngle = 0f - num;
		}
		UpdateAimSpeed();
	}

	protected void CancelTimeSlow()
	{
		if (m_isTimeSlow)
		{
			RLTimeScale.SetTimeScale(TimeScaleType.AimTimeSlowAssist, 1f);
			m_isTimeSlow = false;
		}
	}

	private void UpdateAimSpeed()
	{
		float t = 0f;
		if (m_prevAngle != m_aimAngle)
		{
			t = Mathf.Clamp(Mathf.Abs(m_aimAngle - m_prevAngle), 0f, m_maxAimSpeedSFXDelta) / m_maxAimSpeedSFXDelta;
			m_prevAngle = m_aimAngle;
		}
		m_aimSpeedChangeRelay.Dispatch(t);
	}

	protected float GetArcHeight(float velocityY, float time, float gravity)
	{
		return velocityY * time - gravity * (time * time / 2f);
	}

	protected float GetMaxHeight(float velocityY, float gravity)
	{
		if (gravity == 0f)
		{
			return velocityY;
		}
		return velocityY * velocityY / (2f * gravity);
	}
}
