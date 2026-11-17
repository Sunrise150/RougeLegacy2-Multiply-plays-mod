using System;
using UnityEngine;

[Serializable]
public class MaterialWeightObject : ILookWeight
{
	[SerializeField]
	private Material m_material;

	[SerializeField]
	private bool m_excludeFromWeighing;

	[SerializeField]
	private float m_maleWeight = 1f;

	[SerializeField]
	private float m_femaleWeight = 1f;

	[SerializeField]
	private string[] m_tags;

	public Material Material => m_material;

	public float MaleWeight => m_maleWeight;

	public float FemaleWeight => m_femaleWeight;

	public string[] Tags => m_tags;

	public bool ExcludeFromWeighing => m_excludeFromWeighing;
}
