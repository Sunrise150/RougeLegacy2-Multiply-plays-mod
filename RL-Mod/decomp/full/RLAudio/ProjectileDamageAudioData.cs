namespace RLAudio;

public class ProjectileDamageAudioData : BaseDamageAudioData
{
	private string m_characterDamageAudioPath;

	private string m_breakableDamageAudioEventPath;

	private bool m_arePathsSet;

	public override string BreakableDamageAudioPath
	{
		get
		{
			if (!m_arePathsSet)
			{
				SetAudioPaths();
			}
			return m_breakableDamageAudioEventPath;
		}
	}

	public override string CharacterDamageAudioPath
	{
		get
		{
			if (!m_arePathsSet)
			{
				SetAudioPaths();
			}
			return m_characterDamageAudioPath;
		}
	}

	private string GetProjectileName()
	{
		string text = base.name;
		if (text.EndsWith(" Clone"))
		{
			text = text.Substring(0, text.Length - " Clone".Length);
		}
		else if (text.EndsWith("(Clone)"))
		{
			text = text.Substring(0, text.Length - "(Clone)".Length);
		}
		return text;
	}

	private void SetAudioPaths()
	{
		m_arePathsSet = true;
		ProjectileAudioLibraryEntry entry = ProjectileAudioLibrary.GetEntry(GetProjectileName());
		if (entry != null)
		{
			m_breakableDamageAudioEventPath = entry.HitItemEventPath;
			m_characterDamageAudioPath = entry.HitCharacterEventPath;
		}
	}
}
