using System.Linq;
using UnityEngine;

[CreateAssetMenu(menuName = "Custom/Libraries/Equipment Look Library")]
public class EquipmentLookLibrary : ScriptableObject
{
	public const string RESOURCES_PATH = "Scriptable Objects/Libraries/EquipmentLookLibrary";

	[Space(10f)]
	[Header("Body Parts")]
	[Space(10f)]
	[SerializeField]
	private MaterialGeo_EquipmentLookData m_helmetEquipmentLookLibrary;

	[SerializeField]
	private MaterialGeo_EquipmentLookData m_armorEquipmentLookLibrary;

	[SerializeField]
	private MaterialBlendWeight_EquipmentLookData m_capeEquipmentLookLibrary;

	[Space(10f)]
	[Header("Weapons")]
	[Space(10f)]
	[SerializeField]
	private MaterialGeo_EquipmentLookData m_swordEquipmentLookLibrary;

	[SerializeField]
	private MaterialGeo_EquipmentLookData m_spearEquipmentLookLibrary;

	[SerializeField]
	private MaterialGeo_EquipmentLookData m_staffEquipmentLookLibrary;

	[SerializeField]
	private MaterialGeo_EquipmentLookData m_lanceEquipmentLookLibrary;

	[SerializeField]
	private MaterialGeo_EquipmentLookData m_bowEquipmentLookLibrary;

	[SerializeField]
	private MaterialGeo_EquipmentLookData m_saberEquipmentLookLibrary;

	[SerializeField]
	private MaterialGeo_EquipmentLookData m_ladleEquipmentLookLibrary;

	[SerializeField]
	private MaterialGeo_EquipmentLookData m_axeEquipmentLookLibrary;

	[SerializeField]
	private MaterialGeo_EquipmentLookData m_kunaiEquipmentLookLibrary;

	[SerializeField]
	private MaterialGeo_EquipmentLookData m_chakramEquipmentLookLibrary;

	[SerializeField]
	private MaterialGeo_EquipmentLookData m_tonfaEquipmentLookLibrary;

	[SerializeField]
	private MaterialGeo_EquipmentLookData m_dualBladesEquipmentLookLibrary;

	[SerializeField]
	private MaterialGeo_EquipmentLookData m_fryingPanEquipmentLookLibrary;

	[SerializeField]
	private MaterialGeo_EquipmentLookData m_pistolEquipmentLookLibrary;

	[SerializeField]
	private MaterialGeo_EquipmentLookData m_boxingGloveEquipmentLookLibrary;

	[SerializeField]
	private MaterialGeo_EquipmentLookData m_katanaEquipmentLookLibrary;

	[SerializeField]
	private MaterialGeo_EquipmentLookData m_cannonEquipmentLookLibrary;

	[SerializeField]
	private MaterialGeo_EquipmentLookData m_luteEquipmentLookLibrary;

	[SerializeField]
	private MaterialGeo_EquipmentLookData m_astroWandEquipmentLookLibrary;

	[Space(10f)]
	[SerializeField]
	private Material m_cantAttackMaterial;

	[Space(10f)]
	[Header("Fabled Weapon Materials")]
	[Space(10f)]
	[SerializeField]
	private AbilityTypeMaterialDictionary m_fabledWeaponDict;

	private static EquipmentLookLibrary m_instance;

	private static EquipmentLookLibrary Instance
	{
		get
		{
			if (m_instance == null)
			{
				m_instance = CDGResources.Load<EquipmentLookLibrary>("Scriptable Objects/Libraries/EquipmentLookLibrary");
			}
			return m_instance;
		}
	}

	public static Material CantAttackMaterial => Instance.m_cantAttackMaterial;

	public static Material GetFabledMaterial(AbilityType abilityType)
	{
		if (Instance.m_fabledWeaponDict.TryGetValue(abilityType, out var value))
		{
			return value;
		}
		return null;
	}

	public static MaterialGeoObject[] GetHelmetEquipmentLookData()
	{
		return Instance.m_helmetEquipmentLookLibrary.ObjectList;
	}

