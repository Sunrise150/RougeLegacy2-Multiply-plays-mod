using UnityEngine;

public class TraitorMemoryUnlockIndexOverride : MonoBehaviour
{
	[SerializeField]
	private int m_traitorMemoryUnlockIndex = -1;

	public int TraitorMemoryUnlockIndex => m_traitorMemoryUnlockIndex;
}
