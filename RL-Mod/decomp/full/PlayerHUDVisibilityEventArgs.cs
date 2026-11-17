using System;

public class PlayerHUDVisibilityEventArgs : EventArgs
{
	public float TweenDuration { get; private set; }

	public PlayerHUDVisibilityEventArgs(float tweenDuration)
	{
		Initialize(tweenDuration);
	}

	public void Initialize(float tweenDuration)
	{
		TweenDuration = tweenDuration;
	}
}
