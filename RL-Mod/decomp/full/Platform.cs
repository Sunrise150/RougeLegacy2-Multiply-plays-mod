using UnityEngine;

public class Platform : MonoBehaviour, IPlayHitEffect
{
	public bool PlayDirectionalHitEffect => false;

	public bool PlayHitEffect => true;

	public string EffectNameOverride => null;
}
