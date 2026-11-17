using Sigtrap.Relays;
using UnityEngine;

public interface IEffectTriggerEvent_OnDamage
{
	IRelayLink<GameObject, float, bool> OnDamageEffectTriggerRelay { get; }
}
