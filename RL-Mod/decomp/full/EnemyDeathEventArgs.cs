using System;
using UnityEngine;

public class EnemyDeathEventArgs : EventArgs
{
	public GameObject Killer { get; private set; }

	public EnemyController Victim { get; private set; }

	public EnemyDeathEventArgs(EnemyController victim, GameObject killer)
	{
		Initialize(victim, killer);
	}

	public void Initialize(EnemyController victim, GameObject killer)
	{
		Victim = victim;
		Killer = killer;
	}
}
