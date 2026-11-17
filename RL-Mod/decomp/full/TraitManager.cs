using System;
using System.Collections.Generic;
using UnityEngine;

public class TraitManager : MonoBehaviour
{
	private List<BaseTrait> m_activeTraitsList = new List<BaseTrait>();

	private List<TraitType> m_activeTraitTypeList = new List<TraitType>();

	private Action<MonoBehaviour, EventArgs> m_onTraitsChanged;

	public static bool IsInitialized { get; private set; }

	public static TraitManager Instance { get; private set; }

	public static List<BaseTrait> ActiveTraitList => Instance.m_activeTraitsList;

	public static List<TraitType> ActiveTraitTypeList => Instance.m_activeTraitTypeList;

	private static Dictionary<TraitType, SerializableVector3Int> TraitSpawnOddsTable => SaveManager.PlayerSaveData.TraitSpawnOddsTable;

	public static float GetActualTraitGoldGain(TraitType traitType)
	{
		if (SkillTreeManager.GetSkillObjLevel(SkillTreeType.Traits_Give_Gold) > 0)
		{
			TraitData traitData = TraitLibrary.GetTraitData(traitType);
			if (traitData != null)
			{
				return traitData.GoldBonus * (1f + SkillTreeManager.GetSkillTreeObj(SkillTreeType.Traits_Give_Gold_Gain_Mod).CurrentStatGain);
			}
		}
		return 0f;
	}

	public static bool IsTraitActive(TraitType traitType)
	{
		if (traitType == TraitType.None)
		{
			return true;
		}
		if (IsInitialized)
		{
			if (!Instance || Instance.m_activeTraitTypeList == null)
			{
				if (!Instance)
				{
					Debug.LogException(new Exception("IsTraitActive Error. Instance is Null even though IsInitialized == true"));
				}
				else
				{
					Debug.LogException(new Exception("IsTraitActive Error. Instance.m_activeTraitTypeList is Null."));
				}
				return false;
			}
			return Instance.m_activeTraitTypeList.Contains(traitType);
		}
		return false;
	}

	public static BaseTrait GetActiveTrait(TraitType traitType)
	{
		if (IsInitialized)
		{
			foreach (BaseTrait activeTraits in Instance.m_activeTraitsList)
			{
				if (activeTraits.TraitType == traitType)
				{
					return activeTraits;
				}
			}
		}
		return null;
	}

	private void Awake()
	{
		if (!Instance)
		{
			Instance = this;
			Initialize();
		}
		else
		{
			UnityEngine.Object.Destroy(base.gameObject);
		}
	}

	private void Initialize()
	{
		IsInitialized = true;
		m_onTraitsChanged = OnTraitsChanged;
		Messenger<GameMessenger, GameEvent>.AddListener(GameEvent.TraitsChanged, m_onTraitsChanged);
		if (PlayerManager.IsInstantiated)
		{
			TraitType traitOne = SaveManager.PlayerSaveData.CurrentCharacter.TraitOne;
			TraitType traitTwo = SaveManager.PlayerSaveData.CurrentCharacter.TraitTwo;
			OnTraitsChanged(this, new TraitChangedEventArgs(traitOne, traitTwo));
		}
		else
		{
			Debug.Log("<color=red>WARNING: TRAITMANAGER INITIALIZED BEFORE PLAYERMANAGER. THIS IS BAD.</color>");
		}
	}

	private void OnTraitsChanged(object sender, EventArgs args)
	{
		TraitChangedEventArgs traitChangedEventArgs = args as TraitChangedEventArgs;
		List<BaseTrait> list = new List<BaseTrait>(m_activeTraitsList);
		m_activeTraitsList.Clear();
		m_activeTraitTypeList.Clear();
		foreach (BaseTrait item in list)
		{
			UnityEngine.Object.DestroyImmediate(item.gameObject);
		}
		InstantiateTrait(traitChangedEventArgs.TraitOne, isTraitOne: true);
		InstantiateTrait(traitChangedEventArgs.TraitTwo, isTraitOne: false);
	}

