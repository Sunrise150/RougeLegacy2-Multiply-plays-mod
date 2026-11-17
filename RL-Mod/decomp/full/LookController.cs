using System;
using UnityEngine;

public class LookController : MonoBehaviour
{
	public const float SHOP_MODEL_OUTLINE_SCALE = 1.7f;

	[SerializeField]
	private bool m_isShopModel;

	[SerializeField]
	private bool m_isPortraitModel;

	[Header("Debug")]
	[SerializeField]
	private bool m_generateRandomLook;

	[Header("Player Geo")]
	[SerializeField]
	private SkinnedMeshRenderer m_leftEyeGeo;

	[SerializeField]
	private SkinnedMeshRenderer m_rightEyeGeo;

	[SerializeField]
	private SkinnedMeshRenderer m_mouthGeo;

	[SerializeField]
	private SkinnedMeshRenderer m_headGeo;

	[SerializeField]
	private SkinnedMeshRenderer m_helmetGeo;

	[SerializeField]
	private SkinnedMeshRenderer m_chestHairGeo;

	[SerializeField]
	private SkinnedMeshRenderer m_helmetHairGeo;

	[SerializeField]
	private SkinnedMeshRenderer m_leftShoulderGeo;

	[SerializeField]
	private SkinnedMeshRenderer m_rightShoulderGeo;

	[SerializeField]
	private SkinnedMeshRenderer m_capeGeo;

	[SerializeField]
	private SkinnedMeshRenderer m_scarfGeo;

	[SerializeField]
	private SkinnedMeshRenderer m_chestGeo;

	[SerializeField]
	private SkinnedMeshRenderer m_leftHandClosedGeo;

	[SerializeField]
	private SkinnedMeshRenderer m_rightHandClosedGeo;

	[SerializeField]
	private SkinnedMeshRenderer m_leftHandOpenGeo;

	[SerializeField]
	private SkinnedMeshRenderer m_rightHandOpenGeo;

	[Header("Player Class Geo", order = 10)]
	[SerializeField]
	private GameObject m_knightGeo;

	[SerializeField]
	private GameObject m_barbarianGeo;

	[SerializeField]
	private GameObject m_mageGeo;

	[SerializeField]
	private GameObject m_archerGeo;

	[SerializeField]
	private GameObject m_duelistGeo;

	[SerializeField]
	private GameObject m_spearmanGeo;

	[SerializeField]
	private GameObject m_chefGeo;

	[SerializeField]
	private GameObject m_assassinGeo;

	[SerializeField]
	private GameObject m_gunslingerGeo;

	[SerializeField]
	private GameObject m_boxerGeo;

	[SerializeField]
	private GameObject m_lancerGeo;

	[SerializeField]
	private GameObject m_roninGeo;

	[SerializeField]
	private GameObject m_pirateGeo;

	[SerializeField]
	private GameObject m_bardGeo;

	[SerializeField]
	private GameObject m_astroGeo;

	[Header("Player Weapon Geo")]
	[SerializeField]
	private SkinnedMeshRenderer m_swordGeo;

	[SerializeField]
	private SkinnedMeshRenderer m_spearGeo;

	[SerializeField]
	private SkinnedMeshRenderer m_lanceGeo;

	[SerializeField]
	private SkinnedMeshRenderer m_staffGeo;

	[SerializeField]
	private SkinnedMeshRenderer m_bowGeo;

	[SerializeField]
	private SkinnedMeshRenderer m_arrowGeo;

	[SerializeField]
	private SkinnedMeshRenderer m_saberGeo;

	[SerializeField]
	private SkinnedMeshRenderer m_ladleGeo;

	[SerializeField]
	private SkinnedMeshRenderer m_axeGeo;

	[SerializeField]
	private SkinnedMeshRenderer m_kunaiGeo;

	[SerializeField]
	private SkinnedMeshRenderer m_tonfaGeoL;

	[SerializeField]
	private SkinnedMeshRenderer m_tonfaGeoR;

	[SerializeField]
	private SkinnedMeshRenderer m_chakramGeo;

	[SerializeField]
	private SkinnedMeshRenderer m_dualBladeL;

	[SerializeField]
	private SkinnedMeshRenderer m_dualBladeR;

	[SerializeField]
	private SkinnedMeshRenderer m_fryingPanGeo;

	[SerializeField]
	private SkinnedMeshRenderer m_pistolGeo;

	[SerializeField]
	private SkinnedMeshRenderer m_boxingGlovesGeo;

	[SerializeField]
	private SkinnedMeshRenderer m_katanaGeo;

	[SerializeField]
	private SkinnedMeshRenderer m_cannonGeo;

	[SerializeField]
	private SkinnedMeshRenderer m_luteGeo;

	[SerializeField]
	private SkinnedMeshRenderer m_astroWandGeo;

	[Header("Misc Geo")]
	[SerializeField]
	private GameObject m_visualsGO;

