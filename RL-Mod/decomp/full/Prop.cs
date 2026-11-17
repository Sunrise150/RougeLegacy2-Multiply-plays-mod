using System.Collections;
using System.Runtime.CompilerServices;
using UnityEngine;

public class Prop : MonoBehaviour, IGenericPoolObj, IMirror, IPivotObj
{
	[SerializeField]
	private bool m_verticalTuck;

	[SerializeField]
	private bool m_flipWhenMirrored = true;

	[SerializeField]
	private bool m_maintainUniformScale;

	[SerializeField]
	private GameObject m_pivot;

	private CorgiController_RL m_corgiController;

	private Renderer[] m_renderers;

	private PointLightController[] m_lightControllers;

	private bool m_isMirrored;

	private Animator m_animator;

	private Material[] m_materials;

	private bool m_hasNPCController;

	private bool m_hasForegroundSwapper;

	private bool m_isOnForeground;

	public bool HitboxesDisabled { get; set; }

	public bool IsCulled { get; set; }

	public IRoomConsumer[] RoomConsumers { get; private set; }

	public bool IsFreePoolObj { get; set; }

	public bool IsAwakeCalled { get; protected set; }

	public Light[] Lights { get; private set; }

	public CorgiController_RL CorgiController => m_corgiController;

	public Breakable Breakable { get; private set; }

	public BaseRoom Room { get; private set; }

	public DecoController[] DecoControllers { get; private set; }

	public PropSpawnController PropSpawnController { get; private set; }

	public ICameraLayerController CameraLayerController { get; private set; }

	public IHitboxController HitboxController { get; private set; }

	public GameObject Pivot => m_pivot;

	public Animator[] Animators { get; private set; }

	public bool IsInitialized { get; private set; }

	private void Awake()
	{
		m_renderers = GetComponentsInChildren<Renderer>(includeInactive: true);
		m_materials = new Material[m_renderers.Length];
		for (int i = 0; i < m_materials.Length; i++)
		{
			m_materials[i] = m_renderers[i].sharedMaterial;
		}
		Breakable = GetComponent<Breakable>();
		CameraLayerController = GetComponent<ICameraLayerController>();
		DecoControllers = GetComponents<DecoController>();
		m_animator = GetComponent<Animator>();
		m_lightControllers = GetComponentsInChildren<PointLightController>();
		Lights = GetComponentsInChildren<Light>();
		m_corgiController = GetComponent<CorgiController_RL>();
		if ((bool)m_corgiController)
		{
			m_corgiController.DisableYSlopeOffsetCheck = true;
			m_corgiController.DisableCastRaysAbove = true;
			m_corgiController.NumberOfHorizontalRays = 0;
			m_corgiController.PermanentlyDisableUponTouchingPlatform = true;
		}
		Animators = GetComponentsInChildren<Animator>();
		HitboxController = GetComponentInChildren<IHitboxController>();
		RoomConsumers = GetComponentsInChildren<IRoomConsumer>();
		m_hasForegroundSwapper = GetComponent<ForegroundPropMeshSpriteSwapper>();
		m_hasNPCController = GetComponent<NPCController>();
		IsAwakeCalled = true;
	}

	private void OnValidate()
	{
		if (base.gameObject.tag != "Prop")
		{
			base.gameObject.tag = "Prop";
		}
	}

	public virtual void Initialize(BaseRoom room, PropSpawnController propSpawnController)
	{
		SetRoom(room);
		PropSpawnController = propSpawnController;
		if (m_hasForegroundSwapper)
		{
			ForegroundPropMeshSpriteSwapper component = GetComponent<ForegroundPropMeshSpriteSwapper>();
			if (PropSpawnController.CameraLayer == CameraLayer.Foreground_PERSP)
			{
				component.MeshRenderer.gameObject.SetActive(value: false);
				component.SpriteRenderer.gameObject.SetActive(value: true);
			}
			else
			{
				component.MeshRenderer.gameObject.SetActive(value: true);
				component.SpriteRenderer.gameObject.SetActive(value: false);
			}
		}
		bool flag = propSpawnController.CameraLayer == CameraLayer.Foreground_PERSP && !m_hasNPCController;
		if (m_isOnForeground || flag)
		{
			m_isOnForeground = flag;
			BiomeArtData biomeArtData = Room.BiomeArtDataOverride;
			if (!biomeArtData)
			{
				biomeArtData = BiomeArtDataLibrary.GetArtData(Room.AppearanceBiomeType);
			}
			int num = m_renderers.Length;
			for (int i = 0; i < num; i++)
			{
				SpriteRenderer spriteRenderer = m_renderers[i] as SpriteRenderer;
				if ((bool)spriteRenderer)
				{
					if (m_isOnForeground)
					{
						spriteRenderer.sharedMaterial = biomeArtData.ForegroundData.ForegroundSpriteMaterial;
					}
					else
					{
						spriteRenderer.sharedMaterial = m_materials[i];
					}
				}
			}
		}
		SpriteDrawMode spriteRendererDrawMode = propSpawnController.SpriteRendererDrawMode;
		Vector2 spriteRendererSize = propSpawnController.SpriteRendererSize;
		Vector3 position = propSpawnController.gameObject.transform.position;
		float num2 = position.y;
		if (spriteRendererDrawMode == SpriteDrawMode.Sliced || spriteRendererDrawMode == SpriteDrawMode.Tiled)
		{
			float num3 = spriteRendererSize.y;
			float x = spriteRendererSize.x;
			if (m_verticalTuck && propSpawnController.CameraLayer == CameraLayer.Background_Near_PERSP)
			{
				float tuckYOffset = CameraLayerUtility.GetTuckYOffset(propSpawnController.CameraLayer, propSpawnController.SubLayer);
				num2 += tuckYOffset;
				num3 += 2f * Mathf.Abs(tuckYOffset);
			}
			for (int j = 0; j < m_renderers.Length; j++)
			{
				SpriteRenderer spriteRenderer2 = m_renderers[j] as SpriteRenderer;
				if ((bool)spriteRenderer2 && (spriteRenderer2.drawMode == SpriteDrawMode.Sliced || spriteRenderer2.drawMode == SpriteDrawMode.Tiled))
				{
					ResizeSlicedOrTiledSpriteRenderer(spriteRenderer2, new Vector2(x, num3));
				}
			}
		}
		base.transform.position = new Vector3(position.x, num2, 0f);
		if ((bool)m_corgiController && (bool)Pivot)
		{
			Pivot.transform.localScale = propSpawnController.gameObject.transform.localScale;
			Pivot.transform.rotation = propSpawnController.gameObject.transform.rotation;
			if (m_maintainUniformScale)
			{
				MaintainUniformScale(Pivot.transform);
			}
		}
		else
		{
			base.transform.rotation = propSpawnController.gameObject.transform.rotation;
			base.transform.localScale = propSpawnController.gameObject.transform.localScale;
			if (m_maintainUniformScale)
			{
				MaintainUniformScale(base.transform);
			}
		}
		if (propSpawnController.IsMirrored)
		{
			Mirror();
		}
		if (!CameraLayerController.IsNativeNull())
		{
			CameraLayerController.SetCameraLayer(propSpawnController.CameraLayer);
			CameraLayerController.SetSubLayer(propSpawnController.ActualSubLayer);
		}
		for (int k = 0; k < m_lightControllers.Length; k++)
		{
			m_lightControllers[k].UpdateLocation(propSpawnController.CameraLayer);
		}
		if ((bool)m_corgiController)
		{
			StartCoroutine(ResetCorgiControllerRayParameters(position));
		}
		IsInitialized = true;
	}

