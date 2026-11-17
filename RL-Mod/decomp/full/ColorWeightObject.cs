using System;
using UnityEngine;

[Serializable]
public class ColorWeightObject : ILookWeight
{
	[SerializeField]
	private Color m_color = Color.white;

	[SerializeField]
	private bool m_excludeFromWeighing;

	[SerializeField]
	private float m_maleWeight = 1f;

	[SerializeField]
	private float m_femaleWeight = 1f;

	[SerializeField]
	private string[] m_tags;

	public Color Color => m_color;

	public float MaleWeight => m_maleWeight;

	public float FemaleWeight => m_femaleWeight;

	public string[] Tags => m_tags;

	public bool ExcludeFromWeighing => m_excludeFromWeighing;
}
