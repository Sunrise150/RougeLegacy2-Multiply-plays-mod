using System.Collections.Generic;
using UnityEngine;

public static class LookCreator
{
	private static Dictionary<string, Transform> m_boneMapDict = new Dictionary<string, Transform>();

	public static void InitializeClassLook(ClassType classType, LookController lookObj)
	{
		DisableAllClassOutfitGeo(lookObj);
		lookObj.Animator.SetBool("HideForeheadHair", value: false);
		GameObject gameObject = null;
		switch (classType)
		{
		default:
			gameObject = lookObj.KnightGeo;
			break;
		case ClassType.AxeClass:
			gameObject = lookObj.BarbarianGeo;
			break;
		case ClassType.MagicWandClass:
			gameObject = lookObj.MageGeo;
			break;
		case ClassType.BowClass:
			gameObject = lookObj.ArcherGeo;
			break;
		case ClassType.SaberClass:
			gameObject = lookObj.DuelistGeo;
			break;
		case ClassType.SpearClass:
			gameObject = lookObj.SpearmanGeo;
			break;
		case ClassType.LadleClass:
			gameObject = lookObj.ChefGeo;
			break;
		case ClassType.DualBladesClass:
			gameObject = lookObj.AssassinGeo;
			break;
		case ClassType.GunClass:
			gameObject = lookObj.GunslingerGeo;
			break;
		case ClassType.LanceClass:
			gameObject = lookObj.LancerGeo;
			break;
		case ClassType.BoxingGloveClass:
			gameObject = lookObj.BoxerGeo;
			break;
		case ClassType.KatanaClass:
			gameObject = lookObj.RoninGeo;
			lookObj.Animator.SetBool("HideForeheadHair", value: true);
			break;
		case ClassType.CannonClass:
			gameObject = lookObj.PirateGeo;
			break;
		case ClassType.LuteClass:
			gameObject = lookObj.BardGeo;
			break;
		case ClassType.AstroClass:
			gameObject = lookObj.AstroGeo;
			break;
		}
		gameObject.SetActive(value: true);
		lookObj.CurrentClassOutfit = gameObject;
	}

	private static void SetGeoColor(SkinnedMeshRenderer renderer, int shaderID, MaterialPropertyBlock matPropBlock, Color color)
	{
		renderer.GetPropertyBlock(matPropBlock);
		matPropBlock.SetColor(shaderID, color);
		renderer.SetPropertyBlock(matPropBlock);
	}

	public static void InitializeCharacterLook(CharacterData charData, LookController lookObj, bool generateRandomLook)
	{
		if (generateRandomLook)
		{
			CharacterCreator.GenerateRandomLook(charData);
		}
		_ = lookObj.PropertyBlock;
		MaterialWeightObject materialWeightObject = LookLibrary.GetEyeLookData()[charData.EyeType];
		lookObj.LeftEyeGeo.sharedMaterial = materialWeightObject.Material;
		lookObj.RightEyeGeo.sharedMaterial = materialWeightObject.Material;
		lookObj.LeftEyeGeo.GetPropertyBlock(lookObj.PropertyBlock);
		lookObj.PropertyBlock.SetColor(ShaderID_RL._MainColor, materialWeightObject.Material.GetColor(ShaderID_RL._MainColor));
		lookObj.PropertyBlock.SetColor(ShaderID_RL._AlphaBlendColor, materialWeightObject.Material.GetColor(ShaderID_RL._AlphaBlendColor));
		lookObj.PropertyBlock.SetColor(ShaderID_RL._RimLightColor, materialWeightObject.Material.GetColor(ShaderID_RL._RimLightColor));
		lookObj.LeftEyeGeo.SetPropertyBlock(lookObj.PropertyBlock);
		lookObj.RightEyeGeo.SetPropertyBlock(lookObj.PropertyBlock);
		MaterialWeightObject materialWeightObject2 = LookLibrary.GetMouthLookData()[charData.MouthType];
		lookObj.MouthGeo.sharedMaterial = materialWeightObject2.Material;
		if (charData.ClassType == ClassType.DualBladesClass)
		{
			lookObj.MouthGeo.gameObject.SetActive(value: false);
		}
		else
		{
			lookObj.MouthGeo.gameObject.SetActive(value: true);
		}
		MaterialWeightObject materialWeightObject3 = LookLibrary.GetFacialHairLookData()[charData.FacialHairType];
		lookObj.HeadGeo.sharedMaterial = materialWeightObject3.Material;
		ColorWeightObject colorWeightObject = LookLibrary.GetSkinColorLookData()[charData.SkinColorType];
		SetGeoColor(lookObj.HeadGeo, ShaderID_RL._MainColor, lookObj.PropertyBlock, colorWeightObject.Color);
		MaterialWeightObject materialWeightObject4 = LookLibrary.GetHairLookData()[charData.HairType];
		lookObj.ChestHairGeo.sharedMaterial = materialWeightObject4.Material;
		lookObj.HelmetHairGeo.sharedMaterial = materialWeightObject4.Material;
		ColorWeightObject colorWeightObject2 = LookLibrary.GetHairColorLookData()[charData.HairColorType];
		SetGeoColor(lookObj.ChestHairGeo, ShaderID_RL._MainColor, lookObj.PropertyBlock, colorWeightObject2.Color);
		SetGeoColor(lookObj.HelmetHairGeo, ShaderID_RL._MainColor, lookObj.PropertyBlock, colorWeightObject2.Color);
		BodyWeightObject bodyWeightObject = LookLibrary.GetBodyLookData()[charData.BodyType];
		lookObj.Animator.SetFloat("BodyType", (float)bodyWeightObject.BodyTypeWeightParam);
		lookObj.HeadGeo.SetBlendShapeWeight(0, bodyWeightObject.BlendShapeWeight);
	}

