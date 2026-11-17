using System;

public class SkillLevelChangedEventArgs : EventArgs
{
	public SkillTreeType SkillTreeType { get; private set; }

	public int NewLevel { get; private set; }

	public int PrevLevel { get; private set; }

	public SkillLevelChangedEventArgs(SkillTreeType skillTreeType, int prevLevel, int newLevel)
	{
		Initialize(skillTreeType, prevLevel, newLevel);
	}

	public void Initialize(SkillTreeType skillTreeType, int prevLevel, int newLevel)
	{
		SkillTreeType = skillTreeType;
		NewLevel = newLevel;
		PrevLevel = prevLevel;
	}
}
