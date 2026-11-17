using System;
using UnityEngine;

public class AstroWandProjectileLogic : BaseProjectileLogic
{
	private Action<MonoBehaviour, EventArgs> m_onPlayerCastingAstroWand;

	protected override void Awake()
	{
		base.Awake();
		m_onPlayerCastingAstroWand = OnPlayerCastingAstroWand;
	}

	private void OnEnable()
	{
		base.SourceProjectile.DontFireDeathRelay = false;
		Messenger<GameMessenger, GameEvent>.AddListener(GameEvent.PlayerCastingAstroWand, m_onPlayerCastingAstroWand);
	}

	private void OnDisable()
	{
		Messenger<GameMessenger, GameEvent>.RemoveListener(GameEvent.PlayerCastingAstroWand, m_onPlayerCastingAstroWand);
	}

	private void OnPlayerCastingAstroWand(MonoBehaviour sender, EventArgs args)
	{
		base.SourceProjectile.DontFireDeathRelay = true;
		base.SourceProjectile.FlagForDestruction();
	}
}