	private void InstantiateTrait(TraitType traitType, bool isTraitOne)
	{
		BaseTrait trait = TraitLibrary.GetTrait(traitType);
		if (trait != null)
		{
			trait = UnityEngine.Object.Instantiate(trait);
			trait.transform.SetParent(base.transform);
			if (isTraitOne)
			{
				trait.IsFirstTrait = true;
			}
			else
			{
				trait.IsFirstTrait = false;
				trait.AssignGreenMask();
			}
			m_activeTraitsList.Add(trait);
			m_activeTraitTypeList.Add(traitType);
		}
	}

	public static FoundState GetTraitFoundState(TraitType traitType)
	{
		if (TraitSpawnOddsTable.TryGetValue(traitType, out var value))
		{
			if (value.z > 0)
			{
				return FoundState.Purchased;
			}
			return (FoundState)value.z;
		}
		throw new KeyNotFoundException("Cannot GET trait found state [" + traitType.ToString() + "] because entry not found in Player Save Data.");
	}

	public static void SetTraitFoundState(TraitType traitType, FoundState foundState)
	{
		if (TraitSpawnOddsTable.ContainsKey(traitType))
		{
			Vector3Int vector3Int = TraitSpawnOddsTable[traitType];
			vector3Int.z = (int)foundState;
			TraitSpawnOddsTable[traitType] = vector3Int;
			if (foundState == FoundState.Purchased)
			{
				SetTraitUpgradeLevel(traitType, 1, additive: false, overrideValues: false);
			}
			return;
		}
		throw new KeyNotFoundException("Cannot SET trait found state [" + traitType.ToString() + "] because entry not found in Player Save Data.");
	}

	public static TraitSpawnOdds GetTraitSpawnOdds(TraitType traitType)
	{
		if (TraitSpawnOddsTable.TryGetValue(traitType, out var value))
		{
			return (TraitSpawnOdds)Mathf.Clamp(value.x, 0, GetTraitUpgradeLevel(traitType));
		}
		throw new KeyNotFoundException("Cannot GET trait spawn odds [" + traitType.ToString() + "] because entry not found in Player Save Data.");
	}

	public static void SetTraitSpawnOdds(TraitType traitType, int odds, bool additive)
	{
		if (TraitSpawnOddsTable.ContainsKey(traitType))
		{
			Vector3Int vector3Int = TraitSpawnOddsTable[traitType];
			if (GetTraitFoundState(traitType) < FoundState.Purchased)
			{
				Debug.Log("<color=red>Cannot set trait spawn odds.  Trait has not been purchased.  Call SetTraitFoundState() first.</color>");
				return;
			}
			int x = vector3Int.x;
			x = ((!additive) ? odds : (x + odds));
			x = Mathf.Clamp(x, 0, 3);
			vector3Int.x = x;
			TraitSpawnOddsTable[traitType] = vector3Int;
			return;
		}
		throw new KeyNotFoundException("Cannot SET trait odds [" + traitType.ToString() + "] because entry not found in Player Save Data.");
	}

	public static void SetTraitSpawnOdds(TraitType traitType, TraitSpawnOdds traitSpawnOdds)
	{
		if (TraitSpawnOddsTable.ContainsKey(traitType))
		{
			Vector3Int vector3Int = TraitSpawnOddsTable[traitType];
			vector3Int.x = (int)traitSpawnOdds;
			TraitSpawnOddsTable[traitType] = vector3Int;
			return;
		}
		throw new KeyNotFoundException("Cannot SET trait spawn odds [" + traitType.ToString() + "] because entry not found in Player Save Data.");
	}

	public static int GetTraitBlueprintLevel(TraitType traitType)
	{
		if (TraitSpawnOddsTable.TryGetValue(traitType, out var value))
		{
			return value.y;
		}
		throw new KeyNotFoundException("Cannot GET trait blueprint level [" + traitType.ToString() + "] because entry not found in Player Save Data.");
	}

	public static void SetTraitBlueprintLevel(TraitType traitType, int level, bool additive)
	{
		if (TraitSpawnOddsTable.ContainsKey(traitType))
		{
			Vector3Int vector3Int = TraitSpawnOddsTable[traitType];
			int y = vector3Int.y;
			y = ((!additive) ? level : (y + level));
			vector3Int.y = Mathf.Clamp(y, 0, 3);
			TraitSpawnOddsTable[traitType] = vector3Int;
			return;
		}
		throw new KeyNotFoundException("Cannot SET trait blueprint level [" + traitType.ToString() + "] because entry not found in Player Save Data.");
	}

