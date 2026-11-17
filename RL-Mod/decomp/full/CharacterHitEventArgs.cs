using System;

public class CharacterHitEventArgs : EventArgs
{
	public IDamageObj Attacker { get; private set; }

	public BaseCharacterController Victim { get; private set; }

	public float DamageTaken { get; private set; }

	public CharacterHitEventArgs(IDamageObj attacker, BaseCharacterController victim, float damageTaken)
	{
		Initialize(attacker, victim, damageTaken);
	}

	public void Initialize(IDamageObj attacker, BaseCharacterController victim, float damageTaken)
	{
		Attacker = attacker;
		Victim = victim;
		DamageTaken = damageTaken;
	}
}
