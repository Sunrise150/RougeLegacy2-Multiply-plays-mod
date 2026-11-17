using System.Runtime.CompilerServices;
using UnityEngine;

public class Deco : MonoBehaviour, IGenericPoolObj
{
	private Prop[] m_props;

	private CameraLayerController[] m_cameraLayerControllers;

	private Vector3 m_initialScale;

	private int m_nameHash;

	public bool IsFreePoolObj { get; set; }

	public bool IsAwakeCalled { get; protected set; }

	public int NameHash
	{
		get
		{
			if (m_nameHash == 0)
			{
				m_nameHash = Animator.StringToHash(base.name);
			}
			return m_nameHash;
		}
		set
		{
			m_nameHash = value;
		}
	}

	public CameraLayerController[] CameraLayerControllers
	{
		get
		{
			if (m_cameraLayerControllers == null)
			{
				m_cameraLayerControllers = base.gameObject.GetComponentsInChildren<CameraLayerController>(includeInactive: true);
				for (int i = 0; i < m_cameraLayerControllers.Length; i++)
				{
					m_cameraLayerControllers[i].SetIsDeco(isDeco: true);
				}
			}
			return m_cameraLayerControllers;
		}
	}

	public int PropCount => Props.Length;

	public Prop[] Props
	{
		get
		{
			if (m_props == null)
			{
				m_props = GetComponentsInChildren<Prop>();
			}
			return m_props;
		}
		private set
		{
			m_props = value;
		}
	}

	private void Awake()
	{
		m_initialScale = base.transform.localScale;
		IsAwakeCalled = true;
	}

	public void ResetValues()
	{
		Prop[] props = Props;
		foreach (Prop prop in props)
		{
			if (!prop.gameObject.activeSelf)
			{
				prop.gameObject.SetActive(value: true);
			}
			prop.ResetValues();
		}
		base.transform.localScale = m_initialScale;
	}

	public void SetCameraLayerControllers(Prop ownerProp)
	{
		if ((bool)ownerProp)
		{
			CameraLayerController[] cameraLayerControllers = CameraLayerControllers;
			foreach (CameraLayerController obj in cameraLayerControllers)
			{
				obj.SetCameraLayer(ownerProp.CameraLayerController.CameraLayer);
				obj.SetSubLayer(ownerProp.CameraLayerController.SubLayer, isDeco: true);
			}
		}
	}

	private void OnDisable()
	{
		DisablePooledObjectManager.DisablePooledObject(this);
	}

	[SpecialName]
	GameObject IGenericPoolObj.get_gameObject()
	{
		return base.gameObject;
	}
}
