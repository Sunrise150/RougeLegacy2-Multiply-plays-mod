namespace RLWorldCreation;

public class SetRoomCountEventArgs : BiomeCreationEventArgs
{
	public int StandardCount { get; }

	public int FairyCount { get; }

	public int TrapCount { get; }

	public int BonusCount { get; }

	public int MandatoryCount { get; }

	public SetRoomCountEventArgs(int standardCount, int fairyCount, int trapCount, int bonusCount, int mandatoryCount)
	{
		StandardCount = standardCount;
		FairyCount = fairyCount;
		TrapCount = trapCount;
		BonusCount = bonusCount;
		MandatoryCount = mandatoryCount;
	}
}
