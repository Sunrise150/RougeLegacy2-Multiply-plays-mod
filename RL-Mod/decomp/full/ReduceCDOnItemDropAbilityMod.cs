using System;
using UnityEngine;

[RequireComponent(typeof(BaseAbility_RL))]
public class ReduceCDOnItemDropAbilityMod : MonoBehaviour
{
	[SerializeField]
	private float m_cooldownReductionAmount;

	[SerializeField]
	private bool m_isFlat;

	[SerializeField]
	private bool m_triggerOnCoins;

	[SerializeField]
	private bool m_triggerOnEquipmentOre;

	[SerializeField]
	private bool m_triggerOnRuneOre;

	[SerializeField]
	private bool m_triggerOnHealthDrops;

	[SerializeField]
	private bool m_triggerOnManaDrops;

	[SerializeField]
	private bool m_triggerOnSoul;

	private BaseAbility_RL m_ability;

	private Action<MonoBehaviour, EventArgs> m_onItemCollected;

	private void Awake()
	{
		m_ability = GetComponent<BaseAbility_RL>();
		m_onItemCollected = OnItemCollected;
	}

	private void OnEnable()
	{
		Messenger<GameMessenger, GameEvent>.AddListener(GameEvent.ItemCollected, m_onItemCollected);
	}

	private void OnDisable()
	{
		Messenger<GameMessenger, GameEvent>.RemoveListener(GameEvent.ItemCollected, m_onItemCollected);
	}

	private void OnItemCollected(object sender, EventArgs args)
	{
		ItemCollectedEventArgs obj = args as ItemCollectedEventArgs;
		bool flag = false;
		switch (obj.Item.ItemDropType)
		{
		case ItemDropType.MoneyBag:
		case ItemDropType.Diamond:
		case ItemDropType.Crystal:
		case ItemDropType.Coin:
			if (m_triggerOnCoins)
			{
				flag = true;
			}
			break;
		case ItemDropType.HealthDrop:
		case ItemDropType.PizzaDrop:
		case ItemDropType.MushroomDrop:
		case ItemDropType.CandyDrop:
		case ItemDropType.CookieDrop:
			if (m_triggerOnHealthDrops)
			{
				flag = true;
			}
			break;
		case ItemDropType.ManaDrop:
		case ItemDropType.MilkManaDrop:
			if (m_triggerOnManaDrops)
			{
				flag = true;
			}
			break;
		case ItemDropType.EquipmentOre:
			if (m_triggerOnEquipmentOre)
			{
				flag = true;
			}
			break;
		case ItemDropType.RuneOre:
			if (m_triggerOnRuneOre)
			{
				flag = true;
			}
			break;
		case ItemDropType.Soul:
			if (m_triggerOnSoul)
			{
				flag = true;
			}
			break;
		}
		if (flag)
		{
			if (m_isFlat)
			{
				m_ability.ReduceFlatCooldown((int)m_cooldownReductionAmount);
			}
			else
			{
				m_ability.ReduceCooldown(m_ability.ActualCooldownTime * m_cooldownReductionAmount);
			}
		}
	}
}
