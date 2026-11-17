using UnityEngine;

[CreateAssetMenu(fileName = "New Profile", menuName = "Custom/Mobile Post Processing Profile")]
public class MobilePostProcessingProfile : ScriptableObject
{
	[Header("Blur")]
	public bool EnableBlurEffect;

	[Space]
	public bool OverrideBlurOverallAmount;

	[Range(0f, 1f)]
	public float BlurOverallAmount = 1f;

	[Space]
	public bool OverrideBlurBaselineAmount;

	[Range(0f, 1f)]
	public float BlurBaselineAmount;

	[Space]
	public bool OverrideBlurRedChannel;

	[Range(-1f, 1f)]
	public float BlurRedChannel;

	[Space]
	public bool OverrideBlurGreenChannel;

	[Range(-1f, 1f)]
	public float BlurGreenChannel;

	[Space]
	public bool OverrideBlurBlueChannel;

	[Range(-1f, 1f)]
	public float BlurBlueChannel;

	[Space]
	public bool OverrideBlurAlphaChannel;

	[Range(-1f, 1f)]
	public float BlurAlphaChannel;

	[Header("Bloom")]
	public bool EnableBloomEffect;

	[Space]
	public bool OverrideBloomColor;

	public Color BloomColor = MobilePostProcessing.BLOOM_COLOR_DEFAULT;

	[Space]
	public bool OverrideBloomAmount;

	[Range(0f, 5f)]
	public float BloomAmount = 1f;

	[Space]
	public bool OverrideBloomDiffuse;

	[Range(0f, 1f)]
	public float BloomDiffuse = 0.5f;

	[Space]
	public bool OverrideBloomThreshold;

	[Range(0f, 1f)]
	public float BloomThreshold = 0.5f;

	[Space]
	public bool OverrideBloomSoftness;

	[Range(0f, 1f)]
	public float BloomSoftness = 0.5f;

	[Space]
	[Header("LUT")]
	public bool EnableLUTEffect;

	[Space]
	public bool OverrideLutAmount;

	[Range(0f, 1f)]
	public float LutAmount = 0.5f;

	[Space]
	public bool OverrideSourceLut;

	public Texture2D SourceLut;

	[Space]
	[Header("Image Filtering")]
	public bool EnableImageFilteringEffect;

	[Space]
	public bool OverrideColor;

	public Color Color = MobilePostProcessing.COLOR_DEFAULT;

	[Space]
	public bool OverrideContrast;

	[Range(0f, 1f)]
	public float Contrast;

	[Space]
	public bool OverrideBrightness;

	[Range(-1f, 1f)]
	public float Brightness;

	[Space]
	public bool OverrideSaturation;

	[Range(-1f, 1f)]
	public float Saturation;

	[Space]
	public bool OverrideExposure;

	[Range(-1f, 1f)]
	public float Exposure;

	[Space]
	public bool OverrideGamma;

	[Range(-1f, 1f)]
	public float Gamma;

	[Space]
	[Header("Gradient Map")]
	public bool EnableGradientMapEffect;

	[Space]
	public bool OverrideGradientBase;

	public Color GradientBase = MobilePostProcessing.GRADIENT_BASE_DEFAULT;

	[Space]
	public bool OverrideGradientDark;

	public Color GradientDark = MobilePostProcessing.GRADIENT_DARK_DEFAULT;

	[Space]
	public bool OverrideGradientMid;

	public Color GradientMid = MobilePostProcessing.GRADIENT_MID_DEFAULT;

	[Space]
	public bool OverrideGradientLight;

	public Color GradientLight = MobilePostProcessing.GRADIENT_LIGHT_DEFAULT;

	[Space]
	public bool OverrideGradientAmount;

	[Range(0f, 1f)]
	public float GradientAmount = 0.5f;

	[Space]
	public bool OverrideDarkCutoff;

	[Range(0f, 0.3f)]
	public float DarkCutoff;

	[Space]
	public bool OverrideLightCutoff;

	[Range(0.7f, 1f)]
	public float LightCutoff = 1f;

	[Space]
	[Header("Overlay Gradient")]
	public bool EnableOverlayGradientEffect;

	[Space]
	public bool OverrideOverlayTop;

	public Color OverlayTop = MobilePostProcessing.OVERLAY_TOP_DEFAULT;

	[Space]
	public bool OverrideOverlayBottom;

	public Color OverlayBottom = MobilePostProcessing.OVERLAY_BOTTOM_DEFAULT;

	[Space]
	public bool OverrideOverlayAmount;

	[Range(0f, 1f)]
	public float OverlayAmount = 0.5f;

	[Space]
	[Header("Clamp Black")]
	public bool EnableClampBlackEffect;

	[Space]
	public bool OverrideClampColor;

	public Color ClampColor = MobilePostProcessing.CLAMP_COLOR_DEFAULT;

	[Space]
	public bool OverrideClampFillAmount;

	[Range(0f, 1f)]
	public float ClampFillAmount = 0.5f;

	[Space]
	[Header("Tint")]
	public bool EnableTintEffect;

	[Space]
	public bool OverrideTintColor;

	public Color TintColor = MobilePostProcessing.TINT_COLOR_DEFAULT;

	[Space]
	public bool OverrideTintOverallAmount;

	[Range(0f, 1f)]
	public float TintOverallAmount = 1f;

	[Space]
	public bool OverrideTintBaselineAmount;