	protected virtual void ResizeSlicedOrTiledSpriteRenderer(SpriteRenderer spriteRenderer, Vector2 newSize)
	{
		spriteRenderer.size = newSize;
	}

	private void MaintainUniformScale(Transform propTransform)
	{
		Vector3 localScale = propTransform.localScale;
		float num = Mathf.Abs(localScale.x);
		float num2 = Mathf.Abs(localScale.y);
		if (num > num2)
		{
			bool num3 = localScale.y < 0f;
			localScale.y = num;
			if (num3 && localScale.y > 0f)
			{
				localScale.y *= -1f;
			}
		}
		else
		{
			bool num4 = localScale.x < 0f;
			localScale.x = num2;
			if (num4 && localScale.x > 0f)
			{
				localScale.x *= -1f;
			}
		}
		propTransform.localScale = localScale;
	}

	private void PerformRaycastReposition(Vector3 spawnPos)
	{
		Vector2 origin = spawnPos;
		float num = m_corgiController.Width() / 2f;
		origin.y += 0.1f;
		RaycastHit2D raycastHit2D = Physics2D.Raycast(origin, Vector2.left, num, m_corgiController.PlatformMask);
		if ((bool)raycastHit2D)
		{
			float num2 = raycastHit2D.point.x - (origin.x - num) + 0.01f;
			base.transform.position = new Vector3(spawnPos.x + num2, spawnPos.y, spawnPos.z);
			return;
		}
		raycastHit2D = Physics2D.Raycast(origin, Vector2.right, num, m_corgiController.PlatformMask);
		if ((bool)raycastHit2D)
		{
			float num3 = origin.x + num - raycastHit2D.point.x + 0.01f;
			base.transform.position = new Vector3(spawnPos.x - num3, spawnPos.y, spawnPos.z);
		}
	}

	private IEnumerator ResetCorgiControllerRayParameters(Vector3 spawnPos)
	{
		while (!m_corgiController.IsInitialized)
		{
			yield return null;
		}
		m_corgiController.ResetState();
		m_corgiController.SetRaysParameters();
		m_corgiController.SetHorizontalForce(0f);
	}

	public virtual void Mirror()
	{
		m_isMirrored = true;
		if (m_flipWhenMirrored)
		{
			Vector3 localScale = base.transform.localScale;
			localScale.x *= -1f;
			base.transform.localScale = localScale;
		}
		float z = base.transform.rotation.eulerAngles.z;
		if (z < -0.1f || z > 0.1f)
		{
			z *= -1f;
			Quaternion rotation = Quaternion.Euler(0f, 0f, z);
			base.transform.rotation = rotation;
		}
	}

	public void SetRoom(BaseRoom value)
	{
		Room = value;
	}

	public void ResetValues()
	{
		if ((bool)m_corgiController)
		{
			m_corgiController.ResetPermanentDisable();
		}
		if (m_isMirrored)
		{
			m_isMirrored = false;
			if (m_flipWhenMirrored)
			{
				base.transform.localScale = new Vector3(-1f * base.transform.localScale.x, base.transform.localScale.y, base.transform.localScale.z);
			}
			float z = base.transform.rotation.eulerAngles.z;
			if (z < -0.1f || z > 0.1f)
			{
				z *= -1f;
				Quaternion rotation = Quaternion.Euler(0f, 0f, z);
				base.transform.rotation = rotation;
			}
		}
		if ((bool)Breakable)
		{
			Breakable.ForceBrokenState(isBroken: false);
		}
	}

	public void ResetInitialization()
	{
		IsInitialized = false;
	}

	private void OnDisable()
	{
		if ((bool)m_animator)
		{
			m_animator.WriteDefaultValues();
		}
		IsFreePoolObj = true;
	}

	[SpecialName]
	GameObject IGenericPoolObj.get_gameObject()
	{
		return base.gameObject;
	}
}
