using UnityEngine;

public class DialogueDisplayOverride : MonoBehaviour
{
	[SerializeField]
	private bool m_useLocIDOverride = true;

	[SerializeField]
	private string m_dialogueOverride;

	[SerializeField]
	private string m_repeatedDialogueOverride;

	[SerializeField]
	private string m_speakerOverride;

	[SerializeField]
	private PlayerSaveFlag m_spokenFlagOverride;

	public bool UseLocIDOverride => m_useLocIDOverride;

	public string DialogueOverride => m_dialogueOverride;

	public string RepeatedDialogueOverride => m_repeatedDialogueOverride;

	public string SpeakerOverride => m_speakerOverride;

	public PlayerSaveFlag SpokenFlagOverride => m_spokenFlagOverride;
}
