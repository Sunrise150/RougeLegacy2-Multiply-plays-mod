using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Custom/Libraries/Effect Library")]
public class EffectLibrary : ScriptableObject
{
	public const string RESOURCES_PATH = "Scriptable Objects/Libraries/EffectLibrary";

	[SerializeField]
	private List<EffectEntry> m_genericEffectsList = new List<EffectEntry>();

	[SerializeField]
	private List<EffectEntry> m_playerEffectsList = new List<EffectEntry>();

	[SerializeField]
	private List<EffectEntry> m_playerInteractionEffectsList = new List<EffectEntry>();

	[SerializeField]
	private List<EffectEntry> m_enemyEffectsList = new List<EffectEntry>();

	[SerializeField]
	private List<EffectEntry> m_propsEffectList = new List<EffectEntry>();

	[SerializeField]
	private List<EffectEntry> m_projectilesEffectList = new List<EffectEntry>();

	[SerializeField]
	private List<EffectEntry> m_selfAnimationEffectList = new List<EffectEntry>();

	[SerializeField]
	private List<EffectEntry> m_uiEffectList = new List<EffectEntry>();

	private static EffectLibrary m_instance;

	public static EffectLibrary Instance
	{
		get
		{
			if (m_instance == null)
			{
				m_instance = CDGResources.Load<EffectLibrary>("Scriptable Objects/Libraries/EffectLibrary");
			}
			return m_instance;
		}
	}

	public static IEnumerator LoadAsync()
	{
		CDGAsyncLoadRequest<EffectLibrary> library = CDGResources.LoadAsync<EffectLibrary>("Scriptable Objects/Libraries/EffectLibrary");
		while (!library.IsDone)
		{
			yield return null;
		}
		m_instance = library.Asset;
	}

	public static List<EffectEntry> GetEffectEntryList(EffectCategoryType category)
	{
		return category switch
		{
			EffectCategoryType.Generic => Instance.m_genericEffectsList, 
			EffectCategoryType.Player => Instance.m_playerEffectsList, 
			EffectCategoryType.PlayerInteraction => Instance.m_playerInteractionEffectsList, 
			EffectCategoryType.Enemies => Instance.m_enemyEffectsList, 
			EffectCategoryType.Props => Instance.m_propsEffectList, 
			EffectCategoryType.Projectiles => Instance.m_projectilesEffectList, 
			EffectCategoryType.SelfAnimation => Instance.m_selfAnimationEffectList, 
			EffectCategoryType.UI => Instance.m_uiEffectList, 
			_ => null, 
		};
	}

	public static EffectEntry GetEffectEntry(EffectCategoryType category, int index)
	{
		return GetEffectEntryList(category)[index];
	}

	public static void AddEffectEntry(EffectCategoryType category, EffectEntry newEffectEntry)
	{
		GetEffectEntryList(category).Add(newEffectEntry);
	}
}
