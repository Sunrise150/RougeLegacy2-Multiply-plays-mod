using FMODUnity;
using UnityEngine;

public class Reload_Ability : BaseAbility_RL, ITalent, IAbility
{
	[SerializeField]
	private StudioEventEmitter m_reloadSuccessEmitter;

	[SerializeField]
	private StudioEventEmitter m_reloadLoopEmitter;

	protected override float TellIntroAnimSpeed => 1f;

	protected override float TellIntroAnimExitDelay => 0f;

	protected override float TellAnimSpeed => 1f;

	protected override float TellAnimExitDelay => 0f;

	protected override float AttackIntroAnimSpeed => 1.1f;

	protected override float AttackIntroAnimExitDelay => 0f;

	protected override float AttackAnimSpeed => 1f;

	protected override float AttackAnimExitDelay
	{
		get
		{
			float num = 0.6f;
			BaseAbility_RL ability = m_abilityController.PlayerController.CastAbility.GetAbility(CastAbilityType.Weapon, ignoreOverrides: true);
			if ((bool)ability && ability.AbilityType == AbilityType.PistolWeapon)
			{
				int currentAmmo = ability.CurrentAmmo;
				int maxAmmo = ability.MaxAmmo;
				if (maxAmmo > 0)
				{
					float num2 = 1f - (float)currentAmmo / (float)maxAmmo;
					num = Mathf.Max(0.1f, num * num2);
				}
			}
			return num;
		}
	}

	protected override float ExitAnimSpeed => 2f;

	protected override float ExitAnimExitDelay => 0f;

	protected override void FireProjectile()
	{
		base.FireProjectile();
		m_reloadLoopEmitter.Play();
	}

	public override void StopAbility(bool abilityInterrupted)
	{
		base.StopAbility(abilityInterrupted);
		m_reloadLoopEmitter.Stop();
		if (!abilityInterrupted)
		{
			m_abilityController.ResetAbilityAmmo(CastAbilityType.Weapon, ignoreAbilityOverride: true);
			m_abilityController.PlayerController.BlinkPulseEffect.StartSingleBlinkEffect(Color.white);
			m_reloadSuccessEmitter.Play();
			string text = ((!(Random.Range(0f, 1f) < 0.025f)) ? LocalizationManager.GetString("LOC_ID_STATUS_EFFECT_RELOAD_1", SaveManager.PlayerSaveData.CurrentCharacter.IsFemale) : LocalizationManager.GetString("LOC_ID_STATUS_EFFECT_RELOAD_2", SaveManager.PlayerSaveData.CurrentCharacter.IsFemale));
			TextPopupManager.DisplayTextDefaultPos(TextPopupType.DownstrikeAmmoGain, text, m_abilityController.PlayerController, attachToSource: true);
		}
	}
}
