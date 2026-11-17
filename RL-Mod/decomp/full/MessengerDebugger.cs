using System.Collections.Generic;
using UnityEngine;

public class MessengerDebugger : MonoBehaviour
{
	[SerializeField]
	protected bool m_printDebugStatements;

	[SerializeField]
	private List<ListenerEntry> m_listeners;

	public List<ListenerEntry> Listeners
	{
		get
		{
			return m_listeners;
		}
		private set
		{
			m_listeners = value;
		}
	}
}
