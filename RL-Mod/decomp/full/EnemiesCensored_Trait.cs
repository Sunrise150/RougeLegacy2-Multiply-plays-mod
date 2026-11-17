using System;
using System.Collections;
using System.Linq;
using UnityEngine;

public class EnemiesCensored_Trait : BaseTrait
{
	public const string TRAIT_EFFECT_NAME = "EnemiesCensored_Trait_Effect";

	private Action<MonoBehaviour, EventArgs> m_onPlayerEnterRoom;

	private Action<MonoBehaviour, EventArgs> m_onEnemySummoned;

	public override TraitType TraitType => TraitType.EnemiesCensored;

	protected override void Awake()
	{
		base.Awake();
		m_onPlayerEnterRoom = OnPlayerEnterRoom;
		m_onEnemySummoned = OnEnemySummoned;
	}

	private void Start()
	{
		Messenger<GameMessenger, GameEvent>.AddListener(GameEvent.PlayerEnterRoom, m_onPlayerEnterRoom);
		Messenger<GameMessenger, GameEvent>.AddListener(GameEvent.EnemySummoned, m_onEnemySummoned);
	}

	private void OnDestroy()
	{
		Messenger<GameMessenger, GameEvent>.RemoveListener(GameEvent.PlayerEnterRoom, m_onPlayerEnterRoom);
		Messenger<GameMessenger, GameEvent>.RemoveListener(GameEvent.EnemySummoned, m_onEnemySummoned);
	}

	public override void AssignGreenMask()
	{
		base.AssignGreenMask();
		m_postProcessOverrideController.Profile.OverridePixelGreenChannel = true;
		m_postProcessOverrideController.Profile.PixelGreenChannel = m_postProcessOverrideController.Profile.PixelRedChannel;
		m_postProcessOverrideController.Profile.PixelRedChannel = 0f;
		m_postProcessOverrideController.Profile.OverridePixelRedChannel = false;
	}

	private void OnEnemySummoned(MonoBehaviour sender, EventArgs args)
	{
		if (args is EnemySummonedEventArgs { SummonedEnemy: var summonedEnemy } && (bool)summonedEnemy)
		{
			StartCoroutine(EnableTraitCoroutine(summonedEnemy));
		}
	}

	private void OnPlayerEnterRoom(MonoBehaviour sender, EventArgs args)
	{
		StopAllCoroutines();
		EnemySpawnController[] enemySpawnControllers = PlayerManager.GetCurrentPlayerRoom().SpawnControllerManager.EnemySpawnControllers;
		foreach (EnemySpawnController enemySpawnController in enemySpawnControllers)
		{
			if (enemySpawnController.ShouldSpawn)
			{
				EnemyController enemyInstance = enemySpawnController.EnemyInstance;
				if ((bool)enemyInstance)
				{
					StartCoroutine(EnableTraitCoroutine(enemyInstance));
				}
			}
		}
	}

	private IEnumerator EnableTraitCoroutine(EnemyController enemyInstance)
	{
		if (Trait_EV.TRAIT_EFFECT_EXCEPTION_ARRAY.Contains(enemyInstance.EnemyType))
		{
			yield break;
		}
		while (!enemyInstance.IsInitialized)
		{
			yield return null;
		}
		if (enemyInstance.EnemyType == EnemyType.Zombie)
		{
			yield break;
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
		ApplyCensoredEffect(enemyInstance);
	}

	public static BaseEffect ApplyCensoredEffect(BaseCharacterController charController)
	{
		BaseEffect baseEffect = EffectManager.PlayEffect(charController.gameObject, charController.Animator, "EnemiesCensored_Trait_Effect", charController.Midpoint, 0f, EffectStopType.Gracefully);
		BaseTrait activeTrait = TraitManager.GetActiveTrait(TraitType.EnemiesCensored);
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
		Bounds visualBounds = charController.VisualBounds;
		float num = 1f * charController.transform.lossyScale.x;
		float num2 = visualBounds.size.x + num;
		float num3 = visualBounds.size.y + num;
		bool num4 = num3 > num2;
		Bounds bounds = new Bounds(baseEffect.transform.localPosition, new Vector3(0.52f, 0.52f, 0.52f));
		float num5 = (num4 ? (num3 / bounds.size.y) : (num2 / bounds.size.x));
		Vector2 vector = charController.VisualBoundsObj.Collider.offset * charController.gameObject.transform.localScale.x;
		baseEffect.transform.position = charController.gameObject.transform.localPosition + new Vector3(vector.x, vector.y, 0f);
		baseEffect.transform.localScale = new Vector3(num5, num5, baseEffect.transform.localScale.z);
		baseEffect.transform.SetParent(charController.gameObject.transform, worldPositionStays: true);
		return baseEffect;
	}
}
