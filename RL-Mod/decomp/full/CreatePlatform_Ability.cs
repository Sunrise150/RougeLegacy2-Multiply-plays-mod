using System;
using MoreMountains.CorgiEngine;
using UnityEngine;

public class CreatePlatform_Ability : GenericSpell_Ability
{
	private Action<MonoBehaviour, EventArgs> m_resumeCooldownIfPlayerExitsRoom;

	private Action<Projectile_RL, GameObject> m_resumeCooldown;

	protected override void Awake()
	{
		base.Awake();
		m_resumeCooldownIfPlayerExitsRoom = ResumeCooldownIfPlayerExitsRoom;
		m_resumeCooldown = ResumeCooldown;
	}

	private void OnEnable()
	{
		Messenger<GameMessenger, GameEvent>.AddListener(GameEvent.PlayerEnterRoom, m_resumeCooldownIfPlayerExitsRoom);
	}

	private void OnDisable()
	{
		Messenger<GameMessenger, GameEvent>.RemoveListener(GameEvent.PlayerEnterRoom, m_resumeCooldownIfPlayerExitsRoom);
	}

	protected override void FireProjectile()
	{
		base.FireProjectile();
		m_abilityController.PlayerController.CharacterJump.ResetBrakeForce();
		m_abilityController.PlayerController.SetVelocity(0f, 10f, additive: false);
		m_abilityController.PlayerController.ConditionState = CharacterStates.CharacterConditions.DisableHorizontalMovement;
		base.DecreaseCooldownOverTime = false;
		base.DisplayPausedAbilityCooldown = true;
		if ((bool)m_firedProjectile)
		{
			m_firedProjectile.OnDeathRelay.AddOnce(m_resumeCooldown);
		}
	}

	private void ResumeCooldownIfPlayerExitsRoom(object sender, EventArgs args)
	{
		base.DecreaseCooldownOverTime = true;
		base.DisplayPausedAbilityCooldown = false;
	}

	private void ResumeCooldown(Projectile_RL proj, GameObject obj)
	{
		base.DecreaseCooldownOverTime = true;
		base.DisplayPausedAbilityCooldown = false;
	}

	public override void StopAbility(bool abilityInterrupted)
	{
		base.StopAbility(abilityInterrupted);
		if (m_abilityController.PlayerController.ConditionState != CharacterStates.CharacterConditions.Stunned)
		{
			m_abilityController.PlayerController.ConditionState = CharacterStates.CharacterConditions.Normal;
		}
	}
}
