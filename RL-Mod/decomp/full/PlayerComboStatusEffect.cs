using System.Collections;
using UnityEngine;

public class PlayerComboStatusEffect : BaseStatusEffect
{
	public const float ADD_RELIC_STACKS_HACK = float.MaxValue;

	private bool m_addRelicStacks;

	public override StatusEffectType StatusEffectType => StatusEffectType.Player_Combo;

	public override float StartingDurationOverride => 2f;

	public override int StacksPerHit
	{
		get
		{
			int num = base.StacksPerHit;
			int level = SaveManager.PlayerSaveData.GetRelic(RelicType.WeaponsComboAdd).Level;
			if (level > 0)
			{
				num = ((!m_addRelicStacks) ? level : (num + level));
			}
			return num;
		}
	}

	public override void StartEffect(float duration, IDamageObj caster)
	{
		float duration2 = duration;
		m_addRelicStacks = false;
		if (duration == float.MaxValue)
		{
			m_addRelicStacks = true;
			duration2 = StartingDurationOverride;
		}
		base.StartEffect(duration2, caster);
	}

	protected override IEnumerator StartEffectCoroutine(IDamageObj caster, bool justCasted)
	{
		base.TimesStacked = Mathf.Clamp(base.TimesStacked, 0, 30);
		m_charController.StatusBarController.ApplyUIEffect(StatusBarEntryType.Combo, base.Duration, 30, base.TimesStacked);
		if (base.TimesStacked >= 15)
		{
			PlayerManager.GetPlayerController().LookController.SetCritBlinkEffectEnabled(enable: true, CritBlinkEffectTriggerType.PlayerComboStatusEffect);
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
			PlayerManager.GetPlayerController().LookController.SetCritBlinkEffectEnabled(enable: false, CritBlinkEffectTriggerType.PlayerComboStatusEffect);
		}
	}
}
