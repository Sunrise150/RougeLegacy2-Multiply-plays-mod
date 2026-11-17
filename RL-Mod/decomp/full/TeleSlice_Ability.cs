using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using FMODUnity;
using UnityEngine;

public class TeleSlice_Ability : AimedAbilityFast_RL, ITalent, IAbility
{
	[SerializeField]
	private GameObject m_endAimIndicator;

	[Header("Audio Event Emitters")]
	[SerializeField]
	private StudioEventEmitter m_chargeEmitter;

	[SerializeField]
	private StudioEventEmitter m_cancelChargeEmitter;

	private Vector2 m_storedSoftZone;

	private Action<MonoBehaviour, EventArgs> m_onEnemyHit;

	private List<RaycastHit2D> m_platformHitList = new List<RaycastHit2D>(5);

	protected override float TellIntroAnimSpeed => 4f;

	protected override float TellIntroAnimExitDelay => 0f;

	protected override float TellAnimSpeed => 4f;

	protected override float TellAnimExitDelay => 0f;

	protected override float AttackIntroAnimSpeed => 4f;

	protected override float AttackIntroAnimExitDelay => 0f;

	protected override float AttackAnimSpeed => 4f;

	protected override float AttackAnimExitDelay => 0f;

	protected override float ExitAnimSpeed => 1f;

	protected override float ExitAnimExitDelay => 0f;

	protected override float GravityReduction => 0.425f;

	protected override Vector2 BowPushbackAmount => new Vector2(0f, 2.5f);

	protected override AbilityAnimState StateToHoldAttackOn => AbilityAnimState.Tell;

	protected override void Awake()
	{
		base.Awake();
		m_onEnemyHit = OnEnemyHit;
	}

	public override void Initialize(CastAbility_RL abilityController, CastAbilityType castAbilityType)
	{
		float num = 12f / (float)m_aimLine.positionCount;
		for (int i = 0; i < m_aimLine.positionCount; i++)
		{
			m_aimLine.SetPosition(i, new Vector3(num * (float)i, 0f, 0f));
		}
		m_endAimIndicator.transform.localPosition = new Vector3(12f, 0f, 0f);
		Messenger<GameMessenger, GameEvent>.AddListener(GameEvent.EnemyHit, m_onEnemyHit);
		base.Initialize(abilityController, castAbilityType);
	}

	protected override void OnDestroy()
	{
		Messenger<GameMessenger, GameEvent>.RemoveListener(GameEvent.EnemyHit, m_onEnemyHit);
		base.OnDestroy();
	}

	private void OnEnemyHit(object sender, EventArgs args)
	{
		CharacterHitEventArgs characterHitEventArgs = args as CharacterHitEventArgs;
		if (characterHitEventArgs.Victim.CurrentHealth <= 0f && (bool)m_firedProjectile && characterHitEventArgs.Attacker == m_firedProjectile)
		{
			EndCooldownTimer();
		}
	}

	public override void PreCastAbility()
	{
		base.PreCastAbility();
		m_chargeEmitter.Play();
	}

	protected override IEnumerator ChangeAnim(float duration)
	{
		if (base.CurrentAbilityAnimState < StateToHoldAttackOn)
		{
			if (duration <= 0f)
			{
				yield return null;
			}
			while (duration > 0f)
			{
				duration -= Time.deltaTime;
				if (!Rewired_RL.Player.GetButton(m_abilityController.GetAbilityInputString(base.CastAbilityType)))
				{
					CancelChangeAnimCoroutine();
					m_animator.Play(AbilityTellIntroName.Replace("Tell", "Attack"));
					yield break;
				}
				yield return null;
			}
			m_animator.SetTrigger("Change_Ability_Anim");
		}
		else
		{
			yield return base.ChangeAnim(duration);
		}
	}

