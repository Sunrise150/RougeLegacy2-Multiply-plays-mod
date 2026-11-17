using UnityEngine;

namespace Foreground;

[CreateAssetMenu(menuName = "Custom/Libraries/Foreground Library")]
public class ForegroundLibrary : ScriptableObject
{
	[SerializeField]
	private ForegroundGroup[] m_foregroundGroups;

	public ForegroundGroup[] GetForegrounds()
	{
		return m_foregroundGroups;
	}
}
