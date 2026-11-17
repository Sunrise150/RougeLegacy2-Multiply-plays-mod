public struct RNGWeightTableEntryKey
{
	public bool IsMirrored { get; }

	public int ID { get; }

	public RNGWeightTableEntryKey(int roomHashID, bool isMirrored)
	{
		ID = roomHashID;
		IsMirrored = isMirrored;
	}
}
