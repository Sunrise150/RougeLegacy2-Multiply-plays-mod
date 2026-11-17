using System;
using UnityEngine;

[Serializable]
public class EquipmentSetBonus
{
	[SerializeField]
	private EquipmentSetBonusType m_bonusType;

	[SerializeField]
	private float m_statGain;

	public EquipmentSetBonusType BonusType
	{
		get
		{
			return m_bonusType;
		}
		private set
		{
			m_bonusType = value;
		}
	}

	public float StatGain
	{
		get
		{
			return m_statGain;
		}
		private set
		{
			m_statGain = value;
		}
	}

	public EquipmentSetBonus(EquipmentSetBonusType bonusType, float statGain)
	{
		BonusType = bonusType;
		StatGain = statGain;
	}
}
