using System.Collections;
using MoreMountains.CorgiEngine;
using UnityEngine;

public class RicochetCollision : MonoBehaviour, IWeaponOnEnterHitResponse, IHitResponse, ITerrainOnEnterHitResponse
{
	private enum KnockbackType
	{
		Any,
		VerticalOnly,
		HorizontalOnly
	}

	[SerializeField]
	private KnockbackType m_knockbackType;

	[SerializeField]
	private bool m_ricochetMatchFacing;

	[SerializeField]
	private float m_ricochetControlsLockoutDuration;

	[SerializeField]
	private bool m_resetsPlayerJump;

	private Projectile_RL m_projectile;

	private BaseCharacterController m_charController;

	private Coroutine m_controlsLockoutCoroutine;

	private WaitRL_Yield m_waitYield;

	private void Awake()
	{
		m_waitYield = new WaitRL_Yield(m_ricochetControlsLockoutDuration);
		m_projectile = GetComponent<Projectile_RL>();
		if (m_projectile == null)
		{
			m_charController = this.GetRoot().GetComponent<BaseCharacterController>();
		}
	}

	public void WeaponOnEnterHitResponse(IHitboxController otherHBController)
	{
		Ricochet(otherHBController.gameObject);
	}

	public void TerrainOnEnterHitResponse(IHitboxController otherHBController)
	{
		Ricochet(otherHBController.gameObject);
	}

	private void Ricochet(GameObject otherObj)
	{
		if (m_controlsLockoutCoroutine != null)
		{
			StopCoroutine(m_controlsLockoutCoroutine);
		}
		BaseCharacterController baseCharacterController = m_charController;
		if (m_projectile != null)
		{
			baseCharacterController = m_projectile.OwnerController;
		}
		PlayerController playerController = baseCharacterController as PlayerController;
		if (playerController != null)
		{
			playerController.StopActiveAbilities(stopPersistentAbilities: false);
			playerController.CharacterJump.ResetBrakeForce();
			if (m_resetsPlayerJump)
			{
				playerController.CharacterJump.SetNumberOfJumpsLeft(playerController.CharacterJump.NumberOfJumps - 1);
			}
		}
		GameObject root = otherObj.GetRoot();
		Vector2 bASE_RICOCHET_DISTANCE = Player_EV.BASE_RICOCHET_DISTANCE;
		if (m_knockbackType == KnockbackType.HorizontalOnly)
		{
			bASE_RICOCHET_DISTANCE.y = baseCharacterController.Velocity.y;
		}
		else if (m_knockbackType == KnockbackType.VerticalOnly)
		{
			bASE_RICOCHET_DISTANCE.x = baseCharacterController.Velocity.x;
		}
		if (bASE_RICOCHET_DISTANCE.y > 0f)
		{
			baseCharacterController.CharacterCorgi.MovementState.ChangeState(CharacterStates.MovementStates.Jumping);
		}
		baseCharacterController.Animator.SetTrigger("AirJump");
		if (m_ricochetControlsLockoutDuration > 0f)
		{
			baseCharacterController.ConditionState = CharacterStates.CharacterConditions.DisableHorizontalMovement;
			m_controlsLockoutCoroutine = StartCoroutine(LockoutControls(m_ricochetControlsLockoutDuration, baseCharacterController));
		}
		if (m_ricochetMatchFacing)
		{
			if (root.transform.position.x < baseCharacterController.transform.position.x)
			{
				baseCharacterController.SetVelocity(bASE_RICOCHET_DISTANCE.x, bASE_RICOCHET_DISTANCE.y, additive: false);
			}
			else
			{
				baseCharacterController.SetVelocity(0f - bASE_RICOCHET_DISTANCE.x, bASE_RICOCHET_DISTANCE.y, additive: false);
			}
		}
		else
		{
			baseCharacterController.SetVelocity(bASE_RICOCHET_DISTANCE.x, bASE_RICOCHET_DISTANCE.y, additive: false);
		}
	}

	private IEnumerator LockoutControls(float duration, BaseCharacterController objToRicochet)
	{
		m_waitYield.CreateNew(duration);
		yield return m_waitYield;
		if (objToRicochet.ConditionState != CharacterStates.CharacterConditions.Stunned)
		{
			objToRicochet.ConditionState = CharacterStates.CharacterConditions.Normal;
		}
	}
}