	public static int GetTraitUpgradeLevel(TraitType traitType)
	{
		if (TraitSpawnOddsTable.TryGetValue(traitType, out var value))
		{
			return value.z;
		}
		throw new KeyNotFoundException("Cannot GET trait upgrade level [" + traitType.ToString() + "] because entry not found in Player Save Data.");
	}

	public static bool SetTraitUpgradeLevel(TraitType traitType, int level, bool additive, bool overrideValues)
	{
		if (TraitSpawnOddsTable.ContainsKey(traitType))
		{
			if (GetTraitFoundState(traitType) != 0 && !overrideValues)
			{
				Debug.LogFormat("<color=red>TraitManager.SetTraitUpgradeLevel({0}) failed. First blueprint not purchased yet. Use overrideValues = true if intended or set Found State to Purchased via TraitManager.SetFoundState().</color>", traitType);
				return false;
			}
			Vector3Int vector3Int = TraitSpawnOddsTable[traitType];
			int z = vector3Int.z;
			z = ((!additive) ? level : (z + level));
			if (z > GetTraitBlueprintLevel(traitType) && !overrideValues)
			{
				Debug.LogFormat("<color=red>TraitManager.SetTraitUpgradeLevel({0}) failed. Blueprint Found Level is less than desired Upgrade Level. Use overrideValues = true if intended.</color>", traitType);
				return false;
			}
			if (z < vector3Int.z && !overrideValues)
			{
				Debug.LogFormat("<color=red>TraitManager.SetTraitUpgradeLevel({0}) failed. New Upgrade Level less than current Upgrade Level. Use overrideValues = true if intended.</color>", traitType);
				return false;
			}
			vector3Int.z = Mathf.Clamp(z, 0, 3);
			TraitSpawnOddsTable[traitType] = vector3Int;
			return true;
		}
		throw new KeyNotFoundException("Cannot SET trait upgrade level [" + traitType.ToString() + "] because entry not found in Player Save Data.");
	}

	public static bool CanAfford(TraitType traitType)
	{
		if (SaveManager.PlayerSaveData.GoldCollectedIncludingBank >= 0 && SaveManager.PlayerSaveData.EquipmentOreCollected >= 0)
		{
			return true;
		}
		return false;
	}

	public static bool CanPurchaseTrait(TraitType traitType)
	{
		FoundState traitFoundState = GetTraitFoundState(traitType);
		int traitBlueprintLevel = GetTraitBlueprintLevel(traitType);
		if (traitFoundState > FoundState.NotFound && traitFoundState < FoundState.Purchased && traitBlueprintLevel > 0 && CanAfford(traitType))
		{
			return true;
		}
		return false;
	}

	public static bool CanUpgradeTrait(TraitType traitType)
	{
		FoundState traitFoundState = GetTraitFoundState(traitType);
		int traitBlueprintLevel = GetTraitBlueprintLevel(traitType);
		int traitUpgradeLevel = GetTraitUpgradeLevel(traitType);
		if (traitFoundState == FoundState.Purchased && traitUpgradeLevel < traitBlueprintLevel && traitUpgradeLevel < 3 && CanAfford(traitType))
		{
			return true;
		}
		return false;
	}

	public static TraitSeenState GetTraitSeenState(TraitType traitType)
	{
		if (traitType == TraitType.Antique)
		{
			return TraitSeenState.SeenTwice;
		}
		if (SaveManager.PlayerSaveData.TraitSeenTable.TryGetValue(traitType, out var value))
		{
			return value;
		}
		return TraitSeenState.NeverSeen;
	}

	public static void SetTraitSeenState(TraitType traitType, TraitSeenState seenState, bool forceState)
	{
		if (SaveManager.PlayerSaveData.TraitSeenTable.ContainsKey(traitType) && (SaveManager.PlayerSaveData.TraitSeenTable[traitType] < seenState || forceState))
		{
			SaveManager.PlayerSaveData.TraitSeenTable[traitType] = seenState;
		}
	}

	private void OnDestroy()
	{
		IsInitialized = false;
		Messenger<GameMessenger, GameEvent>.RemoveListener(GameEvent.TraitsChanged, m_onTraitsChanged);
	}
}
