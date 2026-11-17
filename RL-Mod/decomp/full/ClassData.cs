using UnityEngine;

[CreateAssetMenu(menuName = "Custom/Rogue Legacy 2/Class Data")]
public class ClassData : ScriptableObject
{
	[SerializeField]
	private ClassStatsData m_statsData;

	[SerializeField]
	private ClassPassiveData m_passiveData;

	[SerializeField]
	private ClassWeaponData m_weaponData;

	[SerializeField]
	private ClassSpellData m_spellData;

	[SerializeField]
	private ClassTalentData m_talentData;

	public ClassPassiveData PassiveData => m_passiveData;

	public ClassWeaponData WeaponData => m_weaponData;

	public ClassSpellData SpellData => m_spellData;

	public ClassTalentData TalentData => m_talentData;
}
