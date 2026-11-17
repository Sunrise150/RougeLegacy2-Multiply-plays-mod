using System;
using System.Collections;
using UnityEngine;

public class AngryOnHit_Trait : BaseTrait
{
	private bool m_isAngry;

	private Coroutine m_angryCoroutine;

	private WaitRL_Yield m_waitYield;

	private bool m_gameStarted;

	private AngryOnHit_Effect m_effect;

	private Action<MonoBehaviour, EventArgs> m_onPlayerHealthChange;

	private Action<MonoBehaviour, EventArgs> m_onPlayerEnterRoom;

	public float StrengthMultiplier
	{
		get
		{
			if (m_isAngry)
			{
				return 0.25f;
			}
			return 0f;
		}
	}

	public override TraitType TraitType => TraitType.AngryOnHit;

	protected override void Awake()
	{
		base.Awake();
		m_onPlayerHealthChange = OnPlayerHealthChange;
		m_onPlayerEnterRoom = OnPlayerEnterRoom;
	}

	private IEnumerator Start()
	{
		while (!PlayerManager.IsInstantiated)
		{
			yield return null;
		}
		Messenger<GameMessenger, GameEvent>.AddListener(GameEvent.PlayerHealthChange, m_onPlayerHealthChange);
		Messenger<GameMessenger, GameEvent>.AddListener(GameEvent.PlayerEnterRoom, m_onPlayerEnterRoom);
		m_waitYield = new WaitRL_Yield(0f);
		m_effect = CameraController.ForegroundPerspCam.GetComponent<AngryOnHit_Effect>();
		m_isAngry = false;
		m_gameStarted = false;
	}

	private void OnPlayerHealthChange(MonoBehaviour sender, EventArgs args)
	{
		if (!m_gameStarted)
		{
			return;
		}
		HealthChangeEventArgs healthChangeEventArgs = args as HealthChangeEventArgs;
		if (healthChangeEventArgs.PrevHealthValue > healthChangeEventArgs.NewHealthValue)
		{
			if (m_angryCoroutine != null)
			{
				StopCoroutine(m_angryCoroutine);
			}
			m_angryCoroutine = StartCoroutine(AngryCoroutine());
		}
	}

	private void OnPlayerEnterRoom(MonoBehaviour sender, EventArgs args)
	{
		m_gameStarted = true;
	}

	private IEnumerator AngryCoroutine()
	{
		m_waitYield.CreateNew(7.5f);
		StartAnger();
		yield return m_waitYield;
		StopAnger();
	}

	private void StartAnger()
	{
		if (!m_isAngry)
		{
			m_isAngry = true;
			PlayerController playerController = PlayerManager.GetPlayerController();
			playerController.MovementSpeedMod += 0.5f;
			playerController.InitializeStrengthMods();
			m_effect.enabled = true;
			m_effect.Amount = 0.75f;
		}
	}

	private void StopAnger()
	{
		if (m_isAngry)
		{
			m_isAngry = false;
			PlayerController playerController = PlayerManager.GetPlayerController();
			playerController.MovementSpeedMod -= 0.5f;
			playerController.InitializeStrengthMods();
			TweenManager.TweenTo(m_effect, 0.5f, Ease.None, "Amount", 0);
		}
	}

	private void OnDestroy()
	{
		Messenger<GameMessenger, GameEvent>.RemoveListener(GameEvent.PlayerHealthChange, m_onPlayerHealthChange);
		Messenger<GameMessenger, GameEvent>.RemoveListener(GameEvent.PlayerEnterRoom, m_onPlayerEnterRoom);
		if (!PlayerManager.IsDisposed)
		{
			StopAnger();
		}
	}
}
