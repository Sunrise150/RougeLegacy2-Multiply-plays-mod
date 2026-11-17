using UnityEngine;

public abstract class SceneSetupConsumer : MonoBehaviour, ISceneSetupConsumer
{
	public abstract ISceneSetup SceneSetup { get; set; }
}
