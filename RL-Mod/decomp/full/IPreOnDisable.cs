using Sigtrap.Relays;
using UnityEngine;

public interface IPreOnDisable
{
	GameObject gameObject { get; }

	IRelayLink<IPreOnDisable> PreOnDisableRelay { get; }
}
