using System;
using UnityEngine;
using UnityEngine.Serialization;

[Serializable]
public class IntroSkyColorEntry
{
	public Color SkyColor;

	public Color HorizonColor;

	public Color CloudMainColor;

	public Color CloudHighlightColor;

	public Color StarColor;

	public Color RightColor;

	public Color BottomColor;

	[FormerlySerializedAs("FogColor")]
	public Color MistColor;

	public Color FogRenderColor;

	public Color Beam1Color;

	public Color Beam2Color;

	public Color RubbleColor1;

	public Color RubbleColor2;

	public Color GlowColor;

	public MobilePostProcessingProfile PostProcessProfile;

	public Sprite BannerSprite;
}
