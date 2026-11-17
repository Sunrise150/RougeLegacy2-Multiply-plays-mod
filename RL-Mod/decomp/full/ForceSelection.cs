using UnityEngine;

public class ForceSelection : MonoBehaviour
{
	public enum ForceSelectionType
	{
		SelectBase,
		Lock,
		None
	}

	[SerializeField]
	private ForceSelectionType m_selectionType;

	public ForceSelectionType SelectionType => m_selectionType;
}
