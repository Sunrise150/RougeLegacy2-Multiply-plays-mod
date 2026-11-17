using System;

public interface ISceneSetup
{
	bool IsComplete { get; }

	event EventHandler<EventArgs> Complete;
}
