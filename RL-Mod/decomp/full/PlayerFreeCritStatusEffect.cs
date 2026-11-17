using System.Collections;
using RLAudio;
using UnityEngine;

public class PlayerFreeCritStatusEffect : BaseStatusEffect, IAudioEventEmitter
{
	public override StatusEffectType StatusEffectType => StatusEffectType.Player_FreeCrit;

	public override float StartingDurationOverride => 2f;

	public string Description => ToString();

	protected override IEnumerator StartEffectCoroutine(IDamageObj caster, bool justCasted)
	{
		m_charController.StatusBarController.ApplyUIEffect(StatusBarEntryType.FreeCrit, base.Duration);
		PlayerController playerController = PlayerManager.GetPlayerController();
		playerController.LookController.SetCritBlinkEffectEnabled(enable: true, CritBlinkEffectTriggerType.PlayerFreeCritStatusEffect);
		if (base.TimesStacked <= 1)
		{
			PlayChargedStartAudio(playerController);
		}
		while (Time.time < base.EndTime)
		{
			yield return null;
		}
		StopEffect();
	}

	public override void StopEffect(bool interrupted = false)
	{
		base.StopEffect(interrupted);
		if (PlayerManager.IsInstantiated)
		{
			PlayerManager.GetPlayerController().LookController.SetCritBlinkEffectEnabled(enable: false, CritBlinkEffectTriggerType.PlayerFreeCritStatusEffect);
		}
	}

	private void PlayChargedStartAudio(PlayerController playerController)
	{
		if (playerController.CharacterClass.ClassType == ClassType.MagicWandClass)
		{
			AudioManager.PlayOneShotAttached(this, "event:/SFX/Weapons/sfx_weapon_crit_wand_charged", m_charController.gameObject);
		}
		else
		{
			AudioManager.PlayOneShotAttached(this, "event:/SFX/Weapons/sfx_weapon_crit_charged_start", m_charController.gameObject);
		}
	}
}
