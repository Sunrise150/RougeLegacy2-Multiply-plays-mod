using System.Collections;
using UnityEngine;

public class VoidDissolveComponent : MonoBehaviour
{
	[SerializeField]
	private float m_duration = 1f;

	[SerializeField]
	private Renderer[] m_renderers;

	[SerializeField]
	private ParticleSystem[] m_particleSystemsToStop;

	private float[] m_initialValues;

	private bool m_hasDissolved;

	public bool IsDissolving { get; private set; }

	private void Awake()
	{
		m_initialValues = new float[m_renderers.Length];
		int num = m_renderers.Length;
		for (int i = 0; i < num; i++)
		{
			m_renderers[i].material.GetFloat("_AlphaGain");
		}
	}

	public void StartDissolve(bool playSparkle)
	{
		StopAllCoroutines();
		StartCoroutine(DissolveCoroutine(playSparkle));
	}

	public IEnumerator DissolveCoroutine(bool playSparkle)
	{
		if (m_renderers == null)
		{
			yield break;
		}
		if (playSparkle)
		{
			EffectManager.PlayEffect(effectPosition: new Vector3(base.transform.position.x, PlayerManager.GetPlayerController().Midpoint.y), source: base.gameObject, sourceAnimator: null, effectName: "VoidProjectilePassThrough_Effect", duration: 0f, stopType: EffectStopType.Gracefully);
		}
		IsDissolving = true;
		m_hasDissolved = true;
		float startTime = Time.time;
		float endTime = Time.time + m_duration;
		int rendererLength = m_renderers.Length;
		if (m_particleSystemsToStop != null)
		{
			ParticleSystem[] particleSystemsToStop = m_particleSystemsToStop;
			for (int i = 0; i < particleSystemsToStop.Length; i++)
			{
				particleSystemsToStop[i].Stop();
			}
		}
		while (Time.time < endTime)
		{
			float t = Time.time - startTime;
			for (int j = 0; j < rendererLength; j++)
			{
				float b = m_initialValues[j];
				float value = Ease.None(t, b, -1f, m_duration);
				m_renderers[j].material.SetFloat("_AlphaGain", value);
			}
			yield return null;
		}
		Renderer[] renderers = m_renderers;
		for (int i = 0; i < renderers.Length; i++)
		{
			renderers[i].material.SetFloat("_AlphaGain", -1f);
		}
		IsDissolving = false;
	}

	public void ForceDissolved()
	{
		m_hasDissolved = true;
		Renderer[] renderers = m_renderers;
		for (int i = 0; i < renderers.Length; i++)
		{
			renderers[i].material.SetFloat("_AlphaGain", -1f);
		}
	}

	private void OnDisable()
	{
		if (m_hasDissolved)
		{
			Reset();
		}
	}

	public void Reset()
	{
		IsDissolving = false;
		m_hasDissolved = false;
		if (m_renderers != null)
		{
			for (int i = 0; i < m_renderers.Length; i++)
			{
				m_renderers[i].material.SetFloat("_AlphaGain", m_initialValues[i]);
			}
		}
	}
}
