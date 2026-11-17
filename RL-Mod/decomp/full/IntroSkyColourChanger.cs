using UnityEngine;
using UnityEngine.Serialization;

public class IntroSkyColourChanger : MonoBehaviour
{
	[SerializeField]
	private PatchType m_patchTypeToUse;

	[SerializeField]
	private PatchTypeSkyColorEntryDictionary m_skyColorTable;

	[Space(10f)]
	[SerializeField]
	private Renderer m_skyRenderer;

	[SerializeField]
	[FormerlySerializedAs("m_fogRenderer")]
	private Renderer m_mistRenderer;

	[SerializeField]
	private ParticleSystem m_beam1ParticleSystem;

	[SerializeField]
	private ParticleSystem m_beam2ParticleSystem;

	[SerializeField]
	private ParticleSystem m_rubbleParticleSystem;

	[SerializeField]
	private SpriteRenderer m_glowRenderer;

	[SerializeField]
	private MobilePostProcessing m_postProcessing;

	[SerializeField]
	private SpriteRenderer m_logoBannerRenderer;

	private MaterialPropertyBlock m_matBlock;

	public PatchType PatchTypeToUse
	{
		get
		{
			if (HolidayLookController.IsHoliday(HolidayType.Halloween))
			{
				return PatchType.HallowsEve;
			}
			if (HolidayLookController.IsHoliday(HolidayType.Christmas))
			{
				return PatchType.WinterWonderland;
			}
			return m_patchTypeToUse;
		}
	}

	private void Awake()
	{
		m_matBlock = new MaterialPropertyBlock();
		SetSkyColor(PatchTypeToUse);
	}

	public void SetSkyColor(PatchType patchTypeToUse)
	{
		if (m_skyColorTable.TryGetValue(patchTypeToUse, out var value))
		{
			m_skyRenderer.GetPropertyBlock(m_matBlock);
			m_matBlock.SetColor("_SkyColor", value.SkyColor);
			m_matBlock.SetColor("_HorizonColor", value.HorizonColor);
			m_matBlock.SetColor("_CloudMainColor", value.CloudMainColor);
			m_matBlock.SetColor("_CloudHighlightColor", value.CloudHighlightColor);
			m_matBlock.SetColor("_StarColor", value.StarColor);
			m_matBlock.SetColor("_RightColor", value.RightColor);
			m_matBlock.SetColor("_BottomColor", value.BottomColor);
			m_skyRenderer.SetPropertyBlock(m_matBlock);
			m_mistRenderer.GetPropertyBlock(m_matBlock);
			m_matBlock.SetColor("_MistColor", value.MistColor);
			m_mistRenderer.SetPropertyBlock(m_matBlock);
			ParticleSystem.MainModule main = m_beam1ParticleSystem.main;
			main.startColor = value.Beam1Color;
			main = m_beam2ParticleSystem.main;
			main.startColor = value.Beam2Color;
			main = m_rubbleParticleSystem.main;
			ParticleSystem.MinMaxGradient startColor = main.startColor;
			startColor.colorMin = value.RubbleColor1;
			startColor.colorMax = value.RubbleColor2;
			main.startColor = startColor;
			m_glowRenderer.color = value.GlowColor;
			m_postProcessing.SetBaseProfile(value.PostProcessProfile);
			RenderSettings.fogColor = value.FogRenderColor;
			m_logoBannerRenderer.sprite = value.BannerSprite;
		}
		else
		{
			Debug.Log("<color=yellow>WARNING: Failed to change intro sky colour. Patch Type: " + patchTypeToUse.ToString() + " not defined in sky color table.</color>");
		}
	}
}
