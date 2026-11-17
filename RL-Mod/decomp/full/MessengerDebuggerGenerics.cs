using System.Collections.Generic;
using UnityEngine;

public class MessengerDebuggerGenerics<T, U> : MonoBehaviour where T : Messenger<T, U>
{
	[SerializeField]
	protected bool m_printDebugStatements;

	[SerializeField]
	private List<ListenerEntry> m_listeners;
}
