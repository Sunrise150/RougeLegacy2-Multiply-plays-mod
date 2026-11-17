using UnityEngine;

public class AbilityData : ScriptableObject
{
	public string Name;

	public CastAbilityType Type;

	public int BaseCost;

	public int MaxAmmo;

	public float CooldownTime;

	public float LockoutTime;

	public bool CooldownDecreaseOverTime;

	public bool CooldownDecreasePerHit;

	public bool ManaGainPerHit;

	public bool CooldownRefreshesAllAmmo;

	public bool LockDirection;

	public float MovementMod;

	public float AirMovementMod;

	public bool CanCastWhileDashing;

	public bool AttackCancel;

	public bool SpellCancel;

	public bool TalentCancel;

	public bool DashCancel;

	public bool JumpCancel;

	[Space]
	[Header("Text Fields")]
	[Space]
	public string Title;

	public string Controls;

	public string Description;
}
