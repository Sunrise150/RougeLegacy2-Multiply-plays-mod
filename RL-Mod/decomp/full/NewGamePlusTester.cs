using System;
using UnityEngine;

public class NewGamePlusTester : MonoBehaviour
{
	[Serializable]
	private struct BurdenTestEntry
	{
		public BurdenType BurdenType;

		public int BurdenLevel;
	}

	[SerializeField]
	private BurdenTestEntry[] m_burdenTestArray;

	[SerializeField]
	private int m_NGPlusLevel;

	private void Start()
	{
		SaveManager.PlayerSaveData.NewGamePlusLevel = m_NGPlusLevel;
		BurdenTestEntry[] burdenTestArray = m_burdenTestArray;
		for (int i = 0; i < burdenTestArray.Length; i++)
		{
			BurdenTestEntry burdenTestEntry = burdenTestArray[i];
			BurdenManager.SetBurdenLevel(burdenTestEntry.BurdenType, burdenTestEntry.BurdenLevel, additive: false);
		}
	}
}