	protected override void FireProjectile()
	{
		Vector2 vector = CDGHelper.AngleToVector(m_unmoddedAngle);
		Vector3 localPosition = m_abilityController.PlayerController.transform.localPosition;
		if (m_abilityController.PlayerController.IsGrounded)
		{
			m_abilityController.PlayerController.ControllerCorgi.SetLastStandingPosition(localPosition);
		}
		m_abilityController.PlayerController.StartCoroutine(DisableSlopeCheckCoroutine(m_abilityController.PlayerController));
		LayerMask layerMask = (int)m_abilityController.PlayerController.ControllerCorgi.SavedPlatformMask & ~(int)m_abilityController.PlayerController.ControllerCorgi.OneWayPlatformMask;
		ContactFilter2D contactFilter = default(ContactFilter2D);
		contactFilter.NoFilter();
		contactFilter.SetLayerMask(layerMask);
		m_platformHitList.Clear();
		Physics2D.Raycast(m_abilityController.PlayerController.Midpoint, vector, contactFilter, m_platformHitList, 12f);
		RaycastHit2D raycastHit2D = default(RaycastHit2D);
		foreach (RaycastHit2D platformHit in m_platformHitList)
		{
			if ((bool)platformHit && !platformHit.collider.CompareTag("OneWay"))
			{
				raycastHit2D = platformHit;
				break;
			}
		}
		LayerMask layerMask2 = 1073741824;
		RaycastHit2D raycastHit2D2 = Physics2D.Raycast(m_abilityController.PlayerController.Midpoint, vector, 12f, layerMask2);
		bool flag = false;
		if ((bool)raycastHit2D2)
		{
			flag = raycastHit2D2.collider.GetComponentInParent<Cloud>();
		}
		float x = m_abilityController.PlayerController.transform.localPosition.x;
		Vector3 localPosition2 = m_abilityController.PlayerController.transform.localPosition;
		if ((bool)raycastHit2D)
		{
			Vector2 vector2 = -vector;
			Vector3 vector3 = m_abilityController.PlayerController.Midpoint - m_abilityController.PlayerController.transform.localPosition;
			if (vector.x >= 0f)
			{
				localPosition2.x = Mathf.Max(raycastHit2D.point.x - vector3.x + vector2.x, localPosition2.x);
			}
			else
			{
				localPosition2.x = Mathf.Min(raycastHit2D.point.x - vector3.x + vector2.x, localPosition2.x);
			}
			if (vector.y >= 0f)
			{
				localPosition2.y = Mathf.Max(raycastHit2D.point.y - vector3.y + vector2.y, localPosition2.y);
			}
			else
			{
				localPosition2.y = Mathf.Min(raycastHit2D.point.y - vector3.y + vector2.y, localPosition2.y);
			}
		}
		else if (flag)
		{
			Vector2 vector4 = -vector;
			Vector3 vector5 = m_abilityController.PlayerController.Midpoint - m_abilityController.PlayerController.transform.localPosition;
			localPosition2.x = raycastHit2D2.point.x - vector5.x + vector4.x;
			localPosition2.y = raycastHit2D2.point.y - vector5.y + vector4.y;
		}
		else
		{
			localPosition2.x += vector.x * 12f;
			localPosition2.y += vector.y * 12f;
		}
		float num = 0f;
		float num2 = 0f;
		BaseRoom currentPlayerRoom = PlayerManager.GetCurrentPlayerRoom();
		if (currentPlayerRoom.AppearanceBiomeType != BiomeType.Lineage)
		{
			Rect boundsRect = currentPlayerRoom.BoundsRect;
			if (localPosition2.x < boundsRect.xMin)
			{
				num = boundsRect.xMin - localPosition2.x;
			}
			else if (localPosition2.x > boundsRect.xMax)
			{
				num = boundsRect.xMax - localPosition2.x;
			}
			if (localPosition2.y < boundsRect.yMin)
			{
				num2 = boundsRect.yMin - localPosition2.y;
			}
			else if (localPosition2.y > boundsRect.yMax)
			{
				num2 = boundsRect.yMax - localPosition2.y;
			}
			localPosition2.x += num;
			localPosition2.y += num2;
		}
		if (num != 0f || num2 != 0f)
		{
			Vector2 vector6 = CDGHelper.AngleToVector(CDGHelper.AngleBetweenPts(m_abilityController.PlayerController.Midpoint, localPosition2));
			raycastHit2D = Physics2D.Raycast(m_abilityController.PlayerController.Midpoint, vector6, 12f, layerMask);
			if ((bool)raycastHit2D)
			{
				Vector2 vector7 = -vector6;
				Vector3 vector8 = m_abilityController.PlayerController.Midpoint - m_abilityController.PlayerController.transform.localPosition;
				localPosition2.x = raycastHit2D.point.x - vector8.x + vector7.x;
				localPosition2.y = raycastHit2D.point.y - vector8.y + vector7.y;
			}
		}
		m_abilityController.PlayerController.transform.localPosition = localPosition2;
		bool flag2 = false;
		if ((x > m_abilityController.PlayerController.transform.localPosition.x && m_abilityController.PlayerController.IsFacingRight) || (x < m_abilityController.PlayerController.transform.localPosition.x && !m_abilityController.PlayerController.IsFacingRight))
		{
			m_abilityController.PlayerController.CharacterCorgi.Flip();
			flag2 = true;
		}
		m_abilityController.PlayerController.ControllerCorgi.SetRaysParameters();
		m_abilityController.PlayerController.SetVelocity(0f, 0f, additive: false);
		StartCoroutine(PushbackGroundCheck());
		if (m_abilityController.PlayerController.InvincibilityTimer < 0.1f)
		{
			m_abilityController.PlayerController.CharacterHitResponse.SetInvincibleTime(0.1f, additive: false, playInvincibility: false);
		}
		if ((flag2 && m_abilityController.PlayerController.IsFacingRight) || (!flag2 && !m_abilityController.PlayerController.IsFacingRight))
		{
			m_aimAngle *= -1f;
		}
		CinemachineFramingTransposer componentInChildren = CameraController.CinemachineBrain.ActiveVirtualCamera.VirtualCameraGameObject.GetComponentInChildren<CinemachineFramingTransposer>();
		m_storedSoftZone.x = componentInChildren.m_SoftZoneWidth;
		m_storedSoftZone.y = componentInChildren.m_SoftZoneHeight;
		componentInChildren.m_SoftZoneWidth = 1f;
		componentInChildren.m_SoftZoneHeight = 2f;
		TweenManager.TweenTo(componentInChildren, 0.25f, Ease.Quad.EaseOut, "m_SoftZoneWidth", m_storedSoftZone.x, "m_SoftZoneHeight", m_storedSoftZone.y);
		m_chargeEmitter.Stop();
		base.FireProjectile();
	}

	public override void StopAbility(bool abilityInterrupted)
	{
		m_chargeEmitter.Stop();
		if (abilityInterrupted)
		{
			m_cancelChargeEmitter.Play();
		}
		base.StopAbility(abilityInterrupted);
	}

	private IEnumerator DisableSlopeCheckCoroutine(PlayerController playerController)
	{
		playerController.ControllerCorgi.StickWhenWalkingDownSlopes = false;
		yield return null;
		playerController.ControllerCorgi.StickWhenWalkingDownSlopes = true;
	}

	private IEnumerator PushbackGroundCheck()
	{
		yield return null;
		if (!m_abilityController.PlayerController.IsGrounded)
		{
			m_abilityController.PlayerController.SetVelocity(PushbackAmount.x, PushbackAmount.y, additive: false);
		}
	}

	protected override void UpdateArrowAim(bool doNotUpdatePlayerAnims = false)
	{
		base.UpdateArrowAim(doNotUpdatePlayerAnims: true);
	}

	protected override void UpdateAimLine()
	{
		Vector3 localEulerAngles = m_aimLine.transform.localEulerAngles;
		localEulerAngles.z = m_unmoddedAngle;
		m_aimLine.transform.localEulerAngles = localEulerAngles;
	}
}