	private Animator m_animator;

	private MaterialPropertyBlock m_matPropBlock;

	private SkinnedMeshRenderer[] m_bodyGeoArray;

	private SkinnedMeshRenderer[] m_outfitGeoArray;

	private SkinnedMeshRenderer[] m_customGeoArray;

	private GameObject m_currentOutfit;

	private GameObject m_customArmorMesh;

	private GameObject m_customHelmetMesh;

	private bool m_customGeoMeshIsDirty;

	public bool IsInitialized { get; protected set; }

	public bool IsShopModel => m_isShopModel;

	public bool IsPortraitModel => m_isPortraitModel;

	public SkinnedMeshRenderer LeftEyeGeo => m_leftEyeGeo;

	public SkinnedMeshRenderer RightEyeGeo => m_rightEyeGeo;

	public SkinnedMeshRenderer MouthGeo => m_mouthGeo;

	public SkinnedMeshRenderer HeadGeo => m_headGeo;

	public SkinnedMeshRenderer HelmetGeo => m_helmetGeo;

	public SkinnedMeshRenderer ChestHairGeo => m_chestHairGeo;

	public SkinnedMeshRenderer HelmetHairGeo => m_helmetHairGeo;

	public SkinnedMeshRenderer LeftShoulderGeo => m_leftShoulderGeo;

	public SkinnedMeshRenderer RightShoulderGeo => m_rightShoulderGeo;

	public SkinnedMeshRenderer CapeGeo => m_capeGeo;

	public SkinnedMeshRenderer ScarfGeo => m_scarfGeo;

	public SkinnedMeshRenderer ChestGeo => m_chestGeo;

	public SkinnedMeshRenderer LeftHandClosedGeo => m_leftHandClosedGeo;

	public SkinnedMeshRenderer RightHandClosedGeo => m_rightHandClosedGeo;

	public SkinnedMeshRenderer LeftHandOpenGeo => m_leftHandOpenGeo;

	public SkinnedMeshRenderer RightHandOpenGeo => m_rightHandOpenGeo;

	public SkinnedMeshRenderer[] BodyGeoArray => m_bodyGeoArray;

	public GameObject KnightGeo => m_knightGeo;

	public GameObject BarbarianGeo => m_barbarianGeo;

	public GameObject MageGeo => m_mageGeo;

	public GameObject ArcherGeo => m_archerGeo;

	public GameObject DuelistGeo => m_duelistGeo;

	public GameObject SpearmanGeo => m_spearmanGeo;

	public GameObject ChefGeo => m_chefGeo;

	public GameObject AssassinGeo => m_assassinGeo;

	public GameObject GunslingerGeo => m_gunslingerGeo;

	public GameObject BoxerGeo => m_boxerGeo;

	public GameObject LancerGeo => m_lancerGeo;

	public GameObject RoninGeo => m_roninGeo;

	public GameObject BardGeo => m_bardGeo;

	public GameObject PirateGeo => m_pirateGeo;

	public GameObject AstroGeo => m_astroGeo;

	public GameObject VisualsGameObject => m_visualsGO;

	public SkinnedMeshRenderer[] CurrentOutfitGeoArray => m_outfitGeoArray;

	public SkinnedMeshRenderer SwordGeo => m_swordGeo;

	public SkinnedMeshRenderer SpearGeo => m_spearGeo;

	public SkinnedMeshRenderer LanceGeo => m_lanceGeo;

	public SkinnedMeshRenderer WandGeo => m_staffGeo;

	public SkinnedMeshRenderer BowGeo => m_bowGeo;

	public SkinnedMeshRenderer ArrowGeo => m_arrowGeo;

	public SkinnedMeshRenderer SaberGeo => m_saberGeo;

	public SkinnedMeshRenderer LadleGeo => m_ladleGeo;

	public SkinnedMeshRenderer AxeGeo => m_axeGeo;

	public SkinnedMeshRenderer TonfaLeftGeo => m_tonfaGeoL;

	public SkinnedMeshRenderer TonfaRightGeo => m_tonfaGeoR;

	public SkinnedMeshRenderer ChakramGeo => m_chakramGeo;

	public SkinnedMeshRenderer KunaiGeo => m_kunaiGeo;

	public SkinnedMeshRenderer DualBladeLeftGeo => m_dualBladeL;

	public SkinnedMeshRenderer DualBladeRightGeo => m_dualBladeR;

	public SkinnedMeshRenderer FryingPanGeo => m_fryingPanGeo;

	public SkinnedMeshRenderer PistolGeo => m_pistolGeo;

	public SkinnedMeshRenderer BoxingGloveGeo => m_boxingGlovesGeo;

	public SkinnedMeshRenderer KatanaGeo => m_katanaGeo;

	public SkinnedMeshRenderer CannonGeo => m_cannonGeo;

	public SkinnedMeshRenderer LuteGeo => m_luteGeo;

	public SkinnedMeshRenderer AstroWandGeo => m_astroWandGeo;

