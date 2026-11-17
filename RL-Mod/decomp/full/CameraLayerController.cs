using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;

public class CameraLayerController : MonoBehaviour, ICameraLayerController, IRootObj
{
	[SerializeField]
	private GameObject m_visuals;

	[SerializeField]
	private CameraLayer m_cameraLayer = CameraLayer.Game;

	[SerializeField]
	private int m_subLayer;

	[SerializeField]
	private bool m_setVisualLayers = true;

	[SerializeField]
	private bool m_disableNonGameLayerCollisions = true;

	private const int CHARACTER_DEFAULT_Z_POSITION = 0;

	private Prop m_prop;

	private bool m_isDeco;

	private bool m_isSet;

	private ZPositionOverride[] m_zPositionOverrides;

	private ICameraLayerController[] m_childCameraLayerControllers;

	private CorgiController_RL[] m_corgiControllers;

	private IHitboxController[] m_hbControllers;

	private Transform[] m_transforms;

	private Collider2D[] m_platformColliders;

	private Collider2D[] m_propColliders;

	private bool m_transparentFXLayersAssigned;

	private int m_transparentFXLayerFlags;

	public bool IsProp => m_prop;

	public bool IsSet => m_isSet;

	public CameraLayer CameraLayer
	{
		get
		{
			return m_cameraLayer;
		}
		private set
		{
			m_cameraLayer = value;
		}
	}

	public bool DisableNonGameLayerCollisions
	{
		get
		{
			return m_disableNonGameLayerCollisions;
		}
		set
		{
			if (!Application.isPlaying)
			{
				m_disableNonGameLayerCollisions = value;
			}
		}
	}

	public bool SetVisualLayers
	{
		get
		{
			return m_setVisualLayers;
		}
		set
		{
			m_setVisualLayers = value;
		}
	}

	public int SubLayer
	{
		get
		{
			return m_subLayer;
		}
		private set
		{
			m_subLayer = value;
		}
	}

	public GameObject Visuals
	{
		get
		{
			return m_visuals;
		}
		set
		{
			m_visuals = value;
		}
	}

	private void Awake()
	{
		m_prop = base.gameObject.GetComponent<Prop>();
	}

	private void Start()
	{
		if (!m_isSet)
		{
			SetLayerAndSubLayer();
		}
	}

	public void SetLayerAndSubLayer()
	{
		SetLayer();
		SetZPosition();
	}

	public void SetIsDeco(bool isDeco)
	{
		m_isDeco = isDeco;
	}

	public void SetCameraLayer(CameraLayer cameraLayer)
	{
		CameraLayer = cameraLayer;
		if (Application.isPlaying)
		{
			m_isSet = true;
			SetLayer();
		}
	}

	private void SetLayer()
	{
		if (!SetVisualLayers)
		{
			return;
		}
		if (CameraLayer == CameraLayer.None || CameraLayer == CameraLayer.Any)
		{
			SetCameraLayer(CameraLayer.Game);
		}
		LayerMask layer = CameraLayerUtility.GetLayer(CameraLayer);
		if (!Visuals)
		{
			Visuals = base.gameObject;
		}
		InitializeTransforms(Visuals);
		SetLayersRecursively(Visuals, layer);
		if (DisableNonGameLayerCollisions && CameraLayer != CameraLayer.Game)
		{
			if (m_hbControllers == null)
			{
				m_hbControllers = base.gameObject.GetComponentsInChildren<IHitboxController>();
			}
			for (int i = 0; i < m_hbControllers.Length; i++)
			{
				IHitboxController obj = m_hbControllers[i];
				obj.SetHitboxActiveState(HitboxType.Body, active: false);
				obj.SetHitboxActiveState(HitboxType.Weapon, active: false);
				obj.SetHitboxActiveState(HitboxType.Terrain, active: false);
				m_hbControllers[i].DisableAllCollisions = true;
			}
			if (m_platformColliders == null)
			{
				m_platformColliders = (from entry in base.gameObject.GetComponentsInChildren<Collider2D>()
					where entry.gameObject.layer == 8 || entry.gameObject.layer == 10 || entry.gameObject.layer == 9 || entry.gameObject.layer == 11
					select entry).ToArray();
			}
			for (int j = 0; j < m_platformColliders.Length; j++)
			{
				m_platformColliders[j].enabled = false;
			}
			if (m_propColliders == null)
			{
				m_propColliders = (from entry in base.gameObject.GetComponentsInChildren<Collider2D>()
					where entry.gameObject.layer == 12
					select entry).ToArray();
			}
			for (int k = 0; k < m_propColliders.Length; k++)
			{
				m_propColliders[k].enabled = false;
			}
			if (m_corgiControllers == null)
			{
				m_corgiControllers = base.gameObject.GetComponentsInChildren<CorgiController_RL>();
			}
			for (int l = 0; l < m_corgiControllers.Length; l++)
			{
				m_corgiControllers[l].enabled = false;
			}
		}
		else
		{
			if (m_hbControllers == null)
			{
				m_hbControllers = base.gameObject.GetComponentsInChildren<IHitboxController>();
			}
			for (int m = 0; m < m_hbControllers.Length; m++)
			{
				IHitboxController obj2 = m_hbControllers[m];
				obj2.SetHitboxActiveState(HitboxType.Body, active: true);
				obj2.SetHitboxActiveState(HitboxType.Weapon, active: true);
				obj2.SetHitboxActiveState(HitboxType.Terrain, active: true);
				m_hbControllers[m].DisableAllCollisions = false;
			}
			if (m_platformColliders == null)
			{
				m_platformColliders = (from entry in base.gameObject.GetComponentsInChildren<Collider2D>()
					where entry.gameObject.layer == 8 || entry.gameObject.layer == 10 || entry.gameObject.layer == 9 || entry.gameObject.layer == 11
					select entry).ToArray();
			}
			for (int n = 0; n < m_platformColliders.Length; n++)
			{
				m_platformColliders[n].enabled = true;
			}
			if (m_propColliders == null)
			{
				m_propColliders = (from entry in base.gameObject.GetComponentsInChildren<Collider2D>()
					where entry.gameObject.layer == 12
					select entry).ToArray();
			}
			for (int num = 0; num < m_propColliders.Length; num++)
			{
				m_propColliders[num].enabled = true;
			}
			if (m_corgiControllers == null)
			{
				m_corgiControllers = base.gameObject.GetComponentsInChildren<CorgiController_RL>();
			}
			for (int num2 = 0; num2 < m_corgiControllers.Length; num2++)
			{
				m_corgiControllers[num2].enabled = true;
			}
		}
		if (m_childCameraLayerControllers == null)
		{
			ICameraLayerController[] componentsInChildren = base.gameObject.GetComponentsInChildren<CameraLayerController>();
			m_childCameraLayerControllers = componentsInChildren;
		}
		for (int num3 = 1; num3 < m_childCameraLayerControllers.Length; num3++)
		{
			m_childCameraLayerControllers[num3].SetCameraLayer(m_childCameraLayerControllers[num3].CameraLayer);
		}
	}

