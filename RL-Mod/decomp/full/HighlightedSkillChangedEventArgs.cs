using System;

public class HighlightedSkillChangedEventArgs : EventArgs
{
	public SkillTreeType SkillTreeType { get; private set; }

	public HighlightedSkillChangedEventArgs(SkillTreeType skillTreeType)
	{
		Initialize(skillTreeType);
	}

	public void Initialize(SkillTreeType skillTreeType)
	{
		SkillTreeType = skillTreeType;
	}
}
