using System.Collections;
using UnityEngine;

public class GodModeStatusEffect : BaseStatusEffect
{
	private float m_defaultBaseStrength;

	private float m_defaultSpeed;

	public override StatusEffectType StatusEffectType => StatusEffectType.Player_GodMode;

	public override float StartingDurationOverride => 1f;

	protected override IEnumerator StartEffectCoroutine(IDamageObj caster, bool justCasted)
	{
		m_charController.StatusBarController.ApplyUIEffect(StatusBarEntryType.GodMode, base.Duration);
		m_defaultBaseStrength = PlayerManager.GetPlayerController().BaseStrength;
		m_defaultSpeed = PlayerManager.GetPlayerController().BaseMovementSpeed;
		PlayerManager.GetPlayerController().BaseStrength = 5f;
		PlayerManager.GetPlayerController().BaseMovementSpeed *= 2f;
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
			PlayerManager.GetPlayerController().BaseStrength = m_defaultBaseStrength;
			PlayerManager.GetPlayerController().BaseMovementSpeed = m_defaultSpeed;
		}
	}
}
