using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(menuName = "Custom/Libraries/Component Color Library")]
public class ComponentColorLibrary : ScriptableObject
{
	[SerializeField]
	private List<ComponentColorEntry> m_componentColorEntries;

	private static ComponentColorLibrary m_instance = null;

	private const string RESOURCES_PATH = "Scriptable Objects/Libraries/ComponentColorLibrary";

	public const string ASSET_PATH = "Assets/Content/Scriptable Objects/Libraries/ComponentColorLibrary.asset";

	private static Color DEFAULT_COLOR = Color.grey;

	private static ComponentColorLibrary Instance
	{
		get
		{
			if (m_instance == null)
			{
				if (Application.isPlaying)
				{
					m_instance = CDGResources.Load<ComponentColorLibrary>("Scriptable Objects/Libraries/ComponentColorLibrary");
				}
				if (m_instance == null)
				{
					Debug.LogFormat("{0}: Unable to find ComponentColorManager at path ({1})", Time.frameCount, "Scriptable Objects/Libraries/ComponentColorLibrary");
				}
			}
			return m_instance;
		}
	}

	public static List<ComponentColorEntry> ComponentColorEntries
	{
		get
		{
			if (Instance != null)
			{
				return Instance.m_componentColorEntries;
			}
			return new List<ComponentColorEntry>();
		}
	}

	public static Color GetColor(string component)
	{
		IEnumerable<ComponentColorEntry> source = ComponentColorEntries.Where((ComponentColorEntry entry) => entry.Component == component);
		Color result = DEFAULT_COLOR;
		if (source.Count() == 1)
		{
			result = source.First().Color;
		}
		else if (source.Count() > 1)
		{
			Debug.LogFormat("<color=red>[{0}] Found multiple entries matching Component name ({1})</color>", Instance.name, component);
		}
		else
		{
			Debug.LogFormat("<color=red>[{0}] Found zero entries matching Component name ({1})</color>", Instance.name, component);
		}
		return result;
	}
}
