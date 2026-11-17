using System;

[Serializable]
public class TunnelLibraryEntry
{
	public TunnelCategory Category;

	public Tunnel Prefab;

	public TunnelLibraryEntry(TunnelCategory category)
	{
		Category = category;
	}
}
