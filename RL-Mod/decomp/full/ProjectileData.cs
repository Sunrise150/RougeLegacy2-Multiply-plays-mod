using UnityEngine;

public class ProjectileData : ScriptableObject
{
	public string Name;

	public DamageType DamageType;

	public float StrengthScale;

	public float MagicScale;

	public float CooldownReductionPerHit;

	public float ManaGainPerHit;

	public float StunStrength;

	public float KnockbackStrength;

	public float KnockbackModX;

	public float KnockbackModY;

	public float Speed;

	public float TurnSpeed;

	public float LifeSpan;

	public float RepeatHitCheckTimer;

	public string Title;

	public string Controls;

	public string Description;
}
