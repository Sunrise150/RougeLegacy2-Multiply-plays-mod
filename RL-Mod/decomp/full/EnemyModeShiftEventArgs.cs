using System;

public class EnemyModeShiftEventArgs : EventArgs
{
	public EnemyController Enemy { get; private set; }

	public EnemyModeShiftEventArgs(EnemyController enemy)
	{
		Initialize(enemy);
	}

	public void Initialize(EnemyController enemy)
	{
		Enemy = enemy;
	}
}
