using System.Runtime.CompilerServices;
using UnityEngine;

public class Breakable_VoidDashOnly : Breakable, IDamageObj
{
	private VoidDissolveComponent m_voidDissolve;

	public float BaseDamage => 0f;

	public float ActualDamage => Hazard_EV.GetDamageAmount(PlayerManager.GetCurrentPlayerRoom());

	public float ActualCritChance => 0f;

	public float ActualCritDamage => 0f;

	public Vector2 ExternalKnockbackMod => Vector2.one;

	public float BaseKnockbackStrength { get; set; }

	public float ActualKnockbackStrength => 10f;

	public float BaseStunStrength { get; set; }

	public float ActualStunStrength => 10f;

	public StrikeType StrikeType => StrikeType.Blunt;

	public bool IsDotDamage => false;

	public StatusEffectType[] StatusEffectTypes => null;

	public float[] StatusEffectDurations => null;

	protected override void Awake()
	{
		base.Awake();
		m_hitboxController.RepeatHitDuration = 0f;
		m_voidDissolve = GetComponent<VoidDissolveComponent>();
	}

	protected override void TriggerCollision(IDamageObj damageObj)
	{
		if (PlayerManager.GetPlayerController().CharacterDash.IsDashing && SaveManager.PlayerSaveData.GetHeirloomLevel(HeirloomType.UnlockVoidDash) > 0)
		{
			base.TriggerCollision(damageObj);
		}
	}

	protected override void Break(IDamageObj damageObj)
	{
		base.Break(damageObj);
		if ((bool)m_voidDissolve)
		{
			m_voidDissolve.StartDissolve(playSparkle: true);
		}
	}

	public override void ForceBrokenState(bool isBroken)
	{
		if (isBroken)
		{
			m_voidDissolve.ForceDissolved();
		}
		base.ForceBrokenState(isBroken);
	}

	[SpecialName]
	GameObject IDamageObj.get_gameObject()
	{
		return base.gameObject;
	}
}
