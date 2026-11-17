using System;
using System.Linq;

public class ApplySporeBurstOnPlatformProjectileLogic : BaseProjectileLogic
{
	private static string[] m_platformExclusionArray = new string[1] { "CrowsNestPlatformTalentProjectile" };

	private Action<CorgiController_RL> m_onExitLanded;

	private Action<CorgiController_RL> m_onEnterLanded;

	protected override void Awake()
	{
		base.Awake();
		m_onExitLanded = OnExitLanded;
		m_onEnterLanded = OnEnterLanded;
	}

	private void OnEnable()
	{
		if (base.SourceProjectile.IsPersistentProjectile)
		{
			PlayerManager.GetPlayerController().ControllerCorgi.OnCorgiLandedEnterRelay.AddListener(m_onEnterLanded);
		}
		PerformSporeBurstCheck();
	}

	private void OnDisable()
	{
		if (!GameManager.IsApplicationClosing && base.SourceProjectile.IsPersistentProjectile)
		{
			PlayerManager.GetPlayerController().ControllerCorgi.OnCorgiLandedEnterRelay.RemoveListener(m_onEnterLanded);
		}
	}

	private void PerformSporeBurstCheck()
	{
		PlayerController playerController = PlayerManager.GetPlayerController();
		if (playerController.IsGrounded && (bool)playerController.ControllerCorgi.StandingOn && playerController.ControllerCorgi.StandingOn.CompareTag("PlayerProjectile") && !m_platformExclusionArray.Contains(playerController.ControllerCorgi.StandingOn.GetRoot().name))
		{
			if (base.SourceProjectile.IsPersistentProjectile)
			{
				playerController.ControllerCorgi.OnCorgiLandedExitRelay.AddListener(m_onExitLanded);
			}
			base.SourceProjectile.AttachStatusEffect(StatusEffectType.Enemy_SporeBurst);
		}
	}

	private void OnEnterLanded(CorgiController_RL corgi)
	{
		PerformSporeBurstCheck();
	}

	private void OnExitLanded(CorgiController_RL corgi)
	{
		PlayerManager.GetPlayerController().ControllerCorgi.OnCorgiLandedExitRelay.RemoveListener(m_onExitLanded);
		base.SourceProjectile.RemoveStatusEffect(StatusEffectType.Enemy_SporeBurst);
	}
}
