using Rewired;
using UnityEngine;

public class RLInputRemapListener : MonoBehaviour
{
	[SerializeField]
	private Rewired_RL.InputActionType m_inputActionType;

	[SerializeField]
	private bool m_useAxisContribution;

	[SerializeField]
	private bool m_useGamepad;

	[SerializeField]
	[ConditionalHide("m_useAxisContribution", true)]
	private Pole m_axisContribution;

	public bool UsesAxisContribution => m_useAxisContribution;

	public Pole Axis
	{
		get
		{
			if (UsesAxisContribution)
			{
				return m_axisContribution;
			}
			return Pole.Positive;
		}
	}

	public Rewired_RL.InputActionType InputActionType => m_inputActionType;

	public bool UsesGamepad => m_useGamepad;

	public void ListenForRemapInput()
	{
		if (m_useAxisContribution)
		{
			RLInputRemapper.ChangeInputRequested(InputActionType, UsesGamepad, m_axisContribution);
		}
		else
		{
			RLInputRemapper.ChangeInputRequested(InputActionType, UsesGamepad);
		}
	}
}
