using UnityEngine;

public class PlayerFlagTrigger : MonoBehaviour, IDisplaySpeechBubble
{
	[SerializeField]
	private GameObject m_speechBubble;

	[SerializeField]
	private PlayerSaveFlag m_flagToTrigger;

	public SpeechBubbleType BubbleType => SpeechBubbleType.Dialogue;

	public bool ShouldDisplaySpeechBubble => !SaveManager.PlayerSaveData.GetFlag(m_flagToTrigger);

	public void Trigger()
	{
		SaveManager.PlayerSaveData.SetFlag(m_flagToTrigger, value: true);
	}
}
