using System;

public class PlayerCardOpenedEventArgs : EventArgs
{
	public PlayerSaveData PlayerData { get; private set; }

	public PlayerCardOpenedEventArgs(PlayerSaveData playerData)
	{
		Initialize(playerData);
	}

	public void Initialize(PlayerSaveData playerData)
	{
		PlayerData = playerData;
	}
}