	private void InitializeTransforms(GameObject obj)
	{
		if (m_transforms != null)
		{
			return;
		}
		m_transforms = obj.GetComponentsInChildren<Transform>(includeInactive: true);
		if (m_transparentFXLayersAssigned)
		{
			return;
		}
		m_transparentFXLayersAssigned = true;
		for (int i = 0; i < m_transforms.Length; i++)
		{
			if (m_transforms[i].gameObject.layer == 1)
			{
				m_transparentFXLayerFlags |= 1 << i;
			}
		}
	}

	private void SetLayersRecursively(GameObject obj, LayerMask layer)
	{
		if (CameraLayer == CameraLayer.Game)
		{
			for (int i = 0; i < m_transforms.Length; i++)
			{
				Transform transform = m_transforms[i];
				if ((m_transparentFXLayerFlags & (1 << i)) != 0)
				{
					transform.gameObject.layer = 1;
				}
				else
				{
					transform.gameObject.layer = layer;
				}
			}
		}
		else
		{
			Transform[] transforms = m_transforms;
			for (int j = 0; j < transforms.Length; j++)
			{
				transforms[j].gameObject.layer = layer;
			}
		}
	}

	public void SetSubLayer(int subLayer, bool isDeco = false)
	{
		Vector2Int subLayerRange = CameraLayerUtility.GetSubLayerRange(CameraLayer);
		SubLayer = Mathf.Clamp(subLayer, subLayerRange.x, subLayerRange.y);
		if (Application.isPlaying)
		{
			m_isSet = true;
			SetZPosition(isDeco);
		}
	}

	private void SetZPosition(bool isDeco = false)
	{
		if (!Visuals)
		{
			Visuals = base.gameObject;
		}
		float num = ((!IsProp || !m_prop.PropSpawnController || !m_prop.PropSpawnController.IsOverrideZPosition) ? CameraLayerUtility.GetZDepth(CameraLayer, SubLayer, IsProp, isDeco) : m_prop.PropSpawnController.ZPositionOverride);
		if (CompareTag("Enemy") || CompareTag("Player"))
		{
			num = 0f;
		}
		if (CompareTag("Enemy") && Visuals.transform.lossyScale.z > 1f)
		{
			num *= Visuals.transform.lossyScale.z;
		}
		Visuals.transform.position = new Vector3(Visuals.transform.position.x, Visuals.transform.position.y, num);
		if (m_zPositionOverrides == null)
		{
			m_zPositionOverrides = base.gameObject.GetComponentsInChildren<ZPositionOverride>();
		}
		for (int i = 0; i < m_zPositionOverrides.Length; i++)
		{
			m_zPositionOverrides[i].SetZPosition();
		}
		if (m_childCameraLayerControllers == null)
		{
			ICameraLayerController[] componentsInChildren = base.gameObject.GetComponentsInChildren<CameraLayerController>();
			m_childCameraLayerControllers = componentsInChildren;
		}
		for (int j = 1; j < m_childCameraLayerControllers.Length; j++)
		{
			m_childCameraLayerControllers[j].SetSubLayer(m_childCameraLayerControllers[j].SubLayer);
		}
	}

	[SpecialName]
	GameObject IRootObj.get_gameObject()
	{
		return base.gameObject;
	}
}
