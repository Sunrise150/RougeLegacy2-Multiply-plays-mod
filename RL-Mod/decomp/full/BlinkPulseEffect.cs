using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BlinkPulseEffect : MonoBehaviour
{
	[SerializeField]
	private float m_invincibilityPulseRate = 0.1f;

	[SerializeField]
	private Color m_invincibilityTint = new Color(Color.black.r, Color.black.g, Color.black.b, 0.8f);

	[SerializeField]
	private float m_singleBlinkDuration = 0.1f;

	[SerializeField]
	private Color m_blinkOnHitTint = new Color(1f, 1f, 1f, 0.8f);

	[SerializeField]
	private List<Renderer> m_rendererArray;

	private bool m_invincibilityEffectOn;

	private WaitRL_Yield m_blinkWaitYield;

	private WaitRL_Yield m_invincibilityWaitYield;

	private Coroutine m_blinkCoroutine;

	private Coroutine m_invincibilityCoroutine;

	private bool m_isInitialized;

	private BaseCharacterController m_charController;

	private bool m_useOverrideDefaultTint;

	private Color m_defaultTintOverrideColor;

	private static MaterialPropertyBlock m_matPropertyBlock_STATIC;

	private Coroutine m_blackFillFadeCoroutine;

	private bool m_isFadedIn;

	private bool m_isFadedOut = true;

	private Dictionary<BlackFillType, bool> m_blackFillEffectTable = new Dictionary<BlackFillType, bool>();

	private List<RendererArrayEntry> m_rendererArrayDefaultTint;

	public Color BlinkOnHitTint
	{
		get
		{
			return m_blinkOnHitTint;
		}
		set
		{
			m_blinkOnHitTint = value;
		}
	}

	public bool UseOverrideDefaultTint
	{
		get
		{
			if ((bool)m_charController)
			{
				return m_charController.UseOverrideDefaultTint;
			}
			return m_useOverrideDefaultTint;
		}
		set
		{
			if ((bool)m_charController)
			{
				m_charController.UseOverrideDefaultTint = value;
			}
			else
			{
				m_useOverrideDefaultTint = value;
			}
		}
	}

	public Color DefaultTintOverrideColor
	{
		get
		{
			if ((bool)m_charController)
			{
				return m_charController.DefaultTintOverrideColor;
			}
			return m_defaultTintOverrideColor;
		}
		set
		{
			if ((bool)m_charController)
			{
				m_charController.DefaultTintOverrideColor = value;
			}
			else
			{
				m_defaultTintOverrideColor = value;
			}
		}
	}

	public bool UseUnscaledTime { get; set; }

	public float SingleBlinkDuration
	{
		get
		{
			return m_singleBlinkDuration;
		}
		set
		{
			m_singleBlinkDuration = value;
		}
	}

	public List<RendererArrayEntry> RendererArrayDefaultTint
	{
		get
		{
			if ((bool)m_charController && (m_rendererArray == null || m_rendererArray.Count == 0))
			{
				return m_charController.RendererArrayDefaultTint;
			}
			return m_rendererArrayDefaultTint;
		}
	}

	public List<Renderer> RendererArray
	{
		get
		{
			if ((bool)m_charController && (m_rendererArray == null || m_rendererArray.Count == 0))
			{
				return m_charController.RendererArray;
			}
			return m_rendererArray;
		}
	}

	private void Awake()
	{
		m_blinkWaitYield = new WaitRL_Yield(m_singleBlinkDuration);
		m_invincibilityWaitYield = new WaitRL_Yield(m_invincibilityPulseRate);
		if (m_matPropertyBlock_STATIC == null)
		{
			m_matPropertyBlock_STATIC = new MaterialPropertyBlock();
		}
		GameObject root = this.GetRoot();
		if (m_rendererArray == null || m_rendererArray.Count == 0)
		{
			m_charController = root.GetComponent<BaseCharacterController>();
			if (!m_charController)
			{
				root.GetComponentsInChildren(includeInactive: true, m_rendererArray);
			}
		}
	}

	private void OnEnable()
	{
		if (!m_isInitialized)
		{
			Initialize();
		}
	}

	private void Initialize()
	{
		if (!m_charController)
		{
			CameraLayerController component = GetComponent<CameraLayerController>();
			if ((bool)component && component.IsSet && component.IsProp && component.CameraLayer != CameraLayer.Game)
			{
				return;
			}
			m_rendererArrayDefaultTint = new List<RendererArrayEntry>();
			for (int i = 0; i < m_rendererArray.Count; i++)
			{
				RendererArrayEntry item = default(RendererArrayEntry);
				Material sharedMaterial = RendererArray[i].sharedMaterial;
				item.HasAlphaColor = sharedMaterial.HasProperty(ShaderID_RL._AlphaBlendColor);
				item.HasRimColor = sharedMaterial.HasProperty(ShaderID_RL._RimLightColor);
				item.HasRimBias = sharedMaterial.HasProperty(ShaderID_RL._RimBias);
				item.HasRimScale = sharedMaterial.HasProperty(ShaderID_RL._RimScale);
				if (item.HasAlphaColor)
				{
					item.DefaultAlphaColor = sharedMaterial.GetColor(ShaderID_RL._AlphaBlendColor);
				}
				if (item.HasRimColor)
				{
					item.DefaultRimLightColor = sharedMaterial.GetColor(ShaderID_RL._RimLightColor);
				}
				if (item.HasRimBias)
				{
					item.DefaultRimBias = sharedMaterial.GetFloat(ShaderID_RL._RimBias);
				}
				if (item.HasRimScale)
				{
					item.DefaultRimScale = sharedMaterial.GetFloat(ShaderID_RL._RimScale);
				}
				m_rendererArrayDefaultTint.Add(item);
			}
		}
		m_isInitialized = true;
	}

	public void ActivateBlackFill(BlackFillType fillType, float fadeInDuration)
	{
		if ((bool)m_charController && fillType == BlackFillType.EnemiesBlackFill_Trait)
		{
			EnemyController enemyController = m_charController as EnemyController;
			if ((bool)enemyController && Trait_EV.TRAIT_EFFECT_EXCEPTION_ARRAY.Contains(enemyController.EnemyType))
			{
				return;
			}
		}
		if (m_blackFillEffectTable.ContainsKey(fillType))
		{
			m_blackFillEffectTable[fillType] = true;
		}
		else
		{
			m_blackFillEffectTable.Add(fillType, value: true);
		}
		if (m_isFadedOut)
		{
			if (m_blackFillFadeCoroutine != null)
			{
				StopCoroutine(m_blackFillFadeCoroutine);
			}
			if (fadeInDuration > 0f)
			{
				m_blackFillFadeCoroutine = StartCoroutine(BlackFillFadeCoroutine(fadeInDuration, fadeIn: true));
				return;
			}
			Color black = Color.black;
			UseOverrideDefaultTint = true;
			m_isFadedIn = true;
			m_isFadedOut = false;
			DefaultTintOverrideColor = black;
			SetRendererArrayColor(black);
		}
	}

	public void DisableBlackFill(BlackFillType fillType, float fadeOutDuration)
	{
		if (m_blackFillEffectTable.ContainsKey(fillType))
		{
			m_blackFillEffectTable[fillType] = false;
		}
		else
		{
			m_blackFillEffectTable.Add(fillType, value: false);
		}
		bool flag = true;
		foreach (KeyValuePair<BlackFillType, bool> item in m_blackFillEffectTable)
		{
			if (item.Value)
			{
				flag = false;
				break;
			}
		}
		if (flag && m_isFadedIn)
		{
			if (m_blackFillFadeCoroutine != null)
			{
				StopCoroutine(m_blackFillFadeCoroutine);
			}
			if (fadeOutDuration > 0f)
			{
				m_blackFillFadeCoroutine = StartCoroutine(BlackFillFadeCoroutine(fadeOutDuration, fadeIn: false));
			}
			else
			{
				ResetBlackFillState();
			}
		}
	}

	private IEnumerator BlackFillFadeCoroutine(float duration, bool fadeIn)
	{
		while (!m_isInitialized)
		{
			yield return null;
		}
		if (fadeIn)
		{
			m_isFadedIn = true;
			m_isFadedOut = false;
		}
		else
		{
			m_isFadedIn = false;
			m_isFadedOut = true;
		}
		float startTime = Time.time;
		float time = Time.time;
		Color fadeColor = Color.black;
		while (time < duration + startTime)
		{
			float lerpAmount = (time - startTime) / duration;
			if (fadeIn)
			{
				SetRendererArrayColorLerpFromDefault(fadeColor, lerpAmount);
			}
			else
			{
				SetRendererArrayColorLerpToDefault(fadeColor, lerpAmount);
			}
			yield return null;
			time = Time.time;
		}
		if (fadeIn)
		{
			UseOverrideDefaultTint = true;
			DefaultTintOverrideColor = fadeColor;
			SetRendererArrayColor(fadeColor);
		}
		else
		{
			ResetBlackFillState();
		}
	}

	public void ResetAllBlackFills()
	{
		BlackFillType[] typeArray = BlackFillType_RL.TypeArray;
		foreach (BlackFillType blackFillType in typeArray)
		{
			if (blackFillType != 0 && m_blackFillEffectTable.ContainsKey(blackFillType))
			{
				m_blackFillEffectTable[blackFillType] = false;
			}
		}
		ResetBlackFillState();
	}

	private void ResetBlackFillState()
	{
		if (m_isInitialized)
		{
			UseOverrideDefaultTint = false;
			ResetRendererArrayColor();
			m_isFadedIn = false;
			m_isFadedOut = true;
			if (TraitManager.IsTraitActive(TraitType.SmallHitbox) && CompareTag("Player"))
			{
				ActivateBlackFill(BlackFillType.SmallHitbox_Trait, 0f);
			}
			else if (TraitManager.IsTraitActive(TraitType.EnemiesBlackFill) && CompareTag("Enemy"))
			{
				ActivateBlackFill(BlackFillType.EnemiesBlackFill_Trait, 0f);
			}
		}
	}

	public void SetRendererArrayColor(Color color)
	{
		if (!m_isInitialized)
		{
			return;
		}
		for (int i = 0; i < RendererArray.Count; i++)
		{
			Renderer renderer = RendererArray[i];
			renderer.GetPropertyBlock(m_matPropertyBlock_STATIC);
			RendererArrayEntry rendererArrayEntry = RendererArrayDefaultTint[i];
			if (rendererArrayEntry.HasAlphaColor)
			{
				m_matPropertyBlock_STATIC.SetColor(ShaderID_RL._AlphaBlendColor, color);
			}
			if (rendererArrayEntry.HasRimColor)
			{
				m_matPropertyBlock_STATIC.SetColor(ShaderID_RL._RimLightColor, color);
			}
			if (rendererArrayEntry.HasRimBias)
			{
				m_matPropertyBlock_STATIC.SetFloat(ShaderID_RL._RimBias, 0f);
			}
			if (rendererArrayEntry.HasRimScale)
			{
				m_matPropertyBlock_STATIC.SetFloat(ShaderID_RL._RimScale, 0f);
			}
			renderer.SetPropertyBlock(m_matPropertyBlock_STATIC);
		}
	}

	public void SetRendererArrayColorLerpFromDefault(Color color, float lerpAmount)
	{
		if (!m_isInitialized)
		{
			return;
		}
		for (int i = 0; i < RendererArray.Count; i++)
		{
			Renderer renderer = RendererArray[i];
			renderer.GetPropertyBlock(m_matPropertyBlock_STATIC);
			RendererArrayEntry rendererArrayEntry = RendererArrayDefaultTint[i];
			if (rendererArrayEntry.HasAlphaColor)
			{
				m_matPropertyBlock_STATIC.SetColor(ShaderID_RL._AlphaBlendColor, Color.Lerp(rendererArrayEntry.DefaultAlphaColor, color, lerpAmount));
			}
			if (rendererArrayEntry.HasRimColor)
			{
				m_matPropertyBlock_STATIC.SetColor(ShaderID_RL._RimLightColor, Color.Lerp(rendererArrayEntry.DefaultRimLightColor, color, lerpAmount));
			}
			if (rendererArrayEntry.HasRimBias)
			{
				m_matPropertyBlock_STATIC.SetFloat(ShaderID_RL._RimBias, Mathf.Lerp(rendererArrayEntry.DefaultRimBias, 0f, lerpAmount));
			}
			if (rendererArrayEntry.HasRimScale)
			{
				m_matPropertyBlock_STATIC.SetFloat(ShaderID_RL._RimScale, Mathf.Lerp(rendererArrayEntry.DefaultRimScale, 0f, lerpAmount));
			}
			renderer.SetPropertyBlock(m_matPropertyBlock_STATIC);
		}
	}

	public void SetRendererArrayColorLerpToDefault(Color color, float lerpAmount)
	{
		if (!m_isInitialized)
		{
			return;
		}
		for (int i = 0; i < RendererArray.Count; i++)
		{
			Renderer renderer = RendererArray[i];
			renderer.GetPropertyBlock(m_matPropertyBlock_STATIC);
			RendererArrayEntry rendererArrayEntry = RendererArrayDefaultTint[i];
			if (rendererArrayEntry.HasAlphaColor)
			{
				m_matPropertyBlock_STATIC.SetColor(ShaderID_RL._AlphaBlendColor, Color.Lerp(color, rendererArrayEntry.DefaultAlphaColor, lerpAmount));
			}
			if (rendererArrayEntry.HasRimColor)
			{
				m_matPropertyBlock_STATIC.SetColor(ShaderID_RL._RimLightColor, Color.Lerp(color, rendererArrayEntry.DefaultRimLightColor, lerpAmount));
			}
			if (rendererArrayEntry.HasRimBias)
			{
				m_matPropertyBlock_STATIC.SetFloat(ShaderID_RL._RimBias, Mathf.Lerp(0f, rendererArrayEntry.DefaultRimBias, lerpAmount));
			}
			if (rendererArrayEntry.HasRimScale)
			{
				m_matPropertyBlock_STATIC.SetFloat(ShaderID_RL._RimScale, Mathf.Lerp(0f, rendererArrayEntry.DefaultRimScale, lerpAmount));
			}
			renderer.SetPropertyBlock(m_matPropertyBlock_STATIC);
		}
	}

	public void ResetRendererArrayColor()
	{
		if (!m_isInitialized)
		{
			return;
		}
		if (UseOverrideDefaultTint)
		{
			SetRendererArrayColor(DefaultTintOverrideColor);
			return;
		}
		for (int i = 0; i < RendererArray.Count; i++)
		{
			Renderer renderer = RendererArray[i];
			renderer.GetPropertyBlock(m_matPropertyBlock_STATIC);
			RendererArrayEntry rendererArrayEntry = RendererArrayDefaultTint[i];
			if (rendererArrayEntry.HasAlphaColor)
			{
				m_matPropertyBlock_STATIC.SetColor(ShaderID_RL._AlphaBlendColor, rendererArrayEntry.DefaultAlphaColor);
			}
			if (rendererArrayEntry.HasRimColor)
			{
				m_matPropertyBlock_STATIC.SetColor(ShaderID_RL._RimLightColor, rendererArrayEntry.DefaultRimLightColor);
			}
			if (rendererArrayEntry.HasRimBias)
			{
				m_matPropertyBlock_STATIC.SetFloat(ShaderID_RL._RimBias, rendererArrayEntry.DefaultRimBias);
			}
			if (rendererArrayEntry.HasRimScale)
			{
				m_matPropertyBlock_STATIC.SetFloat(ShaderID_RL._RimScale, rendererArrayEntry.DefaultRimScale);
			}
			renderer.SetPropertyBlock(m_matPropertyBlock_STATIC);
		}
	}

	private void OnDisable()
	{
		ResetBlackFillState();
		StopAllCoroutines();
		StopInvincibilityEffect();
	}

	public void StartSingleBlinkEffect()
	{
		StartSingleBlinkEffect(m_blinkOnHitTint);
	}

	public void StartSingleBlinkThenInvincibility()
	{
		if (base.gameObject.activeInHierarchy)
		{
			if (m_blinkCoroutine != null)
			{
				StopCoroutine(m_blinkCoroutine);
			}
			m_blinkCoroutine = StartCoroutine(SingleBlinkThenInvincibilityCoroutine(m_blinkOnHitTint));
		}
	}

	private IEnumerator SingleBlinkThenInvincibilityCoroutine(Color blinkColor)
	{
		yield return SingleBlinkCoroutine(blinkColor);
		StartInvincibilityEffect();
	}

	public void StartSingleBlinkEffect(Color blinkColor)
	{
		if (base.gameObject.activeInHierarchy)
		{
			if (m_blinkCoroutine != null)
			{
				StopCoroutine(m_blinkCoroutine);
			}
			m_blinkCoroutine = StartCoroutine(SingleBlinkCoroutine(blinkColor));
		}
	}

	private IEnumerator SingleBlinkCoroutine(Color blinkColor)
	{
		m_blinkWaitYield.CreateNew(m_singleBlinkDuration, UseUnscaledTime);
		if (RendererArray != null)
		{
			SetRendererArrayColor(blinkColor);
			yield return m_blinkWaitYield;
			ResetRendererArrayColor();
		}
	}

	public void StartInvincibilityEffect(float duration = -1f)
	{
		if (m_blinkCoroutine != null)
		{
			StopCoroutine(m_blinkCoroutine);
		}
		StopInvincibilityEffect();
		m_invincibilityCoroutine = StartCoroutine(InvincibilityCoroutine(duration));
	}

	private IEnumerator InvincibilityCoroutine(float duration = -1f)
	{
		if (duration == -1f)
		{
			duration = 999999f;
		}
		float startTime = Time.time;
		while (Time.time < startTime + duration)
		{
			m_invincibilityWaitYield.CreateNew(m_invincibilityPulseRate, UseUnscaledTime);
			if (!m_invincibilityEffectOn)
			{
				ResetRendererArrayColor();
			}
			else
			{
				SetRendererArrayColor(m_invincibilityTint);
			}
			m_invincibilityEffectOn = !m_invincibilityEffectOn;
			yield return m_invincibilityWaitYield;
		}
		if (RendererArray != null)
		{
			ResetRendererArrayColor();
		}
		m_invincibilityEffectOn = false;
	}

	public void StopInvincibilityEffect()
	{
		if (m_invincibilityCoroutine != null)
		{
			StopCoroutine(m_invincibilityCoroutine);
		}
		if (RendererArray != null)
		{
			ResetRendererArrayColor();
		}
		m_invincibilityEffectOn = false;
	}
}
