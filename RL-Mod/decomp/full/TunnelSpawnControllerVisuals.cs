using System;
using UnityEngine;

[Serializable]
public class TunnelSpawnControllerVisuals
{
	public SpriteRenderer SpriteRenderer;

	public Sprite DefaultSprite;

	public Sprite EntranceSprite;

	public Sprite ExitSprite;

	public Sprite BossEntranceSprite;

	public Sprite BossExitSprite;

	public Sprite FinalEntranceSprite;

	public Sprite FinalExitSprite;

	public void SetSprite(TunnelCategory category, TunnelDirection tunnelType)
	{
		Sprite sprite = DefaultSprite;
		Sprite sprite2 = EntranceSprite;
		Sprite sprite3 = ExitSprite;
		switch (category)
		{
		case TunnelCategory.Boss:
			sprite2 = BossEntranceSprite;
			sprite3 = BossExitSprite;
			break;
		case TunnelCategory.Final:
			sprite2 = FinalEntranceSprite;
			sprite3 = FinalExitSprite;
			break;
		}
		switch (tunnelType)
		{
		case TunnelDirection.Entrance:
			sprite = sprite2;
			break;
		case TunnelDirection.Exit:
			sprite = sprite3;
			break;
		}
		SpriteRenderer.sprite = sprite;
	}
}