	public static void InitializeHelmetLook(EquipmentType equipType, LookController lookObj)
	{
		MaterialGeoObject materialGeoObject = EquipmentLookLibrary.GetHelmetEquipmentLookData(equipType) ?? EquipmentLookLibrary.GetHelmetEquipmentLookData(EquipmentType.None);
		if (materialGeoObject != null)
		{
			if ((bool)lookObj.CustomHelmetMesh)
			{
				if (lookObj.IsPortraitModel)
				{
					Object.Destroy(lookObj.CustomHelmetMesh);
				}
				else
				{
					Object.DestroyImmediate(lookObj.CustomHelmetMesh);
				}
				lookObj.CustomHelmetMesh = null;
				lookObj.SetCustomMeshDirty();
			}
			lookObj.HelmetGeo.gameObject.SetActive(value: true);
			lookObj.HelmetHairGeo.gameObject.SetActive(value: true);
			if ((bool)materialGeoObject.CustomGeo)
			{
				lookObj.CustomHelmetMesh = Object.Instantiate(materialGeoObject.CustomGeo, lookObj.VisualsGameObject.transform, worldPositionStays: false);
				lookObj.CustomHelmetMesh.name = materialGeoObject.CustomGeo.name;
				lookObj.CustomHelmetMesh.SetLayerRecursively(lookObj.HelmetGeo.gameObject.layer);
				SkinnedMeshRenderer component = lookObj.CustomHelmetMesh.transform.FindDeep("geoHelmet").gameObject.GetComponent<SkinnedMeshRenderer>();
				RebindSkinnedMeshRenderers(lookObj.HelmetGeo, component);
				lookObj.HelmetGeo.gameObject.SetActive(value: false);
				SkinnedMeshRenderer component2 = lookObj.CustomHelmetMesh.transform.FindDeep("geoHelmetHair").gameObject.GetComponent<SkinnedMeshRenderer>();
				RebindSkinnedMeshRenderers(lookObj.HelmetHairGeo, component2);
				lookObj.HelmetHairGeo.gameObject.SetActive(value: false);
				GameObject gameObject = lookObj.CustomHelmetMesh.transform.FindDeep("root").gameObject;
				if (lookObj.IsPortraitModel)
				{
					Object.Destroy(gameObject);
				}
				else
				{
					Object.DestroyImmediate(gameObject);
				}
				component.sharedMaterial = materialGeoObject.Material;
				component2.sharedMaterial = lookObj.HelmetHairGeo.sharedMaterial;
				lookObj.HelmetHairGeo.GetPropertyBlock(lookObj.PropertyBlock);
				SetGeoColor(component2, ShaderID_RL._MainColor, lookObj.PropertyBlock, lookObj.PropertyBlock.GetColor(ShaderID_RL._MainColor));
			}
			else
			{
				lookObj.HelmetGeo.sharedMaterial = materialGeoObject.Material;
			}
			SkinnedMeshRenderer[] currentOutfitGeoArray = lookObj.CurrentOutfitGeoArray;
			for (int i = 0; i < currentOutfitGeoArray.Length; i++)
			{
				SetGeoColor(currentOutfitGeoArray[i], ShaderID_RL._HelmetColor, lookObj.PropertyBlock, materialGeoObject.Material.GetColor(ShaderID_RL._MainColor));
			}
		}
		else
		{
			Debug.Log("Could not find Helmet Equipment Look Library entry for " + equipType);
		}
	}

