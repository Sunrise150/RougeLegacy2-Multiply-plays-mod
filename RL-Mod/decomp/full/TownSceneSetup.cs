using System;
using System.Collections.Generic;
using UnityEngine;

public class TownSceneSetup : SceneSetup, ISceneSetupBroker
{
	[SerializeField]
	private List<SceneSetupConsumer> m_sceneSetupConsumers;

	public override bool IsComplete { get; protected set; }

	private void Start()
	{
		InjectSceneSetup();
		Debug.LogFormat("<color=blue>{0}: Town Scene Setup Complete!</color>", Time.frameCount);
		IsComplete = true;
		OnComplete(EventArgs.Empty);
	}

	public void InjectSceneSetup()
	{
		foreach (SceneSetupConsumer sceneSetupConsumer in m_sceneSetupConsumers)
		{
			((ISceneSetupConsumer)sceneSetupConsumer).SceneSetup = this;
		}
	}
}
