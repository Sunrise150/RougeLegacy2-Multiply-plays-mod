using UnityEngine;

public struct GridPointManagerContentEntry
{
	public RoomContentType ContentType { get; }

	public Vector2 LocalPosition { get; }

	public Vector2 WorldPosition { get; }

	public bool IsSpawned { get; }

	public GridPointManagerContentEntry(RoomContentType contentType, Vector2 localPosition, Vector2 worldPosition, bool isSpawned)
	{
		ContentType = contentType;
		LocalPosition = localPosition;
		WorldPosition = worldPosition;
		IsSpawned = isSpawned;
	}
}
