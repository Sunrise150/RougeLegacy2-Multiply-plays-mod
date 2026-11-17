using UnityEngine;

namespace MoreMountains.CorgiEngine;

public struct MMDamageTakenEvent
{
	public Character AffectedCharacter;

	public GameObject Instigator;

	public float CurrentHealth;

	public float DamageCaused;

	public float PreviousHealth;

	public MMDamageTakenEvent(Character affectedCharacter, GameObject instigator, float currentHealth, float damageCaused, float previousHealth)
	{
		AffectedCharacter = affectedCharacter;
		Instigator = instigator;
		CurrentHealth = currentHealth;
		DamageCaused = damageCaused;
		PreviousHealth = previousHealth;
	}
}