	public static void InitializeArmorLook(EquipmentType equipType, LookController lookObj)
	{
		MaterialGeoObject materialGeoObject = EquipmentLookLibrary.GetArmorEquipmentLookData(equipType) ?? EquipmentLookLibrary.GetArmorEquipmentLookData(EquipmentType.None);
		if (materialGeoObject != null)
		{
			if ((bool)lookObj.CustomArmorMesh)
			{
				if (lookObj.IsPortraitModel)
				{
					Object.Destroy(lookObj.CustomArmorMesh);
				}
				else
				{
					Object.DestroyImmediate(lookObj.CustomArmorMesh);
				}
				lookObj.CustomArmorMesh = null;
				lookObj.SetCustomMeshDirty();
			}
			lookObj.ChestGeo.gameObject.SetActive(value: true);
			lookObj.LeftShoulderGeo.gameObject.SetActive(value: true);
			lookObj.RightShoulderGeo.gameObject.SetActive(value: true);
			if ((bool)materialGeoObject.CustomGeo)
			{
				lookObj.CustomArmorMesh = Object.Instantiate(materialGeoObject.CustomGeo, lookObj.VisualsGameObject.transform, worldPositionStays: false);
				lookObj.CustomArmorMesh.name = materialGeoObject.CustomGeo.name;
				lookObj.CustomArmorMesh.SetLayerRecursively(lookObj.ChestGeo.gameObject.layer);
				SkinnedMeshRenderer component = lookObj.CustomArmorMesh.transform.FindDeep("geoChest").gameObject.GetComponent<SkinnedMeshRenderer>();
				RebindSkinnedMeshRenderers(lookObj.ChestGeo, component);
				lookObj.ChestGeo.gameObject.SetActive(value: false);
				SkinnedMeshRenderer component2 = lookObj.CustomArmorMesh.transform.FindDeep("geoShoulder_l").gameObject.GetComponent<SkinnedMeshRenderer>();
				RebindSkinnedMeshRenderers(lookObj.LeftShoulderGeo, component2);
				lookObj.LeftShoulderGeo.gameObject.SetActive(value: false);
				SkinnedMeshRenderer component3 = lookObj.CustomArmorMesh.transform.FindDeep("geoShoulder_r").gameObject.GetComponent<SkinnedMeshRenderer>();
				RebindSkinnedMeshRenderers(lookObj.RightShoulderGeo, component3);
				lookObj.RightShoulderGeo.gameObject.SetActive(value: false);
				GameObject gameObject = lookObj.CustomArmorMesh.transform.FindDeep("root").gameObject;
				if (lookObj.IsPortraitModel)
				{
					Object.Destroy(gameObject);
				}
				else
				{
					Object.DestroyImmediate(gameObject);
				}
				component.sharedMaterial = materialGeoObject.Material;
				component2.sharedMaterial = materialGeoObject.Material;
				component3.sharedMaterial = materialGeoObject.Material;
			}
			else
			{
				lookObj.ChestGeo.sharedMaterial = materialGeoObject.Material;
				lookObj.LeftShoulderGeo.sharedMaterial = materialGeoObject.Material;
				lookObj.RightShoulderGeo.sharedMaterial = materialGeoObject.Material;
			}
			lookObj.LeftHandClosedGeo.sharedMaterial = materialGeoObject.Material;
			lookObj.RightHandClosedGeo.sharedMaterial = materialGeoObject.Material;
			lookObj.LeftHandOpenGeo.sharedMaterial = materialGeoObject.Material;
			lookObj.RightHandOpenGeo.sharedMaterial = materialGeoObject.Material;
			SkinnedMeshRenderer[] currentOutfitGeoArray = lookObj.CurrentOutfitGeoArray;
			for (int i = 0; i < currentOutfitGeoArray.Length; i++)
			{
				SetGeoColor(currentOutfitGeoArray[i], ShaderID_RL._ArmorColor, lookObj.PropertyBlock, materialGeoObject.Material.GetColor(ShaderID_RL._MainColor));
			}
		}
		else
		{
			Debug.Log("Could not find Armor Equipment Look Library entry for " + equipType);
		}
	}

