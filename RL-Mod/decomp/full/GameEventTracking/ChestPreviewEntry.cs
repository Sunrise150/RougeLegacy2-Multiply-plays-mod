using System;
using UnityEngine;

namespace GameEventTracking;

[Serializable]
public class ChestPreviewEntry
{
	[ReadOnly]
	public ChestType ChestType;

	public Sprite PreviewImage;

	public ChestPreviewEntry(ChestType chestType)
	{
		ChestType = chestType;
	}
}
