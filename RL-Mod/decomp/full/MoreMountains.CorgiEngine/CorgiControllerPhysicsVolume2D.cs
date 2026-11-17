using UnityEngine;

namespace MoreMountains.CorgiEngine;

[AddComponentMenu("Corgi Engine/Environment/Corgi Controller Override")]
public class CorgiControllerPhysicsVolume2D : MonoBehaviour
{
	public CorgiControllerParameters ControllerParameters;

	[Header("Force Modification on Entry")]
	public bool ResetForcesOnEntry;

	public bool MultiplyForcesOnEntry;

	public Vector2 ForceMultiplierOnEntry = Vector2.zero;
}
