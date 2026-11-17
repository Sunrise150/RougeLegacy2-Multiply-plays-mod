using System.Collections;
using Sigtrap.Relays;
using UnityEngine;

public class TorchesRoomPropController : BaseSpecialPropController, IBodyOnEnterHitResponse, IHitResponse, IBodyOnStayHitResponse
{
	[SerializeField]
	private GameObject m_shaPingGO;

	private const float TORCH_FLAME_ON_DURATION = 1.15f;

	private Relay<TorchesRoomPropController> m_onHitRelay = new Relay<TorchesRoomPropController>();

	private Relay<bool> m_flameStateChangeRelay = new Relay<bool>();

	public IRelayLink<TorchesRoomPropController> OnHitRelay => m_onHitRelay.link;

	public IRelayLink<bool> FlameStateChangeRelay => m_flameStateChangeRelay.link;

	public bool IsFlameOn { get; private set; }

	public bool KeepFlameOn { get; set; }

	protected override void InitializePooledPropOnEnter()
	{
		StopAllCoroutines();
		IsFlameOn = false;
		KeepFlameOn = false;
		base.Animator.SetBool("FlameOn", value: false);
		if (SaveManager.PlayerSaveData.GetInsightState(InsightType.CastleBoss_DoorOpened) >= InsightState.ResolvedButNotViewed)
		{
			m_shaPingGO.SetActive(value: false);
			ForceFlameOn();
		}
	}

	public void ForceFlameOn()
	{
		base.Animator.SetBool("FlameOn", value: true);
		IsFlameOn = true;
		KeepFlameOn = true;
		base.Animator.Play("FlameOn");
		base.Animator.Update(10f);
		base.Animator.Update(10f);
		m_flameStateChangeRelay.Dispatch(t: true);
	}

	public void BodyOnEnterHitResponse(IHitboxController otherHBController)
	{
		if (!KeepFlameOn && otherHBController.CollisionType == CollisionType.PlayerProjectile && (bool)otherHBController.RootGameObject.GetComponent<DownstrikeProjectile_RL>())
		{
			StopAllCoroutines();
			base.Animator.SetBool("FlameOn", value: true);
			IsFlameOn = true;
			StartCoroutine(StartFlameOnCoroutine());
			m_onHitRelay.Dispatch(this);
			m_flameStateChangeRelay.Dispatch(t: true);
		}
	}

	public void BodyOnStayHitResponse(IHitboxController otherHBController)
	{
		BodyOnEnterHitResponse(otherHBController);
	}

	private void OnGroundTouch()
	{
		if (!KeepFlameOn)
		{
			StopAllCoroutines();
			base.Animator.SetBool("FlameOn", value: false);
			IsFlameOn = false;
			m_flameStateChangeRelay.Dispatch(t: false);
		}
	}

	private IEnumerator StartFlameOnCoroutine()
	{
		float startTime = Time.time;
		while (Time.time < startTime + 1.15f)
		{
			if (PlayerManager.GetPlayerController().IsGrounded)
			{
				OnGroundTouch();
				yield break;
			}
			yield return null;
		}
		if (!KeepFlameOn)
		{
			base.Animator.SetBool("FlameOn", value: false);
			IsFlameOn = false;
			m_flameStateChangeRelay.Dispatch(t: false);
		}
	}
}
