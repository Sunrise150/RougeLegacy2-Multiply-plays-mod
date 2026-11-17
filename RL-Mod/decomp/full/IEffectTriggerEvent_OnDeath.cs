using Sigtrap.Relays;
using UnityEngine;

public interface IEffectTriggerEvent_OnDeath
{
	IRelayLink<GameObject> OnDeathEffectTriggerRelay { get; }
}
