using System;
using System.Collections;
using Sigtrap.Relays;
using UnityEngine;

public class CheerOnKills_Trait : BaseTrait
{
	public Relay<EnemyTypeAndRank> CheerAfterKillRelay = new Relay<EnemyTypeAndRank>();

	public Relay RosesThrownRelay = new Relay();

	public Relay PlayerDeathRelay = new Relay();

	private int m_numEnemiesToKill;

	private int m_numEnemiesKilled;

	private SummonRuleController m_roomSummonRuleController;

	private Action<MonoBehaviour, EventArgs> m_onPlayerEnterRoom;

	private Action<MonoBehaviour, EventArgs> m_onEnemySummoned;

	private Action<MonoBehaviour, EventArgs> m_onEnemyDeath;

	public override TraitType TraitType => TraitType.CheerOnKills;

	protected override void Awake()
	{
		base.Awake();
		m_onPlayerEnterRoom = OnPlayerEnterRoom;
		m_onEnemySummoned = OnEnemySummoned;
		m_onEnemyDeath = OnEnemyDeath;
	}

	private IEnumerator Start()
	{
		while (!PlayerManager.IsInstantiated)
		{
			yield return null;
		}
		PlayerController playerController = PlayerManager.GetPlayerController();
		m_traitMask.transform.position = playerController.Midpoint;
		m_traitMask.transform.SetParent(playerController.transform, worldPositionStays: true);
		Vector3 localScale = new Vector3(5.5f, 5.5f, 1f);
		m_traitMask.transform.localScale = localScale;
		Messenger<GameMessenger, GameEvent>.AddListener(GameEvent.EnemyDeath, m_onEnemyDeath);
		Messenger<GameMessenger, GameEvent>.AddListener(GameEvent.PlayerEnterRoom, m_onPlayerEnterRoom);
		Messenger<GameMessenger, GameEvent>.AddListener(GameEvent.EnemySummoned, m_onEnemySummoned);
	}

	public override void AssignGreenMask()
	{
		base.AssignGreenMask();
		m_postProcessOverrideController.Profile.OverrideTintGreenChannel = true;
		m_postProcessOverrideController.Profile.TintGreenChannel = m_postProcessOverrideController.Profile.TintRedChannel;
		m_postProcessOverrideController.Profile.TintRedChannel = 0f;
		m_postProcessOverrideController.Profile.OverrideTintRedChannel = false;
	}

	private void OnEnemyDeath(MonoBehaviour sender, EventArgs args)
	{
		EnemyDeathEventArgs eventArgs = args as EnemyDeathEventArgs;
		m_numEnemiesKilled++;
		RunCheerCheck(eventArgs);
	}

	private void RunCheerCheck(EnemyDeathEventArgs eventArgs)
	{
		bool flag = m_numEnemiesKilled >= m_numEnemiesToKill && m_numEnemiesToKill > 0;
		if ((bool)m_roomSummonRuleController && m_roomSummonRuleController.StillSummoning)
		{
			flag = false;
		}
		if (flag)
		{
			EffectManager.PlayEffect(base.gameObject, null, "CheerOnKills_Roses_Effect", CameraController.ForegroundOrthoCam.transform.position, 0f, EffectStopType.Gracefully);
			RosesThrownRelay.Dispatch();
			CheerAfterKillRelay.Dispatch(new EnemyTypeAndRank(eventArgs.Victim.EnemyType, eventArgs.Victim.EnemyRank));
		}
	}

	private void OnPlayerEnterRoom(MonoBehaviour sender, EventArgs args)
	{
		m_numEnemiesKilled = 0;
		m_numEnemiesToKill = 0;
		StopAllCoroutines();
		BaseRoom currentPlayerRoom = PlayerManager.GetCurrentPlayerRoom();
		EnemySpawnController[] enemySpawnControllers = currentPlayerRoom.SpawnControllerManager.EnemySpawnControllers;
		foreach (EnemySpawnController enemySpawnController in enemySpawnControllers)
		{
			if (enemySpawnController.ShouldSpawn)
			{
				EnemyController enemyInstance = enemySpawnController.EnemyInstance;
				if (enemyInstance != null)
				{
					StartCoroutine(EnableOnEnemyCoroutine(enemyInstance, increment: true));
				}
			}
		}
		m_roomSummonRuleController = currentPlayerRoom.GetComponentInChildren<SummonRuleController>();
	}

