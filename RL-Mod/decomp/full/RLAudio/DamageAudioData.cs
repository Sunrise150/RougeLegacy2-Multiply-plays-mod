using FMODUnity;
using UnityEngine;
using UnityEngine.Serialization;

namespace RLAudio;

public class DamageAudioData : BaseDamageAudioData
{
	[SerializeField]
	[EventRef]
	[FormerlySerializedAs("m_characterFmodEvent")]
	private string m_breakableAudioEventPath;

	[SerializeField]
	[EventRef]
	[FormerlySerializedAs("m_characterFmodEvent")]
	private string m_characterAudioEventPath;

	public override string BreakableDamageAudioPath => m_breakableAudioEventPath;

	public override string CharacterDamageAudioPath => m_characterAudioEventPath;
}
