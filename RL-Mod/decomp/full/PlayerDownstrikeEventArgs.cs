using System;
using UnityEngine;

public class PlayerDownstrikeEventArgs : EventArgs
{
	public Projectile_RL Projectile { get; private set; }

	public GameObject CollidedObj { get; private set; }

	public PlayerDownstrikeEventArgs(Projectile_RL projectile, GameObject collidedObj)
	{
		Initialise(projectile, collidedObj);
	}

	public void Initialise(Projectile_RL projectile, GameObject collidedObj)
	{
		Projectile = projectile;
		CollidedObj = collidedObj;
	}
}
