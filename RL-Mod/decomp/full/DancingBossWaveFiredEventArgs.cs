using System;
using UnityEngine;

public class DancingBossWaveFiredEventArgs : EventArgs
{
	public RoomSide Side { get; private set; }

	public Vector2 Origin { get; private set; }

	public float Speed { get; private set; }

	public float LifeSpan { get; private set; }

	public DancingBossWaveFiredEventArgs(RoomSide side, Vector2 origin, float speed, float lifeSpan)
	{
		Initialize(side, origin, speed, lifeSpan);
	}

	public void Initialize(RoomSide side, Vector2 origin, float speed, float lifeSpan)
	{
		Side = side;
		Origin = origin;
		Speed = speed;
		LifeSpan = lifeSpan;
	}
}
