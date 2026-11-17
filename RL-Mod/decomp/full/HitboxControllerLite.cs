using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class HitboxControllerLite : MonoBehaviour, IHitboxController
{
	protected struct RepeatHitEntry
	{
		public int ObjHashCode;

		public float HitDuration;

		public LayerType Layer;
	}

	[SerializeField]
	private CollisionType m_collisionType;

	[Space(5f)]
	[SerializeField]
	private GameObject m_bodyHitbox;

	[SerializeField]
	private GameObject m_weaponHitbox;

	[SerializeField]
	private GameObject m_terrainHitbox;

	[SerializeField]
	private GameObject m_platformHitbox;

	[SerializeField]
	[EnumFlag]
	private CollisionType m_weaponCollidesWithType;

	[SerializeField]
	[EnumFlag]
	private CollisionType m_terrainCollidesWithType;

	[Space(5f)]
	[SerializeField]
	private PlatformCollisionType m_platformCollisionType;

	[SerializeField]
	[HideInInspector]
	private bool m_hitboxesInitialized;

	[SerializeField]
	private bool m_initializeOnAwake = true;

	[SerializeField]
	private int m_repeatHitArraySizeOverride = -1;

	private Collider2D m_bodyHitboxCollider;

	private Collider2D m_weaponHitboxCollider;

	private Collider2D m_terrainHitboxCollider;

	private Collider2D m_platformCollider;

	private float m_repeatHitDuration = 0.5f;

	private Rigidbody2D m_rigidBody;

	private IDamageObj m_damageObj;

	private bool m_responseMethodsInitialized;

	private bool m_startExecuted;

	private int m_responseMethodCount;

	private IBodyOnEnterHitResponse[] m_bodyOnEnterHitResponse;

	private IBodyOnStayHitResponse[] m_bodyOnStayHitResponse;

	private IBodyOnExitHitResponse[] m_bodyOnExitHitResponse;

	private IWeaponOnEnterHitResponse[] m_weaponOnEnterHitResponse;

	private IWeaponOnStayHitResponse[] m_weaponOnStayHitResponse;

	private IWeaponOnExitHitResponse[] m_weaponOnExitHitResponse;

	private ITerrainOnEnterHitResponse[] m_terrainOnEnterHitResponse;

	private ITerrainOnStayHitResponse[] m_terrainOnStayHitResponse;

	private ITerrainOnExitHitResponse[] m_terrainOnExitHitResponse;

	private bool m_disableAllCollisions;

	private static List<GameObject> m_gameObjDisableList_STATIC = new List<GameObject>();

	private bool m_culled;

	private bool m_bodyCullState;

	private bool m_weaponCullState;

	private bool m_terrainCullState;

	private bool m_rigidBodyCullState;

	protected List<RepeatHitEntry> m_repeatHitCheckList;

	public bool ResponseMethodsInitialized => m_responseMethodsInitialized;

	public Collider2D LastCollidedWith { get; set; }

	public bool DisableAllCollisions
	{
		get
		{
			return m_disableAllCollisions;
		}
		set
		{
			m_disableAllCollisions = value;
			if ((bool)m_platformCollider)
			{
				if (m_disableAllCollisions)
				{
					m_platformCollider.enabled = false;
				}
				else
				{
					m_platformCollider.enabled = true;
				}
			}
		}
	}

	public IBodyOnEnterHitResponse[] BodyOnEnterHitResponseArray => m_bodyOnEnterHitResponse;

	public IWeaponOnEnterHitResponse[] WeaponOnEnterHitResponseArray => m_weaponOnEnterHitResponse;

	public ITerrainOnEnterHitResponse[] TerrainOnEnterHitResponseArray => m_terrainOnEnterHitResponse;

	public IBodyOnStayHitResponse[] BodyOnStayHitResponseArray => m_bodyOnStayHitResponse;

	public IWeaponOnStayHitResponse[] WeaponOnStayHitResponseArray => m_weaponOnStayHitResponse;

	public ITerrainOnStayHitResponse[] TerrainOnStayHitResponseArray => m_terrainOnStayHitResponse;

	public IBodyOnExitHitResponse[] BodyOnExitHitResponseList => m_bodyOnExitHitResponse;

	public IWeaponOnExitHitResponse[] WeaponOnExitHitResponseArray => m_weaponOnExitHitResponse;

	public ITerrainOnExitHitResponse[] TerrainOnExitHitResponseArray => m_terrainOnExitHitResponse;

	public float RepeatHitDuration
	{
		get
		{
			return m_repeatHitDuration;
		}
		set
		{
			m_repeatHitDuration = value;
		}
	}

	public GameObject RootGameObject { get; private set; }

	public IDamageObj DamageObj => m_damageObj;

	public GameObject BodyHitbox
	{
		get
		{
			return m_bodyHitbox;
		}
		set
		{
			m_bodyHitbox = value;
		}
	}

	public GameObject WeaponHitbox
	{
		get
		{
			return m_weaponHitbox;
		}
		set
		{
			m_weaponHitbox = value;
		}
	}

	public GameObject TerrainHitbox
	{
		get
		{
			return m_terrainHitbox;
		}
		set
		{
			m_terrainHitbox = value;
		}
	}

	public GameObject PlatformHitbox
	{
		get
		{
			return m_platformHitbox;
		}
		set
		{
			m_platformHitbox = value;
		}
	}

	public Collider2D PlatformCollider => m_platformCollider;

	public PlatformCollisionType PlatformCollisionType
	{
		get
		{
			return m_platformCollisionType;
		}
		set
		{
			m_platformCollisionType = value;
		}
	}

	public CollisionType CollisionType
	{
		get
		{
			return m_collisionType;
		}
		set
		{
			if (value == m_collisionType)
			{
				return;
			}
			m_collisionType = value;
			if (Application.isPlaying && IsInitialized)
			{
				string text = TagType_RL.ToString(CollisionType_RL.GetEquivalentTag(m_collisionType));
				HitboxInfo[] componentsInChildren = RootGameObject.GetComponentsInChildren<HitboxInfo>(includeInactive: true);
				for (int i = 0; i < componentsInChildren.Length; i++)
				{
					componentsInChildren[i].tag = text;
				}
			}
		}
	}

	public CollisionType WeaponCollidesWithType
	{
		get
		{
			return m_weaponCollidesWithType;
		}
		set
		{
			m_weaponCollidesWithType = value;
		}
	}

	public CollisionType TerrainCollidesWithType
	{
		get
		{
			return m_terrainCollidesWithType;
		}
		set
		{
			m_terrainCollidesWithType = value;
		}
	}

	public bool IsInitialized { get; private set; }

	public GameObject ContainsHitbox(HitboxType hitboxType, string hitboxName)
	{
		int length = hitboxName.Length;
		GameObject gameObject = null;
		switch (hitboxType)
		{
		case HitboxType.Body:
			gameObject = m_bodyHitbox;
			break;
		case HitboxType.Weapon:
			gameObject = m_weaponHitbox;
			break;
		case HitboxType.Terrain:
			gameObject = m_terrainHitbox;
			break;
		}
		if (gameObject != null && gameObject.name.Substring(0, length).Equals(hitboxName))
		{
			return gameObject;
		}
		return null;
	}

	public void ChangeCollisionType(HitboxType hitboxType, CollisionType collisionType)
	{
		string text = TagType_RL.ToString(CollisionType_RL.GetEquivalentTag(collisionType));
		GameObject gameObject = null;
		switch (hitboxType)
		{
		case HitboxType.Weapon:
			gameObject = m_weaponHitbox;
			break;
		case HitboxType.Body:
			gameObject = m_bodyHitbox;
			break;
		case HitboxType.Terrain:
			gameObject = m_terrainHitbox;
			break;
		case HitboxType.Platform:
			if (m_platformHitbox != null)
			{
				Debug.Log("WARNING: Changing collision type for platform. This is highly not recommended.");
				m_platformHitbox.tag = text;
			}
			return;
		}
		if (gameObject != null)
		{
			gameObject.tag = text;
		}
	}

	public Collider2D GetCollider(HitboxType hitboxType)
	{
		return hitboxType switch
		{
			HitboxType.Platform => m_platformCollider, 
			HitboxType.Body => m_bodyHitboxCollider, 
			HitboxType.Weapon => m_weaponHitboxCollider, 
			HitboxType.Terrain => m_terrainHitboxCollider, 
			_ => null, 
		};
	}

	private GameObject GetHitboxGO(HitboxType hbType)
	{
		return hbType switch
		{
			HitboxType.Body => m_bodyHitbox, 
			HitboxType.Weapon => m_weaponHitbox, 
			HitboxType.Terrain => m_terrainHitbox, 
			HitboxType.Platform => m_platformHitbox, 
			_ => null, 
		};
	}

	public void ChangeCanCollideWith(HitboxType hitboxType, CollisionType newCollisionType)
	{
		switch (hitboxType)
		{
		case HitboxType.Weapon:
			if (m_weaponHitboxCollider != null)
			{
				m_weaponHitboxCollider.GetComponent<HitboxInfo>().CollidesWithType = newCollisionType;
			}
			break;
		case HitboxType.Terrain:
			if (m_terrainHitboxCollider != null)
			{
				m_terrainHitboxCollider.GetComponent<HitboxInfo>().CollidesWithType = newCollisionType;
			}
			break;
		default:
			throw new Exception("Cannot change CollideWith type for any hitboxes other than Weapon or Terrain");
		}
	}

	private void OnEnable()
	{
		if (!Application.isPlaying)
		{
			m_hitboxesInitialized = false;
		}
		if (IsInitialized)
		{
			SetupIgnoreCollisions();
		}
		if (IsInitialized && m_startExecuted && !m_responseMethodsInitialized)
		{
			InitializeResponseMethods();
		}
	}

	private void OnDisable()
	{
		ResetRepeatHitChecks();
	}

	private void Awake()
	{
		if (m_initializeOnAwake && !IsInitialized)
		{
			Initialize();
		}
	}

	private IEnumerator Start()
	{
		m_startExecuted = true;
		if (m_initializeOnAwake)
		{
			while (!IsInitialized)
			{
				yield return null;
			}
			InitializeResponseMethods();
		}
	}

	public void Initialize()
	{
		RootGameObject = this.GetRoot();
		m_damageObj = RootGameObject.GetComponentInChildren<IDamageObj>();
		InitializeRepeatHitCheck();
		InitializeHitboxes();
		SetupIgnoreCollisions();
		if (!m_initializeOnAwake || (m_initializeOnAwake && m_startExecuted))
		{
			InitializeResponseMethods();
		}
		IsInitialized = true;
	}

	private void InitializeHitboxes()
	{
		if (!m_hitboxesInitialized)
		{
			m_gameObjDisableList_STATIC.Clear();
			if ((bool)m_bodyHitbox)
			{
				CreateHitbox(HitboxType.Body, m_gameObjDisableList_STATIC);
			}
			if ((bool)m_weaponHitbox)
			{
				CreateHitbox(HitboxType.Weapon, m_gameObjDisableList_STATIC);
			}
			if ((bool)m_terrainHitbox)
			{
				CreateHitbox(HitboxType.Terrain, m_gameObjDisableList_STATIC);
			}
			if ((bool)m_platformHitbox)
			{
				CreateHitbox(HitboxType.Platform, m_gameObjDisableList_STATIC);
			}
			foreach (GameObject item in m_gameObjDisableList_STATIC)
			{
				if (item != base.gameObject)
				{
					UnityEngine.Object.Destroy(item);
					continue;
				}
				Collider2D component = item.GetComponent<Collider2D>();
				if ((bool)component)
				{
					UnityEngine.Object.Destroy(component);
				}
			}
			m_hitboxesInitialized = true;
			return;
		}
		HitboxInfo[] componentsInChildren = RootGameObject.GetComponentsInChildren<HitboxInfo>(includeInactive: true);
		foreach (HitboxInfo hitboxInfo in componentsInChildren)
		{
			hitboxInfo.HitboxController = this;
			switch (hitboxInfo.gameObject.layer)
			{
			case 16:
			case 17:
				hitboxInfo.CollidesWithType = WeaponCollidesWithType;
				break;
			case 7:
			case 13:
			case 18:
			case 30:
				hitboxInfo.CollidesWithType = TerrainCollidesWithType;
				break;
			}
		}
		RepopulateColliders(HitboxType.Body);
		RepopulateColliders(HitboxType.Weapon);
		RepopulateColliders(HitboxType.Terrain);
		RepopulateColliders(HitboxType.Platform);
	}

	private void SetupIgnoreCollisions()
	{
		Collider2D[] componentsInChildren = RootGameObject.GetComponentsInChildren<Collider2D>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			for (int j = i; j < componentsInChildren.Length; j++)
			{
				Collider2D collider2D = componentsInChildren[i];
				Collider2D collider2D2 = componentsInChildren[j];
				if ((bool)collider2D && (bool)collider2D2 && collider2D != collider2D2)
				{
					Physics2D.IgnoreCollision(collider2D, collider2D2);
				}
			}
		}
	}

	public void RealignHitboxColliders()
	{
		PolygonCollider2D polygonCollider2D = null;
		polygonCollider2D = ((!IsInitialized) ? this.GetRoot().GetComponent<PolygonCollider2D>() : RootGameObject.GetComponent<PolygonCollider2D>());
		if ((bool)polygonCollider2D)
		{
			RealignBox2DColliderToPolyCollider(m_platformCollider, polygonCollider2D);
			RealignBox2DColliderToPolyCollider(m_terrainHitboxCollider, polygonCollider2D);
			RealignBox2DColliderToPolyCollider(m_weaponHitboxCollider, polygonCollider2D);
			RealignBox2DColliderToPolyCollider(m_bodyHitboxCollider, polygonCollider2D);
			UnityEngine.Object.DestroyImmediate(polygonCollider2D);
		}
	}

	private void RealignBox2DColliderToPolyCollider(Collider2D oldCollider, PolygonCollider2D polyCollider)
	{
		if ((bool)oldCollider && (bool)polyCollider)
		{
			BoxCollider2D boxCollider2D = oldCollider as BoxCollider2D;
			if ((bool)boxCollider2D && boxCollider2D.size == (Vector2)polyCollider.bounds.size)
			{
				boxCollider2D.offset = Vector2.zero;
				boxCollider2D.offset = polyCollider.bounds.center - boxCollider2D.bounds.center;
			}
		}
	}

	private void RepopulateColliders(HitboxType hbType)
	{
		if (GameUtility.IsInGame)
		{
			Debug.Log("<color=yellow>WARNING: HitboxController on: " + RootGameObject.name + " is being repopoulated, meaning the awake method on this HBController was called twice. This should only occur in level editor mode.</color>");
		}
		switch (hbType)
		{
		case HitboxType.Body:
			if (BodyHitbox != null)
			{
				m_bodyHitboxCollider = BodyHitbox.GetComponent<Collider2D>();
				BodyHitbox.GetComponent<HitboxInfo>().SetCollider(m_bodyHitboxCollider);
			}
			break;
		case HitboxType.Weapon:
			if (WeaponHitbox != null)
			{
				m_weaponHitboxCollider = WeaponHitbox.GetComponent<Collider2D>();
				WeaponHitbox.GetComponent<HitboxInfo>().SetCollider(m_weaponHitboxCollider);
			}
			break;
		case HitboxType.Terrain:
			if (TerrainHitbox != null)
			{
				m_terrainHitboxCollider = TerrainHitbox.GetComponent<Collider2D>();
				TerrainHitbox.GetComponent<HitboxInfo>().SetCollider(m_terrainHitboxCollider);
			}
			break;
		case HitboxType.Platform:
			if (m_platformHitbox != null)
			{
				m_platformCollider = m_platformHitbox.GetComponent<Collider2D>();
				m_platformHitbox.GetComponent<HitboxInfo>().SetCollider(m_platformCollider);
			}
			break;
		}
	}

	private void InitializeResponseMethods()
	{
		if (m_bodyHitbox != null)
		{
			CreateHitResponseMethods(HitboxType.Body, RootGameObject);
		}
		if (m_weaponHitbox != null)
		{
			CreateHitResponseMethods(HitboxType.Weapon, RootGameObject);
		}
		if (m_terrainHitbox != null)
		{
			CreateHitResponseMethods(HitboxType.Terrain, RootGameObject);
		}
		m_responseMethodsInitialized = true;
	}

	public IHitResponse[] GetHitResponseArray(HitboxType hbType, HitResponseType hbResponseType)
	{
		switch (hbType)
		{
		case HitboxType.Body:
			switch (hbResponseType)
			{
			case HitResponseType.OnEnter:
				return m_bodyOnEnterHitResponse;
			case HitResponseType.OnExit:
				return m_bodyOnExitHitResponse;
			case HitResponseType.OnStay:
				return m_bodyOnStayHitResponse;
			}
			break;
		case HitboxType.Weapon:
			switch (hbResponseType)
			{
			case HitResponseType.OnEnter:
				return m_weaponOnEnterHitResponse;
			case HitResponseType.OnExit:
				return m_weaponOnExitHitResponse;
			case HitResponseType.OnStay:
				return m_weaponOnStayHitResponse;
			}
			break;
		case HitboxType.Terrain:
			switch (hbResponseType)
			{
			case HitResponseType.OnEnter:
				return m_terrainOnEnterHitResponse;
			case HitResponseType.OnExit:
				return m_terrainOnExitHitResponse;
			case HitResponseType.OnStay:
				return m_terrainOnStayHitResponse;
			}
			break;
		}
		return null;
	}

	public void AddHitboxResponseMethod(IHitResponse hitResponse)
	{
		if (m_responseMethodsInitialized)
		{
			if (hitResponse is IBodyOnEnterHitResponse bodyOnEnterHitResponse && !m_bodyOnEnterHitResponse.Contains(bodyOnEnterHitResponse))
			{
				m_bodyOnEnterHitResponse = m_bodyOnEnterHitResponse.Add(bodyOnEnterHitResponse);
			}
			if (hitResponse is IBodyOnExitHitResponse bodyOnExitHitResponse && !m_bodyOnExitHitResponse.Contains(bodyOnExitHitResponse))
			{
				m_bodyOnExitHitResponse = m_bodyOnExitHitResponse.Add(bodyOnExitHitResponse);
			}
			if (hitResponse is IBodyOnStayHitResponse bodyOnStayHitResponse && !m_bodyOnStayHitResponse.Contains(bodyOnStayHitResponse))
			{
				m_bodyOnStayHitResponse = m_bodyOnStayHitResponse.Add(bodyOnStayHitResponse);
			}
			if (hitResponse is IWeaponOnEnterHitResponse weaponOnEnterHitResponse && !m_weaponOnEnterHitResponse.Contains(weaponOnEnterHitResponse))
			{
				m_weaponOnEnterHitResponse = m_weaponOnEnterHitResponse.Add(weaponOnEnterHitResponse);
			}
			if (hitResponse is IWeaponOnExitHitResponse weaponOnExitHitResponse && !m_weaponOnExitHitResponse.Contains(weaponOnExitHitResponse))
			{
				m_weaponOnExitHitResponse = m_weaponOnExitHitResponse.Add(weaponOnExitHitResponse);
			}
			if (hitResponse is IWeaponOnStayHitResponse weaponOnStayHitResponse && !m_weaponOnStayHitResponse.Contains(weaponOnStayHitResponse))
			{
				m_weaponOnStayHitResponse = m_weaponOnStayHitResponse.Add(weaponOnStayHitResponse);
			}
			if (hitResponse is ITerrainOnEnterHitResponse terrainOnEnterHitResponse && !m_terrainOnEnterHitResponse.Contains(terrainOnEnterHitResponse))
			{
				m_terrainOnEnterHitResponse = m_terrainOnEnterHitResponse.Add(terrainOnEnterHitResponse);
			}
			if (hitResponse is ITerrainOnExitHitResponse terrainOnExitHitResponse && !m_terrainOnExitHitResponse.Contains(terrainOnExitHitResponse))
			{
				m_terrainOnExitHitResponse = m_terrainOnExitHitResponse.Add(terrainOnExitHitResponse);
			}
			if (hitResponse is ITerrainOnStayHitResponse terrainOnStayHitResponse && !m_terrainOnStayHitResponse.Contains(terrainOnStayHitResponse))
			{
				m_terrainOnStayHitResponse = m_terrainOnStayHitResponse.Add(terrainOnStayHitResponse);
			}
		}
	}

	public void RemoveHitboxResponseMethod(IHitResponse hitResponse)
	{
		if (m_responseMethodsInitialized)
		{
			if (hitResponse is IBodyOnEnterHitResponse element)
			{
				m_bodyOnEnterHitResponse = m_bodyOnEnterHitResponse.Remove(element);
			}
			if (hitResponse is IBodyOnExitHitResponse element2)
			{
				m_bodyOnExitHitResponse = m_bodyOnExitHitResponse.Remove(element2);
			}
			if (hitResponse is IBodyOnStayHitResponse element3)
			{
				m_bodyOnStayHitResponse = m_bodyOnStayHitResponse.Remove(element3);
			}
			if (hitResponse is IWeaponOnEnterHitResponse element4)
			{
				m_weaponOnEnterHitResponse = m_weaponOnEnterHitResponse.Remove(element4);
			}
			if (hitResponse is IWeaponOnExitHitResponse element5)
			{
				m_weaponOnExitHitResponse = m_weaponOnExitHitResponse.Remove(element5);
			}
			if (hitResponse is IWeaponOnStayHitResponse element6)
			{
				m_weaponOnStayHitResponse = m_weaponOnStayHitResponse.Remove(element6);
			}
			if (hitResponse is ITerrainOnEnterHitResponse element7)
			{
				m_terrainOnEnterHitResponse = m_terrainOnEnterHitResponse.Remove(element7);
			}
			if (hitResponse is ITerrainOnExitHitResponse element8)
			{
				m_terrainOnExitHitResponse = m_terrainOnExitHitResponse.Remove(element8);
			}
			if (hitResponse is ITerrainOnStayHitResponse element9)
			{
				m_terrainOnStayHitResponse = m_terrainOnStayHitResponse.Remove(element9);
			}
		}
	}

	private void CreateHitbox(HitboxType hitboxType, List<GameObject> gameObjsToDisable)
	{
		if (hitboxType == HitboxType.Platform && !m_platformHitbox)
		{
			return;
		}
		LayerType layer = LayerType.Ignore_Raycast;
		GameObject gameObject = null;
		CollisionType collisionType = CollisionType.None;
		string text = null;
		switch (hitboxType)
		{
		case HitboxType.Platform:
			text = " (Active - Platform)";
			layer = (LayerType)m_platformHitbox.layer;
			gameObject = m_platformHitbox;
			break;
		case HitboxType.Body:
			text = " (Active - Body)";
			layer = LayerType.Body_Hitbox;
			gameObject = m_bodyHitbox;
			if (CollisionType == CollisionType.Player)
			{
				layer = LayerType.Body_Hitbox_ForPlayerOnly;
			}
			break;
		case HitboxType.Weapon:
			text = " (Active - Weapon)";
			layer = LayerType.Weapon_Hitbox;
			gameObject = m_weaponHitbox;
			collisionType = m_weaponCollidesWithType;
			if (((collisionType & CollisionType.Player) != 0 || (collisionType & CollisionType.Player_Dodging) != 0) && (collisionType & CollisionType.Enemy) == 0)
			{
				layer = LayerType.Weapon_Hitbox_HitsPlayerOnly;
			}
			break;
		case HitboxType.Terrain:
			text = " (Active - Terrain)";
			layer = LayerType.Terrain_Hitbox;
			gameObject = m_terrainHitbox;
			collisionType = m_terrainCollidesWithType;
			if ((m_terrainCollidesWithType & CollisionType.Platform) != 0)
			{
				layer = LayerType.Terrain_Hitbox_HitsPlatform;
			}
			if (CollisionType == CollisionType.Hazard || CollisionType == CollisionType.TriggerHazard)
			{
				layer = LayerType.TerrainHazard_Hitbox;
			}
			if (CollisionType == CollisionType.ItemDrop)
			{
				layer = LayerType.Terrain_Hitbox_ItemDrop;
			}
			break;
		}
		Collider2D component = gameObject.GetComponent<Collider2D>();
		if (!component || !component.isActiveAndEnabled)
		{
			return;
		}
		GameObject gameObject2 = gameObject;
		HBColliderType hBColliderType = HBColliderType.None;
		Transform parent = (((bool)gameObject.transform.parent && !(gameObject == RootGameObject)) ? gameObject.transform.parent : gameObject.transform);
		if (component is BoxCollider2D)
		{
			gameObject2 = UnityEngine.Object.Instantiate(UtilityLibrary.GetHitControllerBoxColliderGO(), parent, worldPositionStays: false);
			hBColliderType = HBColliderType.Box;
		}
		else if (component is CircleCollider2D)
		{
			gameObject2 = UnityEngine.Object.Instantiate(UtilityLibrary.GetHitControllerCircleColliderGO(), parent, worldPositionStays: false);
			hBColliderType = HBColliderType.Circle;
		}
		else if (component is CapsuleCollider2D)
		{
			gameObject2 = UnityEngine.Object.Instantiate(UtilityLibrary.GetHitControllerCapsuleColliderGO(), parent, worldPositionStays: false);
			hBColliderType = HBColliderType.Capsule;
		}
		else
		{
			if (!(component is PolygonCollider2D))
			{
				Debug.Log("<color=red>ERROR: Invalid collider type: " + component.GetType()?.ToString() + " found in HitboxControllerLite.  Cannot generate hitbox.</color>");
				return;
			}
			if (CDGHelper.CanPolyColliderBeBoxCollider(component as PolygonCollider2D))
			{
				gameObject2 = UnityEngine.Object.Instantiate(UtilityLibrary.GetHitControllerBoxColliderGO(), parent, worldPositionStays: false);
				hBColliderType = HBColliderType.PolygonToBox;
			}
			else
			{
				gameObject2 = UnityEngine.Object.Instantiate(UtilityLibrary.GetHitControllerPolygonColliderGO(), parent, worldPositionStays: false);
				hBColliderType = HBColliderType.Polygon;
			}
		}
		if (gameObject.gameObject != RootGameObject)
		{
			gameObject2.transform.localPosition = gameObject.transform.localPosition;
			gameObject2.transform.localScale = gameObject.transform.localScale;
			gameObject2.transform.localRotation = gameObject.transform.localRotation;
			if (hitboxType == HitboxType.Platform)
			{
				m_platformHitbox.tag = RootGameObject.tag;
			}
			else
			{
				gameObject2.tag = gameObject.tag;
			}
			if (!gameObjsToDisable.Contains(gameObject))
			{
				gameObjsToDisable.Add(gameObject);
			}
		}
		Collider2D component2 = gameObject2.GetComponent<Collider2D>();
		HitboxController.CopyColliderInfo(hBColliderType, component, component2);
		HitboxInfo component3 = gameObject2.GetComponent<HitboxInfo>();
		component3.HitboxController = this;
		component3.CollidesWithType = collisionType;
		TagType equivalentTag = CollisionType_RL.GetEquivalentTag(CollisionType);
		component3.tag = TagType_RL.ToString(equivalentTag);
		component3.SetCollider(component2);
		gameObject2.layer = (int)layer;
		gameObject2.name = gameObject.name + text;
		m_rigidBody = GetComponent<Rigidbody2D>();
		if (gameObject2.transform.parent == base.transform)
		{
			if (!m_rigidBody)
			{
				m_rigidBody = base.gameObject.AddComponent<Rigidbody2D>();
				m_rigidBody.bodyType = RigidbodyType2D.Kinematic;
				m_rigidBody.sleepMode = RigidbodySleepMode2D.NeverSleep;
			}
		}
		else
		{
			Rigidbody2D rigidbody2D = gameObject2.AddComponent<Rigidbody2D>();
			rigidbody2D.bodyType = RigidbodyType2D.Kinematic;
			rigidbody2D.sleepMode = RigidbodySleepMode2D.NeverSleep;
		}
		switch (hitboxType)
		{
		case HitboxType.Platform:
			m_platformCollider = component2;
			m_platformHitbox = m_platformCollider.gameObject;
			break;
		case HitboxType.Body:
			m_bodyHitboxCollider = component2;
			m_bodyHitbox = gameObject2;
			break;
		case HitboxType.Weapon:
			m_weaponHitboxCollider = component2;
			m_weaponHitbox = gameObject2;
			break;
		case HitboxType.Terrain:
			m_terrainHitboxCollider = component2;
			m_terrainHitbox = gameObject2;
			break;
		}
		if (hitboxType == HitboxType.Platform && (bool)m_platformHitbox)
		{
			switch (m_platformCollisionType)
			{
			case PlatformCollisionType.Platform_CollidesWithAll:
				m_platformHitbox.layer = 8;
				break;
			case PlatformCollisionType.Platform_CollidesWithEnemy:
				m_platformHitbox.layer = 10;
				break;
			case PlatformCollisionType.Platform_CollidesWithPlayer:
				m_platformHitbox.layer = 9;
				break;
			case PlatformCollisionType.Platform_OneWay:
				m_platformHitbox.layer = 11;
				break;
			case PlatformCollisionType.Character:
				m_platformHitbox.layer = 2;
				break;
			}
		}
	}

	private void CreateHitResponseMethods(HitboxType hitboxType, GameObject rootGameObj)
	{
		switch (hitboxType)
		{
		case HitboxType.Body:
			m_responseMethodCount += (m_bodyOnEnterHitResponse = rootGameObj.GetComponentsInChildren<IBodyOnEnterHitResponse>()).Length;
			m_responseMethodCount += (m_bodyOnStayHitResponse = rootGameObj.GetComponentsInChildren<IBodyOnStayHitResponse>()).Length;
			m_responseMethodCount += (m_bodyOnExitHitResponse = rootGameObj.GetComponentsInChildren<IBodyOnExitHitResponse>()).Length;
			break;
		case HitboxType.Weapon:
			m_responseMethodCount += (m_weaponOnEnterHitResponse = rootGameObj.GetComponentsInChildren<IWeaponOnEnterHitResponse>()).Length;
			m_responseMethodCount += (m_weaponOnStayHitResponse = rootGameObj.GetComponentsInChildren<IWeaponOnStayHitResponse>()).Length;
			m_responseMethodCount += (m_weaponOnExitHitResponse = rootGameObj.GetComponentsInChildren<IWeaponOnExitHitResponse>()).Length;
			break;
		case HitboxType.Terrain:
			m_responseMethodCount += (m_terrainOnEnterHitResponse = rootGameObj.GetComponentsInChildren<ITerrainOnEnterHitResponse>()).Length;
			m_responseMethodCount += (m_terrainOnStayHitResponse = rootGameObj.GetComponentsInChildren<ITerrainOnStayHitResponse>()).Length;
			m_responseMethodCount += (m_terrainOnExitHitResponse = rootGameObj.GetComponentsInChildren<ITerrainOnExitHitResponse>()).Length;
			break;
		}
	}

	public void HandleCollision(HitResponseType hitResponseType, HitboxInfo hbInfo, Collider2D otherCollider)
	{
		if (!ResponseMethodsInitialized)
		{
			return;
		}
		HitboxInfo component = otherCollider.GetComponent<HitboxInfo>();
		if (component != null && (component.HitboxController == this || component.HitboxController.DisableAllCollisions))
		{
			return;
		}
		LayerType layer = (LayerType)otherCollider.gameObject.layer;
		switch (layer)
		{
		case LayerType.Body_Hitbox_ForPlayerOnly:
		case LayerType.Body_Hitbox:
			if (!(component == null) && (hitResponseType == HitResponseType.OnExit || AllowRepeatHitCheckCollision(component.RootGameObj, layer)))
			{
				LastCollidedWith = otherCollider;
				InvokeAllWeaponHitResponseMethods(hitResponseType, component.HitboxController);
				component.HitboxController.LastCollidedWith = hbInfo.Collider;
				component.HitboxController.InvokeAllBodyHitResponseMethods(hitResponseType, this);
			}
			break;
		case LayerType.Terrain_Hitbox_ItemDrop:
		case LayerType.Platform_CollidesWithAll:
		case LayerType.Platform_CollidesWithPlayer:
		case LayerType.Platform_CollidesWithEnemy:
		case LayerType.Platform_OneWay:
		case LayerType.Terrain_Hitbox:
		case LayerType.Terrain_Hitbox_HitsPlatform:
		case LayerType.TerrainHazard_Hitbox:
		{
			IHitboxController otherHBController = null;
			GameObject gameObject = null;
			if (component != null)
			{
				otherHBController = component.HitboxController;
				gameObject = component.RootGameObj;
			}
			else
			{
				gameObject = otherCollider.gameObject;
			}
			if (hitResponseType == HitResponseType.OnExit || AllowRepeatHitCheckCollision(gameObject, layer))
			{
				LastCollidedWith = otherCollider;
				InvokeAllTerrainHitResponseMethods(hitResponseType, otherHBController);
			}
			break;
		}
		}
	}

	public void InvokeAllBodyHitResponseMethods(HitResponseType hitResponseType, IHitboxController otherHBController)
	{
		switch (hitResponseType)
		{
		case HitResponseType.OnEnter:
			if (m_bodyOnEnterHitResponse != null)
			{
				IBodyOnEnterHitResponse[] bodyOnEnterHitResponse = m_bodyOnEnterHitResponse;
				for (int i = 0; i < bodyOnEnterHitResponse.Length; i++)
				{
					bodyOnEnterHitResponse[i].BodyOnEnterHitResponse(otherHBController);
				}
			}
			break;
		case HitResponseType.OnStay:
			if (m_bodyOnStayHitResponse != null)
			{
				IBodyOnStayHitResponse[] bodyOnStayHitResponse = m_bodyOnStayHitResponse;
				for (int i = 0; i < bodyOnStayHitResponse.Length; i++)
				{
					bodyOnStayHitResponse[i].BodyOnStayHitResponse(otherHBController);
				}
			}
			break;
		case HitResponseType.OnExit:
			if (m_bodyOnExitHitResponse != null)
			{
				IBodyOnExitHitResponse[] bodyOnExitHitResponse = m_bodyOnExitHitResponse;
				for (int i = 0; i < bodyOnExitHitResponse.Length; i++)
				{
					bodyOnExitHitResponse[i].BodyOnExitHitResponse(otherHBController);
				}
			}
			break;
		}
	}

	public void InvokeAllWeaponHitResponseMethods(HitResponseType hitResponseType, IHitboxController otherHBController)
	{
		switch (hitResponseType)
		{
		case HitResponseType.OnEnter:
			if (m_weaponOnEnterHitResponse != null)
			{
				IWeaponOnEnterHitResponse[] weaponOnEnterHitResponse = m_weaponOnEnterHitResponse;
				for (int i = 0; i < weaponOnEnterHitResponse.Length; i++)
				{
					weaponOnEnterHitResponse[i].WeaponOnEnterHitResponse(otherHBController);
				}
			}
			break;
		case HitResponseType.OnStay:
			if (m_weaponOnStayHitResponse != null)
			{
				IWeaponOnStayHitResponse[] weaponOnStayHitResponse = m_weaponOnStayHitResponse;
				for (int i = 0; i < weaponOnStayHitResponse.Length; i++)
				{
					weaponOnStayHitResponse[i].WeaponOnStayHitResponse(otherHBController);
				}
			}
			break;
		case HitResponseType.OnExit:
			if (m_weaponOnExitHitResponse != null)
			{
				IWeaponOnExitHitResponse[] weaponOnExitHitResponse = m_weaponOnExitHitResponse;
				for (int i = 0; i < weaponOnExitHitResponse.Length; i++)
				{
					weaponOnExitHitResponse[i].WeaponOnExitHitResponse(otherHBController);
				}
			}
			break;
		}
	}

	public void InvokeAllTerrainHitResponseMethods(HitResponseType hitResponseType, IHitboxController otherHBController)
	{
		switch (hitResponseType)
		{
		case HitResponseType.OnEnter:
			if (m_terrainOnEnterHitResponse != null)
			{
				ITerrainOnEnterHitResponse[] terrainOnEnterHitResponse = m_terrainOnEnterHitResponse;
				for (int i = 0; i < terrainOnEnterHitResponse.Length; i++)
				{
					terrainOnEnterHitResponse[i].TerrainOnEnterHitResponse(otherHBController);
				}
			}
			break;
		case HitResponseType.OnStay:
			if (m_terrainOnStayHitResponse != null)
			{
				ITerrainOnStayHitResponse[] terrainOnStayHitResponse = m_terrainOnStayHitResponse;
				for (int i = 0; i < terrainOnStayHitResponse.Length; i++)
				{
					terrainOnStayHitResponse[i].TerrainOnStayHitResponse(otherHBController);
				}
			}
			break;
		case HitResponseType.OnExit:
			if (m_terrainOnExitHitResponse != null)
			{
				ITerrainOnExitHitResponse[] terrainOnExitHitResponse = m_terrainOnExitHitResponse;
				for (int i = 0; i < terrainOnExitHitResponse.Length; i++)
				{
					terrainOnExitHitResponse[i].TerrainOnExitHitResponse(otherHBController);
				}
			}
			break;
		}
	}

	public void SetHitboxActiveState(HitboxType hitboxType, bool active)
	{
		Collider2D collider2D = null;
		switch (hitboxType)
		{
		case HitboxType.Body:
			collider2D = m_bodyHitboxCollider;
			break;
		case HitboxType.Weapon:
			collider2D = m_weaponHitboxCollider;
			break;
		case HitboxType.Terrain:
			collider2D = m_terrainHitboxCollider;
			break;
		case HitboxType.Platform:
			collider2D = m_platformCollider;
			break;
		}
		if ((bool)collider2D)
		{
			collider2D.enabled = active;
		}
	}

	public bool GetHitboxActiveState(HitboxType hitboxType)
	{
		switch (hitboxType)
		{
		case HitboxType.Body:
			if ((bool)m_bodyHitboxCollider)
			{
				return m_bodyHitboxCollider.enabled;
			}
			break;
		case HitboxType.Weapon:
			if ((bool)m_weaponHitboxCollider)
			{
				return m_weaponHitboxCollider.enabled;
			}
			break;
		case HitboxType.Terrain:
			if ((bool)m_terrainHitboxCollider)
			{
				return m_terrainHitboxCollider.enabled;
			}
			break;
		case HitboxType.Platform:
			if ((bool)m_platformCollider)
			{
				return m_platformCollider.enabled;
			}
			break;
		}
		return false;
	}

	public void ResetAllHitboxStates()
	{
		SetHitboxActiveState(HitboxType.Weapon, active: true);
		SetHitboxActiveState(HitboxType.Body, active: true);
		SetHitboxActiveState(HitboxType.Terrain, active: true);
		SetHitboxActiveState(HitboxType.Platform, active: true);
	}

	public void SetCulledState(bool culled, bool includeRigidbody)
	{
		if (culled == m_culled)
		{
			return;
		}
		m_culled = culled;
		if (culled)
		{
			m_bodyCullState = (bool)m_bodyHitboxCollider && !m_bodyHitboxCollider.enabled;
			SetHitboxActiveState(HitboxType.Body, active: false);
			m_weaponCullState = (bool)m_weaponHitboxCollider && !m_weaponHitboxCollider.enabled;
			SetHitboxActiveState(HitboxType.Weapon, active: false);
			m_terrainCullState = (bool)m_terrainHitboxCollider && !m_terrainHitboxCollider.enabled;
			SetHitboxActiveState(HitboxType.Terrain, active: false);
			if ((bool)m_rigidBody && includeRigidbody)
			{
				m_rigidBodyCullState = !m_rigidBody.simulated;
				m_rigidBody.simulated = false;
			}
			return;
		}
		if (!m_bodyCullState)
		{
			SetHitboxActiveState(HitboxType.Body, active: true);
		}
		if (!m_weaponCullState)
		{
			SetHitboxActiveState(HitboxType.Weapon, active: true);
		}
		if (!m_terrainCullState)
		{
			SetHitboxActiveState(HitboxType.Terrain, active: true);
		}
		if ((bool)m_rigidBody && includeRigidbody && !m_rigidBodyCullState)
		{
			m_rigidBody.simulated = true;
		}
	}

	private void InitializeRepeatHitCheck()
	{
		if (m_repeatHitArraySizeOverride == -1)
		{
			m_repeatHitArraySizeOverride = 4;
		}
		m_repeatHitCheckList = new List<RepeatHitEntry>(m_repeatHitArraySizeOverride);
	}

	private bool AllowRepeatHitCheckCollision(GameObject obj, LayerType layer)
	{
		if (RepeatHitDuration <= 0f)
		{
			return true;
		}
		int num = -1;
		int count = m_repeatHitCheckList.Count;
		for (int i = 0; i < count; i++)
		{
			RepeatHitEntry repeatHitEntry = m_repeatHitCheckList[i];
			if (repeatHitEntry.ObjHashCode == obj.GetHashCode() && repeatHitEntry.Layer == layer && repeatHitEntry.HitDuration > Time.fixedTime)
			{
				return false;
			}
			if (repeatHitEntry.ObjHashCode == 0 || !(repeatHitEntry.HitDuration > Time.fixedTime))
			{
				num = i;
			}
		}
		RepeatHitEntry repeatHitEntry2 = default(RepeatHitEntry);
		repeatHitEntry2.ObjHashCode = obj.GetHashCode();
		repeatHitEntry2.HitDuration = Time.fixedTime + RepeatHitDuration;
		repeatHitEntry2.Layer = layer;
		if (num > -1)
		{
			m_repeatHitCheckList[num] = repeatHitEntry2;
		}
		else
		{
			m_repeatHitCheckList.Add(repeatHitEntry2);
		}
		return true;
	}

	public void RemoveFromRepeatHitChecks(GameObject obj)
	{
		if (m_repeatHitCheckList == null)
		{
			return;
		}
		int count = m_repeatHitCheckList.Count;
		for (int i = 0; i < count; i++)
		{
			RepeatHitEntry value = m_repeatHitCheckList[i];
			if (value.ObjHashCode == obj.GetHashCode())
			{
				value.ObjHashCode = 0;
				value.HitDuration = 0f;
				value.Layer = LayerType.None;
				m_repeatHitCheckList[i] = value;
			}
		}
	}

	public void ResetRepeatHitChecks()
	{
		if (m_repeatHitCheckList != null)
		{
			int count = m_repeatHitCheckList.Count;
			for (int i = 0; i < count; i++)
			{
				RepeatHitEntry value = m_repeatHitCheckList[i];
				value.ObjHashCode = 0;
				value.HitDuration = 0f;
				value.Layer = LayerType.None;
				m_repeatHitCheckList[i] = value;
			}
		}
	}

	[SpecialName]
	GameObject IHitboxController.get_gameObject()
	{
		return base.gameObject;
	}
}
