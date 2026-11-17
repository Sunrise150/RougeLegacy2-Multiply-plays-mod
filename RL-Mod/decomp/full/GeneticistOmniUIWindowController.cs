using System;
using System.Linq;
using UnityEngine;

public class GeneticistOmniUIWindowController : BaseOmniUIWindowController<BaseOmniUICategoryEntry, GeneticistOmniUIEntry>
{
	public override WindowID ID => WindowID.Geneticist;

	protected override void CreateCategoryEntries()
	{
	}

	protected override void CreateEntries()
	{
		if (base.EntryArray != null)
		{
			Array.Clear(base.EntryArray, 0, base.EntryArray.Length);
			base.EntryArray = null;
		}
		Array typeArray = TraitType_RL.TypeArray;
		base.EntryArray = new GeneticistOmniUIEntry[typeArray.Length - 1];
		int num = 0;
		foreach (TraitType item in typeArray)
		{
			if (item != 0 && !(TraitLibrary.GetTraitData(item) == null))
			{
				base.EntryArray[num] = UnityEngine.Object.Instantiate(m_entryPrefab);
				base.EntryArray[num].transform.SetParent(base.EntryLayoutGroup.transform);
				base.EntryArray[num].transform.localScale = Vector3.one;
				base.EntryArray[num].Initialize(item, this);
				base.EntryArray[num].SetEntryIndex(num);
				num++;
			}
		}
		base.EntryArray = base.EntryArray.Take(num).ToArray();
	}

	protected override void UpdateScrollArrows(float scrollAmount)
	{
		base.UpdateScrollArrows(scrollAmount);
		if (!base.TopScrollArrow.activeSelf && !base.BottomScrollArrow.activeSelf)
		{
			return;
		}
		float num = (float)base.ActiveEntryArray.Length * m_entryHeight;
		float num2 = Mathf.Clamp(1f - base.ScrollBar.value, 0f, 1f);
		float height = base.ContentViewport.rect.height;
		float num3 = height;
		float num4 = num3 - height;
		if (num2 > 0f)
		{
			num3 = height + (num - height) * num2;
			num4 = num3 - height;
		}
		int num5 = Mathf.CeilToInt(num4 / m_entryHeight);
		int num6 = Mathf.FloorToInt(num3 / m_entryHeight);
		if (base.TopScrollArrow.activeSelf)
		{
			bool flag = false;
			bool flag2 = false;
			for (int i = 0; i < num5; i++)
			{
				if (TraitManager.GetTraitFoundState(base.ActiveEntryArray[i].TraitType) == FoundState.FoundButNotViewed)
				{
					flag = true;
					break;
				}
			}
			if (flag)
			{
				SetGameObjectActive(base.TopScrollNewSymbol.gameObject, active: true);
				SetGameObjectActive(base.TopScrollUpgradeSymbol.gameObject, active: false);
			}
			else if (flag2)
			{
				SetGameObjectActive(base.TopScrollNewSymbol.gameObject, active: false);
				SetGameObjectActive(base.TopScrollUpgradeSymbol.gameObject, active: true);
			}
			else
			{
				SetGameObjectActive(base.TopScrollNewSymbol.gameObject, active: false);
				SetGameObjectActive(base.TopScrollUpgradeSymbol.gameObject, active: false);
			}
		}
		if (!base.BottomScrollArrow.activeSelf)
		{
			return;
		}
		bool flag3 = false;
		bool flag4 = false;
		for (int j = num6; j < base.ActiveEntryArray.Length; j++)
		{
			if (TraitManager.GetTraitFoundState(base.ActiveEntryArray[j].TraitType) == FoundState.FoundButNotViewed)
			{
				flag3 = true;
				break;
			}
		}
		if (flag3)
		{
			SetGameObjectActive(base.BottomScrollNewSymbol.gameObject, active: true);
			SetGameObjectActive(base.BottomScrollUpgradeSymbol.gameObject, active: false);
		}
		else if (flag4)
		{
			SetGameObjectActive(base.BottomScrollNewSymbol.gameObject, active: false);
			SetGameObjectActive(base.BottomScrollUpgradeSymbol.gameObject, active: true);
		}
		else
		{
			SetGameObjectActive(base.BottomScrollNewSymbol.gameObject, active: false);
			SetGameObjectActive(base.BottomScrollUpgradeSymbol.gameObject, active: false);
		}
	}

	private void SetGameObjectActive(GameObject obj, bool active)
	{
		if (obj.activeSelf == !active)
		{
			obj.SetActive(active);
		}
	}
}
