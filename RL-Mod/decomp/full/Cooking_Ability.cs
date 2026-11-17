using FMODUnity;
using MoreMountains.CorgiEngine;
using UnityEngine;

public class Cooking_Ability : BaseAbility_RL, ITalent, IAbility
{
	[SerializeField]
	private StudioEventEmitter m_cookingEmitter;

	protected override float TellIntroAnimSpeed => 1.25f;

	protected override float TellIntroAnimExitDelay => 0f;

	protected override float TellAnimSpeed => 1.25f;

	protected override float TellAnimExitDelay => 0f;

	protected override float AttackIntroAnimSpeed => 1.25f;

	protected override float AttackIntroAnimExitDelay => 0f;

	protected override float AttackAnimSpeed => 1f;

	protected override float AttackAnimExitDelay => 0.35f;

	protected override float ExitAnimSpeed => 2f;

	protected override float ExitAnimExitDelay => 0f;

	public override void PreCastAbility()
	{
		base.PreCastAbility();
		if (m_abilityController.PlayerController.CharacterClass.WeaponAbilityType != AbilityType.SpoonsWeapon)
		{
			if (m_abilityController.PlayerController.CharacterClass.WeaponAbilityType != AbilityType.BoxingGloveWeapon && m_abilityController.PlayerController.CharacterClass.WeaponAbilityType != AbilityType.ExplosiveHandsWeapon && (bool)m_abilityController.PlayerController.LookController.CurrentWeaponGeo)
			{
				m_abilityController.PlayerController.LookController.CurrentWeaponGeo.gameObject.SetActive(value: false);
			}
			m_abilityController.PlayerController.LookController.LadleGeo.gameObject.SetActive(value: true);
		}
		m_abilityController.PlayerController.ControllerCorgi.GravityActive(state: false);
		m_abilityController.PlayerController.SetVelocity(0f, 0f, additive: false);
		m_abilityController.PlayerController.ConditionState = CharacterStates.CharacterConditions.DisableHorizontalMovement;
		m_abilityController.PlayerController.ControllerCorgi.RayLengthAdd += 0.05f;
		m_cookingEmitter.Play();
	}

	protected override void OnEnterAttackLogic()
	{
		m_abilityController.BroadcastAbilityCastEvents(base.CastAbilityType);
		FireProjectile();
	}

	protected override void OnEnterExitLogic()
	{
		HealPlayer();
		ApplyAbilityCosts();
		base.OnEnterExitLogic();
	}

	public static void HealPlayer()
	{
		PlayerController playerController = PlayerManager.GetPlayerController();
		float num = Mathf.CeilToInt(playerController.ActualMagic * 2f);
		float num2 = Mathf.CeilToInt(playerController.ActualMagic * 0f + 100f);
		playerController.SetHealth(num, additive: true, runEvents: true);
		playerController.SetMana(num2, additive: true, runEvents: true);
		Vector2 absPos = playerController.Midpoint;
		absPos.y += playerController.CollisionBounds.height / 2f;
		if (TraitManager.IsTraitActive(TraitType.MegaHealth))
		{
			num = 0f;
		}
		string text = string.Format(LocalizationManager.GetString("LOC_ID_STATUS_EFFECT_HEALTH_RESTORE_1", SaveManager.PlayerSaveData.CurrentCharacter.IsFemale), (int)num);
		string text2 = string.Format(LocalizationManager.GetString("LOC_ID_STATUS_EFFECT_MANA_RESTORE_1", SaveManager.PlayerSaveData.CurrentCharacter.IsFemale), (int)num2);
		TextPopupManager.DisplayTextAtAbsPos(TextPopupType.HPGained, text + "\n<color=#47FFFB>" + text2 + "</color>", absPos);
	}

	public override void StopAbility(bool abilityInterrupted)
	{
		base.StopAbility(abilityInterrupted);
		m_cookingEmitter.Stop();
		m_abilityController.PlayerController.ControllerCorgi.RayLengthAdd -= 0.05f;
		if (m_abilityController.PlayerController.CharacterClass.WeaponAbilityType != AbilityType.SpoonsWeapon)
		{
			if ((bool)m_abilityController.PlayerController.LookController.CurrentWeaponGeo)
			{
				m_abilityController.PlayerController.LookController.CurrentWeaponGeo.gameObject.SetActive(value: true);
			}
			m_abilityController.PlayerController.LookController.LadleGeo.gameObject.SetActive(value: false);
		}
		m_abilityController.PlayerController.ControllerCorgi.GravityActive(state: true);
		if (m_abilityController.PlayerController.ConditionState != CharacterStates.CharacterConditions.Stunned)
		{
			m_abilityController.PlayerController.ConditionState = CharacterStates.CharacterConditions.Normal;
		}
	}
}