	public static void InitializeCapeLook(EquipmentType equipType, LookController lookObj)
	{
		MaterialBlendWeightObject materialBlendWeightObject = EquipmentLookLibrary.GetCapeEquipmentLookData(equipType) ?? EquipmentLookLibrary.GetCapeEquipmentLookData(EquipmentType.None);
		if (materialBlendWeightObject != null)
		{
			lookObj.CapeGeo.sharedMaterial = materialBlendWeightObject.Material;
			lookObj.ScarfGeo.sharedMaterial = materialBlendWeightObject.Material;
			lookObj.CapeGeo.SetBlendShapeWeight(0, materialBlendWeightObject.BlendWeight);
			SkinnedMeshRenderer[] currentOutfitGeoArray = lookObj.CurrentOutfitGeoArray;
			for (int i = 0; i < currentOutfitGeoArray.Length; i++)
			{
				SetGeoColor(currentOutfitGeoArray[i], ShaderID_RL._CapeColor, lookObj.PropertyBlock, materialBlendWeightObject.Material.GetColor(ShaderID_RL._MainColor));
			}
		}
		else
		{
			Debug.Log("Could not find Cape Equipment Look Library entry for " + equipType);
		}
	}

	public static void InitializeWeaponLook(EquipmentType equipType, LookController lookObj, AbilityType weaponType, bool hasCantAttackTrait)
	{
		PlayerLookController playerLookController = lookObj as PlayerLookController;
		if ((bool)playerLookController)
		{
			playerLookController.ForceDisableCritBlinkEffect();
		}
		DisableAllWeaponGeo(lookObj);
		MaterialGeoObject materialGeoObject = null;
		SkinnedMeshRenderer skinnedMeshRenderer = null;
		SkinnedMeshRenderer skinnedMeshRenderer2 = null;
		int num = 0;
		bool flag = false;
		if (hasCantAttackTrait)
		{
			materialGeoObject = EquipmentLookLibrary.GetWeaponEquipmentLookData(AbilityType.SwordWeapon, equipType) ?? EquipmentLookLibrary.GetWeaponEquipmentLookData(AbilityType.SwordWeapon, EquipmentType.None);
			skinnedMeshRenderer = lookObj.SwordGeo;
		}
		else
		{
			materialGeoObject = EquipmentLookLibrary.GetWeaponEquipmentLookData(weaponType, equipType) ?? EquipmentLookLibrary.GetWeaponEquipmentLookData(weaponType, EquipmentType.None);
			switch (weaponType)
			{
			case AbilityType.SwordWeapon:
			case AbilityType.SwordBeamWeapon:
				skinnedMeshRenderer = lookObj.SwordGeo;
				break;
			case AbilityType.SpearWeapon:
				skinnedMeshRenderer = lookObj.SpearGeo;
				break;
			case AbilityType.LanceWeapon:
				skinnedMeshRenderer = lookObj.LanceGeo;
				break;
			case AbilityType.MagicWandWeapon:
				skinnedMeshRenderer = lookObj.WandGeo;
				break;
			case AbilityType.BowWeapon:
			case AbilityType.GroundBowWeapon:
				skinnedMeshRenderer = lookObj.BowGeo;
				skinnedMeshRenderer2 = lookObj.ArrowGeo;
				flag = true;
				break;
			case AbilityType.SaberWeapon:
				skinnedMeshRenderer = lookObj.SaberGeo;
				break;
			case AbilityType.FryingPanWeapon:
				skinnedMeshRenderer = lookObj.FryingPanGeo;
				break;
			case AbilityType.SpoonsWeapon:
				skinnedMeshRenderer = lookObj.LadleGeo;
				break;
			case AbilityType.AxeWeapon:
			case AbilityType.AxeSpinnerWeapon:
				skinnedMeshRenderer = lookObj.AxeGeo;
				break;
			case AbilityType.KunaiWeapon:
				skinnedMeshRenderer = lookObj.KunaiGeo;
				break;
			case AbilityType.ChakramWeapon:
				skinnedMeshRenderer = lookObj.ChakramGeo;
				break;
			case AbilityType.TonfaWeapon:
				skinnedMeshRenderer = lookObj.TonfaLeftGeo;
				skinnedMeshRenderer2 = lookObj.TonfaRightGeo;
				num = 1;
				break;
			case AbilityType.DualBladesWeapon:
				skinnedMeshRenderer = lookObj.DualBladeLeftGeo;
				skinnedMeshRenderer2 = lookObj.DualBladeRightGeo;
				num = 1;
				break;
			case AbilityType.ScytheWeapon:
				skinnedMeshRenderer = lookObj.LanceGeo;
				break;
			case AbilityType.PistolWeapon:
			case AbilityType.DragonPistolWeapon:
				skinnedMeshRenderer = lookObj.PistolGeo;
				num = 2;
				break;
			case AbilityType.BoxingGloveWeapon:
			case AbilityType.ExplosiveHandsWeapon:
				skinnedMeshRenderer = lookObj.BoxingGloveGeo;
				num = 1;
				lookObj.LeftHandClosedGeo.gameObject.SetActive(value: false);
				lookObj.LeftHandOpenGeo.gameObject.SetActive(value: false);
				lookObj.RightHandClosedGeo.gameObject.SetActive(value: false);
				lookObj.RightHandOpenGeo.gameObject.SetActive(value: false);
				break;
			case AbilityType.KatanaWeapon:
				skinnedMeshRenderer = lookObj.KatanaGeo;
				break;
			case AbilityType.CannonWeapon:
				skinnedMeshRenderer = lookObj.CannonGeo;
				break;
			case AbilityType.KineticBowWeapon:
			case AbilityType.LuteWeapon:
				skinnedMeshRenderer = lookObj.LuteGeo;
				break;
			case AbilityType.AstroWandWeapon:
				skinnedMeshRenderer = lookObj.AstroWandGeo;
				break;
			}
		}
		if (materialGeoObject != null)
		{
			Material fabledMaterial = EquipmentLookLibrary.GetFabledMaterial(weaponType);
			if ((bool)fabledMaterial)
			{
				skinnedMeshRenderer.sharedMaterial = fabledMaterial;
			}
			else
			{
				skinnedMeshRenderer.sharedMaterial = materialGeoObject.Material;
			}
			skinnedMeshRenderer.gameObject.SetActive(value: true);
			if ((bool)skinnedMeshRenderer2)
			{
				skinnedMeshRenderer2.sharedMaterial = skinnedMeshRenderer.sharedMaterial;
				if (!flag)
				{
					skinnedMeshRenderer2.gameObject.SetActive(value: true);
				}
			}
			lookObj.CurrentWeaponGeo = skinnedMeshRenderer;
			lookObj.SecondaryWeaponGeo = skinnedMeshRenderer2;
		}
		else
		{
			num = 1;
			Debug.Log("Could not find Weapon Equipment Look Library entry for " + equipType.ToString() + " of Weapon Type: " + weaponType);
		}
		if (materialGeoObject == null)
		{
			if ((bool)lookObj.CurrentWeaponGeo)
			{
				lookObj.CurrentWeaponGeo.gameObject.SetActive(value: false);
			}
			if ((bool)lookObj.SecondaryWeaponGeo)
			{
				lookObj.SecondaryWeaponGeo.gameObject.SetActive(value: false);
			}
		}
		lookObj.Animator.SetFloat("Weaponless", num);
		if (hasCantAttackTrait)
		{
			skinnedMeshRenderer.sharedMaterial = EquipmentLookLibrary.CantAttackMaterial;
		}
	}

