using System;
using UnityEngine;

[Serializable]
public class MaterialBlendWeightObject
{
	[SerializeField]
	private EquipmentType m_equipmentType;

	[SerializeField]
	private Material m_material;

	[SerializeField]
	private int m_blendWeight;

	public EquipmentType EquipmentType
	{
		get
		{
			return m_equipmentType;
		}
		set
		{
			m_equipmentType = value;
		}
	}

	public Material Material
	{
		get
		{
			return m_material;
		}
		set
		{
			m_material = value;
		}
	}

	public int BlendWeight
	{
		get
		{
			return m_blendWeight;
		}
		set
		{
			m_blendWeight = value;
		}
	}
}
