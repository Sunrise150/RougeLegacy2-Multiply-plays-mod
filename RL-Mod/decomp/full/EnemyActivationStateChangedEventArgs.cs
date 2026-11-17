using System;

public class EnemyActivationStateChangedEventArgs : EventArgs
{
	public EnemyController Enemy { get; private set; }

	public EnemyActivationStateChangedEventArgs(EnemyController enemy)
	{
		Initialize(enemy);
	}

	public void Initialize(EnemyController enemy)
	{
		Enemy = enemy;
	}
}
