using Sigtrap.Relays;
using UnityEngine;

public interface IEffectTriggerEvent_OnTimeout
{
	IRelayLink<GameObject> OnTimeoutEffectTriggerRelay { get; }
}
