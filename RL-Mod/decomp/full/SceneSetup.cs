using System;
using UnityEngine;

public abstract class SceneSetup : MonoBehaviour, ISceneSetup
{
	public abstract bool IsComplete { get; protected set; }

	public event EventHandler<EventArgs> Complete;

	protected void OnComplete(EventArgs eventArgs)
	{
		if (this.Complete != null)
		{
			this.Complete(this, EventArgs.Empty);
		}
	}
}
