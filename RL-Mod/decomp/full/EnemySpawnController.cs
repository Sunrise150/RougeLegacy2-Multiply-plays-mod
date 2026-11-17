using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class EnemySpawnController : MonoBehaviour, ISpawnController, IHasProjectileNameArray
{
	private const int GROUNDING_RAYCAST_DISTANCE = 20;

	public static Dictionary<Color, int> ColorTable = new Dictionary<Color, int>
	{
		{
			Color.white,
			0
		},
		{
			Color.red,
			1
		},
		{
			Color.green,
			2
		},
		{
			Color.blue,
			3
		},
		{
			Color.cyan,
			4
		},
		{
			Color.magenta,
			5
		},
		{
			Color.yellow,
			6
		},
		{
			Color.grey,
			7
		}
	};

	public static Dictionary<int, Color> IDToColorTable = new Dictionary<int, Color>
	{
		{
			0,
			Color.white
		},
		{
			1,
			Color.red
		},
		{
			2,
			Color.green
		},
		{
			3,
			Color.blue
		},
		{
			4,
			Color.cyan
		},
		{
			5,
			Color.magenta
		},
		{
			6,
			Color.yellow
		},
		{
			7,
			Color.grey
		}
	};

	public static Dictionary<Color, string> ColorNameTable = new Dictionary<Color, string>
	{
		{
			Color.white,
			"White"
		},
		{
			Color.red,
			"Red"
		},
		{
			Color.green,
			"Green"
		},
		{
			Color.blue,
			"Blue"
		},
		{
			Color.cyan,
			"Cyan"
		},
		{
			Color.magenta,
			"Magenta"
		},
		{
			Color.yellow,
			"Yellow"
		},
		{
			Color.grey,
			"Grey"
		}
	};

	private EnemyController m_enemy;

	private Vector3 m_spawnPoint = new Vector3(-1000f, -1000f, -1000f);

	private BaseRoom m_room;

	private SpawnLogicController m_spawnLogic;

	private int m_enemyIndex = -1;

	private string m_gizmoText = "";

	private static RaycastHit2D[] m_raycastAllocArray = new RaycastHit2D[5];

	private Collider2D m_spawnCollider;

	public static string[] EmptyProjectileNameArray_STATIC = new string[0];

	private bool m_isDead;

	private Action<object, EnemyDeathEventArgs> m_onEnemyDeath;

	private Action<object, EnemyActivationStateChangedEventArgs> m_onEnemyTimedOut;

	[SerializeField]
	private bool m_forceFlying;

	[SerializeField]
	private bool m_forceCommander;

	[SerializeField]
	private bool m_override;

	[SerializeField]
	private bool m_overrideLevel;

	[SerializeField]
	private EnemyType m_type;

	[SerializeField]
	private EnemyRank m_rank = EnemyRank.None;

	[SerializeField]
	private int m_level = -1;

	[SerializeField]
	private int m_id = -1;

	[NonSerialized]
	private string[] m_projectileNameArray;

	private bool m_hasCheckedForSpawnLogicController;

	public string[] ProjectileNameArray
	{
		get
		{
			if (m_projectileNameArray == null)
			{
				BaseAIScript aIScript = EnemyClassLibrary.GetEnemyClassData(Type).GetAIScript(Rank);
				if ((bool)aIScript && aIScript.ProjectileNameArray != null)
				{
					m_projectileNameArray = aIScript.ProjectileNameArray;
				}
				else
				{
					m_projectileNameArray = EmptyProjectileNameArray_STATIC;
				}
			}
			return m_projectileNameArray;
		}
	}

	public EnemyController EnemyInstance
	{
		get
		{
			return m_enemy;
		}
		private set
		{
			m_enemy = value;
		}
	}

	public int EnemyIndex
	{
		get
		{
			return m_enemyIndex;
		}
		private set
		{
			m_enemyIndex = value;
		}
	}

	public bool ForceCommander
	{
		get
		{
			return m_forceCommander;
		}
		private set
		{
			m_forceCommander = value;
		}
	}

	public bool ForceFlying
	{
		get
		{
			return m_forceFlying;
		}
		private set
		{
			m_forceFlying = value;
		}
	}

	public GameObject GameObject => base.gameObject;

	public int ID
	{
		get
		{
			return m_id;
		}
		private set
		{
			m_id = value;
		}
	}

	public bool IsDead
	{
		get
		{
			if (!ShouldSpawn)
			{
				return true;
			}
			return m_isDead;
		}
		private set
		{
			m_isDead = value;
		}
	}

	public bool ShouldSpawn
	{
		get
		{
			if (SpawnLogicController != null)
			{
				return SpawnLogicController.ShouldSpawn;
			}
			return true;
		}
	}

	public int Level
	{
		get
		{
			return m_level;
		}
		set
		{
			m_level = value;
		}
	}

	public bool Override
	{
		get
		{
			return m_override;
		}
		set
		{
			m_override = value;
		}
	}

	public bool OverrideLevel
	{
		get
		{
			return m_overrideLevel;
		}
		set
		{
			m_overrideLevel = value;
		}
	}

	public EnemyRank Rank
	{
		get
		{
			if ((bool)Room && Room.SpawnedAsEasyRoom)
			{
				return EnemyRank.Basic;
			}
			return m_rank;
		}
		set
		{
			m_rank = value;
		}
	}

	public BaseRoom Room => m_room;

	public SpawnLogicController SpawnLogicController
	{
		get
		{
			if (!m_hasCheckedForSpawnLogicController)
			{
				m_hasCheckedForSpawnLogicController = true;
				m_spawnLogic = GetComponent<SpawnLogicController>();
			}
			return m_spawnLogic;
		}
		private set
		{
			m_spawnLogic = value;
		}
	}

	public EnemyType Type
	{
		get
		{
			return m_type;
		}
		set
		{
			m_type = value;
		}
	}

	private void Awake()
	{
		m_onEnemyDeath = OnEnemyDeath;
		m_onEnemyTimedOut = OnEnemyTimedOut;
	}

	private void OnDrawGizmos()
	{
	}

	private void OnDrawGizmosSelected()
	{
		Gizmos.DrawSphere(m_spawnPoint, 0.5f);
	}

	private Vector3 GetSpawnPoint(bool isFlying)
	{
		if (isFlying)
		{
			return base.transform.position;
		}
		Array.Clear(m_raycastAllocArray, 0, m_raycastAllocArray.Length);
		LayerMask layerMask = LayerMask.GetMask("Platform_CollidesWithAll", "Platform_CollidesWithEnemy", "Platform_OneWay");
		Physics2D.RaycastNonAlloc(base.transform.position + new Vector3(0f, 0.1f, 0f), Vector2.down, m_raycastAllocArray, 20f, layerMask);
		RaycastHit2D[] raycastAllocArray = m_raycastAllocArray;
		for (int i = 0; i < raycastAllocArray.Length; i++)
		{
			RaycastHit2D raycastHit2D = raycastAllocArray[i];
			if ((bool)raycastHit2D)
			{
				GameObject root = raycastHit2D.collider.GetRoot();
				if (root.CompareTag("Platform") || root.CompareTag("OneWay"))
				{
					m_spawnCollider = raycastHit2D.collider;
					return raycastHit2D.point;
				}
			}
		}
		return base.transform.position;
	}

	private void ResetEnemyPosition(EnemyController enemy)
	{
		if (m_spawnPoint == new Vector3(-1000f, -1000f, -1000f))
		{
			EnemyData enemyData = EnemyClassLibrary.GetEnemyData(Type, Rank);
			m_spawnPoint = GetSpawnPoint(enemyData.IsFlying);
		}
		enemy.ResetPositionForSpawnController(m_spawnPoint, m_spawnCollider);
	}

	public void SetColor(Color color)
	{
		if (!Application.isPlaying)
		{
			GetComponent<SpriteRenderer>().color = color;
			ID = ColorTable[color];
		}
	}

	public void SetForceCommander(bool forceCommander)
	{
		ForceCommander = forceCommander;
	}

	public void SetForceFlying(bool forceFlying)
	{
		ForceFlying = forceFlying;
	}

	public void SetRoom(BaseRoom value)
	{
		m_room = value;
	}

	public void SetEnemy(EnemyType enemyType, EnemyRank enemyRank, int enemyLevel)
	{
		if (!Override)
		{
			Type = enemyType;
			Rank = enemyRank;
		}
		if (!OverrideLevel)
		{
			Level = enemyLevel;
		}
		else if (Level <= 0)
		{
			Level = 1;
		}
	}

	public void SetEnemyIndex(int index)
	{
		EnemyIndex = index;
	}

	public void ForceEnemyDead(bool disableEnemy = true)
	{
		if ((bool)EnemyInstance)
		{
			if (EnemyInstance.IsBoss)
			{
				return;
			}
			if (disableEnemy)
			{
				EnemyInstance.gameObject.SetActive(value: false);
			}
		}
		IsDead = true;
	}

	public void ResetIsDead()
	{
		IsDead = false;
	}

	private void OnEnemyDeath(object sender, EnemyDeathEventArgs eventArgs)
	{
		IsDead = true;
		EnemyInstance.OnEnemyDeathRelay.RemoveListener(m_onEnemyDeath);
		EnemyInstance.OnReactivationTimedOutRelay.RemoveListener(m_onEnemyTimedOut);
	}

	private void OnEnemyTimedOut(object sender, EnemyActivationStateChangedEventArgs eventArgs)
	{
		if (eventArgs.Enemy != null)
		{
			ResetEnemyPosition(eventArgs.Enemy);
		}
	}

	public void RemoveListeners()
	{
		if ((bool)EnemyInstance)
		{
			EnemyInstance.OnEnemyDeathRelay.RemoveListener(m_onEnemyDeath);
			EnemyInstance.OnReactivationTimedOutRelay.RemoveListener(m_onEnemyTimedOut);
		}
	}

	public void InitializeEnemyInstance()
	{
		if (!ShouldSpawn)
		{
			return;
		}
		EnemyInstance = EnemyManager.GetEnemyFromPool(Type, Rank);
		if (!EnemyInstance.IsNativeNull())
		{
			EnemyInstance.SetLevel(Level);
			EnemyInstance.SetEnemyIndex(EnemyIndex);
			EnemyInstance.Summoner = null;
			EnemyInstance.SetRoom(Room);
			EnemyInstance.gameObject.SetActive(value: true);
			EnemyInstance.EnemySpawnController = this;
			if (EnemyInstance.IsInitialized)
			{
				EnemyInstance.ResetCharacter();
			}
			if (Room.SpawnedAsEasyRoom)
			{
				EnemyInstance.IsCommander = false;
			}
			else
			{
				EnemyInstance.IsCommander = ForceCommander;
			}
			EnemyInstance.InitializeCommanderStatusEffects();
			ResetEnemyPosition(EnemyInstance);
			EnemyInstance.OnEnemyDeathRelay.AddListener(m_onEnemyDeath);
			EnemyInstance.OnReactivationTimedOutRelay.AddListener(m_onEnemyTimedOut);
		}
		else
		{
			Debug.LogFormat("<color=red>| {0} | Failed to get a <b>{1} {2}</b> from Enemy Pool</color>", this, Rank, Type);
		}
	}

	private void OnDisable()
	{
		if ((bool)EnemyInstance)
		{
			EnemyInstance.OnEnemyDeathRelay.RemoveListener(m_onEnemyDeath);
			EnemyInstance.OnReactivationTimedOutRelay.RemoveListener(m_onEnemyTimedOut);
			if (!GameManager.IsApplicationClosing && EnemyInstance.gameObject.activeSelf)
			{
				EnemyInstance.gameObject.SetActive(value: false);
			}
			EnemyInstance = null;
		}
	}

	[SpecialName]
	GameObject ISpawnController.get_gameObject()
	{
		return base.gameObject;
	}
}
