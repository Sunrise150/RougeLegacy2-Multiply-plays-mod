using Sigtrap.Relays;
using UnityEngine;

public abstract class FairyRule : MonoBehaviour
{
	private FairyRoomState m_state;

	private static FairyRoomRuleStateChangeEventArgs m_fairyRuleChangeEventArgs_STATIC = new FairyRoomRuleStateChangeEventArgs(null);

	private Relay<FairyRoomRuleStateChangeEventArgs> m_stateChangeRelay = new Relay<FairyRoomRuleStateChangeEventArgs>();

	public IRelayLink<FairyRoomRuleStateChangeEventArgs> StateChangeRelay => m_stateChangeRelay.link;

	public FairyRoomState State
	{
		get
		{
			return m_state;
		}
		protected set
		{
			m_state = value;
		}
	}

	public abstract string Description { get; }

	public abstract bool LockChestAtStart { get; }

	public abstract FairyRuleID ID { get; }

	public abstract void RunRule(FairyRoomController fairyRoomController);

	public virtual void StopRule()
	{
		if (State != FairyRoomState.Failed && State != FairyRoomState.Passed)
		{
			State = FairyRoomState.NotRunning;
		}
	}

	public void SetIsPassed()
	{
		State = FairyRoomState.Passed;
		FireStateChangeEvent();
	}

	public void SetIsFailed()
	{
		State = FairyRoomState.Failed;
		FireStateChangeEvent();
	}

	public virtual void ResetRule()
	{
		State = FairyRoomState.NotRunning;
		FireStateChangeEvent();
	}

	protected void FireStateChangeEvent()
	{
		m_fairyRuleChangeEventArgs_STATIC.Initialise(this);
		m_stateChangeRelay.Dispatch(m_fairyRuleChangeEventArgs_STATIC);
	}
}