	public static void DisableAllWeaponGeo(LookController lookObj)
	{
		lookObj.SwordGeo.gameObject.SetActive(value: false);
		lookObj.SpearGeo.gameObject.SetActive(value: false);
		lookObj.LanceGeo.gameObject.SetActive(value: false);
		lookObj.WandGeo.gameObject.SetActive(value: false);
		lookObj.BowGeo.gameObject.SetActive(value: false);
		lookObj.ArrowGeo.gameObject.SetActive(value: false);
		lookObj.SaberGeo.gameObject.SetActive(value: false);
		lookObj.LadleGeo.gameObject.SetActive(value: false);
		lookObj.AxeGeo.gameObject.SetActive(value: false);
		lookObj.KunaiGeo.gameObject.SetActive(value: false);
		lookObj.ChakramGeo.gameObject.SetActive(value: false);
		lookObj.TonfaLeftGeo.gameObject.SetActive(value: false);
		lookObj.TonfaRightGeo.gameObject.SetActive(value: false);
		lookObj.DualBladeLeftGeo.gameObject.SetActive(value: false);
		lookObj.DualBladeRightGeo.gameObject.SetActive(value: false);
		lookObj.FryingPanGeo.gameObject.SetActive(value: false);
		lookObj.PistolGeo.gameObject.SetActive(value: false);
		lookObj.BoxingGloveGeo.gameObject.SetActive(value: false);
		lookObj.KatanaGeo.gameObject.SetActive(value: false);
		lookObj.CannonGeo.gameObject.SetActive(value: false);
		lookObj.LuteGeo.gameObject.SetActive(value: false);
		lookObj.AstroWandGeo.gameObject.SetActive(value: false);
		lookObj.LeftHandClosedGeo.gameObject.SetActive(value: true);
		lookObj.LeftHandOpenGeo.gameObject.SetActive(value: true);
		lookObj.RightHandClosedGeo.gameObject.SetActive(value: true);
		lookObj.RightHandOpenGeo.gameObject.SetActive(value: true);
		lookObj.CurrentWeaponGeo = null;
		lookObj.SecondaryWeaponGeo = null;
	}

