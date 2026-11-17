using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class HitboxController : MonoBehaviour, IHitboxController
{
	protected struct RepeatHitEntry
	{
		public int ObjHashCode;

		public float HitDuration;

		public LayerType Layer;
	}

	public const int REPEAT_HIT_ARRAY_DEFAULT_SIZE = 4;

	public const string PLATFORM_TITLE = " (Active - Platform)";

	public const string BODY_TITLE = " (Active - Body)";

	public const string WEAPON_TITLE = " (Active - Weapon)";

	public const string TERRAIN_TITLE = " (Active - Terrain)";

	[Space(5f)]
	[SerializeField]
	private CollisionType m_collisionType;

	private float m_repeatHitDuration = 0.5f;

	[Space(5f)]
	[Header("Body Hitboxes")]
	[SerializeField]
	private GameObject[] m_bodyHitboxList = new GameObject[0];

	[Space(5f)]
	[Header("Weapon Hitboxes")]
	[SerializeField]
	private GameObject[] m_weaponHitboxList = new GameObject[0];

	[Space(5f)]
	[Header("Terrain Hitboxes")]
	[SerializeField]
	private GameObject[] m_terrainHitboxList = new GameObject[0];

	[Space(5f)]
	[Header("Platform Hitbox")]
	[SerializeField]
	private GameObject m_platformHitbox;

	[SerializeField]
	private PlatformCollisionType m_platformCollisionType;

	[SerializeField]
	[EnumFlag]
	private CollisionType m_weaponCollisionType;

	[SerializeField]
	[EnumFlag]
	private CollisionType m_terrainCollisionType;

	[SerializeField]
	[HideInInspector]
	private bool m_hitboxesInitialized;

	[SerializeField]
	private bool m_initializeOnAwake = true;

	[SerializeField]
	private int m_repeatHitArraySizeOverride = -1;

	private Collider2D[] m_terrainColliderList;

	private Collider2D[] m_bodyColliderList;

	private Collider2D[] m_weaponColliderList;

	private Collider2D m_platformCollider;

	private Rigidbody2D m_rigidBody;

	private ITerrainOnEnterHitResponse[] m_terrainOnEnterResponseArray;

	private ITerrainOnStayHitResponse[] m_terrainOnStayResponseArray;

	private ITerrainOnExitHitResponse[] m_terrainOnExitResponseArray;

	private IBodyOnEnterHitResponse[] m_bodyOnEnterResponseArray;

	private IBodyOnStayHitResponse[] m_bodyOnStayResponseArray;

	private IBodyOnExitHitResponse[] m_bodyOnExitResponseArray;

	private IWeaponOnEnterHitResponse[] m_weaponOnEnterResponseArray;

	private IWeaponOnStayHitResponse[] m_weaponOnStayResponseArray;

	private IWeaponOnExitHitResponse[] m_weaponOnExitResponseArray;

	private IDamageObj m_damageObj;

	private int m_responseMethodCount;

	private bool m_responseMethodsInitialized;

	private bool m_startExecuted;

	private static List<string> m_warningIssuedTracker = new List<string>();

	private bool m_disableAllCollisions;

	private static List<GameObject> m_gameObjDisableList_STATIC = new List<GameObject>();

	private static List<Collider2D> m_colliderListHelper = new List<Collider2D>();

	private static GameObject[] m_platformArrayHelper = new GameObject[1];

	private int m_cachedWeaponHitboxStates;

	private int m_cachedBodyHitboxStates;

	private int m_cachedTerrainHitboxStates;

	private bool m_weaponHitboxStatesDisabled;

	private bool m_bodyHitboxStatesDisabled;

	private bool m_terrainHitboxStatesDisabled;

	private bool m_culled;

	private bool m_bodyCullState;

	private bool m_weaponCullState;

	private bool m_terrainCullState;

	private bool m_rigidBodyCullState;

	protected List<RepeatHitEntry> m_repeatHitCheckList;

	public bool ResponseMethodsInitialized => m_responseMethodsInitialized;

	public Collider2D LastCollidedWith { get; set; }

	public GameObject RootGameObject { get; private set; }

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

	public CollisionType WeaponCollidesWithType
	{
		get
		{
			return m_weaponCollisionType;
		}
		set
		{
			m_weaponCollisionType = value;
		}
	}

	public CollisionType TerrainCollidesWithType
	{
		get
		{
			return m_terrainCollisionType;
		}
		set
		{
			m_terrainCollisionType = value;
		}
	}

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

	public IDamageObj DamageObj => m_damageObj;

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

	public bool IsInitialized { get; private set; }

	public GameObject[] WeaponHitboxList
	{
		get
		{
			return m_weaponHitboxList;
		}
		set
		{
			m_weaponHitboxList = value;
		}
	}

	public GameObject[] BodyHitboxList
	{
		get
		{
			return m_bodyHitboxList;
		}
		set
		{
			m_bodyHitboxList = value;
		}
	}

	public GameObject[] TerrainHitboxList
	{
		get
		{
			return m_terrainHitboxList;
		}
		set
		{
			m_terrainHitboxList = value;
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

	public Collider2D[] BodyColliderList => m_bodyColliderList;

	public Collider2D[] WeaponColliderList => m_weaponColliderList;

	public Collider2D[] TerrainColliderList => m_terrainColliderList;

	public IBodyOnEnterHitResponse[] BodyOnEnterHitResponseArray => m_bodyOnEnterResponseArray;

	public IWeaponOnEnterHitResponse[] WeaponOnEnterHitResponseArray => m_weaponOnEnterResponseArray;

	public ITerrainOnEnterHitResponse[] TerrainOnEnterHitResponseArray => m_terrainOnEnterResponseArray;

	public IBodyOnStayHitResponse[] BodyOnStayHitResponseArray => m_bodyOnStayResponseArray;

	public IWeaponOnStayHitResponse[] WeaponOnStayHitResponseArray => m_weaponOnStayResponseArray;

	public ITerrainOnStayHitResponse[] TerrainOnStayHitResponseArray => m_terrainOnStayResponseArray;

	public IBodyOnExitHitResponse[] BodyOnExitHitResponseList => m_bodyOnExitResponseArray;

	public IWeaponOnExitHitResponse[] WeaponOnExitHitResponseArray => m_weaponOnExitResponseArray;

	public ITerrainOnExitHitResponse[] TerrainOnExitHitResponseArray => m_terrainOnExitResponseArray;

	public GameObject ContainsHitbox(HitboxType hitboxType, string hitboxName)
	{
		int length = hitboxName.Length;
		GameObject[] array = null;
		switch (hitboxType)
		{
		case HitboxType.Body:
			array = m_bodyHitboxList;
			break;
		case HitboxType.Weapon:
			array = m_weaponHitboxList;
			break;
		case HitboxType.Terrain:
			array = m_terrainHitboxList;
			break;
		}
		if (array != null)
		{
			GameObject[] array2 = array;
			foreach (GameObject gameObject in array2)
			{
				if (gameObject.name.Substring(0, length).Equals(hitboxName))
				{
					return gameObject;
				}
			}
		}
		return null;
	}

	public void ChangeCollisionType(HitboxType hitboxType, CollisionType collisionType)
	{
		string text = TagType_RL.ToString(CollisionType_RL.GetEquivalentTag(collisionType));
		GameObject[] array = null;
		switch (hitboxType)
		{
		case HitboxType.Weapon:
			array = m_weaponHitboxList;
			break;
		case HitboxType.Body:
			array = m_bodyHitboxList;
			break;
		case HitboxType.Terrain:
			array = m_terrainHitboxList;
			break;
		case HitboxType.Platform:
			if (m_platformHitbox != null)
			{
				Debug.Log("WARNING: Changing collision type for platform. This is highly not recommended.");
				m_platformHitbox.tag = text;
			}
			return;
		}
		if (array != null)
		{
			GameObject[] array2 = array;
			for (int i = 0; i < array2.Length; i++)
			{
				array2[i].tag = text;
			}
		}
	}

	public void ChangeCanCollideWith(HitboxType hitboxType, CollisionType newCollisionType)
	{
		switch (hitboxType)
		{
		case HitboxType.Weapon:
		{
			Collider2D[] terrainColliderList = m_weaponColliderList;
			for (int i = 0; i < terrainColliderList.Length; i++)
			{
				terrainColliderList[i].GetComponent<HitboxInfo>().CollidesWithType = newCollisionType;
			}
			break;
		}
		case HitboxType.Terrain:
		{
			Collider2D[] terrainColliderList = m_terrainColliderList;
			for (int i = 0; i < terrainColliderList.Length; i++)
			{
				terrainColliderList[i].GetComponent<HitboxInfo>().CollidesWithType = newCollisionType;
			}
			break;
		}
		default:
			throw new Exception("Cannot change CollideWith type for any hitboxes other than Weapon or Terrain");
		}
	}

	private GameObject[] GetHitboxGOArray(HitboxType hbType)
	{
		return hbType switch
		{
			HitboxType.Body => m_bodyHitboxList, 
			HitboxType.Weapon => m_weaponHitboxList, 
			HitboxType.Terrain => m_terrainHitboxList, 
			_ => null, 
		};
	}

	public bool IsTouching(HitboxType hitbox, Collider2D collider)
	{
		Collider2D[] array = null;
		switch (hitbox)
		{
		case HitboxType.Platform:
			return GetCollider(HitboxType.Platform).IsTouching(collider);
		case HitboxType.Terrain:
			array = TerrainColliderList;
			break;
		case HitboxType.Body:
			array = BodyColliderList;
			break;
		case HitboxType.Weapon:
			array = WeaponColliderList;
			break;
		default:
			Debug.LogFormat("{0}: HitboxController.IsTouching has no switch case for Hitbox Type ({1})", Time.frameCount, hitbox);
			return false;
		}
		Collider2D[] array2 = array;
		for (int i = 0; i < array2.Length; i++)
		{
			if (array2[i].IsTouching(collider))
			{
				return true;
			}
		}
		return false;
	}

	public Collider2D GetCollider(HitboxType hitboxType)
	{
		switch (hitboxType)
		{
		case HitboxType.Platform:
			return m_platformCollider;
		case HitboxType.Body:
			if (m_bodyColliderList != null && m_bodyColliderList.Length != 0)
			{
				return m_bodyColliderList[0];
			}
			break;
		case HitboxType.Weapon:
			if (m_weaponColliderList != null && m_weaponColliderList.Length != 0)
			{
				return m_weaponColliderList[0];
			}
			break;
		case HitboxType.Terrain:
			if (m_terrainColliderList != null && m_terrainColliderList.Length != 0)
			{
				return m_terrainColliderList[0];
			}
			break;
		}
		return null;
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
			CreateHitboxes(HitboxType.Body, m_gameObjDisableList_STATIC);
			CreateHitboxes(HitboxType.Weapon, m_gameObjDisableList_STATIC);
			CreateHitboxes(HitboxType.Terrain, m_gameObjDisableList_STATIC);
			CreateHitboxes(HitboxType.Platform, m_gameObjDisableList_STATIC);
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
			Array.Clear(m_bodyHitboxList, 0, m_bodyHitboxList.Length);
			m_bodyHitboxList = new GameObject[m_bodyColliderList.Length];
			for (int i = 0; i < m_bodyColliderList.Length; i++)
			{
				m_bodyHitboxList[i] = m_bodyColliderList[i].gameObject;
			}
			Array.Clear(m_weaponHitboxList, 0, m_weaponHitboxList.Length);
			m_weaponHitboxList = new GameObject[m_weaponColliderList.Length];
			for (int j = 0; j < m_weaponColliderList.Length; j++)
			{
				m_weaponHitboxList[j] = m_weaponColliderList[j].gameObject;
			}
			Array.Clear(m_terrainHitboxList, 0, m_terrainHitboxList.Length);
			m_terrainHitboxList = new GameObject[m_terrainColliderList.Length];
			for (int k = 0; k < m_terrainColliderList.Length; k++)
			{
				m_terrainHitboxList[k] = m_terrainColliderList[k].gameObject;
			}
			m_hitboxesInitialized = true;
			return;
		}
		if (GameUtility.IsInGame)
		{
			Debug.Log("<color=yellow>WARNING: HitboxController on: " + RootGameObject.name + " is being repopoulated, meaning the awake method on this HBController was called twice. This should only occur in level editor mode.</color>");
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
			Collider2D[] terrainColliderList = m_terrainColliderList;
			foreach (Collider2D oldCollider in terrainColliderList)
			{
				RealignBox2DColliderToPolyCollider(oldCollider, polygonCollider2D);
			}
			terrainColliderList = m_weaponColliderList;
			foreach (Collider2D oldCollider2 in terrainColliderList)
			{
				RealignBox2DColliderToPolyCollider(oldCollider2, polygonCollider2D);
			}
			terrainColliderList = m_bodyColliderList;
			foreach (Collider2D oldCollider3 in terrainColliderList)
			{
				RealignBox2DColliderToPolyCollider(oldCollider3, polygonCollider2D);
			}
			UnityEngine.Object.DestroyImmediate(polygonCollider2D);
		}
	}

	private void RealignBox2DColliderToPolyCollider(Collider2D oldCollider, PolygonCollider2D polyCollider)
	{
		if (!(oldCollider == null) && !(polyCollider == null))
		{
			BoxCollider2D boxCollider2D = oldCollider as BoxCollider2D;
			if ((bool)boxCollider2D && boxCollider2D.size == (Vector2)polyCollider.bounds.size)
			{
				boxCollider2D.offset = Vector2.zero;
				boxCollider2D.offset = polyCollider.bounds.center - boxCollider2D.bounds.center;
			}
		}
	}

	private void InitializeResponseMethods()
	{
		m_bodyOnEnterResponseArray = PopulateResponseMethods<IBodyOnEnterHitResponse>(RootGameObject);
		m_bodyOnStayResponseArray = PopulateResponseMethods<IBodyOnStayHitResponse>(RootGameObject);
		m_bodyOnExitResponseArray = PopulateResponseMethods<IBodyOnExitHitResponse>(RootGameObject);
		m_weaponOnEnterResponseArray = PopulateResponseMethods<IWeaponOnEnterHitResponse>(RootGameObject);
		m_weaponOnStayResponseArray = PopulateResponseMethods<IWeaponOnStayHitResponse>(RootGameObject);
		m_weaponOnExitResponseArray = PopulateResponseMethods<IWeaponOnExitHitResponse>(RootGameObject);
		m_terrainOnEnterResponseArray = PopulateResponseMethods<ITerrainOnEnterHitResponse>(RootGameObject);
		m_terrainOnStayResponseArray = PopulateResponseMethods<ITerrainOnStayHitResponse>(RootGameObject);
		m_terrainOnExitResponseArray = PopulateResponseMethods<ITerrainOnExitHitResponse>(RootGameObject);
		m_responseMethodsInitialized = true;
	}

	private T[] PopulateResponseMethods<T>(GameObject gameObj)
	{
		T[] componentsInChildren = gameObj.GetComponentsInChildren<T>(includeInactive: false);
		if (componentsInChildren.Length != 0)
		{
			m_responseMethodCount += componentsInChildren.Length;
			return componentsInChildren;
		}
		return null;
	}

	public IHitResponse[] GetHitResponseArray(HitboxType hbType, HitResponseType hbResponseType)
	{
		switch (hbType)
		{
		case HitboxType.Body:
			switch (hbResponseType)
			{
			case HitResponseType.OnEnter:
				return m_bodyOnEnterResponseArray;
			case HitResponseType.OnExit:
				return m_bodyOnExitResponseArray;
			case HitResponseType.OnStay:
				return m_bodyOnStayResponseArray;
			}
			break;
		case HitboxType.Weapon:
			switch (hbResponseType)
			{
			case HitResponseType.OnEnter:
				return m_weaponOnEnterResponseArray;
			case HitResponseType.OnExit:
				return m_weaponOnExitResponseArray;
			case HitResponseType.OnStay:
				return m_weaponOnStayResponseArray;
			}
			break;
		case HitboxType.Terrain:
			switch (hbResponseType)
			{
			case HitResponseType.OnEnter:
				return m_terrainOnEnterResponseArray;
			case HitResponseType.OnExit:
				return m_terrainOnExitResponseArray;
			case HitResponseType.OnStay:
				return m_terrainOnStayResponseArray;
			}
			break;
		}
		return null;
	}

	public void AddHitboxResponseMethod(IHitResponse hitResponse)
	{
		if (m_responseMethodsInitialized)
		{
			if (hitResponse is IBodyOnEnterHitResponse bodyOnEnterHitResponse && !m_bodyOnEnterResponseArray.Contains(bodyOnEnterHitResponse))
			{
				m_bodyOnEnterResponseArray = m_bodyOnEnterResponseArray.Add(bodyOnEnterHitResponse);
			}
			if (hitResponse is IBodyOnExitHitResponse bodyOnExitHitResponse && !m_bodyOnExitResponseArray.Contains(bodyOnExitHitResponse))
			{
				m_bodyOnExitResponseArray = m_bodyOnExitResponseArray.Add(bodyOnExitHitResponse);
			}
			if (hitResponse is IBodyOnStayHitResponse bodyOnStayHitResponse && !m_bodyOnStayResponseArray.Contains(bodyOnStayHitResponse))
			{
				m_bodyOnStayResponseArray = m_bodyOnStayResponseArray.Add(bodyOnStayHitResponse);
			}
			if (hitResponse is IWeaponOnEnterHitResponse weaponOnEnterHitResponse && !m_weaponOnEnterResponseArray.Contains(weaponOnEnterHitResponse))
			{
				m_weaponOnEnterResponseArray = m_weaponOnEnterResponseArray.Add(weaponOnEnterHitResponse);
			}
			if (hitResponse is IWeaponOnExitHitResponse weaponOnExitHitResponse && !m_weaponOnExitResponseArray.Contains(weaponOnExitHitResponse))
			{
				m_weaponOnExitResponseArray = m_weaponOnExitResponseArray.Add(weaponOnExitHitResponse);
			}
			if (hitResponse is IWeaponOnStayHitResponse weaponOnStayHitResponse && !m_weaponOnStayResponseArray.Contains(weaponOnStayHitResponse))
			{
				m_weaponOnStayResponseArray = m_weaponOnStayResponseArray.Add(weaponOnStayHitResponse);
			}
			if (hitResponse is ITerrainOnEnterHitResponse terrainOnEnterHitResponse && !m_terrainOnEnterResponseArray.Contains(terrainOnEnterHitResponse))
			{
				m_terrainOnEnterResponseArray = m_terrainOnEnterResponseArray.Add(terrainOnEnterHitResponse);
			}
			if (hitResponse is ITerrainOnExitHitResponse terrainOnExitHitResponse && !m_terrainOnExitResponseArray.Contains(terrainOnExitHitResponse))
			{
				m_terrainOnExitResponseArray = m_terrainOnExitResponseArray.Add(terrainOnExitHitResponse);
			}
			if (hitResponse is ITerrainOnStayHitResponse terrainOnStayHitResponse && !m_terrainOnStayResponseArray.Contains(terrainOnStayHitResponse))
			{
				m_terrainOnStayResponseArray = m_terrainOnStayResponseArray.Add(terrainOnStayHitResponse);
			}
		}
	}

	public void RemoveHitboxResponseMethod(IHitResponse hitResponse)
	{
		if (m_responseMethodsInitialized)
		{
			if (hitResponse is IBodyOnEnterHitResponse element)
			{
				m_bodyOnEnterResponseArray = m_bodyOnEnterResponseArray.Remove(element);
			}
			if (hitResponse is IBodyOnExitHitResponse element2)
			{
				m_bodyOnExitResponseArray = m_bodyOnExitResponseArray.Remove(element2);
			}
			if (hitResponse is IBodyOnStayHitResponse element3)
			{
				m_bodyOnStayResponseArray = m_bodyOnStayResponseArray.Remove(element3);
			}
			if (hitResponse is IWeaponOnEnterHitResponse element4)
			{
				m_weaponOnEnterResponseArray = m_weaponOnEnterResponseArray.Remove(element4);
			}
			if (hitResponse is IWeaponOnExitHitResponse element5)
			{
				m_weaponOnExitResponseArray = m_weaponOnExitResponseArray.Remove(element5);
			}
			if (hitResponse is IWeaponOnStayHitResponse element6)
			{
				m_weaponOnStayResponseArray = m_weaponOnStayResponseArray.Remove(element6);
			}
			if (hitResponse is ITerrainOnEnterHitResponse element7)
			{
				m_terrainOnEnterResponseArray = m_terrainOnEnterResponseArray.Remove(element7);
			}
			if (hitResponse is ITerrainOnExitHitResponse element8)
			{
				m_terrainOnExitResponseArray = m_terrainOnExitResponseArray.Remove(element8);
			}
			if (hitResponse is ITerrainOnStayHitResponse element9)
			{
				m_terrainOnStayResponseArray = m_terrainOnStayResponseArray.Remove(element9);
			}
		}
	}

	private void RepopulateColliders(HitboxType hbType)
	{
		List<Collider2D> list = new List<Collider2D>();
		switch (hbType)
		{
		case HitboxType.Body:
		{
			if (m_bodyColliderList != null)
			{
				Array.Clear(m_bodyColliderList, 0, m_bodyColliderList.Length);
			}
			GameObject[] weaponHitboxList = m_bodyHitboxList;
			foreach (GameObject gameObject3 in weaponHitboxList)
			{
				Collider2D[] components3 = gameObject3.GetComponents<Collider2D>();
				Collider2D[] array = components3;
				foreach (Collider2D item3 in array)
				{
					list.Add(item3);
				}
				HitboxInfo component3 = gameObject3.GetComponent<HitboxInfo>();
				if (components3.Length != 0)
				{
					component3.SetCollider(components3[0]);
				}
			}
			m_bodyColliderList = list.ToArray();
			break;
		}
		case HitboxType.Weapon:
		{
			if (m_weaponColliderList != null)
			{
				Array.Clear(m_weaponColliderList, 0, m_weaponColliderList.Length);
			}
			GameObject[] weaponHitboxList = m_weaponHitboxList;
			foreach (GameObject gameObject in weaponHitboxList)
			{
				Collider2D[] components = gameObject.GetComponents<Collider2D>();
				Collider2D[] array = components;
				foreach (Collider2D item in array)
				{
					list.Add(item);
				}
				HitboxInfo component = gameObject.GetComponent<HitboxInfo>();
				if (components.Length != 0)
				{
					component.SetCollider(components[0]);
				}
			}
			m_weaponColliderList = list.ToArray();
			break;
		}
		case HitboxType.Terrain:
		{
			if (m_terrainColliderList != null)
			{
				Array.Clear(m_terrainColliderList, 0, m_terrainColliderList.Length);
			}
			GameObject[] weaponHitboxList = m_terrainHitboxList;
			foreach (GameObject gameObject2 in weaponHitboxList)
			{
				Collider2D[] components2 = gameObject2.GetComponents<Collider2D>();
				Collider2D[] array = components2;
				foreach (Collider2D item2 in array)
				{
					list.Add(item2);
				}
				HitboxInfo component2 = gameObject2.GetComponent<HitboxInfo>();
				if (components2.Length != 0)
				{
					component2.SetCollider(components2[0]);
				}
			}
			m_terrainColliderList = list.ToArray();
			break;
		}
		case HitboxType.Platform:
			if ((bool)m_platformHitbox)
			{
				m_platformCollider = m_platformHitbox.GetComponent<Collider2D>();
				m_platformHitbox.GetComponent<HitboxInfo>().SetCollider(m_platformCollider);
			}
			break;
		}
	}

	private void CreateHitboxes(HitboxType hitboxType, List<GameObject> gameObjsToDisable)
	{
		if (hitboxType == HitboxType.Platform && !m_platformHitbox)
		{
			return;
		}
		m_colliderListHelper.Clear();
		GameObject[] array = null;
		CollisionType collisionType = CollisionType.None;
		LayerType layer = LayerType.Ignore_Raycast;
		string text = null;
		switch (hitboxType)
		{
		case HitboxType.Platform:
			text = " (Active - Platform)";
			m_platformArrayHelper[0] = m_platformHitbox;
			array = m_platformArrayHelper;
			break;
		case HitboxType.Body:
			text = " (Active - Body)";
			layer = LayerType.Body_Hitbox;
			array = m_bodyHitboxList;
			if (CollisionType == CollisionType.Player)
			{
				layer = LayerType.Body_Hitbox_ForPlayerOnly;
			}
			break;
		case HitboxType.Weapon:
			text = " (Active - Weapon)";
			layer = LayerType.Weapon_Hitbox;
			array = m_weaponHitboxList;
			collisionType = WeaponCollidesWithType;
			if (((collisionType & CollisionType.Player) != 0 || (collisionType & CollisionType.Player_Dodging) != 0) && (collisionType & CollisionType.Enemy) == 0)
			{
				layer = LayerType.Weapon_Hitbox_HitsPlayerOnly;
			}
			break;
		case HitboxType.Terrain:
			text = " (Active - Terrain)";
			layer = LayerType.Terrain_Hitbox;
			array = m_terrainHitboxList;
			collisionType = TerrainCollidesWithType;
			if ((collisionType & CollisionType.Platform) != 0)
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
		GameObject[] array2 = array;
		foreach (GameObject gameObject in array2)
		{
			Collider2D component = gameObject.GetComponent<Collider2D>();
			if (!component || !component.isActiveAndEnabled)
			{
				continue;
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
					Debug.Log("<color=red>ERROR: Invalid collider type: " + component.GetType()?.ToString() + " found in HitboxController.  Cannot generate hitbox.</color>");
					continue;
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
			CopyColliderInfo(hBColliderType, component, component2);
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
			m_colliderListHelper.Add(component2);
		}
		switch (hitboxType)
		{
		case HitboxType.Platform:
			m_platformCollider = m_colliderListHelper[0];
			m_platformHitbox = m_platformCollider.gameObject;
			break;
		case HitboxType.Body:
			m_bodyColliderList = m_colliderListHelper.ToArray();
			break;
		case HitboxType.Weapon:
			m_weaponColliderList = m_colliderListHelper.ToArray();
			break;
		case HitboxType.Terrain:
			m_terrainColliderList = m_colliderListHelper.ToArray();
			break;
		}
		if (hitboxType == HitboxType.Platform && (bool)m_platformHitbox)
		{
			switch (m_platformCollisionType)
			{
			case PlatformCollisionType.Platform_CollidesWithAll:
				m_platformHitbox.layer = LayerMask.NameToLayer("Platform_CollidesWithAll");
				break;
			case PlatformCollisionType.Platform_CollidesWithEnemy:
				m_platformHitbox.layer = LayerMask.NameToLayer("Platform_CollidesWithEnemy");
				break;
			case PlatformCollisionType.Platform_CollidesWithPlayer:
				m_platformHitbox.layer = LayerMask.NameToLayer("Platform_CollidesWithPlayer");
				break;
			case PlatformCollisionType.Platform_OneWay:
				m_platformHitbox.layer = LayerMask.NameToLayer("Platform_OneWay");
				break;
			case PlatformCollisionType.Character:
				m_platformHitbox.layer = LayerMask.NameToLayer("Ignore Raycast");
				break;
			}
		}
	}

	public static void CopyColliderInfo(HBColliderType hbColliderType, Collider2D oldCollider, Collider2D newCollider)
	{
		newCollider.offset = oldCollider.offset;
		switch (hbColliderType)
		{
		case HBColliderType.Box:
		{
			BoxCollider2D boxCollider2D = oldCollider as BoxCollider2D;
			(newCollider as BoxCollider2D).size = boxCollider2D.size;
			break;
		}
		case HBColliderType.Circle:
		{
			CircleCollider2D circleCollider2D = oldCollider as CircleCollider2D;
			(newCollider as CircleCollider2D).radius = circleCollider2D.radius;
			break;
		}
		case HBColliderType.Capsule:
		{
			CapsuleCollider2D capsuleCollider2D = oldCollider as CapsuleCollider2D;
			CapsuleCollider2D obj2 = newCollider as CapsuleCollider2D;
			obj2.size = capsuleCollider2D.size;
			obj2.direction = capsuleCollider2D.direction;
			break;
		}
		case HBColliderType.PolygonToBox:
		{
			PolygonCollider2D polygonCollider2D3 = oldCollider as PolygonCollider2D;
			BoxCollider2D obj = newCollider as BoxCollider2D;
			Vector3 localEulerAngles = polygonCollider2D3.transform.localEulerAngles;
			polygonCollider2D3.transform.localEulerAngles = Vector3.zero;
			obj.size = polygonCollider2D3.bounds.size;
			obj.offset = polygonCollider2D3.bounds.center - polygonCollider2D3.gameObject.transform.position;
			polygonCollider2D3.transform.localEulerAngles = localEulerAngles;
			break;
		}
		case HBColliderType.Polygon:
		{
			PolygonCollider2D polygonCollider2D = oldCollider as PolygonCollider2D;
			PolygonCollider2D polygonCollider2D2 = newCollider as PolygonCollider2D;
			polygonCollider2D2.pathCount = polygonCollider2D.pathCount;
			for (int i = 0; i < polygonCollider2D.pathCount; i++)
			{
				polygonCollider2D2.SetPath(i, polygonCollider2D.GetPath(i));
			}
			break;
		}
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
			if (BodyOnEnterHitResponseArray != null)
			{
				IBodyOnEnterHitResponse[] bodyOnEnterHitResponseArray = BodyOnEnterHitResponseArray;
				for (int i = 0; i < bodyOnEnterHitResponseArray.Length; i++)
				{
					bodyOnEnterHitResponseArray[i].BodyOnEnterHitResponse(otherHBController);
				}
			}
			break;
		case HitResponseType.OnStay:
			if (BodyOnStayHitResponseArray != null)
			{
				IBodyOnStayHitResponse[] bodyOnStayHitResponseArray = BodyOnStayHitResponseArray;
				for (int i = 0; i < bodyOnStayHitResponseArray.Length; i++)
				{
					bodyOnStayHitResponseArray[i].BodyOnStayHitResponse(otherHBController);
				}
			}
			break;
		case HitResponseType.OnExit:
			if (BodyOnExitHitResponseList != null)
			{
				IBodyOnExitHitResponse[] bodyOnExitHitResponseList = BodyOnExitHitResponseList;
				for (int i = 0; i < bodyOnExitHitResponseList.Length; i++)
				{
					bodyOnExitHitResponseList[i].BodyOnExitHitResponse(otherHBController);
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
			if (WeaponOnEnterHitResponseArray != null)
			{
				IWeaponOnEnterHitResponse[] weaponOnEnterHitResponseArray = WeaponOnEnterHitResponseArray;
				for (int i = 0; i < weaponOnEnterHitResponseArray.Length; i++)
				{
					weaponOnEnterHitResponseArray[i].WeaponOnEnterHitResponse(otherHBController);
				}
			}
			break;
		case HitResponseType.OnStay:
			if (WeaponOnStayHitResponseArray != null)
			{
				IWeaponOnStayHitResponse[] weaponOnStayHitResponseArray = WeaponOnStayHitResponseArray;
				for (int i = 0; i < weaponOnStayHitResponseArray.Length; i++)
				{
					weaponOnStayHitResponseArray[i].WeaponOnStayHitResponse(otherHBController);
				}
			}
			break;
		case HitResponseType.OnExit:
			if (WeaponOnExitHitResponseArray != null)
			{
				IWeaponOnExitHitResponse[] weaponOnExitHitResponseArray = WeaponOnExitHitResponseArray;
				for (int i = 0; i < weaponOnExitHitResponseArray.Length; i++)
				{
					weaponOnExitHitResponseArray[i].WeaponOnExitHitResponse(otherHBController);
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
			if (TerrainOnEnterHitResponseArray != null)
			{
				ITerrainOnEnterHitResponse[] terrainOnEnterHitResponseArray = TerrainOnEnterHitResponseArray;
				for (int i = 0; i < terrainOnEnterHitResponseArray.Length; i++)
				{
					terrainOnEnterHitResponseArray[i].TerrainOnEnterHitResponse(otherHBController);
				}
			}
			break;
		case HitResponseType.OnStay:
			if (TerrainOnStayHitResponseArray != null)
			{
				ITerrainOnStayHitResponse[] terrainOnStayHitResponseArray = TerrainOnStayHitResponseArray;
				for (int i = 0; i < terrainOnStayHitResponseArray.Length; i++)
				{
					terrainOnStayHitResponseArray[i].TerrainOnStayHitResponse(otherHBController);
				}
			}
			break;
		case HitResponseType.OnExit:
			if (TerrainOnExitHitResponseArray != null)
			{
				ITerrainOnExitHitResponse[] terrainOnExitHitResponseArray = TerrainOnExitHitResponseArray;
				for (int i = 0; i < terrainOnExitHitResponseArray.Length; i++)
				{
					terrainOnExitHitResponseArray[i].TerrainOnExitHitResponse(otherHBController);
				}
			}
			break;
		}
	}

	public void SetHitboxActiveState(HitboxType hitboxType, bool active)
	{
		Collider2D[] array = null;
		int num = 0;
		switch (hitboxType)
		{
		case HitboxType.Body:
			array = BodyColliderList;
			num = m_cachedBodyHitboxStates;
			if (!active)
			{
				if (m_bodyHitboxStatesDisabled)
				{
					SetHitboxActiveState(HitboxType.Body, active: true);
				}
				m_bodyHitboxStatesDisabled = true;
			}
			else
			{
				m_bodyHitboxStatesDisabled = false;
			}
			break;
		case HitboxType.Weapon:
			array = WeaponColliderList;
			num = m_cachedWeaponHitboxStates;
			if (!active)
			{
				if (m_weaponHitboxStatesDisabled)
				{
					SetHitboxActiveState(HitboxType.Weapon, active: true);
				}
				m_weaponHitboxStatesDisabled = true;
			}
			else
			{
				m_weaponHitboxStatesDisabled = false;
			}
			break;
		case HitboxType.Terrain:
			array = TerrainColliderList;
			num = m_cachedTerrainHitboxStates;
			if (!active)
			{
				if (m_terrainHitboxStatesDisabled)
				{
					SetHitboxActiveState(HitboxType.Terrain, active: true);
				}
				m_terrainHitboxStatesDisabled = true;
			}
			else
			{
				m_terrainHitboxStatesDisabled = false;
			}
			break;
		case HitboxType.Platform:
			if ((bool)m_platformCollider)
			{
				m_platformCollider.enabled = active;
			}
			break;
		}
		if (array == null)
		{
			return;
		}
		if (!active)
		{
			CacheHitboxState(hitboxType);
			for (int i = 0; i < array.Length; i++)
			{
				array[i].enabled = false;
			}
			return;
		}
		for (int j = 0; j < array.Length; j++)
		{
			Collider2D collider2D = array[j];
			if ((num & (1 << j)) == 0)
			{
				collider2D.enabled = true;
			}
		}
	}

	public bool GetHitboxActiveState(HitboxType hitboxType)
	{
		switch (hitboxType)
		{
		case HitboxType.Body:
			return !m_bodyHitboxStatesDisabled;
		case HitboxType.Weapon:
			return !m_weaponHitboxStatesDisabled;
		case HitboxType.Terrain:
			return !m_terrainHitboxStatesDisabled;
		case HitboxType.Platform:
			if ((bool)m_platformCollider)
			{
				return m_platformCollider.enabled;
			}
			break;
		}
		return false;
	}

	private void CacheHitboxState(HitboxType hitboxType)
	{
		Collider2D[] array = null;
		switch (hitboxType)
		{
		case HitboxType.Body:
			array = BodyColliderList;
			break;
		case HitboxType.Weapon:
			array = WeaponColliderList;
			break;
		case HitboxType.Terrain:
			array = TerrainColliderList;
			break;
		}
		int num = 0;
		for (int i = 0; i < array.Length; i++)
		{
			num = ((!array[i].enabled) ? (num | (1 << i)) : (num & ~(1 << i)));
		}
		switch (hitboxType)
		{
		case HitboxType.Body:
			m_cachedBodyHitboxStates = num;
			break;
		case HitboxType.Weapon:
			m_cachedWeaponHitboxStates = num;
			break;
		case HitboxType.Terrain:
			m_cachedTerrainHitboxStates = num;
			break;
		}
	}

	public void ResetAllHitboxStates()
	{
		SetHitboxActiveState(HitboxType.Weapon, active: false);
		SetHitboxActiveState(HitboxType.Body, active: false);
		SetHitboxActiveState(HitboxType.Terrain, active: false);
		SetHitboxActiveState(HitboxType.Platform, active: false);
		m_cachedTerrainHitboxStates = 0;
		m_cachedBodyHitboxStates = 0;
		m_cachedWeaponHitboxStates = 0;
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
			m_bodyCullState = m_bodyHitboxStatesDisabled;
			SetHitboxActiveState(HitboxType.Body, active: false);
			m_weaponCullState = m_weaponHitboxStatesDisabled;
			SetHitboxActiveState(HitboxType.Weapon, active: false);
			m_terrainCullState = m_terrainHitboxStatesDisabled;
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
