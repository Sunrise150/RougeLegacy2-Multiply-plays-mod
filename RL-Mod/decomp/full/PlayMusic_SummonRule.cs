using System;
using System.Collections;
using RLAudio;
using UnityEngine;

[Serializable]
public class PlayMusic_SummonRule : BaseSummonRule
{
	[SerializeField]
	private SongID m_songID;

	public override SummonRuleType RuleType => SummonRuleType.PlayMusic;

	public override string RuleLabel => "Play Music";

	public override IEnumerator RunSummonRule()
	{
		if (m_songID != MusicManager.CurrentSong)
		{
			MusicManager.StopMusic();
			if (m_songID != 0)
			{
				MusicManager.PlayMusic(m_songID, isOverride: true);
			}
		}
		base.IsRuleComplete = true;
		yield break;
	}
}