	public static void DisableAllClassOutfitGeo(LookController lookObj)
	{
		lookObj.BarbarianGeo.SetActive(value: false);
		lookObj.KnightGeo.SetActive(value: false);
		lookObj.MageGeo.SetActive(value: false);
		lookObj.ArcherGeo.SetActive(value: false);
		lookObj.DuelistGeo.SetActive(value: false);
		lookObj.SpearmanGeo.SetActive(value: false);
		lookObj.ChefGeo.SetActive(value: false);
		lookObj.AssassinGeo.SetActive(value: false);
		lookObj.GunslingerGeo.SetActive(value: false);
		lookObj.LancerGeo.SetActive(value: false);
		lookObj.BoxerGeo.SetActive(value: false);
		lookObj.RoninGeo.SetActive(value: false);
		lookObj.PirateGeo.SetActive(value: false);
		lookObj.BardGeo.SetActive(value: false);
		lookObj.AstroGeo.SetActive(value: false);
	}

	public static void RebindSkinnedMeshRenderers(SkinnedMeshRenderer rebindFromRenderer, SkinnedMeshRenderer rebindToRenderer)
	{
		m_boneMapDict.Clear();
		Transform[] bones = rebindFromRenderer.bones;
		foreach (Transform transform in bones)
		{
			m_boneMapDict.Add(transform.gameObject.name, transform);
		}
		Transform[] array = new Transform[rebindToRenderer.bones.Length];
		for (int j = 0; j < rebindToRenderer.bones.Length; j++)
		{
			GameObject gameObject = rebindToRenderer.bones[j].gameObject;
			if (!m_boneMapDict.TryGetValue(gameObject.name, out array[j]))
			{
				Debug.Log("Unable to map bone \"" + gameObject.name + "\" to target skeleton.");
				break;
			}
		}
		rebindToRenderer.rootBone = rebindFromRenderer.rootBone;
		rebindToRenderer.bones = array;
	}
}
