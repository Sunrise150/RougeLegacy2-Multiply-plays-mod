using System;
using System.Collections.Generic;
using UnityEngine;

public class JukeboxOmniUIWindowController : BaseOmniUIWindowController<BaseOmniUICategoryEntry, JukeboxOmniUIEntry>
{
	[SerializeField]
	private JukeboxSpectrumAnalyzer m_jukeboxSpectrum;

	public JukeboxSpectrumAnalyzer JukeboxSpectrum => m_jukeboxSpectrum;

	public override WindowID ID => WindowID.Jukebox;

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
		int count = Jukebox_EV.JukeboxDataDict.Count;
		int num = 0;
		base.EntryArray = new JukeboxOmniUIEntry[count];
		foreach (KeyValuePair<SongID, JukeboxData> item in Jukebox_EV.JukeboxDataDict)
		{
			SongID key = item.Key;
			_ = item.Value;
			base.EntryArray[num] = UnityEngine.Object.Instantiate(m_entryPrefab, base.EntryLayoutGroup.transform);
			base.EntryArray[num].transform.localScale = Vector3.one;
			base.EntryArray[num].Initialize(key, this);
			base.EntryArray[num].SetEntryIndex(num);
			num++;
		}
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
				JukeboxOmniUIEntry jukeboxOmniUIEntry = base.ActiveEntryArray[i];
				if (SaveManager.PlayerSaveData.GetSongFoundState(jukeboxOmniUIEntry.SongType) == FoundState.FoundButNotViewed)
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
			JukeboxOmniUIEntry jukeboxOmniUIEntry2 = base.ActiveEntryArray[j];
			if (SaveManager.PlayerSaveData.GetSongFoundState(jukeboxOmniUIEntry2.SongType) == FoundState.FoundButNotViewed)
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
