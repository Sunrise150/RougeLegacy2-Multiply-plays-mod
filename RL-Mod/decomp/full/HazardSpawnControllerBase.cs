using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public abstract class HazardSpawnControllerBase : ComplexSpawnController, IHazardSpawnController, IComplexSpawnController, ISpawnController, IRoomConsumer, IIDConsumer, IStateConsumer, ISetSpawnType, IHasProjectileNameArray
{
	[SerializeField]
	[ReadOnly]
	private HazardType m_type;

	[SerializeField]
	[ReadOnly]
	private StateID m_initialState;

	protected HazardArgs m_hazardArgs;

	[NonSerialized]
	private string[] m_projectileNameArray;

	public string[] ProjectileNameArray
	{
		get
		{
			if (m_projectileNameArray == null)
			{
				List<string> list = new List<string>();
				Hazard prefab = HazardLibrary.GetPrefab(Type);
				if ((bool)prefab)
				{
					IHasProjectileNameArray[] componentsInChildren = prefab.GetComponentsInChildren<IHasProjectileNameArray>(includeInactive: true);
					for (int i = 0; i < componentsInChildren.Length; i++)
					{
						string[] projectileNameArray = componentsInChildren[i].ProjectileNameArray;
						foreach (string item in projectileNameArray)
						{
							if (!list.Contains(item))
							{
								list.Add(item);
							}
						}
					}
				}
				m_projectileNameArray = list.ToArray();
			}
			return m_projectileNameArray;
		}
	}

	public abstract HazardCategory Category { get; }

	public IHazard Hazard { get; protected set; }

	public StateID InitialState
	{
		get
		{
			return m_initialState;
		}
		private set
		{
			m_initialState = value;
		}
	}

	public HazardType Type
	{
		get
		{
			return m_type;
		}
		private set
		{
			m_type = value;
		}
	}

	private void Start()
	{
		SpriteRenderer component = base.gameObject.GetComponent<SpriteRenderer>();
		if (component != null)
		{
			component.enabled = false;
		}
	}

	protected void CreateHazard()
	{
		Hazard = GetHazard(Type);
		if (!Hazard.IsNativeNull())
		{
			Hazard.gameObject.SetActive(value: true);
			Hazard.gameObject.transform.position = base.transform.position;
			Hazard.gameObject.transform.rotation = base.transform.rotation;
			Hazard.gameObject.transform.localScale = base.transform.localScale;
			IRoomConsumer[] componentsInChildren = Hazard.gameObject.GetComponentsInChildren<IRoomConsumer>();
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				componentsInChildren[i].SetRoom(base.Room);
			}
		}
	}

	public override void SetRoom(BaseRoom room)
	{
		base.SetRoom(room);
		if (GameUtility.IsInLevelEditor)
		{
			SetSpawnType();
		}
	}

	public void SetSpawnType()
	{
		if (Type == HazardType.BiomeSpecific)
		{
			HazardType[] array = HazardLibrary.GetDefaultHazardTypes(Category);
			if (HazardLibrary.GetHazardsInBiomeTable(Category).ContainsKey(base.Room.AppearanceBiomeType))
			{
				array = HazardLibrary.GetHazardsInBiomeTable(Category)[base.Room.AppearanceBiomeType];
			}
			int num = 0;
			if (array.Length > 1)
			{
				num = RNGManager.GetRandomNumber(RngID.Hazards_RoomSeed, "Get Random Hazard Index", 0, array.Length);
			}
			HazardType hazardType = array[num];
			if (hazardType == HazardType.Orbiter && base.Room.AppearanceBiomeType == BiomeType.Castle && BurdenManager.IsBurdenActive(BurdenType.CastleBiomeUp))
			{
				hazardType = HazardType.Triple_Orbiter;
			}
			if (hazardType == HazardType.IceCrystal && base.Room.AppearanceBiomeType == BiomeType.Forest && BurdenManager.IsBurdenActive(BurdenType.ForestBiomeUp))
			{
				hazardType = HazardType.SentryWithIce;
			}
			SetType(hazardType);
		}
	}

	protected virtual IHazard GetHazard(HazardType hazardType)
	{
		return HazardManager.GetHazard(Type);
	}

	public void SetInitialState(StateID state)
	{
	}

	public void SetType(HazardType hazardType)
	{
		Type = hazardType;
	}

	[SpecialName]
	GameObject ISpawnController.get_gameObject()
	{
		return base.gameObject;
	}
}
