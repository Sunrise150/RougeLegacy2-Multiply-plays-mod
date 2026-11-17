using FMODUnity;
using UnityEngine;

namespace RLAudio;

public class ItemDropAudioEventController : MonoBehaviour, IAudioEventEmitter
{
	private BaseItemDrop m_itemDrop;

	private CorgiController_RL m_corgiController;

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
		m_itemDrop = GetComponent<BaseItemDrop>();
		if (m_itemDrop != null)
		{
			m_itemDrop.CollectedRelay.AddListener(OnItemCollected);
		}
		m_corgiController = GetComponent<CorgiController_RL>();
		if (m_corgiController != null)
		{
			m_corgiController.OnCorgiLandedEnterRelay.AddListener(OnItemLanded);
		}
	}

	private void OnDestroy()
	{
		if (m_itemDrop != null)
		{
			m_itemDrop.CollectedRelay.RemoveListener(OnItemCollected);
		}
		if (m_corgiController != null)
		{
			m_corgiController.OnCorgiLandedEnterRelay.RemoveListener(OnItemLanded);
		}
	}

	private void OnItemLanded(CorgiController_RL controller)
	{
		if (m_itemDrop != null)
		{
			string landEventPath = ItemDropAudioLibrary.GetItemDropAudioLibraryEntry(m_itemDrop.ItemDropType).LandEventPath;
			PlayAudio(landEventPath);
		}
	}

	private void OnItemCollected()
	{
		if (m_itemDrop != null)
		{
			string collectEventPath = ItemDropAudioLibrary.GetItemDropAudioLibraryEntry(m_itemDrop.ItemDropType).CollectEventPath;
			PlayAudio(collectEventPath);
		}
	}

	private void PlayAudio(string audioPath)
	{
		try
		{
			AudioManager.PlayOneShot(this, audioPath, base.transform.position);
		}
		catch (EventNotFoundException)
		{
			Debug.LogFormat("<color=red>| {0} | The FMOD Audio Event path <b>({1})</b> is not valid. Check the ItemDropAudioLibrary's <b>({2})</b> entry and update the path if required.</color>", this, audioPath, m_itemDrop.ItemDropType);
		}
	}
}
