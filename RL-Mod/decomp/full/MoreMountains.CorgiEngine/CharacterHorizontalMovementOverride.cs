using UnityEngine;

namespace MoreMountains.CorgiEngine;

[AddComponentMenu("Corgi Engine/Environment/Character Hztal Mvt Override")]
public class CharacterHorizontalMovementOverride : MonoBehaviour
{
	public float MovementSpeed = 8f;

	protected float _previousMovementSpeed;

	protected virtual void OnTriggerEnter2D(Collider2D collider)
	{
		CharacterHorizontalMovement component = collider.GetComponent<CharacterHorizontalMovement>();
		if (!(component == null))
		{
			_previousMovementSpeed = component.MovementSpeed;
			component.MovementSpeed = MovementSpeed;
		}
	}

	protected virtual void OnTriggerExit2D(Collider2D collider)
	{
		CharacterHorizontalMovement component = collider.GetComponent<CharacterHorizontalMovement>();
		if (!(component == null))
		{
			component.MovementSpeed = _previousMovementSpeed;
		}
	}
}