	private void OnEnemySummoned(MonoBehaviour sender, EventArgs args)
	{
		if (args is EnemySummonedEventArgs { SummonedEnemy: var summonedEnemy } && (bool)summonedEnemy)
		{
			StartCoroutine(EnableOnEnemyCoroutine(summonedEnemy, increment: true));
		}
	}

	public void ApplyOnEnemy(EnemyController enemy, bool increment)
	{
		StartCoroutine(EnableOnEnemyCoroutine(enemy, increment));
	}

	private IEnumerator EnableOnEnemyCoroutine(EnemyController enemyInstance, bool increment)
	{
		while (!enemyInstance.IsInitialized)
		{
			yield return null;
		}
		if (enemyInstance.IsSummoned || enemyInstance.ActivatedByFairyRoomTrigger)
		{
			while (!enemyInstance.IsBeingSummoned)
			{
				if (!enemyInstance.gameObject.activeSelf)
				{
					yield break;
				}
				yield return null;
			}
		}
		if (increment)
		{
			EnemyType enemyType = enemyInstance.EnemyType;
			if (enemyType != EnemyType.BouncySpike && enemyType != EnemyType.Dummy && enemyType != EnemyType.Eggplant)
			{
				m_numEnemiesToKill++;
			}
		}
		BaseEffect baseEffect = EffectManager.PlayEffect(enemyInstance.gameObject, enemyInstance.Animator, "CheerOnKills_Mask_Effect", enemyInstance.Midpoint, 0f, EffectStopType.Gracefully);
		BaseTrait activeTrait = TraitManager.GetActiveTrait(TraitType);
		if ((bool)activeTrait)
		{
			ChangeEffectRendererColor component = baseEffect.GetComponent<ChangeEffectRendererColor>();
			if (activeTrait.IsFirstTrait)
			{
				component.ChangeColor(Color.red);
			}
			else
			{
				component.ChangeColor(Color.green);
			}
		}
		Bounds visualBounds = enemyInstance.VisualBounds;
		float num = 1f * enemyInstance.transform.lossyScale.x;
		float num2 = visualBounds.size.x + num;
		float num3 = visualBounds.size.y + num;
		bool num4 = num3 > num2;
		Bounds bounds = new Bounds(baseEffect.transform.localPosition, new Vector3(5.68f, 5.68f, 5.68f));
		float num5 = (num4 ? (num3 / bounds.size.y) : (num2 / bounds.size.x));
		baseEffect.transform.localScale = new Vector3(num5, num5, baseEffect.transform.localScale.z);
		baseEffect.transform.SetParent(enemyInstance.gameObject.transform, worldPositionStays: true);
	}

	public override void DisableOnDeath()
	{
		PlayerDeathRelay.Dispatch();
		EffectManager.DisableAllEffectWithName("CheerOnKills_Mask_Effect");
		m_traitMask.gameObject.SetActive(value: false);
		base.DisableOnDeath();
	}

	private void OnDestroy()
	{
		if ((bool)m_traitMask)
		{
			UnityEngine.Object.Destroy(m_traitMask.gameObject);
		}
		Messenger<GameMessenger, GameEvent>.RemoveListener(GameEvent.EnemyDeath, m_onEnemyDeath);
		Messenger<GameMessenger, GameEvent>.RemoveListener(GameEvent.PlayerEnterRoom, m_onPlayerEnterRoom);
		Messenger<GameMessenger, GameEvent>.RemoveListener(GameEvent.EnemySummoned, m_onEnemySummoned);
	}
}
