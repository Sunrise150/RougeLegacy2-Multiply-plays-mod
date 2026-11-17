using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BossModeShiftController : MonoBehaviour
{
	[SerializeField]
	private GameObject m_visuals;

	[SerializeField]
	private Renderer[] m_materialsToChangeFrom;

	[SerializeField]
	private Material[] m_materialsToChangeTo;

	[SerializeField]
	private Renderer[] m_renderersToNotDissolve;

	private Material[] m_initialMats;

	private Renderer[] m_renderersToDissolve;

	private static MaterialPropertyBlock m_matBlock_STATIC;

	private void Awake()
	{
		if (m_matBlock_STATIC == null)
		{
			m_matBlock_STATIC = new MaterialPropertyBlock();
		}
		List<Renderer> list = m_visuals.GetComponentsInChildren<Renderer>().ToList();
		Renderer[] renderersToNotDissolve = m_renderersToNotDissolve;
		foreach (Renderer item in renderersToNotDissolve)
		{
			list.Remove(item);
		}
		m_renderersToDissolve = list.ToArray();
		m_initialMats = new Material[m_materialsToChangeFrom.Length];
		for (int j = 0; j < m_materialsToChangeFrom.Length; j++)
		{
			m_initialMats[j] = m_materialsToChangeFrom[j].material;
		}
	}

	private void OnEnable()
	{
		Reset();
	}

	public void ChangeMaterials()
	{
		if (m_materialsToChangeFrom.Length != m_materialsToChangeTo.Length)
		{
			throw new Exception("Cannot Change Materials. Material Change arrays must be same size.");
		}
		for (int i = 0; i < m_materialsToChangeFrom.Length; i++)
		{
			m_materialsToChangeFrom[i].material = m_materialsToChangeTo[i];
		}
	}

	public void DissolveMaterials(float duration)
	{
		StopAllCoroutines();
		StartCoroutine(DissolveCoroutine(duration));
	}

	private IEnumerator DissolveCoroutine(float duration)
	{
		float startTime = Time.time;
		Renderer[] renderersToDissolve;
		while (Time.time < startTime + duration)
		{
			float value = (Time.time - startTime) / duration;
			renderersToDissolve = m_renderersToDissolve;
			foreach (Renderer obj in renderersToDissolve)
			{
				obj.GetPropertyBlock(m_matBlock_STATIC);
				m_matBlock_STATIC.SetFloat(ShaderID_RL._Dissolve, value);
				obj.SetPropertyBlock(m_matBlock_STATIC);
			}
			yield return null;
		}
		renderersToDissolve = m_renderersToDissolve;
		foreach (Renderer obj2 in renderersToDissolve)
		{
			obj2.GetPropertyBlock(m_matBlock_STATIC);
			m_matBlock_STATIC.SetFloat(ShaderID_RL._Dissolve, 1f);
			obj2.SetPropertyBlock(m_matBlock_STATIC);
		}
	}

	public void Reset()
	{
		for (int i = 0; i < m_materialsToChangeFrom.Length; i++)
		{
			m_materialsToChangeFrom[i].material = m_initialMats[i];
		}
		Renderer[] renderersToDissolve = m_renderersToDissolve;
		foreach (Renderer obj in renderersToDissolve)
		{
			obj.GetPropertyBlock(m_matBlock_STATIC);
			m_matBlock_STATIC.SetFloat(ShaderID_RL._Dissolve, 0f);
			obj.SetPropertyBlock(m_matBlock_STATIC);
		}
	}
}
