using System;
using System.Collections;
using UnityEngine;

public class ColorTrails_Trait : BaseTrait
{
	private BaseEffect m_playerTrailEffect;

	private Action<MonoBehaviour, EventArgs> m_onPlayerEnterRoom;

	private Action<MonoBehaviour, EventArgs> m_onEnemySummoned;

	public const string TRAIT_EFFECT_NAME = "ColorTrails_Trait_Effect";

	public override TraitType TraitType => TraitType.ColorTrails;

	protected override void Awake()
	{
		base.Awake();
		m_onEnemySummoned = OnEnemySummoned;
		m_onPlayerEnterRoom = OnPlayerEnterRoom;
	}

	private IEnumerator Start()
	{
		Messenger<GameMessenger, GameEvent>.AddListener(GameEvent.PlayerEnterRoom, m_onPlayerEnterRoom);
		Messenger<GameMessenger, GameEvent>.AddListener(GameEvent.EnemySummoned, m_onEnemySummoned);
		while (!PlayerManager.IsInstantiated)
		{
			yield return null;
		}
		PlayerController playerController = PlayerManager.GetPlayerController();
		while (!playerController.IsInitialized)
		{
			yield return null;
		}
		CreatePlayerColorTrails();
	}

	private void CreatePlayerColorTrails()
	{
		PlayerController playerController = PlayerManager.GetPlayerController();
		Debug.Log("Creating player color trail");
		BaseEffect baseEffect = EffectManager.PlayEffect(playerController.gameObject, playerController.Animator, "ColorTrails_Trait_Effect", playerController.Midpoint, 0f, EffectStopType.Gracefully);
		Bounds visualBounds = playerController.VisualBounds;
		_ = visualBounds.size;
		_ = visualBounds.size;
		TrailRenderer componentInChildren = baseEffect.GetComponentInChildren<TrailRenderer>();
		_ = componentInChildren.bounds;
		componentInChildren.startColor = UnityEngine.Random.ColorHSV(0f, 1f, 1f, 1f, 0.5f, 1f);
		componentInChildren.endColor = UnityEngine.Random.ColorHSV(0f, 1f, 1f, 1f, 0.5f, 1f);
		componentInChildren.widthMultiplier = 1.5f;
		componentInChildren.time = 18f;
		baseEffect.transform.SetParent(playerController.gameObject.transform, worldPositionStays: true);
		baseEffect.DisableDestroyOnRoomChange = true;
		baseEffect.transform.GetChild(0).gameObject.layer = 29;
		m_playerTrailEffect = baseEffect;
	}

	private void OnDestroy()
	{
		Messenger<GameMessenger, GameEvent>.RemoveListener(GameEvent.PlayerEnterRoom, m_onPlayerEnterRoom);
		Messenger<GameMessenger, GameEvent>.RemoveListener(GameEvent.EnemySummoned, m_onEnemySummoned);
		EffectManager.DisableAllEffectWithName("ColorTrails_Trait_Effect");
	}

	private void OnPlayerEnterRoom(MonoBehaviour sender, EventArgs args)
	{
		if (!m_playerTrailEffect || ((bool)m_playerTrailEffect && (!m_playerTrailEffect.gameObject.activeSelf || !m_playerTrailEffect.Source.CompareTag("Player"))))
		{
			CreatePlayerColorTrails();
		}
		if ((bool)m_playerTrailEffect)
		{
			m_playerTrailEffect.GetComponentInChildren<TrailRenderer>().Clear();
		}
		StopAllCoroutines();
		BaseRoom currentPlayerRoom = PlayerManager.GetCurrentPlayerRoom();
		if (currentPlayerRoom.BiomeType == BiomeType.HubTown)
		{
			EnableTraitOnPizzaGirl();
		}
		EnemySpawnController[] enemySpawnControllers = currentPlayerRoom.SpawnControllerManager.EnemySpawnControllers;
		foreach (EnemySpawnController enemySpawnController in enemySpawnControllers)
		{
			if (enemySpawnController.ShouldSpawn)
			{
				EnemyController enemyInstance = enemySpawnController.EnemyInstance;
				if (enemyInstance != null)
				{
					StartCoroutine(EnableTraitCoroutine(enemyInstance));
				}
			}
		}
	}

	private void EnableTraitOnPizzaGirl()
	{
		PropSpawnController propSpawnController = PlayerManager.GetCurrentPlayerRoom().gameObject.FindObjectReference<PropSpawnController>("PizzaGirl");
		Prop prop = null;
		if ((bool)propSpawnController)
		{
			prop = propSpawnController.PropInstance;
		}
		if ((bool)prop && NPCType_RL.IsNPCUnlocked(NPCType.PizzaGirl))
		{
			Vector3 position = prop.transform.position;
			position.y += prop.HitboxController.GetCollider(HitboxType.Platform).bounds.extents.y;
			BaseEffect baseEffect = EffectManager.PlayEffect(prop.gameObject, prop.Animators[0], "ColorTrails_Trait_Effect", position, 0f, EffectStopType.Gracefully);
			TrailRenderer componentInChildren = baseEffect.GetComponentInChildren<TrailRenderer>();
			componentInChildren.startColor = UnityEngine.Random.ColorHSV(0f, 1f, 1f, 1f, 0.5f, 1f);
			componentInChildren.endColor = UnityEngine.Random.ColorHSV(0f, 1f, 1f, 1f, 0.5f, 1f);
			componentInChildren.widthMultiplier = 1.5f;
			componentInChildren.time = 18f;
			baseEffect.transform.GetChild(0).gameObject.layer = 24;
			baseEffect.transform.SetParent(prop.gameObject.transform, worldPositionStays: true);
		}
	}

	private void OnEnemySummoned(MonoBehaviour sender, EventArgs args)
	{
		if (args is EnemySummonedEventArgs { SummonedEnemy: var summonedEnemy } && (bool)summonedEnemy)
		{
			StartCoroutine(EnableTraitCoroutine(summonedEnemy));
		}
	}

	private IEnumerator EnableTraitCoroutine(EnemyController enemyInstance)
	{
		while (!enemyInstance.IsInitialized)
		{
			yield return null;
		}
		if (enemyInstance.EnemyType != EnemyType.Zombie)
		{
			BaseEffect baseEffect = EffectManager.PlayEffect(enemyInstance.gameObject, enemyInstance.Animator, "ColorTrails_Trait_Effect", enemyInstance.Midpoint, 0f, EffectStopType.Gracefully);
			TrailRenderer componentInChildren = baseEffect.GetComponentInChildren<TrailRenderer>();
			componentInChildren.startColor = UnityEngine.Random.ColorHSV(0f, 1f, 1f, 1f, 0.5f, 1f);
			componentInChildren.endColor = UnityEngine.Random.ColorHSV(0f, 1f, 1f, 1f, 0.5f, 1f);
			componentInChildren.widthMultiplier = 1.5f;
			componentInChildren.time = 18f;
			baseEffect.transform.GetChild(0).gameObject.layer = 29;
			baseEffect.transform.SetParent(enemyInstance.gameObject.transform, worldPositionStays: true);
		}
	}
}
