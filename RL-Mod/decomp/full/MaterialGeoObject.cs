using System;
using UnityEngine;

[Serializable]
public class MaterialGeoObject
{
	[SerializeField]
	private EquipmentType m_equipmentType;

	[SerializeField]
	private Material m_material;

	[SerializeField]
	private GameObject m_customGeo;

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

	public GameObject CustomGeo
	{
		get
		{
			return m_customGeo;
		}
		set
		{
			m_customGeo = value;
		}
	}
}