	public SkinnedMeshRenderer CurrentWeaponGeo { get; set; }

	public SkinnedMeshRenderer SecondaryWeaponGeo { get; set; }

	public GameObject CustomArmorMesh
	{
		get
		{
			return m_customArmorMesh;
		}
		set
		{
			if (value != m_customArmorMesh)
			{
				m_customGeoMeshIsDirty = true;
			}
			m_customArmorMesh = value;
		}
	}

	public GameObject CustomHelmetMesh
	{
		get
		{
			return m_customHelmetMesh;
		}
		set
		{
			if (value != m_customHelmetMesh)
			{
				m_customGeoMeshIsDirty = true;
			}
			m_customHelmetMesh = value;
		}
	}

	public SkinnedMeshRenderer[] CustomGeoArray => m_customGeoArray;

	public GameObject CurrentClassOutfit
	{
		get
		{
			return m_currentOutfit;
		}
		set
		{
			m_currentOutfit = value;
			if (m_currentOutfit != null)
			{
				if (m_outfitGeoArray != null)
				{
					Array.Clear(m_outfitGeoArray, 0, m_outfitGeoArray.Length);
					m_outfitGeoArray = null;
				}
				m_outfitGeoArray = m_currentOutfit.GetComponentsInChildren<SkinnedMeshRenderer>();
			}
		}
	}

	public Animator Animator => m_animator;

	public MaterialPropertyBlock PropertyBlock => m_matPropBlock;

	public bool GenerateRandomLook
	{
		get
		{
			if (SaveManager.IsRunning)
			{
				return false;
			}
			if (LineageWindowController.CharacterLoadedFromLineage)
			{
				return false;
			}
			return m_generateRandomLook;
		}
		set
		{
			m_generateRandomLook = value;
		}
	}

	protected virtual void Awake()
	{
		if (!IsInitialized)
		{
			Initialize();
		}
	}

	public virtual void Initialize()
	{
		m_animator = GetComponent<Animator>();
		m_matPropBlock = new MaterialPropertyBlock();
		m_bodyGeoArray = LeftEyeGeo.transform.parent.GetComponentsInChildren<SkinnedMeshRenderer>(includeInactive: true);
		IsInitialized = true;
	}

	public virtual void InitializeLook(CharacterData charData)
	{
		if (!IsInitialized)
		{
			Initialize();
		}
		LookCreator.InitializeClassLook(charData.ClassType, this);
		LookCreator.InitializeCharacterLook(charData, this, GenerateRandomLook);
		LookCreator.DisableAllWeaponGeo(this);
	}

	protected void ApplyOutlineScale()
	{
		if (IsShopModel)
		{
			SkinnedMeshRenderer[] bodyGeoArray = BodyGeoArray;
			foreach (SkinnedMeshRenderer obj in bodyGeoArray)
			{
				obj.GetPropertyBlock(m_matPropBlock);
				m_matPropBlock.SetFloat(ShaderID_RL._OutlineScale, 1.7f);
				obj.SetPropertyBlock(m_matPropBlock);
			}
			bodyGeoArray = CurrentOutfitGeoArray;
			foreach (SkinnedMeshRenderer obj2 in bodyGeoArray)
			{
				obj2.GetPropertyBlock(m_matPropBlock);
				m_matPropBlock.SetFloat(ShaderID_RL._OutlineScale, 1.7f);
				obj2.SetPropertyBlock(m_matPropBlock);
			}
			bodyGeoArray = CustomGeoArray;
			foreach (SkinnedMeshRenderer obj3 in bodyGeoArray)
			{
				obj3.GetPropertyBlock(m_matPropBlock);
				m_matPropBlock.SetFloat(ShaderID_RL._OutlineScale, 1.7f);
				obj3.SetPropertyBlock(m_matPropBlock);
			}
		}
	}

	public void SetCustomMeshDirty()
	{
		m_customGeoMeshIsDirty = true;
	}

	protected void UpdateCustomMeshArray()
	{
		if (m_customGeoMeshIsDirty || m_customGeoArray == null)
		{
			SkinnedMeshRenderer[] array = ((m_customArmorMesh != null) ? m_customArmorMesh.GetComponentsInChildren<SkinnedMeshRenderer>() : null);
			SkinnedMeshRenderer[] array2 = ((m_customHelmetMesh != null) ? m_customHelmetMesh.GetComponentsInChildren<SkinnedMeshRenderer>() : null);
			int num = ((array != null) ? array.Length : 0);
			int num2 = ((array2 != null) ? array2.Length : 0);
			m_customGeoArray = new SkinnedMeshRenderer[num + num2];
			if (array != null)
			{
				Array.Copy(array, m_customGeoArray, num);
			}
			if (array2 != null)
			{
				Array.Copy(array2, 0, m_customGeoArray, num, num2);
			}
			m_customGeoMeshIsDirty = false;
		}
	}
}
