using System;
using RLAudio;
using UnityEngine;

public class Fart_Trait : BaseTrait, IAudioEventEmitter
{
	private string m_description = string.Empty;

	private Action<MonoBehaviour, EventArgs> m_onAbilityCast;

	public override TraitType TraitType => TraitType.Fart;

	public string Description
	{
		get
		{
			if (string.IsNullOrEmpty(m_description))
			{
				m_description = ToString();
			}
			return m_description;
		}
	}

	protected override void Awake()
	{
		base.Awake();
		m_onAbilityCast = OnAbilityCast;
	}

	private void Start()
	{
		Messenger<GameMessenger, GameEvent>.AddListener(GameEvent.PlayerDash, m_onAbilityCast);
		Messenger<GameMessenger, GameEvent>.AddListener(GameEvent.PlayerJump, m_onAbilityCast);
	}

	private void OnDestroy()
	{
		Messenger<GameMessenger, GameEvent>.RemoveListener(GameEvent.PlayerDash, m_onAbilityCast);
		Messenger<GameMessenger, GameEvent>.RemoveListener(GameEvent.PlayerJump, m_onAbilityCast);
	}

	private void OnAbilityCast(MonoBehaviour sender, EventArgs args)
	{
		float num = 0.16f;
		if (UnityEngine.Random.Range(0f, 1f) < num)
		{
			ProjectileManager.Instance.AddProjectileToPool("FartProjectile");
			PlayerController playerController = PlayerManager.GetPlayerController();
			ProjectileManager.FireProjectile(offset: Trait_EV.FART_POSITION, source: playerController.gameObject, projectileName: "FartProjectile");
		}
	}
}
