using UnityEngine;

namespace RLAudio;

public class ItemDropManagerAudioEventController : MonoBehaviour, IAudioEventEmitter
{
	[SerializeField]
	private int m_minMultipleGoldCount = 3;

	private ItemDropManager m_dropManager;

	private string m_description = string.Empty;

	public string Description
	{
		get
		{
			if (string.IsNullOrEmpty(m_description))
			{
				m_description = ToString();
			}
			return m_description;
		}
	}

	private void Awake()
	{
		m_dropManager = GetComponent<ItemDropManager>();
		if (m_dropManager != null)
		{
			m_dropManager.DropItemRelay.AddListener(OnItemDropped);
			m_dropManager.DropGoldRelay.AddListener(OnMultipleGoldDropped);
		}
	}

	private void OnItemDropped(ItemDropType itemDrop, Vector2 position)
	{
		string spawnSingleEventPath = ItemDropAudioLibrary.GetItemDropAudioLibraryEntry(itemDrop).SpawnSingleEventPath;
		if (!string.IsNullOrEmpty(spawnSingleEventPath))
		{
			AudioManager.PlayOneShot(this, spawnSingleEventPath, position);
		}
	}

	private void OnMultipleGoldDropped(ItemDropType itemDrop, Vector2 position, int count)
	{
		ItemDropAudioLibraryEntry itemDropAudioLibraryEntry = ItemDropAudioLibrary.GetItemDropAudioLibraryEntry(itemDrop);
		string text = itemDropAudioLibraryEntry.SpawnSingleEventPath;
		if (count >= m_minMultipleGoldCount && itemDropAudioLibraryEntry.UseManyAudio)
		{
			text = itemDropAudioLibraryEntry.SpawnManyEventPath;
		}
		if (!string.IsNullOrEmpty(text))
		{
			AudioManager.PlayOneShot(this, text, position);
		}
	}
}