	public static MaterialGeoObject GetHelmetEquipmentLookData(EquipmentType equipType)
	{
		return Instance.m_helmetEquipmentLookLibrary.ObjectList.Where((MaterialGeoObject obj) => obj.EquipmentType == equipType).SingleOrDefault();
	}

	public static MaterialGeoObject[] GetArmorEquipmentLookData()
	{
		return Instance.m_armorEquipmentLookLibrary.ObjectList;
	}

	public static MaterialGeoObject GetArmorEquipmentLookData(EquipmentType equipType)
	{
		return Instance.m_armorEquipmentLookLibrary.ObjectList.Where((MaterialGeoObject obj) => obj.EquipmentType == equipType).SingleOrDefault();
	}

	public static MaterialBlendWeightObject[] GetCapeEquipmentLookData()
	{
		return Instance.m_capeEquipmentLookLibrary.ObjectList;
	}

	public static MaterialBlendWeightObject GetCapeEquipmentLookData(EquipmentType equipType)
	{
		return Instance.m_capeEquipmentLookLibrary.ObjectList.Where((MaterialBlendWeightObject obj) => obj.EquipmentType == equipType).SingleOrDefault();
	}

	public static MaterialGeo_EquipmentLookData GetWeaponEquipmentLookData(AbilityType weaponAbilityType)
	{
		switch (weaponAbilityType)
		{
		case AbilityType.SwordWeapon:
		case AbilityType.SwordBeamWeapon:
			return Instance.m_swordEquipmentLookLibrary;
		case AbilityType.SaberWeapon:
			return Instance.m_saberEquipmentLookLibrary;
		case AbilityType.SpearWeapon:
			return Instance.m_spearEquipmentLookLibrary;
		case AbilityType.LanceWeapon:
		case AbilityType.ScytheWeapon:
			return Instance.m_lanceEquipmentLookLibrary;
		case AbilityType.AxeWeapon:
		case AbilityType.AxeSpinnerWeapon:
			return Instance.m_axeEquipmentLookLibrary;
		case AbilityType.MagicWandWeapon:
			return Instance.m_staffEquipmentLookLibrary;
		case AbilityType.BowWeapon:
		case AbilityType.GroundBowWeapon:
			return Instance.m_bowEquipmentLookLibrary;
		case AbilityType.FryingPanWeapon:
			return Instance.m_fryingPanEquipmentLookLibrary;
		case AbilityType.DualBladesWeapon:
			return Instance.m_dualBladesEquipmentLookLibrary;
		case AbilityType.PistolWeapon:
		case AbilityType.DragonPistolWeapon:
			return Instance.m_pistolEquipmentLookLibrary;
		case AbilityType.BoxingGloveWeapon:
		case AbilityType.ExplosiveHandsWeapon:
			return Instance.m_boxingGloveEquipmentLookLibrary;
		case AbilityType.ChakramWeapon:
			return Instance.m_chakramEquipmentLookLibrary;
		case AbilityType.TonfaWeapon:
			return Instance.m_tonfaEquipmentLookLibrary;
		case AbilityType.KunaiWeapon:
			return Instance.m_kunaiEquipmentLookLibrary;
		case AbilityType.SpoonsWeapon:
			return Instance.m_ladleEquipmentLookLibrary;
		case AbilityType.KatanaWeapon:
			return Instance.m_katanaEquipmentLookLibrary;
		case AbilityType.CannonWeapon:
			return Instance.m_cannonEquipmentLookLibrary;
		case AbilityType.KineticBowWeapon:
		case AbilityType.LuteWeapon:
			return Instance.m_luteEquipmentLookLibrary;
		case AbilityType.AstroWandWeapon:
			return Instance.m_astroWandEquipmentLookLibrary;
		default:
			return null;
		}
	}

	public static MaterialGeoObject GetWeaponEquipmentLookData(AbilityType weaponAbilityType, EquipmentType equipType)
	{
		MaterialGeo_EquipmentLookData weaponEquipmentLookData = GetWeaponEquipmentLookData(weaponAbilityType);
		if ((bool)weaponEquipmentLookData)
		{
			return weaponEquipmentLookData.ObjectList.Where((MaterialGeoObject obj) => obj.EquipmentType == equipType).SingleOrDefault();
		}
		return null;
	}
}
