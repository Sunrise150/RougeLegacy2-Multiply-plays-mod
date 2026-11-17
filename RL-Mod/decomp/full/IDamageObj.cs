using UnityEngine;

public interface IDamageObj
{
	GameObject gameObject { get; }

	float BaseDamage { get; }

	float ActualDamage { get; }

	float ActualCritChance { get; }

	float ActualCritDamage { get; }

	Vector2 ExternalKnockbackMod { get; }

	float BaseKnockbackStrength { get; set; }

	float ActualKnockbackStrength { get; }

	float BaseStunStrength { get; set; }

	float ActualStunStrength { get; }

	StrikeType StrikeType { get; }

	bool IsDotDamage { get; }

	StatusEffectType[] StatusEffectTypes { get; }

	float[] StatusEffectDurations { get; }
}
