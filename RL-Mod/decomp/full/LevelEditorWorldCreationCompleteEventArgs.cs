using System;

public class LevelEditorWorldCreationCompleteEventArgs : EventArgs
{
	public BaseRoom BuiltRoom { get; private set; }

	public LevelEditorWorldCreationCompleteEventArgs(BaseRoom builtRoom)
	{
		BuiltRoom = builtRoom;
	}
}
