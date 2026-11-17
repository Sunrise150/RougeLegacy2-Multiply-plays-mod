using System.Collections.Generic;
using UnityEngine;

namespace SceneManagement_RL;

[CreateAssetMenu(menuName = "Custom/Transition Library")]
public class TransitionLibrary : ScriptableObject
{
	[SerializeField]
	private Transition_V2[] m_transitions;

	private const string RESOURCES_PATH = "Scriptable Objects/Libraries/TransitionLibrary";

	public static string ASSET_PATH = "Assets/Content/Scriptable Objects/Libraries/TransitionLibrary.asset";

	private static Dictionary<TransitionID, ITransition> m_transitionTable = null;

	private static TransitionLibrary m_instance = null;

	private static TransitionLibrary Instance
	{
		get
		{
			if (m_instance == null)
			{
				m_instance = CDGResources.Load<TransitionLibrary>("Scriptable Objects/Libraries/TransitionLibrary");
			}
			return m_instance;
		}
	}

	public static void InitializeTransitionInstances()
	{
		if (m_transitionTable == null)
		{
			m_transitionTable = new Dictionary<TransitionID, ITransition>();
			Transition_V2[] transitions = Instance.m_transitions;
			for (int i = 0; i < transitions.Length; i++)
			{
				Transition_V2 transition_V = Object.Instantiate(transitions[i] as Transition_V2, null);
				m_transitionTable.Add(transition_V.ID, transition_V);
				transition_V.gameObject.SetActive(value: false);
				Object.DontDestroyOnLoad(transition_V);
			}
		}
	}

	public static ITransition GetTransitionInstance(TransitionID transitionID)
	{
		if (m_transitionTable == null)
		{
			InitializeTransitionInstances();
		}
		if (m_transitionTable.ContainsKey(transitionID))
		{
			return m_transitionTable[transitionID];
		}
		if (transitionID != 0)
		{
			Debug.LogFormat("<color=red>|{0}| Transition Table does not contain any entry for Transition ID ({1})</color>", Instance, transitionID);
		}
		return null;
	}
}