	[Range(0f, 1f)]
	public float TintBaselineAmount;

	[Space]
	public bool OverrideTintRedChannel;

	[Range(-1f, 1f)]
	public float TintRedChannel;

	[Space]
	public bool OverrideTintGreenChannel;

	[Range(-1f, 1f)]
	public float TintGreenChannel;

	[Space]
	public bool OverrideTintBlueChannel;

	[Range(-1f, 1f)]
	public float TintBlueChannel;

	[Space]
	public bool OverrideTintAlphaChannel;

	[Range(-1f, 1f)]
	public float TintAlphaChannel;

	[Space]
	[Header("Chromatic Abberation")]
	public bool EnableChromaticAbberationEffect;

	[Space]
	public bool OverrideOffset;

	public float Offset = 1f;

	[Space]
	public bool OverrideFishEyeDistortion;

	[Range(-1f, 1f)]
	public float FishEyeDistortion;

	[Space]
	[Header("Distortion")]
	public bool EnableDistortionEffect;

	[Space]
	public bool OverrideLensDistortion;

	[Range(0f, 1f)]
	public float LensDistortion = 0.5f;

	[Space]
	[Header("Vignette")]
	public bool EnableVignetteEffect;

	[Space]
	public bool OverrideVignetteCenter;

	public Vector2 VignetteCenter = MobilePostProcessing.VIGNETTE_CENTER_DEFAULT;

	[Space]
	public bool OverrideVignetteColor;

	public Color VignetteColor = MobilePostProcessing.VIGNETTE_COLOR_DEFAULT;

	[Space]
	public bool OverrideVignetteAmount;

	[Range(0f, 1f)]
	public float VignetteAmount = 0.5f;

	[Space]
	public bool OverrideVignetteSoftness;

	[Range(0.001f, 1f)]
	public float VignetteSoftness = 0.5f;

	[Space]
	public bool OverrideVignetteRoundness;

	[Range(0f, 1f)]
	public float VignetteRoundness = 0.5f;

	[Space]
	[Header("Mist")]
	public bool EnableMistEffect;

	[Space]
	public bool OverrideMistColor;

	public Color MistColor = MobilePostProcessing.MIST_COLOR_DEFAULT;

	[Space]
	public bool OverrideMistAmount;

	[Range(0f, 1f)]
	public float MistAmount = 0.1f;

	[Space]
	[Header("Pixelation")]
	public bool EnablePixelationEffect;

	[Space]
	public bool OverridePixelResolution;

	public Vector2 PixelResolution = MobilePostProcessing.PIXEL_RESOLUTION_DEFAULT;

	[Space]
	public bool OverridePixelBaselineAmount;

	[Range(0f, 1f)]
	public float PixelBaselineAmount;

	[Space]
	public bool OverridePixelRedChannel;

	[Range(-1f, 1f)]
	public float PixelRedChannel;

	[Space]
	public bool OverridePixelGreenChannel;

	[Range(-1f, 1f)]
	public float PixelGreenChannel;

	[Space]
	public bool OverridePixelBlueChannel;

	[Range(-1f, 1f)]
	public float PixelBlueChannel;

	[Space]
	public bool OverridePixelAlphaChannel;

	[Range(-1f, 1f)]
	public float PixelAlphaChannel;

	[Space]
	[Header("Dither")]
	public bool EnableDitherEffect;

	[Space]
	public bool OverrideDitherDark;

	public Color DitherDark = MobilePostProcessing.DITHER_DARK_DEFAULT;

	[Space]
	public bool OverrideDitherLight;

	public Color DitherLight = MobilePostProcessing.DITHER_LIGHT_DEFAULT;

	[Space]
	[Header("Screen Flip")]
	public bool OverrideScreenFlipXEffect;

	public bool ScreenFlipX;

	[Space]
	public bool OverrideScreenFlipYEffect;

	public bool ScreenFlipY;

	[Space]
	[Header("Animated Overlay")]
	public bool EnableAnimatedOverlayEffect;

	[Space]
	public bool OverrideAnimOverlayTexture;

	public Texture2D AnimOverlayTexture = MobilePostProcessing.ANIMOVERLAY_TEXTURE_DEFAULT;

	[Space]
	public bool OverrideAnimOverlaySpritesheetSize;

	public Vector2 AnimOverlaySpritesheetSize = MobilePostProcessing.ANIMOVERLAY_SPRITESHEET_SIZE_DEFAULT;

	[Space]
	public bool OverrideAnimOverlayFramerate;

	[Range(0f, 60f)]
	public int AnimOverlayFramerate = 30;

	[Space]
	public bool OverrideAnimOverlayStrength;

	[Range(0f, 1f)]
	public float AnimOverlayStrength = 0.5f;

	[Space]
	[Header("Circular Darkness")]
	public bool EnableCircularDarknessEffect;

	[Space]
	public bool OverrideCircDarknessColor;

	public Color CircDarknessColor = MobilePostProcessing.CIRCDARKNESS_COLOR_DEFAULT;

	[Space]
	public bool OverrideCircDarknessAmount;

	[Range(0f, 1f)]
	public float CircDarknessAmount = 0.5f;

	[Space]
	public bool OverrideCircDarknessSoftness;

	[Range(0.001f, 1f)]
	public float CircDarknessSoftness = 0.5f;

	public MobilePostProcessingProfile Clone()
	{
		return (MobilePostProcessingProfile)MemberwiseClone();
	}
}
