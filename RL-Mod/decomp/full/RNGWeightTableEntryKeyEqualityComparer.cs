using System.Collections.Generic;

public class RNGWeightTableEntryKeyEqualityComparer : IEqualityComparer<RNGWeightTableEntryKey>
{
	public bool Equals(RNGWeightTableEntryKey x, RNGWeightTableEntryKey y)
	{
		bool num = x.ID == y.ID;
		bool flag = x.IsMirrored == y.IsMirrored;
		return num && flag;
	}

	public int GetHashCode(RNGWeightTableEntryKey obj)
	{
		int num = 1;
		if (obj.IsMirrored)
		{
			num = 2;
		}
		return obj.ID * num;
	}
}
