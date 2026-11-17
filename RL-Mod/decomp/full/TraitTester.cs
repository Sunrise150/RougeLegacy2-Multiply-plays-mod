using System.Collections;
using UnityEngine;

public class TraitTester : MonoBehaviour
{
	[SerializeField]
	private TraitType m_trait1Test;

	[SerializeField]
	private TraitType m_trait2Test;

	private bool m_traitsTriggered;

	public TraitType Trait1
	{
		get
		{
			return m_trait1Test;
		}
		set
		{
			m_trait1Test = value;
		}
	}

	public TraitType Trait2
	{
		get
		{
			return m_trait2Test;
		}
		set
		{
			m_trait2Test = value;
		}
	}

	private void OnEnable()
	{
		if (!m_traitsTriggered)
		{
			StartCoroutine(TestCoroutine());
		}
	}

	private void Start()
	{
		if (!m_traitsTriggered)
		{
			StartCoroutine(TestCoroutine());
		}
	}

	private IEnumerator TestCoroutine()
	{
		m_traitsTriggered = true;
		yield return new WaitForSeconds(1f);
		SaveManager.PlayerSaveData.CurrentCharacter.TraitOne = m_trait1Test;
		SaveManager.PlayerSaveData.CurrentCharacter.TraitTwo = m_trait2Test;
		Messenger<GameMessenger, GameEvent>.Broadcast(GameEvent.TraitsChanged, this, new TraitChangedEventArgs(m_trait1Test, m_trait2Test));
	}
}
