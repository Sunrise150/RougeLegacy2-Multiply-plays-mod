using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ClownHUDController : MonoBehaviour
{
	[SerializeField]
	private TMP_Text m_bronzeText;

	[SerializeField]
	private Image m_bronzeBalloon;

	[SerializeField]
	private TMP_Text m_silverText;

	[SerializeField]
	private Image m_silverBalloon;

	[SerializeField]
	private TMP_Text m_goldText;

	[SerializeField]
	private Image m_goldBalloon;

	[SerializeField]
	private TMP_Text m_ammoText;

	[SerializeField]
	private TMP_Text m_hitsText;

	private Action<MonoBehaviour, EventArgs> m_onEnterClownRoom;

	private Action<MonoBehaviour, EventArgs> m_onExitClownRoom;

	public ClownRoomController ClownRoomController { get; private set; }

	private void Awake()
	{
		m_onEnterClownRoom = OnEnterClownRoom;
		m_onExitClownRoom = OnExitClownRoom;
		Messenger<GameMessenger, GameEvent>.AddListener(GameEvent.PlayerEnterClownRoom, m_onEnterClownRoom);
		Messenger<GameMessenger, GameEvent>.AddListener(GameEvent.PlayerExitClownRoom, m_onExitClownRoom);
		base.gameObject.SetActive(value: false);
	}

	private void OnDestroy()
	{
		Messenger<GameMessenger, GameEvent>.RemoveListener(GameEvent.PlayerEnterClownRoom, m_onEnterClownRoom);
		Messenger<GameMessenger, GameEvent>.RemoveListener(GameEvent.PlayerExitClownRoom, m_onExitClownRoom);
	}

	private void OnEnterClownRoom(object sender, EventArgs args)
	{
		ClownRoomEnteredEventArgs clownRoomEnteredEventArgs = args as ClownRoomEnteredEventArgs;
		ClownRoomController = clownRoomEnteredEventArgs.ClownRoomController;
		ClownRoomController clownRoomController = ClownRoomController;
		clownRoomController.CurrentAmmoChangedEvent = (ClownRoomEventHandler)Delegate.Combine(clownRoomController.CurrentAmmoChangedEvent, new ClownRoomEventHandler(OnAmmoChanged));
		ClownRoomController clownRoomController2 = ClownRoomController;
		clownRoomController2.BronzeGoalReachedEvent = (ClownRoomEventHandler)Delegate.Combine(clownRoomController2.BronzeGoalReachedEvent, new ClownRoomEventHandler(OnBronzeGoalReached));
		ClownRoomController clownRoomController3 = ClownRoomController;
		clownRoomController3.SilverGoalReachedEvent = (ClownRoomEventHandler)Delegate.Combine(clownRoomController3.SilverGoalReachedEvent, new ClownRoomEventHandler(OnSilverGoalReached));
		ClownRoomController clownRoomController4 = ClownRoomController;
		clownRoomController4.GoldGoalReachedEvent = (ClownRoomEventHandler)Delegate.Combine(clownRoomController4.GoldGoalReachedEvent, new ClownRoomEventHandler(OnGoldGoalReached));
		ClownRoomController clownRoomController5 = ClownRoomController;
		clownRoomController5.CurrentGoalAmountChangedEvent = (ClownRoomEventHandler)Delegate.Combine(clownRoomController5.CurrentGoalAmountChangedEvent, new ClownRoomEventHandler(OnCurrentGoalAmountChanged));
		ResetHUD();
		base.gameObject.SetActive(value: true);
	}

	private void OnExitClownRoom(object sender, EventArgs args)
	{
		if (ClownRoomController != null)
		{
			ClownRoomController clownRoomController = ClownRoomController;
			clownRoomController.CurrentAmmoChangedEvent = (ClownRoomEventHandler)Delegate.Remove(clownRoomController.CurrentAmmoChangedEvent, new ClownRoomEventHandler(OnAmmoChanged));
			ClownRoomController clownRoomController2 = ClownRoomController;
			clownRoomController2.BronzeGoalReachedEvent = (ClownRoomEventHandler)Delegate.Remove(clownRoomController2.BronzeGoalReachedEvent, new ClownRoomEventHandler(OnBronzeGoalReached));
			ClownRoomController clownRoomController3 = ClownRoomController;
			clownRoomController3.SilverGoalReachedEvent = (ClownRoomEventHandler)Delegate.Remove(clownRoomController3.SilverGoalReachedEvent, new ClownRoomEventHandler(OnSilverGoalReached));
			ClownRoomController clownRoomController4 = ClownRoomController;
			clownRoomController4.GoldGoalReachedEvent = (ClownRoomEventHandler)Delegate.Remove(clownRoomController4.GoldGoalReachedEvent, new ClownRoomEventHandler(OnGoldGoalReached));
			ClownRoomController clownRoomController5 = ClownRoomController;
			clownRoomController5.CurrentGoalAmountChangedEvent = (ClownRoomEventHandler)Delegate.Remove(clownRoomController5.CurrentGoalAmountChangedEvent, new ClownRoomEventHandler(OnCurrentGoalAmountChanged));
		}
		base.gameObject.SetActive(value: false);
	}

	private void OnAmmoChanged()
	{
		UpdateHUD();
	}

	private void OnCurrentGoalAmountChanged()
	{
		UpdateHUD();
	}

	private void OnBronzeGoalReached()
	{
		m_bronzeBalloon.enabled = true;
	}

	private void OnSilverGoalReached()
	{
		m_silverBalloon.enabled = true;
	}

	private void OnGoldGoalReached()
	{
		m_goldBalloon.enabled = true;
	}

	private void UpdateHUD()
	{
		m_ammoText.text = $"{ClownRoomController.CurrentAmmo}/{ClownRoomController.StartingAmmo}";
		m_hitsText.text = ClownRoomController.CurrentGoalAmount.ToString();
	}

	private void ResetHUD()
	{
		m_ammoText.text = $"{ClownRoomController.CurrentAmmo}/{ClownRoomController.StartingAmmo}";
		m_hitsText.text = ClownRoomController.CurrentGoalAmount.ToString();
		m_bronzeBalloon.enabled = false;
		m_silverBalloon.enabled = false;
		m_goldBalloon.enabled = false;
		m_bronzeText.text = ClownRoomController.BronzeGoal.ToString();
		m_silverText.text = ClownRoomController.SilverGoal.ToString();
		m_goldText.text = ClownRoomController.GoldGoal.ToString();
	}
}
