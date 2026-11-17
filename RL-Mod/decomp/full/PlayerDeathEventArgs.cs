using System;
using UnityEngine;

public class PlayerDeathEventArgs : EventArgs
{
	public GameObject Killer { get; private set; }

	public PlayerController Victim { get; private set; }

	public PlayerDeathEventArgs(PlayerController victim, GameObject killer)
	{
		Initialize(victim, killer);
	}

	public void Initialize(PlayerController victim, GameObject killer)
	{
		Victim = victim;
		Killer = killer;
	}
}
