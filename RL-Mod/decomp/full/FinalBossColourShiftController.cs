using System.Collections;
using UnityEngine;

public class FinalBossColourShiftController : MonoBehaviour
{
	private const string TextureBlendShaderID = "_TextureBlend";

	[SerializeField]
	private Renderer[] m_renderers;

	private static MaterialPropertyBlock m_colourShiftMatBlock;

	private Coroutine m_colourShiftLerpCoroutine;

	public void ShiftColour(bool shiftToWhite, float lerpSpeed, bool isAdvanced)
	{
		if (m_colourShiftMatBlock == null)
		{
			m_colourShiftMatBlock = new MaterialPropertyBlock();
		}
		if (m_colourShiftLerpCoroutine != null)
		{
			StopCoroutine(m_colourShiftLerpCoroutine);
		}
		if (!shiftToWhite)
		{
			if (lerpSpeed > 0f)
			{
				m_colourShiftLerpCoroutine = StartCoroutine(ColourShiftLerpCoroutine(shiftToWhite: false, lerpSpeed, isAdvanced));
				return;
			}
			Renderer[] renderers = m_renderers;
			foreach (Renderer obj in renderers)
			{
				obj.GetPropertyBlock(m_colourShiftMatBlock);
				m_colourShiftMatBlock.SetFloat("_TextureBlend", 1f);
				obj.SetPropertyBlock(m_colourShiftMatBlock);
			}
		}
		else if (lerpSpeed > 0f)
		{
			m_colourShiftLerpCoroutine = StartCoroutine(ColourShiftLerpCoroutine(shiftToWhite: true, lerpSpeed, isAdvanced));
		}
		else
		{
			Renderer[] renderers = m_renderers;
			foreach (Renderer obj2 in renderers)
			{
				obj2.GetPropertyBlock(m_colourShiftMatBlock);
				m_colourShiftMatBlock.SetFloat("_TextureBlend", 0f);
				obj2.SetPropertyBlock(m_colourShiftMatBlock);
			}
		}
	}

	private IEnumerator ColourShiftLerpCoroutine(bool shiftToWhite, float lerpSpeed, bool isAdvanced)
	{
		if (!shiftToWhite)
		{
			_ = Color.black;
		}
		else
		{
			_ = Color.white;
		}
		GetComponent<EnemyController>();
		PlayerManager.GetPlayerController();
		float lerpDuration = lerpSpeed + Time.time;
		while (Time.time < lerpDuration)
		{
			float value = (shiftToWhite ? ((lerpDuration - Time.time) / lerpSpeed) : (1f - (lerpDuration - Time.time) / lerpSpeed));
			Renderer[] renderers = m_renderers;
			foreach (Renderer obj in renderers)
			{
				obj.GetPropertyBlock(m_colourShiftMatBlock);
				m_colourShiftMatBlock.SetFloat("_TextureBlend", value);
				obj.SetPropertyBlock(m_colourShiftMatBlock);
			}
			yield return null;
		}
	}
}
