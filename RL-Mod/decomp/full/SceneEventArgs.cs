using System;

public class SceneEventArgs : EventArgs
{
	public string SceneName { get; private set; }

	public SceneEventArgs(string sceneName)
	{
		SceneName = sceneName;
	}
}
