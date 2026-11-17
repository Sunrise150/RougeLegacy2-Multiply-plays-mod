using System;
using UnityEngine;

[Serializable]
public class BodyWeightObject : ILookWeight
{
	[SerializeField]
	private BodyTypeWeightParam m_bodyTypeWeightParam = BodyTypeWeightParam.None;

	[SerializeField]
	private int m_blendShapeWeight = 100;

	[SerializeField]
	private bool m_excludeFromWeighing;

	[SerializeField]
	private float m_maleWeight = 1f;

	[SerializeField]
	private float m_femaleWeight = 1f;

	[SerializeField]
	private string[] m_tags;

	public BodyTypeWeightParam BodyTypeWeightParam => m_bodyTypeWeightParam;

	public int BlendShapeWeight => m_blendShapeWeight;

	public float MaleWeight => m_maleWeight;

	public float FemaleWeight => m_femaleWeight;

	public string[] Tags => m_tags;

	public bool ExcludeFromWeighing => m_excludeFromWeighing;
}
