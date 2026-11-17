using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class MobilePostProcessing : MonoBehaviour
{
	[Space]
	public RenderTexture Mask;

	[SerializeField]
	private MobilePostProcessingProfile baseProfile;

	private static readonly int ID_maskTexture = Shader.PropertyToID("_MaskTex");

	public static readonly string BlurKeyword = "BLUR";

	public const float BLUR_OVERALL_AMOUNT_DEFAULT = 1f;

	public const float BLUR_BASELINE_AMOUNT_DEFAULT = 0f;

	public const float BLUR_RED_CHANNEL_DEFAULT = 0f;

	public const float BLUR_GREEN_CHANNEL_DEFAULT = 0f;

	public const float BLUR_BLUE_CHANNEL_DEFAULT = 0f;

	public const float BLUR_ALPHA_CHANNEL_DEFAULT = 0f;

	private static readonly int ID_blurTex = Shader.PropertyToID("_BlurTex");

	private static readonly int ID_blurOverallAmount = Shader.PropertyToID("_BlurOverallAmount");

	private static readonly int ID_blurBaselineAmount = Shader.PropertyToID("_BlurBaselineAmount");

	private static readonly int ID_blurRGBAChannels = Shader.PropertyToID("_BlurRGBAChannels");

	[Space]
	public bool Blur;

	[Range(0f, 1f)]
	public float BlurOverallAmount = 1f;

	[Range(0f, 1f)]
	public float BlurBaselineAmount;

	[Range(-1f, 1f)]
	public float BlurRedChannel;

	[Range(-1f, 1f)]
	public float BlurGreenChannel;

	[Range(-1f, 1f)]
	public float BlurBlueChannel;

	[Range(-1f, 1f)]
	public float BlurAlphaChannel;

	public static readonly string BloomKeyword = "BLOOM";

	public static readonly Color BLOOM_COLOR_DEFAULT = Color.white;

	public const float BLOOM_AMOUNT_DEFAULT = 1f;

	public const float BLOOM_DIFFUSE_DEFAULT = 0.5f;

	public const float BLOOM_THRESHOLD_DEFAULT = 0.5f;

	public const float BLOOM_SOFTNESS_DEFAULT = 0.5f;

	private static readonly int ID_bloomColor = Shader.PropertyToID("_BloomColor");

	private static readonly int ID_bloomDiffuse = Shader.PropertyToID("_BloomDiffuse");

	private static readonly int ID_bloomData = Shader.PropertyToID("_BloomData");

	[Space]
	public bool Bloom;

	public Color BloomColor = BLOOM_COLOR_DEFAULT;

	[Range(0f, 5f)]
	public float BloomAmount = 1f;

	[Range(0f, 1f)]
	public float BloomDiffuse = 0.5f;

	[Range(0f, 1f)]
	public float BloomThreshold = 0.5f;

	[Range(0f, 1f)]
	public float BloomSoftness = 0.5f;

	public static readonly string LutKeyword = "LUT";

	public const Texture2D SOURCE_LUT_DEFAULT = null;

	public const float LUT_AMOUNT_DEFAULT = 0.5f;

	private static readonly int ID_lutTexture = Shader.PropertyToID("_LutTex");

	private static readonly int ID_lutAmount = Shader.PropertyToID("_LutAmount");

	[Space]
	public bool LUT;

	[Range(0f, 1f)]
	public float LutAmount = 0.5f;

	public Texture2D SourceLut;

	public static readonly string FilterKeyword = "FILTER";

	public static readonly Color COLOR_DEFAULT = Color.white;

	public const float CONTRAST_DEFAULT = 0f;

	public const float BRIGHTNESS_DEFAULT = 0f;

	public const float SATURATION_DEFAULT = 0f;

	public const float EXPOSURE_DEFAULT = 0f;

	public const float GAMMA_DEFAULT = 0f;

	private static readonly int ID_filterColor = Shader.PropertyToID("_FilterColor");

	private static readonly int ID_filterContrast = Shader.PropertyToID("_FilterContrast");

	private static readonly int ID_filterBrightness = Shader.PropertyToID("_FilterBrightness");

	private static readonly int ID_filterSaturation = Shader.PropertyToID("_FilterSaturation");

	[Space]
	public bool ImageFiltering;

	public Color Color = Color.white;

	[Range(0f, 1f)]
	public float Contrast;

	[Range(-1f, 1f)]
	public float Brightness;

	[Range(-1f, 1f)]
	public float Saturation;

	[Range(-1f, 1f)]
	public float Exposure;

	[Range(-1f, 1f)]
	public float Gamma;

	public static readonly string GradientKeyword = "GRADIENT";

	public static readonly Color GRADIENT_BASE_DEFAULT = Color.black;

	public static readonly Color GRADIENT_DARK_DEFAULT = Color.white;

	public static readonly Color GRADIENT_MID_DEFAULT = Color.white;

	public static readonly Color GRADIENT_LIGHT_DEFAULT = Color.white;

	public const float GRADIENT_AMOUNT_DEFAULT = 0.5f;

	public const float DARK_CUTOFF_DEFAULT = 0f;

	public const float LIGHT_CUTOFF_DEFAULT = 1f;

	private static readonly int ID_gradientBase = Shader.PropertyToID("_GradientBase");

	private static readonly int ID_gradientDark = Shader.PropertyToID("_GradientDark");

	private static readonly int ID_gradientMid = Shader.PropertyToID("_GradientMid");

	private static readonly int ID_gradientLight = Shader.PropertyToID("_GradientLight");

	private static readonly int ID_gradientAmount = Shader.PropertyToID("_GradientAmount");

	private static readonly int ID_gradientDarkCutoff = Shader.PropertyToID("_GradientDarkCutoff");

	private static readonly int ID_gradientLightCutoff = Shader.PropertyToID("_GradientLightCutoff");

	[Space]
	public bool GradientMap;

	public Color GradientBase = GRADIENT_BASE_DEFAULT;

	public Color GradientDark = GRADIENT_DARK_DEFAULT;

	public Color GradientMid = GRADIENT_MID_DEFAULT;

	public Color GradientLight = GRADIENT_LIGHT_DEFAULT;

	[Range(0f, 1f)]
	public float GradientAmount = 0.5f;

	[Range(0f, 0.3f)]
	public float DarkCutoff;

	[Range(0.7f, 1f)]
	public float LightCutoff = 1f;

	public static readonly string OverlayKeyword = "OVERLAY";

	public static readonly Color OVERLAY_TOP_DEFAULT = Color.white;

	public static readonly Color OVERLAY_BOTTOM_DEFAULT = Color.black;

	public const float OVERLAY_AMOUNT_DEFAULT = 0.5f;

	private static readonly int ID_overlayTop = Shader.PropertyToID("_OverlayTop");

	private static readonly int ID_overlayBottom = Shader.PropertyToID("_OverlayBottom");

	private static readonly int ID_overlayAmount = Shader.PropertyToID("_OverlayAmount");

	[Space]
	public bool OverlayGradient;

	public Color OverlayTop = OVERLAY_TOP_DEFAULT;

	public Color OverlayBottom = OVERLAY_BOTTOM_DEFAULT;

	[Range(0f, 1f)]
	public float OverlayAmount = 0.5f;

	public static readonly string ClampKeyword = "CLAMP";

	public static readonly Color CLAMP_COLOR_DEFAULT = Color.black;

	public const float CLAMP_FILL_AMOUNT_DEFAULT = 0.5f;

	private static readonly int ID_clampColor = Shader.PropertyToID("_ClampColor");

	private static readonly int ID_clampFillAmount = Shader.PropertyToID("_ClampFillAmount");

	[Space]
	public bool ClampBlack;

	public Color ClampColor = CLAMP_COLOR_DEFAULT;

	[Range(0f, 1f)]
	public float ClampFillAmount = 0.5f;

	public static readonly string TintKeyword = "TINT";

	public static readonly Color TINT_COLOR_DEFAULT = Color.black;

	public const float TINT_OVERALL_AMOUNT_DEFAULT = 1f;

	public const float TINT_BASELINE_AMOUNT_DEFAULT = 0f;

	public const float TINT_RED_CHANNEL_DEFAULT = 0f;

	public const float TINT_GREEN_CHANNEL_DEFAULT = 0f;

	public const float TINT_BLUE_CHANNEL_DEFAULT = 0f;

	public const float TINT_ALPHA_CHANNEL_DEFAULT = 0f;

	private static readonly int ID_tintColor = Shader.PropertyToID("_TintColor");

	private static readonly int ID_tintOverallAmount = Shader.PropertyToID("_TintOverallAmount");

	private static readonly int ID_tintBaselineAmount = Shader.PropertyToID("_TintBaselineAmount");

	private static readonly int ID_tintRGBAChannels = Shader.PropertyToID("_TintRGBAChannels");

	[Space]
	public bool Tint;

	public Color TintColor = TINT_COLOR_DEFAULT;

	[Range(0f, 1f)]
	public float TintOverallAmount = 1f;

	[Range(0f, 1f)]
	public float TintBaselineAmount;

	[Range(-1f, 1f)]
	public float TintRedChannel;

	[Range(-1f, 1f)]
	public float TintGreenChannel;

	[Range(-1f, 1f)]
	public float TintBlueChannel;

	[Range(-1f, 1f)]
	public float TintAlphaChannel;

	public static readonly string ChromaKeyword = "CHROMA";

	public const float OFFSET_DEFAULT = 1f;

	public const float FISH_EYE_DISTORTION_DEFAULT = 0f;

	private static readonly int ID_chromaOffset = Shader.PropertyToID("_ChromaOffset");

	private static readonly int ID_chromaFishEye = Shader.PropertyToID("_ChromaFishEye");

	[Space]
	public bool ChromaticAberration;

	public float Offset = 1f;

	[Range(-1f, 1f)]
	public float FishEyeDistortion;

	public static readonly string DistortionKeyword = "DISTORTION";

	public const float LENS_DISTORTION_DEFAULT = 0.5f;

	private static readonly int ID_lensdistortion = Shader.PropertyToID("_LensDistortion");

	[Space]
	public bool Distortion;

	[Range(0f, 1f)]
	public float LensDistortion = 0.5f;

	public static readonly string VignetteKeyword = "VIGNETTE";

	public static readonly Vector2 VIGNETTE_CENTER_DEFAULT = new Vector2(0.5f, 0.5f);

	public static readonly Color VIGNETTE_COLOR_DEFAULT = Color.black;

	public const float VIGNETTE_AMOUNT_DEFAULT = 0.5f;

	public const float VIGNETTE_SOFTNESS_DEFAULT = 0.5f;

	public const float VIGNETTE_ROUNDNESS_DEFAULT = 0.5f;

	private static readonly int ID_vignetteColor = Shader.PropertyToID("_VignetteColor");

	private static readonly int ID_vignetteAmount = Shader.PropertyToID("_VignetteAmount");

	private static readonly int ID_vignetteSoftness = Shader.PropertyToID("_VignetteSoftness");

	private static readonly int ID_vignetteCenter = Shader.PropertyToID("_VignetteCenter");

	private static readonly int ID_vignetteRoundness = Shader.PropertyToID("_VignetteRoundness");

	[Space]
	public bool Vignette;

	public Vector2 VignetteCenter = VIGNETTE_CENTER_DEFAULT;

	public Color VignetteColor = VIGNETTE_COLOR_DEFAULT;

	[Range(0f, 1f)]
	public float VignetteAmount = 0.5f;

	[Range(0.001f, 1f)]
	public float VignetteSoftness = 0.5f;

	[Range(0f, 1f)]
	public float VignetteRoundness = 0.5f;

	public static readonly string MistKeyword = "MIST";

	public static readonly Color MIST_COLOR_DEFAULT = Color.white;

	public const float MIST_AMOUNT_DEFAULT = 0.1f;

	private static readonly int ID_mistColor = Shader.PropertyToID("_MistColor");

	private static readonly int ID_mistAmount = Shader.PropertyToID("_MistAmount");

	[Space]
	public bool Mist;

	public Color MistColor = MIST_COLOR_DEFAULT;

	[Range(0f, 1f)]
	public float MistAmount = 0.1f;

	public static readonly Vector2 PIXEL_RESOLUTION_DEFAULT = new Vector2(100f, 100f);

	public const float PIXEL_BASELINE_AMOUNT_DEFAULT = 0f;

	public const float PIXEL_RED_CHANNEL_DEFAULT = 0f;

	public const float PIXEL_GREEN_CHANNEL_DEFAULT = 0f;

	public const float PIXEL_BLUE_CHANNEL_DEFAULT = 0f;

	public const float PIXEL_ALPHA_CHANNEL_DEFAULT = 0f;

	private static readonly int ID_pixelResolution = Shader.PropertyToID("_PixelResolution");

	private static readonly int ID_pixelBaselineAmount = Shader.PropertyToID("_PixelBaselineAmount");

	private static readonly int ID_pixelRGBAChannels = Shader.PropertyToID("_PixelRGBAChannels");

	[Space]
	public bool Pixelation;

	public Vector2 PixelResolution = PIXEL_RESOLUTION_DEFAULT;

	[Range(0f, 1f)]
	public float PixelBaselineAmount;

	[Range(-1f, 1f)]
	public float PixelRedChannel;

	[Range(-1f, 1f)]
	public float PixelGreenChannel;

	[Range(-1f, 1f)]
	public float PixelBlueChannel;

	[Range(-1f, 1f)]
	public float PixelAlphaChannel;

	public static readonly string DitherKeyword = "DITHER";

	public static readonly Color DITHER_DARK_DEFAULT = Color.black;

	public static readonly Color DITHER_LIGHT_DEFAULT = Color.white;

	private static readonly int ID_ditherDark = Shader.PropertyToID("_DitherDark");

	private static readonly int ID_ditherLight = Shader.PropertyToID("_DitherLight");

	[Space]
	public bool Dither;

	public Color DitherDark = DITHER_DARK_DEFAULT;

	public Color DitherLight = DITHER_LIGHT_DEFAULT;

	public static readonly string ScreenFlipXKeyword = "SCREENFLIP_X";

	public static readonly string ScreenFlipYKeyword = "SCREENFLIP_Y";

	[Space]
	public bool ScreenFlipX;

	public bool ScreenFlipY;

	public static readonly string AnimOverlayKeyword = "ANIMOVERLAY";

	public static readonly Texture2D ANIMOVERLAY_TEXTURE_DEFAULT = null;

	public static readonly Vector2 ANIMOVERLAY_SPRITESHEET_SIZE_DEFAULT = Vector2.one;

	public const int ANIMOVERLAY_FRAMERATE_DEFAULT = 30;

	public const float ANIMOVERLAY_STRENGTH_DEFAULT = 0.5f;

	private static readonly int ID_animOverlayTexture = Shader.PropertyToID("_AnimOverlayTex");

	private static readonly int ID_animOverlaySpritesheetSize = Shader.PropertyToID("_AnimOverlaySpritesheetSize");

	private static readonly int ID_animOverlayFramerate = Shader.PropertyToID("_AnimOverlayFramerate");

	private static readonly int ID_animOverlayStrength = Shader.PropertyToID("_AnimOverlayStrength");

	[Space]
	public bool AnimatedOverlay;

	public Texture2D AnimOverlayTexture = ANIMOVERLAY_TEXTURE_DEFAULT;

	public Vector2 AnimOverlaySpritesheetSize = ANIMOVERLAY_SPRITESHEET_SIZE_DEFAULT;

	[Range(0f, 60f)]
	public int AnimOverlayFramerate = 30;

	[Range(0f, 1f)]
	public float AnimOverlayStrength = 0.5f;

	public static readonly string CircDarknessKeyword = "CIRCDARKNESS";

	public static readonly Color CIRCDARKNESS_COLOR_DEFAULT = Color.black;

	public const float CIRCDARKNESS_AMOUNT_DEFAULT = 0.5f;

	public const float CIRCDARKNESS_SOFTNESS_DEFAULT = 0.5f;

	private static readonly int ID_circDarknessColor = Shader.PropertyToID("_CircDarknessColor");

	private static readonly int ID_circDarknessAmount = Shader.PropertyToID("_CircDarknessAmount");

	private static readonly int ID_circDarknessSoftness = Shader.PropertyToID("_CircDarknessSoftness");

	private static readonly int ID_circDarknessCenter = Shader.PropertyToID("_CircDarknessCenter");

	[Space]
	public bool CircularDarkness;

	public Color CircDarknessColor = CIRCDARKNESS_COLOR_DEFAULT;

	[Range(0f, 1f)]
	public float CircDarknessAmount = 0.5f;

	[Range(0.001f, 1f)]
	public float CircDarknessSoftness = 0.5f;

	private Material m_material;

	private MobilePostProcessingProfile m_previousProfile;

	private Texture2D m_previousLut;

	private Texture3D m_converted3D;

	private List<MobilePostProcessingProfile> m_traitOverrides = new List<MobilePostProcessingProfile>(2);

	private List<MobilePostProcessingProfile> m_dimensionOverrides = new List<MobilePostProcessingProfile>(2);

	private bool m_isDirty;

	private Coroutine m_lerpToProfileCoroutine;

	private Tween[] m_tweenArray = new Tween[15];

	private void ResetBlur()
	{
		Blur = false;
		BlurOverallAmount = 1f;
		BlurBaselineAmount = 0f;
		BlurRedChannel = 0f;
		BlurGreenChannel = 0f;
		BlurBlueChannel = 0f;
		BlurAlphaChannel = 0f;
	}

	private void ApplyProfileBlur(MobilePostProcessingProfile profile)
	{
		if (profile.EnableBlurEffect)
		{
			Blur = true;
			if (profile.OverrideBlurOverallAmount)
			{
				BlurOverallAmount = profile.BlurOverallAmount;
			}
			if (profile.OverrideBlurBaselineAmount)
			{
				BlurBaselineAmount = profile.BlurBaselineAmount;
			}
			if (profile.OverrideBlurRedChannel)
			{
				BlurRedChannel = profile.BlurRedChannel;
			}
			if (profile.OverrideBlurGreenChannel)
			{
				BlurGreenChannel = profile.BlurGreenChannel;
			}
			if (profile.OverrideBlurBlueChannel)
			{
				BlurBlueChannel = profile.BlurBlueChannel;
			}
			if (profile.OverrideBlurAlphaChannel)
			{
				BlurAlphaChannel = profile.BlurAlphaChannel;
			}
		}
	}

	private void ApplyShaderBlur()
	{
		if (Blur && BlurOverallAmount > 0f)
		{
			m_material.EnableKeyword(BlurKeyword);
			int num = Mathf.Max(Mathf.CeilToInt(BlurOverallAmount * 4f), 1);
			m_material.SetFloat(ID_blurOverallAmount, (num > 1) ? ((BlurOverallAmount * 4f - (float)Mathf.FloorToInt(BlurOverallAmount * 4f - 0.001f)) * 0.5f + 0.5f) : (BlurOverallAmount * 4f));
			m_material.SetFloat(ID_blurBaselineAmount, BlurBaselineAmount);
			m_material.SetVector(ID_blurRGBAChannels, new Vector4(BlurRedChannel, BlurGreenChannel, BlurBlueChannel, BlurAlphaChannel));
		}
		else
		{
			m_material.DisableKeyword(BlurKeyword);
		}
	}

	private void ResetBloom()
	{
		Bloom = false;
		BloomColor = BLOOM_COLOR_DEFAULT;
		BloomAmount = 1f;
		BloomDiffuse = 0.5f;
		BloomThreshold = 0.5f;
		BloomSoftness = 0.5f;
	}

	private void ApplyProfileBloom(MobilePostProcessingProfile profile)
	{
		if (profile.EnableBloomEffect)
		{
			Bloom = true;
			if (profile.OverrideBloomColor)
			{
				BloomColor = profile.BloomColor;
			}
			if (profile.OverrideBloomAmount)
			{
				BloomAmount = profile.BloomAmount;
			}
			if (profile.OverrideBloomDiffuse)
			{
				BloomDiffuse = profile.BloomDiffuse;
			}
			if (profile.OverrideBloomThreshold)
			{
				BloomThreshold = profile.BloomThreshold;
			}
			if (profile.OverrideBloomSoftness)
			{
				BloomSoftness = profile.BloomSoftness;
			}
		}
	}

	private void ApplyShaderBloom()
	{
		if (Bloom && BloomAmount > 0f)
		{
			m_material.EnableKeyword(BloomKeyword);
			m_material.SetColor(ID_bloomColor, BloomColor * BloomAmount);
			m_material.SetFloat(ID_bloomDiffuse, BloomDiffuse);
			int num = Mathf.Max(Mathf.CeilToInt(BloomDiffuse * 4f), 1);
			m_material.SetFloat(ID_bloomDiffuse, (num > 1) ? ((BloomDiffuse * 4f - (float)Mathf.FloorToInt(BloomDiffuse * 4f - 0.001f)) * 0.5f + 0.5f) : (BloomDiffuse * 4f));
			float num2 = BloomThreshold * BloomSoftness;
			m_material.SetVector(ID_bloomData, new Vector4(BloomThreshold, BloomThreshold - num2, 2f * num2, 1f / (4f * num2 + 1E-05f)));
		}
		else
		{
			m_material.DisableKeyword(BloomKeyword);
		}
	}

	private void ResetLut()
	{
		LUT = false;
		LutAmount = 0.5f;
		SourceLut = null;
	}

	private void ApplyProfileLut(MobilePostProcessingProfile profile)
	{
		if (profile.EnableLUTEffect)
		{
			LUT = true;
			if (profile.OverrideLutAmount)
			{
				LutAmount = profile.LutAmount;
			}
			if (profile.OverrideSourceLut)
			{
				SourceLut = profile.SourceLut;
			}
		}
	}

	private void ApplyShaderLut()
	{
		if (SourceLut != m_previousLut)
		{
			m_previousLut = SourceLut;
			if (SourceLut != null)
			{
				Convert3D(SourceLut);
			}
		}
		if (LUT)
		{
			m_material.EnableKeyword(LutKeyword);
			m_material.SetFloat(ID_lutAmount, LutAmount);
			m_material.SetTexture(ID_lutTexture, m_converted3D);
		}
		else
		{
			m_material.DisableKeyword(LutKeyword);
		}
	}

	private void Convert3D(Texture2D temp3DTex)
	{
		Color[] pixels = temp3DTex.GetPixels();
		Color[] array = new Color[pixels.Length];
		for (int i = 0; i < 16; i++)
		{
			for (int j = 0; j < 16; j++)
			{
				for (int k = 0; k < 16; k++)
				{
					int num = 16 - j - 1;
					array[i + j * 16 + k * 256] = pixels[k * 16 + i + num * 256];
				}
			}
		}
		m_converted3D.SetPixels(array);
		m_converted3D.Apply();
	}

	private void ResetImageFiltering()
	{
		ImageFiltering = false;
		Color = COLOR_DEFAULT;
		Contrast = 0f;
		Brightness = 0f;
		Saturation = 0f;
		Exposure = 0f;
		Gamma = 0f;
	}

	private void ApplyProfileImageFiltering(MobilePostProcessingProfile profile)
	{
		if (profile.EnableImageFilteringEffect)
		{
			ImageFiltering = true;
			if (profile.OverrideColor)
			{
				Color = profile.Color;
			}
			if (profile.OverrideContrast)
			{
				Contrast = profile.Contrast;
			}
			if (profile.OverrideBrightness)
			{
				Brightness = profile.Brightness;
			}
			if (profile.OverrideSaturation)
			{
				Saturation = profile.Saturation;
			}
			if (profile.OverrideExposure)
			{
				Exposure = profile.Exposure;
			}
			if (profile.OverrideGamma)
			{
				Gamma = profile.Gamma;
			}
		}
	}

	private void ApplyShaderImageFiltering()
	{
		if (ImageFiltering)
		{
			m_material.EnableKeyword(FilterKeyword);
			m_material.SetColor(ID_filterColor, (Mathf.Pow(2f, Exposure) - Gamma) * Color);
			m_material.SetFloat(ID_filterContrast, Contrast + 1f);
			m_material.SetFloat(ID_filterBrightness, Brightness * 0.5f - Contrast);
			m_material.SetFloat(ID_filterSaturation, Saturation + 1f);
		}
		else
		{
			m_material.DisableKeyword(FilterKeyword);
		}
	}

	private void ResetGradientMap()
	{
		GradientMap = false;
		GradientBase = GRADIENT_BASE_DEFAULT;
		GradientDark = GRADIENT_DARK_DEFAULT;
		GradientMid = GRADIENT_MID_DEFAULT;
		GradientLight = GRADIENT_LIGHT_DEFAULT;
		GradientAmount = 0.5f;
		DarkCutoff = 0f;
		LightCutoff = 1f;
	}

	private void ApplyProfileGradientMap(MobilePostProcessingProfile profile)
	{
		if (profile.EnableGradientMapEffect)
		{
			GradientMap = true;
			if (profile.OverrideGradientBase)
			{
				GradientBase = profile.GradientBase;
			}
			if (profile.OverrideGradientDark)
			{
				GradientDark = profile.GradientDark;
			}
			if (profile.OverrideGradientMid)
			{
				GradientMid = profile.GradientMid;
			}
			if (profile.OverrideGradientLight)
			{
				GradientLight = profile.GradientLight;
			}
			if (profile.OverrideGradientAmount)
			{
				GradientAmount = profile.GradientAmount;
			}
			if (profile.OverrideDarkCutoff)
			{
				DarkCutoff = profile.DarkCutoff;
			}
			if (profile.OverrideLightCutoff)
			{
				LightCutoff = profile.LightCutoff;
			}
		}
	}

	private void ApplyShaderGradientMap()
	{
		if (GradientMap)
		{
			m_material.EnableKeyword(GradientKeyword);
			m_material.SetColor(ID_gradientBase, GradientBase);
			m_material.SetColor(ID_gradientDark, GradientDark);
			m_material.SetColor(ID_gradientMid, GradientMid);
			m_material.SetColor(ID_gradientLight, GradientLight);
			m_material.SetFloat(ID_gradientAmount, GradientAmount);
			m_material.SetFloat(ID_gradientDarkCutoff, DarkCutoff);
			m_material.SetFloat(ID_gradientLightCutoff, LightCutoff);
		}
		else
		{
			m_material.DisableKeyword(GradientKeyword);
		}
	}

	private void ResetOverlayGradient()
	{
		OverlayGradient = false;
		OverlayTop = OVERLAY_TOP_DEFAULT;
		OverlayBottom = OVERLAY_BOTTOM_DEFAULT;
		OverlayAmount = 0.5f;
	}

	private void ApplyProfileOverlayGradient(MobilePostProcessingProfile profile)
	{
		if (profile.EnableOverlayGradientEffect)
		{
			OverlayGradient = true;
			if (profile.OverrideOverlayTop)
			{
				OverlayTop = profile.OverlayTop;
			}
			if (profile.OverrideOverlayBottom)
			{
				OverlayBottom = profile.OverlayBottom;
			}
			if (profile.OverrideOverlayAmount)
			{
				OverlayAmount = profile.OverlayAmount;
			}
		}
	}

	private void ApplyShaderOverlayGradient()
	{
		if (OverlayGradient)
		{
			m_material.EnableKeyword(OverlayKeyword);
			m_material.SetColor(ID_overlayTop, OverlayTop);
			m_material.SetColor(ID_overlayBottom, OverlayBottom);
			m_material.SetFloat(ID_overlayAmount, OverlayAmount);
		}
		else
		{
			m_material.DisableKeyword(OverlayKeyword);
		}
	}

	private void ResetClampBlack()
	{
		ClampBlack = false;
		ClampColor = CLAMP_COLOR_DEFAULT;
		ClampFillAmount = 0.5f;
	}

	private void ApplyProfileClampBlack(MobilePostProcessingProfile profile)
	{
		if (profile.EnableClampBlackEffect)
		{
			ClampBlack = true;
			if (profile.OverrideClampColor)
			{
				ClampColor = profile.ClampColor;
			}
			if (profile.OverrideClampFillAmount)
			{
				ClampFillAmount = profile.ClampFillAmount;
			}
		}
	}

	private void ApplyShaderClampBlack()
	{
		if (ClampBlack)
		{
			m_material.EnableKeyword(ClampKeyword);
			m_material.SetColor(ID_clampColor, ClampColor);
			m_material.SetFloat(ID_clampFillAmount, ClampFillAmount);
		}
		else
		{
			m_material.DisableKeyword(ClampKeyword);
		}
	}

	private void ResetTint()
	{
		Tint = false;
		TintColor = TINT_COLOR_DEFAULT;
		TintOverallAmount = 1f;
		TintBaselineAmount = 0f;
		TintRedChannel = 0f;
		TintGreenChannel = 0f;
		TintBlueChannel = 0f;
		TintAlphaChannel = 0f;
	}

	private void ApplyProfileTint(MobilePostProcessingProfile profile)
	{
		if (profile.EnableTintEffect)
		{
			Tint = true;
			if (profile.OverrideTintColor)
			{
				TintColor = profile.TintColor;
			}
			if (profile.OverrideTintOverallAmount)
			{
				TintOverallAmount = profile.TintOverallAmount;
			}
			if (profile.OverrideTintBaselineAmount)
			{
				TintBaselineAmount = profile.TintBaselineAmount;
			}
			if (profile.OverrideTintRedChannel)
			{
				TintRedChannel = profile.TintRedChannel;
			}
			if (profile.OverrideTintGreenChannel)
			{
				TintGreenChannel = profile.TintGreenChannel;
			}
			if (profile.OverrideTintBlueChannel)
			{
				TintBlueChannel = profile.TintBlueChannel;
			}
			if (profile.OverrideTintAlphaChannel)
			{
				TintAlphaChannel = profile.TintAlphaChannel;
			}
		}
	}

	private void ApplyShaderTint()
	{
		if (Tint)
		{
			m_material.EnableKeyword(TintKeyword);
			m_material.SetColor(ID_tintColor, TintColor);
			m_material.SetFloat(ID_tintOverallAmount, TintOverallAmount);
			m_material.SetFloat(ID_tintBaselineAmount, TintBaselineAmount);
			m_material.SetVector(ID_tintRGBAChannels, new Vector4(TintRedChannel, TintGreenChannel, TintBlueChannel, TintAlphaChannel));
		}
		else
		{
			m_material.DisableKeyword(TintKeyword);
		}
	}

	private void ResetChromaticAbberation()
	{
		ChromaticAberration = false;
		Offset = 1f;
		FishEyeDistortion = 0f;
	}

	private void ApplyProfileChromaticAbberation(MobilePostProcessingProfile profile)
	{
		if (profile.EnableChromaticAbberationEffect)
		{
			ChromaticAberration = true;
			if (profile.OverrideOffset)
			{
				Offset = profile.Offset;
			}
			if (profile.OverrideFishEyeDistortion)
			{
				FishEyeDistortion = profile.FishEyeDistortion;
			}
		}
	}

	private void ApplyShaderChromaticAbberation()
	{
		if (ChromaticAberration)
		{
			m_material.EnableKeyword(ChromaKeyword);
			m_material.SetFloat(ID_chromaOffset, 10f * Offset);
			m_material.SetFloat(ID_chromaFishEye, 0.1f * FishEyeDistortion);
		}
		else
		{
			m_material.DisableKeyword(ChromaKeyword);
		}
	}

	private void ResetLensDistortion()
	{
		Distortion = false;
		LensDistortion = 0.5f;
	}

	private void ApplyProfileDistortion(MobilePostProcessingProfile profile)
	{
		if (profile.EnableDistortionEffect)
		{
			Distortion = true;
			if (profile.OverrideLensDistortion)
			{
				LensDistortion = profile.LensDistortion;
			}
		}
	}

	private void ApplyShaderDistortion()
	{
		if (Distortion)
		{
			m_material.EnableKeyword(DistortionKeyword);
			m_material.SetFloat(ID_lensdistortion, 0f - LensDistortion);
		}
		else
		{
			m_material.DisableKeyword(DistortionKeyword);
		}
	}

	private void ResetVignette()
	{
		Vignette = false;
		VignetteCenter = VIGNETTE_CENTER_DEFAULT;
		VignetteColor = VIGNETTE_COLOR_DEFAULT;
		VignetteAmount = 0.5f;
		VignetteSoftness = 0.5f;
		VignetteRoundness = 0.5f;
	}

	private void ApplyProfileVignette(MobilePostProcessingProfile profile)
	{
		if (profile.EnableVignetteEffect)
		{
			Vignette = true;
			if (profile.OverrideVignetteCenter)
			{
				VignetteCenter = profile.VignetteCenter;
			}
			if (profile.OverrideVignetteColor)
			{
				VignetteColor = profile.VignetteColor;
			}
			if (profile.OverrideVignetteAmount)
			{
				VignetteAmount = profile.VignetteAmount;
			}
			if (profile.OverrideVignetteSoftness)
			{
				VignetteSoftness = profile.VignetteSoftness;
			}
			if (profile.OverrideVignetteRoundness)
			{
				VignetteRoundness = profile.VignetteRoundness;
			}
		}
	}

	private void ApplyShaderVignette()
	{
		if (Vignette)
		{
			m_material.EnableKeyword(VignetteKeyword);
			m_material.SetVector(ID_vignetteCenter, VignetteCenter);
			m_material.SetColor(ID_vignetteColor, VignetteColor);
			m_material.SetFloat(ID_vignetteAmount, 1f - VignetteAmount);
			m_material.SetFloat(ID_vignetteSoftness, 1f - VignetteSoftness - VignetteAmount);
			float value = 6f - VignetteRoundness * 4f;
			m_material.SetFloat(ID_vignetteRoundness, value);
		}
		else
		{
			m_material.DisableKeyword(VignetteKeyword);
		}
	}

	private void ResetMist()
	{
		Mist = false;
		MistColor = MIST_COLOR_DEFAULT;
		MistAmount = 0.1f;
	}

	private void ApplyProfileMist(MobilePostProcessingProfile profile)
	{
		if (profile.EnableMistEffect)
		{
			Mist = true;
			if (profile.OverrideMistColor)
			{
				MistColor = profile.MistColor;
			}
			if (profile.OverrideMistAmount)
			{
				MistAmount = profile.MistAmount;
			}
		}
	}

	private void ApplyShaderMist()
	{
		if (Mist)
		{
			m_material.EnableKeyword(MistKeyword);
			m_material.SetColor(ID_mistColor, MistColor);
			m_material.SetFloat(ID_mistAmount, MistAmount * 10f);
		}
		else
		{
			m_material.DisableKeyword(MistKeyword);
		}
	}

	private void ResetPixelation()
	{
		Pixelation = false;
		PixelResolution = PIXEL_RESOLUTION_DEFAULT;
		PixelBaselineAmount = 0f;
		PixelRedChannel = 0f;
		PixelGreenChannel = 0f;
		PixelBlueChannel = 0f;
		PixelAlphaChannel = 0f;
	}

	private void ApplyProfilePixelation(MobilePostProcessingProfile profile)
	{
		if (profile.EnablePixelationEffect)
		{
			Pixelation = true;
			if (profile.OverridePixelResolution)
			{
				PixelResolution = profile.PixelResolution;
			}
			if (profile.OverridePixelBaselineAmount)
			{
				PixelBaselineAmount = profile.PixelBaselineAmount;
			}
			if (profile.OverridePixelRedChannel)
			{
				PixelRedChannel = profile.PixelRedChannel;
			}
			if (profile.OverridePixelGreenChannel)
			{
				PixelGreenChannel = profile.PixelGreenChannel;
			}
			if (profile.OverridePixelBlueChannel)
			{
				PixelBlueChannel = profile.PixelBlueChannel;
			}
			if (profile.OverridePixelAlphaChannel)
			{
				PixelAlphaChannel = profile.PixelAlphaChannel;
			}
		}
	}

	private void ApplyShaderPixelation()
	{
		if (Pixelation)
		{
			m_material.SetVector(ID_pixelResolution, PixelResolution);
			m_material.SetFloat(ID_pixelBaselineAmount, PixelBaselineAmount);
			m_material.SetVector(ID_pixelRGBAChannels, new Vector4(PixelRedChannel, PixelGreenChannel, PixelBlueChannel, PixelAlphaChannel));
		}
	}

	private void ResetDither()
	{
		Dither = false;
		DitherDark = DITHER_DARK_DEFAULT;
		DitherLight = DITHER_LIGHT_DEFAULT;
	}

	private void ApplyProfileDither(MobilePostProcessingProfile profile)
	{
		if (profile.EnableDitherEffect)
		{
			Dither = true;
			if (profile.OverrideDitherDark)
			{
				DitherDark = profile.DitherDark;
			}
			if (profile.OverrideDitherLight)
			{
				DitherLight = profile.DitherLight;
			}
		}
	}

	private void ApplyShaderDither()
	{
		if (Dither)
		{
			m_material.EnableKeyword(DitherKeyword);
			m_material.SetColor(ID_ditherDark, DitherDark);
			m_material.SetColor(ID_ditherLight, DitherLight);
		}
		else
		{
			m_material.DisableKeyword(DitherKeyword);
		}
	}

	private void ResetScreenFlip()
	{
		ScreenFlipX = false;
		ScreenFlipY = false;
	}

	private void ApplyProfileScreenFlip(MobilePostProcessingProfile profile)
	{
		if (profile.OverrideScreenFlipXEffect)
		{
			ScreenFlipX = profile.ScreenFlipX;
		}
		if (profile.OverrideScreenFlipYEffect)
		{
			ScreenFlipY = profile.ScreenFlipY;
		}
	}

	private void ApplyShaderScreenFlip()
	{
		if (ScreenFlipX)
		{
			m_material.EnableKeyword(ScreenFlipXKeyword);
		}
		else
		{
			m_material.DisableKeyword(ScreenFlipXKeyword);
		}
		if (ScreenFlipY)
		{
			m_material.EnableKeyword(ScreenFlipYKeyword);
		}
		else
		{
			m_material.DisableKeyword(ScreenFlipYKeyword);
		}
	}

	private void ResetAnimatedOverlay()
	{
		AnimatedOverlay = false;
		AnimOverlayTexture = ANIMOVERLAY_TEXTURE_DEFAULT;
		AnimOverlaySpritesheetSize = ANIMOVERLAY_SPRITESHEET_SIZE_DEFAULT;
		AnimOverlayFramerate = 30;
		AnimOverlayStrength = 0.5f;
	}

	private void ApplyProfileAnimatedOverlay(MobilePostProcessingProfile profile)
	{
		if (profile.EnableAnimatedOverlayEffect)
		{
			AnimatedOverlay = true;
			if (profile.OverrideAnimOverlayTexture)
			{
				AnimOverlayTexture = profile.AnimOverlayTexture;
			}
			if (profile.OverrideAnimOverlaySpritesheetSize)
			{
				AnimOverlaySpritesheetSize = profile.AnimOverlaySpritesheetSize;
			}
			if (profile.OverrideAnimOverlayFramerate)
			{
				AnimOverlayFramerate = profile.AnimOverlayFramerate;
			}
			if (profile.OverrideAnimOverlayStrength)
			{
				AnimOverlayStrength = profile.AnimOverlayStrength;
			}
		}
	}

	private void ApplyShaderAnimatedOverlay()
	{
		if (AnimatedOverlay)
		{
			m_material.EnableKeyword(AnimOverlayKeyword);
			m_material.SetTexture(ID_animOverlayTexture, AnimOverlayTexture);
			m_material.SetVector(ID_animOverlaySpritesheetSize, AnimOverlaySpritesheetSize);
			m_material.SetFloat(ID_animOverlayFramerate, AnimOverlayFramerate);
			m_material.SetFloat(ID_animOverlayStrength, AnimOverlayStrength);
		}
		else
		{
			m_material.DisableKeyword(AnimOverlayKeyword);
		}
	}

	private void ResetCircularDarkness()
	{
		CircularDarkness = false;
		CircDarknessColor = CIRCDARKNESS_COLOR_DEFAULT;
		CircDarknessAmount = 0.5f;
		CircDarknessSoftness = 0.5f;
	}

	private void ApplyProfileCircularDarkness(MobilePostProcessingProfile profile)
	{
		if (profile.EnableCircularDarknessEffect)
		{
			CircularDarkness = true;
			if (profile.OverrideCircDarknessColor)
			{
				CircDarknessColor = profile.CircDarknessColor;
			}
			if (profile.OverrideCircDarknessAmount)
			{
				CircDarknessAmount = profile.CircDarknessAmount;
			}
			if (profile.OverrideCircDarknessSoftness)
			{
				CircDarknessSoftness = profile.CircDarknessSoftness;
			}
		}
	}

	private void ApplyShaderCircularDarkness()
	{
		if (CircularDarkness)
		{
			m_material.EnableKeyword(CircDarknessKeyword);
			m_material.SetColor(ID_circDarknessColor, CircDarknessColor);
			m_material.SetFloat(ID_circDarknessAmount, 1f - CircDarknessAmount);
			m_material.SetFloat(ID_circDarknessSoftness, 1f - CircDarknessSoftness - CircDarknessAmount);
		}
		else
		{
			m_material.DisableKeyword(CircDarknessKeyword);
		}
	}

	public void Start()
	{
		if (m_material == null)
		{
			m_material = new Material(Shader.Find("SupGames/Mobile/PostProcess"));
		}
		if (Mask == null)
		{
			Shader.SetGlobalTexture(ID_maskTexture, Texture2D.whiteTexture);
		}
		else
		{
			Shader.SetGlobalTexture(ID_maskTexture, Mask);
		}
		m_converted3D = new Texture3D(16, 16, 16, TextureFormat.ARGB32, mipChain: false);
		m_converted3D.wrapMode = TextureWrapMode.Clamp;
	}

	public void LateUpdate()
	{
		if (baseProfile != m_previousProfile)
		{
			m_isDirty = true;
		}
		if (m_isDirty)
		{
			ApplyEffects();
			m_isDirty = false;
		}
		if (CircularDarkness && PlayerManager.IsInstantiated && !PlayerManager.IsDisposed && (bool)CameraController.ForegroundPerspCam)
		{
			PlayerController playerController = PlayerManager.GetPlayerController();
			Vector2 vector = CameraController.ForegroundPerspCam.WorldToViewportPoint(playerController.Midpoint);
			m_material.SetVector(ID_circDarknessCenter, vector);
		}
	}

	private void OnDestroy()
	{
		if (m_converted3D != null)
		{
			Object.DestroyImmediate(m_converted3D);
		}
		m_converted3D = null;
	}

	public void AddTraitOverride(MobilePostProcessingProfile overrideProfile)
	{
		m_traitOverrides.Add(overrideProfile);
		m_isDirty = true;
	}

	public void RemoveTraitOverride(MobilePostProcessingProfile overrideProfile)
	{
		m_traitOverrides.Remove(overrideProfile);
		m_isDirty = true;
	}

	public void AddDimensionOverride(MobilePostProcessingProfile dimensionProfile)
	{
		m_dimensionOverrides.Add(dimensionProfile);
		m_isDirty = true;
	}

	public void RemoveDimensionOverride(MobilePostProcessingProfile dimensionProfile)
	{
		m_dimensionOverrides.Remove(dimensionProfile);
		m_isDirty = true;
	}

	public void SetBaseProfile(MobilePostProcessingProfile profile)
	{
		StopLerpToProfile();
		baseProfile = profile;
		m_isDirty = true;
	}

	public void ForceDirty()
	{
		m_isDirty = true;
	}

	public void LerpToProfile(MobilePostProcessingProfile profile, float lerpSpeed)
	{
		StopLerpToProfile();
		if ((bool)baseProfile)
		{
			m_lerpToProfileCoroutine = StartCoroutine(LerpToProfileCoroutine(profile, lerpSpeed));
		}
	}

	private IEnumerator LerpToProfileCoroutine(MobilePostProcessingProfile profile, float lerpSpeed)
	{
		if (baseProfile.EnableBlurEffect && profile.EnableBlurEffect)
		{
			m_tweenArray[0] = TweenManager.TweenTo(this, lerpSpeed, Ease.None, "BlurOverallAmount", profile.BlurOverallAmount, "BlurBaselineAmount", profile.BlurBaselineAmount, "BlurRedChannel", profile.BlurRedChannel, "BlurGreenChannel", profile.BlurGreenChannel, "BlurBlueChannel", profile.BlurBlueChannel, "BlurAlphaChannel", profile.BlurAlphaChannel);
		}
		if (baseProfile.EnableBloomEffect && profile.EnableBloomEffect)
		{
			m_tweenArray[1] = TweenManager.TweenTo(this, lerpSpeed, Ease.None, "BloomAmount", profile.BloomAmount, "BloomDiffuse", profile.BloomDiffuse, "BloomThreshold", profile.BloomThreshold, "BloomSoftness", profile.BloomSoftness, "BloomColor.r", profile.BloomColor.r, "BloomColor.g", profile.BloomColor.g, "BloomColor.b", profile.BloomColor.b, "BloomColor.a", profile.BloomColor.a);
		}
		if (baseProfile.EnableImageFilteringEffect && profile.EnableImageFilteringEffect)
		{
			m_tweenArray[2] = TweenManager.TweenTo(this, lerpSpeed, Ease.None, "Color.r", profile.Color.r, "Color.g", profile.Color.g, "Color.b", profile.Color.b, "Color.a", profile.Color.a, "Contrast", profile.Contrast, "Brightness", profile.Brightness, "Saturation", profile.Saturation, "Exposure", profile.Exposure, "Gamma", profile.Gamma);
		}
		if (baseProfile.EnableGradientMapEffect && profile.EnableGradientMapEffect)
		{
			m_tweenArray[3] = TweenManager.TweenTo(this, lerpSpeed, Ease.None, "GradientBase.r", profile.GradientBase.r, "GradientBase.g", profile.GradientBase.g, "GradientBase.b", profile.GradientBase.b, "GradientBase.a", profile.GradientBase.a, "GradientDark.r", profile.GradientDark.r, "GradientDark.g", profile.GradientDark.g, "GradientDark.b", profile.GradientDark.b, "GradientDark.a", profile.GradientDark.a, "GradientMid.r", profile.GradientMid.r, "GradientMid.g", profile.GradientMid.g);
			m_tweenArray[4] = TweenManager.TweenTo(this, lerpSpeed, Ease.None, "GradientMid.b", profile.GradientMid.b, "GradientMid.a", profile.GradientMid.a, "GradientLight.r", profile.GradientLight.r, "GradientLight.g", profile.GradientLight.g, "GradientLight.b", profile.GradientLight.b, "GradientLight.a", profile.GradientLight.a, "GradientAmount", profile.GradientAmount, "DarkCutoff", profile.DarkCutoff, "LightCutoff", profile.LightCutoff);
		}
		if (baseProfile.EnableOverlayGradientEffect && profile.EnableOverlayGradientEffect)
		{
			m_tweenArray[5] = TweenManager.TweenTo(this, lerpSpeed, Ease.None, "OverlayTop.r", profile.OverlayTop.r, "OverlayTop.g", profile.OverlayTop.g, "OverlayTop.b", profile.OverlayTop.b, "OverlayTop.a", profile.OverlayTop.a, "OverlayBottom.r", profile.OverlayBottom.r, "OverlayBottom.g", profile.OverlayBottom.g, "OverlayBottom.b", profile.OverlayBottom.b, "OverlayBottom.a", profile.OverlayBottom.a, "OverlayAmount", profile.OverlayAmount);
		}
		if (baseProfile.EnableClampBlackEffect && profile.EnableClampBlackEffect)
		{
			m_tweenArray[6] = TweenManager.TweenTo(this, lerpSpeed, Ease.None, "ClampColor.r", profile.ClampColor.r, "ClampColor.g", profile.ClampColor.g, "ClampColor.b", profile.ClampColor.b, "ClampColor.a", profile.ClampColor.a, "ClampFillAmount", profile.ClampFillAmount);
		}
		if (baseProfile.EnableTintEffect && profile.EnableTintEffect)
		{
			m_tweenArray[7] = TweenManager.TweenTo(this, lerpSpeed, Ease.None, "TintColor.r", profile.TintColor.r, "TintColor.g", profile.TintColor.g, "TintColor.b", profile.TintColor.b, "TintColor.a", profile.TintColor.a, "TintOverallAmount", profile.TintOverallAmount, "TintBaselineAmount", profile.TintBaselineAmount, "TintRedChannel", profile.TintRedChannel, "TintGreenChannel", profile.TintGreenChannel, "TintBlueChannel", profile.TintBlueChannel, "TintAlphaChannel", profile.TintAlphaChannel);
		}
		if (baseProfile.EnableChromaticAbberationEffect && profile.EnableChromaticAbberationEffect)
		{
			m_tweenArray[8] = TweenManager.TweenTo(this, lerpSpeed, Ease.None, "Offset", profile.Offset, "FishEyeDistortion", profile.FishEyeDistortion);
		}
		if (baseProfile.EnableDistortionEffect && profile.EnableDistortionEffect)
		{
			m_tweenArray[9] = TweenManager.TweenTo(this, lerpSpeed, Ease.None, "LensDistortion", profile.LensDistortion);
		}
		if (baseProfile.EnableVignetteEffect && profile.EnableVignetteEffect)
		{
			m_tweenArray[10] = TweenManager.TweenTo(this, lerpSpeed, Ease.None, "VignetteCenter.x", profile.VignetteCenter.x, "VignetteCenter.y", profile.VignetteCenter.y, "VignetteColor.r", profile.VignetteColor.r, "VignetteColor.g", profile.VignetteColor.g, "VignetteColor.b", profile.VignetteColor.b, "VignetteColor.a", profile.VignetteColor.a, "VignetteAmount", profile.VignetteAmount, "VignetteSoftness", profile.VignetteSoftness, "VignetteRoundness", profile.VignetteRoundness);
		}
		if (baseProfile.EnableMistEffect && profile.EnableMistEffect)
		{
			m_tweenArray[11] = TweenManager.TweenTo(this, lerpSpeed, Ease.None, "MistColor.r", profile.MistColor.r, "MistColor.g", profile.MistColor.g, "MistColor.b", profile.MistColor.b, "MistColor.a", profile.MistColor.a, "MistAmount", profile.MistAmount);
		}
		if (baseProfile.EnablePixelationEffect && profile.EnablePixelationEffect)
		{
			m_tweenArray[12] = TweenManager.TweenTo(this, lerpSpeed, Ease.None, "PixelResolution.x", profile.PixelResolution.x, "PixelResolution.y", profile.PixelResolution.y, "PixelBaselineAmount", profile.PixelBaselineAmount, "PixelRedChannel", profile.PixelRedChannel, "PixelGreenChannel", profile.PixelGreenChannel, "PixelBlueChannel", profile.PixelBlueChannel, "PixelAlphaChannel", profile.PixelAlphaChannel);
		}
		if (baseProfile.EnableDitherEffect && profile.EnableDitherEffect)
		{
			m_tweenArray[13] = TweenManager.TweenTo(this, lerpSpeed, Ease.None, "DitherDark.r", profile.DitherDark.r, "DitherDark.g", profile.DitherDark.g, "DitherDark.b", profile.DitherDark.b, "DitherDark.a", profile.DitherDark.a, "DitherLight.r", profile.DitherLight.r, "DitherLight.g", profile.DitherLight.g, "DitherLight.b", profile.DitherLight.b, "DitherLight.a", profile.DitherLight.a);
		}
		if (baseProfile.EnableCircularDarknessEffect && profile.EnableCircularDarknessEffect)
		{
			m_tweenArray[14] = TweenManager.TweenTo(this, lerpSpeed, Ease.None, "CircDarknessColor.r", profile.CircDarknessColor.r, "CircDarknessColor.g", profile.CircDarknessColor.g, "CircDarknessColor.b", profile.CircDarknessColor.b, "CircDarknessColor.a", profile.CircDarknessColor.a, "CircDarknessAmount", profile.CircDarknessAmount, "CircDarknessSoftness", profile.CircDarknessSoftness);
		}
		float duration = Time.time + lerpSpeed;
		while (Time.time < duration)
		{
			ApplyShader();
			yield return null;
		}
		m_lerpToProfileCoroutine = null;
	}

	public void StopLerpToProfile()
	{
		if (m_lerpToProfileCoroutine != null)
		{
			StopCoroutine(m_lerpToProfileCoroutine);
			for (int i = 0; i < m_tweenArray.Length; i++)
			{
				Tween tween = m_tweenArray[i];
				if ((bool)tween)
				{
					tween.StopTweenWithConditionChecks(runEndHandlers: false, baseProfile);
					m_tweenArray[i] = null;
				}
			}
		}
		m_lerpToProfileCoroutine = null;
	}

	private void ApplyEffects()
	{
		if (baseProfile == null)
		{
			Debug.Log("<color=red>No base MobilePostProcessingProfile is set!</color>");
			return;
		}
		m_previousProfile = baseProfile;
		ResetToDefaults();
		ApplyProfile(baseProfile);
		for (int i = 0; i < m_dimensionOverrides.Count; i++)
		{
			ApplyProfile(m_dimensionOverrides[i]);
		}
		for (int j = 0; j < m_traitOverrides.Count; j++)
		{
			ApplyProfile(m_traitOverrides[j]);
		}
		ApplyShader();
	}

	private void ResetToDefaults()
	{
		ResetBlur();
		ResetBloom();
		ResetLut();
		ResetImageFiltering();
		ResetGradientMap();
		ResetOverlayGradient();
		ResetClampBlack();
		ResetTint();
		ResetChromaticAbberation();
		ResetLensDistortion();
		ResetVignette();
		ResetMist();
		ResetPixelation();
		ResetDither();
		ResetScreenFlip();
		ResetAnimatedOverlay();
		ResetCircularDarkness();
	}

	private void ApplyProfile(MobilePostProcessingProfile profile)
	{
		ApplyProfileBlur(profile);
		ApplyProfileBloom(profile);
		ApplyProfileLut(profile);
		ApplyProfileImageFiltering(profile);
		ApplyProfileGradientMap(profile);
		ApplyProfileOverlayGradient(profile);
		ApplyProfileClampBlack(profile);
		ApplyProfileTint(profile);
		ApplyProfileChromaticAbberation(profile);
		ApplyProfileDistortion(profile);
		ApplyProfileVignette(profile);
		ApplyProfileMist(profile);
		ApplyProfilePixelation(profile);
		ApplyProfileDither(profile);
		ApplyProfileScreenFlip(profile);
		ApplyProfileAnimatedOverlay(profile);
		ApplyProfileCircularDarkness(profile);
	}

	public void ApplyShader()
	{
		ApplyShaderBlur();
		ApplyShaderBloom();
		ApplyShaderLut();
		ApplyShaderImageFiltering();
		ApplyShaderGradientMap();
		ApplyShaderOverlayGradient();
		ApplyShaderClampBlack();
		ApplyShaderTint();
		ApplyShaderChromaticAbberation();
		ApplyShaderDistortion();
		ApplyShaderVignette();
		ApplyShaderMist();
		ApplyShaderPixelation();
		ApplyShaderDither();
		ApplyShaderScreenFlip();
		ApplyShaderAnimatedOverlay();
		ApplyShaderCircularDarkness();
	}

	private void OnRenderImage(RenderTexture source, RenderTexture destination)
	{
		RenderTexture renderTexture = null;
		int num = 0;
		if (Bloom && BloomAmount > 0f)
		{
			num = Mathf.Max(Mathf.CeilToInt(BloomDiffuse * 4f), 1);
		}
		if (Blur && BlurOverallAmount > 0f)
		{
			num = Mathf.Max(Mathf.CeilToInt(BlurOverallAmount * 4f), 1);
		}
		if (num > 0)
		{
			switch (num)
			{
			case 1:
				renderTexture = RenderTexture.GetTemporary(Screen.width / 2, Screen.height / 2, 0, source.format);
				Graphics.Blit(source, renderTexture, m_material, 0);
				break;
			case 2:
			{
				renderTexture = RenderTexture.GetTemporary(Screen.width / 2, Screen.height / 2, 0, source.format);
				RenderTexture temporary4 = RenderTexture.GetTemporary(Screen.width / 4, Screen.height / 4, 0, source.format);
				Graphics.Blit(source, temporary4);
				Graphics.Blit(temporary4, renderTexture, m_material, 0);
				RenderTexture.ReleaseTemporary(temporary4);
				break;
			}
			case 3:
			{
				renderTexture = RenderTexture.GetTemporary(Screen.width / 4, Screen.height / 4, 0, source.format);
				RenderTexture temporary3 = RenderTexture.GetTemporary(Screen.width / 8, Screen.height / 8, 0, source.format);
				Graphics.Blit(source, renderTexture);
				Graphics.Blit(renderTexture, temporary3);
				Graphics.Blit(temporary3, renderTexture, m_material, 0);
				RenderTexture.ReleaseTemporary(temporary3);
				break;
			}
			case 4:
			{
				renderTexture = RenderTexture.GetTemporary(Screen.width / 4, Screen.height / 4, 0, source.format);
				RenderTexture temporary = RenderTexture.GetTemporary(Screen.width / 8, Screen.height / 8, 0, source.format);
				RenderTexture temporary2 = RenderTexture.GetTemporary(Screen.width / 16, Screen.height / 16, 0, source.format);
				Graphics.Blit(source, renderTexture);
				Graphics.Blit(renderTexture, temporary);
				Graphics.Blit(temporary, temporary2);
				Graphics.Blit(temporary2, temporary, m_material, 0);
				Graphics.Blit(temporary, renderTexture, m_material, 0);
				RenderTexture.ReleaseTemporary(temporary);
				RenderTexture.ReleaseTemporary(temporary2);
				break;
			}
			}
			m_material.SetTexture(ID_blurTex, renderTexture);
		}
		if (Pixelation)
		{
			RenderTexture temporary5 = RenderTexture.GetTemporary(Screen.width, Screen.height, 0, source.format);
			Graphics.Blit(source, temporary5, m_material, 1);
			Graphics.Blit(temporary5, destination, m_material, 2);
			RenderTexture.ReleaseTemporary(temporary5);
		}
		else
		{
			Graphics.Blit(source, destination, m_material, 1);
		}
		if (renderTexture != null)
		{
			RenderTexture.ReleaseTemporary(renderTexture);
		}
	}
}
