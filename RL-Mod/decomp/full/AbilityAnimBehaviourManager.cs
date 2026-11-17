using Sigtrap.Relays;

public class AbilityAnimBehaviourManager
{
	private AbilityAnimBehaviour m_currentAnimatorState;

	private bool m_animationComplete;

	private Relay<AbilityAnimState> m_onAnimStateEnterRelay = new Relay<AbilityAnimState>();

	private Relay<AbilityAnimState> m_onAnimStateExitRelay = new Relay<AbilityAnimState>();

	private static AbilityAnimBehaviourManager m_instance;

	private static AbilityAnimBehaviourManager Instance
	{
		get
		{
			if (m_instance == null)
			{
				m_instance = new AbilityAnimBehaviourManager();
				m_instance.Initialize();
			}
			return m_instance;
		}
	}

	public static IRelayLink<AbilityAnimState> OnAnimStateEnterRelay => Instance.m_onAnimStateEnterRelay.link;

	public static IRelayLink<AbilityAnimState> OnAnimStateExitRelay => Instance.m_onAnimStateExitRelay.link;

	public static AbilityAnimBehaviour CurrentAnimatorState
	{
		get
		{
			return Instance.m_currentAnimatorState;
		}
		set
		{
			Instance.m_currentAnimatorState = value;
		}
	}

	public static bool AnimationComplete
	{
		get
		{
			return Instance.m_animationComplete;
		}
		set
		{
			Instance.m_animationComplete = value;
		}
	}

	public static void InvokeAnimOnEnter(AbilityAnimState animState)
	{
		Instance.m_onAnimStateEnterRelay.Dispatch(animState);
	}

	public static void InvokeAnimOnExit(AbilityAnimState animState)
	{
		Instance.m_onAnimStateExitRelay.Dispatch(animState);
	}

	private void Initialize()
	{
	}

	public static void ResetManager()
	{
		CurrentAnimatorState = null;
		AnimationComplete = false;
	}
}
